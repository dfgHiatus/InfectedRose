using RakDotNet.IO;

namespace InfectedRose.Nif;

public class SkinInfoSet : NiObject
{
    public uint NumSkinInfo;
    public NiRef<SkinInfo>[] SkinInfo;
    
    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }
    public override void Deserialize(BitReader reader)
    {
        NumSkinInfo = reader.Read<uint>();
        SkinInfo = reader.ReadArrayN<NiRef<SkinInfo>>((int) NumSkinInfo, File);
    }
}