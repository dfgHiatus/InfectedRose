using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiSkinPartition : NiObject
{
    public uint NumPartitions;
    public SkinPartition[] Partitions;
    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        NumPartitions = reader.Read<uint>();
        Partitions = reader.ReadArrayD<SkinPartition>((int) NumPartitions);
    }
}