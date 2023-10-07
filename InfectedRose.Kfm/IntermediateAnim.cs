using InfectedRose.Core;
using InfectedRose.Nif;
using RakDotNet.IO;

namespace InfectedRose.Kfm;

public class IntermediateAnim : IConstruct
{
    public int UnknownInt;
    public SizedString Event;
    public void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public void Deserialize(BitReader reader)
    {
        UnknownInt = reader.Read<int>();
        Event = reader.Read<SizedString>();
    }
}