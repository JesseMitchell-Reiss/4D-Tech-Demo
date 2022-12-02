// rotation system
using UnityEngine;

using static UnityEngine.Mathf;

public class Bivector4
{
    float WX, YW, WZ, XY, YZ, ZX;

    public float wx { get => WX; } 
    public float yw { get => YW; }
    public float wz { get => WZ; }
    public float xy { get => XY; }
    public float yz { get => YZ; }
    public float zx { get => ZX; }

    public Bivector4(float wx, float yw, float wz, float xy, float yz, float zx)
    {
        WX = wx; YW = yw; WZ = wz; XY = xy; YZ = yz; ZX = zx;
    }

    public static Bivector4 operator *(float a, Bivector4 b) =>
        new Bivector4(a * b.wx, a * b.yw, a * b.wz, a * b.xy, a * b.yz, a * b.zx);

    public static Bivector4 operator * (Bivector4 b, float a) =>
        new Bivector4(a * b.wx, a * b.yw, a * b.wz, a * b.xy, a * b.yz, a * b.zx);

    public static Bivector4 zero { get => new Bivector4(0f, 0f, 0f, 0f, 0f, 0f); }

    public Bivector4 normalized { get => this * (1f / magnitude); }

    public float magnitude
    { 
        get => 
            Sqrt(WX * WX + YW * YW + WZ * WZ + XY * XY + YZ * YZ + ZX * ZX); 
    }

    public static Bivector4 Wedge(Vector4 a, Vector4 b) => new Bivector4(
        a.w * b.x - b.w * a.x, a.y * b.w - b.y * a.w, a.w * b.z - b.w * a.z,
        a.x * b.y - b.x * a.y, a.y * b.z - b.y * a.z, a.z * b.x - b.z * a.x
    );
}

// defines a matrix that operates on vectors to rotate them (R' * a * R)
public class Rotor4
{
    // scalar value (contains cosine of angle)
    float A;

    // plane of the vectors (contains sine of angle)
    // providing both the bivector form and the individual coordinates for convenience
    Bivector4 B;
    float WX, YW, WZ, XY, YZ, ZX;

    public float a { get => A; }
    public Bivector4 b { get => B; }
    public float wx { get => WX; }
    public float yw { get => YW; }
    public float wz { get => WZ; }
    public float xy { get => XY; }
    public float yz { get => YZ; }
    public float zx { get => ZX; }

    public Rotor4(float a, Bivector4 b)
    {
        A = a; B = b; WX = b.wx; YW = b.yw; WZ = b.wz; XY = b.xy; YZ = b.yz; ZX = b.zx;
    }

    public Rotor4(float a, float wx, float yw, float wz, float xy, float yz, float zx)
    {
        A = a; B = new Bivector4(wx, yw, wz, xy, yz, zx); 
        WX = wx; YW = yw; WZ = wz; XY = xy; YZ = yz; ZX = zx;
    }

    public static Rotor4 operator *(Rotor4 r, float s) =>
        new Rotor4(s * r.a, s * r.b);

    public static Rotor4 operator *(float s, Rotor4 r) =>
        new Rotor4(s * r.a, s * r.b);

    // rotor-rotor product for combined rotations
    public static Rotor4 operator * (Rotor4 p, Rotor4 q)
    {
        // dot product, sorta
        float a =
            p.a * q.a - p.wx * q.wx - p.yw * q.yw - p.wz * q.wz -
            p.xy * q.xy - p.yz * p.yz - p.zx * p.zx;

        // wedge product, also sorta
        float wx =
            p.wx * q.a + q.wx * p.a -
            p.xy * q.yw + q.xy * p.yw -
            p.zx * q.wz + q.zx * p.wz;

        float yw =
            p.yw * q.a + q.yw * p.a -
            p.wx * q.xy + q.wx * p.xy +
            p.wz * q.yz - p.yz * q.wz;

        float wz =
            p.wz * q.a + q.wz * p.a +
            p.zx * q.wx - q.zx * p.wx +
            p.yz * q.yw - q.yz * p.yw;

        float xy =
            p.xy * q.a + q.xy * p.a -
            p.yw * q.wx + q.yw * p.wx -
            p.zx * q.yz + q.zx * p.yz;

        float yz =
            p.yz * q.a + q.yz * p.a -
            p.wz * q.yw + q.wz * p.yw -
            p.zx * q.xy + q.zx * p.xy;

        float zx =
            p.zx * q.a + q.zx * p.a +
            p.wx * q.wz - q.wx * p.wz -
            p.xy * q.yz + q.xy * p.yz;

        return new Rotor4(a, wx, yw, wz, xy, yz, zx);
    }

