using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using RakDotNet.IO;

namespace InfectedRose.Terrain.Editor
{
    public class TerrainEditor
    {
        public TerrainFile Source { get; private set; }
        
        public HeightLayer HeightLayer { get; }
        
        public ColorLayer ColorLayer { get; }
        
        public ColorLayer SecondColorLayer { get; }

        public int ChunkSize { get; private set; } = 65;

        public TerrainEditor(TerrainFile file)
        {
            Source = file;

            HeightLayer = new HeightLayer(this);
            ColorLayer = new ColorLayer(this, false);
            SecondColorLayer = new ColorLayer(this, true);
        }

        public static async Task<TerrainEditor> OpenEmptyAsync(TerrainSettings setting)
        {
            var file = new TerrainFile();

            var chunks = new List<Chunk>();

            var index = 0;

            var center = setting.Size / 2;

            for (var x = 0; x < setting.Size; x++)
            {
                for (var y = 0; y < setting.Size; y++)
                {
                    var chunk = GenerateEmptyChunk(index++, setting);

                    chunk.HeightMap.PositionX = (x - center) * setting.ChunkSpacing;
                    chunk.HeightMap.PositionY = (y - center) * setting.ChunkSpacing;

                    chunks.Add(chunk);
                }
            }

            file.Weight = setting.Size;
            file.Height = setting.Size;
            file.ChunkTotalCount = setting.Size * setting.Size;

            file.Chunks = chunks;

            return new TerrainEditor(file)
            {
                ChunkSize = setting.ChunkSize
            };
        }

        private static Chunk GenerateEmptyChunk(int index, TerrainSettings settings)
        {
            var chunk = Chunk.Empty;

            chunk.HeightMap = HeightMap.FromSize(settings.ChunkSize, settings.DefaultHeight);

            chunk.Index = index;

            return chunk;
        }

        public static async Task<TerrainEditor> OpenAsync(string path)
        {
#if NETSTANDARD2_1_OR_GREATER
            await using var stream = File.OpenRead(path);
#else
            using var stream = File.OpenRead(path);
#endif

            using var reader = new BitReader(stream);

            var file = new TerrainFile();

            file.Deserialize(reader);

            return new TerrainEditor(file);
        }
        public async Task SaveAsync(string path)
        {
            var heightMap = Source.GenerateHeightMap();

            Source.ApplyHeightMap(heightMap);
#if NETSTANDARD2_1_OR_GREATER
            await using var stream = File.Create(path);
#else
            using var stream = File.Create(path);
#endif
            using var writer = new BitWriter(stream);

            Source.Serialize(writer);
        }

        public Chunk FindChunkAt(Vector2 position)
        {
            var x = (int) (position.X % ChunkSize);
            var y = (int) (position.Y % ChunkSize);

            var chunk = Source.Chunks[x * Source.Weight + y];

            return chunk;
        }
        public void Load()
        {
            HeightLayer.LoadHeightMap();
            ColorLayer.LoadColorMap();
            SecondColorLayer.LoadColorMap();
        }

        public void Apply()
        {
            HeightLayer.ApplyHeightMap();
            ColorLayer.ApplyColorMap();
            SecondColorLayer.ApplyColorMap();
        }
    }
}