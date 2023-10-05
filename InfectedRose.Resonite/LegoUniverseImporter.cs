using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using RakDotNet.IO;
using ResoniteModLoader;
using System.Collections.Generic;
using System.IO;
using InfectedRose.Nif;
using System.Runtime.Remoting.Messaging;
using Newtonsoft.Json.Linq;

namespace InfectedRose.Resonite;

public class LegoUniverseImporter : ResoniteMod
{
    public override string Name => "Lego Universe Importer";
    public override string Author => "dfgHiatus and FroZen";
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

                // Could we use reflection to make this less repetitive?
                AttachDynamicValueVariableWithSpaceAndValue(header, "Version", (uint)file.Header.Version);
                AttachDynamicValueVariableWithSpaceAndValue(header, "Endianness", (byte)file.Header.Endian);
                AttachDynamicValueVariableWithSpaceAndValue(header, "Version String", file.Header.VersionString);
                AttachDynamicValueVariableWithSpaceAndValue(header, "User Version", file.Header.UserVersion);

                // Need to define this as NodeInfo has two members, not one
                var sNodeInfo = header.AddSlot("Node Info");
                foreach (var item in file.Header.NodeInfo)
                {
                    var scNodeInfo = header.AddSlot("Type Index");
                    sNodeInfo.AttachComponent<DynamicValueVariable<ushort>>().Value.Value = item.TypeIndex;

                    var scSize = header.AddSlot("Size");
                    sNodeInfo.AttachComponent<DynamicValueVariable<uint>>().Value.Value = item.Size;
                }

                AttachDynamicValueVariableCollectionWithSpaceAndValues(header, "Node Types", "Type", file.Header.NodeTypes);
                AttachDynamicValueVariableCollectionWithSpaceAndValues(header, "Strings", "String", file.Header.Strings);
                AttachDynamicValueVariableWithSpaceAndValue(header, "Max String Length", file.Header.MaxStringLength);
                AttachDynamicValueVariableCollectionWithSpaceAndValues(header, "Groups", "Group", file.Header.Groups);


            }
            
            if (notLego.Count <= 0) return false;
            files = notLego.ToArray();
            return true;
        }
    }

    private static void AttachDynamicValueVariableWithSpaceAndValue<T>(Slot header, string s, T value)
    {
        var slot = header.AddSlot(s);
        var dVar = slot.AttachComponent<DynamicValueVariable<T>>();
        dVar.VariableName.Value = DYN_VAR_SPACE_PREFIX + value.ToString();
        dVar.Value.Value = value;
    }

    private static void AttachDynamicValueVariableCollectionWithSpaceAndValues<T>(Slot header, string s, string sc, T[] collection)
    {
        var slot = header.AddSlot(s);
        foreach (var item in collection)
        {
            var scStrings = header.AddSlot(sc);
            var dVar = slot.AttachComponent<DynamicValueVariable<T>>();
            dVar.VariableName.Value = DYN_VAR_SPACE_PREFIX + item.ToString();
            dVar.Value.Value = item;
        }
    }
}
