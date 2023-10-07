using RakDotNet.IO;
using InfectedRose.Core;

namespace InfectedRose.Nif;

public class AVObject : NiObject
{
    public SizedString Name;
    public NiRef<NiAvObject> Object;
    
    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        Name = reader.Read<SizedString>();
        Object = reader.Read<NiRef<NiAvObject>>(File);
    }
}