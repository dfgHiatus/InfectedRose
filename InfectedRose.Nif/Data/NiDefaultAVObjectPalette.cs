using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiDefaultAVObjectPalette : NiAVObjectPalette
{
    public NiRef<NiAvObject> Scene;
    public uint NumObjs;
    public AVObject[] Objs;
    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        Scene = reader.Read<NiRef<NiAvObject>>(File);
        NumObjs = reader.Read<uint>();
        Objs = reader.ReadArrayN<AVObject>((int)NumObjs, File);
    }
}