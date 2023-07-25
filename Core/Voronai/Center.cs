using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Hexxy.Core.Voronai;

public class Center
{
    public int Index { get; set; }
    public Vector2 Point { get; set; }
    public TerrainType Type { get; set; } = TerrainType.Grass;
    public bool IsBorder { get; set; } = false;
    public bool IsWater {get; set; } = false;
    public bool IsCoast {get;set;} = false;
    public float Elevation { get; set; }
    public List<Center> Neighbors { get; set; } = new List<Center>();
    public List<Edge> Borders { get; set; } = new List<Edge>();
    public List<Corner> Corners { get; set; } = new List<Corner>();
}