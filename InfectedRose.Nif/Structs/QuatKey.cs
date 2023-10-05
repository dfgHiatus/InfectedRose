using System.Numerics;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class QuatKey : IConstruct
{
    public float Time;
    public NiQuaternion Value;
    
    public virtual void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public virtual void Deserialize(BitReader reader)
    {
        Time = reader.Read<float>();
        Value = reader.Read<NiQuaternion>();
    }
}

public class QuatKey_TBC : QuatKey
{
    public TBC TBC;
    public override void Serialize(BitWriter writer)
    {
        base.Serialize(writer);
    }
    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        TBC = reader.Read<TBC>();
    }
}