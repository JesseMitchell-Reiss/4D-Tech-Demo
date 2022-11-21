using System.Linq;
using UnityEngine;
using System.Collections.Generic;

/*
 * ALL functions in this script will be run on a frame by frame basis at runtime
 * ALL functions thus need to be optimally efficient
 */

// 4 dimensional line data type consisting of 2 vector4 points
public class line
{
    Vector4 point1;
    Vector4 point2;
    float dist;

    // constructor takes inputs and assigns the main variables
    public line(Vector4 a = new Vector4(), Vector4 b = new Vector4())
    {
        point1 = a;
        point2 = b;
        dist = (point1.w - point2.w);
    }

    // gets a point on the line based on the w coordinate of the point
    public bool getPoint(float w, out Vector3 point)
    {
        // check that w bisects the line
        if ((w < point1.w && w > point2.w) || (w > point1.w && w < point2.w))
        {
            // generate point weights based on w
            float point1Offset = (point2.w - w) / dist;
            float point2Offset = (point1.w - w) / dist;

            // weight and subtract each value to get resultant vector3
            point.x = point2Offset * point2.x - point1Offset * point1.x;
            point.y = point2Offset * point2.y - point1Offset * point1.y;
            point.z = point2Offset * point2.z - point1Offset * point1.z;
            
            // return successful point generation from function
            return true;
        }
        // define null vector3 and return false if w does not bisect the line
        point = new Vector3();
        return false;
    }
}

// 4 dimensional tetrahedron data type consisting of four vector4 points
public class tetrahedron
{
    List<line> lines;
    List<Vector4> points;

    // construct tetrahedron based on four vector4 inputs
    public tetrahedron(Vector4 a, Vector4 b, Vector4 c, Vector4 d)
    {
        points = new List<Vector4>(new Vector4[] { a, b, c, d });
    }

    /* make not fail when hitting point */
    // create a cross section if the plane in w bisects the tetrahedron, otherwise return false
    public bool getCrossSection(float w, out List<triangle> tris)
    {
        tris = new List<triangle>(new triangle[0]);

        /* maybe create lines on start and operate on them? */
        // make arrays for either side of plane
        List<Vector4> greaterW = new List<Vector4>(new Vector4[0]);
        List<Vector4> lesserW = new List<Vector4>(new Vector4[0]);
        foreach (Vector4 i in points)
        {
            if (i.w >= w)
            {
                greaterW.Add(i);
            }
            else
            {
                lesserW.Add(i);
            }
        }

        // get all lines in tetrahedron and append to lines array
        lines = new List<line>(new line[0]);
        foreach (Vector4 i in greaterW)
        {
            foreach (Vector4 j in lesserW)
            {
                lines.Add(new line(i, j)); /* append each line that crosses the plane */
            }
        }

        // create vector3 array of points at which lines intersect the w plane
        List<Vector3> linePoints = new List<Vector3>(new Vector3[0]);
        foreach (line i in lines)
        {
            if(i.getPoint(w, out Vector3 tempPoint))
            {
                linePoints.Add(tempPoint);
            }
        }

        // output list of triangular cross section(s) of the tetrahedron
        if (linePoints.Count == 3)
        {
            tris.Add(new triangle(linePoints[0], linePoints[1], linePoints[2]));
            return true; /* only one output triangle is necessary if there are exactly 3 points */
        }

        // if there are more than 3 points, square cross sections need to be converted to triangles
        else if (linePoints.Count == 4)
        {
            // get the indecies of the points that define the hypotenuse
            List<int> hypIndecies = new List<int>(new int[2]);
            float distance = 0; /* longest distance placeholder */
            int iIndex = 0;
            int jIndex = 0;
            foreach (Vector3 i in linePoints)
            {
                foreach (Vector3 j in linePoints)
                {
                    if (Vector3.Distance(i, j) > distance)
                    {
                        distance = Vector3.Distance(i, j);
                        hypIndecies = new List<int>(new int[2] { iIndex, jIndex }); /* add longest line indecies */
                    }
                    jIndex++;
                }
                jIndex = 0;
                iIndex++;
            }
            List<int> nonHypIndecies = new List<int>();

            /* do this better */
            // get non-hypotenuse indeceies
            for (int i = 0; i < 4; i++)
            {
                bool nonHyp = true;
                foreach (int j in hypIndecies)
                {
                    if (i == j)
                    {
                        nonHyp = false;
                        goto hypEnd;
                    }
                }
                if (nonHyp)
                {
                    nonHypIndecies.Add(i);
                }
            hypEnd: continue;
            }

            // Add tris that share both hypotenuse points
            tris.Add(new triangle(linePoints[hypIndecies[0]], linePoints[hypIndecies[1]], linePoints[nonHypIndecies[0]]));
            tris.Add(new triangle(linePoints[hypIndecies[0]], linePoints[hypIndecies[1]], linePoints[nonHypIndecies[1]]));
            return true;
        }

        // cross section failure
        else
            return false;
    }
}

// 3 dimensional triangle data type consisting of 3 vector3 points
public class triangle
{
    List<Vector3> points;

    // construct triangle based on set of 3 vector3 variables in the standard spacial dimensions
    public triangle(Vector3 a = new Vector3(), Vector3 b = new Vector3(), Vector3 c = new Vector3())
    {
        points = new List<Vector3>(new Vector3[] { a, b, c });
    }

    // get vector3 array output
    public List<Vector3> getPoints()
    {
        return points;
    }
}

// mesh data type consisting of multiple tetrahedrons
public class tetrahedronMesh
{
    List<tetrahedron> mesh;

    // constructor takes array of tetrahedrons and adds them to the internal array
    public tetrahedronMesh(List<tetrahedron> tets)
    {
        mesh = tets;
    }

    // function outputs all cross sectional triangles
    public bool getCrossSection(float w, out List<triangle> tris)
    {
        // initialize tris output
        tris = new List<triangle>();

        // bool for any valid sections
        bool validSection = false;

        // create output variable
        List<triangle> outputTris;
        /* multithread */
        // check for valid sections and output if they exist
        foreach (tetrahedron i in mesh)
        {
            // working to here, get cross section of tetrahedron return false
            outputTris = new List<triangle>();
            if (i.getCrossSection(w, out outputTris))
            {
                validSection = true;
                foreach (triangle j in outputTris)
                {
                    tris.Add(j); /* append all triangles from cross section of specific tetrahedron */
                }
            }
        }
        return validSection;
    }
}