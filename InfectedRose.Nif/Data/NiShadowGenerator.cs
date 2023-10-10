using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiShadowGenerator : NiObject
{
    public NiStringRef Name;
    public ushort Flags;
    public uint NumShadowCasters;
    public NiRef<NiNode>[] ShadowCasters;
    public uint NumShadowReceivers;
    public NiRef<NiNode>[] ShadowReceivers;
    public NiRef<NiDynamicEffect> Target;
    public float DepthBias;
    public ushort SizeHint;
    public float NearClippingDistance;
    public float FarClippingDistance;
    public float DirectionalLightFrustumWidth;
    
    
    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        Name = reader.Read<NiStringRef>(File);
        Flags = reader.Read<ushort>();
        NumShadowCasters = reader.Read<uint>();
        ShadowCasters = reader.ReadArrayN<NiRef<NiNode>>((int) NumShadowCasters, File);
        NumShadowReceivers = reader.Read<uint>();
        ShadowReceivers = reader.ReadArrayN<NiRef<NiNode>>((int) NumShadowReceivers, File);
        Target = reader.Read<NiRef<NiDynamicEffect>>(File);
        DepthBias = reader.Read<float>();
        SizeHint = reader.Read<ushort>();
        NearClippingDistance = reader.Read<float>();
        FarClippingDistance = reader.Read<float>();
        DirectionalLightFrustumWidth = reader.Read<float>();
    }
}