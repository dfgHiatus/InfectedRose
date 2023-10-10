using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public abstract class NiPSysModifier : NiObject
{
    public NiStringRef Name;
    public uint Order;
    public NiRef<NiParticleSystem> Target;
    public bool Active;
    
    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        Name = reader.Read<NiStringRef>(File);
        Order = reader.Read<uint>();
        Target = reader.Read<NiRef<NiParticleSystem>>(File);
        Active = reader.ReadBool();
    }
}