// rotating 
using UnityEngine;

public class Rotors3d : MonoBehaviour
{
    Vector3[] ParallelogramVectorsToVertices (
        Vector3 a, Vector3 b
    ) {
        return new Vector3[4]
        {
            Vector3.zero, a, b, a + b
        };
    }
}
