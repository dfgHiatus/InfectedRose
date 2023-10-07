
using System.Text;
using InfectedRose.Core;
using InfectedRose.Nif;
using RakDotNet.IO;

namespace InfectedRose.Kfm;

public class KfmFile : IConstruct
{
    public string VersionString;
    public byte Unknown0;
    public SizedString NiFFileName;
    public SizedString Master;
    public int Unknown1;
    public int Unknown2;
    public float Unknown3;
    public float Unknown4;
    public int NumAnimations;
    public Animation[] Animations;
    public int Unknown5;

    public void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }
    public void Deserialize(BitReader reader)
    {
        var versionStringBuilder = new StringBuilder();
        var character = reader.Read<byte>();
        while (character != 0xA)
        {
            versionStringBuilder.Append((char) character);

            character = reader.Read<byte>();
        }
        VersionString = versionStringBuilder.ToString();
        Unknown0 = reader.Read<byte>();
        NiFFileName = reader.Read<SizedString>();
        Master = reader.Read<SizedString>();
        Unknown1 = reader.Read<int>();
        Unknown2 = reader.Read<int>();
        Unknown3 = reader.Read<float>();
        Unknown4 = reader.Read<float>();
        NumAnimations = reader.Read<int>();
        Animations = reader.ReadArrayD<Animation>(NumAnimations);
        Unknown5 = reader.Read<int>();
    }
}