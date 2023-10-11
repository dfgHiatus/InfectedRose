using System.Collections.Generic;
using FrooxEngine;
using InfectedRose.Nif;
using ResoniteModLoader;

namespace InfectedRose.Resonite.Parsers;

internal static class NiHeaderParser
{
    static void Msg(object o)
    {
        ResoniteMod.Msg(o);
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
            dynVar.VariableName.Value = LegoUniverseImporter.DYN_VAR_SPACE_PREFIX + s;

            s = "Size";
            var scSize = sNodeInfo.AddSlot(s);
            var dynVar2 = scSize.AttachComponent<DynamicValueVariable<uint>>();
            dynVar2.Value.Value = item.TypeIndex;
            dynVar2.VariableName.Value = LegoUniverseImporter.DYN_VAR_SPACE_PREFIX + s;
        }

        AttachDynamicValueVariableCollectionWithValues(header, "Node Types", "Type", file.Header.NodeTypes);
        AttachDynamicValueVariableCollectionWithValues(header, "Strings", "String", file.Header.Strings);
        AttachDynamicValueVariableWithValue(header, "Max String Length", file.Header.MaxStringLength);
        AttachDynamicValueVariableCollectionWithValues(header, "Groups", "Group", file.Header.Groups);
    }

    private static Slot AttachDynamicValueVariableWithValue<T>(Slot header, string slotName, T value)
    {
        var slot = header.AddSlot(slotName);
        var dVar = slot.AttachComponent<DynamicValueVariable<T>>();
        dVar.VariableName.Value = LegoUniverseImporter.DYN_VAR_SPACE_PREFIX + value.ToString();
        dVar.Value.Value = value;
        return slot;
    }

    private static Slot AttachDynamicValueVariableCollectionWithValues<T>(Slot header, string slotName, string slotChildName, IEnumerable<T> collection)
    {
        var slot = header.AddSlot(slotName);
        foreach (var item in collection)
        {
            var scStrings = header.AddSlot(slotChildName);
            var dVar = scStrings.AttachComponent<DynamicValueVariable<T>>();
            dVar.VariableName.Value = LegoUniverseImporter.DYN_VAR_SPACE_PREFIX + item.ToString();
            dVar.Value.Value = item;
        }
        return slot;
    }
}
