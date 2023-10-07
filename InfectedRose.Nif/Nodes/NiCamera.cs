using System;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiCamera : NiAvObject
{
    public ushort CameraFlags; //obsolete
    public float FrustumLeft;
    public float FrustumRight;
    public float FrustumTop;
    public float FrustumBottom;
    public float FrustumNear;
    public float FrustumFar;
    public bool Orthographic;
    public float ViewportLeft;
    public float ViewportRight;
    public float ViewportTop;
    public float ViewportBottom;
    public float LODAdjust;
    public NiRef<NiAvObject> Scene;
    public uint NumScreenPolygons; //always 0, deprecated;
    public uint NumScreenTextures; //always 0, deprecated;
    
    public override void Serialize(BitWriter writer)
    {
        throw new NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);

        CameraFlags = reader.Read<ushort>();
        FrustumLeft = reader.Read<float>();
        FrustumRight = reader.Read<float>();
        FrustumTop = reader.Read<float>();
        FrustumBottom = reader.Read<float>();
        FrustumNear = reader.Read<float>();
        FrustumFar = reader.Read<float>();
        Orthographic = reader.ReadBool();
        ViewportLeft = reader.Read<float>();
        ViewportRight = reader.Read<float>();
        ViewportTop = reader.Read<float>();
        ViewportBottom = reader.Read<float>();
        LODAdjust = reader.Read<float>();
        Scene = reader.Read<NiRef<NiAvObject>>(File);
        NumScreenPolygons = reader.Read<uint>();
        NumScreenTextures = reader.Read<uint>();
    }
}