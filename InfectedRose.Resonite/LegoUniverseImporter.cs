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
    private static ModConfigurationKey<bool> enabled =
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
                // TODO Make async?
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                using var reader = new BitReader(stream);
                var file = new NiFile();
                file.Deserialize(reader);
                file.ReadBlocks(reader); 

                var header = slot.AddSlot("Header");
                ParseNiFile(file, header, path);
            }
            
            if (notLego.Count <= 0) return false;
            files = notLego.ToArray();
            return true;
        }
    }

    private static void ParseNiFile(NiFile file, Slot header, string path)
    {
        // Could we use reflection to make this less repetitive?
        AttachDynamicValueVariableWithSpaceAndValue(header, "Version", (uint)file.Header.Version);
        AttachDynamicValueVariableWithSpaceAndValue(header, "Endianness", (byte)file.Header.Endian);
        AttachDynamicValueVariableWithSpaceAndValue(header, "Version String", file.Header.VersionString);
        AttachDynamicValueVariableWithSpaceAndValue(header, "User Version", file.Header.UserVersion);

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

        AttachDynamicValueVariableCollectionWithSpaceAndValues(header, "Node Types", "Type", file.Header.NodeTypes);
        AttachDynamicValueVariableCollectionWithSpaceAndValues(header, "Strings", "String", file.Header.Strings);
        AttachDynamicValueVariableWithSpaceAndValue(header, "Max String Length", file.Header.MaxStringLength);
        AttachDynamicValueVariableCollectionWithSpaceAndValues(header, "Groups", "Group", file.Header.Groups);

        var sBlock = header.AddSlot("Block");
        if (file.Blocks[0] is NiNode root) // Will contain 1 element
            ParseNiNode(new NiFileContext { Path = path }, sBlock, root, null);
    }

    private static void ParseNiNode(NiFileContext context, Slot slot, NiNode obj, NiNode parent)
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
                    ParseNiNode(context, sNode, node, obj);
                    break;
                case NiTriShape triShape:
                    var triShapeNode = slot.AddSlot("Mesh");
                    ParseTriShape(context, triShapeNode, triShape);
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

    internal static void ParseTriShape(NiFileContext context, Slot slot, NiTriShape shape)
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
        mr.Materials.Add(DetermineMaterial(context, slot, shape));
        slot.AttachComponent<MeshCollider>();
    }

    internal static PBS_VertexColorMetallic DetermineMaterial(NiFileContext context, Slot slot, NiTriShape nts)
    {
        NiVertexColorProperty niVertexColorProperty = null;
        NiAlphaProperty niAlphaProperty = null;
        NiSpecularProperty niSpecularProperty = null;
        NiMaterialProperty niMaterialProperty = null;
        NiTexturingProperty niTexturingProperty = null;

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

        //if (niMaterialProperty != null)
        //{
        //    niMaterialProperty.Name.Contains("");
        //}

        var material = slot.AttachComponent<PBS_VertexColorMetallic>();
        
        material.AlphaHandling.Value = AlphaHandling.Opaque;

        if (niTexturingProperty is not null)
        {
            if (niTexturingProperty.HasBaseTexture)
                material.AlbedoTexture.Target = ImportTexture(context, slot, niTexturingProperty.BaseTexture);
            if (niTexturingProperty.HasGlowTexture)
                material.EmissiveMap.Target = ImportTexture(context, slot, niTexturingProperty.GlowTexture);
            if (niTexturingProperty.HasNormalTexture)
                material.NormalMap.Target = ImportTexture(context, slot, niTexturingProperty.NormalTexture);
        }
        if (niAlphaProperty is not null)
        {
            //0b1000000000 //alpha test mask
            //0b0000011110 //source blend mode
            //0b0111100000 //dest blend mode
            //0b0011101101 //problematicresult
            //problem source blend = 6, SRC_ALPHA
            //problem dest blend = 7, INV_SRC_ALPHA
            //todo: there are more alpha blend modes in the flags but the material only supports opaque, clip, and blend
            
            //todo: i dont know how the modes work, ???
            if ((niAlphaProperty.Flags & 0x0001) > 0 && niAlphaProperty.Threshold > 0) //alpha blend
            {
                material.AlphaHandling.Value = AlphaHandling.AlphaBlend;
            }
            else if (niAlphaProperty.Threshold > 0) material.AlphaHandling.Value = AlphaHandling.AlphaClip;

            material.AlphaClip.Value = niAlphaProperty.Threshold / 255f;
        }
        if (niSpecularProperty is not null)
        {
            //specular's flags is an enum with two values, essentially a boolean, with the values
            //"SPECULAR_DISABLED" and "SPECULAR_ENABLED"
            if (niSpecularProperty.Flags > 0)
            {
                
            }
        }

        if (niMaterialProperty is not null)
        {
            material.AlbedoColor.Value = niMaterialProperty.DiffuseColor.ToFrooxEngine().SetA(niMaterialProperty.Alpha);
            material.EmissiveColor.Value = niMaterialProperty.EmissiveColor.ToFrooxEngine() * niMaterialProperty.EmitMultiplier;
            material.Smoothness.Value = MathX.Clamp01(niMaterialProperty.Glossiness / 100f); //todo
            
        }
        // TODO Add more material types
        return material;
    }

    internal static IAssetProvider<ITexture2D> ImportTexture(NiFileContext context, Slot slot, TexDesc target)
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

    internal static Light AttachLightWithValues(Slot slot, string s, LightType lightType, Color3 diffuse)
    {
        var lightSlot = slot.AddSlot(s);
        var light = lightSlot.AttachComponent<Light>();
        light.LightType.Value = lightType;
        light.Color.Value = diffuse.ToFrooxEngine();
        light.ShadowType.Value = ShadowType.None; // TODO Test me!
        return light;
    }

    internal static Slot AttachDynamicValueVariableWithSpaceAndValue<T>(Slot header, string s, T value)
    {
        var slot = header.AddSlot(s);
        var dVar = slot.AttachComponent<DynamicValueVariable<T>>();
        dVar.VariableName.Value = DYN_VAR_SPACE_PREFIX + value.ToString();
        dVar.Value.Value = value;
        return slot;
    }

    internal static Slot AttachDynamicValueVariableCollectionWithSpaceAndValues<T>(Slot header, string s, string sc, T[] collection)
    {
        var slot = header.AddSlot(s);
        foreach (var item in collection)
        {
            var scStrings = header.AddSlot(sc);
            var dVar = slot.AttachComponent<DynamicValueVariable<T>>();
            dVar.VariableName.Value = DYN_VAR_SPACE_PREFIX + item.ToString();
            dVar.Value.Value = item;
        }
        return slot;
    }
}

internal class NiFileContext
{
    public string Path;
    public Dictionary<NiObject, Slot> ObjectSlots = new();
}