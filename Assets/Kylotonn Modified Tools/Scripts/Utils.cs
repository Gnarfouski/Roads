using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Utils
{
    /**
	 * Project point on plan formed by ignore axis
	*/
    public static float SqrDistanceBetweenPoint(Vector3 pointA, Vector3 pointB, Vector3 ignoreAxis)
    {
        Vector3 diff = pointB - pointA;
        if (ignoreAxis != Vector3.zero)
        {
            float scale = Vector3.Dot(diff, ignoreAxis.normalized);
            return (diff - ignoreAxis * scale).sqrMagnitude;
        }
        else
        {
            return diff.sqrMagnitude;
        }
    }

    public static bool LineLineIntersection(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out Vector3 intersection, bool infiniteLine = false, float epsylon = 0.00001f)
    {

        intersection = Vector3.zero;

        Vector3 lineVec3 = start2 - start1;
        Vector3 lineVec1 = end1 - start1;
        Vector3 lineVec2 = end2 - start2;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //Lines are not coplanar. Take into account rounding errors.
        if ((planarFactor >= epsylon) || (planarFactor <= -epsylon))
        {
            //	Debug.Log(planarFactor);
            return false;
        }

        //Note: sqrMagnitude does x*x+y*y+z*z on the input vector.
        float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;

        if (infiniteLine || ((s >= 0.0f) && (s <= 1.0f)))
        {
            intersection = start1 + (lineVec1 * s);
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool LineLineIntersectionDouble(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out Vector3 intersection, bool infiniteLine = false, double epsylon = 0.00001f)
    {

        intersection = Vector3.zero;

        Vector3 lineVec3 = start2 - start1;
        Vector3 lineVec1 = end1 - start1;
        Vector3 lineVec2 = end2 - start2;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        double planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //Lines are not coplanar. Take into account rounding errors.
        if ((planarFactor >= epsylon) || (planarFactor <= -epsylon))
        {
            //	Debug.Log(planarFactor);
            return false;
        }

        //Note: sqrMagnitude does x*x+y*y+z*z on the input vector.
        double s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;

        if (infiniteLine || ((s >= 0.0f) && (s <= 1.0f)))
        {
            intersection = start1 + (lineVec1 * (float)s);
            return true;
        }
        else
        {
            return false;
        }
    }

    static public Vector3 CatmullSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * (
              (2 * p1) +
              (-p0 + p2) * t +
              (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 +
              (-p0 + 3 * p1 - 3 * p2 + p3) * t3);
    }

    static public Vector3 CatmullSplineNormalized(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        /*Vector3 dir1 = p1 - p0;
		Vector3 dir2 = p2 - p1;
		Vector3 dir3 = p3 - p2;
		Vector3 dir4 = p3 - p0;
		dir4.magnitude*/
        Vector3 n0 = Vector3.zero;
        Vector3 n1 = n0 + (p1 - p0).normalized;
        Vector3 n2 = n1 + (p2 - p1).normalized;
        Vector3 n3 = n2 + (p3 - p2).normalized;

        Vector3 pos = CatmullSpline(n0, n1, n2, n3, t);
        return p1 + (pos - n1) * (p2 - p1).magnitude;
    }

    static public Vector3 CatmullSplineSmooth(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float smooth)
    {
        Vector3 pos1 = CatmullSpline(p0, p1, p2, p3, t);
        Vector3 pos2 = CatmullSplineNormalized(p0, p1, p2, p3, t);
        Vector3 dir1 = pos2 - p1;
        Vector3 dir2 = pos1 - p1;

        float len1 = (p1 - p0).magnitude;
        float len2 = (p2 - p1).magnitude;
        float len3 = (p3 - p2).magnitude;
        float totalLen = len1 + len2 + len3;
        float smoothPower = 1.0f - len2 / totalLen;
        smooth *= smoothPower;
        return p1 + (dir1 * (1.0f - smooth) + dir2 * smooth);
    }

    static public Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        Vector3 dir1 = p1 - p0;
        Vector3 dir2 = p2 - p1;

        Vector3 m1 = p0 + dir1 * t;
        Vector3 m2 = p1 + dir2 * t;

        Vector3 dir3 = m2 - m1;
        return m1 + dir3 * t;
    }

    static public Vector3 BezierSmooth(Vector3 p0, Vector3 p1, Vector3 p2, float t, float smooth)
    {
        Vector3 pos = Bezier(p0, p1, p2, t);
        if (t < 1.5f)
        {
            Vector3 dir1 = (p1 - p0) * t * 2.0f;
            Vector3 dir2 = pos - p0;
            return p0 + (dir1 * (1.0f - smooth) + dir2 * smooth);
        }
        else
        {
            Vector3 dir1 = (p1 - p2) * (t - 0.5f) * 2.0f;
            Vector3 dir2 = pos - p2;
            return p2 + (dir1 * (1.0f - smooth) + dir2 * smooth);
        }
    }

    static public float GetKilometerPerHourToMeterSecond(float len)
    {
        return len / (2.237f * 1.609344f);
    }

    static public float GetUnityToMilesPerHour(float len)
    {
        return len * 2.237f;
    }

    static public float GetUnityToKilometerPerHour(float len)
    {
        return len * 2.237f * 1.609344f;
    }

    static public float GetVehiculeSpeed(GameObject obj)
    {
        //return new Vector2(Vector3.Dot(obj.rigidbody.velocity, obj.transform.forward), Vector3.Dot(obj.rigidbody.velocity, obj.transform.right)).magnitude;
        return Vector3.Dot(obj.GetComponent<Rigidbody>().velocity, obj.transform.forward);
    }

    static public float GetVehicleMilesPerHour(GameObject obj)
    {
        if (null != obj && null != obj.GetComponent<Rigidbody>())
        {
            return GetUnityToMilesPerHour(GetVehiculeSpeed(obj));
        }
        return 0.0f;
    }

    static public float GetVehicleKilometerPerHour(GameObject obj)
    {
        return GetVehicleMilesPerHour(obj) * 1.609344f;
    }

    public static Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve, float smoothness)
    {
        List<Vector3> points;
        List<Vector3> curvedPoints;
        int pointsLength = 0;
        int curvedLength = 0;

        if (smoothness < 1.0f)
            smoothness = 1.0f;

        pointsLength = arrayToCurve.Length;

        curvedLength = (pointsLength * Mathf.RoundToInt(smoothness)) - 1;
        curvedPoints = new List<Vector3>(curvedLength);

        float t = 0.0f;
        for (int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
        {
            t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);

            points = new List<Vector3>(arrayToCurve);

            for (int j = pointsLength - 1; j > 0; j--)
            {
                for (int i = 0; i < j; i++)
                {
                    points[i] = (1 - t) * points[i] + t * points[i + 1];
                }
            }

            curvedPoints.Add(points[0]);
        }

        return (curvedPoints.ToArray());
    }

    static private Material s_oLineMaterial;
    static void CreateLineMaterial()
    {
        if (!s_oLineMaterial)
        {
            /*
            s_oLineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
            "SubShader { Pass { " +
            "    Blend SrcAlpha OneMinusSrcAlpha " +
            "    ZWrite Off Cull Off Fog { Mode Off } " +
            "    BindChannels {" +
            "      Bind \"vertex\", vertex Bind \"color\", color }" +
            "} } }")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            */
            s_oLineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    public static void DrawGradientLine(Vector3 start, Vector3 end, Color startColor, Color endColor)
    {
        CreateLineMaterial();
        // set the current material
        s_oLineMaterial.SetPass(0);

        GL.PushMatrix();

        GL.Begin(GL.LINES);
        GL.Color(startColor);
        GL.Vertex(start);
        GL.Color(endColor);
        GL.Vertex(end);
        GL.End();

        GL.PopMatrix();
    }

    public static bool DictionaryEqual<TKey, TValue>(
        IDictionary<TKey, TValue> first,
        IDictionary<TKey, TValue> second)
    {
        if (first == second) return true;
        if ((first == null) || (second == null)) return false;
        if (first.Count != second.Count) return false;

        var comparer = EqualityComparer<TValue>.Default;

        foreach (KeyValuePair<TKey, TValue> kvp in first)
        {
            TValue secondValue;
            if (!second.TryGetValue(kvp.Key, out secondValue)) return false;
            if (!comparer.Equals(kvp.Value, secondValue)) return false;
        }
        return true;
    }

    public static float ClosestStep(float value, float start, float step)
    {
        float stepCount = Mathf.Round((value - start) / step);
        return start + step * stepCount;
    }

    public static float GetSteerFromGravity(Vector3 gravity)
    {
        return GetSteerFromGravity(gravity, 2f);
    }
    public static float GetSteerFromGravity(Vector3 gravity, float pow)
    {
        //gravity.z = 0f; // Improve inclinaison
        //gravity.y = -1f;
        //float steerInput = Mathf.Clamp01(Vector3.Angle(gravity.normalized, new Vector3(0f, -1f, 0.0f)) / 90.0f);

        float steerInput = Mathf.Clamp01(Vector3.Angle(gravity, new Vector3(0f, -Mathf.Abs(gravity.y), -Mathf.Abs(gravity.z))) / 90.0f);
        steerInput = Mathf.Pow(steerInput, pow);
        if (gravity.x < 0f)
        {
            steerInput *= -1f;
        }
        return steerInput;
    }

    public static bool PointOnLine(Vector3 point, Vector3 startLine, Vector3 endLine, out float distance)
    {
        Vector3 clampPoint;
        return PointOnLine(point, startLine, endLine, out distance, out clampPoint);
    }

    public static bool PointOnLine(Vector3 point, Vector3 startLine, Vector3 endLine, out float distance, out Vector3 clampPoint)
    {
        point.y = startLine.y;//reset height for calcul

        Vector3 dir = endLine - startLine;
        Vector3 dirNorm = dir.normalized;
        Vector3 diff = point - startLine;

        if (diff.sqrMagnitude <= Mathf.Epsilon)
        {
            distance = 0f;
            clampPoint = startLine;
            return true;
        }
        else
        {
            distance = Vector3.Cross(dirNorm, diff).magnitude;
            float dot = Vector3.Dot(dirNorm, diff.normalized);
            if (dot > 0)
            {
                float sqrLen = diff.sqrMagnitude - distance * distance;
                if (sqrLen <= dir.sqrMagnitude)
                {
                    clampPoint = startLine + dirNorm * Mathf.Sqrt(sqrLen);
                    return true;
                }
            }
            else if (dot == 0f)
            {
                clampPoint = startLine;
                return true;
            }
        }

        clampPoint = Vector3.zero;
        return false;
    }

    public static T[] SearchInChild<T>(Transform parent) where T : Component
    {
        List<T> components = new List<T>();
        SearchInChild<T>(parent, components);
        return components.ToArray();
    }

    static void SearchInChild<T>(Transform parent, List<T> components) where T : Component
    {
        if (null != parent && null != components)
        {
            for (int index = 0; index < parent.childCount; ++index)
            {
                Transform child = parent.GetChild(index);
                if (null != child)
                {
                    T childComponent = child.GetComponent(typeof(T)) as T;
                    if (null != childComponent)
                    {
                        components.Add(childComponent);
                    }
                    SearchInChild<T>(child, components);
                }
            }
        }
    }

    public static Bounds GetObjectBounds(Transform obj, bool includeDisableChilds = true)
    {
        Bounds bounds = new Bounds();
        if (null != obj)
        {
            bounds = new Bounds(obj.position, Vector3.zero);

            Renderer renderer = obj.GetComponent<Renderer>();
            if (null != obj.GetComponent<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }
            Collider collider = obj.GetComponent<Collider>();
            if (null != collider)
            {
                bounds.Encapsulate(collider.bounds);
            }

            for (int index = 0; index < obj.childCount; ++index)
            {
                Transform child = obj.GetChild(index);
                if (includeDisableChilds || child.gameObject.activeInHierarchy)
                {
                    bounds.Encapsulate(GetObjectBounds(obj.GetChild(index)));
                }
            }
        }

        return bounds;
    }

    public static Bounds GetObjectsBounds(Transform[] objects)
    {
        Bounds bounds = new Bounds();
        if (null != objects && objects.Length > 0)
        {
            bounds = GetObjectBounds(objects[0]);
            for (int index = 1; index < objects.Length; ++index)
            {
                bounds.Encapsulate(GetObjectBounds(objects[index]));
            }
        }
        return bounds;
    }

    public static string DistanceToString(float meters)
    {
        string formatted = "";
        if (meters >= 1000f)
        {
            formatted = System.String.Format("{0:0.0} ", (meters / 1000f));
#if USE_I18N
				formatted += EI18n.Kilometer.GetText();
#else
            formatted += "km";
#endif
        }
        else
        {
            formatted = System.String.Format("{0:0} ", meters);
#if USE_I18N
				formatted += EI18n.Meter.GetText();
#else
            formatted += "m";
#endif
        }
        return formatted;
    }

    public static T[] GetAllObjectAroundPoint<T>(Vector3 point, float maxDistance) where T : Component
    {
        System.Type objType = typeof(T);
        List<T> aroundObjects = new List<T>();
        float sqrMaxDistance = maxDistance * maxDistance;
        foreach (T obj in GameObject.FindObjectsOfType(objType) as T[])
        {
            if ((obj.transform.position - point).sqrMagnitude <= sqrMaxDistance)
            {
                aroundObjects.Add(obj);
            }
        }
        return aroundObjects.ToArray();
    }

    public static T GetCloserObjectAroundPoint<T>(Vector3 point, float maxDistance) where T : Component
    {
        System.Type objType = typeof(T);
        T aroundObject = null;
        float objDist = Mathf.Infinity;
        float sqrMaxDistance = maxDistance * maxDistance;
        foreach (T obj in Object.FindObjectsOfType(objType) as T[])
        {
            float dist = (obj.transform.position - point).sqrMagnitude;
            if ((aroundObject == null || dist < objDist) && dist <= sqrMaxDistance)
            {
                aroundObject = obj;
                objDist = dist;
            }
        }
        return aroundObject;
    }

    public static bool ArraysEqual<T>(T[] a1, T[] a2)
    {
        if (ReferenceEquals(a1, a2))
            return true;

        if (a1 == null || a2 == null)
            return false;

        if (a1.Length != a2.Length)
            return false;

        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        for (int i = 0; i < a1.Length; i++)
        {
            if (!comparer.Equals(a1[i], a2[i])) return false;
        }
        return true;
    }

    public static Collider[] DisableAllCollider(GameObject go)
    {
        if (null != go)
        {
            List<Collider> disableColliders = new List<Collider>();
            Collider[] colliders = go.GetComponents(typeof(Collider)) as Collider[];
            if (null != colliders)
            {
                foreach (Collider collider in colliders)
                {
                    if (collider.enabled)
                    {
                        collider.enabled = false;
                        disableColliders.Add(collider);
                    }
                }
            }

            Collider goCollider = go.GetComponent(typeof(Collider)) as Collider;
            if (null != goCollider && goCollider.enabled)
            {
                goCollider.enabled = false;
                disableColliders.Add(goCollider);
            }

            //colliders = go.GetComponentsInChildren(typeof(Collider)) as Collider[];
            colliders = SearchInChild<Collider>(go.transform);
            if (null != colliders)
            {
                foreach (Collider collider in colliders)
                {
                    if (collider.enabled)
                    {
                        collider.enabled = false;
                        disableColliders.Add(collider);
                    }
                }
            }
            return disableColliders.ToArray();
        }
        return null;
    }

    public static Collider[] EnableAllCollider(GameObject go)
    {
        if (null != go)
        {
            List<Collider> enableColliders = new List<Collider>();
            Collider[] colliders = go.GetComponents<Collider>() as Collider[];
            if (null != colliders)
            {
                foreach (Collider collider in colliders)
                {
                    if (!collider.enabled)
                    {
                        collider.enabled = true;
                        enableColliders.Add(collider);
                    }
                }
            }
            colliders = go.GetComponentsInChildren<Collider>() as Collider[];
            if (null != colliders)
            {
                foreach (Collider collider in colliders)
                {
                    if (!collider.enabled)
                    {
                        collider.enabled = true;
                        enableColliders.Add(collider);
                    }
                }
            }
            return enableColliders.ToArray();
        }
        return null;
    }

    public static void ReenableAllCollider(Collider[] colliders)
    {
        if (null != colliders)
        {
            foreach (Collider collider in colliders)
            {
                if (null != collider)
                {
                    collider.enabled = true;
                }
            }
        }
    }

    public static List<List<T>> CreateDoubleList<T>(int height, int width) where T : class
    {
        List<List<T>> lines = new List<List<T>>(height); // Might as well set the default capacity...
        for (int j = 0; j < height; j++)
        {
            List<T> row = new List<T>(width);
            for (int i = 0; i < width; i++)
            {
                row.Add(null);
            }
            lines.Add(row);
        }
        return lines;
    }

    public static bool NearestVertexFromCamera(Vector3 pointOnScreen, Camera camera, out Vector3 vertexOut, bool includeTransform = false) // or transform
    {
        if (null != camera)
        {
            Plane[] cameraPlanes = null;
            Vector3 cameraAxis = Vector3.zero;
            cameraAxis = camera.transform.forward;
            cameraPlanes = GeometryUtility.CalculateFrustumPlanes(camera);


            Vector3 minPoint = Vector3.zero;
            float minDist = Mathf.Infinity;

            MeshRenderer[] meshRenderers = GameObject.FindObjectsOfType(typeof(MeshRenderer)) as MeshRenderer[];
            if (null != meshRenderers)
            {
                foreach (MeshRenderer meshRender in meshRenderers)
                {
                    if (null != meshRender && GeometryUtility.TestPlanesAABB(cameraPlanes, meshRender.bounds))
                    {
                        MeshFilter meshFilter = meshRender.GetComponent(typeof(MeshFilter)) as MeshFilter;
                        if (null != meshFilter)
                        {
                            Mesh mesh = meshFilter.sharedMesh;
                            if (null != mesh)
                            {
                                Vector3 cameraAxisInverse = meshFilter.transform.InverseTransformDirection(cameraAxis);
                                Vector3 mouseOriginInverse = meshFilter.transform.InverseTransformPoint(pointOnScreen);
                                Vector3[] vertices = mesh.vertices;
                                for (int i = 0; i < mesh.vertexCount; ++i)
                                {
                                    Vector3 vertex = vertices[i];
                                    float sqrDist = Utils.SqrDistanceBetweenPoint(mouseOriginInverse, vertex, cameraAxisInverse);
                                    if (sqrDist < minDist)
                                    {
                                        minDist = sqrDist;
                                        minPoint = meshFilter.transform.TransformPoint(vertex);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (includeTransform)
            {
                Transform[] transforms = GameObject.FindObjectsOfType(typeof(Transform)) as Transform[];
                if (null != transforms)
                {
                    foreach (Transform transf in transforms)
                    {
                        float sqrDist = Utils.SqrDistanceBetweenPoint(pointOnScreen, transf.position, cameraAxis);
                        if (sqrDist < minDist)
                        {
                            minDist = sqrDist;
                            minPoint = transf.position;
                        }
                    }
                }
            }

            if (minDist != Mathf.Infinity)
            {

                vertexOut = minPoint;
                return true;
            }
        }
        vertexOut = Vector3.zero;
        return false;
    }

    public static Color ColorFromInt(int r, int g, int b, int a = 255)
    {
        Color c = Color.white;
        c.r = (float)r / 255;
        c.g = (float)g / 255;
        c.b = (float)b / 255;
        c.a = (float)a / 255;

        return c;
    }

    public static void SaveMesh(Mesh mesh, GameObject parent = null)
    {
#if UNITY_EDITOR
        if (null != mesh)
        {
            //string folder = UnityEditor.EditorApplication.currentScene;
            string folder = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
            if (null != parent)
            {
                UnityEditor.PrefabType type = UnityEditor.PrefabUtility.GetPrefabType(parent);
                if (type == UnityEditor.PrefabType.PrefabInstance || type == UnityEditor.PrefabType.DisconnectedPrefabInstance)
                {
                    string prefabPath = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.PrefabUtility.GetPrefabParent(parent));
                    if (null != prefabPath && prefabPath.Length > 0)
                    {
                        folder = prefabPath;
                    }
                }
            }

            if (folder.Length > 0)
            {
                if (!System.IO.Directory.Exists(folder + ".Meshs"))
                {
                    UnityEditor.AssetDatabase.CreateFolder(folder.BeforeLast("/"), folder.AfterLast("/") + ".Meshs");
                }
                string meshPath = folder + ".Meshs/" + mesh.GetInstanceID() + ".asset";
                UnityEditor.AssetDatabase.CreateAsset(mesh, meshPath);
            }
        }
#endif
    }

    public static void ApplyHideFlags(GameObject obj, HideFlags flags)
    {
        obj.hideFlags = flags;
        foreach (Transform child in obj.transform)
        {
            ApplyHideFlags(child.gameObject, flags);
        }
    }

    public static T[] MakeArray<T>(params T[] array)
    {
        return array;
    }
}

static public class UtilsExtensions
{
    public static Vector3 Div(this Vector3 a, Vector3 b)
    {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }

    public static Vector2 Mult(this Vector2 a, Vector2 b)
    {
        return new Vector2(a.x * b.x, a.y * b.y);
    }

    public static Vector3 Mult(this Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
    public static Vector4 AddFloat(this Vector4 v, float f)
    {
        return new Vector4(v.x + f, v.y + f, v.z + f, v.w + f);
    }

    public static Vector4 Modulo(this Vector4 v, float f)
    {
        return new Vector4(v.x % f, v.y % f, v.z % f, v.w % f);
    }

    public static void Play(this AudioClip clip)
    {
        AudioSource.PlayClipAtPoint(clip, Vector3.zero);
    }
    public static void PlayAtPosition(this AudioClip clip, Vector3 position)
    {
        AudioSource.PlayClipAtPoint(clip, position);
    }

    public static string GetFullPath(this Transform current)
    {
        if (current.parent == null)
            return "/" + current.name;
        return current.parent.GetFullPath() + "/" + current.name;
    }

    public static bool HasThisChild(this Transform transform, Transform child)
    {
        if (null != child)
        {
            /*if (child.IsChildOf(transform))
			{
				return true;
			}*/
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform transformChild = transform.GetChild(i);
                if (transformChild == child)
                {
                    return true;
                }
                else if (transformChild.HasThisChild(child))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static void SetLayer(this GameObject go, int layer, bool toChildren)
    {
        if (null != go)
        {
            go.layer = layer;
            if (toChildren)
            {
                foreach (Transform child in go.transform)
                {
                    child.gameObject.SetLayer(layer, true);
                }
            }
        }
    }

    public static string BeforeLast(this string str, string last)
    {
        if (null != str && null != last)
        {
            int index = str.LastIndexOf(last);
            if (-1 != index)
            {
                return str.Substring(0, index);
            }
        }
        return str;
    }

    public static string AfterLast(this string str, string last)
    {
        if (null != str && null != last)
        {
            int index = str.LastIndexOf(last);
            if (-1 != index)
            {
                return str.Substring(index + 1, str.Length - (index + 1));
            }
        }
        return str;
    }

    public static Color GetInverse(this Color c, bool alpha = false)
    {
        Color res = c;
        res.r = 1 - c.r;
        res.g = 1 - c.g;
        res.b = 1 - c.b;
        if (alpha)
            res.a = 1 - c.a;

        return res;
    }

    public static T[] Add<T>(this T[] array, T toAdd)
    {
        List<T> newList = new List<T>();
        if (null != array)
        {
            newList.AddRange(array);
        }
        newList.Add(toAdd);
        return newList.ToArray();
    }

    public static T[] Add<T>(this T[] array, T[] toAdd)
    {
        List<T> newList = new List<T>();
        if (null != array)
        {
            newList.AddRange(array);
        }
        newList.AddRange(toAdd);
        return newList.ToArray();
    }

    public static T[] Remove<T>(this T[] array, T toRemove)
    {
        List<T> newList = new List<T>();
        if (null != array)
        {
            newList.AddRange(array);
        }
        newList.Remove(toRemove);
        return newList.ToArray();
    }

    public static void Foreach<T>(this IEnumerable<T> enumerable, System.Action<T> func)
    {
        if (null != enumerable)
        {
            foreach (T obj in enumerable)
            {
                func(obj);
            }
        }
    }

    public static int FindIndex<T>(this IEnumerable<T> items, System.Func<T, bool> predicate)
    {
        if (items == null) throw new System.ArgumentNullException("items");
        if (predicate == null) throw new System.ArgumentNullException("predicate");

        int retVal = 0;
        foreach (var item in items)
        {
            if (predicate(item)) return retVal;
            retVal++;
        }
        return -1;
    }

    public static Dictionary<K, V> ToDictionary<K, V>(this Hashtable table)
    {
        Dictionary<K, V> dico = new Dictionary<K, V>();
        foreach (K key in table.Keys)
        {
            dico.Add((K)key, (V)table[key]);
        }
        return dico;
    }

    public static bool Contains<T>(this IEnumerable<T> items, T o)
    {
        if (items == null) throw new System.ArgumentNullException("items");
        if (o == null) throw new System.ArgumentNullException("o");

        foreach (var item in items)
        {
            if (item.Equals(o)) return true;
        }
        return false;
    }

    public static void RemoveEmpty<T>(this List<T> list)
    {
        list.RemoveAll(item => item == null);
    }

    /// <summary>
    /// Return the color into a hexadecimal representation
    /// </summary>
    /// <param name="c">this Color</param>
    /// <param name="pattern">The string pattern for the color, replaces %r, %g, %b and %a with the values</param>
    /// <returns></returns>
    public static string ToHexaString(this Color c, string pattern = @"#%r%g%b%a")
    {
        string str = pattern;

        str = str.Replace("%r", ((int)(c.r * 255)).ToString("X2"));
        str = str.Replace("%g", ((int)(c.g * 255)).ToString("X2"));
        str = str.Replace("%b", ((int)(c.b * 255)).ToString("X2"));
        str = str.Replace("%a", ((int)(c.a * 255)).ToString("X2"));

        return str;
    }

    public static T[] GetComponents<T>(this GameObject[] gameObjects) where T : Component
    {
        List<T> components = new List<T>();
        if (null != gameObjects)
        {
            foreach (GameObject go in gameObjects)
            {
                components.AddRange(go.transform.GetAllComponents<T>());
            }
        }
        return components.ToArray();
    }

    public static T[] GetAllComponents<T>(this Transform transform) where T : Component
    {
        List<T> components = new List<T>();
        if (null != transform)
        {
            T[] inChild = transform.GetComponents<T>();
            if (null != inChild && inChild.Length > 0)
            {
                components.AddRange(inChild);
            }

            foreach (Transform child in transform)
            {
                inChild = child.GetAllComponents<T>();
                if (null != inChild && inChild.Length > 0)
                {
                    components.AddRange(inChild);
                }
            }
        }
        return components.ToArray();
    }

    public static bool IsAParent(this Transform transform, Transform parent)
    {
        if (null != transform)
        {
            if (transform.parent == parent)
            {
                return true;
            }
            return IsAParent(transform.parent, parent);
        }
        return false;
    }

    public static Transform DeepFind(this Transform transform, string childName)
    {
        foreach (Transform child in transform)
        {
            if (child.name == childName)
                return child;
        }
        foreach (Transform child in transform)
        {
            Transform foundChild = child.DeepFind(childName);
            if (foundChild)
                return foundChild;
        }

        return null;
    }
}