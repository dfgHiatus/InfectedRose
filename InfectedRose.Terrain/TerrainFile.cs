using System;
using System.Collections.Generic;
using System.Linq;
using InfectedRose.Core;
using InfectedRose.Nif;
using RakDotNet.IO;

namespace InfectedRose.Terrain
{
    public class TerrainFile : IConstruct
    {
        public List<Chunk> Chunks { get; set; }

        public int ChunkTotalCount { get; set; }

        public int Weight { get; set; }

        public int Height { get; set; }

        public byte[] Magic { get; set; } = {0x20, 0x00, 0x00};

        public void Serialize(BitWriter writer)
        {
            writer.Write(Magic);

            writer.Write(ChunkTotalCount);
            writer.Write(Weight);
            writer.Write(Height);

            foreach (var chunk in Chunks)
            {
                chunk.Serialize(writer);
            }
        }

        public void Deserialize(BitReader reader)
        {
            Chunks = new List<Chunk>();

            Magic = reader.ReadBuffer(3);
            
            ChunkTotalCount = reader.Read<int>();
            Weight = reader.Read<int>();
            Height = reader.Read<int>();

            for (var i = 0; i < ChunkTotalCount; i++)
            {
                var chunk = new Chunk();
                chunk.Deserialize(reader);
                
                Chunks.Add(chunk);
            }
        }

        public Chunk GetChunk(int x, int y)
        {
            return Chunks[y * Weight + x];
        }
        public float[,] GenerateHeightMap(string save = default)
        {
            var weight = Chunks.Max(i => i.HeightMap.Width);
            var height = Chunks.Max(i => i.HeightMap.Height);

            var w = Weight * weight;
            var h = Height * height;
            
            var heights = new float[
                w,
                h
            ];

            for (var i = 0; i < w; i++)
            {
                var x = (double) i / w;
                var partX = x * Weight;
                var chunkX = (int)Math.Floor(partX);
                var insideX = partX - chunkX;
                for (var j = 0; j < h; j++)
                {
                    var y = (double) j / h;
                    var partY = y * Height;
                    var chunkY = (int)Math.Floor(partY);
                    var insideY = partY - chunkY;
                    var chunk = GetChunk(chunkX, chunkY);
                    var insidePixelX = (int) Math.Floor(insideX * chunk.HeightMap.Width);
                    var insidePixelY = (int) Math.Floor(insideY * chunk.HeightMap.Height);
                    heights[i, j] = chunk.HeightMap.GetValue(insidePixelX, insidePixelY);
                }
            }
            return heights;
        }
        
        public ByteColor4[,] GenerateColorMap(bool second = false)
        {
            var weight = Chunks.Max(i => second ? i.Colormap1.Size : i.Colormap0.Size);
            var height = weight;
            
            var w = Weight * weight;
            var h = Height * height;

            var colors = new ByteColor4[
                w,
                h
            ];
            for (var i = 0; i < w; i++)
            {
                var x = (double) i / w;
                var partX = x * Weight;
                var chunkX = (int)Math.Floor(partX);
                var insideX = partX - chunkX;
                for (var j = 0; j < h; j++)
                {
                    var y = (double) j / h;
                    var partY = y * Height;
                    var chunkY = (int)Math.Floor(partY);
                    var insideY = partY - chunkY;
                    var chunk = GetChunk(chunkX, chunkY);

                    var colorMap = second ? chunk.Colormap1 : chunk.Colormap0;
                    
                    var insidePixelX = (int) Math.Floor(insideX * colorMap.Size);
                    var insidePixelY = (int) Math.Floor(insideY * colorMap.Size);
                    colors[i, j] = colorMap.GetValue(insidePixelX, insidePixelY);
                }
            }
            return colors;
        }

        public void ApplyHeightMap(float[,] heightMap)
        {
            var weight = Chunks[0].HeightMap.Width;
            var height = Chunks[0].HeightMap.Height;
            
            for (var chunkY = 0; chunkY < Height; ++chunkY)
            {
                for (var chunkX = 0; chunkX < Weight; ++chunkX)
                {
                    var chunk = Chunks[chunkY * Weight + chunkX];

                    for (var y = 0; y < chunk.HeightMap.Height; ++y)
                    {
                        for (var x = 0; x < chunk.HeightMap.Width; ++x)
                        {
                            var pixelX = chunkX * weight + x;
                            var pixelY = chunkY * height + y;
                            chunk.HeightMap.SetValue(x, y, heightMap[pixelX, pixelY]);
                        }
                    }
                }
            }
        }

        public void ApplyColorMap(ByteColor4[,] colors, bool second = false)
        {
            var weight = second ? Chunks[0].Colormap1.Size : Chunks[0].Colormap0.Size;
            var height = second ? Chunks[0].Colormap1.Size : Chunks[0].Colormap0.Size;
            
            for (var chunkY = 0; chunkY < Height; ++chunkY)
            {
                for (var chunkX = 0; chunkX < Weight; ++chunkX)
                {
                    var chunk = Chunks[chunkY * Weight + chunkX];

                    var colorMap = second ? chunk.Colormap1 : chunk.Colormap0;

                    for (var y = 0; y < weight; ++y)
                    {
                        for (var x = 0; x < height; ++x)
                        {
                            var pixelX = chunkX * weight + x;
                            var pixelY = chunkY * height + y;

                            var color = colors[pixelX, pixelY];

                            colorMap.SetColor(x, y, color);
                        }
                    }
                }
            }
        }
    }
}