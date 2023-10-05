using RakDotNet.IO;

namespace InfectedRose.Nif
{
    public class NiTriShapeData : NiTriBasedGeomData
    {
        public uint TrianglePointCount;
        public Triangle[] Triangles { get; set; }
        
        public NiMatchGroup[] Groups { get; set; }
        
        public override void Deserialize(BitReader reader)
        {
            base.Deserialize(reader);

            TrianglePointCount = reader.Read<uint>();

            if (reader.Read<byte>() is not 0)
            {
                Triangles = new Triangle[TriangleCount];

                for (var i = 0; i < TriangleCount; i++)
                {
                    Triangles[i] = reader.Read<Triangle>();
                }
            }
            Groups = new NiMatchGroup[reader.Read<ushort>()];
            for (var i = 0; i < Groups.Length; i++)
            {
                Groups[i] = reader.Read<NiMatchGroup>(File);
            }
        }
        public override void Serialize(BitWriter writer)
        {
            TriangleCount = (ushort) Triangles.Length;
            
            base.Serialize(writer);

            writer.Write((uint) (TriangleCount * 3));

            foreach (var triangle in Triangles)
            {
                writer.Write(triangle);
            }
            writer.Write((ushort) Groups.Length);
            
            foreach (var group in Groups)
            {
                //writer.Write(group);
            }
        }
    }
}