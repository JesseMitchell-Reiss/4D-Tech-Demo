// rotating in 3d to test the math in a more intuitive setting
using System;
using UnityEngine;

public class Rotors : MonoBehaviour
{
    [SerializeField]
    float angle;

    [SerializeField]
    Bivector plane;

    private void Update()
    {
        Rotor r = ConstructRotor(angle, plane);
        Debug.Log(ApplyRotor(r, Vector3.right));
    }

    // dot wedge product
    struct Rotor
    {
        public float a;
        public float yz, xz, xy;

        public Rotor(float dot, Bivector bivector)
        {
            a = dot;
            yz = bivector.yz;
            xz = bivector.xz;
            xy = bivector.xy;
        }
    }

    [Serializable]
    struct Bivector
    {
        public float yz, xz, xy;

        public Bivector(float yzPlane, float xzPlane, float xyPlane)
        {
            yz = yzPlane;
            xz = xzPlane;
            xy = xyPlane;
        }
    }

    Vector3 ApplyRotor(Rotor r, Vector3 v)
    {
        // matrix multiplication
        Vector3 p = Vector3.zero;
        p[0] =  r.a  * v.x + r.xy * v.y + r.xz * v.z;
        p[1] = -r.xy * v.x + r.a  * v.y + r.yz * v.z;
        p[2] = -r.xz * v.x - r.yz * v.y + r.a  * v.z;

        // trivector part (represents volume of parallelepiped)
        float p012 = r.yz * v.x - r.xz * v.y + r.xy * v.z;

        // more matrix multiplication
        Vector3 q = Vector3.zero;
        q[0] =  r.a  * p[0] + r.xy * p[1] + r.xz * p[2] + r.yz * p012;
        q[1] = -r.xy * p[0] + r.a  * p[1] + r.yz * p[2] - r.xz * p012;
        q[2] = -r.xz * p[0] - r.yz * p[1] + r.a  * p[2] + r.xy * p012;

        return q;
    }

    // rotating in a plane however many radians
    Rotor ConstructRotor(float radians, Bivector plane)
    {
        // normalizing plane so you don't have to worry about it
        float planeMag = Mathf.Sqrt(
            plane.yz * plane.yz + plane.xz * plane.xz + plane.xy * plane.xy
        );
        plane.yz /= planeMag;
        plane.xz /= planeMag;
        plane.xy /= planeMag;

        float sin = Mathf.Sin(radians / 2f);
        float cos = Mathf.Cos(radians / 2f);
        Bivector b = new Bivector(-sin * plane.yz, -sin * plane.xz, -sin * plane.xy);

        return new Rotor(cos, b);
    }
}
