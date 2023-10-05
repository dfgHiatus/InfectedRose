using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiMatchGroup : NiObject
{
    public ushort VerticesCount { get; set; }

    public ushort[] VertexIndices { get; set; }
    public override void Serialize(BitWriter writer)
    {
        writer.Write(VerticesCount);
        for (var i = 0; i < VerticesCount; i++)
        {
            writer.Write(VertexIndices[i]);
        }
    }

    public override void Deserialize(BitReader reader)
    {
        VerticesCount = reader.Read<ushort>();
        VertexIndices = new ushort[VerticesCount];
        for (var i = 0; i < VerticesCount; i++)
        {
            VertexIndices[i] = reader.Read<ushort>();
        }
    }
}