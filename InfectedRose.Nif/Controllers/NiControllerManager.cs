using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif.Controllers;

public class NiControllerManager : NiTimeController
{

    public bool Cumulative;
    public uint NumControllerSequences;
    public NiRef<NiControllerSequence>[] Sequences;
    public NiRef<NiDefaultAVObjectPalette> ObjectPalette;
    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        Cumulative = reader.ReadBool();
        NumControllerSequences = reader.Read<uint>();
        Sequences = reader.ReadArrayN<NiRef<NiControllerSequence>>((int)NumControllerSequences, File);
        ObjectPalette = reader.Read<NiRef<NiDefaultAVObjectPalette>>(File);
    }
}