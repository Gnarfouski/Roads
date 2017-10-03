using System;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#pragma warning disable 219

public class QRoadMeshGeneration
{
    public QuadraticPolynomial[] GetQPP(Vector3[] points)
    {
        QuadraticPolynomial[] res = new QuadraticPolynomial[points.Length - 1];
        res = PathQuadraticPiecewisePolynom(points);
        return res;
    }

    private XmlDocument CreateCoreXMLDocument()
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlNode rootNode = xmlDoc.CreateElement("Roads");
        xmlDoc.AppendChild(rootNode);

        XmlAttribute creationDate = xmlDoc.CreateAttribute("date");
        creationDate.Value = DateTime.Now.ToShortDateString();
        rootNode.Attributes.Append(creationDate);

        return xmlDoc;
    }

    private XmlNode CreateRoadMeshNode(XmlDocument core, string name)
    {
        XmlNode mainNode = core.CreateElement("RM" + name);

        XmlAttribute aidr = core.CreateAttribute("idr");
        aidr.Value = name;
        mainNode.Attributes.Append(aidr);

        return mainNode;
    }

    private XmlNode CreateLaneNode(XmlDocument core, long id, float offset, float width, bool reverse)
    {
        XmlNode laneNode = core.CreateElement("Lane");

        XmlAttribute aidl = core.CreateAttribute("idl");
        XmlAttribute aoffset = core.CreateAttribute("offset");
        XmlAttribute awidth = core.CreateAttribute("width");
        XmlAttribute adirection = core.CreateAttribute("reverse");

        aidl.Value = id.ToString();
        aoffset.Value = offset.ToString();
        awidth.Value = width.ToString();
        adirection.Value = reverse.ToString();

        laneNode.Attributes.Append(aidl);
        laneNode.Attributes.Append(aoffset);
        laneNode.Attributes.Append(awidth);
        laneNode.Attributes.Append(adirection);

        return laneNode;
    }

    private XmlNode CreateSegmentNode(XmlDocument core, long id, Vector3[] coeffs)
    {
        XmlNode segmentNode = core.CreateElement("Segment");

        XmlAttribute at2x = core.CreateAttribute("t2x");
        XmlAttribute at2y = core.CreateAttribute("t2y");
        XmlAttribute at2z = core.CreateAttribute("t2z");
        XmlAttribute at1x = core.CreateAttribute("t1x");
        XmlAttribute at1y = core.CreateAttribute("t1y");
        XmlAttribute at1z = core.CreateAttribute("t1z");
        XmlAttribute at0x = core.CreateAttribute("t0x");
        XmlAttribute at0y = core.CreateAttribute("t0y");
        XmlAttribute at0z = core.CreateAttribute("t0z");
        XmlAttribute aid = core.CreateAttribute("id");

        at2x.Value = coeffs[0].x.ToString();
        at2y.Value = coeffs[0].y.ToString();
        at2z.Value = coeffs[0].z.ToString();
        at1x.Value = coeffs[1].x.ToString();
        at1y.Value = coeffs[1].y.ToString();
        at1z.Value = coeffs[1].z.ToString();
        at0x.Value = coeffs[2].x.ToString();
        at0y.Value = coeffs[2].y.ToString();
        at0z.Value = coeffs[2].z.ToString();
        aid.Value = id.ToString();

        segmentNode.Attributes.Append(at2x);
        segmentNode.Attributes.Append(at2y);
        segmentNode.Attributes.Append(at2z);
        segmentNode.Attributes.Append(at1x);
        segmentNode.Attributes.Append(at1y);
        segmentNode.Attributes.Append(at1z);
        segmentNode.Attributes.Append(at0x);
        segmentNode.Attributes.Append(at0y);
        segmentNode.Attributes.Append(at0z);
        segmentNode.Attributes.Append(aid);

        return segmentNode;
    }

    private XmlNode CreateLinkBetweenNode(XmlDocument core, long originID, long targetID)
    {
        XmlNode linkNode = core.CreateElement("Link");

        XmlAttribute aorigin = core.CreateAttribute("origin");
        XmlAttribute atarget = core.CreateAttribute("target");

        aorigin.Value = originID.ToString();
        atarget.Value = targetID.ToString();

        linkNode.Attributes.Append(aorigin);
        linkNode.Attributes.Append(atarget);

        return linkNode;
    }

    public Mesh QGenerateMesh(Vector3[] realPoints, Vector3[] points, float[] wayWidths, float vertexSpacing, bool[] directions, string roadMeshName)
    {
        if (points == null || points.Length < 2) return null;

        if (vertexSpacing < 1)
        {
            vertexSpacing = 1;
            Debug.LogWarning("vertex spacing set to 1");
        }

        float totalRoadWidth = 0;

        foreach (float f in wayWidths)
        {
            totalRoadWidth += f;
        }

        List<RoadWayDivider> wayDividers = new List<RoadWayDivider>();

        for (int i = 0; i < wayWidths.Length + 1; i++)
        {
            wayDividers.Add(new RoadWayDivider());
        }

        // Set the sideways distance of way dividers

        float tempWidth = 0;

        wayDividers[0].setSidewaysShift(totalRoadWidth / 2);

        for (int i = 1; i < wayDividers.Count; i++)
        {
            tempWidth = wayWidths[i - 1];
            wayDividers[i].setSidewaysShift(wayDividers[i - 1].getSidewaysShift() - tempWidth);
        }

        // Get the central (directive) road piecewise polynom and create the way dividers point lists

        QuadraticPolynomial[] centralPPolynom = PathQuadraticPiecewisePolynom(points);
        QuadraticPolynomial[] realCentralPPolynom = PathQuadraticPiecewisePolynom(realPoints);


        XmlDocument xmlDoc = new XmlDocument();
        //Debug.Log(Application.loadedLevelName);
        if (File.Exists(Application.dataPath + "/" + Application.loadedLevelName + "Info.xml"))
            xmlDoc.Load(Application.dataPath + "/" + Application.loadedLevelName + "Info.xml");
        else
            xmlDoc = CreateCoreXMLDocument();

        XmlNode toReplace = null;

        foreach (XmlNode n in xmlDoc.FirstChild.ChildNodes)
        {
            if (n.Name.Equals("RM" + roadMeshName)) toReplace = n;
        }

        XmlNode main = CreateRoadMeshNode(xmlDoc, roadMeshName);
        long baseId = long.Parse(roadMeshName);

        XmlAttribute anblanes = xmlDoc.CreateAttribute("nblanes");
        XmlAttribute anbsegments = xmlDoc.CreateAttribute("nbsegments");

        anblanes.Value = (wayDividers.Count - 1).ToString();
        anbsegments.Value = (realCentralPPolynom.Length).ToString();

        main.Attributes.Append(anblanes);
        main.Attributes.Append(anbsegments);

        if (toReplace != null)
            xmlDoc.FirstChild.ReplaceChild(main, toReplace);
        else
            xmlDoc.FirstChild.AppendChild(main);

        XmlNode anchors = xmlDoc.CreateElement("Anchors");
        main.AppendChild(anchors);
        foreach(Vector3 v in realPoints)
        {
            XmlNode pt = xmlDoc.CreateElement("Point");
            anchors.AppendChild(pt);

            XmlAttribute ax = xmlDoc.CreateAttribute("x");
            XmlAttribute ay = xmlDoc.CreateAttribute("y");
            XmlAttribute az = xmlDoc.CreateAttribute("z");

            ax.Value = v.x.ToString();
            ay.Value = v.y.ToString();
            az.Value = v.z.ToString();

            pt.Attributes.Append(ax);
            pt.Attributes.Append(ay);
            pt.Attributes.Append(az);
        }

        for (int j = 0; j < wayDividers.Count - 1; j++)
        {
            XmlNode lane = CreateLaneNode(
                xmlDoc,
                baseId * 100 + j + 1,
                (wayDividers[j].getSidewaysShift() + wayDividers[j + 1].getSidewaysShift()) / 2,
                Mathf.Abs(wayDividers[j].getSidewaysShift() - wayDividers[j + 1].getSidewaysShift()),
                directions[j]);

            main.AppendChild(lane);

            for (int i = 0; i < realCentralPPolynom.Length; i++)
            {
                XmlNode segment = CreateSegmentNode(
                    xmlDoc,
                    (baseId * 100 + j + 1) * 1000 + i + 1,
                    realCentralPPolynom[i].getCoeffs()
                    );
                lane.AppendChild(segment);
            }
        }

        /*
        for (int i = 0; i < centralPPolynom.Length; i++)
        {
            for (int j = 0; j < wayDividers.Count - 1; j++)
            {
                XmlNode contactNode = xmlDoc.CreateElement("Contact");
                xmlDoc.FirstChild.AppendChild(contactNode);

                XmlAttribute aidc = xmlDoc.CreateAttribute("idc");
                XmlAttribute aorigin = xmlDoc.CreateAttribute("o");
                XmlAttribute atarget = xmlDoc.CreateAttribute("t");

                if (i != centralPPolynom.Length - 1)
                {
                    aidc.Value = ((j + 1) * 1000 + i).ToString() + ((j + 1) * 1000 + i + 1).ToString();
                    aorigin.Value = ((j + 1) * 1000 + i).ToString();
                    atarget.Value = ((j + 1) * 1000 + i + 1).ToString();

                    contactNode.Attributes.Append(aidc);
                    contactNode.Attributes.Append(aorigin);
                    contactNode.Attributes.Append(atarget);
                }
                else
                {
                    aidc.Value = ((j + 1) * 1000 + i).ToString() + ((j + 1) * 1000).ToString();
                    aorigin.Value = ((j + 1) * 1000 + i).ToString();
                    atarget.Value = ((j + 1) * 1000).ToString();

                    contactNode.Attributes.Append(aidc);
                    contactNode.Attributes.Append(aorigin);
                    contactNode.Attributes.Append(atarget);
                }
            }
        }
        */

        xmlDoc.Save(Application.dataPath + "/" + Application.loadedLevelName + "Info.xml");

        Vector3[][] mods = new Vector3[centralPPolynom.Length][];
        Vector3[][] newPoints = new Vector3[centralPPolynom.Length][];

        mods[0] = new Vector3[11];
        newPoints[0] = new Vector3[11];

        for (int i = 0; i <= 10; i++)
        {
            mods[0][i] = Vector3.Cross(centralPPolynom[0].CalculateFirstDerivative((float)i/10), Vector3.up);
            mods[0][i].y = 0;
            newPoints[0][i] = centralPPolynom[0].Calculate((float)i/10);
        }

        for (int j = 1; j < centralPPolynom.Length; j++)
        {
            mods[j] = new Vector3[10];
            newPoints[j] = new Vector3[10];
            for (int i = 1; i <= 10; i++)
            {
                mods[j][i - 1] = Vector3.Cross(centralPPolynom[j].CalculateFirstDerivative((float)i / 10), Vector3.up);
                newPoints[j][i - 1] = centralPPolynom[j].Calculate((float)i / 10);
            }
        }

        for (int i = 0; i < mods.Length; i++)
        {
            for (int j = 0; j < mods[i].Length; j++)
            {
                for (int k = 0; k < wayDividers.Count; k++)
                {
                    wayDividers[k].addPoint(newPoints[i][j] + mods[i][j].normalized * -wayDividers[k].getSidewaysShift());
                    //Debug.Log(newPoints[i][j] + " " + mods[i][j].normalized + " " + (-wayDividers[k].getSidewaysShift()));
                    //Debug.Log("new pt at div " + k + " poly " + i + " point " + j + " : " + wayDividers[k].getPointsArray()[wayDividers[k].getPointsArray().Length-1]);
                }
            }
        }

        // Generate divider-wise piecewise polynoms

        for (int i = 0; i < wayDividers.Count; i++)
        {
            wayDividers[i].setPPolynom(PathPiecewisePolynom(wayDividers[i].getPointsArray()));
        }

        // Generate the vertices on each divider depending on arc length approximation and specified vertex spacing

        int approximationVectorsNumber = 10;

        for (int i = 0; i < wayDividers.Count; i++)
        {
            wayDividers[i].generateVertices(approximationVectorsNumber, vertexSpacing);
        }

        List<Mesh> meshes = new List<Mesh>();

        List<List<Vector3>> vertices = new List<List<Vector3>>();
        List<List<Vector2>> uv = new List<List<Vector2>>();
        List<List<Color32>> colors = new List<List<Color32>>();
        List<List<Vector3>> normals = new List<List<Vector3>>();
        List<List<Vector3>> tangents = new List<List<Vector3>>();
        List<List<int>> triangles = new List<List<int>>();

        for (int i = 0; i < wayWidths.Length; ++i)
        {
            meshes.Add(new Mesh());
            vertices.Add(new List<Vector3>());
            uv.Add(new List<Vector2>());
            colors.Add(new List<Color32>());
            normals.Add(new List<Vector3>());
            triangles.Add(new List<int>());
        }

        // For each way, scan the two adjacent dividers and create uv, color and normal for each vertice

        for (int i = 0; i < wayWidths.Length; i++)
        {
            float numberTiles = Math.Max(wayDividers[i].getVerticeCount(), wayDividers[i + 1].getVerticeCount()) / 3;

            /**
             * !!! ^ NEED CONSTRUCTION IN REGARD TO REAL SIZE OF TEXTURE ^ !!!
             */

            int divider1Count = 1;
            int pos1Count = 0;
            int divider2Count = 1;
            int pos2Count = 1;

            // add the right and left vertices at position 0

            vertices[i].Add(wayDividers[i].getVertice(0).coordinates);
            uv[i].Add(i >= wayWidths.Length / 2 ? new Vector2(1, 0) : new Vector2(0, 0));
            colors[i].Add(Color.white);
            normals[i].Add(Vector3.up);

            vertices[i].Add(wayDividers[i + 1].getVertice(0).coordinates);
            uv[i].Add(i >= wayWidths.Length / 2 ? new Vector2(0, 0) : new Vector2(1, 0));
            colors[i].Add(Color.white);
            normals[i].Add(Vector3.up);

            // add the next positionned vertice and its uv value - uv is inverted for the second half of the street

            if (wayDividers[i].getVertice(divider1Count).globalPosition <= wayDividers[i + 1].getVertice(divider2Count).globalPosition)
            {
                vertices[i].Add(wayDividers[i].getVertice(divider1Count).coordinates);
                uv[i].Add(i >= wayWidths.Length / 2 ? 
                    new Vector2(1, wayDividers[i].getVertice(divider1Count).globalPosition * numberTiles) : 
                    new Vector2(0, wayDividers[i].getVertice(divider1Count).globalPosition * numberTiles));

                divider1Count++;
                pos1Count = vertices[i].Count - 1;
            }
            else
            {
                vertices[i].Add(wayDividers[i + 1].getVertice(divider2Count).coordinates);
                uv[i].Add(i >= wayWidths.Length / 2 ? 
                    new Vector2(0, wayDividers[i + 1].getVertice(divider2Count).globalPosition * numberTiles) : 
                    new Vector2(1, wayDividers[i + 1].getVertice(divider2Count).globalPosition * numberTiles));

                divider2Count++;
                pos2Count = vertices[i].Count - 1;
            }

            colors[i].Add(Color.white);
            normals[i].Add(Vector3.up);

            // create triangle with the first three vertices

            if (isUpwardsTriangle(vertices[i][0], vertices[i][1], vertices[i][2]))
            {
                triangles[i].Add(0);
                triangles[i].Add(1);
                triangles[i].Add(2);
            }
            else
            {
                triangles[i].Add(0);
                triangles[i].Add(2);
                triangles[i].Add(1);
            }

            // while one of the dividers has unused vertices

            while (!(divider1Count == wayDividers[i].getVerticeCount() && divider2Count == wayDividers[i + 1].getVerticeCount()))
            {
                // get the last used vertice on each divider
                List<int> tempVertsIndexes = new List<int>();
                tempVertsIndexes.Add(pos1Count);
                tempVertsIndexes.Add(pos2Count);

                // get the vertice with the smallest global score

                float val1 = divider1Count < wayDividers[i].getVerticeCount() ? wayDividers[i].getVertice(divider1Count).globalPosition : float.PositiveInfinity;
                float val2 = divider2Count < wayDividers[i + 1].getVerticeCount() ? wayDividers[i + 1].getVertice(divider2Count).globalPosition : float.PositiveInfinity;

                if (val1 <= val2)
                {
                    vertices[i].Add(wayDividers[i].getVertice(divider1Count).coordinates);
                    uv[i].Add(i >= wayWidths.Length / 2 ? 
                        new Vector2(1, wayDividers[i].getVertice(divider1Count).globalPosition * numberTiles) : 
                        new Vector2(0, wayDividers[i].getVertice(divider1Count).globalPosition * numberTiles));

                    divider1Count++;
                    pos1Count = vertices[i].Count - 1;
                }
                else
                {
                    vertices[i].Add(wayDividers[i + 1].getVertice(divider2Count).coordinates);
                    uv[i].Add(i >= wayWidths.Length / 2 ? 
                        new Vector2(0, wayDividers[i + 1].getVertice(divider2Count).globalPosition * numberTiles) : 
                        new Vector2(1, wayDividers[i + 1].getVertice(divider2Count).globalPosition * numberTiles));

                    divider2Count++;
                    pos2Count = vertices[i].Count - 1;
                }

                colors[i].Add(Color.white);
                normals[i].Add(Vector3.up);

                tempVertsIndexes.Add(vertices[i].Count - 1);

                if (isUpwardsTriangle(vertices[i][tempVertsIndexes[0]], vertices[i][tempVertsIndexes[1]], vertices[i][tempVertsIndexes[2]]))
                {
                    triangles[i].Add(tempVertsIndexes[0]);
                    triangles[i].Add(tempVertsIndexes[1]);
                    triangles[i].Add(tempVertsIndexes[2]);
                }
                else
                {
                    triangles[i].Add(tempVertsIndexes[0]);
                    triangles[i].Add(tempVertsIndexes[2]);
                    triangles[i].Add(tempVertsIndexes[1]);
                }
            }
        }

        int combinedVerts = vertices[0].Count;

        for (int i = 1; i < wayWidths.Length; ++i)
        {
            vertices[0].AddRange(vertices[i]);
            uv[0].AddRange(uv[i]);
            normals[0].AddRange(normals[i]);

            for (int j = 0; j < triangles[i].Count; j++) triangles[i][j] += combinedVerts;
            combinedVerts += vertices[i].Count;
            //Debug.Log("combined verts " + combinedVerts);
        }

        Mesh mesh = new Mesh();

        mesh.vertices = vertices[0].ToArray();
        mesh.uv = uv[0].ToArray();
        mesh.normals = normals[0].ToArray();

        mesh.subMeshCount = wayWidths.Length;
        for (int i = 0; i < wayWidths.Length; ++i)
        {
            mesh.SetTriangles(triangles[i].ToArray(), i);
        }

        return mesh;
        //*/
    }

    QuadraticPolynomial[] PathQuadraticPiecewisePolynom(Vector3[] points)
    {
        QuadraticPolynomial[] res = new QuadraticPolynomial[points.Length - 1];

        res[0] = new QuadraticPolynomial();
        res[0].setCoeff(1, new Vector3(
            points[1].x - points[0].x, 
            points[1].y - points[0].y, 
            points[1].z - points[0].z));
        res[0].setCoeff(2, new Vector3(
            points[0].x, 
            points[0].y, 
            points[0].z));

        
        for (int i = 1; i < points.Length-1; i++)
        {
            Vector3 ai = res[i - 1].getCoeffs()[0];
            Vector3 bi = res[i - 1].getCoeffs()[1];

            res[i] = new QuadraticPolynomial();
            res[i].setCoeff(0, new Vector3(
                points[i + 1].x - points[i].x - 2 * ai.x - bi.x,
                points[i + 1].y - points[i].y - 2 * ai.y - bi.y,
                points[i + 1].z - points[i].z - 2 * ai.z - bi.z));
            res[i].setCoeff(1, new Vector3(
                2 * ai.x + bi.x,
                2 * ai.y + bi.y,
                2 * ai.z + bi.z));
            res[i].setCoeff(2, new Vector3(
                points[i].x, 
                points[i].y, 
                points[i].z));
        }

        return res;
    }

    Vector3Poly[] PathPiecewisePolynom(Vector3[] points)
    {
        Vector3[] modPoints = new Vector3[points.Length + 2];

        Vector3 startDir = (points[0] - points[1]).normalized;
        modPoints[0] = points[0] + startDir;

        Vector3 endDir = (points[points.Length - 1] - points[points.Length - 2]).normalized;
        modPoints[modPoints.Length - 1] = points[points.Length - 1] + endDir;

        for (int i = 0; i < points.Length; i++) modPoints[i + 1] = points[i];

        if (modPoints.Length < 4)
        {
            Debug.LogError("Bad Control Points Vector");
            return null;
        }

        CatmullRom CR = new CatmullRom();
        Vector3Poly[] res = new Vector3Poly[modPoints.Length - 3];

        for (int i = 0; i < modPoints.Length - 3; i++)
        {
            res[i] = CR.GetPolynom(modPoints[i], modPoints[i + 1], modPoints[i + 2], modPoints[i + 3], 1);
        }

        return res;
    }

    public bool isUpwardsTriangle(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        Vector3 center = (p0 + p1 + p2) / 3;
        Vector3 normal = Vector3.Cross(p0 - center, p1 - center).normalized;
        float dot = Vector3.Dot(Vector3.up, normal);
        return dot >= 0;
    }
}
