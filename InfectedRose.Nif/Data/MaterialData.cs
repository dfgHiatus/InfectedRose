using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class MaterialData : IConstruct
{
    public uint NumMaterials;
    public uint[] MaterialName; //this refers to strings within the header
    public int[] MaterialExtraData;
    public int ActiveMaterial;
    public bool MaterialNeedsUpdate;
    
    public void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public void Deserialize(BitReader reader)
    {
        NumMaterials = reader.Read<uint>();
        MaterialName = reader.ReadArray<uint>((int) NumMaterials);
        MaterialExtraData = reader.ReadArray<int>((int) NumMaterials);
        ActiveMaterial = reader.Read<int>();
        MaterialNeedsUpdate = reader.ReadBool();
    }
}