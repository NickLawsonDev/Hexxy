
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

        var points = Hexs.Select(x => new Vector2(x.X, x.Y)).ToList();
        Voronoi.BuildGraph(points);
        var g = Voronoi.Corners;
        var h = Voronoi.Centers;
        var j = Voronoi.Edges;

        foreach(var hex in Hexs)
        {
            var v = HexxyMathHelper.FindClosest(new(hex.X, hex.Y), h.Select(x => x.Point).ToList());

            var u = HexxyMathHelper.NearlyEqual(h.First().Point.X, v.X) && HexxyMathHelper.NearlyEqual(h.First().Point.Y, v.Y);
            
            if (h.First(x => HexxyMathHelper.NearlyEqual(x.Point.X, v.X) && HexxyMathHelper.NearlyEqual(x.Point.Y, v.Y)).IsBorder)
            {
                hex.Type = TerrainType.Dirt;
                hex.Texture = _content.Load<Texture2D>("tiles/Dirt");
            }
            else
            {
                hex.Type = TerrainType.Water;
                hex.Texture = _content.Load<Texture2D>("tiles/Water");
            }
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
                _ => (_content.Load<Texture2D>("tiles/Water"), TerrainType.Water)
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