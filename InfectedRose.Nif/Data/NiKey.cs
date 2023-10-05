using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiKey<T> : IConstruct where T : struct
{
    public float Time;
    public T Value;
    
    public virtual void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public virtual void Deserialize(BitReader reader)
    {
        Time = reader.Read<float>();
        Value = reader.Read<T>();
    }
}

public class NiKeyQuadratic<T> : NiKey<T> where T : struct
{
    public T Forward;
    public T Backward;
    public override void Serialize(BitWriter writer)
    {
        base.Serialize(writer);
    }

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        Forward = reader.Read<T>();
        Backward = reader.Read<T>();
    }
}

public class NiKeyTBC<T> : NiKey<T> where T : struct
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