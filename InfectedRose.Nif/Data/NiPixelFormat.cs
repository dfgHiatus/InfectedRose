using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public abstract class NiPixelFormat : NiObject
{
    public uint PixelFormat;
    public byte BitsPerPixel;
    public uint RendererHint;
    public uint ExtraData;
    public byte Flags;
    public uint Tiling;
    public bool SRGBSpace;
    public PixelFormatComponent[] Channels;

    public override void Deserialize(BitReader reader)
    {
        PixelFormat = reader.Read<uint>();
        BitsPerPixel = reader.Read<byte>();
        RendererHint = reader.Read<uint>();
        ExtraData = reader.Read<uint>();
        Flags = reader.Read<byte>();
        Tiling = reader.Read<uint>();
        SRGBSpace = reader.ReadBool();
        Channels = reader.ReadArrayD<PixelFormatComponent>(4);
    }
}