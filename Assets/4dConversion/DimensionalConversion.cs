using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DimensionalConversion : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Tuple<Vector4[], Vector4[]> parseModel4D(string file)
    {
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
            // concert line into vector4 results
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
        return Tuple.Create<Vector4[], Vector4[]>(points, tetrahedrons);
    }
}
