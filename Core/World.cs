
using System;
using System.Collections.Generic;
using Hexxy.Core;
using Hexxy.Noise;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Hexxy.Core.Voronai;
using Microsoft.Xna.Framework;
using Hexxy.Helpers;

namespace Core.World;

public class World
{
    public List<Hex> Hexs { get; set; } = new List<Hex>();
    public float scale = 1.0f;
    public float[,] HeightMap { get; set; }
    public float[,] TemperatureMap { get; set; }
    public float[,] MoistureMap { get; set; }
    private ContentManager _content;
    public float LakeThreshold { get; set; } = 0.3f;

    public void GenerateWorld(ContentManager content)
    {
        Hexs.Clear();
        _content = content;
        HeightMap = Noise.Generate(Global.WorldSizeWidth, Global.WorldSizeHeight, new Random().Next(0, int.MaxValue), 0.013f, 2, 2, 0.7f);
        var q = new float[HeightMap.GetLength(0) * HeightMap.GetLength(1)];
        Buffer.BlockCopy(HeightMap, 0, q, 0, q.Length * sizeof(float));
        var c = q.Max();
        var b = q.Min();
        var z = q.Average();
        TemperatureMap = Noise.Generate(Global.WorldSizeWidth, Global.WorldSizeHeight, new Random().Next(0, int.MaxValue), 0.04f, 4, 4, 0.5f);
        MoistureMap = Noise.Generate(Global.WorldSizeWidth, Global.WorldSizeHeight, new Random().Next(0, int.MaxValue), 0.09f, 2, 2, 0.5f);

        for (int x = 0; x < Global.WorldSizeWidth; x++)
        {
            for (int y = 0; y < Global.WorldSizeHeight; y++)
            {
                var biome = GetBiome(x, y);
                Hexs.Add(new(x, y, biome.Item1, biome.Item2));
            }
        }

        var points = GenerateRandom(Global.WorldSizeWidth * Global.WorldSizeHeight, Global.WorldSizeWidth);
        Voronoi.BuildGraph(points);
        var g = Voronoi.Corners;
        var h = Voronoi.Centers;
        var j = Voronoi.Edges;

        AssignCornerElevation();
        AssignOceanCoastAndLand();
        RedistributeElevations();
        foreach (var corner in Voronoi.Corners) if (corner.Type == TerrainType.Ocean || corner.IsCoast) corner.Elevation = 0;
        AssignPolygonElevations();

        foreach (var hex in Hexs)
        {
            var closest = FindClosestCenter(new(hex.X, hex.Y), Voronoi.Centers);
            if (closest.IsCoast)
            {
                hex.Type = TerrainType.Coast;
                hex.Texture = content.Load<Texture2D>("tiles/Sand");
            }
            else if (closest.Type == TerrainType.Ocean && closest.IsWater)
            {
                hex.Type = TerrainType.Ocean;
                hex.Texture = content.Load<Texture2D>("tiles/Water");
            }
            else if (closest.Type != TerrainType.Ocean && closest.IsWater)
            {
                hex.Type = TerrainType.Lake;
                hex.Texture = content.Load<Texture2D>("tiles/Water");
            }
            else
            {
                hex.Type = TerrainType.Grass;
                hex.Texture = content.Load<Texture2D>("tiles/Grass");
            }
        }
    }

    private static Center FindClosestCenter(Vector2 point, List<Center> centers)
    {
        var closest = new Center();
        closest.Point = Vector2.Zero;
        var closestDistance = float.MaxValue;
        foreach (var center in centers)
        {
            var position = center.Point;
            var distance = Vector2.DistanceSquared(position, point);
            if (closest.Point == Vector2.Zero || distance < closestDistance)
            {
                closest = center;
                closestDistance = distance;
            }
        }

        return closest;
    }

    private void AssignCornerElevation()
    {
        var queue = new Queue<Corner>();
        var random = new Random();
        foreach (var q in Voronoi.Corners)
        {
            if (q.IsBorder)
            {
                q.Elevation = 0f;
                queue.Enqueue(q);
            }
            else q.Elevation = float.MaxValue;
        }

        while (queue.Count > 0)
        {
            var q = queue.Dequeue();

            foreach (var s in q.Adjacent)
            {
                var newElevation = 0.01f + q.Elevation;
                if (!q.IsWater && !s.IsWater)
                {
                    newElevation += 1;
                    newElevation += random.NextFloat();
                }

                if (newElevation < s.Elevation)
                {
                    s.Elevation = newElevation;
                    queue.Enqueue(s);
                }
            }
        }
    }

