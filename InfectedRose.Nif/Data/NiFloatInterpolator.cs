using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiFloatInterpolator : NiKeyBasedInterpolator
{
    public float Value;
    public NiRef<NiFloatData> Data;

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        Value = reader.Read<float>();
        Data = reader.Read<NiRef<NiFloatData>>(File);
    }
}