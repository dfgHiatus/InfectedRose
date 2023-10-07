using System.Numerics;
using RakDotNet.IO;
using InfectedRose.Core;

namespace InfectedRose.Nif;

public class NiPosData : NiObject
{
    public NiKeyGroup<Vector3> Data;
    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        Data = reader.Read<NiKeyGroup<Vector3>>();
    }
}