using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Hexxy.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hexxy.Core;

public class Hex
{
    public int Column { get; private set; }
    public int Row { get; private set; }
    public Texture2D Texture { get; set; }
    public TerrainType Type { get; set; } = TerrainType.Water;
    public Dictionary<int, (int, int)> Neighbors { get; set; } = new Dictionary<int, (int, int)>();
    public float X { get; set; } = 0f;
    public float Y { get; set; } = 0f;

    private readonly (int, int, int)[] _cubeDirectionVectors = new (int, int, int)[]
    {
        (+1, 0, -1), (+1, -1, 0), (0, -1, +1),
        (-1, 0, +1), (-1, +1, 0), (0, +1, -1),
    };

    public Hex(int col, int row, Texture2D texture, TerrainType type)
    {
        Column = col;
        Row = row;
        Texture = texture;
        Type = type;

        for (var i = 0; i <= 5; i++)
        {
            var neighbor = GetNeighbor(OffsetToCube(), i);
            Neighbors.Add(i, (neighbor.Item1, neighbor.Item2));
        }
        CalculatePosition();
    }

    private void CalculatePosition()
    {
        if (Global.HexIsFlat)
        {
            var shouldOffset = (Column % 2) == 0;
            var width = 2f * Global.HexSize;
            var height = 1.73205f * Global.HexSize;

            var horizantalDistance = width * 0.75f;
            var verticalDistance = height;

            var offset = shouldOffset ? height / 2 : 0;

            X = Column * horizantalDistance;
            Y = (Row * verticalDistance) - offset;
        }
        else
        {
            var shouldOffset = (Row % 2) == 0;
            var width = 1.73205f * Global.HexSize;
            var height = 2f * Global.HexSize;

            var horizantalDistance = width;
            var verticalDistance = 0.75f * height;

            var offset = shouldOffset ? width / 2 : 0;

            X = (Column * horizantalDistance) + offset;
            Y = Row * verticalDistance;
        }
    }

    public void Draw(SpriteBatch spriteBatch, bool debug = false)
    {
        spriteBatch.Draw(Texture, new Vector2(X, Y), null, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 1);
        if (debug)
            spriteBatch.DrawString(Global.Font, $"{Column},{Row}", new Vector2(X + 5, Y + 13), Color.Black, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
    }

    public (int, int, int) OffsetToCube()
    {
        var q = Column;
        var r = Row - (Column - (Column & 1)) / 2;
        return (q, r, -q - r);
    }

    public (int, int) CubeToOffset()
    {
        var cubeCoords = OffsetToCube();
        return (cubeCoords.Item1, cubeCoords.Item2 + (cubeCoords.Item1 - (cubeCoords.Item1 & 1)) / 2);
    }

    public (float, float) OffsetToPixel()
    {
        var x = Global.HexSize * 1.73205f * (Column + 0.5f * (Row & 1));
        var y = Global.HexSize * 1.5f * Row;
        return (x, y);
    }

    private (int, int, int) CubeDirection(int direction) => _cubeDirectionVectors[direction];
    private static (int, int, int) AddCube((int, int, int) cubeCoords, (int, int, int) direction) => (cubeCoords.Item1 + direction.Item1, cubeCoords.Item2 + direction.Item2, cubeCoords.Item3 + direction.Item3);
    private (int, int) GetNeighbor((int, int, int) cubeCoords, int direction) => CoordinatesHelper.CubeToOffset(AddCube(cubeCoords, CubeDirection(direction)));
}