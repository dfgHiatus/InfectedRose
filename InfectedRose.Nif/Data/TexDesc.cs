using System.Numerics;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class TexDesc : NiObject
{
    public NiRef<NiSourceTexture> Source;
    public ushort TexturingMapFlags;
    public bool HasTextureTransform;
    public Vector2 Translation;
    public Vector2 Scale;
    public float Rotation;
    public uint TransformMethod;
    public Vector2 Center;
    
    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        Source = reader.Read<NiRef<NiSourceTexture>>(File);
        TexturingMapFlags = reader.Read<ushort>();
        HasTextureTransform = reader.ReadBool();
        if (HasTextureTransform)
        {
            Translation = reader.Read<Vector2>();
            Scale = reader.Read<Vector2>();
            Rotation = reader.Read<float>();
            TransformMethod = reader.Read<uint>();
            Center = reader.Read<Vector2>();
        }
    }
}