    public static List<Vector2> GenerateRandom(int numberOfPoints, int size, int seed = 0)
    {
        var random = seed == 0 ? new Random() : new Random(seed);
        var points = new List<Vector2>();

        for (var i = 0; i < numberOfPoints; i++)
        {
            var p = new Vector2(random.NextFloatRange(0, size),
                            random.NextFloatRange(0, size));
            points.Add(p);
        }
        return points;

    }
    private void AssignOceanCoastAndLand()
    {
        var queue = new Queue<Center>();
        var water = 0;
        foreach (var center in Voronoi.Centers)
        {
            water = 0;
            foreach (var corner in center.Corners)
            {
                if (corner.IsBorder)
                {
                    center.IsBorder = true;
                    center.Type = TerrainType.Ocean;
                    corner.IsWater = true;
                    queue.Enqueue(center);
                }
                if (corner.IsWater) water += 1;
            }
            center.IsWater = center.IsWater || water >= center.Corners.Count * LakeThreshold;
        }

        while (queue.Count > 0)
        {
            var center = queue.Dequeue();

            foreach (var neighbor in center.Neighbors)
            {
                if (neighbor.IsWater && neighbor.Type != TerrainType.Ocean)
                {
                    neighbor.Type = TerrainType.Ocean;
                    queue.Enqueue(neighbor);
                }
            }
        }

        var ocean = 0;
        var land = 0;

        foreach (var center in Voronoi.Centers)
        {
            ocean = 0;
            land = 0;

            foreach (var neighbor in center.Neighbors)
            {
                ocean += neighbor.Type == TerrainType.Ocean ? 1 : 0;
                land += !neighbor.IsWater ? 1 : 0;
            }
            center.IsCoast = ocean > 0 && land > 0;
        }

        foreach (var corner in Voronoi.Corners)
        {
            ocean = 0;
			land = 0;

            foreach (var touch in corner.Touches)
            {
                ocean += touch.Type == TerrainType.Ocean ? 1 : 0;
                land += !touch.IsWater ? 1 : 0;
            }

            var isOcean = ocean == corner.Touches.Count;
            corner.IsCoast = ocean > 0 && land > 0;
            corner.IsWater = corner.IsBorder || (land != corner.Touches.Count && !corner.IsCoast);
            if (corner.IsWater && isOcean) corner.Type = TerrainType.Ocean;
            if (corner.IsCoast) corner.Type = TerrainType.Coast;
            if (corner.IsWater && !isOcean) corner.Type = TerrainType.Lake;
        }
    }

    private static void RedistributeElevations()
    {
        var scaleFactor = 1.1f;
        Voronoi.Corners = Voronoi.Corners.OrderByDescending(x => x.Elevation).ToList();
        var locations = LandCorners();

        for (var i = 0; i < locations.Count(); i++)
        {
            var y = i / Voronoi.Corners.Count;
            var x = MathF.Sqrt(scaleFactor) - MathF.Sqrt(scaleFactor * (1 - y));
            if (x > 1) x = 1;
            Voronoi.Corners[i].Elevation = x;
        }
    }

    private static IEnumerable<Corner> LandCorners()
    {
        return Voronoi.Corners.Where(x => x.Type != TerrainType.Ocean && !x.IsCoast);
    }

    private static void AssignPolygonElevations()
    {
        var elevation = 0f;
        foreach (var center in Voronoi.Centers)
        {
            foreach (var corner in center.Corners)
            {
                elevation += corner.Elevation;
            }
            center.Elevation = elevation / center.Corners.Count;
        }
    }

    private (Texture2D, TerrainType) GetBiome(int x, int y)
    {
        var height = HeightMap[x, y];
        var moist = MoistureMap[x, y];
        var temp = TemperatureMap[x, y];
        var rand = new Random();
        //Ocean
        if (height >= Global.WaterLevel)
        {
            return rand.Next(0, 100) switch
            {
                _ => (_content.Load<Texture2D>("tiles/Water"), TerrainType.Ocean)
            };
        }
        //Beach
        else if (height <= Global.WaterLevel && height >= Global.WaterLevel - 0.2f)
        {
            return rand.Next(0, 100) switch
            {
                _ => (_content.Load<Texture2D>("tiles/Sand"), TerrainType.Coast)
            };
        }
        else if (height <= Global.WaterLevel - 0.2f && height >= Global.WaterLevel - 0.4f)
        {
            if (moist >= 0.1f && temp >= 0 && temp <= 1.8)
                return (_content.Load<Texture2D>("tiles/Grass"), TerrainType.Grass);
            // if (moist <= 0.5f && temp >= 1.6)
            //     return (_content.Load<Texture2D>("tiles/Sand"), TerrainType.Desert);
            return (_content.Load<Texture2D>("tiles/Dirt"), TerrainType.Coast);
        }
        //Mountain
        else if (height <= Global.WaterLevel + 0.4f && height >= Global.WaterLevel - 0.6f)
        {
            return rand.Next(0, 100) switch
            {
                _ => (_content.Load<Texture2D>("tiles/Rock"), TerrainType.Mountain)
            };
        }
        else if (height <= Global.WaterLevel - 0.6f)
        {
            return rand.Next(0, 100) switch
            {
                _ => (_content.Load<Texture2D>("tiles/Snow"), TerrainType.Mountain)
            };
        }

        return (_content.Load<Texture2D>("tiles/Water"), TerrainType.Mountain);
    }
}