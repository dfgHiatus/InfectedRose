using System.Runtime.CompilerServices;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiTexturingProperty : NiProperty
{
    public ushort Flags;
    public uint TextureCount;
    public bool HasBaseTexture;
    public TexDesc BaseTexture;
    public bool HasDarkTexture;
    public TexDesc DarkTexture;
    public bool HasDetailTexture;
    public TexDesc DetailTexture;
    public bool HasGlossTexture;
    public TexDesc GlossTexture;
    public bool HasGlowTexture;
    public TexDesc GlowTexture;
    public bool HasBumpMapTexture;
    public TexDesc BumpMapTexture;
    public float BumpMapLumaScale;
    public float BumpMapLumaOffset;
    public bool HasNormalTexture;
    public TexDesc NormalTexture;
    public bool HasParallaxTexture;
    public TexDesc ParallaxTexture;
    public float ParallaxOffset;
    public bool HasDecal0Texture;
    public TexDesc Decal0Texture;
    public bool HasDecal1Texture;
    public TexDesc Decal1Texture;
    public bool HasDecal2Texture;
    public TexDesc Decal2Texture;
    public bool HasDecal3Texture;
    public TexDesc Decal3Texture;
    public uint NumShaderTextures;
    public ShaderTexDesc[] ShaderTextures;

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        Flags = reader.Read<ushort>();
        TextureCount = reader.Read<uint>();
        
        ReadBasicTexture(out HasBaseTexture, ref BaseTexture, reader);
        ReadBasicTexture(out HasDarkTexture, ref DarkTexture, reader);
        ReadBasicTexture(out HasDetailTexture, ref DetailTexture, reader);
        ReadBasicTexture(out HasGlossTexture, ref GlossTexture, reader);
        ReadBasicTexture(out HasGlowTexture, ref GlowTexture, reader);

        if (TextureCount > 5)
        {
            HasBumpMapTexture = reader.ReadBool();
            if (HasBumpMapTexture)
            {
                BumpMapTexture = reader.Read<TexDesc>(File);
                BumpMapLumaScale = reader.Read<float>();
                BumpMapLumaOffset = reader.Read<float>();
            }

            if (TextureCount > 6)
            {
                ReadBasicTexture(out HasNormalTexture, ref NormalTexture, reader);
                if (TextureCount > 7)
                {
                    HasParallaxTexture = reader.ReadBool();
                    if (HasParallaxTexture)
                    {
                        ParallaxTexture = reader.Read<TexDesc>(File);
                        ParallaxOffset = reader.Read<float>();
                    }

                    if (TextureCount > 8)
                    {
                        ReadBasicTexture(out HasDecal0Texture, ref Decal0Texture, reader);
                        if (TextureCount > 9)
                        {
                            ReadBasicTexture(out HasDecal0Texture, ref Decal0Texture, reader);
                            if (TextureCount > 10)
                            {
                                ReadBasicTexture(out HasDecal0Texture, ref Decal0Texture, reader);
                                if (TextureCount > 11)
                                {
                                    ReadBasicTexture(out HasDecal0Texture, ref Decal0Texture, reader);
                                }
                            }
                        }
                    }
                }
            }
        }
        NumShaderTextures = reader.Read<uint>();
        ShaderTextures = reader.ReadArrayN<ShaderTexDesc>((int) NumShaderTextures, File);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadBasicTexture(out bool b, ref TexDesc desc, BitReader reader)
    {
        b = reader.ReadBool();
        if (b) desc = reader.Read<TexDesc>(File);
    }
}