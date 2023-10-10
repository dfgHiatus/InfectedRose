using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiPSysGrowFadeModifier : NiPSysModifier
{
    public float GrowTime;
    public ushort GrowGeneration;
    public float FadeTime;
    public ushort FadeGeneration;

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        GrowTime = reader.Read<float>();
        GrowGeneration = reader.Read<ushort>();
        FadeTime = reader.Read<float>();
        FadeGeneration = reader.Read<ushort>();
    }
    
}