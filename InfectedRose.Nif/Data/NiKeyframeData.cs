using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public class NiKeyframeData : NiObject
{
    public uint NumRotationKeys;
    public uint RotationType;
    public List<QuatKey> QuaternionKeys;
    public NiKeyGroup<float>[] XYZRotations;
    public NiKeyGroup<Vector3> Translations;
    public NiKeyGroup<float> Scales;

    public override void Serialize(BitWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(BitReader reader)
    {
        NumRotationKeys = reader.Read<uint>();
        if (NumRotationKeys != 0)
        {
            RotationType = reader.Read<uint>();
        }
        if (RotationType is not KeyType.XYZ_ROTATION_KEY)
        {
            QuaternionKeys = RotationType is KeyType.TBC_KEY
                ? new List<QuatKey>(reader.ReadArrayD<QuatKey_TBC>((int) NumRotationKeys))
                : reader.ReadArrayD<QuatKey>((int) NumRotationKeys).ToList();
        }
        else
        {
            XYZRotations = reader.ReadArrayD<NiKeyGroup<float>>(3);
        }
        Translations = reader.Read<NiKeyGroup<Vector3>>();
        Scales = reader.Read<NiKeyGroup<float>>();
    }
}