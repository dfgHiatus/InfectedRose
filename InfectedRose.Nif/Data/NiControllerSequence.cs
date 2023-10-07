using InfectedRose.Nif.Controllers;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiControllerSequence : NiSequence
{
    public float Weight;
    public NiRef<NiTextKeyExtraData> TextKeys;
    public uint CycleType; //0 = loop, 1 = reverse, 2 = clamp
    public float Frequency;
    public float StartTime;
    public float StopTime;
    public NiRef<NiControllerManager> Manager;
    public NiStringRef AccumRootName;
    public uint AccumFlags; //enum AccumFlags
    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        Weight = reader.Read<float>();
        TextKeys = reader.Read<NiRef<NiTextKeyExtraData>>(File);
        CycleType = reader.Read<uint>();
        Frequency = reader.Read<float>();
        StartTime = reader.Read<float>();
        StopTime = reader.Read<float>();
        Manager = reader.Read<NiRef<NiControllerManager>>(File);
        AccumRootName = reader.Read<NiStringRef>(File);
        AccumFlags = reader.Read<uint>();
    }
}