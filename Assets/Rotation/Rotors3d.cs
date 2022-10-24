// rotating in 3d to test the math in a more intuitive setting
using UnityEngine;

public class Rotors3d : MonoBehaviour
{
    /* defining the parallelogram two vectors form by unit planes
     * coordinates in yz plane, xz plane, and xy plane in that order
     * because they're respectively parallel to the x, y, z axes */
    Vector3 WedgeProduct(Vector3 a, Vector3 b) =>
        new Vector3(a.y * b.z - b.y * a.z, a.x * b.z - b.x * a.z, a.x * b.y - b.x * a.y);

    // gets the vertices of the parallelogram two vectors form in local position
    Vector3[] ParallelogramVectorsToVertices(Vector3 a, Vector3 b) =>
        new Vector3[4] {Vector3.zero, a, b, a + b};
}
