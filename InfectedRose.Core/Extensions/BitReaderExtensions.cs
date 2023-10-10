using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using RakDotNet.IO;

namespace InfectedRose.Core
{
    public static class BitReaderExtensions
    {
        public static string ReadNiString(this BitReader @this, bool wide = false, bool small = false)
        {
            var len = small ? @this.Read<byte>() : @this.Read<uint>();
            var str = new char[len];

            for (var i = 0; i < len; i++)
            {
                str[i] = (char) (wide ? @this.Read<ushort>() : @this.Read<byte>());
            }

            return new string(str);
        }

        public static Quaternion ReadNiQuaternion(this BitReader @this)
        {
            return new Quaternion
            {
                W = @this.Read<float>(),
                X = @this.Read<float>(),
                Y = @this.Read<float>(),
                Z = @this.Read<float>()
            };
        }

        public static byte[] ReadBuffer(this BitReader @this, uint length)
        {
            var buffer = new byte[length];

            for (var i = 0; i < length; i++)
            {
                buffer[i] = @this.Read<byte>();
            }

            return buffer;
        }

        public static T Read<T>(this BitReader @this) where T : IDeserializable
        {
            var instance = Activator.CreateInstance<T>();
            
            instance.Deserialize(@this);

            return instance;
        }
        
        public static T[] ReadArrayD<T>(this BitReader @this, uint size) where T : IDeserializable
        {
            var array = new T[size];
            
            for (var i = 0; i < size; i++) array[i] = @this.Read<T>();
            
            return array;
        }
        
        public static T[] ReadArrayD<T>(this BitReader @this, int size) where T : IDeserializable
        {
            var array = new T[size];
            
            for (var i = 0; i < size; i++) array[i] = @this.Read<T>();
            
            return array;
        }
        
        public static T[] ReadArray<T>(this BitReader @this, uint size) where T : struct
        {
            var array = new T[size];
            
            for (var i = 0; i < size; i++) array[i] = @this.Read<T>();
            
            return array;
        }

        public static T[] ReadArray<T>(this BitReader @this, int size) where T : struct
        {
            var array = new T[size];
            
            for (var i = 0; i < size; i++) array[i] = @this.Read<T>();
            
            return array;
        }
        
        public static bool ReadBool(this BitReader @this)
        {
            return @this.Read<byte>() != 0;
        }

        public static T[][] Read2DArray<T>(this BitReader @this, int size1, int size2) where T : struct
        {
            var array = new T[size1][];
            
            for (var i = 0; i < size1; i++) array[i] = @this.ReadArray<T>(size2);
            
            return array;
        }
        public static T[][] Read2DArray<T>(this BitReader @this, int size1, int[] size2) where T : struct
        {
            var array = new T[size1][];
            
            for (var i = 0; i < size1; i++) array[i] = @this.ReadArray<T>(size2[i]);
            
            return array;
        }
        public static T[][] Read2DArray<T>(this BitReader @this, int size1, ushort[] size2) where T : struct
        {
            var array = new T[size1][];
            
            for (var i = 0; i < size1; i++) array[i] = @this.ReadArray<T>(size2[i]);
            
            return array;
        }
    }
}