    public float magnitude { get => Sqrt(A * A + b.magnitude); }

    public Rotor4 normalized { get => this * (1f / magnitude); }

    public Rotor4 zero { get => new Rotor4(0f, Bivector4.zero); }

    // constructs and applies a rotor
    public static Vector4 Rotate(Vector4 v1, Vector4 v2, Vector4 vFrom, float t = 1f)
    {
        // normalizing
        Vector4 u1 = v1.normalized;
        Vector4 u2 = v2.normalized;
        Vector4 uFrom = vFrom.normalized;

        Rotor4 r = ConstructRotor(u1, u2);
        Rotor4 interpolatedRotor = InterpolateRotor(r, t);
        Vector4 v = ApplyRotor(interpolatedRotor, uFrom) * v2.magnitude;
        return v;
    }

    // interpolates rotor, r must be normalized and t must be between 0 and 1
    static Rotor4 InterpolateRotor(Rotor4 r, float t)
    {
        // rotation angle
        float theta = Acos(r.a) * t;
        float cosT = Cos(theta);
        float sinT = Sin(theta);

        // constructing rotor
        Bivector4 b = r.b.normalized * sinT;
        return new Rotor4(cosT, b);
    }

    // rotates a vector by a rotor
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
    static Vector4 ApplyRotor(Rotor4 r, Vector4 v)
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
        m.w = r.a * v.w + r.wx * v.x - r.yw * v.y + r.wz * v.z;
        m.x = -r.wx * v.w + r.a * v.x + r.xy * v.y - r.zx * v.z;
        m.y = r.yw * v.w - r.xy * v.x + r.a * v.y + r.yz * v.z;
        m.z = -r.wz * v.w + r.zx * v.x - r.yz * v.y + r.a * v.z;
        float qv = tv.w * v.w + tv.x * v.x + tv.y * v.y + tv.z * v.z;

        /* |  a   -wx   yw   wz  |
         * |  wx   a   -xy   zx  |
         * | -yw   xy   a   -yz  |
         * |  wz  -zx   yz   a   |
         * |  xyz -yzw -zwx  wxy |
         */
        Vector4 n;
        n.w = m.w * r.a + m.x * r.wx - m.y * r.yw + m.z * r.wz + qv * tv.w;
        n.x = -m.w * r.wx + m.x * r.a + m.y * r.xy - m.z * r.zx - qv * tv.x;
        n.y = m.w * r.yw - m.x * r.xy + m.y * r.a + m.z * r.yz - qv * tv.y;
        n.z = -m.w * r.wz + m.x * r.zx - m.y * r.yz + m.z * r.a + qv * tv.z;
        return n;
    }

    // constructs a Rotor to rotate from one vector to another
    static Rotor4 ConstructRotor(Vector4 vFrom, Vector4 vTo)
    {
        float a = 1f + Vector4.Dot(vTo, vFrom);

        // left rotation is b * a so flip vectors
        Bivector4 b = Bivector4.Wedge(vTo, vFrom);

        return new Rotor4(a, b).normalized;
    }

    Rotor4 RotorInverse(Rotor4 r) => new Rotor4(r.a, r.b * -1f);
}