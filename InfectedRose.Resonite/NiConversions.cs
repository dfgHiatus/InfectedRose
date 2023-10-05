using Elements.Core;
using InfectedRose.Nif;

namespace InfectedRose.Resonite;

internal static class NiConversions
{
    internal static colorX ToFrooxEngine(this Color3 color)
    {
        return new colorX(color.R, color.G, color.B);
    }

    internal static colorX ToFrooxEngine(this Color4 color)
    {
        return new colorX(color.R, color.G, color.B, color.A);
    }
}
