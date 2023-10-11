using System;
using InfectedRose.Core;
using InfectedRose.Nif;
using RakDotNet.IO;

namespace InfectedRose.Terrain
{
    public class ColorMap : IConstruct
    {
        public int Size { get; set; }
        
        public ByteColor4[] Data { get; set; }

        public ByteColor4 GetValue(int x, int y)
        {
            return Data[y * Size + x];
        }

        public void SetColor(int x, int y, ByteColor4 value)
        {
            Data[y * Size + x] = value;
        }

        public void Serialize(BitWriter writer)
        {
            writer.Write(Size);

            for (var i = 0; i < Size * Size; i++)
            {
                writer.Write(Data[i]);
            }
        }

        public void Deserialize(BitReader reader)
        {
            Size = reader.Read<int>();
            
            Data = new ByteColor4[Size * Size];

            for (var i = 0; i < Data.Length; i++)
            {
                Data[i] = reader.Read<ByteColor4>();
            }
        }

        public static ColorMap Empty
        {
            get
            {
                var map = new ColorMap {Size = 32};
                
                map.Data = new ByteColor4[map.Size * map.Size];

                for (var i = 0; i < map.Data.Length; i++)
                {
                    map.Data[i] = new ByteColor4{ G = 255 }; // Creates green
                }

                return map;
            }
        }
    }
}