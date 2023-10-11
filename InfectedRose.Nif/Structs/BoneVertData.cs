using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class BoneVertData : IConstruct
{
    public ushort Index;
    public float Weight;
    public void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public void Deserialize(BitReader reader)
    {
        Index = reader.Read<ushort>();
        Weight = reader.Read<float>();
    }
}