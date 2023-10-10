using System;
using System.Numerics;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiParticlesData : NiGeometryData
{
    public bool HasRadii;
    public float[] Radii;
    public ushort NumActive;
    public bool HasSizes;
    public float[] Sizes;
    public bool HasRotations;
    public NiQuaternion[] Rotations;
    public bool HasRotationAngles;
    public float[] RotationAngles;
    public bool HasRotationAxes;
    public Vector3[] RotationAxes;

    public override void Serialize(BitWriter writer)
    {
        throw new NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        HasRadii = reader.ReadBool();
        if (HasRadii) Radii = reader.ReadArray<float>(NumVertices);
        NumActive = reader.Read<ushort>();
        HasSizes = reader.ReadBool();
        if (HasSizes) Sizes = reader.ReadArray<float>(NumVertices);
        HasRotations = reader.ReadBool();
        if (HasRotations) Rotations = reader.ReadArray<NiQuaternion>(NumVertices);
        HasRotationAngles = reader.ReadBool();
        if (HasRotationAngles) RotationAngles = reader.ReadArray<float>(NumVertices);
        HasRotationAxes = reader.ReadBool();
        if (HasRotationAxes) RotationAxes = reader.ReadArray<Vector3>(NumVertices);
    }
}