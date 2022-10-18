using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class line
{
    Vector4 point1;
    Vector4 point2;
    float dist;

    public line(Vector4 a = new Vector4(), Vector4 b = new Vector4())
    {
        point1 = a;
        point2 = b;
        dist = (point1.w - point2.w);
    }

    public bool getPoint(float w, out Vector3 point)
    {
        if((w <= point1.w && w >= point2.w) || (w >= point1.w && w <= point2.w))
        {
            // owen can shut up about it, my way is better and i will not accept pull requests for anything else
            float point1Offset = (point2.w - w) / dist;
            float point2Offset = (point1.w - w) / dist;
            point.x = point2Offset * point2.x - point1Offset * point1.x;
            point.y = point2Offset * point2.y - point1Offset * point1.y;
            point.z = point2Offset * point2.z - point1Offset * point1.z;
            return true;
        }
        point = new Vector3();
        return false;
    }
}

public class tetrahedron {
    Vector4[] points;

    public tetrahedron(Vector4 a, Vector4 b, Vector4 c, Vector4 d)
    {
        points = new Vector4[4] { a, b, c, d };
    }

    public bool getCrossSection(float w, out triangle[] tris)
    {
        // set value of tris
        tris = new triangle[0];
        // get greater and lesser points
        Vector4[] greaterW = new Vector4[0];
        Vector4[] lesserW = new Vector4[0];
        foreach(Vector4 i in points)
        {
            if(i.w >= w)
            {
                greaterW.Append(i);
            }
            else
            {
                lesserW.Append(i);
            }
        }
        // get lines
        line[] lines = new line[0];
        foreach(Vector4 i in greaterW)
        {
            foreach(Vector4 j in lesserW)
            {
                lines.Append(new line(i, j));
            }
        }
        // get points
        Vector3[] linePoints = new Vector3[0];
        foreach(line i in lines)
        {
            i.getPoint(w, out Vector3 tempPoint);
            linePoints.Append(tempPoint);
        }
        // detect whether there will be 1 or 2 triangles
        if(linePoints.Length == 3)
        {
            tris.Append(new triangle(linePoints[0], linePoints[1], linePoints[2]));
            return true;
        }
        else if(linePoints.Length == 4)
        {
            // detect longest line for hypotenuse and get end indecies
            int[] hypIndecies = new int[2];
            float distance = 0;
            int iIndex = 0;
            int jIndex = 0;
            foreach(Vector3 i in linePoints)
            {
                foreach (Vector3 j in linePoints)
                {
                    if(Vector3.Distance(i,j) > distance)
                    {
                        hypIndecies = new int[2] { iIndex, jIndex };
                    }
                    jIndex++;
                }
                jIndex = 0;
                iIndex++;
            }
            int[] nonHypIndecies = new int[0];
            // get non-hypotenuse indeceies
            for(int i = 0; i < 4; i++)
            {
                bool nonHyp = true;
                foreach(int j in hypIndecies)
                {
                    if(i==j)
                    {
                        nonHyp = false;
                    }
                }
                if (nonHyp)
                {
                    nonHypIndecies.Append(i);
                }
            }
            // append tris that share hypotenuse
            tris.Append(new triangle(linePoints[hypIndecies[0]], linePoints[hypIndecies[1]], linePoints[nonHypIndecies[0]]));
            tris.Append(new triangle(linePoints[hypIndecies[0]], linePoints[hypIndecies[1]], linePoints[nonHypIndecies[1]]));
            return true;
        }
        else
            return false;
    }
}

public class triangle {
    public Vector3[] points;
    public triangle(Vector3 a = new Vector3(), Vector3 b = new Vector3(), Vector3 c = new Vector3())
    {
        points = new Vector3[] { a, b, c };
    }
}
