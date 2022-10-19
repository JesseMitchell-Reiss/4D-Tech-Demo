using System;
using System.IO;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class DimensionalConversion : MonoBehaviour
{
    tetrahedronMesh mesh4d;
    public string definitionFile;

    // Start is called before the first frame update
    void Start()
    {
        // create output tetrahedrons
        tetrahedron[] tetsOutput;
        // extract tetrahedrons from file and add to mesh
        if(parseModel4D(definitionFile, out tetsOutput))
        {
            mesh4d = new tetrahedronMesh(tetsOutput);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // get cross section and output to mesh
        triangle[] tris;
        mesh4d.getCrossSection(FindObjectOfType<Player>().w, out tris);
        int[] triangleArray = new int[0];
        Vector3[] pointArray = new Vector3[0];
        toMesh(tris, out triangleArray, out pointArray);
        Mesh mesh = new Mesh();
        this.GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = pointArray;
        mesh.triangles = triangleArray;
    }
    
    // create tuple of vector arrays for points and tetrahedrons based on file and return
    bool parseModel4D(string file, out tetrahedron[] outTets)
    {
        // initialize output
        outTets = new tetrahedron[0];
        StreamReader reader = File.OpenText(file);
        Vector4[] points = new Vector4[0];
        Vector4[] tetrahedrons = new Vector4[0];
        string line;
        bool passedMiddle = false;
        while ((line = reader.ReadLine()) != null)
        {
            // check for passing middle and skip
            if (line == "Tetrahedrons:")
            {
                passedMiddle = true;
                continue;
            }
            // covert line into vector4 results
            Vector4 results;
            string[] vals = line.Split(",");
            results.w = float.Parse(vals[0]);
            results.x = float.Parse(vals[1]);
            results.y = float.Parse(vals[2]);
            results.z = float.Parse(vals[3]);
            if (!passedMiddle)
            {
                points.Append(results);
            }
            else
            {
                tetrahedrons.Append(results);
            }
        }
        foreach(Vector4 i in tetrahedrons)
        {
            outTets.Append(new tetrahedron(points[(int)Math.Round(i.w)], points[(int)Math.Round(i.x)], points[(int)Math.Round(i.y)], points[(int)Math.Round(i.z)]));
        }
        if (outTets.Length > 0)
            return true;
        else
            return false;
    }
    
    // convert points into one vector and create indecies vector
    void toMesh(triangle[] faces, out int[] triangles, out Vector3[] points)
    {
        triangles = new int[0];
        points = new Vector3[0];
        int iterator = 0;
        foreach(triangle i in faces)
        {
            for(int j = 0; j < 3; j++)
            {
                points.Append(i.getPoints()[j]);
                triangles.Append(iterator);
                iterator++;
            }
        }
    }
}
