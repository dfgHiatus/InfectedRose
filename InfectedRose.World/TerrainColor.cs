using System.Numerics;
using System.Xml.Serialization;
using InfectedRose.Nif;

namespace InfectedRose.World
{
    [XmlRoot("Color")]
    public class TerrainColor
    {
        [XmlElement] public float Size { get; set; }
        
        [XmlElement] public ByteColor4 Color { get; set; }
        
        [XmlElement] public Vector2 Position { get; set; }
    }
}