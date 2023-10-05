using System.Numerics;

namespace InfectedRose.Nif
{
    public struct Color3
    {
        public float R;

        public float G;

        public float B;

        public static implicit operator Vector3(Color3 a) => new(a.R, a.G, a.B);
    }
}