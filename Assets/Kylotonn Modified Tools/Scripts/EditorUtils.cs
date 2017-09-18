
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;


//(GUIStyle)"OL Minus"
//(GUIStyle)"OL Plus"

class EditorUtils
{
	public static void SaveAsset(Object asset, string path)
	{
		Object obj = AssetDatabase.LoadMainAssetAtPath(path);
		if (null != obj)
		{
			EditorUtility.CopySerialized(asset, obj);
			AssetDatabase.SaveAssets();
		}
		else
		{
			//obj = default(T);
			//EditorUtility.CopySerialized(asset, obj);
			AssetDatabase.CreateAsset(asset, path);
		}
	}

	static GUIStyle s_Splitter;
	static GUIStyle GetSplitterSkin()
	{
		//GUISkin skin = GUI.skin;
		//if (null == s_Splitter)
		{
			s_Splitter = new GUIStyle();
			s_Splitter.normal.background = EditorGUIUtility.whiteTexture;
			s_Splitter.stretchWidth = true;
			//s_Splitter.margin = new RectOffset(0, 0, 0, 0);
		}

		return s_Splitter;
	}

	public static GUIStyle GetPickerSkin()
	{
		/*
		GUIStyle picker = new GUIStyle(EditorStyles.colorField);

		picker.imagePosition = ImagePosition.ImageOnly;
		picker.clipping = TextClipping.Clip;
		picker.contentOffset = Vector2.zero;
		picker.margin = new RectOffset(0, 0, 0, 0);
		picker.padding = new RectOffset(0, 0, 0, 0);
		picker.stretchWidth = false;

		picker.overflow = new UnityEngine.RectOffset(0, 0, 0, 0);
		*/
		//return picker;
		return (GUIStyle)"IN ColorField";
	}

	public static GUIStyle GetPlusButtonSkin()
	{
		//(GUIStyle)"OL Minus"
		return (GUIStyle)"OL Plus";
	}

	public static GUIStyle GetMinusButtonSkin()
	{
		return (GUIStyle)"OL Minus";
	}


