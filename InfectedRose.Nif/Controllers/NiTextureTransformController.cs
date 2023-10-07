using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif.Controllers;

public class NiTextureTransformController : NiFloatInterpController
{
    public bool ShaderMap;
    public uint TextureSlot;
    public uint Operation;
    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        ShaderMap = reader.ReadBool();
        TextureSlot = reader.Read<uint>();
        Operation = reader.Read<uint>();
    }
}