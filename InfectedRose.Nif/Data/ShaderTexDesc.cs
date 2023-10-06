using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class ShaderTexDesc : NiObject
{
    public bool HasMap;
    public TexDesc Map;
    public uint MapId;
    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        HasMap = reader.ReadBool();
        if (HasMap)
        {
            Map = reader.Read<TexDesc>();
            MapId = reader.Read<uint>();
        }
    }
}