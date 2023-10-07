using System.Numerics;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiPSysGravityModifier : NiPSysModifier
{
    public NiRef<NiAvObject> GravityObject;
    public Vector3 GravityAxis;
    public float Decay;
    public float Strength;
    public uint ForceType; //0 = planar, 1 = spherical, 2 = unknown
    public float Turbulence;
    public float TurbulenceScale;

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        GravityObject = reader.Read<NiRef<NiAvObject>>(File);
        GravityAxis = reader.Read<Vector3>();
        Decay = reader.Read<float>();
        Strength = reader.Read<float>();
        ForceType = reader.Read<uint>();
        Turbulence = reader.Read<float>();
        TurbulenceScale = reader.Read<float>();
    }
}