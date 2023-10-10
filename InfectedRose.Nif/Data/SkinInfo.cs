using RakDotNet.IO;

namespace InfectedRose.Nif;

public class SkinInfo : NiObject
{
    public NiRef<NiTriBasedGeom> Shape;
    public NiRef<NiSkinInstance> SkinInstance;
    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        Shape = reader.Read<NiRef<NiTriBasedGeom>>(File);
        SkinInstance = reader.Read<NiRef<NiSkinInstance>>(File);
    }
}