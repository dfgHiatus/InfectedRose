using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NodeSet : NiObject
{
    public uint NumNodes;
    public NiRef<NiNode>[] Nodes;
    
    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }
    public override void Deserialize(BitReader reader)
    {
        NumNodes = reader.Read<uint>();
        Nodes = reader.ReadArrayN<NiRef<NiNode>>((int) NumNodes, File);
    }
}