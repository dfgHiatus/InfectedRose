using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiPSysAgeDeathModifier : NiPSysModifier
{
    public bool SpawnOnDeath;
    public NiRef<NiPSysSpawnModifier> SpawnModifier;

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        SpawnOnDeath = reader.ReadBool();
        SpawnModifier = reader.Read<NiRef<NiPSysSpawnModifier>>(File);
    }
}