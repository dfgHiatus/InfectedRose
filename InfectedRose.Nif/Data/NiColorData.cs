using RakDotNet.IO;
using InfectedRose.Core;

namespace InfectedRose.Nif;

public class NiColorData : NiObject
{
    public NiKeyGroup<Color4> Data;
    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        Data = reader.Read<NiKeyGroup<Color4>>();
    }
}