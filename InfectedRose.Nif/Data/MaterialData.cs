using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class MaterialData : NiObject
{
    public uint NumMaterials;
    public NiStringRef[] MaterialName;
    public int[] MaterialExtraData;
    public int ActiveMaterial;
    public bool MaterialNeedsUpdate;
    
    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        NumMaterials = reader.Read<uint>();
        MaterialName = reader.ReadArrayN<NiStringRef>((int) NumMaterials, File);
        MaterialExtraData = reader.ReadArray<int>((int) NumMaterials);
        ActiveMaterial = reader.Read<int>();
        MaterialNeedsUpdate = reader.ReadBool();
    }
}