using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiBillboardNode : NiNode
{
    public ushort BillboardMode;
    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        BillboardMode = reader.Read<ushort>();
    }
}