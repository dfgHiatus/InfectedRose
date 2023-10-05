using System;
using SkiaSharp;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Terrain
{
    public class ColorMap : IConstruct
    {
        public int Size { get; set; }
        
        public SKColor[] Data { get; set; }

        public SKColor GetValue(int x, int y)
        {
            return Data[y * Size + x];
        }

        public void SetColor(int x, int y, SKColor value)
        {
            Data[y * Size + x] = value;
        }

        public void Serialize(BitWriter writer)
        {
            writer.Write(Size);

            for (var i = 0; i < Size * Size; i++)
            {
                var color = Data[i];

                var bytes = new[]
                {
                    color.Red,
                    color.Green,
                    color.Blue,
                    color.Alpha,
                };

                var value = BitConverter.ToUInt32(bytes);

                writer.Write(value);
            }
        }

        public void Deserialize(BitReader reader)
        {
            Size = reader.Read<int>();
            
            Data = new SKColor[Size * Size];

            for (var i = 0; i < Data.Length; i++)
            {
                var data = reader.Read<uint>();

                var bytes = BitConverter.GetBytes(data);

                var color = new SKColor(bytes[2], bytes[1], bytes[0], bytes[3]);

                Data[i] = color;
            }
        }

        public static ColorMap Empty
        {
            get
            {
                var map = new ColorMap {Size = 32};
                
                map.Data = new SKColor[map.Size * map.Size];

                for (var i = 0; i < map.Data.Length; i++)
                {
                    map.Data[i] = new SKColor(127, 127, 127); // Creates green
                }

                return map;
            }
        }
    }
}