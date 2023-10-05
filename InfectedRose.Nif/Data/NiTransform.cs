using System.Numerics;
using InfectedRose.Core;
using RakDotNet.IO;

namespace InfectedRose.Nif;

public struct NiTransform
{
    public Matrix3X3 Rotation;
    public Vector3 Translation;
    public float Scale;
}