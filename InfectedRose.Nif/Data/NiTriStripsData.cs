using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiTriStripsData : NiTriBasedGeomData
{
    public ushort StripsCount { get; set; }

    public ushort StripsLengths { get; set; }

    public bool HasPoints { get; set; }

    public ushort[][] Points { get; set; }

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        StripsCount = reader.Read<ushort>();
        StripsLengths = reader.Read<ushort>();
        HasPoints = reader.Read<byte>() != 0;
        if (!HasPoints) return;
        Points = reader.Read2DArray<ushort>(StripsCount, StripsLengths);
    }
}