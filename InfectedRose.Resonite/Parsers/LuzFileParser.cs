using System;
using Elements.Core;
using FrooxEngine;
using InfectedRose.Database;
using InfectedRose.Database.Fdb;
using InfectedRose.Luz;
using InfectedRose.Lvl;
using InfectedRose.Terrain;
using RakDotNet.IO;
using ResoniteModLoader;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InfectedRose.Resonite.Parsers;

internal static class LuzFileParser
{
    static void Msg(object o)
    {
        ResoniteMod.Msg(o);
    }

    internal static async Task ParseLuzFile(Slot root, string path, IProgressIndicator pbi)
    {
        await default(ToBackground);

        if (!File.Exists(LegoUniverseImporter.Config.GetValue(LegoUniverseImporter.CDClientDirectory)))
        {
            await default(ToWorld);
            pbi.ProgressFail("Config not pointing at valid cdclient");
            await default(ToBackground);
            return;
        }

        await default(ToWorld);
        root.LocalRotation = floatQ.Euler(90, 0, 0);
        pbi.UpdateProgress(0f, "Parsing luz file", "");
        await default(ToBackground);

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BitReader(stream);
        var file = new LuzFile();
        file.Deserialize(reader);

        await default(ToWorld);
        var newRoot = root.AddSlot(Path.GetFileName(path));
        await default(ToBackground);
        await ParseLuzFile(newRoot, file, path, pbi);
    }

