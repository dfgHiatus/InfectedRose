using System.Numerics;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiPSysRotationModifier : NiPSysModifier
{
    public float RotationSpeed;
    public float RotationSpeedVariation;
    public float RotationAngle;
    public float RotationAngleVariation;
    public bool RandomRotSpeedSign;
    public bool RandomAxis;
    public Vector3 Axis;
    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        RotationSpeed = reader.Read<float>();
        RotationSpeedVariation = reader.Read<float>();
        RotationAngle = reader.Read<float>();
        RotationAngleVariation = reader.Read<float>();
        RandomRotSpeedSign = reader.ReadBool();
        RandomAxis = reader.ReadBool();
        Axis = reader.Read<Vector3>();
    }
}