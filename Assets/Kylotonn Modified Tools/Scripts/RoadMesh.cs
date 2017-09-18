using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoadMesh : MonoBehaviour
{
    static public bool s_UsePositionhandle = false;

#region structs and enums

    [System.Serializable]
    public enum EWayType
    {
        None,
        Road,
        Pedestrian,
        Bus,
        Cyclable
    }

    [System.Serializable]
    public struct Road
    {
        public float m_Width;
        public bool m_Reverse;
        public bool m_NotRoad;
        public EWayType m_Type;

        public Material m_Material;
        public bool m_TurnUv;
        public float m_UvLen;
        public Vector2 m_UvRange;

        public Road(Road copy)
        {
            m_Width = copy.m_Width;
            m_Reverse = copy.m_Reverse;
            m_NotRoad = copy.m_NotRoad;
            m_Type = copy.m_Type;
            m_Material = copy.m_Material;
            m_TurnUv = copy.m_TurnUv;
            m_UvLen = copy.m_UvLen;
            m_UvRange = copy.m_UvRange;
        }

        public void CopyTemplate(RoadConfig.RoadTemplate template)
        {
            m_NotRoad = template.m_NotRoad;
            m_Type = template.m_Type;
            m_Width = template.m_Width;
            m_Material = template.m_Material;
            m_TurnUv = template.m_TurnUv;
            m_UvLen = template.m_UvLen;
            m_UvRange = template.m_UvRange;
        }
    }

    [System.Serializable]
    public enum ELineCenter
    {
        Left,
        Middle,
        Right
    }

    [System.Serializable]
    public enum RoadNodeType
    {
        Road,
        Exit,
        RoundAbout,
        None,
    }

    [System.Serializable]
    public class RoadLine
    {
        public bool m_Show = false;
        public ELineCenter m_Center = ELineCenter.Middle;
        public bool m_Triangle = false;
        public float m_Width = 0.1f;
        public float m_Length = 1f;
        public float m_Space = 1f;
        public float m_Offset = 0f;
        public float m_Height = 0f;
        public Color m_Color = Color.white;
        public Material m_Material;
        public Material m_GPSMaterial;

        public string m_RayCloneTemplate;

        public RoadLine() { }

        public RoadLine(RoadLine copy)
        {
            m_Show = copy.m_Show;
            m_Triangle = copy.m_Triangle;
            m_Width = copy.m_Width;
            m_Length = copy.m_Length;
            m_Space = copy.m_Space;
            m_Offset = copy.m_Offset;
            m_Height = copy.m_Height;
            m_Color = copy.m_Color;
            m_Material = copy.m_Material;
            m_GPSMaterial = copy.m_GPSMaterial;
            m_RayCloneTemplate = copy.m_RayCloneTemplate;
        }

        public void CopyTemplate(RoadConfig.RoadLineTemplate template)
        {
            m_Show = template.m_Show;
            m_Width = template.m_Width;
            m_Length = template.m_Length;
            m_Space = template.m_Space;
            m_Height = template.m_Height;
            m_Space = template.m_Space;
            m_Color = template.m_Color;
            m_Material = template.m_Material;
        }
    }

#endregion

    [HideInInspector]
    public Road[] m_Roads = new Road[1];
    [HideInInspector]
    public RoadLine[] m_RoadLines = new RoadLine[2];

    public int m_RoadCount
    {
        get
        {
            return m_Roads.Length;
        }
        set
        {
            int newCount = System.Math.Max(1, value);
            List<Road> newRoads = new List<Road>(m_Roads);
            while (newCount != newRoads.Count)
            {
                if (newCount < newRoads.Count)
                {
                    newRoads.RemoveAt(newRoads.Count - 1);
                }
                else
                {
                    if (newRoads.Count > 0)
                    {
                        newRoads.Insert(newRoads.Count - 1, newRoads[newRoads.Count - 1]);
                    }
                    else
                    {
                        newRoads.Insert(0, new Road());
                    }
                }
            }
            m_Roads = newRoads.ToArray();
            FixRoadLine();
        }
    }

    public int m_UsableRoadCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < m_RoadCount; ++i)
            {
                if (!m_Roads[i].m_NotRoad && m_Roads[i].m_Type != EWayType.Pedestrian)
                {
                    ++count;
                }
            }
            return count;
        }
    }

    public float m_RoadWidth
    {
        get
        {
            float size = 0f;
            for (int i = 0; i < m_RoadCount; ++i)
            {
                size += GetRoadWidth(i);
            }
            return size;
        }
    }

    public float m_SegmentSize = 3f;

    public Vector3[] m_Points = new Vector3[] { Vector3.right, Vector3.right * 2f };

    [SerializeField]
    [HideInInspector]
    private Vector3[] m_SplinePoints;
    [SerializeField]
    [HideInInspector]
    private Vector3[] m_StartPoints;
    [SerializeField]
    [HideInInspector]
    private Vector3[] m_EndPoints;

    [SerializeField]
    [HideInInspector]
    private Vector3[][] m_RoadPoints;
    [SerializeField]
    [HideInInspector]
    private Vector3[][] m_LinePoints;

    private Vector3 m_StartPoint
    {
        get
        {
            if (null != m_Points && m_Points.Length > 0)
            {
                return m_Points[0];
            }
            return Vector3.zero;
        }
    }
    private Vector3 m_EndPoint
    {
        get
        {
            if (null != m_Points && m_Points.Length > 0)
            {
                return m_Points[m_Points.Length - 1];
            }
            return Vector3.zero;
        }
    }

    static List<GameObject> s_ToDestroy = new List<GameObject>();

    public Vector3[] points
    {
        get
        {
            return m_SplinePoints;
        }
    }

    public Vector3 startPoint
    {
        get
        {
            return m_StartPoint;
        }
    }

    public Vector3[] startPoints
    {
        get
        {
            return m_StartPoints;
        }
    }

    public Vector3 endPoint
    {
        get
        {
            return m_EndPoint;
        }
    }

    public Vector3[] endPoints
    {
        get
        {
            return m_EndPoints;
        }
    }

    public Mesh mesh
    {
        get
        {
            MeshFilter meshFilter = GetComponent(typeof(MeshFilter)) as MeshFilter;
            if (null != meshFilter)
            {
                return meshFilter.sharedMesh;
            }
            return null;
        }
    }


    private float m_RequestTime = 0f;
    private bool m_NeedGeneration = false;

    public bool UpdateCreation()
    {
        if (m_NeedGeneration)
        {
            if ((Time.realtimeSinceStartup - m_RequestTime) < 0.5f)
            {
                return true;
            }

            GenerateMesh();
            m_NeedGeneration = false;
            return true;
        }
        return false;

    }

