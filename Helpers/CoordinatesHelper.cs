using System;
using Hexxy.Core;

namespace Hexxy.Helpers;

public static class CoordinatesHelper
{
    public static (int, int, int) GetCubeCoordinates(int column, int row)
    {
        var q = column;
        var r = row - (column - (column & 1)) / 2;
        var coords = (q, r, -q - r);
        if (coords.q + coords.r + coords.Item3 != 0) throw new ArgumentException("Something went wrong with calculation");
        return coords;
    }

    public static (int, int) CubeToOffset((int, int, int)cubeCoords)
    {
        return (cubeCoords.Item1, cubeCoords.Item2 + (cubeCoords.Item1 - (cubeCoords.Item1 & 1)) / 2);
    }

    public static (float, float) OffsetToPixel(int column, int row)
    {
        var x = Global.HexSize * 1.73205f * (column + 0.5f * (row&1));
        var y = Global.HexSize * 1.5f * row;
        return (x, y);
    }
}