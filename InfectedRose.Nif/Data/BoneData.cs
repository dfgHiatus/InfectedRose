using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class BoneData : IConstruct
{
    public NiTransform SkinTransform;
    public NiBound BoundingSphere;
    public ushort NumVertices;

    public virtual void Serialize(BitWriter writer)
    {
        writer.Write(SkinTransform);
        writer.Write(BoundingSphere);
        writer.Write(NumVertices);
    }

    public virtual void Deserialize(BitReader reader)
    {
        SkinTransform = reader.Read<NiTransform>();
        BoundingSphere = reader.Read<NiBound>();
        NumVertices = reader.Read<ushort>();
    }
}

public class WeightedBoneData : BoneData
{
    public BoneVertData[] VertexWeights;
    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        VertexWeights = new BoneVertData[NumVertices];
        for (var i = 0; i < NumVertices; i++)
        {
            VertexWeights[i] = reader.Read<BoneVertData>();
        }
    }

    public override void Serialize(BitWriter writer)
    {
        base.Serialize(writer);
        for (var i = 0; i < NumVertices; i++)
        {
            writer.Write(VertexWeights[i]);
        }
    }
}