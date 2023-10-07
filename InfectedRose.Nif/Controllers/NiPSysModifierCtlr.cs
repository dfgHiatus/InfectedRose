using InfectedRose.Nif.Controllers;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiPSysModifierCtlr : NiSingleInterpController
{
    public NiStringRef ModifierName;
    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        ModifierName = reader.Read<NiStringRef>(File);
    }
}