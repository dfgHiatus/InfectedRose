using Elements.Core;
using FrooxEngine;
using InfectedRose.Nif;
using ResoniteModLoader;
using System.Threading.Tasks;

namespace InfectedRose.Resonite.Parsers;

internal static class NiNodeParser
{
    static void Msg(object o)
    {
        ResoniteMod.Msg(o);
    }

    internal static async Task ParseNiNode(Slot slot, NiFileContext context, NiNode obj, NiNode parent)
    {
        await default(ToBackground);
        context.ObjectSlots.Add(obj, slot);
        await default(ToWorld);
        slot.LocalPosition = obj.Position.ToFrooxEngine();
        slot.LocalRotation = obj.Rotation.ToFrooxEngine();
        slot.LocalScale = new float3(obj.Scale, obj.Scale, obj.Scale);
        await default(ToBackground);

        for (var i = 0; i < obj.Children.Length; i++)
        {
            switch (obj.Children[i].Value)
            {
                case NiLODNode lod:
                    await default(ToWorld);
                    var lNode = slot.AddSlot(string.IsNullOrWhiteSpace(lod.Name) ? "LOD" : lod.Name);
                    await default(ToBackground);
                    await NiLODParser.ParseLODNode(lNode, context, lod, obj);
                    break;
                case NiNode node:
                    await default(ToWorld);
                    var sNode = slot.AddSlot(string.IsNullOrWhiteSpace(node.Name) ? "Node" : node.Name);
                    await default(ToBackground);
                    await ParseNiNode(sNode, context, node, obj);
                    break;
                case NiTriShape triShape:
                    await default(ToWorld);
                    var sMesh = slot.AddSlot(string.IsNullOrWhiteSpace(triShape.Name) ? "TriShape" : triShape.Name);
                    await default(ToBackground);
                    await TriBasedGeomParser.ParseTriGeom(sMesh, context, triShape);
                    break;
                case NiTriStrips triStrips:
                    await default(ToWorld);
                    var stMesh = slot.AddSlot(string.IsNullOrWhiteSpace(triStrips.Name) ? "TriStrips" : triStrips.Name);
                    await default(ToBackground);
                    await TriBasedGeomParser.ParseTriGeom(stMesh, context, triStrips);
                    break;
                default:
                    Msg($"Unknown child type: {obj.Children[i].Value.GetType()}");
                    break;
            }
        }

        await default(ToWorld);
        for (var i = 0; i < obj.Effects.Length; i++)
        {
            var o = obj.Effects[i].Value;
            if (o is NiLight)
            {
                if (o.Affected.Length == 0) continue;
            }
            switch (o)
            {
                case NiDirectionalLight dLight:
                    var dLightSlot = "Directional Light";
                    AttachLightWithValues(slot.AddSlot(dLightSlot), dLightSlot, LightType.Directional, dLight);
                    break;
                case NiSpotLight sLight:
                    var sLightSlot = "Spot Light";
                    var light = AttachLightWithValues(slot.AddSlot(sLightSlot), sLightSlot, LightType.Spot, sLight);
                    light.SpotAngle.Value = sLight.OuterSpotAngle;
                    break;
                case NiPointLight pLight:
                    var pLightSlot = "Point Light";
                    AttachLightWithValues(slot.AddSlot(pLightSlot), pLightSlot, LightType.Point, pLight);
                    break;
            }
        }
        await default(ToBackground);
    }

    private static Light AttachLightWithValues(Slot slot, string slotName, LightType lightType, NiLight obj)
    {
        var lightSlot = slot.AddSlot(slotName);
        var light = lightSlot.AttachComponent<Light>();
        light.LightType.Value = lightType;
        light.Color.Value = obj.Diffuse.ToFrooxEngine() * obj.Dimmer;
        light.ShadowType.Value = ShadowType.None; // TODO Test me! Will this look good?
        lightSlot.LocalRotation = floatQ.Euler(180, 0, 0);
        return light;
    }
}
