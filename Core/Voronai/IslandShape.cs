using System;
using Hexxy.Helpers;
using Microsoft.Xna.Framework;

namespace Hexxy.Core.Voronai;

public static class IslandShape
{
    public static float IslandFactor { get; set; } = 1.07f;

    private static float StartAngle { get; set; }
    private static float DipAngle { get; set; }
    private static float DipWidth { get; set; }
    private static int Bumps { get; set; }

    public static bool MakeRadial(Vector2 point, int seed)
    {
        var random = new Random(seed);
        Bumps = random.Next(1, 6);
        StartAngle = random.NextFloatRange(0, 2 * MathF.PI);
        DipAngle = random.NextFloatRange(0, 2 * MathF.PI);
        DipWidth = random.NextFloatRange(0.2f, 0.7f);

        var angle = MathF.Atan2(point.Y, point.X);
        var length = 0.5f * (MathF.Max(MathF.Abs(point.X), MathF.Abs(point.Y)) + point.Length());

        var r1 = 0.5f + 0.40f * MathF.Sin(StartAngle + Bumps * angle + MathF.Cos((Bumps + 3) * angle));
        var r2 = 0.7f - 0.20f * MathF.Sin(StartAngle + Bumps * angle - MathF.Sin((Bumps + 2) * angle));
        if (MathF.Abs(angle - DipAngle) < DipWidth
            || MathF.Abs(angle - DipAngle + 2 * MathF.PI) < DipWidth
            || MathF.Abs(angle - DipAngle - 2 * MathF.PI) < DipWidth)
        {
            r1 = r2 = 0.2f;
        }
        return length < r1 || (length > r1 * IslandFactor && length < r2);
    }

    public static bool MakePerlin(Vector2 point, int seed)
    {
        var noise = Noise.Noise.Generate(256, 256, seed, 8, 64, 64, 0.5f);

        var c = (float)noise.GetValue((int)(point.X + 1) * 128, (int)((point.Y + 1) * 128) & 0xff / 255);
        return c > (0.3f + 0.3f * point.Length() * point.Length());
    }
}