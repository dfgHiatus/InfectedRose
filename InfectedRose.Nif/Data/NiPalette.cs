using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiPalette : NiObject
{
    public bool HasAlpha;
    public uint NumEntries;
    public ByteColor4[] Palette;
    
    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        HasAlpha = reader.ReadBool();
        NumEntries = reader.Read<uint>();
        Palette = reader.ReadArray<ByteColor4>(NumEntries);
    }
}