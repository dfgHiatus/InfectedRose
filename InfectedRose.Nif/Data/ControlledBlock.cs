using RakDotNet.IO;

namespace InfectedRose.Nif;

public class ControlledBlock : NiObject
{
    public NiStringRef NodeName;
    public NiStringRef PropertyType;
    public NiStringRef ControllerType;
    public NiStringRef ControllerID;
    public NiStringRef InterpolatorID;

    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }
    public override void Deserialize(BitReader reader)
    {
        NodeName = reader.Read<NiStringRef>(File);
        PropertyType = reader.Read<NiStringRef>(File);
        ControllerType = reader.Read<NiStringRef>(File);
        ControllerID = reader.Read<NiStringRef>(File);
        InterpolatorID = reader.Read<NiStringRef>(File);
    }
}