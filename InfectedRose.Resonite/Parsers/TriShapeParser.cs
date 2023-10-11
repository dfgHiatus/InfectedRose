using Elements.Core;
using FrooxEngine;
using InfectedRose.Nif;
using ResoniteModLoader;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InfectedRose.Resonite.Parsers;

internal class TriShapeParser
{
    static void Msg(object o)
    {
        ResoniteMod.Msg(o);
    }

    internal static async Task ParseTriShape(Slot slot, NiFileContext context, NiTriShape shape)
    {
        await default(ToBackground);
        context.ObjectSlots.Add(shape, slot);

        await default(ToWorld);
        slot.LocalPosition = new float3(shape.Position.X, shape.Position.Y, shape.Position.Z);
        slot.LocalRotation = shape.Rotation.ToFrooxEngine();
        slot.LocalScale = new float3(shape.Scale, shape.Scale, shape.Scale);
        await default(ToBackground);

        var skinned = shape.Skin.Value is not null;
        var mesh = (shape.Data.Value as NiTriShapeData).ToFrooxEngine(shape.Skin.Value);

        var localDb = Engine.Current.LocalDB;
        var tempFilePath = localDb.GetTempFilePath(".meshx");
        mesh.SaveToFile(tempFilePath);
        var url = await localDb.ImportLocalAssetAsync(tempFilePath, LocalDB.ImportLocation.Move);

        await default(ToWorld);
        var staticMesh = context.AssetSlot.AttachComponent<StaticMesh>();
        staticMesh.URL.Value = url;

        MeshRenderer mr;
        if (skinned)
        {
            var smr = slot.AttachComponent<SkinnedMeshRenderer>();
            var bones = shape.Skin.Value.Bones;
            foreach (var bone in bones)
            {
                smr.Bones.Add(context.ObjectSlots.TryGetValue(bone.Value, out var boneSlot) ? boneSlot : null);
            }
            mr = smr;
        }
        else
        {
            mr = slot.AttachComponent<MeshRenderer>();
        }

        await default(ToBackground);
        var mat = await DetermineMaterial(slot, shape, context);
        await default(ToWorld);

        mr.Mesh.Target = staticMesh;
        mr.Materials.Add(mat);
        slot.AttachComponent<MeshCollider>();
        await default(ToBackground);
    }

