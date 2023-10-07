using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiParticleSystem : NiParticles
{
    public bool WorldSpace;
    public uint NumModifiers;
    public NiRef<NiPSysModifier>[] Modifiers;

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        WorldSpace = reader.ReadBool();
        NumModifiers = reader.Read<uint>();
        Modifiers = reader.ReadArrayN<NiRef<NiPSysModifier>>((int) NumModifiers, File);
    }
}