using FrooxEngine;
using InfectedRose.Nif;
using System.Collections.Generic;

namespace InfectedRose.Resonite;

internal class NiFileContext
{
    public string Path;
    public Slot AssetSlot;
    public Dictionary<NiObject, Slot> ObjectSlots = new();
}
