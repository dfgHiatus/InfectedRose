using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using InfectedRose.Nif;
using RakDotNet.IO;
using ResoniteModLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Remoting.Contexts;
using System.Xml.Xsl;
using static FrooxEngine.RadiantUI_Constants;
using AlphaHandling = FrooxEngine.AlphaHandling;

namespace InfectedRose.Resonite;

public class LegoUniverseImporter : ResoniteMod
{
    public override string Name => "Lego Universe Importer";
    public override string Author => "dfgHiatus and Fro Zen";
    public override string Version => "1.0.0";

    internal const string NIF_EXTENSION = ".nif";
    internal const string DYN_VAR_SPACE_PREFIX = "LegoUniverse/";
    internal static ModConfiguration config;

    [AutoRegisterConfigKey]
    internal static ModConfigurationKey<bool> enabled =
        new("enabled", "Enabled", () => true);

    public override void OnEngineInit()
    {
        new Harmony("net.dfgHiatus.LegoUniverseImporter").PatchAll();
        config = GetConfiguration();
    }

    [HarmonyPatch(typeof(UniversalImporter), "Import", typeof(AssetClass), typeof(IEnumerable<string>),
            typeof(World), typeof(float3), typeof(floatQ), typeof(bool))]
    public class UniversalImporterPatch
    {
        static bool Prefix(ref IEnumerable<string> files)
        {
            if (!config.GetValue(enabled)) return true; // Skip if disabled

            List<string> hasLego = new();
            List<string> notLego = new();
            foreach (var file in files)
            {
                if (Path.GetExtension(file).ToLower() == NIF_EXTENSION)
                    hasLego.Add(file);
                else
                    notLego.Add(file);
            }

            var slot = Engine.Current.WorldManager.FocusedWorld.AddSlot("Lego Universe Import");
            slot.PositionInFrontOfUser();

            var dSpace = slot.AttachComponent<DynamicVariableSpace>();
            dSpace.OnlyDirectBinding.Value = true;
            dSpace.SpaceName.Value = DYN_VAR_SPACE_PREFIX.TrimEnd('/');

            foreach (var path in hasLego)
            {
                // TODO Make async
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                using var reader = new BitReader(stream);
                var file = new NiFile();
                file.Deserialize(reader);
                file.ReadBlocks(reader); 

                var header = slot.AddSlot("Header");
                ParseNiFile(header, file, path);
            }
            
            if (notLego.Count <= 0) return false;
            files = notLego.ToArray();
            return true;
        }
    }

    internal static void ParseNiFile(Slot header, NiFile file, string path)
    {
        ParseNiHeader(header, file);

        var sBlock = header.AddSlot("Block");
        if (file.Blocks[0] is NiNode root) // The root Block will contain exactly 1 element
            ParseNiNode(sBlock, new NiFileContext(), root, null);
    }

    internal static void ParseNiHeader(Slot header, NiFile file)
    {
        // Could we use reflection to make this less repetitive?
        AttachDynamicValueVariableWithValue(header, "Version", (uint)file.Header.Version);
        AttachDynamicValueVariableWithValue(header, "Endianness", (byte)file.Header.Endian);
        AttachDynamicValueVariableWithValue(header, "Version String", file.Header.VersionString);
        AttachDynamicValueVariableWithValue(header, "User Version", file.Header.UserVersion);

        // Need to define this as NodeInfo has two members, not one
        var sNodeInfos = header.AddSlot("Node Infos");
        string s;
        foreach (var item in file.Header.NodeInfo)
        {
            var sNodeInfo = sNodeInfos.AddSlot("Node Info");

            s = "Type Index";
            var scNodeInfo = sNodeInfo.AddSlot(s);
            var dynVar = scNodeInfo.AttachComponent<DynamicValueVariable<ushort>>();
            dynVar.Value.Value = item.TypeIndex;
            dynVar.VariableName.Value = DYN_VAR_SPACE_PREFIX + s;

            s = "Size";
            var scSize = sNodeInfo.AddSlot(s);
            var dynVar2 = scSize.AttachComponent<DynamicValueVariable<uint>>();
            dynVar2.Value.Value = item.TypeIndex;
            dynVar2.VariableName.Value = DYN_VAR_SPACE_PREFIX + s;
        }

        AttachDynamicValueVariableCollectionWithValues(header, "Node Types", "Type", file.Header.NodeTypes);
        AttachDynamicValueVariableCollectionWithValues(header, "Strings", "String", file.Header.Strings);
        AttachDynamicValueVariableWithValue(header, "Max String Length", file.Header.MaxStringLength);
        AttachDynamicValueVariableCollectionWithValues(header, "Groups", "Group", file.Header.Groups);
    }

