using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Class designed to house information about the curve between two ways of a road
 * A way's mesh is constructed from the vertices present on the two surrounding dividers.
 */

public class QRoadWayDivider
{
    // The points guiding the ppolynom in space
    List<Vector3> points = new List<Vector3>();

    // The right-hand distance from the center of the road
    // With an even number of ways, the central one will have a shift of 0
    float sidewaysShift;

    // The divider's piecewise polynom and its total time span;
    QuadraticPolynomial[] pPolynom;
    //QuadraticPolynomial[] pPoly;
    float totalT;

    // The (approximated) length of the ppolynom in space
    float totalArcLength;
    float[] arcLength;

    // The number of generated points for the mesh
    List<comparableVertex> vertices;

    public void addPoint(Vector3 p)
    {
        points.Add(p);
    }

    public Vector3[] getPointsArray()
    {
        return points.ToArray();
    }

    public float getSidewaysShift()
    {
        return sidewaysShift;
    }

    public void setSidewaysShift(float s)
    {
        sidewaysShift = s;
    }

    public void setPPolynom(QuadraticPolynomial[] pp)
    {
        pPolynom = pp;
    }

    public comparableVertex getVertice(int n)
    {
        return vertices[n];
    }

    public int getVerticeCount()
    {
        return vertices.Count;
    }

    public List<comparableVertex> getVertices()
    {
        return vertices;
    }

    // A way to order the vertices in relation to their normalized time position in the ppolynom
    public struct comparableVertex
    {
        public float globalPosition;
        public Vector3 coordinates;
    }

    float calculatePPArcLength(QuadraticPolynomial[] polynoms, float approximationVectorsNumber)
    {
        float res = 0;

        for (int i = 0; i < polynoms.Length; i++)
        {
            float step = 1f / approximationVectorsNumber;
            Vector3 last = polynoms[i].Calculate(0);

            for (float j = step; j < 1f; j += step)
            {
                res += (polynoms[i].Calculate(j) - last).magnitude;
                last = polynoms[i].Calculate(j);
            }
        }
        return res;
    }

    public void generateVertices(int approximationVectorsNumber, float vertexSpacing)
    {
        totalArcLength = calculatePPArcLength(pPolynom, approximationVectorsNumber);
        vertices = new List<comparableVertex>();

        totalT = points.Count-1;


        float tIncrement = totalT / (totalArcLength / vertexSpacing);

        // Place the vertices along the dividers with the t increments

        float remainingT = 1;
        float currentT = 0;
        float accumulatedT = 0;
        int currentPoly = 0;

        bool hasEnded = false;

        while (!hasEnded)
        {
            comparableVertex cv = new comparableVertex();
            cv.coordinates = pPolynom[currentPoly].Calculate(currentT);
            cv.globalPosition = accumulatedT / totalT;
            vertices.Add(cv);

            currentT += tIncrement;
            accumulatedT += tIncrement;

            while (currentT > remainingT)
            {
                currentT -= remainingT;
                currentPoly++;
                if (currentPoly >= pPolynom.Length)
                {
                    hasEnded = true;
                    break;
                }
                remainingT = 1;
            }
        }

        comparableVertex cvEnd = new comparableVertex();
        cvEnd.coordinates = pPolynom[pPolynom.Length - 1].Calculate(1);
        cvEnd.globalPosition = 1;
        vertices.Add(cvEnd);

        vertices.Sort((s1, s2) => s1.globalPosition.CompareTo(s2.globalPosition));
    }
}


