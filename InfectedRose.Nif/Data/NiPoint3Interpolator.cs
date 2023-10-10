using System.Numerics;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiPoint3Interpolator : NiKeyBasedInterpolator
{
    public Vector3 Value;
    public NiRef<NiPosData> Data;
    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        Value = reader.Read<Vector3>();
        Data = reader.Read<NiRef<NiPosData>>(File);
    }
}