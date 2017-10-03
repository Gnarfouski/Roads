using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics;
using System.Xml;
using System.IO;
using System;

public class RoadContainer : Container
{
    public List<Segment> segments;
    public List<Lane> lanes;
    public Dictionary<long, List<Vector3>> anchorPoints;

    //*
    private void Awake()
    {
        segments = new List<Segment>();
        lanes = new List<Lane>();
        anchorPoints = new Dictionary<long, List<Vector3>>();


        XmlDocument xmlDoc = new XmlDocument();

        if (File.Exists(Application.dataPath + "/" + Application.loadedLevelName + "Info.xml"))
        {
            xmlDoc.Load(Application.dataPath + "/" + Application.loadedLevelName + "Info.xml");

            foreach (XmlNode n in xmlDoc.FirstChild)
            {
                List<Segment> tempSegments = new List<Segment>();
                long id = long.Parse(n.Attributes["idr"].Value, System.Globalization.CultureInfo.InvariantCulture);
                int nbLanes = int.Parse(n.Attributes["nblanes"].Value, System.Globalization.CultureInfo.InvariantCulture);
                int nbSegments = int.Parse(n.Attributes["nbsegments"].Value, System.Globalization.CultureInfo.InvariantCulture);

                anchorPoints.Add(id, new List<Vector3>());

                foreach (XmlNode node in n.ChildNodes)
                {
                    if (node.Name.Equals("Lane"))
                    {
                        Lane l = new Lane(
                            long.Parse(node.Attributes["idl"].Value, System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(node.Attributes["offset"].Value, System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(node.Attributes["width"].Value, System.Globalization.CultureInfo.InvariantCulture),
                            bool.Parse(node.Attributes["reverse"].Value));
                        lanes.Add(l);


                        foreach (XmlNode segment in node.ChildNodes)
                        {
                            Vector3 t2 = new Vector3(
                                float.Parse(segment.Attributes["t2x"].Value, System.Globalization.CultureInfo.InvariantCulture),
                                float.Parse(segment.Attributes["t2y"].Value, System.Globalization.CultureInfo.InvariantCulture),
                                float.Parse(segment.Attributes["t2z"].Value, System.Globalization.CultureInfo.InvariantCulture));
                            Vector3 t1 = new Vector3(
                                float.Parse(segment.Attributes["t1x"].Value, System.Globalization.CultureInfo.InvariantCulture),
                                float.Parse(segment.Attributes["t1y"].Value, System.Globalization.CultureInfo.InvariantCulture),
                                float.Parse(segment.Attributes["t1z"].Value, System.Globalization.CultureInfo.InvariantCulture));
                            Vector3 t0 = new Vector3(
                                float.Parse(segment.Attributes["t0x"].Value, System.Globalization.CultureInfo.InvariantCulture),
                                float.Parse(segment.Attributes["t0y"].Value, System.Globalization.CultureInfo.InvariantCulture),
                                float.Parse(segment.Attributes["t0z"].Value, System.Globalization.CultureInfo.InvariantCulture));
                            Vector3[] coeffs = new Vector3[] { t2, t1, t0 };
                            QuadraticPolynomial qp = new QuadraticPolynomial();
                            qp.setCoeffs(coeffs);

                            Segment s = new Segment(
                                long.Parse(segment.Attributes["id"].Value, System.Globalization.CultureInfo.InvariantCulture),
                                qp,
                                l);
                            tempSegments.Add(s);
                        }
                    }
                    else if (node.Name.Equals("Anchors"))
                    {
                        foreach (XmlNode pt in node.ChildNodes)
                        {
                            Vector3 point = new Vector3(
                                float.Parse(pt.Attributes["x"].Value, System.Globalization.CultureInfo.InvariantCulture),
                                float.Parse(pt.Attributes["y"].Value, System.Globalization.CultureInfo.InvariantCulture),
                                float.Parse(pt.Attributes["z"].Value, System.Globalization.CultureInfo.InvariantCulture));
                            anchorPoints[id].Add(point);
                        }
                    }
                }

                for (int i = 0; i < nbLanes; i++)
                {
                    for (int j = 0; j < nbSegments; j++)
                    {
                        /*
                        if(i < nbLanes - 1)
                        {
                            tempSegments[i * nbSegments + j].contacts.Add(new Contact(0.5f,tempSegments[(i + 1) * nbSegments + j]));
                            tempSegments[(i + 1) * nbSegments + j].contacts.Add(new Contact(0.5f,tempSegments[i * nbSegments + j]));
                        }
                        */
                        if (!tempSegments[i * nbSegments + j].parentLane.direction && j < nbSegments - 1)
                        {
                            //Debug.Log(tempSegments[i * nbSegments + j].id + " one");
                            tempSegments[i * nbSegments + j].contacts.Add(new Contact(1, tempSegments[i * nbSegments + j + 1]));
                            //tempSegments[i * nbSegments + j + 1].contacts.Add(new Contact(0,tempSegments[i * nbSegments + j]));
                        }

                        if (tempSegments[i * nbSegments + j].parentLane.direction && j > 0)
                        {
                            //Debug.Log(tempSegments[i * nbSegments + j].id + " two");
                            tempSegments[i * nbSegments + j].contacts.Add(new Contact(0, tempSegments[i * nbSegments + j - 1]));
                            //tempSegments[i * nbSegments + j + 1].contacts.Add(new Contact(0,tempSegments[i * nbSegments + j]));
                        }
                    }
                }

                foreach (Segment s in tempSegments)
                {
                    s.selfIndex = segments.Count;
                    segments.Add(s);
                }
            }

            int s1 = -1, s2 = -1, s3 = -1, s4 = -1, s5 = -1, s6 = -1, s7 = -1, s8 = -1, s9 = -1, s10 = -1, s11 = -1, s12 = -1, s13 = -1, s14 = -1, s15 = -1, s16 = -1;
            foreach (Segment s in segments)
            {
                if (s.id == 1173002008) s1 = segments.IndexOf(s);
                if (s.id == 1173003008) s2 = segments.IndexOf(s);
                if (s.id == 1173002003) s5 = segments.IndexOf(s);
                if (s.id == 1173003003) s6 = segments.IndexOf(s);
                if (s.id == 1173002006) s11 = segments.IndexOf(s);
                if (s.id == 1173003006) s12 = segments.IndexOf(s);
                if (s.id == 1173002001) s3 = segments.IndexOf(s);
                if (s.id == 1173003001) s4 = segments.IndexOf(s);
                if (s.id == 1168003001) s7 = segments.IndexOf(s);
                if (s.id == 1168002001) s8 = segments.IndexOf(s);
                if (s.id == 1168002003) s9 = segments.IndexOf(s);
                if (s.id == 1168003003) s10 = segments.IndexOf(s);
                if (s.id == 1175602001) s13 = segments.IndexOf(s);
                if (s.id == 1175603001) s14 = segments.IndexOf(s);
                if (s.id == 1175602007) s15 = segments.IndexOf(s);
                if (s.id == 1175603007) s16 = segments.IndexOf(s);
            }
            segments[s3].contacts.Add(new Contact(0, segments[s1]));
            segments[s2].contacts.Add(new Contact(1, segments[s4]));
            segments[s5].contacts.Add(new Contact(0.1f, segments[s7]));
            segments[s6].contacts.Add(new Contact(0.1f, segments[s7]));
            segments[s11].contacts.Add(new Contact(0.5f, segments[s9]));
            segments[s12].contacts.Add(new Contact(0.5f, segments[s9]));
            segments[s11].contacts.Add(new Contact(0.25f, segments[s14]));
            segments[s12].contacts.Add(new Contact(0.25f, segments[s14]));
            segments[s13].contacts.Add(new Contact(0, segments[s11]));
            segments[s13].contacts.Add(new Contact(0, segments[s12]));
            segments[s8].contacts.Add(new Contact(0, segments[s5]));
            segments[s8].contacts.Add(new Contact(0, segments[s6]));
            segments[s10].contacts.Add(new Contact(1, segments[s11]));
            segments[s10].contacts.Add(new Contact(1, segments[s12]));
            segments[s3].contacts.Add(new Contact(0.75f, segments[s15]));
            segments[s4].contacts.Add(new Contact(0.75f, segments[s15]));
            segments[s16].contacts.Add(new Contact(1, segments[s3]));
            segments[s16].contacts.Add(new Contact(1, segments[s4]));

            //*
            foreach (Segment s in segments)
            {
                if(s.parentLane.id%10 == 2 || s.parentLane.id % 10 == 3)
                    Debug.Log(s.ToString());
            }
            //*/
        }
        else
            Debug.LogError("RoadContainer : No XML file to load.");
    }

    public int GetRandomSegmentIndex()
    {
        Segment s = null;
        int i = -1;

        while(s == null || s.parentLane.id%100 == 1 || s.parentLane.id%100 == 4)
        {
            i = UnityEngine.Random.Range(0, segments.Count);
            s = segments[i];
        }

        return i;
    }

    public float CheckClosestRoot(int segmentID, Vector3 position)
    {
        return segments[segmentID].GetClosestRoot(position);
    }

    public float GetOffset(int segmentID)
    {
        return segments[segmentID].parentLane.offset;
    }

    private void OnDrawGizmos()
    {
        if (segments != null)
        {
            Gizmos.color = Color.white;
            foreach (Segment s in segments)
            {
                Gizmos.DrawSphere(s.centerLine.Calculate(0.1f), 1);
            }
            Gizmos.color = Color.yellow;
            foreach (Segment s in segments)
            {
                Gizmos.DrawSphere(s.centerLine.Calculate(0.5f), 1);
            }
            Gizmos.color = Color.blue;
            foreach (Segment s in segments)
            {
                Gizmos.DrawSphere(s.centerLine.Calculate(0.9f), 1);
            }
        }
    }

    public int FindStartingIndex(Vector3 pos)
    {
        Dictionary<int, float> segmentPossibilities = new Dictionary<int, float>();

        float minDist = float.PositiveInfinity;
        int minIndex = -1;

        foreach (Segment s in segments)
        {
            float root = s.GetClosestRoot(pos);

            if (root > 0 && root < 1)
            {
                float dist = s.GetOffsetDistance(pos);
                if (dist < minDist)
                {
                    minDist = dist;
                    minIndex = s.selfIndex;
                }
            }
        }
        if (minIndex == -1) Debug.LogError("No Segment Found");
        return minIndex;
    }
}
