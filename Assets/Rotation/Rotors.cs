// rotating in 3d to test the math in a more intuitive setting
using UnityEngine;

public class Rotors : MonoBehaviour
{
    [SerializeField]
    float angleX, angleY, angleZ;

    private void Update()
    {
        Debug.Log(Rotate3(transform.localPosition + Vector3.one, angleX, angleY, angleZ));
    }

    // angles are in radians
    Vector3 Rotate3(Vector3 v, float angleX, float angleY, float angleZ)
    {
        Vector4 rotorX = ConstructRotor3(new Vector3(1f, 0f, 0f), angleX);
        Vector4 rotorY = ConstructRotor3(new Vector3(0f, 1f, 0f), angleY);
        Vector4 rotorZ = ConstructRotor3(new Vector3(0f, 0f, 1f), angleZ);

        v = ApplyRotor3(rotorX, v);
        v = ApplyRotor3(rotorY, v);
        v = ApplyRotor3(rotorZ, v);
        return v;
    }

    Vector3 ApplyRotor3(Vector4 r, Vector3 v)
    {
        Vector3 q = Vector3.zero;

        // matrix multiplication (pretend the lines are brackets)
        /* | a   xz  xy | | x |
         * | -xz a   yz | | y |
         * | -xy -yz a  | | z | */

        q[0] =  r[0] * v[0] + r[2] * v[1] + r[3] * v[2];
        q[1] = -r[2] * v[0] + r[0] * v[1] + r[1] * v[2];
        q[2] = -r[3] * v[0] - r[1] * v[1] + r[0] * v[2];

        // trivector part
        float tv = v[0] * r[1] + v[1] * r[2] + v[2] * r[3];

        Vector3 u = Vector3.zero;

        // more matrix multiplication to get the actual rotated vector
        /* | a   yz  xz xy  | | q0 |
         * | -yz a   xy -xz | | q1 |
         * | -xz -xy a  yz  | | q2 |
         *                    | tv | */

        u[0] =  r[0] * q[0] + r[1] * q[1] + r[2] * q[2] + r[3] * tv;
        u[1] = -r[1] * q[0] + r[0] * q[1] + r[3] * q[2] - r[2] * tv;
        u[2] = -r[2] * q[0] - r[3] * q[1] + r[0] * q[2] + r[1] * tv;

        return u;
    }

    // plane coord order: yz, xz, xy (perpendicular to x, y, z axes respectively)
    // olane must be normalized, angle must be in radians
    Vector4 ConstructRotor3(Vector3 plane, float angle)
    {
        Vector4 rotor = Vector4.zero;

        // rotation will be double the angle given to it
        rotor[0] = Mathf.Cos(angle / 2f);

        float sinAngle = Mathf.Sin(angle / 2f);
        rotor[1] = -sinAngle * plane[0];
        rotor[2] = -sinAngle * plane[1];
        rotor[3] = -sinAngle * plane[2];

        return rotor;
    }
}
