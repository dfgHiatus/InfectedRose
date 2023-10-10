using System;
using RakDotNet.IO;

namespace InfectedRose.Nif.Controllers;

public class NiMultiTargetTransformController : NiInterpController
{
    public ushort NumExtraTargets;
    public NiRef<NiAvObject>[] ExtraTargets;

    public override void Serialize(BitWriter writer)
    {
        throw new NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        NumExtraTargets = reader.Read<ushort>();
        ExtraTargets = reader.ReadArrayN<NiRef<NiAvObject>>(NumExtraTargets, File);
    }
}