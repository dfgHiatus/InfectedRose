﻿using Elements.Assets;
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
                ParseNiFile(header, file);
            }
            
            if (notLego.Count <= 0) return false;
            files = notLego.ToArray();
            return true;
        }
    }

    internal static void ParseNiFile(Slot header, NiFile file)
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
        mr.Materials.Add(DetermineMaterial(slot, shape));
        slot.AttachComponent<MeshCollider>();
    }

    internal static IAssetProvider<Material> DetermineMaterial(Slot slot, NiTriShape nts)
    {
        NiVertexColorProperty niVertexColorProperty = null;
        NiAlphaProperty niAlphaProperty = null;
        NiSpecularProperty niSpecularProperty = null;
        NiMaterialProperty niMaterialProperty = null;

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
                return slot.AttachComponent<PBS_VertexColorMetallic>();
            }

            foreach (var name in LegoMaterialDefinitions.PBS_VertexColorMetallic_Emissive)
            {
                if (!name.Contains(niMaterialProperty.Name.Value)) 
                    continue;
                var m = slot.AttachComponent<PBS_VertexColorMetallic>();
                m.EmissiveColor.Value = colorX.White;
                return m;
            }

            foreach (var name in LegoMaterialDefinitions.PBS_VertexColorMetallic_Transparent)
            {
                if (!name.Contains(niMaterialProperty.Name.Value)) 
                    continue;
                var m = slot.AttachComponent<PBS_VertexColorMetallic>();
                m.AlphaHandling.Value = FrooxEngine.AlphaHandling.AlphaBlend;
                m.AlbedoColor.Value = new colorX(1f, 1f, 1f, 0.33f);
                return m;
            }

            foreach (var name in LegoMaterialDefinitions.PBS_VertexColorMetallic_Metallic)
            {
                if (!name.Contains(niMaterialProperty.Name.Value)) 
                    continue;
                var m = slot.AttachComponent<PBS_VertexColorMetallic>();
                m.Metallic.Value = 1f;
                return m;
            }

            foreach (var name in LegoMaterialDefinitions.PBS_VertexColorSpecular)
            {
                if (!name.Contains(niMaterialProperty.Name.Value)) 
                    continue;
                return slot.AttachComponent<PBS_VertexColorSpecular>();
            }

            foreach (var name in LegoMaterialDefinitions.PBS_VertexColorSpecular_Specular)
            {
                if (!name.Contains(niMaterialProperty.Name.Value)) 
                    continue;
                return slot.AttachComponent<PBS_VertexColorSpecular>();
            }

            foreach (var name in LegoMaterialDefinitions.PBS_RimMetallic)
            {
                if (!name.Contains(niMaterialProperty.Name.Value)) 
                    continue;
                return slot.AttachComponent<PBS_RimMetallic>();
            }

            foreach (var name in LegoMaterialDefinitions.GrayscaleMaterial)
            {
                if (!name.Contains(niMaterialProperty.Name.Value)) 
                    continue;
                return slot.AttachComponent<GrayscaleMaterial>();
            }

            foreach (var name in LegoMaterialDefinitions.GrayscaleMaterial)
            {
                if (!name.Contains(niMaterialProperty.Name.Value)) 
                    continue;
                return slot.AttachComponent<UnlitDistanceLerpMaterial>();
            }

            foreach (var name in LegoMaterialDefinitions.Unlit)
            {
                if (!name.Contains(niMaterialProperty.Name.Value)) 
                    continue;
                return slot.AttachComponent<UnlitMaterial>();
            }

            foreach (var name in LegoMaterialDefinitions.Unlit_VertexColorAndAlpha)
            {
                if (!name.Contains(niMaterialProperty.Name.Value)) 
                    continue;
                var m = slot.AttachComponent<UnlitMaterial>();
                m.UseVertexColors.Value = true;
                m.BlendMode.Value = BlendMode.Alpha;
                m.TintColor.Value = new colorX(1f, 1f, 1f, 0.2f);
                return m;
            }

            foreach (var name in LegoMaterialDefinitions.Unlit_Overlay)
            {
                if (!name.Contains(niMaterialProperty.Name.Value)) continue;
                return slot.AttachComponent<OverlayUnlitMaterial>();
            }
        }

        return slot.AttachComponent<PBS_VertexColorMetallic>();
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