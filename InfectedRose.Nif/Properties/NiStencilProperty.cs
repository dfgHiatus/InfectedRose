using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiStencilProperty : NiProperty
{
    public ushort Flags; //StencilFlags
    public uint StencilRef;
    public uint StencilMask;

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        Flags = reader.Read<ushort>();
        StencilRef = reader.Read<uint>();
        StencilMask = reader.Read<uint>();
    }
}