using System.Linq;
using System.Numerics;
using InfectedRose.Nif;
namespace InfectedRose.Terrain.Editor
{
    public class ColorBrush : Brush
    {
        public ByteColor4 Color { get; set; }
        
        public bool TargetSecond { get; set; }
        
        public ColorBrush(TerrainEditor editor) : base(editor)
        {
        }

        public override void Apply(Vector2 position)
        {
            var layer = TargetSecond ? Editor.SecondColorLayer : Editor.ColorLayer;
            
            var points = layer.Colors.Where(
                pair => Vector2.Distance(pair.Key, position) <= Size
            ).ToArray();

            foreach (var (key, _) in points)
            {
                layer.SetColor(key, Color);
            }
        }
    }
}