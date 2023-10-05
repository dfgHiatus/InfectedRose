using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;

namespace InfectedRose.Terrain.Pipeline
{
    public static class TerrainFileExtensions
    {
        public struct TerrainTriData
        {
            public Vector3 V1;
            public Vector2 UV1;
            public Vector3 V2;
            public Vector2 UV2;
            public Vector3 V3;
            public Vector2 UV3;
        }

        public static TerrainTriData[] ExportTriData(this TerrainFile terrain)
        {
            var heightMap = terrain.GenerateRealMap();
            var width = heightMap.GetLength(0);
            float uvW = width - 1;
            var height = heightMap.GetLength(1);
            float uvH = height - 1;
            bool TryGetValue(int x, int y, out Vector3 value)
            {
                try
                {
                    value = heightMap[x, y];
                    return true;
                }
                catch
                {
                    value = Vector3.Zero;
                    return false;
                }
            }
            var triangles = new List<TerrainTriData>();

            for (var y = 0; y < height; y += 1)
            {
                for (var x = 0; x < width; x += 1)
                {
                    {
                        if (!TryGetValue(x, y, out var source))
                            continue;
                        if (!TryGetValue(x + 1, y, out var next))
                            continue;
                        if (!TryGetValue(x, y + 1, out var top))
                            continue;
                        var triangle = new TerrainTriData()
                        {
                            V1 = top,
                            UV1 = new Vector2(x / uvW, (y+1) / uvH),
                            V2 = next,
                            UV2 = new Vector2((x+1) / uvW, y / uvH),
                            V3 = source,
                            UV3 = new Vector2(x / uvW, y / uvH),
                        };
                        triangles.Add(triangle);
                    }
                    {
                        if (!TryGetValue(x, y + 1, out var source))
                            continue;
                        if (!TryGetValue(x + 1, y + 1, out var next))
                            continue;
                        if (!TryGetValue(x + 1, y, out var top))
                            continue;
                        var triangle = new TerrainTriData()
                        {
                            V1 = top,
                            UV1 = new Vector2((x+1) / uvW, y / uvH),
                            V2 = next,
                            UV2 = new Vector2((x+1) / uvW, (y+1) / uvH),
                            V3 = source,
                            UV3 = new Vector2(x / uvW, (y+1) / uvH),
                        };
                        triangles.Add(triangle);
                    }
                }
            }

            return triangles.ToArray();
        }

        public static string ExportAsObj(this TerrainFile @this, int offset, out int newOffset)
        {
            var triangles = @this.Triangulate();
            
            var obj = new StringBuilder();

            foreach (var triangle in triangles.SelectMany(t => t.ToArray))
            {
                obj.AppendLine($"v {triangle.X.ToString(CultureInfo.InvariantCulture)}" +
                               $" {triangle.Y.ToString(CultureInfo.InvariantCulture)}" +
                               $" {triangle.Z.ToString(CultureInfo.InvariantCulture)}"
                );
            }

            for (var i = 0; i < triangles.Length; i++)
            {
                var index = i * 3 + 1 + offset;

                obj.AppendLine($"f {index}//{index} {index + 1}//{index + 1} {index + 2}//{index + 2}");
            }

            newOffset = offset + triangles.Length * 3;
            
            return obj.ToString();
        }

        public static Triangle[] Triangulate(this TerrainFile @this)
        {
            var heightMap = @this.GenerateRealMap();

            var weight = heightMap.GetLength(0);
            var height = heightMap.GetLength(1);

            bool TryGetValue(int x, int y, out Vector3 value)
            {
                try
                {
                    value = heightMap[x, y];
                    return true;
                }
                catch
                {
                    value = Vector3.Zero;
                    return false;
                }
            }

            var triangles = new List<Triangle>();

            for (var y = 0; y < height; y += 1)
            {
                for (var x = 0; x < weight; x += 1)
                {
                    {
                        if (!TryGetValue(x, y, out var source))
                            continue;
                        if (!TryGetValue(x + 1, y, out var next))
                            continue;
                        if (!TryGetValue(x, y + 1, out var top))
                            continue;

                        var triangle = new Triangle
                        {
                            Position1 = top,
                            Position2 = next,
                            Position3 = source
                        };

                        triangles.Add(triangle);
                    }
                    {
                        if (!TryGetValue(x, y + 1, out var source))
                            continue;
                        if (!TryGetValue(x + 1, y + 1, out var next))
                            continue;
                        if (!TryGetValue(x + 1, y, out var top))
                            continue;

                        var triangle = new Triangle
                        {
                            Position1 = source,
                            Position2 = next,
                            Position3 = top
                        };

                        triangles.Add(triangle);
                    }
                }
            }
            return triangles.ToArray();
        }

        public static Vector3[,] GenerateRealMap(this TerrainFile @this)
        {
            const float scale = 3.125f;
            
            var heightMap = @this.GenerateHeightMap();

            var centerX = (heightMap.GetLength(0) - 1) / 2;
            var centerY = (heightMap.GetLength(1) - 1) / 2;
            
            var weight = heightMap.GetLength(0);
            var height = heightMap.GetLength(1);

            var inGameValues = new Vector3[weight, height];

            for (var x = 0; x < weight; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var value = heightMap[x, y];

                    var realX = x - centerX;
                    var realY = y - centerY;
                    
                    var inGame = new Vector2(realX, realY);

                    inGame *= scale;

                    inGameValues[x, y] = new Vector3(inGame.X, value, inGame.Y);
                }
            }

            return inGameValues;
        }
    }
}