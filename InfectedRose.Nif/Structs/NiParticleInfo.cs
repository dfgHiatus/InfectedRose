using System.Numerics;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiParticleInfo : IConstruct
{
    public Vector3 Velocity;
    public float Age;
    public float Lifespan;
    public float LastUpdate;
    public ushort SpawnGeneration;
    public ushort Code;
    public void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public void Deserialize(BitReader reader)
    {
        Velocity = reader.Read<Vector3>();
        Age = reader.Read<float>();
        Lifespan = reader.Read<float>();
        LastUpdate = reader.Read<float>();
        SpawnGeneration = reader.Read<ushort>();
        Code = reader.Read<ushort>();
    }
}