using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiPSysBoxEmitter : NiPSysVolumeEmitter
{
    //vector3?
    public float Width;
    public float Height;
    public float Depth;
    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        Width = reader.Read<float>();
        Height = reader.Read<float>();
        Depth = reader.Read<float>();
    }
}