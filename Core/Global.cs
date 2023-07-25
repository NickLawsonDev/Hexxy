using Microsoft.Xna.Framework.Graphics;

namespace Hexxy.Core;

public static class Global
{
    public static SpriteFont Font { get; set; }
    public static float HexSize => 16.5f;
    public static bool HexIsFlat => true;
    public static int BackBufferWidth => 1920;
    public static int BackBufferHeight => 1080;
    public static int WorldSizeWidth => 20;
    public static int WorldSizeHeight => 20;

    public static float WaterLevel { get; set; } = 0.8f;
    public static float Pow { get; set; } = 2f;
}