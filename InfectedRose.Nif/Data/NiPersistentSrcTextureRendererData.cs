using System;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiPersistentSrcTextureRendererData : NiPixelFormat
{
    public NiRef<NiPalette> Palette;
    public uint NumMipMaps;
    public uint BytesPerPixel;
    public MipMap[] MipMaps;
    public uint NumPixels;
    public uint PadNumPixels;
    public uint NumFaces;
    public uint Platform;
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
        MipMaps = reader.ReadArray<MipMap>(NumMipMaps);
        NumPixels = reader.Read<uint>();
        PadNumPixels = reader.Read<uint>();
        NumFaces = reader.Read<uint>();
        Platform = reader.Read<uint>();
        PixelData = reader.ReadArray<byte>(NumPixels * NumFaces);
    }
}