using System;
using UnityEngine;

/*
 * Class designed to house a Vector3 3rd degree polynomial for the Catmull-Rom calculation.
 */

public class Vector3Poly
{
    // Coefficients of 3D curve and its first derivative (tangent vectors)
    Vector3[] coeffs;
    Vector3[] firstDerivative;

    // time definition of the curve. 
    float firstKnot, lastKnot;

    public Vector3Poly()
    {
        coeffs = new Vector3[4];
        firstDerivative = new Vector3[3];

        for(int i = 0; i < 4; i++)
        {
            coeffs[i] = Vector3.zero;
        }
        for (int i = 0; i < 3; i++)
        {
            firstDerivative[i] = Vector3.zero;
        }
    }

    public Vector3[] getCoeffs()
    {
        return coeffs;
    }

    public Vector3[] getFirstDerivative()
    {
        return firstDerivative;
    }

    public float getFirstKnot()
    {
        return firstKnot;
    }

    public float getLastKnot()
    {
        return lastKnot;
    }

    public float getKnotRange()
    {
        return lastKnot - firstKnot;
    }

    // The knots define between which t (time) values is the 3D curve relevant
    public void setKnots(float first, float last)
    {
        firstKnot = first;
        lastKnot = last;
    }

    public void setCoeffs(Vector3[] c)
    {
        if(c.Length != 4)
        {
            Debug.LogError("Assigning size " + c.Length + " coef vector on size 4 polynom");
            return;
        }
        coeffs = c;
        recalculateDerivative();
    }

    public void setCoeff(int position, Vector3 value)
    {
        if (position < 0 || position > 3)
        {
            Debug.LogError("Looking for index " + position + " in a size 4 vector");
            return;
        }
        coeffs[position] = value;
        recalculateDerivative();
    }

    public Vector3 Calculate(float t)
    {
        if(t < firstKnot || t > lastKnot)
        {
            Debug.LogError("Calling the polynom with value out of predefined domain (" + t + ") " + firstKnot + " - " + lastKnot);
            return Vector3.zero;
        }
        return coeffs[0] + coeffs[1] * t + coeffs[2] * (float)Math.Pow(t, 2) + coeffs[3] * (float)Math.Pow(t, 3);
    }

    public Vector3 CalculateFirstDerivative(float t)
    {
        if (t < firstKnot || t > lastKnot)
        {
            Debug.LogError("Calling the polynom derivative with value out of predefined domain (" + t + ") " + firstKnot + " - " + lastKnot);
            return Vector3.zero;
        }
        return firstDerivative[0] + firstDerivative[1] * t + firstDerivative[2] * (float)Math.Pow(t, 2);
    }

    public override string ToString()
    {
        string res = "";
        res += coeffs[0] + " + " + coeffs[1] + "x + " + coeffs[2] + "x2 + " + coeffs[3] + "x3";
        return res;
    }

    private void recalculateDerivative()
    {
        for(int i = 3;i > 0;i--)
        {
            firstDerivative[i - 1] = coeffs[i] * i;
        }
    }

#region Operators

    public static Vector3Poly operator +(Vector3Poly a, Vector3Poly b)
    {
        Vector3Poly res = new Vector3Poly();

        for(int i = 0; i < 4; i++)
        {
            res.setCoeff(i, a.getCoeffs()[i] + b.getCoeffs()[i]);
        }

        return res;
    }

    public static Vector3Poly operator -(Vector3Poly a, Vector3Poly b)
    {
        Vector3Poly res = new Vector3Poly();

        for (int i = 0; i < 4; i++)
        {
            res.setCoeff(i, a.getCoeffs()[i] - b.getCoeffs()[i]);
        }

        return res;
    }

    public static Vector3Poly operator *(Vector3Poly a, float b)
    {
        Vector3Poly res = new Vector3Poly();

        for (int i = 0; i < 4; i++)
        {
            res.setCoeff(i, a.getCoeffs()[i]*b);
        }

        return res;
    }

    public static Vector3Poly operator /(Vector3Poly a, float b)
    {
        Vector3Poly res = new Vector3Poly();

        for (int i = 0; i < 4; i++)
        {
            res.setCoeff(i, a.getCoeffs()[i] / b);
        }

        return res;
    }

#endregion

}



