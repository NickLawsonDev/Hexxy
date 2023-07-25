using System;

namespace Hexxy.Helpers;

public static class RandomExtensions
{
    public static float NextFloatRange(this Random random, float minimum, float maximum)
    {
        return (float)random.NextDouble() * (maximum - minimum) + minimum;
    }

    public static float NextFloat(this Random random)
    {
        return (float)random.NextDouble();
    }
}