    internal static void ParseNiNode(Slot slot, NiFileContext context, NiNode obj, NiNode parent)
    {
        context.ObjectSlots.Add(obj, slot);
        
        slot.LocalPosition = new float3(obj.Position.X, obj.Position.Y, obj.Position.Z);
        slot.LocalRotation = obj.Rotation.ToFrooxEngine();
        slot.LocalScale = new float3(obj.Scale, obj.Scale, obj.Scale);

        for (var i = 0; i < obj.Children.Length; i++)
        {
            switch (obj.Children[i].Value)
            {
                case NiNode node:
                    var sNode = slot.AddSlot("Node");
                    ParseNiNode(sNode, context, node, obj);
                    break;
                case NiTriShape triShape:
                    var sMesh = slot.AddSlot("Mesh");
                    ParseTriShape(sMesh, context, triShape);
                    break;
            }
        }

        for (var i = 0; i < obj.Effects.Length; i++)
        {
            switch (obj.Effects[i].Value)
            {
                case NiDirectionalLight dLight:
                    var dLightSlot = "Directional Light";
                    AttachLightWithValues(slot.AddSlot(dLightSlot), dLightSlot, LightType.Directional, dLight.Diffuse);
                    break;
                case NiSpotLight sLight:
                    var sLightSlot = "Spot Light";
                    var light = AttachLightWithValues(slot.AddSlot(sLightSlot), sLightSlot, LightType.Spot, sLight.Diffuse);
                    light.SpotAngle.Value = sLight.OuterSpotAngle;
                    break;
                case NiPointLight pLight:
                    var pLightSlot = "Point Light";
                    AttachLightWithValues(slot.AddSlot(pLightSlot), pLightSlot, LightType.Point, pLight.Diffuse);
                    break;
            }
        }
    }

    internal static void ParseTriShape(Slot slot, NiFileContext context, NiTriShape shape)
    {
        context.ObjectSlots.Add(shape, slot);
        
        slot.LocalPosition = new float3(shape.Position.X, shape.Position.Y, shape.Position.Z);
        slot.LocalRotation = shape.Rotation.ToFrooxEngine();
        slot.LocalScale = new float3(shape.Scale,shape.Scale,shape.Scale);

        var skinned = shape.Skin.Value != null;
        var mesh = (shape.Data.Value as NiTriShapeData).ToFrooxEngine(shape.Skin.Value);

        var localDb = Engine.Current.LocalDB;
        var tempFilePath = localDb.GetTempFilePath(".meshx");
        mesh.SaveToFile(tempFilePath);
        var url = localDb.ImportLocalAssetAsync(tempFilePath, LocalDB.ImportLocation.Move).Result;
        var staticMesh = slot.AttachComponent<StaticMesh>();
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

        mr.Mesh.Target = staticMesh;
        mr.Materials.Add(DetermineMaterial(slot, shape, context));
        slot.AttachComponent<MeshCollider>();
    }

