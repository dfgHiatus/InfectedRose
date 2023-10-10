using RakDotNet.IO;

namespace InfectedRose.Nif.Controllers;

public class NiMaterialColorController : NiPoint3InterpController
{
    public ushort TargetColor;
    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        TargetColor = reader.Read<ushort>();
    }
}