    internal static async Task<IAssetProvider<Material>> DetermineMaterial(Slot slot, NiTriShape nts, NiFileContext context)
    {
        await default(ToBackground);
        NiVertexColorProperty niVertexColorProperty = null;
        NiAlphaProperty niAlphaProperty = null;
        NiSpecularProperty niSpecularProperty = null;
        NiMaterialProperty niMaterialProperty = null;
        NiTexturingProperty niTexturingProperty = null;

        IPBS_Metallic pbsMetallic = null;
        IPBS_Specular pbsSpecular = null;
        PBS_RimMetallic pbsRimMaterial = null;
        UnlitMaterial unlitMaterial = null;
        UnlitDistanceLerpMaterial unlitDistanceLerpMaterial = null;

        IAssetProvider<ITexture2D> BaseTexture = null;
        IAssetProvider<ITexture2D> GlowTexture = null;
        IAssetProvider<ITexture2D> NormalTexture = null;
        BlendMode? blendMode = null;
        float? alphaCutoff = null;
        colorX? albedoColor = null;
        colorX? emissiveColor = null;
        float? smoothness = null;
        float? alpha = null;

        foreach (var item in nts.Properties)
        {
            switch (item.Value)
            {
                case NiVertexColorProperty nVCP:
                    niVertexColorProperty = nVCP;
                    break;
                case NiAlphaProperty nAP:
                    niAlphaProperty = nAP;
                    break;
                case NiSpecularProperty nSP:
                    niSpecularProperty = nSP;
                    break;
                case NiMaterialProperty nMP:
                    niMaterialProperty = nMP;
                    break;
                case NiTexturingProperty nTP:
                    niTexturingProperty = nTP;
                    break;
            }
        }

        if (niTexturingProperty is not null)
        {
            if (niTexturingProperty.HasBaseTexture)
                BaseTexture = await ImportTexture(slot, context, niTexturingProperty.BaseTexture);
            if (niTexturingProperty.HasGlowTexture)
                GlowTexture = await ImportTexture(slot, context, niTexturingProperty.GlowTexture);
            if (niTexturingProperty.HasNormalTexture)
                NormalTexture = await ImportTexture(slot, context, niTexturingProperty.NormalTexture);
        }
        if (niAlphaProperty is not null)
        {
            if ((niAlphaProperty.Flags & 0x0001) > 0 && niAlphaProperty.Threshold > 0) //alpha blend
            {
                blendMode = BlendMode.Alpha;
            }
            else if (niAlphaProperty.Threshold > 0) blendMode = BlendMode.Transparent;
            alphaCutoff = niAlphaProperty.Threshold / 255f;
        }
        if (niMaterialProperty is not null)
        {
            albedoColor = niMaterialProperty.DiffuseColor.ToFrooxEngine().SetA(niMaterialProperty.Alpha);
            emissiveColor = niMaterialProperty.EmissiveColor.ToFrooxEngine() * niMaterialProperty.EmitMultiplier;
            smoothness = MathX.Clamp01(niMaterialProperty.Glossiness / 100f); //todo
        }

        // TODO Incorporate NiVertexColorProperty, NiAlphaProperty, NiSpecularProperty
        // Can we cut this down somehow?
        if (niMaterialProperty is not null)
        {
            if (LegoMaterialDefinitions.PBS_VertexColorMetallic.Any(name => name.Contains(niMaterialProperty.Name.Value)))
            {
                await default(ToWorld);
                pbsMetallic = slot.AttachComponent<PBS_VertexColorMetallic>();
                await default(ToBackground);
            }
            else if (LegoMaterialDefinitions.PBS_VertexColorMetallic_Emissive.Any(name =>
                         name.Contains(niMaterialProperty.Name.Value)))
            {
                await default(ToWorld);
                var m = slot.AttachComponent<PBS_VertexColorMetallic>();
                m.EmissiveColor.Value = colorX.White;
                pbsMetallic = m;
                await default(ToBackground);
            }
            else if (LegoMaterialDefinitions.PBS_VertexColorMetallic_Transparent.Any(name => name.Contains(niMaterialProperty.Name.Value)))
            {
                await default(ToWorld);
                var m = slot.AttachComponent<PBS_VertexColorMetallic>();
                m.AlphaHandling.Value = AlphaHandling.AlphaBlend;
                m.AlbedoColor.Value = new colorX(1f, 1f, 1f, 0.33f);
                pbsMetallic = m;
                await default(ToWorld);
            }
            else if (LegoMaterialDefinitions.PBS_VertexColorMetallic_Metallic.Any(name => name.Contains(niMaterialProperty.Name.Value)))
            {
                await default(ToWorld);
                var m = slot.AttachComponent<PBS_VertexColorMetallic>();
                m.Metallic.Value = 1f;
                pbsMetallic = m;
                await default(ToBackground);
            }
            else if (LegoMaterialDefinitions.PBS_VertexColorSpecular.Any(name =>
                         name.Contains(niMaterialProperty.Name.Value)))
            {
                await default(ToWorld);
                pbsSpecular = slot.AttachComponent<PBS_VertexColorSpecular>();
                await default(ToBackground);
            }
            else if (LegoMaterialDefinitions.PBS_VertexColorSpecular_Specular.Any(name =>
                         name.Contains(niMaterialProperty.Name.Value)))
            {
                await default(ToWorld);
                pbsSpecular = slot.AttachComponent<PBS_VertexColorSpecular>();
                await default(ToBackground);
            }
            else if (LegoMaterialDefinitions.PBS_RimMetallic.Any(name =>
                         name.Contains(niMaterialProperty.Name.Value)))
            {
                await default(ToWorld);
                pbsRimMaterial = slot.AttachComponent<PBS_RimMetallic>();
                await default(ToBackground);
            }
            else if (LegoMaterialDefinitions.GrayscaleMaterial.Any(name => name.Contains(niMaterialProperty.Name.Value)))
            {
                await default(ToWorld);
                var m = slot.AttachComponent<GrayscaleMaterial>();
                m.RatioBlue.Value = 1f;
                m.RatioGreen.Value = 1f;
                m.RatioRed.Value = 1f;
                await default(ToBackground);
                return m; // No further processing needed

            }
            else if (LegoMaterialDefinitions.UnlitDistanceLerp.Any(name =>
                         name.Contains(niMaterialProperty.Name.Value)))
            {
                await default(ToWorld);
                unlitDistanceLerpMaterial = slot.AttachComponent<UnlitDistanceLerpMaterial>();
                await default(ToBackground);
            }
            else if (LegoMaterialDefinitions.Unlit.Any(name =>
                         name.Contains(niMaterialProperty.Name.Value)))
            {
                await default(ToWorld);
                unlitMaterial = slot.AttachComponent<UnlitMaterial>();
                await default(ToBackground);
            }

            //0b1000000000 //alpha test mask
            //0b0000011110 //source blend mode
            //0b0111100000 //dest blend mode
            //0b0011101101 //problematicresult
            //problem source blend = 6, SRC_ALPHA
            //problem dest blend = 7, INV_SRC_ALPHA
            //todo: there are more alpha blend modes in the flags but the material only supports opaque, clip, and blend
            //todo: i dont know how the modes work, ???

            //specular's flags is an enum with two values, essentially a boolean, with the values
            //"SPECULAR_DISABLED" and "SPECULAR_ENABLED"

            await default(ToWorld);
            if (pbsMetallic is not null)
            {
                pbsMetallic.BlendMode = blendMode ?? BlendMode.Opaque;
                if (alphaCutoff.HasValue) pbsMetallic.AlphaCutoff = alphaCutoff.Value;
                pbsMetallic.AlbedoTexture = BaseTexture;
                pbsMetallic.EmissiveMap = GlowTexture;
                pbsMetallic.NormalMap = NormalTexture;
                if (albedoColor.HasValue) pbsMetallic.AlbedoColor = albedoColor.Value;
                if (emissiveColor.HasValue) pbsMetallic.EmissiveColor = emissiveColor.Value;
                if (smoothness.HasValue) pbsMetallic.Smoothness = smoothness.Value;
                return pbsMetallic;
            }
            if (pbsSpecular is not null)
            {
                pbsSpecular.BlendMode = blendMode ?? BlendMode.Opaque;
                if (alphaCutoff.HasValue) pbsSpecular.AlphaCutoff = alphaCutoff.Value;
                pbsSpecular.AlbedoTexture = BaseTexture;
                pbsSpecular.EmissiveMap = GlowTexture;
                pbsSpecular.NormalMap = NormalTexture;
                if (albedoColor.HasValue) pbsSpecular.AlbedoColor = albedoColor.Value;
                if (emissiveColor.HasValue) pbsSpecular.EmissiveColor = emissiveColor.Value;
                if (smoothness.HasValue) pbsSpecular.Smoothness = smoothness.Value;

                if (niSpecularProperty is not null)
                {
                    if (niSpecularProperty.Flags > 0)
                    {
                        // TODO Set Specular color
                        // pbsSpecular.SpecularColor = ...
                    }
                }

                return pbsSpecular;
            }
            if (pbsRimMaterial is not null)
            {
                pbsRimMaterial.AlbedoTexture.Target = BaseTexture;
                pbsRimMaterial.EmissiveMap.Target = GlowTexture;
                pbsRimMaterial.NormalMap.Target = NormalTexture;
                if (albedoColor.HasValue) pbsRimMaterial.AlbedoColor.Value = albedoColor.Value;
                if (emissiveColor.HasValue) pbsRimMaterial.EmissiveColor.Value = emissiveColor.Value;
                if (smoothness.HasValue) pbsRimMaterial.Smoothness.Value = smoothness.Value;
                if (blendMode.HasValue)
                {
                    pbsRimMaterial.Transparent.Value = blendMode.Value != BlendMode.Opaque;
                    pbsRimMaterial.AlbedoColor.Value = pbsRimMaterial.AlbedoColor.Value.SetA(alphaCutoff.Value); //???
                }

                return pbsRimMaterial;
            }

            if (unlitMaterial is not null)
            {
                unlitMaterial.BlendMode.Value = blendMode ?? BlendMode.Opaque;
                if (alphaCutoff.HasValue) unlitMaterial.AlphaCutoff.Value = alphaCutoff.Value;
                unlitMaterial.Texture.Target = BaseTexture;
                //unlitMaterial.EmissiveMap = GlowTexture;
                unlitMaterial.NormalMap = NormalTexture;
                if (albedoColor.HasValue) unlitMaterial.TintColor.Value = albedoColor.Value;
                //if (emissiveColor.HasValue) unlitMaterial.EmissiveColor = emissiveColor.Value;
                //if (smoothness.HasValue) unlitMaterial.Smoothness = smoothness.Value;

                unlitMaterial.BlendMode.Value = BlendMode.Opaque;
                return unlitMaterial;
            }

            if (unlitDistanceLerpMaterial is not null)
            {
                unlitDistanceLerpMaterial.BlendMode.Value = blendMode ?? BlendMode.Opaque;
                if (alphaCutoff.HasValue) unlitDistanceLerpMaterial.AlphaCutoff.Value = alphaCutoff.Value;
                unlitDistanceLerpMaterial.NearTexture.Target = BaseTexture;
                unlitDistanceLerpMaterial.FarTexture.Target = BaseTexture;
                //unlitDistanceLerpMaterial.NearEmissiveMap = GlowTexture;
                //unlitDistanceLerpMaterial.NormalMap = NormalTexture;
                if (albedoColor.HasValue)
                {
                    unlitDistanceLerpMaterial.NearColor.Value = albedoColor.Value;
                    unlitDistanceLerpMaterial.FarColor.Value = albedoColor.Value;
                }
                //if (emissiveColor.HasValue) unlitDistanceLerpMaterial.EmissiveColor = emissiveColor.Value;
                //if (smoothness.HasValue) unlitDistanceLerpMaterial.Smoothness = smoothness.Value;

                return unlitDistanceLerpMaterial;
            }
            await default(ToBackground);
        }

        await default(ToWorld);
        var pbsm = slot.AttachComponent<PBS_VertexColorMetallic>();
        await default(ToBackground);

        return pbsm;
    }

