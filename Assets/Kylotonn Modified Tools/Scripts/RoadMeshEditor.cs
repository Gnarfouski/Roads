using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;


[CustomEditor(typeof(RoadMesh))]
public class RoadMeshEditor : AsyncEditor
{
    [MenuItem("GameObject/Create Other/RoadMesh")]
    static void CreateRoadMesh()
    {
        RoadConfig roadConfig = RoadConfig.GetInstance();
        if (null != roadConfig)
        {
            if (null != roadConfig.m_RoadTemplates && roadConfig.m_RoadTemplates.Length > 0)
            {
                GameObject roadObj = new GameObject("RoadMesh");
                roadObj.isStatic = true;
                roadObj.layer = LayerMask.NameToLayer("Road");
                RoadMesh newRoadMesh = roadObj.AddComponent(typeof(RoadMesh)) as RoadMesh;
                if (null != roadConfig.m_FullRoadTemplate && roadConfig.m_FullRoadTemplate.Length > 0)
                {
                    roadConfig.m_FullRoadTemplate[0].CopyToRoadMesh(roadConfig, newRoadMesh);
                }
                else
                {
                    newRoadMesh.m_RoadCount = 1;
                    newRoadMesh.m_Roads[0].CopyTemplate(roadConfig.m_RoadTemplates[0]);
                }
                newRoadMesh.RequestFullGeneration();
                Selection.activeGameObject = roadObj;
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "No Road template found in RoadConfig", "Ok");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "RoadConfig not found in scene", "Ok");
        }

    }

    private static Vector3 s_PointSnap = Vector3.one * 0.1f;
    private static Vector3 s_Snapped = Vector3.zero;

    Vector2 m_ScrollLines = Vector2.zero;

    int m_SelectedRoad = -1;
    int m_SelectedRoadLine = -1;


    private static RoadMesh[] s_InstanceCache = null;

    static void OnHierachyChange()
    {
        s_InstanceCache = null;
    }

    static void OnAsyncUpdate()
    {
        if (!Application.isPlaying)
        {
            if (null == s_InstanceCache)
            {
                s_InstanceCache = GameObject.FindObjectsOfType(typeof(RoadMesh)) as RoadMesh[];
            }

            if (null != s_InstanceCache)
            {
                s_InstanceCache = s_InstanceCache.Remove(null);
                foreach (RoadMesh roadMesh in s_InstanceCache)
                {
                    if (null != roadMesh && roadMesh.enabled)
                    {
                        roadMesh.UpdateCreation();
                    }
                }
            }
        }
    }

    void OnSceneGUI()
    {
        serializedObject.Update();
        RoadMesh roadMesh = (RoadMesh)target;
        Transform roadMeshTransform = roadMesh.transform;

        if (null != roadMesh.m_Points && roadMesh.enabled)
        {
            Vector3 dir = Vector3.zero;
            Vector3 lastPoint = roadMeshTransform.position;
            for (int i = 0; i < roadMesh.m_Points.Length; i++)
            {
                Vector3 oldPoint = roadMeshTransform.TransformPoint(roadMesh.m_Points[i]);
                float handleSize = 0.05f;
                handleSize *= HandleUtility.GetHandleSize(oldPoint);

                if (Event.current.shift || Event.current.control)
                {
                    Handles.color = Event.current.shift ? Color.red : Color.cyan;
                    if (Handles.Button(oldPoint, Quaternion.identity, handleSize, s_PointSnap.x, Handles.DotHandleCap))
                    {
                        Debug.Log("Click");
                        if (Event.current.shift)
                        {
                            Undo.RecordObject(roadMesh, "Remove Node");
                            List<Vector3> newPoints = new List<Vector3>(roadMesh.m_Points);
                            newPoints.RemoveAt(i);
                            roadMesh.m_Points = newPoints.ToArray();
                            roadMesh.RequestFullGeneration();
                        }
                        else
                        {
                            Undo.RecordObject(roadMesh, "Duplicate Node");
                            List<Vector3> newPoints = new List<Vector3>(roadMesh.m_Points);

                            Vector3 pt1, pt2;
                            if (i == 0)
                            {
                                Vector3 tan = (roadMesh.m_Points[i + 1] - roadMesh.m_Points[i]) * 0.5f;
                                if (tan.magnitude > 1f) tan.Normalize();
                                pt1 = roadMesh.m_Points[i];
                                pt2 = roadMesh.m_Points[i] + tan;
                            }
                            else if (i == roadMesh.m_Points.Length - 1)
                            {
                                Vector3 tan = (roadMesh.m_Points[i] - roadMesh.m_Points[i - 1]) * 0.5f;
                                if (tan.magnitude > 1f) tan.Normalize();
                                pt1 = roadMesh.m_Points[i] - tan;
                                pt2 = roadMesh.m_Points[i];
                            }
                            else
                            {
                                Vector3 tan = (roadMesh.m_Points[i] - roadMesh.m_Points[i - 1]) * 0.5f;
                                if (tan.magnitude > 1f) tan.Normalize();
                                pt1 = roadMesh.m_Points[i] - tan;
                                tan = (roadMesh.m_Points[i + 1] - roadMesh.m_Points[i]) * 0.5f;
                                if (tan.magnitude > 1f) tan.Normalize();
                                pt2 = roadMesh.m_Points[i] + tan;
                            }

                            newPoints.Insert(i, roadMesh.m_Points[i]);
                            newPoints[i] = pt1;
                            newPoints[i + 1] = pt2;

                            roadMesh.m_Points = newPoints.ToArray();
                            roadMesh.RequestFullGeneration();
                        }
                        break;
                    }
                    //Handles.Button()
                }
                else
                {
                    dir = lastPoint - oldPoint;
                    Handles.color = Event.current.control ? Color.cyan : Color.white;

                    Vector3 newPoint = oldPoint;
                    if (RoadMesh.s_UsePositionhandle)
                    {
                        newPoint = Handles.DoPositionHandle(oldPoint, Quaternion.identity);
                    }
                    else
                    {
                        newPoint = Handles.FreeMoveHandle(oldPoint, Quaternion.identity, handleSize, s_PointSnap, Handles.DotHandleCap);
                    }

                    if (newPoint != oldPoint)
                    {

                        Undo.RecordObject(roadMesh, "Move Node");

                        roadMesh.m_Points[i] = roadMeshTransform.InverseTransformPoint(newPoint);
                        roadMesh.RequestFullGeneration();

                        if (i == 0 || i == roadMesh.m_Points.Length - 1)
                        {
                            float snapHandleSize = HandleUtility.GetHandleSize(newPoint) * 0.5f;
                            s_Snapped = roadMesh.SnapRoad(snapHandleSize, false, i != 0, true);
                            if (s_Snapped != Vector3.zero)
                            {
                                roadMesh.m_Points[i] = roadMeshTransform.InverseTransformPoint(s_Snapped);
                                roadMesh.RequestFullGeneration();
                            }
                        }
                        break;
                    }
                    else if ((i == 0 || i == (roadMesh.m_Points.Length - 1)) && Vector3.zero != s_Snapped && Event.current.type == EventType.used)
                    {
                        roadMesh.SnapRoad(0.1f, true, i != 0);
                        s_Snapped = Vector3.zero;
                    }

                }

                lastPoint = oldPoint;
            }

            if (!Event.current.shift && !Event.current.control)
            {
                Handles.color = Color.cyan;

                float handleSize = HandleUtility.GetHandleSize(lastPoint);

                lastPoint -= dir.normalized * handleSize;

                handleSize *= 0.05f;

                Vector3 addedPoint = lastPoint;
                if (RoadMesh.s_UsePositionhandle)
                {
                    Handles.color = Color.blue;
                    addedPoint = Handles.DoPositionHandle(lastPoint, Quaternion.identity);
                }
                else
                {
                    addedPoint = Handles.FreeMoveHandle(lastPoint, Quaternion.identity, handleSize, s_PointSnap, Handles.DotHandleCap); ;
                }


                if (addedPoint != lastPoint)
                {
                    Undo.RecordObject(roadMesh, "Add Node");
                    List<Vector3> newList = new List<Vector3>(roadMesh.m_Points);
                    newList.Add(roadMeshTransform.InverseTransformPoint(addedPoint));
                    roadMesh.m_Points = newList.ToArray();
                    roadMesh.RequestFullGeneration();
                }
            }


            if (serializedObject.ApplyModifiedProperties()
                || (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed"))
            {
                roadMesh.RequestFullGeneration();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        Event currentEvent = Event.current;

        RoadMesh roadMesh = this.target as RoadMesh;

        RoadConfig roadConfig = RoadConfig.GetInstance();

        if (null == roadConfig)
        {
            EditorGUILayout.HelpBox("RoadConfig not found", MessageType.Error);
            return;
        }

        bool hasChange = false;

        //Create road lines array
        roadMesh.FixRoadLine();

        Rect rect;
        rect = EditorGUILayout.GetControlRect();
        RoadMesh.s_UsePositionhandle = EditorUtils.OnOffButton(rect, RoadMesh.s_UsePositionhandle, "Use position handle");
        rect = EditorGUILayout.GetControlRect();
        GUI.Label(rect, "", (GUIStyle)"WindowBottomResize");

        GUILayout.Space(3f);

        if (null != roadConfig.m_FullRoadTemplate && roadConfig.m_FullRoadTemplate.Length > 0)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply template"))
            {
                GUIContent[] popupContents = new GUIContent[roadConfig.m_FullRoadTemplate.Length];
                for (int i = 0; i < roadConfig.m_FullRoadTemplate.Length; ++i)
                {
                    popupContents[i] = new GUIContent(roadConfig.m_FullRoadTemplate[i].m_Name);
                }
                EditorUtility.DisplayCustomMenu(
                    new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0),
                    popupContents,
                    -1,
                    delegate (object obj, string[] contentStrings, int selected)
                    {
                        if (selected >= 0 && selected < roadConfig.m_FullRoadTemplate.Length)
                        {
                            roadConfig.m_FullRoadTemplate[selected].CopyToRoadMesh(roadConfig, roadMesh);
                            roadMesh.RequestFullGeneration();
                        }
                    },
                    null
                );
            }

            if (GUILayout.Button("Adjust Terrain"))
            {
                roadMesh.AdjustTerrain();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3f);
        }

        const float minRoadWidth = 25f;
        const float minRoadLineWidth = 10f;

        const float widthFactor = 30f;
        const float heightFactor = 30f;
        float totalWidth = 0f;
        for (int i = 0; i < roadMesh.m_Roads.Length; ++i)
        {
            totalWidth += Mathf.Max(minRoadWidth, roadMesh.m_Roads[i].m_Width * widthFactor);
        }

        for (int i = 0; i < roadMesh.m_RoadLines.Length; ++i)
        {
            if (roadMesh.m_RoadLines[i].m_Show)
            {
                totalWidth += Mathf.Max(10f, roadMesh.m_RoadLines[i].m_Width * widthFactor);
            }
            else
            {
                totalWidth += minRoadLineWidth;
            }
        }

        bool isLargerThanScreen = (totalWidth + 36) > Screen.width;
        if (isLargerThanScreen)
        {
            m_ScrollLines = EditorGUILayout.BeginScrollView(m_ScrollLines, GUILayout.Height(325f));
        }


        GUILayout.Box("", GUILayout.Width(totalWidth), GUILayout.Height(300f));
        Rect totalRoadRect = GUILayoutUtility.GetLastRect();

        float currentX = 0f;
        for (int i = 0; i <= roadMesh.m_RoadCount; ++i)
        {
            float roadLineWidth = 10f;

            if (roadMesh.m_RoadLines[i].m_Show)
            {
                roadLineWidth = Mathf.Max(minRoadLineWidth, roadMesh.m_RoadLines[i].m_Width * widthFactor);
            }

            Rect roadLineRect = new Rect(totalRoadRect.xMin + currentX, totalRoadRect.yMin, roadLineWidth, totalRoadRect.height);
            currentX += roadLineWidth;

            if (roadMesh.m_RoadLines[i].m_Show)
            {
                if (roadMesh.m_RoadLines[i].m_Space > 0f)
                {
                    float startY = 0f;
                    bool isSpace = false;
                    while (startY < totalRoadRect.height)
                    {
                        float nextY = startY;
                        if (isSpace)
                        {
                            nextY += heightFactor * roadMesh.m_RoadLines[i].m_Space;
                        }
                        else
                        {
                            nextY += heightFactor * roadMesh.m_RoadLines[i].m_Length;
                        }
                        nextY = Mathf.Clamp(nextY, 0f, totalRoadRect.height);

                        if (!isSpace)
                        {
                            MyDrawing.DrawLine(
                               new Vector2(roadLineRect.center.x, roadLineRect.yMin + startY),
                               new Vector2(roadLineRect.center.x, roadLineRect.yMin + nextY),
                               roadMesh.m_RoadLines[i].m_Color,
                               roadMesh.m_RoadLines[i].m_Width * widthFactor,
                               false);
                        }
                        startY = nextY;
                        isSpace = !isSpace;
                    }
                }
                else
                {
                    MyDrawing.DrawLine(new Vector2(roadLineRect.center.x, roadLineRect.yMin), new Vector2(roadLineRect.center.x, roadLineRect.yMax), roadMesh.m_RoadLines[i].m_Color, roadMesh.m_RoadLines[i].m_Width * widthFactor, false);
                }
            }


            Color lineColor = Color.white;
            if (i == m_SelectedRoadLine)
            {
                lineColor = Color.cyan;
            }
            MyDrawing.DrawLine(
                new Vector2(roadLineRect.xMin, roadLineRect.yMin - 2f),
                new Vector2(roadLineRect.xMax, roadLineRect.yMin - 2f),
                lineColor,
                4f,
                false);

            if (currentEvent.type == EventType.MouseDown && roadLineRect.Contains(Event.current.mousePosition))
            {
                if (currentEvent.button == 0)
                {
                    m_SelectedRoad = -1;
                    m_SelectedRoadLine = i;
                    Repaint();
                    Event.current.Use();
                }
                else if (currentEvent.button == 1)
                {
                    ShowRoadLineMenu(roadMesh, i);
                    Event.current.Use();
                }
            }

            if (i < roadMesh.m_RoadCount)
            {
                float roadWidth = Mathf.Max(minRoadWidth, roadMesh.m_Roads[i].m_Width * widthFactor);

                Rect roadRect = new Rect(totalRoadRect.xMin + currentX, totalRoadRect.yMin, roadWidth, totalRoadRect.height);
                if (null != roadMesh.m_Roads[i].m_Material)
                {
                    if (null != roadMesh.m_Roads[i].m_Material.mainTexture)
                    {
                        GUI.DrawTexture(roadRect, roadMesh.m_Roads[i].m_Material.mainTexture);
                    }
                    else
                    {
                        GUI.color = roadMesh.m_Roads[i].m_Material.color;
                        GUI.DrawTexture(roadRect, EditorGUIUtility.whiteTexture);
                        GUI.color = Color.white;
                    }
                }

                if (roadMesh.m_Roads[i].m_Type != RoadMesh.EWayType.None)
                {
                    lineColor = Color.yellow;
                    switch (roadMesh.m_Roads[i].m_Type)
                    {
                        case RoadMesh.EWayType.Road:
                            lineColor = Color.yellow;
                            break;
                        case RoadMesh.EWayType.Pedestrian:
                            lineColor = Color.green;
                            break;
                        default:
                            break;
                    }

                    MyDrawing.DrawLine(
                        new Vector2(roadRect.center.x, roadRect.yMin),
                        new Vector2(roadRect.center.x, roadRect.yMax),
                        lineColor,
                        2f,
                        true);
                }
                else if (!roadMesh.m_Roads[i].m_NotRoad)
                {
                    lineColor = Color.yellow;
                    MyDrawing.DrawLine(
                        new Vector2(roadRect.center.x, roadRect.yMin),
                        new Vector2(roadRect.center.x, roadRect.yMax),
                        Color.yellow,
                        2f,
                        true);
                }

                MyDrawing.DrawLine(
                    new Vector2(roadRect.xMin, roadRect.yMin - 2f),
                    new Vector2(roadRect.xMax, roadRect.yMin - 2f),
                    i == m_SelectedRoad ? Color.cyan : Color.grey,
                    4f,
                    false);

                float arrowWidth = Mathf.Min(10f, roadWidth / 2f);

                Rect arrowRect = new Rect(roadRect.center.x - arrowWidth, roadRect.center.y - 10f, arrowWidth * 2f, 20f);

                if (roadMesh.m_Roads[i].m_Reverse)
                {
                    MyDrawing.DrawLine(
                        new Vector2(arrowRect.xMin, arrowRect.yMin),
                        new Vector2(arrowRect.center.x, arrowRect.yMax),
                        lineColor,
                        2f,
                        true);

                    MyDrawing.DrawLine(
                        new Vector2(arrowRect.xMax, arrowRect.yMin),
                        new Vector2(arrowRect.center.x, arrowRect.yMax),
                        lineColor,
                        2f,
                        true);
                }
                else
                {
                    MyDrawing.DrawLine(
                        new Vector2(arrowRect.xMin, arrowRect.yMax),
                        new Vector2(arrowRect.center.x, arrowRect.yMin),
                        lineColor,
                        2f,
                        true);

                    MyDrawing.DrawLine(
                        new Vector2(arrowRect.xMax, arrowRect.yMax),
                        new Vector2(arrowRect.center.x, arrowRect.yMin),
                        lineColor,
                        2f,
                        true);
                }

                if (currentEvent.type == EventType.MouseDown && roadRect.Contains(Event.current.mousePosition))
                {
                    if (currentEvent.button == 0)
                    {
                        if (arrowRect.Contains(Event.current.mousePosition) && m_SelectedRoad == i)
                        {
                            roadMesh.m_Roads[i].m_Reverse = !roadMesh.m_Roads[i].m_Reverse;
                        }
                        else
                        {
                            m_SelectedRoad = i;
                            m_SelectedRoadLine = -1;
                            Repaint();
                        }
                        Event.current.Use();
                    }
                    else if (currentEvent.button == 1)
                    {
                        ShowRoadMenu(roadMesh, i);
                        Event.current.Use();
                    }
                }

                currentX += roadWidth;
            }
        }
        if (isLargerThanScreen)
        {
            EditorGUILayout.EndScrollView();
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("", EditorUtils.GetPlusButtonSkin(), GUILayout.Width(18f)))
        {
            ++roadMesh.m_RoadCount;
            hasChange = true;
            m_SelectedRoad = -1;
            m_SelectedRoadLine = -1;
        }
        if (GUILayout.Button("", EditorUtils.GetMinusButtonSkin(), GUILayout.Width(18f)))
        {
            --roadMesh.m_RoadCount;
            hasChange = true;
            m_SelectedRoad = -1;
            m_SelectedRoadLine = -1;
        }
        GUILayout.EndVertical();

        EditorGUI.BeginChangeCheck();

        if ((m_SelectedRoad >= 0 && m_SelectedRoad < roadMesh.m_Roads.Length) || (m_SelectedRoadLine >= 0 && m_SelectedRoadLine < roadMesh.m_RoadLines.Length))
        {
            ++EditorGUI.indentLevel;
            EditorGUILayout.BeginVertical((GUIStyle)"AnimationEventBackground");

            if (m_SelectedRoad >= 0 && m_SelectedRoad < roadMesh.m_Roads.Length)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply template", (GUIStyle)"Popup"))
                {
                    ShowRoadMenu(roadMesh, m_SelectedRoad);
                }

                if (GUILayout.Button("Save to Template"))
                {
                    roadConfig.m_RoadTemplates = roadConfig.m_RoadTemplates.Add(new RoadConfig.RoadTemplate(roadMesh.m_Roads[m_SelectedRoad]));
                    EditorUtility.SetDirty(roadConfig);
                }

                EditorGUILayout.EndHorizontal();
                roadMesh.m_Roads[m_SelectedRoad].m_Width = EditorGUILayout.FloatField("Width", roadMesh.m_Roads[m_SelectedRoad].m_Width);
                roadMesh.m_Roads[m_SelectedRoad].m_Reverse = EditorGUILayout.Toggle("Reverse", roadMesh.m_Roads[m_SelectedRoad].m_Reverse);
                roadMesh.m_Roads[m_SelectedRoad].m_NotRoad = EditorGUILayout.Toggle("Not add node", roadMesh.m_Roads[m_SelectedRoad].m_NotRoad);
                roadMesh.m_Roads[m_SelectedRoad].m_Type = (RoadMesh.EWayType)EditorGUILayout.EnumPopup("Road type", roadMesh.m_Roads[m_SelectedRoad].m_Type);

                roadMesh.m_Roads[m_SelectedRoad].m_Material = EditorGUILayout.ObjectField("Material", roadMesh.m_Roads[m_SelectedRoad].m_Material, typeof(Material), true) as Material;
                roadMesh.m_Roads[m_SelectedRoad].m_TurnUv = EditorGUILayout.Toggle("Turn UV", roadMesh.m_Roads[m_SelectedRoad].m_TurnUv);
                roadMesh.m_Roads[m_SelectedRoad].m_UvLen = EditorGUILayout.FloatField("Uv Length", roadMesh.m_Roads[m_SelectedRoad].m_UvLen);

                roadMesh.m_Roads[m_SelectedRoad].m_UvRange.x = EditorGUILayout.Slider("Uv min", roadMesh.m_Roads[m_SelectedRoad].m_UvRange.x, 0f, 1f);
                roadMesh.m_Roads[m_SelectedRoad].m_UvRange.y = EditorGUILayout.Slider("Uv max", roadMesh.m_Roads[m_SelectedRoad].m_UvRange.y, 0f, 1f);
            }
            else if (m_SelectedRoadLine >= 0 && m_SelectedRoadLine < roadMesh.m_RoadLines.Length)
            {
                if (GUILayout.Button("Apply template", (GUIStyle)"Popup"))
                {
                    ShowRoadLineMenu(roadMesh, m_SelectedRoadLine);
                }
                roadMesh.m_RoadLines[m_SelectedRoadLine].m_Show = EditorGUILayout.Toggle("Show", roadMesh.m_RoadLines[m_SelectedRoadLine].m_Show);
                roadMesh.m_RoadLines[m_SelectedRoadLine].m_Center = (RoadMesh.ELineCenter)EditorGUILayout.EnumPopup("Center", roadMesh.m_RoadLines[m_SelectedRoadLine].m_Center);
                roadMesh.m_RoadLines[m_SelectedRoadLine].m_Triangle = EditorGUILayout.Toggle("Triangle (beta)", roadMesh.m_RoadLines[m_SelectedRoadLine].m_Triangle);
                roadMesh.m_RoadLines[m_SelectedRoadLine].m_Width = EditorGUILayout.FloatField("Width", roadMesh.m_RoadLines[m_SelectedRoadLine].m_Width);
                roadMesh.m_RoadLines[m_SelectedRoadLine].m_Length = EditorGUILayout.FloatField("Length", roadMesh.m_RoadLines[m_SelectedRoadLine].m_Length);
                roadMesh.m_RoadLines[m_SelectedRoadLine].m_Space = EditorGUILayout.FloatField("Space", roadMesh.m_RoadLines[m_SelectedRoadLine].m_Space);
                roadMesh.m_RoadLines[m_SelectedRoadLine].m_Offset = EditorGUILayout.FloatField("Offset", roadMesh.m_RoadLines[m_SelectedRoadLine].m_Offset);
                roadMesh.m_RoadLines[m_SelectedRoadLine].m_Height = EditorGUILayout.FloatField("Height", roadMesh.m_RoadLines[m_SelectedRoadLine].m_Height);
                roadMesh.m_RoadLines[m_SelectedRoadLine].m_Color = EditorGUILayout.ColorField("Color", roadMesh.m_RoadLines[m_SelectedRoadLine].m_Color);
                roadMesh.m_RoadLines[m_SelectedRoadLine].m_Material = EditorGUILayout.ObjectField("Material", roadMesh.m_RoadLines[m_SelectedRoadLine].m_Material, typeof(Material), true) as Material;
                roadMesh.m_RoadLines[m_SelectedRoadLine].m_GPSMaterial = EditorGUILayout.ObjectField("GPS Material", roadMesh.m_RoadLines[m_SelectedRoadLine].m_GPSMaterial, typeof(Material), true) as Material;

                //"Select template"
            }
            EditorGUILayout.EndVertical();
            --EditorGUI.indentLevel;

            GUILayout.Space(10f);
        }


        GUILayout.Label("", (GUIStyle)"WindowBottomResize", GUILayout.ExpandWidth(true));

        hasChange |= EditorGUI.EndChangeCheck();

        GUILayout.Label("Road length " + roadMesh.GetRoadLength() + " meters", (GUIStyle)"HelpBox");

        if (DrawDefaultInspector() || hasChange)
        {
            roadMesh.RequestFullGeneration();
        }
    }

    void ShowRoadMenu(RoadMesh roadMesh, int roadIndex)
    {
        RoadConfig roadConfig = RoadConfig.GetInstance();

        RoadConfig.RoadTemplate currentTemplate = roadConfig.GetRoadTemplate(roadMesh.m_Roads[roadIndex]);

        GUIContent[] contents = new GUIContent[roadConfig.m_RoadTemplates.Length];
        for (int i = 0; i < roadConfig.m_RoadTemplates.Length; ++i)
        {
            contents[i] = new GUIContent("Templates/" + roadConfig.m_RoadTemplates[i].m_Name);
            if (roadConfig.m_RoadTemplates[i] == currentTemplate)
            {
                contents[i].text += "\t<=";
            }
        }
        EditorUtility.DisplayCustomMenu(
            new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0)
            , contents
            , -1
            , delegate (object obj, string[] contentStrings, int selected)
            {
                if (selected >= 0 && selected < roadConfig.m_RoadTemplates.Length)
                {
                    roadMesh.m_Roads[roadIndex].CopyTemplate(roadConfig.m_RoadTemplates[selected]);
                    roadMesh.RequestFullGeneration();
                }
            }
            , null
        );
    }

    void ShowRoadLineMenu(RoadMesh roadMesh, int roadLineIndex)
    {
        RoadConfig roadConfig = RoadConfig.GetInstance();

        GUIContent[] contents = new GUIContent[roadConfig.m_RoadLinesTemplates.Length + 6];
        for (int i = 0; i < roadConfig.m_RoadLinesTemplates.Length; ++i)
        {
            contents[i] = new GUIContent("Templates/" + roadConfig.m_RoadLinesTemplates[i].m_Name);
        }

        contents[roadConfig.m_RoadLinesTemplates.Length] = new GUIContent("Rayclones/<None>");


        contents[roadConfig.m_RoadLinesTemplates.Length + 1] = new GUIContent();
        contents[roadConfig.m_RoadLinesTemplates.Length + 2] = new GUIContent(roadLineIndex == 0 ? "" : "Remove line and previous road");
        contents[roadConfig.m_RoadLinesTemplates.Length + 3] = new GUIContent(roadLineIndex == roadMesh.m_RoadCount ? "" : "Remove line and next road"); ;
        contents[roadConfig.m_RoadLinesTemplates.Length + 4] = new GUIContent();
        contents[roadConfig.m_RoadLinesTemplates.Length + 5] = new GUIContent(roadLineIndex == 0 ? "" : "Insert road before");
        contents[roadConfig.m_RoadLinesTemplates.Length + 6] = new GUIContent(roadLineIndex == roadMesh.m_RoadCount ? "" : "Insert road after");

        EditorUtility.DisplayCustomMenu(
            new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0)
            , contents
            , -1
            , delegate (object obj, string[] contentStrings, int selected)
            {
                if (selected >= 0 && selected < roadConfig.m_RoadLinesTemplates.Length)
                {
                    Undo.RecordObject(roadMesh, "Apply template on road line");
                    roadMesh.m_RoadLines[roadLineIndex].CopyTemplate(roadConfig.m_RoadLinesTemplates[selected]);
                    roadMesh.RequestFullGeneration();
                }
                else if (selected >= roadConfig.m_RoadLinesTemplates.Length && selected <= roadConfig.m_RoadLinesTemplates.Length)
                {
                    roadMesh.RequestFullGeneration();
                }
                else if (selected == (roadConfig.m_RoadLinesTemplates.Length + 2))
                {
                    //Remove previous
                    Undo.RecordObject(roadMesh, "Remove line and previous road");
                    roadMesh.RemoveRoad(roadLineIndex, false);
                    roadMesh.RequestFullGeneration();
                }
                else if (selected == (roadConfig.m_RoadLinesTemplates.Length + 3))
                {
                    //Remove next
                    Undo.RecordObject(roadMesh, "Remove line and  next road");
                    roadMesh.RemoveRoad(roadLineIndex, true);
                    roadMesh.RequestFullGeneration();
                }
                else if (selected == (roadConfig.m_RoadLinesTemplates.Length + 5))
                {
                    //Insert before
                    Undo.RecordObject(roadMesh, "Insert road before");
                    roadMesh.InsertRoad(roadLineIndex, false);
                    roadMesh.RequestFullGeneration();
                }
                else if (selected == (roadConfig.m_RoadLinesTemplates.Length + 6))
                {
                    //Insert After
                    Undo.RecordObject(roadMesh, "Insert road after");
                    roadMesh.InsertRoad(roadLineIndex, true);
                    roadMesh.RequestFullGeneration();
                }
            }
            , null
        );
    }
}