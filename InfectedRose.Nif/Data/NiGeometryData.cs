using System.Numerics;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif
{
    public class NiGeometryData : NiObject
    {
        public int GroupId { get; set; }

        public ushort NumVertices;

        public byte KeepFlags { get; set; }

        public byte CompressFlags { get; set; }

        public bool HasVertices;

        public Vector3[] Vertices { get; set; }
        public ushort DataFlags { get; set; }
        public bool HasNormals;

        public Vector3[] Normals { get; set; }

        public Vector3 Center { get; set; }

        public float Radius { get; set; }

        public bool HasVertexColors;

        public Color4[] VertexColors { get; set; }

        public ushort ConsistencyFlags { get; set; }

        public NiRef<AbstractAdditionalGeometryData> AdditionData { get; set; }
        public Vector2[][] Uv { get; set; }

        public Vector3[] Tangents { get; set; }

        public Vector3[] BitTangents { get; set; }

        public override void Serialize(BitWriter writer)
        {
            var verticesCount = (ushort) Vertices.Length;
            
            writer.Write(GroupId);

            writer.Write(verticesCount);

            writer.Write(KeepFlags);

            writer.Write(CompressFlags);

            writer.Write((byte) (verticesCount > 0 ? 1 : 0));

            foreach (var vertex in Vertices)
            {
                writer.Write(vertex);
            }

            writer.Write(DataFlags);

            writer.Write((byte) (Normals.Length > 0 ? 1 : 0));

            foreach (var normal in Normals)
            {
                writer.Write(normal);
            }

            if (Normals.Length > 0 & (DataFlags & 61440) != 0)
            {
                foreach (var tangent in Tangents)
                {
                    writer.Write(tangent);
                }

                foreach (var bitTangent in BitTangents)
                {
                    writer.Write(bitTangent);
                }
            }

            writer.Write(Center);

            writer.Write(Radius);

            writer.Write((byte) (VertexColors.Length > 0 ? 1 : 0));

            foreach (var vertexColor in VertexColors)
            {
                writer.Write(vertexColor);
            }

            foreach (var layer in Uv)
            {
                foreach (var vector2 in layer)
                {
                    writer.Write(vector2);
                }
            }

            writer.Write(ConsistencyFlags);

            writer.Write(AdditionData);
        }

        public override void Deserialize(BitReader reader)
        {
            GroupId = reader.Read<int>();
            NumVertices = reader.Read<ushort>();
            KeepFlags = reader.Read<byte>();
            CompressFlags = reader.Read<byte>();
            HasVertices = reader.ReadBool();
            if (HasVertices)
            {
                Vertices = reader.ReadArray<Vector3>(NumVertices);
            }
            DataFlags = reader.Read<ushort>();
            HasNormals = reader.ReadBool();
            if (HasNormals)
            {
                Normals = reader.ReadArray<Vector3>(NumVertices);
            }
            if (HasNormals && (DataFlags & 0b1111000000000000) != 0)
            {
                Tangents = reader.ReadArray<Vector3>(NumVertices);
                BitTangents = reader.ReadArray<Vector3>(NumVertices);
            }
            Center = reader.Read<Vector3>();
            Radius = reader.Read<float>();
            HasVertexColors = reader.ReadBool();
            if (HasVertexColors)
            {
                VertexColors = reader.ReadArray<Color4>(NumVertices);
            }

            Uv = reader.Read2DArray<Vector2>(DataFlags & 63, NumVertices);

            ConsistencyFlags = reader.Read<ushort>();

            AdditionData = reader.Read<NiRef<AbstractAdditionalGeometryData>>(File);
        }
    }
}