    internal static IAssetProvider<Material> DetermineMaterial(Slot slot, NiTriShape nts, NiFileContext context)
    {
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

        // TODO Incorporate NiVertexColorProperty, NiAlphaProperty, NiSpecularProperty
        // Can we cut this down somehow?
        if (niMaterialProperty != null)
        {
            foreach (var name in LegoMaterialDefinitions.PBS_VertexColorMetallic)
            {
                if (!name.Contains(niMaterialProperty.Name.Value))
                    continue;
                pbsMetallic = slot.AttachComponent<PBS_VertexColorMetallic>();
                goto FoundMaterial;
            }

            foreach (var name in LegoMaterialDefinitions.PBS_VertexColorMetallic_Emissive)
            {
                if (!name.Contains(niMaterialProperty.Name.Value))
                    continue;
                var m = slot.AttachComponent<PBS_VertexColorMetallic>();
                m.EmissiveColor.Value = colorX.White;
                pbsMetallic = m;
                goto FoundMaterial;
            }

            foreach (var name in LegoMaterialDefinitions.PBS_VertexColorMetallic_Transparent)
            {
                if (!name.Contains(niMaterialProperty.Name.Value))
                    continue;
                var m = slot.AttachComponent<PBS_VertexColorMetallic>();
                m.AlphaHandling.Value = AlphaHandling.AlphaBlend;
                m.AlbedoColor.Value = new colorX(1f, 1f, 1f, 0.33f);
                pbsMetallic = m;
                goto FoundMaterial;
            }

            foreach (var name in LegoMaterialDefinitions.PBS_VertexColorMetallic_Metallic)
            {
                if (!name.Contains(niMaterialProperty.Name.Value))
                    continue;
                var m = slot.AttachComponent<PBS_VertexColorMetallic>();
                m.Metallic.Value = 1f;
                pbsMetallic = m;
                goto FoundMaterial;
            }

            foreach (var name in LegoMaterialDefinitions.PBS_VertexColorSpecular)
            {
                if (!name.Contains(niMaterialProperty.Name.Value))
                    continue;
                pbsSpecular = slot.AttachComponent<PBS_VertexColorSpecular>();
                goto FoundMaterial;
            }

            foreach (var name in LegoMaterialDefinitions.PBS_VertexColorSpecular_Specular)
            {
                if (!name.Contains(niMaterialProperty.Name.Value))
                    continue;
                pbsSpecular = slot.AttachComponent<PBS_VertexColorSpecular>();
                goto FoundMaterial;
            }

            foreach (var name in LegoMaterialDefinitions.PBS_RimMetallic)
            {
                if (!name.Contains(niMaterialProperty.Name.Value))
                    continue;
                pbsRimMaterial = slot.AttachComponent<PBS_RimMetallic>();
                goto FoundMaterial;
            }

            foreach (var name in LegoMaterialDefinitions.GrayscaleMaterial)
            {
                if (!name.Contains(niMaterialProperty.Name.Value))
                    continue;
                var m = slot.AttachComponent<GrayscaleMaterial>();
                m.RatioBlue.Value = 1f;
                m.RatioGreen.Value = 1f;
                m.RatioRed.Value = 1f;
                return m; // No further processing needed
            }

            foreach (var name in LegoMaterialDefinitions.UnlitDistanceLerp)
            {
                if (!name.Contains(niMaterialProperty.Name.Value))
                    continue;
                unlitDistanceLerpMaterial = slot.AttachComponent<UnlitDistanceLerpMaterial>();
                goto FoundMaterial;
            }

            foreach (var name in LegoMaterialDefinitions.Unlit)
            {
                if (!name.Contains(niMaterialProperty.Name.Value))
                    continue;
                unlitMaterial = slot.AttachComponent<UnlitMaterial>();
                goto FoundMaterial;
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

            FoundMaterial:

            if (pbsMetallic != null)
            {
                pbsMetallic.BlendMode = BlendMode.Opaque;
                if (niTexturingProperty is not null)
                {
                    if (niTexturingProperty.HasBaseTexture)
                        pbsMetallic.AlbedoTexture = ImportTexture(slot, context, niTexturingProperty.BaseTexture);
                    if (niTexturingProperty.HasGlowTexture)
                        pbsMetallic.EmissiveMap = ImportTexture(slot, context, niTexturingProperty.GlowTexture);
                    if (niTexturingProperty.HasNormalTexture)
                        pbsMetallic.NormalMap = ImportTexture(slot, context, niTexturingProperty.NormalTexture);
                }
                if (niAlphaProperty is not null)
                {
                    if ((niAlphaProperty.Flags & 0x0001) > 0 && niAlphaProperty.Threshold > 0) //alpha blend
                    {
                        pbsMetallic.BlendMode = BlendMode.Alpha;
                    }
                    else if (niAlphaProperty.Threshold > 0) pbsMetallic.BlendMode = BlendMode.Transparent;

                    pbsMetallic.AlphaCutoff = niAlphaProperty.Threshold / 255f;
                }

                if (niMaterialProperty is not null)
                {
                    pbsMetallic.AlbedoColor = niMaterialProperty.DiffuseColor.ToFrooxEngine().SetA(niMaterialProperty.Alpha);
                    pbsMetallic.EmissiveColor = niMaterialProperty.EmissiveColor.ToFrooxEngine() * niMaterialProperty.EmitMultiplier;
                    pbsMetallic.Smoothness = MathX.Clamp01(niMaterialProperty.Glossiness / 100f); //todo
                }

                return pbsMetallic;
            }

            if (pbsSpecular != null)
            {
                pbsSpecular.BlendMode = BlendMode.Opaque;
                if (niTexturingProperty is not null)
                {
                    if (niTexturingProperty.HasBaseTexture)
                        pbsSpecular.AlbedoTexture = ImportTexture(slot, context, niTexturingProperty.BaseTexture);
                    if (niTexturingProperty.HasGlowTexture)
                        pbsSpecular.EmissiveMap = ImportTexture(slot, context, niTexturingProperty.GlowTexture);
                    if (niTexturingProperty.HasNormalTexture)
                        pbsSpecular.NormalMap = ImportTexture(slot, context, niTexturingProperty.NormalTexture);
                }
                if (niAlphaProperty is not null)
                {
                    if ((niAlphaProperty.Flags & 0x0001) > 0 && niAlphaProperty.Threshold > 0)
                    {
                        pbsSpecular.BlendMode = BlendMode.Opaque;
                    }
                    else if (niAlphaProperty.Threshold > 0) pbsSpecular.BlendMode = BlendMode.Alpha;

                    pbsSpecular.AlphaCutoff = niAlphaProperty.Threshold / 255f;
                }
                if (niSpecularProperty is not null)
                {
                    if (niSpecularProperty.Flags > 0)
                    {
                        // TODO Set Specular color
                        // pbsSpecular.SpecularColor = ...
                    }
                }

                if (niMaterialProperty is not null)
                {
                    pbsSpecular.AlbedoColor = niMaterialProperty.DiffuseColor.ToFrooxEngine().SetA(niMaterialProperty.Alpha);
                    pbsSpecular.EmissiveColor = niMaterialProperty.EmissiveColor.ToFrooxEngine() * niMaterialProperty.EmitMultiplier;
                    pbsSpecular.Smoothness = MathX.Clamp01(niMaterialProperty.Glossiness / 100f); //todo
                }

                return pbsSpecular;
            }

            if (pbsRimMaterial != null)
            {
                pbsRimMaterial.Transparent.Value = false;
                if (niTexturingProperty is not null)
                {
                    if (niTexturingProperty.HasBaseTexture)
                        pbsRimMaterial.AlbedoTexture.Target = ImportTexture(slot, context, niTexturingProperty.BaseTexture);
                    if (niTexturingProperty.HasGlowTexture)
                        pbsRimMaterial.EmissiveMap.Target = ImportTexture(slot, context, niTexturingProperty.GlowTexture);
                    if (niTexturingProperty.HasNormalTexture)
                        pbsRimMaterial.NormalMap.Target = ImportTexture(slot, context, niTexturingProperty.NormalTexture);
                }
                if (niAlphaProperty is not null)
                {
                    if ((niAlphaProperty.Flags & 0x0001) > 0 && niAlphaProperty.Threshold > 0 || niAlphaProperty.Threshold > 0)
                    {
                        pbsRimMaterial.Transparent.Value = true;
                    }

                    pbsRimMaterial.AlbedoColor.Value = new colorX(
                        pbsRimMaterial.AlbedoColor.Value.rbg, niAlphaProperty.Threshold / 255f);
                }

                if (niMaterialProperty is not null)
                {
                    pbsRimMaterial.AlbedoColor.Value = niMaterialProperty.DiffuseColor.ToFrooxEngine().SetA(niMaterialProperty.Alpha);
                    pbsRimMaterial.EmissiveColor.Value = niMaterialProperty.EmissiveColor.ToFrooxEngine() * niMaterialProperty.EmitMultiplier;
                    pbsRimMaterial.Metallic.Value = MathX.Clamp01(niMaterialProperty.Glossiness / 100f); //todo
                }
            }

            if (unlitMaterial != null)
            {
                unlitMaterial.BlendMode.Value = BlendMode.Opaque;
                if (niTexturingProperty is not null)
                {
                    if (niTexturingProperty.HasBaseTexture)
                        unlitMaterial.Texture.Target = ImportTexture(slot, context, niTexturingProperty.BaseTexture);
                    if (niTexturingProperty.HasNormalTexture)
                        unlitMaterial.NormalMap = ImportTexture(slot, context, niTexturingProperty.NormalTexture);
                }
                if (niAlphaProperty is not null)
                {
                    if ((niAlphaProperty.Flags & 0x0001) > 0 && niAlphaProperty.Threshold > 0) //alpha blend
                    {
                        unlitMaterial.BlendMode.Value = BlendMode.Transparent;
                    }
                    else if (niAlphaProperty.Threshold > 0) unlitMaterial.BlendMode.Value = BlendMode.Alpha;

                    unlitMaterial.TintColor.Value = new colorX(
                        unlitMaterial.TintColor.Value.rbg, niAlphaProperty.Threshold / 255f);
                }

                if (niMaterialProperty is not null)
                {
                    unlitMaterial.TintColor.Value = niMaterialProperty.DiffuseColor.ToFrooxEngine().SetA(niMaterialProperty.Alpha);
                }
            }

            if (unlitDistanceLerpMaterial != null)
            {
                unlitDistanceLerpMaterial.BlendMode.Value = BlendMode.Opaque;
                if (niTexturingProperty is not null)
                {
                    var texture = ImportTexture(slot, context, niTexturingProperty.BaseTexture);
                    if (niTexturingProperty.HasBaseTexture)
                    {
                        unlitDistanceLerpMaterial.NearTexture.Target = texture;
                        unlitDistanceLerpMaterial.FarTexture.Target = texture;
                    }
                }
                if (niAlphaProperty is not null)
                {
                    if ((niAlphaProperty.Flags & 0x0001) > 0 && niAlphaProperty.Threshold > 0) //alpha blend
                    {
                        unlitDistanceLerpMaterial.BlendMode.Value = BlendMode.Transparent;
                    }
                    else if (niAlphaProperty.Threshold > 0) unlitDistanceLerpMaterial.BlendMode.Value = BlendMode.Alpha;

                    var color = new colorX(
                        unlitDistanceLerpMaterial.FarColor.Value.rbg, niAlphaProperty.Threshold / 255f);
                    unlitDistanceLerpMaterial.FarColor.Value = color;
                    unlitDistanceLerpMaterial.NearColor.Value = color;
                }

                if (niMaterialProperty is not null)
                {
                    var color = niMaterialProperty.DiffuseColor.ToFrooxEngine().SetA(niMaterialProperty.Alpha);
                    unlitDistanceLerpMaterial.FarColor.Value = color;
                    unlitDistanceLerpMaterial.NearColor.Value = color;
                }
            }
        }

        return slot.AttachComponent<PBS_VertexColorMetallic>();
    }

    internal static IAssetProvider<ITexture2D> ImportTexture(Slot slot, NiFileContext context, TexDesc target)
    {
        var path = target.Source.Value.Path.Value;
        var overallPath = Path.Combine(Path.GetDirectoryName(context.Path), path);
        if (!File.Exists(overallPath)) return null;
        var url = Engine.Current.LocalDB.ImportLocalAssetAsync(overallPath, LocalDB.ImportLocation.Copy).Result;
        var provider = slot.AttachComponent<StaticTexture2D>();
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
        //target.
        return provider;
    }

    internal static Light AttachLightWithValues(Slot slot, string slotName, LightType lightType, Color3 diffuse)
    {
        var lightSlot = slot.AddSlot(slotName);
        var light = lightSlot.AttachComponent<Light>();
        light.LightType.Value = lightType;
        light.Color.Value = diffuse.ToFrooxEngine();
        light.ShadowType.Value = ShadowType.None; // TODO Test me! Will this look good?
        return light;
    }

    internal static Slot AttachDynamicValueVariableWithValue<T>(Slot header, string slotName, T value)
    {
        var slot = header.AddSlot(slotName);
        var dVar = slot.AttachComponent<DynamicValueVariable<T>>();
        dVar.VariableName.Value = DYN_VAR_SPACE_PREFIX + value.ToString();
        dVar.Value.Value = value;
        return slot;
    }

    internal static Slot AttachDynamicValueVariableCollectionWithValues<T>(Slot header, string slotName, string slotChildName, T[] collection)
    {
        var slot = header.AddSlot(slotName);
        foreach (var item in collection)
        {
            var scStrings = header.AddSlot(slotChildName);
            var dVar = slot.AttachComponent<DynamicValueVariable<T>>();
            dVar.VariableName.Value = DYN_VAR_SPACE_PREFIX + item.ToString();
            dVar.Value.Value = item;
        }
        return slot;
    }
}