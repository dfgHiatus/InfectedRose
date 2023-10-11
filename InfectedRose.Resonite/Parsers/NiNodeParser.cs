using Elements.Core;
using FrooxEngine;
using InfectedRose.Nif;
using ResoniteModLoader;
using System.Threading.Tasks;

namespace InfectedRose.Resonite.Parsers;

internal class NiNodeParser
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
        slot.LocalPosition = new float3(obj.Position.X, obj.Position.Y, obj.Position.Z);
        slot.LocalRotation = obj.Rotation.ToFrooxEngine();
        slot.LocalScale = new float3(obj.Scale, obj.Scale, obj.Scale);
        await default(ToBackground);

        for (var i = 0; i < obj.Children.Length; i++)
        {
            switch (obj.Children[i].Value)
            {
                case NiNode node:
                    await default(ToWorld);
                    var sNode = slot.AddSlot("Node");
                    await default(ToBackground);
                    await ParseNiNode(sNode, context, node, obj);
                    break;
                case NiTriShape triShape:
                    await default(ToWorld);
                    var sMesh = slot.AddSlot("Mesh");
                    await default(ToBackground);
                    await TriShapeParser.ParseTriShape(sMesh, context, triShape);
                    break;
                default:
                    Msg($"Unknown child type: {obj.Children[i].Value.GetType()}");
                    break;
            }
        }

        await default(ToWorld);
        for (var i = 0; i < obj.Effects.Length; i++)
        {
            switch (obj.Effects[i].Value)
            {
                case NiDirectionalLight dLight:
                    var dLightSlot = "Directional Light";
                    AttachLightWithValues(slot.AddSlot(dLightSlot), dLightSlot, LightType.Directional, dLight.Diffuse);
                    break;
                case NiSpotLight sLight:
                    var sLightSlot = "Spot Light";
                    var light = AttachLightWithValues(slot.AddSlot(sLightSlot), sLightSlot, LightType.Spot, sLight.Diffuse);
                    light.SpotAngle.Value = sLight.OuterSpotAngle;
                    break;
                case NiPointLight pLight:
                    var pLightSlot = "Point Light";
                    AttachLightWithValues(slot.AddSlot(pLightSlot), pLightSlot, LightType.Point, pLight.Diffuse);
                    break;
            }
        }
        await default(ToBackground);
    }

    private static Light AttachLightWithValues(Slot slot, string slotName, LightType lightType, Color3 diffuse)
    {
        var lightSlot = slot.AddSlot(slotName);
        var light = lightSlot.AttachComponent<Light>();
        light.LightType.Value = lightType;
        light.Color.Value = diffuse.ToFrooxEngine();
        light.ShadowType.Value = ShadowType.None; // TODO Test me! Will this look good?
        return light;
    }
}
