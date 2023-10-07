using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiPSysData : NiParticlesData
{
    public NiParticleInfo[] ParticleInfo;
    public bool HasRotationSpeeds;
    public float[] RotationSpeeds;
    public ushort NumAddedParticles;
    public ushort AddedParticlesBase;

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        ParticleInfo = reader.ReadArray<NiParticleInfo>(NumVertices);
        HasRotationSpeeds = reader.ReadBool();
        if (HasRotationSpeeds) RotationSpeeds = reader.ReadArray<float>(NumVertices);
        NumAddedParticles = reader.Read<ushort>();
        AddedParticlesBase = reader.Read<ushort>();
    }
}