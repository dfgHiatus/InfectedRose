using System;
using System.Linq;
using System.Numerics;
using Elements.Core;
using InfectedRose.Nif;
using InfectedRose.Terrain;
using InfectedRose.Terrain.Pipeline;

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
        return q.ToFrooxEngine();
    }
    
    internal static floatQ ToFrooxEngine(this Quaternion q)
    {
        return new floatQ(-q.X, -q.Y, q.Z, q.W); // TODO: axis conversion
    }

    internal static float3 ToFrooxEngine(this Vector3 vec)
    {
        return new float3(vec.X, vec.Y, -vec.Z);
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
                    vert.Normal = -data.Normals[i].ToFrooxEngine();
                    //if ((data.DataFlags & 61440) != 0) vert.Tangent = data.Tangents[i].ToFrooxEngine();
                    // No bitangents
                }
                if (data.HasVertexColors) vert.Color = data.VertexColors[i].ToFrooxEngine().BaseColor;
                for (var u = 0; u < data.Uv.Length; u++) vert.SetUV(u, data.Uv[u][i]);

                // Add vertex skinning data, if skinned
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

            // Add bone rest positions
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
        mesh.ReverseWinding();
        return mesh;
    }

    internal static MeshX ToFrooxEngine(this TerrainFile file)
    {
        var triangles = file.Triangulate();
        var mesh = new MeshX();
        //var offset = new float3(file.Chunks[0].HeightMap.PositionX, 0, file.Chunks[0].HeightMap.PositionY);
        var mul = new float3(-1, 1, -1);
        foreach (var tri in triangles)
        {
            var v1 = mesh.AddVertex(tri.Position1.ToFrooxEngine().zyx * mul);
            var v2 = mesh.AddVertex(tri.Position2.ToFrooxEngine().zyx * mul);
            var v3 = mesh.AddVertex(tri.Position3.ToFrooxEngine().zyx * mul);
            mesh.AddTriangle(v1, v2, v3);
        }

        var xMin = mesh.Vertices.Min(i => i.Position.X);
        var xMax = mesh.Vertices.Max(i => i.Position.X);
        var yMin = mesh.Vertices.Min(i => i.Position.Z);
        var yMax = mesh.Vertices.Max(i => i.Position.Z);

        foreach (var vert in mesh.Vertices)
            vert.SetUV(
                0,
                new float2(
                    MathX.Remap(vert.Position.X, xMin, xMax, 0, 1),
                    MathX.Remap(vert.Position.Z, yMin, yMax, 0, 1)));

        mesh = mesh.GetMergedDoubles();
        mesh.RecalculateNormals();
        mesh.FlipNormals();
        return mesh;
    }
}
