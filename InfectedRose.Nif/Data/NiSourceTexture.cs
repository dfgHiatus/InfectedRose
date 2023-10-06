using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiSourceTexture : NiTexture
{
    public bool UseExternal;
    public NiStringRef Path;
    public NiRef<NiPixelFormat> PixelData;
    public FormatPrefs FormatPrefs;
    public bool IsStatic;
    public bool DirectRender;
    public bool PersistentRenderData;

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        UseExternal = reader.ReadBool();
        Path = reader.Read<NiStringRef>();
        PixelData = reader.Read<NiRef<NiPixelFormat>>();
        FormatPrefs = reader.Read<FormatPrefs>();
        IsStatic = reader.ReadBool();
        DirectRender = reader.ReadBool();
        PersistentRenderData = reader.ReadBool();
    }
}