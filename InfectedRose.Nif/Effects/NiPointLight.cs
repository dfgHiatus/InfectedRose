using RakDotNet.IO;

namespace InfectedRose.Nif
{
    public class NiPointLight : NiLight
    {
        public float ConstantAttenuation { get; set; }
        public float LinearAttenuation { get; set; }
        public float QuadraticAttenuation { get; set; }
        public override void Deserialize(BitReader reader)
        {
            base.Deserialize(reader);
            ConstantAttenuation = reader.Read<float>();
            LinearAttenuation = reader.Read<float>();
            QuadraticAttenuation = reader.Read<float>();
        }
    }
}