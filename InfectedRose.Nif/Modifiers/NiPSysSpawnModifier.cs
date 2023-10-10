using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiPSysSpawnModifier : NiPSysModifier
{
    public ushort NumSpawnGenerations;
    public float PercentageSpawned;
    public ushort MinNumToSpawn;
    public ushort MaxNumToSpawn;
    public float SpawnSpeedVariation;
    public float SpawnDirVariation;
    public float LifeSpan;
    public float LifeSpanVariation;
    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        NumSpawnGenerations = reader.Read<ushort>();
        PercentageSpawned = reader.Read<float>();
        MinNumToSpawn = reader.Read<ushort>();
        MaxNumToSpawn = reader.Read<ushort>();
        SpawnSpeedVariation = reader.Read<float>();
        SpawnDirVariation = reader.Read<float>();
        LifeSpan = reader.Read<float>();
        LifeSpanVariation = reader.Read<float>();
    }
}