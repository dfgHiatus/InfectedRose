using RakDotNet.IO;

namespace InfectedRose.Nif.Controllers;

public class NiPSysEmitterCtlr : NiPSysModifierCtlr
{
    public NiRef<NiInterpolator> VisibilityInterpolator;
    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        VisibilityInterpolator = reader.Read<NiRef<NiInterpolator>>(File);
    }
}