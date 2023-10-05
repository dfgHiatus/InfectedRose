using System.Collections.Generic;
using System.Linq;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiSkinData : NiObject
{
    public NiTransform SkinTransform;
    public uint NumBones;
    public bool HasVertexWeights;
    public List<BoneData> BoneList;
    public override void Serialize(BitWriter writer)
    {
        
    }

    public override void Deserialize(BitReader reader)
    {
        SkinTransform = reader.Read<NiTransform>();
        NumBones = reader.Read<uint>();
        HasVertexWeights = reader.ReadBool();
        if (HasVertexWeights)
            BoneList = reader.ReadArrayD<WeightedBoneData>((int) NumBones).Select(i => i as BoneData).ToList();
        else
            BoneList = reader.ReadArrayD<BoneData>((int) NumBones).ToList();
    }
}