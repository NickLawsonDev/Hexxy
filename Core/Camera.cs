using System.Security.Cryptography.X509Certificates;
using Hexxy.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hexxy.Core;

public class Camera
{
    public float Zoom { get; set; } = 0.3f;
    public Matrix Transform { get; private set; }
    public Vector2 Position { get; set; } = new(0, 0);

    public Camera()
    {
        var pixelMiddleMap = CoordinatesHelper.OffsetToPixel(Global.WorldSizeWidth / 2, Global.WorldSizeHeight / 2);
        Position = new(pixelMiddleMap.Item1, pixelMiddleMap.Item2);
    }

    public void Update(Vector2 amount)
    {
        // var leftBoundary = CoordinatesHelper.OffsetToPixel(0, 0).Item1 + Global.BackBufferWidth / 2;
        // var rightBoundary = CoordinatesHelper.OffsetToPixel(Global.WorldSizeWidth, 0).Item1 - Global.BackBufferWidth;
        // var topBoundary = CoordinatesHelper.OffsetToPixel(1, 0).Item2 + (Global.BackBufferHeight / 2);
        // var bottomBoundary = CoordinatesHelper.OffsetToPixel(0, Global.WorldSizeHeight-1).Item2 + (Global.BackBufferHeight/2) - 32*5;
        // var newPos = Position + amount;
        // if (newPos.X <= leftBoundary)
        //     return;
        // if (newPos.X >= rightBoundary)
        //     return;
        // if (newPos.Y <= topBoundary)
        //     return;
        // if (newPos.Y >= bottomBoundary)
        //     return;

        Position += amount;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Transform = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                                         Matrix.CreateRotationZ(0.0f) *
                                         Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                                         Matrix.CreateTranslation(new Vector3(Global.BackBufferWidth * 0.5f, Global.BackBufferHeight * 0.5f, 0));
        spriteBatch.Begin(SpriteSortMode.BackToFront,
                        transformMatrix: Transform);
    }
}