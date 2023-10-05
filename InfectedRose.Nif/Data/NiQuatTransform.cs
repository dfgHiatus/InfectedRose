using System.Numerics;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public struct NiQuatTransform
{
    public Vector3 Translation;
    public NiQuaternion Rotation;
    public float Scale;
}