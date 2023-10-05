using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiTransformInterpolator : NiKeyBasedInterpolator
{
    public NiQuatTransform Transform;
    public NiRef<NiTransformData> Data;
    public override void Serialize(BitWriter writer)
    {
        base.Serialize(writer);
    }

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        
    }
}