using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiSequence : NiObject
{
    public NiStringRef Name;
    public uint NumControlledBlocks;
    public uint ArrayGrowBy;
    public ControlledBlock[] ControlledBlocks;
    
    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        Name = reader.Read<NiStringRef>(File);
        NumControlledBlocks = reader.Read<uint>();
        ArrayGrowBy = reader.Read<uint>();
        ControlledBlocks = reader.ReadArrayN<ControlledBlock>((int)NumControlledBlocks, File);
    }
}