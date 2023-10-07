using InfectedRose.Core;
using InfectedRose.Nif;
using RakDotNet.IO;

namespace InfectedRose.Kfm;

public class Animation : IConstruct
{
    /*
    <compound name="Animation">
        <add name="Event Code" type="int" />
        <add name="Name" type="SizedString" ver2="16927488" />
        <add name="KF File Name" type="SizedString" />
        <add name="Index" type="int">An index?</add>
        <add name="Num Transitions" type="int" />
        <add name="Transitions" type="Transition" arr1="Num Transitions">Max = Num Animations -1? No transition from animation to itself.</add>
        <add name="Num Unknown Data" type="uint" ver1="33685531" ver2="33685531" />
        <add name="Unknown Data" type="UnknownData" arr1="Num Unknown Data" ver1="33685531" ver2="33685531" />
        <add name="Unknown Int" type="uint" ver1="33685531" ver2="33685531" />
        <add name="Num Unknown Data 2" type="uint" ver1="33685531" ver2="33685531" />
        <add name="Unknown Data 2" type="UnknownData2" arr1="Num Unknown Data 2" ver1="33685531" ver2="33685531" />
    </compound>
     */
    public int EventCode;
    public SizedString KFFileName;
    public int Index;
    public int NumTransitions;
    public Transition[] Transitions;
    public void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public void Deserialize(BitReader reader)
    {
        EventCode = reader.Read<int>();
        KFFileName = reader.Read<SizedString>();
        Index = reader.Read<int>();
        NumTransitions = reader.Read<int>();
        Transitions = reader.ReadArrayD<Transition>(NumTransitions);
    }
}