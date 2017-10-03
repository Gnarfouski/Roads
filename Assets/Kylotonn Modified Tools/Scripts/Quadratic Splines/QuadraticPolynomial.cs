using System;
using UnityEngine;

/*
 * Class designed to house a Vector3 2nd degree polynomial for quadratic interpolation.
 */

public class QuadraticPolynomial
{
    // Coefficients of 3D curve and its first derivative (tangent vectors)
    Vector3[] coeffs;
    Vector3[] firstDerivative;

    public QuadraticPolynomial()
    {
        coeffs = new Vector3[3];
        firstDerivative = new Vector3[2];

        for(int i = 0; i < 3; i++)
        {
            coeffs[i] = Vector3.zero;
        }
        for (int i = 0; i < 2; i++)
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

    public void setCoeffs(Vector3[] c)
    {
        if(c.Length != 3)
        {
            Debug.LogError("Assigning size " + c.Length + " coef vector on size 3 polynom");
            return;
        }
        coeffs = c;
        recalculateDerivative();
    }

    public void setCoeff(int position, Vector3 value)
    {
        if (position < 0 || position > 2)
        {
            Debug.LogError("Looking for index " + position + " in a size 3 vector");
            return;
        }
        coeffs[position] = value;
        recalculateDerivative();
    }

    public Vector3 Calculate(float t)
    {
        if(t < 0 || t > 1)
        {
            Debug.LogError("Knot Vector for QP is 0 - 1 (" + t + ")");
            return Vector3.zero;
        }
        return coeffs[0] * t * t + coeffs[1] * t + coeffs[2];
    }

    public Vector3 CalculateOuter(float t)
    {
        return coeffs[0] * t * t + coeffs[1] * t + coeffs[2];
    }

    public Vector3 CalculateFirstDerivative(float t)
    {
        if (t < 0 || t > 1)
        {
            Debug.LogError("Knot Vector for QP' is 0 - 1 (" + t + ")");
            return Vector3.zero;
        }
        return firstDerivative[0] * t + firstDerivative[1];
    }

    public Vector3 CalculateOuterFirstDerivative(float t)
    {
        return firstDerivative[0] * t + firstDerivative[1];
    }

    public override string ToString()
    {
        return "QP : " + coeffs[0] + " t2 + " + coeffs[1] + " t + " + coeffs[2];
    }

    private void recalculateDerivative()
    {
        firstDerivative[0] = 2 * coeffs[0];
        firstDerivative[1] = coeffs[1];
    }

#region Operators

    public static QuadraticPolynomial operator +(QuadraticPolynomial a, QuadraticPolynomial b)
    {
        QuadraticPolynomial res = new QuadraticPolynomial();

        for(int i = 0; i < 3; i++)
        {
            res.setCoeff(i, a.getCoeffs()[i] + b.getCoeffs()[i]);
        }

        return res;
    }

    public static QuadraticPolynomial operator -(QuadraticPolynomial a, QuadraticPolynomial b)
    {
        QuadraticPolynomial res = new QuadraticPolynomial();

        for (int i = 0; i < 3; i++)
        {
            res.setCoeff(i, a.getCoeffs()[i] - b.getCoeffs()[i]);
        }

        return res;
    }

    public static QuadraticPolynomial operator *(QuadraticPolynomial a, float b)
    {
        QuadraticPolynomial res = new QuadraticPolynomial();

        for (int i = 0; i < 3; i++)
        {
            res.setCoeff(i, a.getCoeffs()[i]*b);
        }

        return res;
    }

    public static QuadraticPolynomial operator /(QuadraticPolynomial a, float b)
    {
        QuadraticPolynomial res = new QuadraticPolynomial();

        for (int i = 0; i < 3; i++)
        {
            res.setCoeff(i, a.getCoeffs()[i] / b);
        }

        return res;
    }

#endregion

}



