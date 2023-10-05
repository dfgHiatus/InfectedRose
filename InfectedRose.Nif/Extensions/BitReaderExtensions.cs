using System;
using RakDotNet.IO;

namespace InfectedRose.Nif
{
    public static class BitReaderExtensions
    {
        public static T Read<T>(this BitReader @this, NiFile file) where T : NiObject
        {
            var instance = Activator.CreateInstance<T>();

            instance.File = file;
            
            instance.Deserialize(@this);

            return instance;
        }
        public static T[] ReadArrayN<T>(this BitReader @this, int size, NiFile file) where T : NiObject
        {
            var array = new T[size];
            
            for (var i = 0; i < size; i++) array[i] = @this.Read<T>(file);
            
            return array;
        }
    }
}