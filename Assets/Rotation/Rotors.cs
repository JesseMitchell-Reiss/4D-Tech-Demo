// rotation system
using UnityEngine;

public class Rotors : MonoBehaviour
{
    [SerializeField]
    Vector4 vFrom, vTo;

    // parallelogram spanned by two vectors
    struct Bivector
    {
        public float wx, yw, wz, xy, zx, yz;

        public Bivector(float wx, float yw, float wz, float xy, float zx, float yz)
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
    struct Rotor
    {
        // dot product (contains cosine of angle)
        public float a;

        // plane (contains sine of angle)
        public float wx, yw, wz, xy, zx, yz;

        public Rotor(float a, Bivector b)
        {
            this.a = a;
            wx = b.wx;
            yw = b.yw;
            wz = b.wz;
            xy = b.xy;
            zx = b.zx;
            yz = b.yz;
        }
    }

    // constructs and applies a Rotor
    Vector4 Rotate(Vector4 vFrom, Vector4 vTo)
    {
        // normalizing
        Vector4 uFrom = vFrom.normalized;
        Vector4 uTo = vTo.normalized;

        Rotor r = ConstructRotor(uFrom, uTo);
        Vector4 v = ApplyRotor(r, uFrom) * vTo.magnitude;
        return v;
    }

    // rotates a vector by a Rotor
    // ab: bivector, u.a * v.b - u.b * v.a
    // abc: trivector, (r.bc * v.a) + (r.ca * v.b) + (r.ab * v.c)
    // last row of left matrix gives quadvector (4-volume)
    /* left matrix * vector produces a row 5-vector which gets multipled
     * w/ columns of right matrix */
    /* |  a   wx  -yw   wz | | w | |  a   -wx   yw   wz |
     * | -wx  a    xy  -zx | | x | |  wx   a   -xy   zx |
     * |  yw -xy   a    yz | | y | | -yw   xy   a   -yz |
     * | -wz  zx  -yz   a  | | z | |  wz  -zx   yz   a  | 
     * | xyz ywz  zwx  wxy |       |  xyz ywz  zwx  wxy | */
    Vector4 ApplyRotor(Rotor r, Vector4 v)
    {
        // trivectors
        Vector4 tv;

        //     xyz          yzx          zxy
        tv.w = r.yz * v.x + r.zx * v.y + r.xy * v.z;

        //     ywz          wzy          zyw
        tv.x = r.wz * v.y - r.yz * v.w + r.yw * v.z;

        //     zwx          wxz          xzw
        tv.y = r.wx * v.z - r.zx * v.w - r.wz * v.x;

        //     wxy          xyw          ywx
        tv.z = r.xy * v.w + r.yw * v.x + r.wx * v.y;

        Vector4 m;
        m.w =  r.a  * v.w + r.wx * v.x - r.yw * v.y + r.wz * v.z;
        m.x = -r.wx * v.w + r.a  * v.x + r.xy * v.y - r.zx * v.z;
        m.y =  r.yw * v.w - r.xy * v.x + r.a  * v.y + r.yz * v.z;
        m.z = -r.wz * v.w + r.zx * v.x - r.yz * v.y + r.a  * v.z;
        float qv = tv.w * v.w + tv.x * v.x + tv.y * v.y + tv.z * v.z;

        /* |  a   -wx   yw   wz  |
         * |  wx   a   -xy   zx  |
         * | -yw   xy   a   -yz  |
         * |  wz  -zx   yz   a   |
         * |  xyz -yzw -zwx  wxy |
         */
        Vector4 n;
        n.w =  m.w * r.a  + m.x * r.wx - m.y * r.yw + m.z * r.wz + qv * tv.w;
        n.x = -m.w * r.wx + m.x * r.a  + m.y * r.xy - m.z * r.zx - qv * tv.x;
        n.y =  m.w * r.yw - m.x * r.xy + m.y * r.a  + m.z * r.yz - qv * tv.y;
        n.z = -m.w * r.wz + m.x * r.zx - m.y * r.yz + m.z * r.a  + qv * tv.z;
        return n;
    }

    // constructs a Rotor to rotate from one vector to another
    Rotor ConstructRotor(Vector4 vFrom, Vector4 vTo)
    {
        float a = 1f + Vector4.Dot(vTo, vFrom);

        // left rotation is b * a so flip vectors
        Bivector b = Wedge(vTo, vFrom);

        // normalizing
        float sqrMag =
            a * a + b.wx * b.wx + b.yw * b.yw + b.wz * b.wz + b.xy * b.xy + b.yz * b.yz +
            b.zx * b.zx;

        float magnitude = Mathf.Sqrt(sqrMag);

        a /= magnitude;
        b.wx /= magnitude;
        b.yw /= magnitude;
        b.wz /= magnitude;
        b.xy /= magnitude;
        b.yz /= magnitude;
        b.zx /= magnitude;

        return new Rotor(a, b);
    }

    // Rotor-Rotor product
    Rotor RotorProduct(Rotor p, Rotor q)
    {
        Rotor r;
        // dot product, sorta
        r.a =
            p.a * q.a - p.wx * q.wx - p.yw * q.yw - p.wz * q.wz -
            p.xy * q.xy - p.yz * p.yz - p.zx * p.zx;

        // wedge product, also sorta
        r.wx =
            p.wx * q.a + q.wx * p.a -
            p.xy * q.yw + q.xy * p.yw -
            p.zx * q.wz + q.zx * p.wz;

        r.yw =
            p.yw * q.a + q.yw * p.a -
            p.wx * q.xy + q.wx * p.xy +
            p.wz * q.yz - p.yz * q.wz;

        r.wz =
            p.wz * q.a + q.wz * p.a +
            p.zx * q.wx - q.zx * p.wx +
            p.yz * q.yw - q.yz * p.yw;

        r.xy =
            p.xy * q.a + q.xy * p.a -
            p.yw * q.wx + q.yw * p.wx -
            p.zx * q.yz + q.zx * p.yz;

        r.yz =
            p.yz * q.a + q.yz * p.a -
            p.wz * q.yw + q.wz * p.yw -
            p.zx * q.xy + q.zx * p.xy;

        r.zx =
            p.zx * q.a + q.zx * p.a +
            p.wx * q.wz - q.wx * p.wz -
            p.xy * q.yz + q.xy * p.yz;

        return r;
    }

    Rotor RotorInverse(Rotor r) => new Rotor(
        r.a, new Bivector(-r.wx, -r.yw, -r.wz, -r.xy, -r.zx, -r.yz
    ));

    // area spanned by two vectors
    // ab = -ba so order matters
    Bivector Wedge(Vector4 a, Vector4 b) => new Bivector(
        a.w * b.x - b.w * a.x, a.y * b.w - b.y * a.w, a.w * b.z - b.w * a.z,
        a.x * b.y - b.x * a.y, a.z * b.x - b.z * a.x, a.y * b.z - b.y * a.z
    );
}