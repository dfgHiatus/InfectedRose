using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif.Controllers;

public class NiBoneLODController : NiTimeController
{
    public uint LOD;
    public uint NumLODs;
    public uint NumNodeGroups;
    public NodeSet[] NodeGroups;
    public uint NumShapeGroups0;
    public SkinInfoSet[] ShapeGroups0;
    public uint NumShapeGroups1;
    public NiRef<NiTriBasedGeom>[] ShapeGroups1;

    public override void Deserialize(BitReader reader)
    {
        base.Deserialize(reader);
        LOD = reader.Read<uint>();
        NumLODs = reader.Read<uint>();
        NumNodeGroups = reader.Read<uint>();
        NodeGroups = reader.ReadArrayN<NodeSet>((int) NumLODs, File);
        NumShapeGroups0 = reader.Read<uint>();
        ShapeGroups0 = reader.ReadArrayN<SkinInfoSet>((int) NumShapeGroups0, File);
        NumShapeGroups1 = reader.Read<uint>();
        ShapeGroups1 = reader.ReadArrayN<NiRef<NiTriBasedGeom>>((int) NumShapeGroups1, File);
    }
}