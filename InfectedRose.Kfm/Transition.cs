using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Kfm;

public class Transition : IConstruct
{
    /*
    <compound name="Transition">
        <add name="Animation" type="int">Refers to another animation</add>
        <add name="Type" type="int" />
        <add name="Duration" type="float" cond="Type != 5" />
        <add name="Num Intermediate Anims" type="int" cond="Type != 5" />
        <add name="Intermediate Anims" type="IntermediateAnim" arr1="Num Intermediate Anims" cond="Type != 5" />
        <add name="Num TextKeyPairs" type="int" cond="Type != 5" />
    </compound>
     */
    public int Animation;
    public int Type;
    public float Duration;
    public int NumIntermediateAnims;
    public IntermediateAnim[] IntermediateAnims;
    public int NumTextKeyPairs;
    //???
    
    public void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public void Deserialize(BitReader reader)
    {
        Animation = reader.Read<int>();
        Type = reader.Read<int>(); //?????????? what is this?
        if (Type != 5)
        {
            Duration = reader.Read<float>();
            NumIntermediateAnims = reader.Read<int>();
            IntermediateAnims = reader.ReadArrayD<IntermediateAnim>(NumIntermediateAnims);
            NumTextKeyPairs = reader.Read<int>();
        }
    }
}