	public static bool Button(GUIContent content, params GUILayoutOption[] options)
	{
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
		GUILayout.Space(5f + 15f * EditorGUI.indentLevel);
		bool click = GUILayout.Button(content, options);
		EditorGUILayout.EndHorizontal();
		return click;
	}
	public static bool Button(string text, params GUILayoutOption[] options)
	{
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
		GUILayout.Space(5f + 15f * EditorGUI.indentLevel);
		bool click = GUILayout.Button(text, options);
		EditorGUILayout.EndHorizontal();
		return click;
	}
	public static bool Button(Texture image, params GUILayoutOption[] options)
	{
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
		GUILayout.Space(5f + 15f * EditorGUI.indentLevel);
		bool click = GUILayout.Button(image, options);
		EditorGUILayout.EndHorizontal();
		return click;
	}
	public static bool Button(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
	{
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
		GUILayout.Space(5f + 15f * EditorGUI.indentLevel);
		bool click = GUILayout.Button(content, style, options);
		EditorGUILayout.EndHorizontal();
		return click;
	}
	public static bool Button(string text, GUIStyle style, params GUILayoutOption[] options)
	{
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
		GUILayout.Space(5f + 15f * EditorGUI.indentLevel);
		bool click = GUILayout.Button(text, style, options);
		GUILayout.EndHorizontal();
		return click;
	}
	public static bool Button(Texture image, GUIStyle style, params GUILayoutOption[] options)
	{
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
		GUILayout.Space(5f + 15f * EditorGUI.indentLevel);
		bool click = GUILayout.Button(image, style, options);
		EditorGUILayout.EndHorizontal();
		return click;
	}

	public static bool PlusButton(string text = null, bool withIndent = true)
	{
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false), GUILayout.Width(20f), GUILayout.Height(20f));
		if (withIndent)
		{
			GUILayout.Space(5f + 15f * EditorGUI.indentLevel);
		}
		bool click = GUILayout.Button("", EditorUtils.GetPlusButtonSkin(), GUILayout.Width(20f), GUILayout.Height(20f));
		if (null != text)
		{
			GUILayout.Label(text);
		}
		EditorGUILayout.EndHorizontal();
		return click;
	}

	public static bool MinusButton(string text = null, bool withIndent = true)
	{
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false), GUILayout.Width(20f), GUILayout.Height(20f));
		if (withIndent)
		{
			GUILayout.Space(5f + 15f * EditorGUI.indentLevel);
		}
		bool click = GUILayout.Button("", EditorUtils.GetMinusButtonSkin(), GUILayout.Width(20f), GUILayout.Height(20f));
		if (null != text)
		{
			GUILayout.Label(text);
		}
		EditorGUILayout.EndHorizontal();
		return click;
	}

	public static void Label(string text)
	{
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(5f + 15f * EditorGUI.indentLevel);
		GUILayout.Label(text);
		EditorGUILayout.EndHorizontal();
	}
	public static void Label(string text, params GUILayoutOption[] options)
	{
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(5f + 15f * EditorGUI.indentLevel);
		GUILayout.Label(text, options);
		EditorGUILayout.EndHorizontal();
	}
	public static void Label(string text, GUIStyle style, params GUILayoutOption[] options)
	{
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(5f + 15f * EditorGUI.indentLevel);
		GUILayout.Label(text, style, options);
		EditorGUILayout.EndHorizontal();
	}

	public static bool EditableFoldout(Rect position, bool state, ref string editableText, ref bool textChanged)
	{
		Rect textRect = new Rect(position); 
		textRect.x += 16f;
		textRect.width -= 16f;
		string newText = EditorGUI.TextField(textRect, editableText);
		if (newText != editableText)
		{
			textChanged = true;
			editableText = newText;
		}

		Rect foldoutRect = new Rect(position);
		state = EditorGUI.Foldout(foldoutRect, state, "          ", false);

		return state;
	}

	public static bool EditableFoldout(bool state, ref string editableText, ref bool textChanged, UnityEngine.Object undoObject = null, string undoText = "")
	{
		Rect foldoutRect = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.fieldWidth, 16f, 16f, EditorStyles.foldout);
		bool textChangedTemp = false;
		string editableTextTemp = editableText;
		bool ret = EditableFoldout(foldoutRect, state, ref editableTextTemp, ref textChangedTemp);
		if (textChangedTemp && null != undoObject)
		{
			Undo.RecordObject(undoObject, undoText);
		}
		textChanged |= textChangedTemp;
		editableText = editableTextTemp;
		return ret;
	}

	public static void SamllErrorBox(string text, params GUILayoutOption[] options)
	{
		GUILayout.Label(new GUIContent(text, EditorGUIUtility.FindTexture("d_console.erroricon.sml")), (GUIStyle)"HelpBox", options); 
	}

	public static bool Picker(Rect rect)
	{
		return Picker(new Vector2(rect.x, rect.y));
	}

	public static bool Picker(Vector2 pos)
	{
		return GUI.Button(new Rect(pos.x, pos.y, 20f, 18f), "", GetPickerSkin());
	}

	public static bool Picker()
	{
		return GUILayout.Button("", GetPickerSkin(), new GUILayoutOption[] {GUILayout.Width(20f), GUILayout.Height(18f)});
	}

	public static void DrawSplitter()
	{
		DrawSplitter(new Color(0.35f, 0.35f, 0.35f), 1f);
	}

	public static void DrawSplitter(Color color, float height)
	{
		Rect position = GUILayoutUtility.GetRect(GUIContent.none, GetSplitterSkin(), GUILayout.Height(height));
		if (Event.current.type == EventType.Repaint)
		{
			Color restoreColor = GUI.color;
			GUI.color = color;
			GetSplitterSkin().Draw(position, false, false, false, false);
			GUI.color = restoreColor;
		}
	}

	public static List<string> layers;
	public static List<int> layerNumbers;
	public static string[] layerNames;
	public static long lastUpdateTick;

	/** Displays a LayerMask field.
	 * \param showSpecial Use the Nothing and Everything selections
	 * \param selected Current LayerMask
	 * \version Unity 3.5 and up will use the EditorGUILayout.MaskField instead of a custom written one.
	 */
	public static LayerMask LayerMaskField(string label, LayerMask selected, bool showEmpty = true)
	{
		if (layers == null || (System.DateTime.Now.Ticks - lastUpdateTick > 10000000L && Event.current.type == EventType.Layout))
		{
			lastUpdateTick = System.DateTime.Now.Ticks;
			if (layers == null)
			{
				layers = new List<string>();
				layerNumbers = new List<int>();
				layerNames = new string[4];
			}
			else
			{
				layers.Clear();
				layerNumbers.Clear();
			}

			int emptyLayers = 0;
			for (int i = 0; i < 32; i++)
			{
				string layerName = LayerMask.LayerToName(i);

				if (layerName != "")
				{
					if (showEmpty)
					{
						for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer " + (i - emptyLayers));
					}
					else
					{
						for (; emptyLayers > 0; emptyLayers--) layers.Add(null);
					}
					layerNumbers.Add(i);
					layers.Add(layerName);
				}
				else
				{
					emptyLayers++;
				}
			}

			if (layerNames.Length != layers.Count)
			{
				layerNames = new string[layers.Count];
			}
			for (int i = 0; i < layerNames.Length; i++) layerNames[i] = layers[i];
		}

		selected.value = EditorGUILayout.MaskField(label, selected.value, layerNames);

		return selected;
	}

	private static MethodInfo s_SearchFieldMethod = null;
	public static string SearchField(Rect rect, string text)
	{
		if (null == s_SearchFieldMethod)
		{
			s_SearchFieldMethod = typeof(EditorGUI).GetMethod(
				"SearchField",
				BindingFlags.Static | BindingFlags.NonPublic,
				null,
				new System.Type[] {
							typeof(Rect),
							typeof(string)
						},
				null
			);
		}

		return (string)s_SearchFieldMethod.Invoke(
			null,
			new object[] {
						rect,
						text
					}
		);
	}

	public static string SearchField(string text)
	{
		Rect rect = GUILayoutUtility.GetRect(0f, 20f, 16f, 16f);

		return SearchField(rect, text);
	}

	public static void DrawSelector(Vector3 pos, Color color, float size = 1f)
	{
		Handles.color = color;
		float handleSize = HandleUtility.GetHandleSize(pos) * size;
		if (null != Camera.current)
		{
			Handles.ConeHandleCap(0, pos - Camera.current.transform.right * handleSize * 2f, Quaternion.LookRotation(Camera.current.transform.right), handleSize, EventType.Repaint);
			Handles.ConeHandleCap(0, pos - Camera.current.transform.up * handleSize * 2f, Quaternion.LookRotation(Camera.current.transform.up), handleSize, EventType.Repaint);

            Handles.ConeHandleCap(0, pos + Camera.current.transform.right * handleSize * 2f, Quaternion.LookRotation(-Camera.current.transform.right), handleSize, EventType.Repaint);
			Handles.ConeHandleCap(0, pos + Camera.current.transform.up * handleSize * 2f, Quaternion.LookRotation(-Camera.current.transform.up), handleSize, EventType.Repaint);
		}
		else
		{
			Handles.DrawWireDisc(pos, Vector3.forward, handleSize);
			Handles.DrawWireDisc(pos, Vector3.up, handleSize);
			Handles.DrawWireDisc(pos, Vector3.right, handleSize);
		}
	}

	public static void HightLightGameObject(GameObject go, Color color)
	{
		if (null != go)
		{
			Color tempColor = Handles.color;
			Handles.color = color;

			MeshFilter meshFilter = go.GetComponent(typeof(MeshFilter)) as MeshFilter;
			if (null != meshFilter && null != meshFilter.sharedMesh)
			{

				if (null == s_PingMaterial)
				{
					//s_PingMaterial = new Material(Shader.Find("Transparent/Diffuse"));
					s_PingMaterial = new Material(Shader.Find("DriveBox/Overlay"));
				}
				s_PingMaterial.SetPass(0);

				s_PingMaterial.color = color;
				Graphics.DrawMeshNow(meshFilter.sharedMesh, go.transform.localToWorldMatrix);
			}

			BoxCollider boxCollider = go.GetComponent(typeof(BoxCollider)) as BoxCollider;
			if (null != boxCollider)
			{
				Vector3 pt1 = boxCollider.transform.TransformPoint(boxCollider.center + new Vector3(-boxCollider.size.x, boxCollider.size.y, -boxCollider.size.z) / 2f);
				Vector3 pt2 = boxCollider.transform.TransformPoint(boxCollider.center + new Vector3(boxCollider.size.x, boxCollider.size.y, -boxCollider.size.z) / 2f);
				Vector3 pt3 = boxCollider.transform.TransformPoint(boxCollider.center + new Vector3(boxCollider.size.x, boxCollider.size.y, boxCollider.size.z) / 2f);
				Vector3 pt4 = boxCollider.transform.TransformPoint(boxCollider.center + new Vector3(-boxCollider.size.x, boxCollider.size.y, boxCollider.size.z) / 2f);

				Vector3 pt5 = boxCollider.transform.TransformPoint(boxCollider.center + new Vector3(-boxCollider.size.x, -boxCollider.size.y, -boxCollider.size.z) / 2f);
				Vector3 pt6 = boxCollider.transform.TransformPoint(boxCollider.center + new Vector3(boxCollider.size.x, -boxCollider.size.y, -boxCollider.size.z) / 2f);
				Vector3 pt7 = boxCollider.transform.TransformPoint(boxCollider.center + new Vector3(boxCollider.size.x, -boxCollider.size.y, boxCollider.size.z) / 2f);
				Vector3 pt8 = boxCollider.transform.TransformPoint(boxCollider.center + new Vector3(-boxCollider.size.x, -boxCollider.size.y, boxCollider.size.z) / 2f);

				float lineSize = /*HandleUtility.GetHandleSize(boxCollider.bounds.center) **/ 10f;
				Handles.DrawAAPolyLine(lineSize, new Vector3[] { pt1, pt2, pt3, pt4, pt1, pt5, pt6, pt7, pt8, pt5 });
				Handles.DrawAAPolyLine(lineSize, new Vector3[] { pt2, pt6 });
				Handles.DrawAAPolyLine(lineSize, new Vector3[] { pt3, pt7 });
				Handles.DrawAAPolyLine(lineSize, new Vector3[] { pt8, pt4 });
			}

			Handles.color = tempColor;
		}
	}

	public static void ShowSelection(SerializedProperty property, Color color, float size = 1f)
	{
		GameObject go = null;
		if (null != property.objectReferenceValue)
		{
			GameObject gameObject = property.objectReferenceValue as GameObject;
			if (null != gameObject)
			{
				go = gameObject;
			}
			else
			{
				Component component = property.objectReferenceValue as Component;
				if (null != component)
				{
					go = component.gameObject;
				}
			}
		}
		ShowGameObject(go);
	}

	static Vector3 s_ShowPosition;
	static GameObject s_ShowGameObject;
	static float s_ShowPositionTime = 0f;
	static Material s_PingMaterial = null;
	public static void ShowPosition(Vector3? pos)
	{
		SceneView.onSceneGUIDelegate -= ShowPositionOnSceneGUI;
		if (null != pos)
		{
			s_ShowPosition = pos.Value;
			s_ShowGameObject = null;
			s_ShowPositionTime = Time.realtimeSinceStartup;
			SceneView.onSceneGUIDelegate += ShowPositionOnSceneGUI;
		}
	}

	public static void ShowGameObject(GameObject gameObject)
	{
		SceneView.onSceneGUIDelegate -= ShowPositionOnSceneGUI;
		if (null != gameObject)
		{
			s_ShowGameObject = gameObject;
			s_ShowPositionTime = Time.realtimeSinceStartup;
			SceneView.onSceneGUIDelegate += ShowPositionOnSceneGUI;
		}
	}

	static void ShowPositionOnSceneGUI(SceneView sceneView)
	{
		float alpha = 1f - Mathf.Clamp01(((Time.realtimeSinceStartup - s_ShowPositionTime) - 3f) / 1f);
		Vector3 pos = s_ShowPosition;
		if (null != s_ShowGameObject)
		{
			pos = s_ShowGameObject.transform.position;
		}
		float materialAlphaModulo = (Mathf.Sin((float)EditorApplication.timeSinceStartup * 4f) / 2f + 0.5f) * 0.7f + 0.3f;
		Color overlayColor = new Color(0, 0.5f, 1f, materialAlphaModulo * alpha);
		EditorUtils.HightLightGameObject(s_ShowGameObject, overlayColor);
		EditorUtils.DrawSelector(pos, new Color(0f, 1f, 1f, alpha), 0.25f);

		if ((Time.realtimeSinceStartup - s_ShowPositionTime) > 4f)
		{
			SceneView.onSceneGUIDelegate -= ShowPositionOnSceneGUI;
		}
		sceneView.Repaint();
	}

	public static float ClosestStep(float value, float start, float step)
	{
		int stepCount = (int)Mathf.Round((value - start) / step);
		return (float)((double)start + (double)step * (double)stepCount);
	}

	public static void LockObject(GameObject go, bool undoEnable = true)
	{
		if (undoEnable)
		{
			Undo.RecordObject(go, "Lock Object");
		}

		go.hideFlags |= HideFlags.NotEditable;

		foreach (Component comp in go.GetComponents(typeof(Component)))
		{
			if (!(comp is Transform))
			{
				//if (EditorPrefs.GetBool(DisableSelectionPrefKey))
				{
					comp.hideFlags |= HideFlags.NotEditable;
					comp.hideFlags |= HideFlags.HideInHierarchy;
				}
			}
		}

		EditorUtility.SetDirty(go);
	}

	public static void UnlockObject(GameObject go, bool undoEnable = true)
	{
		if (undoEnable)
		{
			Undo.RecordObject(go, "Unlock Object");
		}

		go.hideFlags &= ~HideFlags.NotEditable;

		foreach (Component comp in go.GetComponents(typeof(Component)))
		{
			if (!(comp is Transform))
			{
				// Don't check pref key; no harm in removing flags that aren't there
				comp.hideFlags &= ~HideFlags.NotEditable;
				comp.hideFlags &= ~HideFlags.HideInHierarchy;
			}
		}

		EditorUtility.SetDirty(go);
	}

	public static bool IsObjectLocked(GameObject go)
	{
		return 0 != (go.hideFlags & HideFlags.NotEditable);
	}

	static Texture2D s_LockTexture;
	public static Texture2D lockTexture
	{
		get
		{
			if (null == s_LockTexture)
			{
				s_LockTexture = AssetDatabase.LoadAssetAtPath("Assets/Editor/Icons/Lock.png", typeof(Texture2D)) as Texture2D;
			}
			return s_LockTexture;
		}
	}

	static Texture2D s_VertexCenterTexture;
	public static Texture2D vertexCenterTexture
	{
		get
		{
			if (null == s_VertexCenterTexture)
			{
				s_VertexCenterTexture = AssetDatabase.LoadAssetAtPath("Assets/Editor/Icons/VertexCenter.png", typeof(Texture2D)) as Texture2D;
			}
			return s_VertexCenterTexture;
		}
	}

	static Texture2D s_TerrainTexture;
	public static Texture2D terrainTexture
	{
		get
		{
			if (null == s_TerrainTexture)
			{
				s_TerrainTexture = AssetDatabase.LoadAssetAtPath("Assets/Editor/Icons/Terrain.png", typeof(Texture2D)) as Texture2D;
			}
			return s_TerrainTexture;
		}
	}

	public static bool OnOffButton(Rect position, bool isOn, string label)
	{
		const float buttonWidth = 50f;
		GUI.Label(
				new Rect(position.x, position.y, position.width - buttonWidth, position.height)
				, label);
		if (GUI.Button(
			new Rect(position.x + position.width - buttonWidth, position.y, buttonWidth, position.height)
			, isOn ? "On" : "Off"
			, isOn ? (GUIStyle)"sv_label_3" : (GUIStyle)"sv_label_6"
		))
		{
			return !isOn;
		}
		return isOn;
	}


	static Stack<int> s_StackGUIIndentLevel = new Stack<int>();

	public static void PushGUIIndent(int newLevel = 0)
	{
		s_StackGUIIndentLevel.Push(EditorGUI.indentLevel);
		EditorGUI.indentLevel = newLevel;
	}

	public static void PopGUIIndent()
	{
		if (s_StackGUIIndentLevel.Count > 0)
		{
			EditorGUI.indentLevel = s_StackGUIIndentLevel.Pop();
		}
		else
		{
			Debug.LogError("No more GUI indent in stack");
		}
	}

	static Stack<GUIStyle> s_StackBoxStyle = new Stack<GUIStyle>();
	public static void StartBox(GUIStyle style)
	{
		s_StackBoxStyle.Push(style);
		GUILayout.Space(style.border.top / 2f);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(EditorGUI.indentLevel * 15f + style.border.left / 2f);
		EditorUtils.PushGUIIndent();
		EditorGUILayout.BeginVertical(style);
	}

	public static void EndBox()
	{
		GUIStyle style = s_StackBoxStyle.Pop();
		EditorGUILayout.EndVertical();
		GUILayout.Space(style.border.right / 2f);
		EditorUtils.PopGUIIndent();
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(style.border.bottom / 2f);
	}

	public static bool NearestVertexFromCamera(out Vector3 vertexOut, bool includeTransform = false)
	{
		return Utils.NearestVertexFromCamera(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin, Camera.current, out vertexOut, includeTransform);
	}

	public static void ToggleGizmos(bool gizmosOn, System.Type typeToToggle = null)
	{
		int val = gizmosOn ? 1 : 0;
		Assembly asm = Assembly.GetAssembly(typeof(Editor));
		System.Type type = asm.GetType("UnityEditor.AnnotationUtility");
		if (type != null)
		{
			MethodInfo getAnnotations = type.GetMethod("GetAnnotations", BindingFlags.Static | BindingFlags.NonPublic);
			MethodInfo setGizmoEnabled = type.GetMethod("SetGizmoEnabled", BindingFlags.Static | BindingFlags.NonPublic);
			MethodInfo setIconEnabled = type.GetMethod("SetIconEnabled", BindingFlags.Static | BindingFlags.NonPublic);
			var annotations = getAnnotations.Invoke(null, null);
			foreach (object annotation in (System.Collections.IEnumerable)annotations)
			{
				System.Type annotationType = annotation.GetType();
				FieldInfo classIdField = annotationType.GetField("classID", BindingFlags.Public | BindingFlags.Instance);
				FieldInfo scriptClassField = annotationType.GetField("scriptClass", BindingFlags.Public | BindingFlags.Instance);
				if (classIdField != null && scriptClassField != null)
				{
					int classId = (int)classIdField.GetValue(annotation);
					string scriptClass = (string)scriptClassField.GetValue(annotation);
					if (null == typeToToggle || typeToToggle.ToString() == scriptClass)
					{
						setGizmoEnabled.Invoke(null, new object[] { classId, scriptClass, val });
						setIconEnabled.Invoke(null, new object[] { classId, scriptClass, val });
					}
				}
			}
		}
	}

	public static IEnumerable<GameObject> SceneRoots()
	{
		//EditorApplication.RepaintHierarchyWindow();
		return (GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[]).Where(go => null != go && go.transform.parent == null);
		/*var prop = new HierarchyProperty(HierarchyType.GameObjects);
		var expanded = new int[0];
		while (prop.Next(expanded)) {
			yield return prop.pptrValue as GameObject;
		}*/
	}

	public static void ApplyHideFlags(GameObject obj, HideFlags flags)
	{
		obj.hideFlags = flags;
		EditorUtility.SetDirty(obj);
		foreach (Transform child in obj.transform)
		{
			ApplyHideFlags(child.gameObject, flags);
		}
	}

	public static void AddSceneToBuild(string scenePath, bool enable = true)
	{
		if (EditorBuildSettings.scenes.Count(scene => scene.path == scenePath) == 0)
		{
			EditorBuildSettings.scenes = EditorBuildSettings.scenes.Add(new EditorBuildSettingsScene(scenePath, enable));
		}
	}

	public static void RemoveSceneToBuild(string scenePath)
	{
		EditorBuildSettings.scenes = EditorBuildSettings.scenes.Where(scene => scene.path != scenePath).ToArray();
	}

	public static void RemoveSceneToBuild(System.Text.RegularExpressions.Regex regex)
	{
		EditorBuildSettings.scenes = EditorBuildSettings.scenes.Where(scene => !regex.IsMatch(scene.path)).ToArray();
	}
}

static class EditorUtilsExtensions
{
	/*public static string GetDisplayName(this SerializedProperty property)
	{
		if (null == EditorUtilsExtensions.s_DisplayNamePropertyInfo)
		{
			s_DisplayNamePropertyInfo = typeof(SerializedProperty).GetProperty("displayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance);
		}

		return (string)s_DisplayNamePropertyInfo.GetValue(property, null);
	}*/

	public static int Depth(this Transform transform)
	{
		Transform root = transform.root;
		Transform current = transform;
		int depth = 0;

		while (current != root)
		{
			++depth;
			current = current.parent;
		}

		return depth;
	}
}
