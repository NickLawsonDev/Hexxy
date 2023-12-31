using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Hexxy.Core.Voronai;

public static class Voronoi
{
    public static List<Center> Centers { get; set; } = new();
    public static List<Corner> Corners { get; set; } = new();
    public static List<Edge> Edges { get; set; } = new();

    public static void BuildGraph(List<Vector2> points)
    {
        var centerLookUp = new Dictionary<Vector2, Center>();

        foreach (var point in points)
        {
            var c = new Center()
            {
                Index = Centers.Count,
                Point = point
            };

            Centers.Add(c);
            centerLookUp[point] = c;
        }

        var edges = new csDelaunay.Voronoi(points, new Rectf(0, 0, Global.WorldSizeWidth, Global.WorldSizeHeight)).Edges;
        foreach (var edge in edges)
        {
            var dEdge = edge.DelaunayLine();
            var vEdge = edge.VoronoiEdge();

            var newEdge = new Edge()
            {
                Index = Edges.Count,
                Midpoint = Vector2.Lerp(vEdge.p0, vEdge.p1, 0.5f),
                V0 = MakeCorner(vEdge.p0),
                V1 = MakeCorner(vEdge.p1),
                D0 = centerLookUp[dEdge.p0],
                D1 = centerLookUp[dEdge.p1]
            };
            newEdge.D0?.Borders.Add(newEdge);
            newEdge.D1?.Borders.Add(newEdge);
            newEdge.V0?.Protrudes.Add(newEdge);
            newEdge.V1?.Protrudes.Add(newEdge);

            // Centers point to centers.
            if (newEdge.D0 != null && newEdge.D1 != null)
            {
                AddToCenterList(newEdge.D0.Neighbors, newEdge.D1);
                AddToCenterList(newEdge.D1.Neighbors, newEdge.D0);
            }

            // Centers point to corners
            if (newEdge.D0 != null)
            {
                AddToCornerList(newEdge.D0.Corners, newEdge.V0);
                AddToCornerList(newEdge.D0.Corners, newEdge.V1);
            }
            if (newEdge.D1 != null)
            {
                AddToCornerList(newEdge.D1.Corners, newEdge.V0);
                AddToCornerList(newEdge.D1.Corners, newEdge.V1);
            }

            // Corners point to centers
            if (newEdge.V0 != null)
            {
                AddToCenterList(newEdge.V0.Touches, newEdge.D0);
                AddToCenterList(newEdge.V0.Touches, newEdge.D1);
            }
            if (newEdge.V1 != null)
            {
                AddToCenterList(newEdge.V1.Touches, newEdge.D0);
                AddToCenterList(newEdge.V1.Touches, newEdge.D1);
            }

            Edges.Add(newEdge);
        }
    }

    private static Dictionary<float, List<Corner>> _cornersMap { get; set; } = new();
    private static Corner MakeCorner(Vector2 point)
    {
        if (point == Vector2.Zero) 
			return null;

        var bucket = 0;
        for (bucket = (int)point.X - 1; bucket <= point.X + 1; bucket++)
        {
            _cornersMap.TryGetValue(bucket, out var cornermap);
            cornermap ??= new List<Corner>();
            foreach (var c in cornermap)
            {
                var dx = point.X - c.Point.X;
                var dy = point.Y - c.Point.Y;
                var f = dx * dx;
                var d = dy * dy;
                if (dx * dx + dy * dy < 1e-6) return c;
            }
        }

        bucket = (int)point.X;
        if (_cornersMap.Count <= bucket || !_cornersMap.TryGetValue(bucket, out _)) 
            _cornersMap[bucket] = new List<Corner>();

        var newCorner = new Corner()
        {
            Index = Corners.Count,
            Point = point,
            IsBorder = point.X == 0 || point.X == Global.WorldSizeWidth
                        || point.Y == 0 || point.Y == Global.WorldSizeHeight,
            IsWater = point.X == 0 || point.X == Global.WorldSizeWidth
                        || point.Y == 0 || point.Y == Global.WorldSizeHeight,
            Type = TerrainType.Ocean
        };
        Corners.Add(newCorner);
        _cornersMap.TryGetValue(bucket, out var cc);
        cc.Add(newCorner);
        return newCorner;
    }

    private static void AddToCornerList(List<Corner> v, Corner x)
    {
        if (x != null && !v.Contains(x)) v.Add(x);
    }
    private static void AddToCenterList(List<Center> v, Center x)
    {
        if (x != null && !v.Contains(x)) v.Add(x);
    }
}