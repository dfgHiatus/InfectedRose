using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif
{
    public class NiGeometry : NiAvObject
    {
        public NiRef<NiGeometryData> Data { get; set; }
        
        public NiRef<NiSkinInstance> Skin { get; set; }
        
        public MaterialData MaterialData { get; set; }
        

        public override void Deserialize(BitReader reader)
        {
            base.Deserialize(reader);

            Data = reader.Read<NiRef<NiGeometryData>>(File);

            Skin = reader.Read<NiRef<NiSkinInstance>>(File);

            MaterialData = reader.Read<MaterialData>();
        }

        public override void Serialize(BitWriter writer)
        {
            base.Serialize(writer);

            writer.Write(Data);

            writer.Write(Skin);

            writer.Write(MaterialData);
        }
    }
}