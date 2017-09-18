using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Class designed to house information about the curve between two ways of a road
 * A way's mesh is constructed from the vertices present on the two surrounding dividers.
 */

public class RoadWayDivider
{
    // The points guiding the ppolynom in space
    List<Vector3> points = new List<Vector3>();

    // The right-hand distance from the center of the road
    // With an even number of ways, the central one will have a shift of 0
    float sidewaysShift;

    // The divider's piecewise polynom and its total time span;
    Vector3Poly[] pPolynom;
    //QuadraticPolynomial[] pPoly;
    float totalT;

    // The (approximated) length of the ppolynom in space
    float arcLength;

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

    public void setPPolynom(Vector3Poly[] pp)
    {
        pPolynom = pp;
    }

    /*
    public void setPPoly(QuadraticPolynomial[] pp)
    {
        pPoly = pp;
    }
    */

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

    float calculatePPArcLength(Vector3Poly[] polynoms, int approximationVectorsNumber)
    {
        float res = 0;

        for (int i = 0; i < polynoms.Length; i++)
        {
            float step = (polynoms[i].getLastKnot() - polynoms[i].getFirstKnot()) / approximationVectorsNumber;
            Vector3 last = polynoms[i].Calculate(polynoms[i].getFirstKnot());

            for (float j = polynoms[i].getFirstKnot() + step; j < polynoms[i].getLastKnot(); j += step)
            {
                res += (polynoms[i].Calculate(j) - last).magnitude;
                last = polynoms[i].Calculate(j);
            }
        }
        return res;
    }

    public void generateVertices(int approximationVectorsNumber, float vertexSpacing)
    {
        arcLength = calculatePPArcLength(pPolynom, approximationVectorsNumber);
        vertices = new List<comparableVertex>();

        totalT = 0;
        //totalT = points.Count - 1;

        for (int i = 0; i < points.Count - 1; i++)
        {
            totalT += pPolynom[i].getKnotRange();
        }

        float tIncrement = totalT / (arcLength / vertexSpacing);

        // Place the vertices along the dividers with the t increments

        float remainingT = pPolynom[0].getKnotRange();
        float currentT = 0;
        float accumulatedT = 0;
        int currentPoly = 0;

        bool hasEnded = false;

        float tim2 = Time.time;

        while (!hasEnded && Time.time - tim2 < 10)
        {
            comparableVertex cv = new comparableVertex();
            cv.coordinates = pPolynom[currentPoly].Calculate(pPolynom[currentPoly].getFirstKnot() + currentT);
            cv.globalPosition = accumulatedT / totalT;
            vertices.Add(cv);

            currentT += tIncrement;
            accumulatedT += tIncrement;

            while (currentT > remainingT && Time.time - tim2 < 10)
            {
                currentT -= remainingT;
                currentPoly++;
                if (currentPoly >= pPolynom.Length)
                {
                    hasEnded = true;
                    break;
                }
                remainingT = pPolynom[currentPoly].getKnotRange();
            }
        }

        comparableVertex cvEnd = new comparableVertex();
        cvEnd.coordinates = pPolynom[pPolynom.Length - 1].Calculate(pPolynom[pPolynom.Length - 1].getLastKnot());
        //cvEnd.coordinates = pPoly[pPoly.Length - 1].Calculate(1);
        cvEnd.globalPosition = 1;
        vertices.Add(cvEnd);

        vertices.Sort((s1, s2) => s1.globalPosition.CompareTo(s2.globalPosition));
    }
}


