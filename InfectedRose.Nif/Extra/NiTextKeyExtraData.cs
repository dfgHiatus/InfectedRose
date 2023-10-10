using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiTextKeyExtraData : NiExtraData
{
    public uint NumTextKeys;
    public NiKeyObject<NiStringRef> TextKeys;

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        NumTextKeys = reader.Read<uint>();
        TextKeys = reader.Read<NiKeyObject<NiStringRef>>(File);
    }
}