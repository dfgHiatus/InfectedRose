using RakDotNet.IO;

namespace InfectedRose.Nif
{
    public class NiAlphaProperty : NiProperty
    {
        public ushort Flags { get; set; }
        public byte Threshold { get; set; }

        public override void Deserialize(BitReader reader)
        {
            base.Deserialize(reader);
            Flags = reader.Read<ushort>();
            Threshold = reader.Read<byte>();
        }

        public override void Serialize(BitWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Flags);
            writer.Write(Threshold);
        }
    }
}