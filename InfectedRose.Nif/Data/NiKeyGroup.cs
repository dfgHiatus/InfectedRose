using System.Collections.Generic;
using System.Linq;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiKeyGroup<T> : IConstruct where T : struct
{
    public uint NumKeys;
    public uint Interpolation;
    public List<NiKey<T>> Keys;
    
    public void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public void Deserialize(BitReader reader)
    {
        NumKeys = reader.Read<uint>();
        Interpolation = reader.Read<uint>();
        Keys = Interpolation switch
        {
            KeyType.QUADRATIC_KEY => new List<NiKey<T>>(reader.ReadArrayD<NiKeyQuadratic<T>>((int) NumKeys)),
            KeyType.TBC_KEY => new List<NiKey<T>>(reader.ReadArrayD<NiKeyTBC<T>>((int) NumKeys)),
            _ => reader.ReadArrayD<NiKey<T>>((int) NumKeys).ToList()
        };
    }
}