    internal static async Task ParseLuzFile(Slot root, LuzFile file, string path, IProgressIndicator pbi)
    {
        await default(ToWorld);
        pbi.UpdateProgress(0f, "Creating root object slots", "");
        
        var terrainSlot = root.AddSlot("Terrain");
        //terrainSlot.LocalScale = float3.One * 0.25f;
        terrainSlot.LocalRotation = floatQ.Euler(0, 0, 90); //TODO: do the conversion properly
        
        var nifs = root.AddSlot("NifTemplates");
        nifs.ActiveSelf = false;
        
        var objectTemplates = root.AddSlot("ObjectTemplates");
        objectTemplates.ActiveSelf = false;
        
        var sBlock = root.AddSlot("Scene");
        //sBlock.LocalScale = float3.One * 0.25f;
        
        pbi.UpdateProgress(0f, "Parsing terrain file", "");
        await default(ToBackground);
        
        {
            
            using var stream = new FileStream(Path.Combine(Path.GetDirectoryName(path), file.TerrainFileName), FileMode.Open, FileAccess.Read);
            using var reader = new BitReader(stream);
            var terrain = new TerrainFile();
            terrain.Deserialize(reader);

            var mesh = terrain.ToFrooxEngine();

            var localDb = Engine.Current.LocalDB;
            var tempFilePath = localDb.GetTempFilePath(".meshx");
            mesh.SaveToFile(tempFilePath);
            var url = await localDb.ImportLocalAssetAsync(tempFilePath, LocalDB.ImportLocation.Move);

            await default(ToWorld);
            var staticMesh = terrainSlot.AttachComponent<StaticMesh>();
            staticMesh.URL.Value = url;

            var meshRenderer = terrainSlot.AttachComponent<MeshRenderer>();
            meshRenderer.Mesh.Target = staticMesh;

            var terrainMaterial = terrainSlot.AttachComponent<PBS_Metallic>();
            meshRenderer.Materials.Add(terrainMaterial);

            var terrainCollider = terrainSlot.AttachComponent<MeshCollider>();
            terrainCollider.Mesh.Target = staticMesh;
            terrainCollider.CharacterCollider.Value = true;
            terrainCollider.Type.Value = ColliderType.Static;

            pbi.UpdateProgress(0f, "Parsing lvl files", "");
            await default(ToBackground);
        }

        var lvls = new Dictionary<string, LvlFile>();
        var clientDirectory = LegoUniverseImporter.Config.GetValue(LegoUniverseImporter.CDClientDirectory);
        var clientBaseDirectory = Path.GetDirectoryName(clientDirectory);
        
        foreach (var p in file.Scenes.Select(i => i.FileName))
        {
            if (Path.GetExtension(p).ToLower() is not LegoUniverseImporter.LVL_EXTENSION) continue;
            using var stream = new FileStream(Path.Combine(Path.GetDirectoryName(path), p), FileMode.Open, FileAccess.Read);
            using var reader = new BitReader(stream);
            var lvl = new LvlFile();
            lvl.Deserialize(reader);
            lvls.Add(p, lvl);
        }
        var ids = lvls.Select(i => i.Value).Where(i => i.LevelObjects is not null).SelectMany(i => i.LevelObjects.Templates).Select(i => i.Lot).Distinct().ToList();

        var sceneIds = file.Scenes.Select(i => i.SceneId).ToList();

        using var clientStream = new FileStream(clientDirectory, FileMode.Open, FileAccess.Read);
        using var clientReader = new BitReader(clientStream);
        var databaseFile = new DatabaseFile();
        databaseFile.Deserialize(clientReader);
        var database = new AccessDatabase(databaseFile);

        var renderAssets = ids.
            Select(database.GetRenderComponent).
            Where(get => get is not null).
            Select(i => i.render_asset.Replace(@"\\", "/").Replace(@"\", "/").ToLower().Trim()).
            Distinct().
            ToList();
        
        var processed = 0;
        var total = renderAssets.Count;

        var needsDone = new List<string>(renderAssets);
        var tasks = new List<Task>();
        while (needsDone.Count > 0)
        {
            var count = Math.Min(needsDone.Count, 10 - tasks.Count);
            if (count > 0)
            {
                var doing = needsDone.Take(count);
                needsDone.RemoveRange(0, count);
                var toAppend = doing.Select(i => root.World.RootSlot.StartGlobalTask(async () =>
                {
                    if (Path.GetExtension(i) is not LegoUniverseImporter.NIF_EXTENSION) return; //todo: kfm
                    var fullPath = Path.Combine(clientBaseDirectory, i);
                    await NiFileParser.ParseNiFile(nifs, fullPath, i, pbi);
                }));
                tasks.AddRange(toAppend);
            }
            await Task.WhenAny(tasks);
            processed += tasks.RemoveAll(i => i.IsCompleted);
            await default(ToWorld);
            pbi.UpdateProgress((processed / (float) total) * 0.33333f, $"Processing nifs {processed}/{total}", "");
            await default(ToBackground);
        }
        await default(ToWorld);
        pbi.UpdateProgress(0.33333f, "Finishing up processing nifs...", "");
        await default(ToBackground);
        await Task.WhenAll(tasks);

        /*
        for (var index = 0; index < renderAssets.Count; index++)
        {
            var asset = renderAssets[index];

            await default(ToWorld);
            pbi.UpdateProgress(index / (float)renderAssets.Count * 0.33333f, $"Processing {index}/{renderAssets.Count}: {asset}", "");
            await default(ToBackground);
            
            var fullPath = Path.Combine(clientBaseDirectory, asset);
            if (Path.GetExtension(fullPath) is not LegoUniverseImporter.NIF_EXTENSION) continue; //todo: kfm
            await NiFileParser.ParseNiFile(nifs, fullPath, asset, pbi);
        }
        */

        for (var index = 0; index < ids.Count; index++)
        {
            var id = ids[index];
            
            await default(ToWorld);
            var template = objectTemplates.AddSlot(id.ToString());
            pbi.UpdateProgress((index / (float)ids.Count * 0.33333f) + 0.33333f, $"Processing templates {index}/{ids.Count}", "");
            await default(ToBackground);

            var renderAsset = database.GetRenderComponent(id);
            if (renderAsset is not null)
            {
                var partialPath = renderAsset.render_asset.Replace(@"\\", "/").Replace(@"\", "/").ToLower().Trim();

                await default(ToWorld);
                var get = nifs.FindChild(partialPath)?.FindChild("Scene");
                if (get is not null)
                {
                    var templateRenderComponent = template.AddSlot("RenderComponent");
                    templateRenderComponent.Tag = get.Parent.Name;
                    var d = get.Duplicate(templateRenderComponent, false);

                    var phys = database.GetPhysicsComponent(id);
                    
                    if (phys is not null)
                    {
                        foreach (var mesh in d.GetComponentsInChildren<MeshRenderer>())
                        {
                            if (mesh is SkinnedMeshRenderer) continue;
                            var collider = mesh.Slot.AttachComponent<MeshCollider>();
                            collider.Mesh.Target = mesh.Mesh.Target;
                            collider.CharacterCollider.Value = true;
                            if (phys.collisionGroup is not 21) //TODO: figure out how collision groups actually work
                            {
                                collider.Type.Value = ColliderType.Static;
                            }
                        }
                    }
                }
                await default(ToBackground);
            }
        }

        await default(ToWorld);
        
        pbi.UpdateProgress(0.75f, "Copying templates into scene", "");
        
        foreach (var lvl in lvls.Where(i => i.Value.LevelObjects is not null))
        {
            foreach (var objects in lvl.Value.LevelObjects.Templates)
            {
                var template = objectTemplates.FindChild(objects.Lot.ToString());
                if (template is not null)
                {
                    var o = template.Duplicate(sBlock, false);
                    o.Tag = o.Name;
                    o.Name = objects.ObjectId.ToString();
                    o.LocalPosition = objects.Position.ToFrooxEngine();
                    o.LocalRotation = objects.Rotation.ToFrooxEngine();
                    o.LocalScale = new float3(objects.Scale, objects.Scale, objects.Scale);
                    if (objects.LegoInfo.TryGetValue("renderDisabled", out var disabled) && (bool)disabled)
                    {
                        foreach (var mesh in o.GetComponentsInChildren<MeshRenderer>())
                        {
                            mesh.Enabled = false;
                        }
                    }
                    /*
                    else
                    {
                        var scene = file.Scenes.FirstOrDefault(i => i.FileName == lvl.Key);
                        if (scene is not null && 
                            objects.LegoInfo.TryGetValue("sceneIDOverrideEnabled", out var shouldOverride) &&
                            (bool)shouldOverride && objects.LegoInfo.TryGetValue("sceneIDOverride", out var over) &&
                            !sceneIds.Contains((uint)(int)over))
                        {
                            foreach (var mesh in o.GetComponentsInChildren<MeshRenderer>())
                            {
                                mesh.Enabled = false;
                            }
                        }
                    }
                    */
                }
            }
        }
        await default(ToBackground);
    }
}