#if UNITY_EDITOR
    public void Update()
    {
        foreach (Object obj in s_ToDestroy)
        {
            GameObject.DestroyImmediate(obj);
        }
        s_ToDestroy.Clear();

    }

    public QuadraticPolynomial[] centralQPP = new QuadraticPolynomial[0];
    public float[] closestRealRoots = new float[0];
    public Vector3 closestGizmoPos = Vector3.zero;
    public Vector3 needGizmo = Vector3.zero;
    public Vector3 correctionGizmo = Vector3.zero;
    public Vector3 directionGizmoPos = Vector3.zero;
    public Transform trackedTransform;
    public WheelDrive agentWD;
    Color[] cs = new Color[0];

    Segment[] segments;

    public void OnDrawGizmos()
    {
        if (centralQPP.Length != 0)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(closestGizmoPos, 1);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(correctionGizmo, 1);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(needGizmo, 1);
        }
    }

    public void OnDrawGizmosSelected()
    {
        /*Gizmos.color = new Color(1f, 0, 0, 0.5f);
		for (int i = 0; i < m_RoadCount; ++i )
		{
			Vector3[] roadPoints = GetRoadPoints(i);
			foreach (Vector3 pt in roadPoints)
			{
				Gizmos.DrawCube(pt, Vector3.one * 1f);
			}
		}

		Gizmos.color = new Color(0, 1f, 1f, 0.5f);
		for (int i = 0; i < m_RoadLines.Length; ++i)
		{
			Vector3[] roadPoints = GetLinePoints(i);
			foreach (Vector3 pt in roadPoints)
			{
				Gizmos.DrawCube(pt, Vector3.one * 1f);
			}
		}*/

        /*
		Gizmos.color = new Color(0f, 0, 1f, 0.5f);
		foreach (Vector3 point in SplineGenerator.s_InserectPoint)
		{
			Gizmos.DrawCube(transform.TransformPoint(point), Vector3.one * 0.1f);
		}

		Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
		//foreach (Vector3 point in SplineGenerator.s_InserectLine)
		for (int i = 0; i < SplineGenerator.s_InserectLine.Count; i+=4)
		{
			Gizmos.DrawCube(transform.TransformPoint(SplineGenerator.s_InserectLine[i * 4 + 0]), Vector3.one * 0.1f);
			Gizmos.DrawCube(transform.TransformPoint(SplineGenerator.s_InserectLine[i * 4 + 2]), Vector3.one * 0.1f);

			UnityEditor.Handles.DrawAAPolyLine(3f, transform.TransformPoint(SplineGenerator.s_InserectLine[i * 4 + 0]), transform.TransformPoint(SplineGenerator.s_InserectLine[i * 4 + 1]));
			UnityEditor.Handles.DrawAAPolyLine(3f, transform.TransformPoint(SplineGenerator.s_InserectLine[i * 4 + 2]), transform.TransformPoint(SplineGenerator.s_InserectLine[i * 4 + 3]));
		}
		 * */

        Transform transChild = transform.Find("Transform");
        if (null != transChild)
        {
            Vector3 pt1 = transform.TransformPoint(m_SplinePoints[0]);
            Vector3 pt2 = transform.TransformPoint(m_SplinePoints[1]);
            float dist;
            if (Utils.PointOnLine(transChild.position, pt1, pt2, out dist))
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawCube(transChild.position, Vector3.one * 0.2f);
        }
    }
