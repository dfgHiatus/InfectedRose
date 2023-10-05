using System;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class SkinPartition : IConstruct
{
    public ushort NumVertices;
    public ushort NumTriangles;
    public ushort NumBones;
    public ushort NumStrips;
    public ushort NumWeightsPerVertex;
    public ushort[] Bones;
    public bool HasVertexMap;
    public ushort[] VertexMap;
    public bool HasVertexWeights;
    public float[][] VertexWeights;
    public ushort[] StripLengths;
    public bool HasFaces;
    public ushort[][] Strips;
    public Triangle[] Triangles;
    public bool HasBoneIndices;
    public byte[][] BoneIndices;
    
    public void Serialize(BitWriter writer)
    {
        //eh
        throw new NotImplementedException();
    }

    public void Deserialize(BitReader reader)
    {
        NumVertices = reader.Read<ushort>();
        NumTriangles = reader.Read<ushort>();
        NumBones = reader.Read<ushort>();
        NumStrips = reader.Read<ushort>();
        NumWeightsPerVertex = reader.Read<ushort>();
        Bones = reader.ReadArray<ushort>(NumBones);
        HasVertexMap = reader.ReadBool();
        if (HasVertexMap)
        {
            VertexMap = reader.ReadArray<ushort>(NumVertices);
        }
        HasVertexWeights = reader.ReadBool();
        if (HasVertexWeights)
        {
            VertexWeights = reader.Read2DArray<float>(NumVertices, NumWeightsPerVertex);
        }
        StripLengths = reader.ReadArray<ushort>(NumStrips);
        HasFaces = reader.ReadBool();
        if (HasFaces)
        {
            if (NumStrips != 0) Strips = reader.Read2DArray<ushort>(NumStrips, StripLengths);
            else Triangles = reader.ReadArray<Triangle>(NumTriangles);
        }
        HasBoneIndices = reader.ReadBool();
        if (HasBoneIndices)
        {
            BoneIndices = reader.Read2DArray<byte>(NumVertices, NumWeightsPerVertex);
        }
    }
}