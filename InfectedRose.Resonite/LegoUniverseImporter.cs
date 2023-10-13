using Elements.Assets;
using Elements.Core;
using SkyFrost.Base;
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
    
    /*
    [AutoRegisterConfigKey]
    internal readonly static ModConfigurationKey<float4> DEBUG_QUATERNIONMULTIPLIER =
        new("debugquatmulti", "QUATMULTI", () => float4.One);
    
    [AutoRegisterConfigKey]
    internal readonly static ModConfigurationKey<int4> DEBUG_QUATERNIONSWIZZLE =
        new("debugquatswizzle", "QUATSWIZZLE", () => new int4(0, 1, 2, 3));
    
    
    [AutoRegisterConfigKey]
    internal readonly static ModConfigurationKey<float3> DEBUG_VECTORMULTIPLIER =
        new("debugvectormulti", "VECMULTI", () => float3.One);
    
    [AutoRegisterConfigKey]
    internal readonly static ModConfigurationKey<int3> DEBUG_VECTORSWIZZLE =
        new("debugvectorswizzle", "VECSWIZZLE", () => new int3(0, 1, 2));
*/
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

            var root = Engine.Current.WorldManager.FocusedWorld.RootSlot;
            root.StartGlobalTask(async delegate
            {
                var pbi = await root.World.
                    AddSlot("Import Indicator").
                    SpawnEntity<ProgressBarInterface, LegacySegmentCircleProgress>
                        (FavoriteEntity.ProgressBar);
                pbi.Slot.PositionInFrontOfUser();
                pbi.Initialize(canBeHidden: true);

                foreach (var path in hasLego)
                {
                    switch (Path.GetExtension(path).ToLower())
                    {
                        case NIF_EXTENSION:
                            await NiFileParser.ParseNiFile(slot, path, pbi);
                            await default(ToWorld);
                            pbi.ProgressDone("Import finished");
                            // root.RunInSeconds(5f, () => pbi.Slot.Destroy()); // TEST ME!
                            await default(ToBackground);
                            break;
                        case LUZ_EXTENSION:
                            await LuzFileParser.ParseLuzFile(slot, path, pbi);
                            await default(ToWorld);
                            pbi.ProgressDone("Import finished");
                            // root.RunInSeconds(5f, () => pbi.Slot.Destroy()); // TEST ME!
                            await default(ToBackground);
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