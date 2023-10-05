using System;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif
{
    public class NiSkinInstance : NiObject
    {
        public NiRef<NiSkinData> Data;
        public NiRef<NiSkinPartition> Partition;
        public NiRef<NiNode> SkeletonRoot;
        public uint NumBones;
        public NiRef<NiNode>[] Bones;
        public override void Serialize(BitWriter writer)
        {
            throw new NotImplementedException();
        }

        public override void Deserialize(BitReader reader)
        {
            Data = reader.Read<NiRef<NiSkinData>>(File);
            Partition = reader.Read<NiRef<NiSkinPartition>>(File);
            SkeletonRoot = reader.Read<NiRef<NiNode>>(File);
            NumBones = reader.Read<uint>();
            Bones = reader.ReadArrayD<NiRef<NiNode>>((int) NumBones);
        }
    }
}