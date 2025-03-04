using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using InfectedRose.Database.Concepts.Tables;
using InfectedRose.World;

namespace InfectedRose.Interface
{
    public static class ZoneManager
    {
        public static async Task GenerateZoneAsync(string file)
        {
            var serializer = new XmlSerializer(typeof(Zone));
            
            var zone = new Zone();
            
            if (File.Exists(file))
            {
#if NETSTANDARD2_1_OR_GREATER
                await using var stream = File.OpenRead(file);
#else
                using var stream = File.OpenRead(file);
#endif

                zone = (Zone) serializer.Deserialize(stream);
            }
            else
            {
#if NETSTANDARD2_1_OR_GREATER
                await using var stream = File.Create(file);
#else
                using var stream = File.Create(file);
#endif
                serializer.Serialize(stream, zone);
                
                Console.WriteLine($"Created zone config file: \"{file}\".");
                
                return;
            }
            
            await zone.SaveAsync(Path.Combine(Interface.Configuration.Output, "maps"));

            var zones = Interface.Database["ZoneTable"];

            var row = zones.Create(zone.Id);

            var _ = new ZoneTableTable(row)
            {
                zoneName = zone.Name,
                zoneID = (int) zone.Id,
                ghostdistance = zone.GhostDistance,
                scriptID = -1,
                locStatus = 0,
                DisplayDescription = zone.Description,
                heightInChunks = zone.Terrain.Size,
                widthInChunks = zone.Terrain.Size
            };
        }
    }
}