    internal static async Task<IAssetProvider<ITexture2D>> ImportTexture(Slot slot, NiFileContext context, TexDesc target)
    {
        await default(ToBackground);
        var path = target.Source.Value.Path.Value;
        if (string.IsNullOrWhiteSpace(path))
            return null;
        var overallPath = Path.Combine(Path.GetDirectoryName(context.Path), path);
        if (!File.Exists(overallPath))
            return null;
        var url = await Engine.Current.LocalDB.ImportLocalAssetAsync(overallPath, LocalDB.ImportLocation.Copy);

        await default(ToWorld);
        var provider = context.AssetSlot.AttachComponent<StaticTexture2D>();
        provider.URL.Value = url;
        var filterModeFlag = (target.TexturingMapFlags & 0b0000111100000000) >> 8;
        provider.FilterMode.Value = filterModeFlag switch
        {
            0 => TextureFilterMode.Point,
            1 => TextureFilterMode.Bilinear,
            2 => TextureFilterMode.Trilinear,
            3 => TextureFilterMode.Point,
            4 => TextureFilterMode.Point,
            5 => TextureFilterMode.Bilinear,
            6 => TextureFilterMode.Anisotropic,
            _ => TextureFilterMode.Bilinear,
        };
        var filterClampFlag = (target.TexturingMapFlags & 0b0011000000000000) >> 12;
        switch (filterClampFlag)
        {
            case 0:
                provider.WrapModeU.Value = TextureWrapMode.Clamp;
                provider.WrapModeV.Value = TextureWrapMode.Clamp;
                break;
            case 1:
                provider.WrapModeU.Value = TextureWrapMode.Clamp;
                provider.WrapModeV.Value = TextureWrapMode.Repeat;
                break;
            case 2:
                provider.WrapModeU.Value = TextureWrapMode.Repeat;
                provider.WrapModeV.Value = TextureWrapMode.Clamp;
                break;
            case 3:
                provider.WrapModeU.Value = TextureWrapMode.Repeat;
                provider.WrapModeV.Value = TextureWrapMode.Repeat;
                break;
        }

        await default(ToBackground);
        return provider;
    }
}
