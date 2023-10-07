using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiPixelData : NiPixelFormat
{
    public NiRef<NiPalette> Palette;
    public uint NumMipMaps;
    public uint BytesPerPixel;
    public MipMap[] Mipmaps;
    public uint NumPixels;
    public uint NumFaces;
    public byte[] PixelData;

    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        Palette = reader.Read<NiRef<NiPalette>>(File);
        NumMipMaps = reader.Read<uint>();
        BytesPerPixel = reader.Read<uint>();
        Mipmaps = reader.ReadArray<MipMap>(NumMipMaps);
        NumPixels = reader.Read<uint>();
        NumFaces = reader.Read<uint>();
        PixelData = reader.ReadArray<byte>(NumPixels * NumFaces);
    }
}