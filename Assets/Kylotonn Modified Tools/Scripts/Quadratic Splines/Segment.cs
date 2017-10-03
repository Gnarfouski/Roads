using MathNet.Numerics;
using UnityEngine;
using System.Collections.Generic;
using System;

public class Segment {

    public long id;
    public QuadraticPolynomial centerLine;
    public List<Contact> contacts;
    public Lane parentLane;

    public int selfIndex;
    public int value;
    public Segment predecessor;

    public Segment(long id, QuadraticPolynomial qp, Lane l)
    {
        this.id = id;
        centerLine = qp;
        contacts = new List<Contact>();
        parentLane = l;
    }

    // used by agent to set vectors of wheel drive
    public Tuple<Vector3, Vector3> GetDirectionVectors(Vector3 pos, float root)
    {
        //if (root < 0 || root > 1) return null;
        //root = Mathf.Clamp01(root);

        Vector3 tangentBound = centerLine.CalculateOuterFirstDerivative(root);
        Vector3 offsetVector = Vector3.Cross(Vector3.up, tangentBound).normalized * -parentLane.offset;
        Vector3 splineBound = (centerLine.CalculateOuter(root) + offsetVector) - pos;

        return new Tuple<Vector3, Vector3>(tangentBound, splineBound);
    }

    public float GetOffsetDistance(Vector3 pos)
    {
        float root = GetClosestRoot(pos);

        if (root <= 0 | root >= 1) return float.PositiveInfinity;

        Vector3 offsetVector = Vector3.Cross(centerLine.CalculateFirstDerivative(root), Vector3.up).normalized * parentLane.offset;
        //Debug.Log(root + " " + centerLine.Calculate(root) + " " + pos);

        return ((centerLine.Calculate(root) + offsetVector) - pos).magnitude;    
    }

    public float GetClosestRoot(Vector3 pos)
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

            if (roots.Item1 != double.NaN)
            {
                if (roots.Item1 < 0) possibilities.Add(0);
                else if (roots.Item1 > 1) possibilities.Add(1);
                else possibilities.Add((float)roots.Item1);
            }
            if (roots.Item2 != double.NaN)
            {
                if (roots.Item2 < 0) possibilities.Add(0);
                else if (roots.Item2 > 1) possibilities.Add(1);
                else possibilities.Add((float)roots.Item2);
            }
            if (roots.Item3 != double.NaN)
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

    public override string ToString()
    {
        string s = "";
        foreach (Contact c in contacts)
            s += " - " + c.switchT + " " + c.target.id + " - ";
        return "Segment " + id + "\n" + centerLine.ToString() + "\nContacts : " + s;
    }

    public bool CheckPath(Segment segment, int v)
    {
        if(v < value)
        {
            predecessor = segment;
            value = v;
            return true;
        }
        return false;
    }
}
