// rotation system
using UnityEngine;

public class Rotors : MonoBehaviour
{
    [SerializeField]
    Vector4 vFrom, vTo;

    private void Update()
    {
        Debug.Log(Rotate(vFrom, vTo));
    }

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
        public float wx, yw, wz, xy, zx, yz;

        public Rotor4(float a, Bivector4 b)
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

    // constructs and applies a rotor
    Vector4 Rotate(Vector4 vFrom, Vector4 vTo)
    {
        // normalizing
        Vector4 uFrom = vFrom.normalized;
        Vector4 uTo = vTo.normalized;

        Rotor4 r = ConstructRotor(uFrom, uTo);
        Vector4 v = ApplyRotor(r, uFrom) * vTo.magnitude;
        return v;
    }

    // rotates a vector by a rotor
    // ab: bivector, a * b - b * a
    // abc: trivector, (r.bc * v.a) + (r.ca * v.b) + (r.ab * v.c)
    // last row of left matrix gives quadvector (4-volume)
    /* left matrix * vector produces a row 5-vector which gets multipled
     * w/ columns of right matrix */
    /* |  a   wx  -yw   wz | | w | |  a   -wx   yw   wz  |
     * | -wx  a    xy  -zx | | x | |  wx   a   -xy   zx  |
     * |  yw -xy   a    yz | | y | | -yw   xy   a   -yz  |
     * | -wz  zx  -yz   a  | | z | |  wz  -zx   yz   a   | 
     * | xyz -yzw -zwx wxy |       |  xyz -yzw -zwx  wxy | */
    Vector4 ApplyRotor(Rotor4 r, Vector4 v)
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

    // constructs a rotor to rotate from one vector to another
    Rotor4 ConstructRotor(Vector4 vFrom, Vector4 vTo)
    {
        float a = 1f + Vector4.Dot(vTo, vFrom);

        // left rotation is b * a so flip vectors
        Bivector4 b = Wedge(vTo, vFrom);

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

        return new Rotor4(a, b);
    }

    // area spanned by two vectors
    // ab = -ba so order matters
    Bivector4 Wedge(Vector4 a, Vector4 b) => new Bivector4(
        a.w * b.x - b.w * a.x, a.y * b.w - b.y * a.w, a.w * b.z - b.w * a.z,
        a.x * b.y - b.x * a.y, a.z * b.x - b.z * a.x, a.y * b.z - b.y * a.z
    );
}