using System;
using System.IO;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using System.Collections.Generic;

public class DimensionalConversion : MonoBehaviour
{
    tetrahedronMesh mesh4d;
    public string definitionFile;
    [SerializeField]
    Player player;
    float oldW;

    // Start is called before the first frame update
    void Start()
    {
        // create output tetrahedrons
        List<tetrahedron> tetsOutput;
        // extract tetrahedrons from file and add to mesh
        if(parseModel4D(definitionFile, out tetsOutput))
        {
            mesh4d = new tetrahedronMesh(tetsOutput);
        }
        oldW = player.w;
        // mesh4d is correct
    }

    // Update is called once per frame
    void Update()
    {
        if(oldW != player.w)
        {
            // get cross section and output to mesh
            List<triangle> tris;
            // outputs list with some potentially incorrect triangles
            mesh4d.getCrossSection(player.w, out tris);
            int[] triangleArray;
            Vector3[] pointArray;
            toMesh(tris, out triangleArray, out pointArray);
            Mesh mesh = new Mesh();
            this.GetComponent<MeshFilter>().mesh = mesh;
            mesh.vertices = pointArray;
            mesh.triangles = triangleArray;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            oldW = player.w;
        }
    }
    
    // working
    // create tuple of vector arrays for points and tetrahedrons based on file and return
    bool parseModel4D(string file, out List<tetrahedron> outTets)
    {
        // initialize output
        outTets = new List<tetrahedron>(new tetrahedron[0]);
        StreamReader reader = File.OpenText(file);
        List<Vector4> points = new List<Vector4>(new Vector4[0]);
        List<Vector4> tetrahedrons = new List<Vector4>(new Vector4[0]);
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
                points.Add(results);
            }
            else
            {
                tetrahedrons.Add(results);
            }
        }
        foreach(Vector4 i in tetrahedrons)
        {
            outTets.Add(new tetrahedron(points[(int)Math.Round(i.w) - 1], points[(int)Math.Round(i.x) - 1], points[(int)Math.Round(i.y) - 1], points[(int)Math.Round(i.z) - 1]));
        }
        if (outTets.Count > 0)
            return true;
        else
            return false;
    }
    
    // convert points into one vector and create indecies vector
    void toMesh(List<triangle> faces, out int[] triangles, out Vector3[] points)
    {
        triangles = Enumerable.Range(0, faces.Count * 3).ToArray();
        points = new Vector3[faces.Count * 3];
        int iterator = 0;
        foreach(triangle i in faces)
        {
            foreach(Vector3 j in i.getPoints())
            {
                points[iterator] = j;
                iterator++;
            }
        }
    }
}
