using System.Numerics;

namespace InfectedRose.Nif;

public struct NiParticleInfo
{
    public Vector3 Velocity;
    public float Age;
    public float Lifespan;
    public float LastUpdate;
    public ushort SpawnGeneration;
    public ushort Code;
}