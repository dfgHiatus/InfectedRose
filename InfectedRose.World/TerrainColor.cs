using System.Numerics;
using System.Xml.Serialization;
using SkiaSharp;

namespace InfectedRose.World
{
    [XmlRoot("Color")]
    public class TerrainColor
    {
        [XmlElement] public float Size { get; set; }
        
        [XmlElement] public SKColor Color { get; set; }
        
        [XmlElement] public Vector2 Position { get; set; }
    }
}