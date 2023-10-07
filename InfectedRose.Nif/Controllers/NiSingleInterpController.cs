using RakDotNet.IO;

namespace InfectedRose.Nif.Controllers;

public class NiSingleInterpController : NiInterpController
{
    public NiRef<NiInterpolator> Interpolator;
    public override void Serialize(BitWriter writer)
    {
        base.Serialize(writer);
    }

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        Interpolator = reader.Read<NiRef<NiInterpolator>>(File);
    }
}