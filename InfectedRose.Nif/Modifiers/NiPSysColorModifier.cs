using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiPSysColorModifier : NiPSysModifier
{
    public NiRef<NiColorData> Data;
    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        Data = reader.Read<NiRef<NiColorData>>(File);
    }
}