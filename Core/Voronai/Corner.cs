using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Hexxy.Core.Voronai;

public class Corner
{
    public int Index { get; set; }
    public Vector2 Point { get; set; }
    public TerrainType Type { get; set; }
    public bool IsBorder { get; set; }
    public List<Center> Touches { get; set; } = new List<Center>();
    public List<Edge> Protrudes { get; set; } = new List<Edge>();
    public List<Corner> Adjacent { get; set; } = new List<Corner>();
}