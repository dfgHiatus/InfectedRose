using System.Numerics;

namespace InfectedRose.Nif;

public struct NiQuaternion
{
    public float W;
    public float X;
    public float Y;
    public float Z;
    public static implicit operator Quaternion(NiQuaternion a) => new(a.X, a.Y, a.Z, a.W);
}