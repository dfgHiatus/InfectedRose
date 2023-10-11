using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using InfectedRose.Database;
using InfectedRose.Database.Fdb;
using InfectedRose.Luz;
using InfectedRose.Lvl;
using InfectedRose.Nif;
using RakDotNet.IO;
using ResoniteModLoader;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AlphaHandling = FrooxEngine.AlphaHandling;

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
    internal static ModConfiguration config;

    [AutoRegisterConfigKey]
    internal readonly static ModConfigurationKey<bool> enabled =
        new("enabled", "Enabled", () => true);
    
    [AutoRegisterConfigKey]
    internal readonly static ModConfigurationKey<string> cdclientDirectory =
        new("cdClientDirectory", "Path to cdclient.fdb", () => "");

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
                            await ParseNiFile(slot, path);
                            break;
                        case LUZ_EXTENSION:
                            await ParseLuzFile(slot, path);
                            break;
                    }
                }
            });

            if (notLego.Count <= 0) return false;
            files = notLego.ToArray();
            return true;
        }
    }

    internal static async Task ParseLuzFile(Slot root, string path)
    {
        await default(ToBackground);

        if (!File.Exists(config.GetValue(cdclientDirectory)))
        {
            Msg("Config not pointing at valid cdclient");
            return;
        }
        
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BitReader(stream);
        var file = new LuzFile();
        file.Deserialize(reader);

        await default(ToWorld);
        var newRoot = root.AddSlot(Path.GetFileName(path));
        await default(ToBackground);
        await ParseLuzFile(newRoot, file, path);
    }
    
    internal static async Task ParseLuzFile(Slot root, LuzFile file, string path)
    {
        await default(ToBackground);

        Msg($"Parsing luz file");
        var lvls = new List<LvlFile>();
        var clientDirectory = config.GetValue(cdclientDirectory);
        var clientBaseDirectory = Path.GetDirectoryName(clientDirectory);
        
        Msg($"Parsing lvl files");

        foreach (var p in file.Scenes.Select(i => i.FileName))
        {
            if (Path.GetExtension(p).ToLower() is not LVL_EXTENSION) continue;
            using var stream = new FileStream(Path.Combine(Path.GetDirectoryName(path), p), FileMode.Open, FileAccess.Read);
            using var reader = new BitReader(stream);
            var lvl = new LvlFile();
            lvl.Deserialize(reader);
            lvls.Add(lvl);
        }
        Msg($"Got {lvls.Count} lvl files");
        var ids = lvls.
            Where(i => i.LevelObjects is not null).
            SelectMany(i => i.LevelObjects.Templates).
            Select(i => i.Lot).
            Distinct().
            ToList();
        Msg($"Got {ids.Count} unique lots");
        
        using var clientStream = new FileStream(clientDirectory, FileMode.Open, FileAccess.Read);
        using var clientReader = new BitReader(clientStream);
        var databaseFile = new DatabaseFile();
        databaseFile.Deserialize(clientReader);
        var database = new AccessDatabase(databaseFile);

        var renderAssets = ids.
            Select(database.GetRenderComponent).
            Where(get => get is not null).
            Select(i => i.render_asset).
            Distinct().
            ToList();

        await default(ToWorld);
        var nifs = root.AddSlot("NifTemplates");
        nifs.ActiveSelf = false;
        await default(ToBackground);

        foreach (var asset in renderAssets)
        {
            var partialPath = asset.Replace(@"\\", "/").ToLower();
            var fullPath = Path.Combine(clientBaseDirectory, partialPath);
            if (Path.GetExtension(fullPath) is not NIF_EXTENSION) continue; //todo: kfm
            Msg($"Importing nif {partialPath}");
            await ParseNiFile(nifs, fullPath, partialPath);
        }
        
        await default(ToWorld);
        var objectTemplates = root.AddSlot("ObjectTemplates");
        objectTemplates.ActiveSelf = false;
        await default(ToBackground);

        foreach (var id in ids)
        {
            await default(ToWorld);
            var template = objectTemplates.AddSlot(id.ToString());
            await default(ToBackground);

            var renderAsset = database.GetRenderComponent(id);
            if (renderAsset is not null)
            {
                var partialPath = renderAsset.render_asset.Replace(@"\\", "/").ToLower();

                await default(ToWorld);
                var get = nifs.FindChild(partialPath)?.FindChild("Scene");
                await default(ToBackground);

                if (get is not null)
                {
                    await default(ToWorld);
                    var templateRenderComponent = template.AddSlot("RenderComponent");
                    var d = get.Duplicate(templateRenderComponent, false);
                    await default(ToBackground);

                    if (database.GetPhysicsComponent(id) is not null)
                    {
                        await default(ToWorld);
                        foreach (var mesh in d.GetComponentsInChildren<MeshRenderer>())
                        {
                            if (mesh is SkinnedMeshRenderer) continue;
                            var collider = mesh.Slot.AttachComponent<MeshCollider>();
                            collider.Mesh.Target = mesh.Mesh.Target;
                            collider.CharacterCollider.Value = true;
                        }
                        await default(ToBackground);
                    }
                }
            }
        }
        
        await default(ToWorld);
        var sBlock = root.AddSlot("Scene");
        sBlock.LocalScale = float3.One * 0.25f;

        foreach (var objects in lvls.Where(i => i.LevelObjects is not null).SelectMany(i => i.LevelObjects.Templates))
        {
            var template = objectTemplates.FindChild(objects.Lot.ToString());
            if (template is not null)
            {
                var o = template.Duplicate(sBlock, false);
                o.LocalPosition = objects.Position.ToFrooxEngine();
                o.LocalRotation = objects.Rotation; //TODO
                o.LocalScale = new float3(objects.Scale, objects.Scale, objects.Scale);
            }
        }
        await default(ToBackground);
    }

    internal static async Task ParseNiFile(Slot root, string path, string name)
    {
        await default(ToBackground);
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BitReader(stream);
        var file = new NiFile();
        file.Deserialize(reader);
        file.ReadBlocks(reader);
        // await file.ReadBlocksAsync(reader); 

        //var header = slot.AddSlot("Header");
        await default(ToWorld);
        var header = root.AddSlot(name);
        await default(ToBackground);

        await ParseNiFile(header, file, path);
    }

    internal static async Task ParseNiFile(Slot root, string path) => await ParseNiFile(root, path, Path.GetFileName(path));

    internal static async Task ParseNiFile(Slot header, NiFile file, string path)
    {
        await default(ToWorld);
        ParseNiHeader(header, file);
        await default(ToBackground);

        var context = new NiFileContext();
        context.Path = path;

        await default(ToWorld);
        context.AssetSlot = header.AddSlot("Assets");    
        var sBlock = header.AddSlot("Scene");
        await default(ToBackground);

        if (file.Blocks[0] is NiNode root) // The root Block will contain exactly 1 element
            await ParseNiNode(sBlock, context, root, null);
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

    internal static async Task ParseNiNode(Slot slot, NiFileContext context, NiNode obj, NiNode parent)
    {
        await default(ToBackground);
        context.ObjectSlots.Add(obj, slot);
        
        await default(ToWorld);
        slot.LocalPosition = new float3(obj.Position.X, obj.Position.Y, obj.Position.Z);
        slot.LocalRotation = obj.Rotation.ToFrooxEngine();
        slot.LocalScale = new float3(obj.Scale, obj.Scale, obj.Scale);
        await default(ToBackground);

        for (var i = 0; i < obj.Children.Length; i++)
        {
            switch (obj.Children[i].Value)
            {
                case NiNode node:
                    await default(ToWorld);
                    var sNode = slot.AddSlot("Node");
                    await default(ToBackground);
                    await ParseNiNode(sNode, context, node, obj);
                    break;
                case NiTriShape triShape:
                    await default(ToWorld);
                    var sMesh = slot.AddSlot("Mesh");
                    await default(ToBackground);
                    await ParseTriShape(sMesh, context, triShape);
                    break;
                default:
                    Msg($"Unknown child type: {obj.Children[i].Value.GetType()}");
                    break;
            }
        }

        await default(ToWorld);
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
        await default(ToBackground);
    }

    internal static async Task ParseTriShape(Slot slot, NiFileContext context, NiTriShape shape)
    {
        await default(ToBackground);
        context.ObjectSlots.Add(shape, slot);
        
        await default(ToWorld);
        slot.LocalPosition = new float3(shape.Position.X, shape.Position.Y, shape.Position.Z);
        slot.LocalRotation = shape.Rotation.ToFrooxEngine();
        slot.LocalScale = new float3(shape.Scale,shape.Scale,shape.Scale);
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