#endif

    public void RemoveRoad(int lineIndex, bool rightRoad)
    {
        if (m_RoadCount > 0)
        {
            Road[] newRoads = new Road[m_Roads.Length - 1];
            RoadLine[] newRoadLines = new RoadLine[m_RoadLines.Length - 1];

            int middleRoad = rightRoad ? (lineIndex) : (lineIndex - 1);

            for (int i = 0; i < lineIndex; ++i)
            {
                if (i >= 0 && i <= m_RoadCount)
                {
                    newRoadLines[i] = m_RoadLines[i];
                }
            }
            for (int i = lineIndex; i < newRoadLines.Length; ++i)
            {
                if (i >= 0 && i <= m_RoadCount)
                {
                    newRoadLines[i] = m_RoadLines[i + 1];
                }
            }

            for (int i = 0; i < middleRoad; ++i)
            {
                if (i >= 0 && i < m_RoadCount)
                {
                    newRoads[i] = m_Roads[i];
                }
            }
            for (int i = middleRoad; i < newRoads.Length; ++i)
            {
                if (i >= 0 && i < m_RoadCount)
                {
                    newRoads[i] = m_Roads[i + 1];
                }
            }

            m_Roads = newRoads;
            m_RoadLines = newRoadLines;
        }
    }

    public void InsertRoad(int lineIndex, bool rightRoad)
    {
        int middleLine = rightRoad ? (lineIndex + 1) : (lineIndex);

        Road[] newRoads = new Road[m_Roads.Length + 1];
        RoadLine[] newRoadLines = new RoadLine[m_RoadLines.Length + 1];

        for (int i = 0; i < middleLine; ++i)
        {
            if (i >= 0 && i <= m_RoadCount)
            {
                newRoadLines[i] = m_RoadLines[i];
            }
        }
        newRoadLines[middleLine] = new RoadLine(m_RoadLines[lineIndex]);
        newRoadLines[middleLine] = new RoadLine();
        for (int i = middleLine + 1; i < m_RoadLines.Length; ++i)
        {
            if (i >= 0 && i <= m_RoadCount)
            {
                newRoadLines[i] = m_RoadLines[i - 1];
            }
        }

        for (int i = 0; i < lineIndex; ++i)
        {
            if (i >= 0 && i < m_RoadCount)
            {
                newRoads[i] = m_Roads[i];
            }
        }
        newRoads[lineIndex] = new Road();
        for (int i = (lineIndex + 1); i <= m_RoadCount; ++i)
        {
            if (i >= 0 && i <= m_RoadCount)
            {
                newRoads[i] = m_Roads[i - 1];
            }
        }

        m_Roads = newRoads;
        m_RoadLines = newRoadLines;
    }

    public void CopySettings(RoadMesh toCopy)
    {
        if (null != toCopy)
        {
            m_Roads = toCopy.m_Roads.Clone() as Road[];
            m_RoadLines = toCopy.m_RoadLines.Clone() as RoadLine[];
        }
    }

    private void FixedUpdate()
    {

        Vector3 current = segments[trackedS].GetDirectionVector(trackedTransform.position);
        Vector3 next = segments[incomingS].GetDirectionVector(trackedTransform.position);

        agentWD.NewCoords(current);
        needGizmo = trackedTransform.position + current;

        if(next != Vector3.zero)
        {
            trackedS++;
            incomingS++;
            if (trackedS >= segments.Length) trackedS = 0;
            if (incomingS >= segments.Length) incomingS = 0;
            Debug.LogWarning(trackedS + " " + incomingS + " " + segments.Length);
        }
    }

    int trackedS = 1;
    int incomingS = 2;

    private void Start()
    {
        QRoadMeshGeneration G = new QRoadMeshGeneration();
        Vector3[] realPoints = new Vector3[m_Points.Length];
        for (int i = 0; i < realPoints.Length; i++)
        {
            realPoints[i] = transform.TransformPoint(m_Points[i]);
        }
        centralQPP = G.GetQPP(realPoints);

        segments = new Segment[centralQPP.Length];
        for (int i = 0; i < centralQPP.Length; i++)
        {
            segments[i] = new Segment(i, centralQPP[i],2.25f);
        }

        /*
        segments[1].TrackedAgent = trackedTransform;
        segments[2].IncomingAgent = trackedTransform;

        segments[0].nextSegment = segments[1];
        segments[0].previousSegment = segments[centralQPP.Length-1];

        segments[centralQPP.Length-1].nextSegment = segments[0];
        segments[centralQPP.Length-1].previousSegment = segments[centralQPP.Length - 2];

        for (int i = 1; i < centralQPP.Length - 1; i++)
        {
            segments[i].nextSegment = segments[i + 1];
            segments[i].previousSegment = segments[i - 1];
        }
                */

    }

    public void FixRoadLine()
    {
        if (m_RoadLines == null || m_RoadLines.Length != (m_RoadCount + 1))
        {
            RoadLine[] roadLines = new RoadMesh.RoadLine[m_RoadCount + 1];
            if (null != m_RoadLines && m_RoadLines.Length > 0)
            {
                System.Array.Copy(m_RoadLines, roadLines, System.Math.Min(m_RoadLines.Length, roadLines.Length));
            }

            for (int i = System.Math.Min(m_RoadLines.Length, roadLines.Length) - 1; i < roadLines.Length; ++i)
            {
                roadLines[i] = new RoadLine();
            }
            m_RoadLines = roadLines;
        }

        for (int i = 0; i < m_RoadLines.Length; ++i)
        {
            if (null == m_RoadLines[i])
            {
                m_RoadLines[i] = new RoadLine();
            }
        }
    }

    public void Clear()
    {
        foreach (Transform child in transform)
        {
            s_ToDestroy.Add(child.gameObject);
        }
    }

    public void RequestFullGeneration()
    {
        m_NeedGeneration = true;
        m_RequestTime = Time.realtimeSinceStartup;
    }

    public void GenerateMesh()
    {
        if (null == RoadConfig.GetInstance())
        {
            Debug.Log("RoadConfig is null 1");
            return;
        }

        gameObject.isStatic = true;

        float[] roadwidths = new float[m_RoadCount];
        for(int i = 0; i < m_RoadCount; i++)
        {
            roadwidths[i] = m_Roads[i].m_Width;
        }

        QRoadMeshGeneration G = new QRoadMeshGeneration();

        Mesh newMesh = G.QGenerateMesh(m_Points, roadwidths, m_SegmentSize);

        MeshFilter meshFilter = GetComponent(typeof(MeshFilter)) as MeshFilter;
        if (null != meshFilter)
        {
            meshFilter.mesh = newMesh;
        }

        MeshCollider meshCollider = GetComponent(typeof(MeshCollider)) as MeshCollider;
        if (null == meshCollider)
        {
            meshCollider = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
        }

        if (null != meshCollider)
        {
            meshCollider.sharedMesh = newMesh;
        }

        MeshRenderer meshRenderer = GetComponent(typeof(MeshRenderer)) as MeshRenderer;
        if (null != meshRenderer)
        {
            meshRenderer.receiveShadows = true;

            Material[] materials = new Material[m_RoadCount];
            if (null != RoadConfig.GetInstance())
            {
                for (int i = 0; i < m_RoadCount; ++i)
                {
                    materials[i] = m_Roads[i].m_Material;
                }
            }
            meshRenderer.sharedMaterials = materials;
        }
    }

    public Vector3 SnapRoad(float snapSize, bool align, bool snapLastPoint, bool useCameraAxis = false)
    {
        if (null != points && points.Length >= 2)
        {
            Vector3 point;
            if (snapLastPoint)
            {
                point = transform.TransformPoint(m_EndPoint);
            }
            else
            {
                point = transform.TransformPoint(m_StartPoint);
            }

            Vector3 newPoint = Vector3.zero;
            Vector3 dir = Vector3.zero;

            Vector3 cameraAxis = Vector3.zero;
            if (null != Camera.current)
            {
                cameraAxis = Camera.current.transform.forward;
            }

            RoadMesh[] allRoadMeshs = GameObject.FindObjectsOfType(typeof(RoadMesh)) as RoadMesh[];

            foreach (RoadMesh otherRoadMesh in allRoadMeshs)
            {
                if (this != otherRoadMesh && otherRoadMesh.gameObject.activeInHierarchy && null != otherRoadMesh.points && otherRoadMesh.points.Length >= 2)
                {
                    //if (otherRoadMesh.m_RoadCount == m_RoadCount)
                    if (otherRoadMesh.m_UsableRoadCount == m_UsableRoadCount)
                    {
                        Vector3 startPos = otherRoadMesh.transform.TransformPoint(otherRoadMesh.startPoint);
                        if (Utils.SqrDistanceBetweenPoint(point, startPos, cameraAxis) < snapSize * snapSize)
                        {
                            newPoint = startPos;
                            dir = otherRoadMesh.transform.TransformPoint(otherRoadMesh.points[0]) - otherRoadMesh.transform.TransformPoint(otherRoadMesh.points[1]);
                        }

                        startPos = otherRoadMesh.transform.TransformPoint(otherRoadMesh.endPoint);
                        if (Utils.SqrDistanceBetweenPoint(point, startPos, cameraAxis) < snapSize * snapSize)
                        {
                            newPoint = startPos;
                            dir = otherRoadMesh.transform.TransformPoint(otherRoadMesh.points[otherRoadMesh.points.Length - 1]) - otherRoadMesh.transform.TransformPoint(otherRoadMesh.points[otherRoadMesh.points.Length - 2]);
                        }
                    }
                    else
                    {
                        Vector3 pointToTest = point;
                        Vector3 diff = Vector3.zero;
                        if (/*(otherRoadMesh.m_RoadCount % 2) !=*/ (m_RoadCount % 2) == 0)
                        {
                            if (snapLastPoint)
                            {
                                //pointToTest = transform.TransformPoint(endPoints[0]);
                                //diff = transform.TransformDirection(endPoints[m_RoadCount / 2] - endPoint);
                                //pointToTest -= 0.5f * transform.TransformDirection(endPoints[0] - endPoints[1]);
                            }
                            else
                            {
                                //pointToTest = transform.TransformPoint(startPoints[0]);
                                //diff = transform.TransformDirection(startPoints[m_RoadCount / 2] - startPoint);
                                //pointToTest -= 0.5f * transform.TransformDirection(startPoints[0] - startPoints[1]);
                            }
                        }

                        /*if (snapLastPoint)
						{
							pointToTest = transform.TransformPoint(endPoints[pointIndex]);
						}
						else
						{
							pointToTest = transform.TransformPoint(startPoints[pointIndex]);
						}*/

                        //diff = Vector3.zero;
                        for (int j = 0; j < otherRoadMesh.m_RoadCount; ++j)
                        {
                            Vector3 otherPoint = otherRoadMesh.transform.TransformPoint(otherRoadMesh.startPoints[j]);
                            //if ((otherRoadMesh.m_RoadCount % 2) != (m_RoadCount % 2) && otherRoadMesh.m_RoadCount > 1)
                            if ((m_RoadCount % 2) == 0 && otherRoadMesh.m_RoadCount > 1)
                            {
                                diff = otherRoadMesh.transform.TransformDirection((otherRoadMesh.startPoints[1] - otherRoadMesh.startPoints[0]).normalized * otherRoadMesh.GetRoadWidth(j));
                                otherPoint += diff / 2f;
                            }

                            if (Utils.SqrDistanceBetweenPoint(pointToTest, otherPoint, cameraAxis) < snapSize * snapSize)

                            {
                                newPoint = otherPoint /*+ diff*/;
                                dir = otherRoadMesh.transform.TransformPoint(otherRoadMesh.points[0]) - otherRoadMesh.transform.TransformPoint(otherRoadMesh.points[1]);
                                break;
                            }


                            otherPoint = otherRoadMesh.transform.TransformPoint(otherRoadMesh.endPoints[j]);
                            //if ((otherRoadMesh.m_RoadCount % 2) != (m_RoadCount % 2) && otherRoadMesh.m_RoadCount > 1)
                            if ((m_RoadCount % 2) == 0 && otherRoadMesh.m_RoadCount > 1)
                            {
                                diff = otherRoadMesh.transform.TransformDirection((otherRoadMesh.endPoints[1] - otherRoadMesh.endPoints[0]).normalized * otherRoadMesh.GetRoadWidth(j));
                                otherPoint -= diff / 2f;
                            }

                            if (Utils.SqrDistanceBetweenPoint(pointToTest, otherPoint, cameraAxis) < snapSize * snapSize)
                            {
                                newPoint = otherPoint /*+ diff*/;
                                dir = otherRoadMesh.transform.TransformPoint(otherRoadMesh.points[otherRoadMesh.points.Length - 1]) - otherRoadMesh.transform.TransformPoint(otherRoadMesh.points[otherRoadMesh.points.Length - 2]);
                                break;
                            }
                        }
                    }
                }
            }

            if (dir == Vector3.zero)
            {
                int inputCount = 0;
                int outputCount = 0;

                for (int i = 0; i < m_RoadCount; ++i)
                {
                    if (!m_Roads[i].m_NotRoad && m_Roads[i].m_Type != EWayType.Pedestrian)
                    {
                        if (m_Roads[i].m_Reverse)
                        {
                            ++outputCount;
                        }
                        else
                        {
                            ++inputCount;
                        }
                    }
                }

                if (snapLastPoint)
                {
                    int temp = outputCount;
                    outputCount = inputCount;
                    inputCount = temp;
                }

                int offsetCount = 0;
                float offsetLen = 0f;
                float tempWidth = 0f;
                for (int i = 0; i < m_RoadCount; ++i)
                {
                    if (!m_Roads[i].m_NotRoad && m_Roads[i].m_Type != EWayType.Pedestrian)
                    {
                        offsetLen += tempWidth + GetRoadWidth(i) * 0.5f;
                        ++offsetCount;
                    }

                    tempWidth += GetRoadWidth(i);
                }
                if (offsetCount > 0)
                {
                    offsetLen /= offsetCount;
                    offsetLen = m_RoadWidth / 2f - offsetLen;
                    if (snapLastPoint)
                    {
                        offsetLen = -offsetLen;
                    }
                }

                
            }

            

            if (dir != Vector3.zero)
            {
                List<Vector3> newPoints = new List<Vector3>(m_Points);
                if (align)
                {
                    Vector3 addedPoint;
                    if (snapLastPoint)
                    {
                        addedPoint = transform.TransformPoint(points[points.Length - 1]) + dir.normalized * m_SegmentSize * 0.9f;
                        float len = (m_Points[m_Points.Length - 1] - m_Points[m_Points.Length - 2]).sqrMagnitude;
                        if (len > m_SegmentSize * m_SegmentSize)
                        {
                            newPoints.Add(m_Points[m_Points.Length - 1]);
                        }
                        newPoints[newPoints.Count - 2] = transform.InverseTransformPoint(addedPoint);
                    }
                    else
                    {
                        addedPoint = transform.TransformPoint(points[0]) + dir.normalized * m_SegmentSize * 0.9f;
                        float len = (m_Points[0] - m_Points[1]).sqrMagnitude;
                        if (len > m_SegmentSize * m_SegmentSize)
                        {
                            newPoints.Insert(0, m_Points[0]);
                        }
                        newPoints[1] = transform.InverseTransformPoint(addedPoint);
                    }
                }

                if (snapLastPoint)
                {
                    newPoints[newPoints.Count - 1] = transform.InverseTransformPoint(newPoint);
                }
                else
                {
                    newPoints[0] = transform.InverseTransformPoint(newPoint);
                }

                m_Points = newPoints.ToArray();
                GenerateMesh();
                return newPoint;
            }
        }
        return Vector3.zero;
    }

    public float GetRoadWidth(int roadIndex)
    {
        if (roadIndex >= 0 && roadIndex < m_Roads.Length)
        {
            return m_Roads[roadIndex].m_Width;
        }
        return 0f;
    }

    public Vector3[] GetRoadPoints(int roadIndex, bool transformed = true)
    {
        if (roadIndex >= 0 && roadIndex < m_Roads.Length)
        {
            if (transformed)
            {
                Vector3[] points = new Vector3[m_RoadPoints[roadIndex].Length];

                for (int i = 0; i < m_RoadPoints[roadIndex].Length; ++i)
                {
                    points[i] = transform.TransformPoint(m_RoadPoints[roadIndex][i]);
                }
                return points;
            }
            else
            {
                return m_RoadPoints[roadIndex];
            }
            /*
			//roadIndex = m_Roads.Length - 1 - roadIndex;
			Mesh currentMesh = mesh;

			List<Vector3> roadPoints = new List<Vector3>();

			int[] roadTriangles = currentMesh.GetTriangles(roadIndex);
			int vertexCount = roadTriangles.Length;
			for (int i = 0; i < vertexCount; i += 6)
			{
				//Vector3 pt1 = currentMesh.vertices[i + roadIndex * 4]; 
				//Vector3 pt2 = currentMesh.vertices[i + roadIndex * 4 + 2];
				Vector3 pt1 = currentMesh.vertices[roadTriangles[i]];
				Vector3 pt2 = currentMesh.vertices[roadTriangles[i + 2]];
				roadPoints.Add(transform.TransformPoint(pt1 + (pt2 - pt1) / 2f));
			}

			return roadPoints.ToArray();
			*/
        }
        return null;
        /*if (roadIndex >= 0 && roadIndex < m_Roads.Length)
		{
			List<Vector3> roadPoints = new List<Vector3>();
			float totalWidth = GetTotalRoadWidth();
			float width = 0f;
			
			for (int i = 0; i < roadIndex; ++i)
			{
				width += GetRoadWidth(i);
			}
			width += GetRoadWidth(roadIndex) / 2f;

			if (null != m_SplinePoints && m_SplinePoints.Length >= 2)
			{

				Vector3 dir = transform.TransformDirection(m_SplinePoints[1] - m_SplinePoints[0]);
				Vector3 lastPoint = m_SplinePoints[0] - dir;
				foreach (Vector3 point in m_SplinePoints)
				{
					dir = point - lastPoint;
					Vector3 right = Vector3.Cross(dir.normalized, Vector3.up);
					right = transform.TransformDirection(right);
					
					Vector3 newPos = transform.TransformPoint(point) + Vector3.up * 0.5f;
					newPos -= right * width - right * totalWidth / 2f;

					roadPoints.Add(newPos);

					lastPoint = point;
				}
			}

			return roadPoints.ToArray();
		}
		return null;*/
    }

    public int GetLinePointCount()
    {
        Mesh currentMesh = mesh;
        if (null != currentMesh)
        {
            return currentMesh.vertexCount / (4 * m_RoadCount);
        }
        return 0;
    }

    public Vector3[] GetLinePoints(int lineIndex, Vector3[] vertices = null, bool transformed = true)
    {
        Mesh currentMesh = mesh;
        if (lineIndex >= 0 && lineIndex < m_RoadLines.Length && null != currentMesh)
        {
            if (transformed)
            {
                Vector3[] points = new Vector3[m_LinePoints[lineIndex].Length];

                for (int i = 0; i < m_LinePoints[lineIndex].Length; ++i)
                {
                    points[i] = transform.TransformPoint(m_LinePoints[lineIndex][i]);
                }
                return points;
            }
            else
            {
                return m_LinePoints[lineIndex];
            }
        }
        return null;
    }

    public float GetRoadLength()
    {
        float len = 0f;
        if (null != m_SplinePoints)
        {
            Vector3 lastPoint = Vector3.zero;
            for (int i = 0; i < m_SplinePoints.Length; ++i)
            {
                Vector3 pt = transform.TransformPoint(m_SplinePoints[i]);
                if (i > 0)
                {
                    len += (pt - lastPoint).magnitude;
                }
                lastPoint = pt;
            }
        }
        return len;
    }

    public static RoadMesh CreateRoadMesh(string gameObjectName)
    {
        GameObject newGo = new GameObject(gameObjectName);
        RoadMesh roadMesh = newGo.AddComponent(typeof(RoadMesh)) as RoadMesh;
        Destroy(newGo.GetComponent(typeof(MeshFilter)));
        Destroy(newGo.GetComponent(typeof(MeshRenderer)));
        newGo.AddComponent(typeof(MeshFilter));
        newGo.AddComponent(typeof(MeshRenderer));
        return roadMesh;
    }

    public int m_AdjustSmooth = 4;
    public float m_AdjustHeightOffset = 0.2f;

    public void AdjustTerrain()
    {
#if UNITY_EDITOR
        Terrain[] terrains = GameObject.FindObjectsOfType(typeof(Terrain)) as Terrain[];


        if (null != terrains)
        {
            List<TerrainData> terrainDatas = new List<TerrainData>();
            foreach (Terrain terrain in terrains)
            {
                terrainDatas.Add(terrain.terrainData);
            }
            UnityEditor.Undo.RecordObjects(terrainDatas.ToArray(), "Adjust terrains");

            foreach (Terrain terrain in terrains)
            {
                TerrainChanger changer = new TerrainChanger(terrain);

                Collider currentCollider = GetComponent<Collider>();
                if (null != currentCollider)
                {
                    changer.SetHeightOnCollider(currentCollider, m_AdjustHeightOffset);
                }

                changer.Smooth(m_AdjustSmooth);
                changer.Apply();
            }
        }
#endif
    }
}
