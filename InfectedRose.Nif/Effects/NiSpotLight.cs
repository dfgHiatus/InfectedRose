using RakDotNet.IO;

namespace InfectedRose.Nif
{
    public class NiSpotLight : NiPointLight
    {
        public float OuterSpotAngle { get; set; }
        public float InnerSpotAngle { get; set; }
        public float Exponent { get; set; }
        public override void Deserialize(BitReader reader)
        {
            base.Deserialize(reader);
            OuterSpotAngle = reader.Read<float>();
            InnerSpotAngle = reader.Read<float>();
            Exponent = reader.Read<float>();
        }
    }
}