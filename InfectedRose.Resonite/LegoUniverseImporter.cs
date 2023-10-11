using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using InfectedRose.Resonite.Parsers;
using ResoniteModLoader;
using System.Collections.Generic;
using System.IO;

namespace InfectedRose.Resonite;

public class LegoUniverseImporter : ResoniteMod
{
    public override string Name => "Lego Universe Importer";
    public override string Author => "dfgHiatus and Fro Zen";
    public override string Version => "1.0.0";

    internal const string LUZ_EXTENSION = ".luz";
    internal const string LVL_EXTENSION = ".lvl";
    internal const string NIF_EXTENSION = ".nif";
    internal const string DYN_VAR_SPACE_PREFIX = "LegoUniverse/";
    internal static ModConfiguration Config;

    [AutoRegisterConfigKey]
    internal readonly static ModConfigurationKey<bool> enabled =
        new("enabled", "Enabled", () => true);
    
    [AutoRegisterConfigKey]
    internal readonly static ModConfigurationKey<string> CDClientDirectory =
        new("cdClientDirectory", "Path to cdclient.fdb", () => "");

    public override void OnEngineInit()
    {
        new Harmony("net.dfgHiatus.LegoUniverseImporter").PatchAll();
        Config = GetConfiguration();
    }

    [HarmonyPatch(typeof(UniversalImporter), "Import", typeof(AssetClass), typeof(IEnumerable<string>),
            typeof(World), typeof(float3), typeof(floatQ), typeof(bool))]
    public class UniversalImporterPatch
    {
        static bool Prefix(ref IEnumerable<string> files)
        {
            if (!Config.GetValue(enabled)) return true; // Skip if disabled

            List<string> hasLego = new();
            List<string> notLego = new();
            foreach (var file in files)
            {
                if (Path.GetExtension(file).ToLower() is NIF_EXTENSION or LUZ_EXTENSION or LVL_EXTENSION)
                    hasLego.Add(file);
                else
                    notLego.Add(file);
            }

            var slot = Engine.Current.WorldManager.FocusedWorld.AddSlot("Lego Universe Import");
            slot.PositionInFrontOfUser();

            var dSpace = slot.AttachComponent<DynamicVariableSpace>();
            dSpace.OnlyDirectBinding.Value = true;
            dSpace.SpaceName.Value = DYN_VAR_SPACE_PREFIX.TrimEnd('/');

            Engine.Current.WorldManager.FocusedWorld.RootSlot.StartGlobalTask(async delegate
            {
                foreach (var path in hasLego)
                {
                    switch (Path.GetExtension(path).ToLower())
                    {
                        case NIF_EXTENSION:
                            await NiFileParser.ParseNiFile(slot, path);
                            break;
                        case LUZ_EXTENSION:
                            await LuzFileParser.ParseLuzFile(slot, path);
                            break;
                    }
                }
            });

            if (notLego.Count <= 0) return false;
            files = notLego.ToArray();
            return true;
        }
    }
}