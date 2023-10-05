using System;
using System.Collections.Generic;
using InfectedRose.Core;
using RakDotNet.IO;
using SkiaSharp;

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

        public float[,] GenerateHeightMap(string save = default)
        {
            var weight = Chunks[0].HeightMap.Width;
            var height = Chunks[0].HeightMap.Height;
            
            var heights = new float[
                Weight * weight,
                Height * height
            ];

            var map = new SKBitmap(Weight * weight, Height * height);

            for (var chunkY = 0; chunkY < Height; ++chunkY)
            {
                for (var chunkX = 0; chunkX < Weight; ++chunkX)
                {
                    var chunk = Chunks[chunkY * Weight + chunkX];

                    for (var y = 0; y < chunk.HeightMap.Height; ++y)
                    {
                        for (var x = 0; x < chunk.HeightMap.Width; ++x)
                        {
                            var value = chunk.HeightMap.GetValue(x, y);
                            var pixelX = chunkX * weight + x;
                            var pixelY = chunkY * height + y;

                            heights[pixelX, pixelY] = value;
                            
                            var bytes = BitConverter.GetBytes(value);

                            map.SetPixel(pixelX, pixelY, new SKColor(bytes[1], bytes[2], bytes[3], bytes[0]));
                        }
                    }
                }
            }
            //if (!string.IsNullOrWhiteSpace(save)) map.Save(save);
            //map.RotateFlip(RotateFlipType.Rotate90FlipX);
            
            var w = Weight * weight;
            var h = Height * height;
            var a = new float[w,h];
            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
                a[w - x - 1, y] = heights[x, y];
            return a;
        }
        
        public SKColor[,] GenerateColorMap(bool second = false)
        {
            var weight = second ? Chunks[0].Colormap1.Size : Chunks[0].Colormap0.Size;
            var height = second ? Chunks[0].Colormap1.Size : Chunks[0].Colormap0.Size;
            
            var colors = new SKColor[
                Weight * weight,
                Height * height
            ];

            var map = new SKBitmap(Weight * weight, Height * height);

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
                            var value = colorMap.GetValue(x, y);

                            var pixelX = chunkX * weight + x;
                            var pixelY = chunkY * height + y;

                            colors[pixelX, pixelY] = value;

                            map.SetPixel(pixelX, pixelY, value);
                        }
                    }
                }
            }
            //map = map.FlipX();
            //map.RotateFlip(RotateFlipType.Rotate90FlipX);

            for (var x = 0; x < map.Width; x++)
            {
                for (var y = 0; y < map.Height; y++)
                {
                    var color = map.GetPixel(x, y);

                    colors[x, y] = color;
                }
            }
            
            return colors;
        }

        public void ApplyHeightMap(float[,] heightMap)
        {
            var weight = Chunks[0].HeightMap.Width;
            var height = Chunks[0].HeightMap.Height;
            
            var map = new SKBitmap(Weight * weight, Height * height);

            for (var x = 0; x < map.Width; x++)
            {
                for (var y = 0; y < map.Height; y++)
                {
                    var value = heightMap[x, y];
                    
                    var bytes = BitConverter.GetBytes(value);

                    map.SetPixel(x, y, new SKColor(bytes[1], bytes[2], bytes[3], bytes[0]));
                }
            }
            
            //map = map.FlipX();
            //map.RotateFlip(RotateFlipType.Rotate90FlipX);
            
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
                            
                            var color = map.GetPixel(pixelX, pixelY);
                            
                            var bytes = new[]
                            {
                                color.Alpha,
                                color.Red,
                                color.Green,
                                color.Blue
                            };

                            var value = BitConverter.ToSingle(bytes);

                            chunk.HeightMap.SetValue(x, y, value);
                        }
                    }
                }
            }
        }

        public void ApplyColorMap(SKColor[,] colors, bool second = false)
        {
            var weight = second ? Chunks[0].Colormap1.Size : Chunks[0].Colormap0.Size;
            var height = second ? Chunks[0].Colormap1.Size : Chunks[0].Colormap0.Size;

            var map = new SKBitmap(Weight * weight, Height * height);

            for (var x = 0; x < map.Width; x++)
            {
                for (var y = 0; y < map.Height; y++)
                {
                    var value = colors[x, y];

                    map.SetPixel(x, y, value);
                }
            }

            //map = map.FlipX();
            //map.RotateFlip(RotateFlipType.Rotate90FlipX);

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

                            var color = map.GetPixel(pixelX, pixelY);

                            colorMap.SetColor(x, y, color);
                        }
                    }
                }
            }
        }
    }
}