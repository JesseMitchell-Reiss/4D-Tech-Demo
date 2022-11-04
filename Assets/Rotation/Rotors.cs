// rotation system
using UnityEngine;

public class Rotors : MonoBehaviour
{
    // parallelogram spanned by two vectors
    struct Bivector4
    {
        public float wx, yw, wz, xy, zx, yz;

        public Bivector4(float wx, float yw, float wz, float xy, float zx, float yz)
        {
            this.wx = wx;
            this.yw = yw;
            this.wz = wz;
            this.xy = xy;
            this.zx = zx;
            this.yz = yz;
        }
    }

    // matrix for the rotation basically
    struct Rotor4
    {
        // dot product (contains cosine of angle)
        public float a;

        // plane (contains sine of angle)
        public float wx, yw, wz, yz, xy, zx;

        public Rotor4(float a, Bivector4 b)
        {
            this.a = a;
            wx = b.wx;
            yw = b.yw;
            wz = b.wz;
            yz = b.yz;
            xy = b.xy;
            zx = b.zx;
        }
    }
}
