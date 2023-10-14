using System.Threading.Tasks;
using FrooxEngine;
using InfectedRose.Nif;

namespace InfectedRose.Resonite.Parsers;

public static class NiLODParser
{
    internal static async Task ParseLODNode(Slot slot, NiFileContext context, NiLODNode obj, NiNode parent)
    {
        await NiNodeParser.ParseNiNode(slot, context, obj, obj);
        var lodData = obj.Data.Value;
        if (lodData is NiRangeLODData range)
        {
            await default(ToWorld);
            var center = slot.AddSlot("LODCenter");
            center.LocalPosition = range.Center.ToFrooxEngine();
            for (var i = 0; i < range.Ranges.Length; i++)
            {
                var r = range.Ranges[i];
                var affected = obj.Children[i].Value;
                if (context.ObjectSlots.TryGetValue(affected, out var affectedSlot))
                {
                    var near = center.AttachComponent<UserDistanceValueDriver<bool>>();
                    var nearValue = center.AttachComponent<ValueField<bool>>();
                    var far = center.AttachComponent<UserDistanceValueDriver<bool>>();
                    var farValue = center.AttachComponent<ValueField<bool>>();
                    var combiner = center.AttachComponent<MultiBoolConditionDriver>();

                    near.Distance.Value = r.Near * NiConversions.SCALING_FACTOR;
                    near.FarValue.Value = true;
                    near.NearValue.Value = false;
                    near.TargetField.Target = nearValue.Value;
                    near.Node.Value = UserRoot.UserNode.View;
                    
                    far.Distance.Value = r.Far * NiConversions.SCALING_FACTOR;
                    far.FarValue.Value = false;
                    far.NearValue.Value = true;
                    far.TargetField.Target = farValue.Value;
                    far.Node.Value = UserRoot.UserNode.View;

                    var nearCondition = combiner.Conditions.Add();
                    nearCondition.Field.Target = nearValue.Value;
                    var farCondition = combiner.Conditions.Add();
                    farCondition.Field.Target = farValue.Value;
                    combiner.Mode.Value = MultiBoolConditionDriver.ConditionMode.All;

                    combiner.Target.Target = affectedSlot.ActiveSelf_Field;
                    //nearCondition.
                }
            }
        }
    }
}