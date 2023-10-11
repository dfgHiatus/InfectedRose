using System.Runtime.InteropServices;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class PixelFormatComponent : IConstruct
{
    public uint Type;
    public uint Convention;
    public byte BitsPerChannel;
    public byte IsSigned;
    public void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public void Deserialize(BitReader reader)
    {
        Type = reader.Read<uint>();
        Convention = reader.Read<uint>();
        BitsPerChannel = reader.Read<byte>();
        IsSigned = reader.Read<byte>();
    }
}