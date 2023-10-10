using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiPSysEmitter : NiPSysModifier
{
    public float Speed;
    public float SpeedVariation;
    public float Declination;
    public float DeclinationVariatioon;
    public float PlanarAngle;
    public float PlanarAngleVariation;
    public Color4 InitialColor;
    public float InitialRadius;
    public float RadiusVariation;
    public float LifeSpan;
    public float LifeSpanVariation;

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        Speed = reader.Read<float>();
        SpeedVariation = reader.Read<float>();
        Declination = reader.Read<float>();
        DeclinationVariatioon = reader.Read<float>();
        PlanarAngle = reader.Read<float>();
        PlanarAngleVariation = reader.Read<float>();
        InitialColor = reader.Read<Color4>();
        InitialRadius = reader.Read<float>();
        RadiusVariation = reader.Read<float>();
        LifeSpan = reader.Read<float>();
        LifeSpanVariation = reader.Read<float>();
    }
}