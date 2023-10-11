using System.Collections.Generic;
using System.Numerics;
using InfectedRose.Nif;

namespace InfectedRose.Terrain.Editor
{
    public class ColorLayer
    {
        public TerrainEditor Editor { get; }
        
        public Dictionary<Vector2, ByteColor4> Colors { get; }
        
        private bool Second { get; }

        public ColorLayer(TerrainEditor editor, bool second)
        {
            Editor = editor;
            
            Colors = new Dictionary<Vector2, ByteColor4>();

            Second = second;
        }

        public void SetColor(Vector2 position, ByteColor4 value)
        {
            position /= 2;

            Colors[position] = value;

            var additionalX = (position.X % 65).Equals(0);
            var additionalY = (position.Y % 65).Equals(0);

            if (additionalX)
            {
                SetColor(position - Vector2.UnitX, value);
            }

            if (additionalY)
            {
                SetColor(position - Vector2.UnitY, value);
            }

            if (additionalX && additionalY)
            {
                SetColor(position - new Vector2(1, 1), value);
            }
        }
        public void LoadColorMap()
        {
            var colorMap = Editor.Source.GenerateColorMap(Second);

            for (var x = 0; x < colorMap.GetLength(0) * 2; x += 2)
            {
                for (var y = 0; y < colorMap.GetLength(1) * 2; y += 2)
                {
                    var realX = x / 2;
                    var realY = y / 2;

                    Colors[new Vector2(x, y)] = colorMap[realX, realY];
                    Colors[new Vector2(x + 1, y)] = colorMap[realX, realY];
                    Colors[new Vector2(x, y + 1)] = colorMap[realX, realY];
                    Colors[new Vector2(x + 1, y + 1)] = colorMap[realX, realY];
                }
            }
        }
        public void ApplyColorMap()
        {
            var colorMap = Editor.Source.GenerateColorMap(Second);

            for (var x = 0; x < colorMap.GetLength(0); x++)
            {
                for (var y = 0; y < colorMap.GetLength(1); y++)
                {
                    colorMap[x, y] = Colors[new Vector2(x, y)];
                }
            }

            Editor.Source.ApplyColorMap(colorMap, Second);
        }
    }
}