using FrooxEngine;
using InfectedRose.Nif;
using RakDotNet.IO;
using ResoniteModLoader;
using System.IO;
using System.Threading.Tasks;

namespace InfectedRose.Resonite.Parsers;

internal static class NiFileParser
{
    static void Msg(object o)
    {
        ResoniteMod.Msg(o);
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
        NiHeaderParser.ParseNiHeader(header, file);
        await default(ToBackground);

        var context = new NiFileContext();
        context.Path = path;

        await default(ToWorld);
        context.AssetSlot = header.AddSlot("Assets");
        var sBlock = header.AddSlot("Scene");
        await default(ToBackground);

        if (file.Blocks[0] is NiNode root) // The root Block will contain exactly 1 element
            await NiNodeParser.ParseNiNode(sBlock, context, root, null);
    }
}
