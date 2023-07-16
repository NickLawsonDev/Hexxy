using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Hexxy.Helpers;

public static class HexxyMathHelper
{
    public static float StandardDeviation(float[] values)
    {
        var avg = values.Average();
        return MathF.Sqrt(values.Average(v => MathF.Pow(v - avg, 2)));
    }

    public static Vector2 FindClosest(Vector2 value, IEnumerable<Vector2> positions)
    {
        Vector2 closest = Vector2.Zero;
        var closestDistance = float.MaxValue;
        foreach (var position in positions)
        {
            var distance = Vector2.DistanceSquared(position, value);
            if (closest == Vector2.Zero || distance < closestDistance)
            {
                closest = position;
                closestDistance = distance;
            }
        }

        return closest;
    }

    public static bool NearlyEqual(float a, float b, float epsilon = 0.00001f)
    {
        float absA = MathF.Abs(a);
        float absB = MathF.Abs(b);
        float diff = MathF.Abs(a - b);

        if (a == b)
        { // shortcut, handles infinities
            return true;
        }
        else if (a == 0 || b == 0 || absA + absB < float.MinValue)
        {
            // a or b is zero or both are extremely close to it
            // relative error is less meaningful here
            return diff < (epsilon * float.MinValue);
        }
        else
        { // use relative error
            return diff / (absA + absB) < epsilon;
        }
    }
}