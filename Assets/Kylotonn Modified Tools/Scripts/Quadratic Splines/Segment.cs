using MathNet.Numerics;
using UnityEngine;
using System.Collections.Generic;

public class Segment {

    int id;
    QuadraticPolynomial centerLine;
    float offset;

    public Segment(int id, QuadraticPolynomial qp, float offset)
    {
        this.id = id;
        centerLine = qp;
        this.offset = offset;
    }

    public Vector3 GetDirectionVector(Vector3 pos)
    {
        float root = GetClosestRoot(pos);
        if (root < 0 || root > 1) return Vector3.zero;

        Vector3 tangentBound = centerLine.CalculateFirstDerivative(root);
        Vector3 offsetVector = Vector3.Cross(Vector3.up, tangentBound).normalized * offset;
        Vector3 splineBound = pos - (centerLine.Calculate(root) + offsetVector);
        float alpha = GetDirectionVectorPonderation(splineBound.magnitude);

        return alpha * -splineBound + (1 - alpha) * tangentBound;
    }

    private float GetDirectionVectorPonderation(float dist)
    {
        //Debug.Log(dist);
        if (dist > 5) return 1;
        else if (dist < 0.25) return 0;
        else return (dist / 5)*0.8f + 0.2f;
    }

    private float GetClosestRoot(Vector3 pos)
    {
        if (centerLine.getCoeffs()[0] != Vector3.zero)
        {
            float[] distance = new float[5];
            double[] deriv = new double[4];

            distance[0] =
                Mathf.Pow(centerLine.getCoeffs()[0].x, 2) +
                Mathf.Pow(centerLine.getCoeffs()[0].y, 2) +
                Mathf.Pow(centerLine.getCoeffs()[0].z, 2);
            distance[1] =
                2 * centerLine.getCoeffs()[0].x * centerLine.getCoeffs()[1].x +
                2 * centerLine.getCoeffs()[0].y * centerLine.getCoeffs()[1].y +
                2 * centerLine.getCoeffs()[0].z * centerLine.getCoeffs()[1].z;
            distance[2] =
                Mathf.Pow(centerLine.getCoeffs()[1].x, 2) + 2 * centerLine.getCoeffs()[0].x * (centerLine.getCoeffs()[2] - pos).x +
                Mathf.Pow(centerLine.getCoeffs()[1].y, 2) + 2 * centerLine.getCoeffs()[0].y * (centerLine.getCoeffs()[2] - pos).y +
                Mathf.Pow(centerLine.getCoeffs()[1].z, 2) + 2 * centerLine.getCoeffs()[0].z * (centerLine.getCoeffs()[2] - pos).z;
            distance[3] =
                2 * centerLine.getCoeffs()[1].x * (centerLine.getCoeffs()[2] - pos).x +
                2 * centerLine.getCoeffs()[1].y * (centerLine.getCoeffs()[2] - pos).y +
                2 * centerLine.getCoeffs()[1].z * (centerLine.getCoeffs()[2] - pos).z;
            distance[4] =
                Mathf.Pow((centerLine.getCoeffs()[2] - pos).x, 2) +
                Mathf.Pow((centerLine.getCoeffs()[2] - pos).y, 2) +
                Mathf.Pow((centerLine.getCoeffs()[2] - pos).z, 2);


            deriv[0] = 1;                                       //t3
            deriv[1] = (3 * distance[1]) / (4 * distance[0]);   //t2
            deriv[2] = (2 * distance[2]) / (4 * distance[0]);   //t
            deriv[3] = (distance[3]) / (4 * distance[0]);       //c

            Tuple<double, double, double> roots = MathNet.Numerics.RootFinding.Cubic.RealRoots(deriv[3], deriv[2], deriv[1]);

            List<float> possibilities = new List<float>();

            if (roots.Item1 != double.NaN && roots.Item1 >= 0 && roots.Item1 < 1)
            {
                if (roots.Item1 < 0) possibilities.Add(0);
                else if (roots.Item1 > 1) possibilities.Add(1);
                else possibilities.Add((float)roots.Item1);
            }
            if (roots.Item2 != double.NaN && roots.Item2 >= 0 && roots.Item2 < 1)
            {
                if (roots.Item2 < 0) possibilities.Add(0);
                else if (roots.Item2 > 1) possibilities.Add(1);
                else possibilities.Add((float)roots.Item2);
            }
            if (roots.Item3 != double.NaN && roots.Item3 >= 0 && roots.Item3 < 1)
            {
                if (roots.Item3 < 0) possibilities.Add(0);
                else if (roots.Item3 > 1) possibilities.Add(1);
                else possibilities.Add((float)roots.Item3);
            }

            int minIndex = -1;
            float minValue = float.PositiveInfinity;

            for (int j = 0; j < possibilities.Count; j++)
            {
                float t = possibilities[j];
                if ((centerLine.Calculate(t) - pos).magnitude < minValue)
                {
                    minIndex = j;
                    minValue = (centerLine.Calculate(t) - pos).magnitude;
                }

            }

            if (minIndex == -1) return -1;
            return possibilities[minIndex];
        }
        else
        {
            return -1 *
                (2 * centerLine.getCoeffs()[1].x * (centerLine.getCoeffs()[2] - pos).x +
                2 * centerLine.getCoeffs()[1].y * (centerLine.getCoeffs()[2] - pos).y +
                2 * centerLine.getCoeffs()[1].z * (centerLine.getCoeffs()[2] - pos).z)
                / (2 * 
                (Mathf.Pow(centerLine.getCoeffs()[1].x, 2) +
                Mathf.Pow(centerLine.getCoeffs()[1].y, 2) +
                Mathf.Pow(centerLine.getCoeffs()[1].z, 2)));
        }
    }
}
