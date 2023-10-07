using System;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class SizedString : IConstruct
{
    public uint Length;
    public byte[] Value;

    public static implicit operator string(SizedString a) => BitConverter.ToString(a.Value);

    public void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public void Deserialize(BitReader reader)
    {
        Length = reader.Read<uint>();
        Value = reader.ReadArray<byte>(Length);
    }
}