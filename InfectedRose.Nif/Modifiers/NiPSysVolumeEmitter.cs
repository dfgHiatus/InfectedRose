using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiPSysVolumeEmitter : NiPSysEmitter
{
    public NiRef<NiNode> EmitterObject;
    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        EmitterObject = reader.Read<NiRef<NiNode>>(File);
    }
}