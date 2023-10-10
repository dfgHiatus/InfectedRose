using RakDotNet.IO;

namespace InfectedRose.Nif.Controllers;

public class NiLightColorController : NiPoint3InterpController
{
    public ushort TargetColor; //0 = diffuse, 1 = ambient
    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        TargetColor = reader.Read<ushort>();
    }
}