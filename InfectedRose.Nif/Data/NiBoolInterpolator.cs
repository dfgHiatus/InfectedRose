using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiBoolInterpolator : NiKeyBasedInterpolator
{
    public bool Value;
    public NiRef<NiBoolData> Data;

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        Value = reader.ReadBool();
        Data = reader.Read<NiRef<NiBoolData>>(File);
    }
}