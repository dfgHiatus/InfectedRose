using RakDotNet.IO;

namespace InfectedRose.Nif.Controllers
{
    public class NiTimeController : NiObject
    {
        public NiRef<NiTimeController> NextController;
        public ushort Flags;
        public float Frequency;
        public float Phase;
        public float StartTime;
        public float StopTime;
        public NiRef<NiObjectNet> Target;
        public override void Serialize(BitWriter writer)
        {
        }

        public override void Deserialize(BitReader reader)
        {
            NextController = reader.Read<NiRef<NiTimeController>>(File);
            Flags = reader.Read<ushort>();
            Frequency = reader.Read<float>();
            Phase = reader.Read<float>();
            StartTime = reader.Read<float>();
            StopTime = reader.Read<float>();
            Target = reader.Read<NiRef<NiObjectNet>>(File);
        }
    }
}
