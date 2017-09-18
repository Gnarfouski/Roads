using System;
using UnityEngine;

/*
 * Polynomial function generation in 3D space with the Catmull-Rom algorithm
 */

public class CatmullRom
{
    // The four points used for computation
    Vector3[] points;

    // The knots controlling the curve shape
    float[] knotVector = new float[4];

    // knot type parameter
    // 0   : uniform (standard) spline
    // 0.5 : centripetal spline
    // 1   : chordal spline
    float alpha = 0.5f;

    // Polynoms storing different levels of the curve
    Vector3Poly[] pAs = new Vector3Poly[3];
    Vector3Poly[] pBs = new Vector3Poly[2];
    Vector3Poly pC;

    public Vector3Poly GetPolynom(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float a)
    {
        if (a < 0) alpha = 0;
        else if (a > 1) alpha = 1;
        else alpha = a;

        points = new Vector3[] { p1, p2, p3, p4 };
        Catmull_Rom();
        return pC;
    }

    public Vector3Poly GetPolynom(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        alpha = 0.5f;

        points = new Vector3[] { p1, p2, p3, p4 };
        Catmull_Rom();
        return pC;
    }

    // Calculate the C polynom and set its knot domain
    void Catmull_Rom()
    {
        knotVector[0] = 0;
        knotVector[1] = GetT(knotVector[0], points[0], points[1]);
        knotVector[2] = GetT(knotVector[1], points[1], points[2]);
        knotVector[3] = GetT(knotVector[2], points[2], points[3]);

        pAs[0] = GetA(1);
        pAs[1] = GetA(2);
        pAs[2] = GetA(3);

        pBs[0] = GetB(1);
        pBs[1] = GetB(2);

        pC = GetC();
        pC.setKnots(knotVector[1], knotVector[2]);
    }

    /*
     * First-degree Basis functions for the Catmull Spline
     */
    Vector3Poly GetA(int index)
    {
        Vector3Poly res = new Vector3Poly();
        float div = knotVector[index] - knotVector[index - 1];

        Vector3 first = (points[index-1] * knotVector[index] - points[index]*knotVector[index-1]) / div;
        Vector3 second = (points[index] - points[index-1]) / div;

        res.setCoeff(0, first);
        res.setCoeff(1, second);
        return res;
    }

    /*
     * Second-degree functions
     */
    Vector3Poly GetB(int index)
    {
        Vector3Poly res = new Vector3Poly();
        float div = knotVector[index + 1] - knotVector[index - 1];

        Vector3Poly first = (pAs[index-1] * knotVector[index + 1] - pAs[index] * knotVector[index - 1]) / div;
        Vector3Poly second = (pAs[index] - pAs[index-1]) / div;

        res.setCoeffs(new Vector3[] {
            first.getCoeffs()[0],
            first.getCoeffs()[1] + second.getCoeffs()[0],
            second.getCoeffs()[1],
            Vector3.zero
        });
        return res;
    }

    /*
     * Third-degree 3D polynom defined between the second and third points
     */
    Vector3Poly GetC()
    {
        Vector3Poly res = new Vector3Poly();
        float div = knotVector[2] - knotVector[1];

        Vector3Poly first = (pBs[0] * knotVector[2] - pBs[1] * knotVector[1]) / div;
        Vector3Poly second = (pBs[1] - pBs[0]) / div;

        res.setCoeffs(new Vector3[] {
            first.getCoeffs()[0],
            first.getCoeffs()[1] + second.getCoeffs()[0],
            first.getCoeffs()[2] + second.getCoeffs()[1],
            second.getCoeffs()[2] });
        return res;
    }

    /*
     * Classic formula for distance between points for knot vector building
     */
    float GetT(float t, Vector3 p0, Vector3 p1)
    {
        float a = Mathf.Pow((p1.x - p0.x), 2.0f) + Mathf.Pow((p1.y - p0.y), 2.0f) + Mathf.Pow((p1.z - p0.z), 2.0f);
        float b = Mathf.Pow(a, 0.5f);
        float c = Mathf.Pow(b, alpha);

        return (c + t);
    }
}
