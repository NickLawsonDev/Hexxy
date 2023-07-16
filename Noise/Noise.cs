using System;
using System.Collections.Generic;
using System.Linq;
using Hexxy.Core;
using Hexxy.Helpers;
using Microsoft.Xna.Framework;

namespace Hexxy.Noise;

public record Wave(float Seed, float Frequency, float Amplitude);

public static class Noise
{
    private static readonly bool _useFallOff = true;
    public static float[,] Generate(int width, int height, int seed, float frequency, int octaves, int lacunarity, float gain)
    {
        float[,] noiseMap = new float[width, height];
        var noise = new FastNoiseLite();
        noise.SetSeed(seed);
        noise.SetFrequency(frequency);
        noise.SetFractalOctaves(octaves);
        noise.SetFractalLacunarity(lacunarity);
        noise.SetFractalGain(gain);
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                noiseMap[x, y] = MathF.Abs(noise.GetNoise(x, y));
            }
        }

        return noiseMap;
    }
}