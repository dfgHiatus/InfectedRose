using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiPSysBoundUpdateModifier : NiPSysModifier
{
    public ushort UpdateSkip;
    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        UpdateSkip = reader.Read<ushort>();
    }
}