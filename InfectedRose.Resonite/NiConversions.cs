using System;
using System.Linq;
using System.Numerics;
using Elements.Core;
using InfectedRose.Nif;

namespace InfectedRose.Resonite;

internal static class NiConversions
{
    internal static colorX ToFrooxEngine(this Color3 color)
    {
        return new colorX(color.R, color.G, color.B);
    }

    internal static colorX ToFrooxEngine(this Color4 color)
    {
        return new colorX(color.R, color.G, color.B, color.A);
    }

    internal static floatQ ToFrooxEngine(this Matrix3X3 mat)
    {
        var q = Quaternion.CreateFromRotationMatrix(mat.FourByFour);
        return new floatQ(q.X, q.Y, q.Z, q.W); //TODO: axis conversion
    }

    internal static float3 ToFrooxEngine(this Vector3 vec)
    {
        return new float3(vec.X, vec.Y, -vec.Z); //TODO: axis conversion
    }

    internal static int[] Data(this Nif.Triangle tri)
    {
        return new[] { (int)tri.V1, tri.V2, tri.V3 };
    }

    internal static MeshX ToFrooxEngine(this NiTriShapeData data, NiSkinInstance skin = null)
    {
        var mesh = new MeshX();
        var partition = skin?.Partition.Value;

        var skinned = skin is not null;
        
        if (data.HasVertices)
        {
            for (var i = 0; i < data.NumVertices; i++)
            {
                var vert = mesh.AddVertex();
                vert.Position = data.Vertices[i].ToFrooxEngine();
                if (data.HasNormals)
                {
                    vert.Normal = data.Normals[i].ToFrooxEngine();
                    if ((data.DataFlags & 61440) != 0) vert.Tangent = data.Tangents[i];
                    //no bitangent
                }
                if (data.HasVertexColors) vert.Color = data.VertexColors[i].ToFrooxEngine().BaseColor;
                for (var u = 0; u < data.Uv.Length; u++) vert.SetUV(u, data.Uv[u][i]);

                //add vert skinning data, if skinned
                if (skinned)
                {
                    if (partition.NumPartitions > 0)
                    {
                        var currentPartition = partition.Partitions[0]; //TOOD:???
                        if (currentPartition.HasVertexMap && currentPartition.HasBoneIndices)
                        {
                            var currentVert = currentPartition.VertexMap[i];
                            var bindingCount = currentPartition.NumWeightsPerVertex;
                            var boneIndices = currentPartition.BoneIndices[i];
                            var weightsCapped = Math.Min(4, (int)bindingCount);
                            for (var w = 0; w < weightsCapped; w++)
                            {
                                vert.BoneBinding.AddBone(currentPartition.Bones[boneIndices[0]],
                                    currentPartition.VertexWeights[currentVert][w]);
                                //?????
                            }
                        }
                    }
                }
            }

            //add bone rest positions
            if (skinned)
            {
                var boneData =
                    skin.Data.Value;
                for (var i = 0; i < boneData.NumBones; i++)
                {
                    var name = skin.Bones[i].Value.Name.Value;
                    var bone = mesh.AddBone(name);
                    var boneData2 = boneData.BoneList[i];
                    var skinTransform = boneData2.SkinTransform;
                    var pos = skinTransform.Translation.ToFrooxEngine();
                    var rotation = skinTransform.Rotation.ToFrooxEngine();
                    var scale = new float3(skinTransform.Scale,skinTransform.Scale,skinTransform.Scale);
                    bone.BindPose = float4x4.Transform(pos, rotation, scale);
                }
            }

            for (var i = 0; i < data.TriangleCount; i++)
            {
                var tri = data.Triangles[i];
                mesh.AddTriangle(tri.V1, tri.V2, tri.V3);
            }
            
        }
        return mesh;
    }
}
