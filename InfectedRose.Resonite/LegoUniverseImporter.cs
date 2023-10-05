using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using InfectedRose.Nif;
using RakDotNet.IO;
using ResoniteModLoader;
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
            dSpace.SpaceName.Value = DYN_VAR_SPACE_PREFIX.Remove('/');

            foreach (var path in hasLego)
            {
                // TODO Make async?
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                using var reader = new BitReader(stream);
                var file = new NiFile();
                file.Deserialize(reader);
                file.ReadBlocks(reader); 

                var header = slot.AddSlot("Header");
                ParseNiFile(file, header);
            }
            
            if (notLego.Count <= 0) return false;
            files = notLego.ToArray();
            return true;
        }
    }

    private static void ParseNiFile(NiFile file, Slot header)
    {
        // Could we use reflection to make this less repetitive?
        header = AttachDynamicValueVariableWithSpaceAndValue(header, "Version", (uint)file.Header.Version);
        header = AttachDynamicValueVariableWithSpaceAndValue(header, "Endianness", (byte)file.Header.Endian);
        header = AttachDynamicValueVariableWithSpaceAndValue(header, "Version String", file.Header.VersionString);
        header = AttachDynamicValueVariableWithSpaceAndValue(header, "User Version", file.Header.UserVersion);

        // Need to define this as NodeInfo has two members, not one
        var sNodeInfo = header.AddSlot("Node Info");
        foreach (var item in file.Header.NodeInfo)
        {
            var scNodeInfo = header.AddSlot("Type Index");
            sNodeInfo.AttachComponent<DynamicValueVariable<ushort>>().Value.Value = item.TypeIndex;

            var scSize = header.AddSlot("Size");
            sNodeInfo.AttachComponent<DynamicValueVariable<uint>>().Value.Value = item.Size;
        }

        header = AttachDynamicValueVariableCollectionWithSpaceAndValues(header, "Node Types", "Type", file.Header.NodeTypes);
        header = AttachDynamicValueVariableCollectionWithSpaceAndValues(header, "Strings", "String", file.Header.Strings);
        header = AttachDynamicValueVariableWithSpaceAndValue(header, "Max String Length", file.Header.MaxStringLength);
        header = AttachDynamicValueVariableCollectionWithSpaceAndValues(header, "Groups", "Group", file.Header.Groups);

        var sBlock = header.AddSlot("Block");
        if (file.Blocks[0] is NiNode root) // Will contain 1 element
            ParseNiNode(new NiFileContext(), sBlock, root, null);
    }

    private static void ParseNiNode(NiFileContext context, Slot slot, NiNode obj, NiNode parent)
    {
        context.ObjectSlots.Add(obj, slot);
        
        slot.LocalPosition = new float3(obj.Position.X, obj.Position.Y, obj.Position.Z);
        slot.LocalRotation = obj.Rotation.ToFrooxEngine();
        slot.LocalScale = new float3(obj.Scale, obj.Scale, obj.Scale);

        for (int i = 0; i < obj.Children.Length; i++)
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

        for (int i = 0; i < obj.Effects.Length; i++)
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

        if (skinned)
        {
            var renderer = slot.AttachComponent<SkinnedMeshRenderer>();
            var bones = shape.Skin.Value.Bones;
            foreach (var bone in bones)
            {
                renderer.Bones.Add(context.ObjectSlots.TryGetValue(bone.Value, out var boneSlot) ? boneSlot : null);
            }
        }
        else
        {
            var renderer = slot.AttachComponent<MeshRenderer>();
        }
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
    public Dictionary<NiObject, Slot> ObjectSlots = new();
}