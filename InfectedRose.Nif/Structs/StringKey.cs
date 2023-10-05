using RakDotNet.IO;
using InfectedRose.Core;

namespace InfectedRose.Nif;

public class StringKey : NiObject
{
    public float Time { get; set; }
    public string Value { get; set; }
    public string Forward { get; set; }
    public string Backward { get; set; }
    public TBC TBC { get; set; }
    public override void Deserialize(BitReader reader)
    {
        Time = reader.Read<float>();
        Value = reader.ReadNiString();
        Forward = reader.ReadNiString();
        Backward = reader.ReadNiString();
        TBC = reader.Read<TBC>();
    }

    public override void Serialize(BitWriter writer)
    {

        writer.Write(Time);
        writer.WriteNiString(Value);
        writer.WriteNiString(Forward);
        writer.WriteNiString(Backward);
        writer.Write(TBC);
    }
}