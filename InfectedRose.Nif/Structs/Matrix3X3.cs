using System.Numerics;

namespace InfectedRose.Nif
{
    public struct Matrix3X3
    {
        public float M11;

        public float M21;

        public float M31;

        public float M12;

        public float M22;

        public float M32;

        public float M13;

        public float M23;

        public float M33;
        
        public Matrix4x4 FourByFour =>
            new(
                M11, M12, M13, 0,
                M21, M22, M23, 0,
                M31, M32, M33, 0,
                0,0,0,1
            );
    }
}