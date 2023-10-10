using RakDotNet.IO;
using InfectedRose.Core;

namespace InfectedRose.Nif;

public class NiBoolData : NiObject
{
    public NiKeyGroup<byte> Data;
    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        Data = reader.Read<NiKeyGroup<byte>>();
    }
}