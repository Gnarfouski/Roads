using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[InitializeOnLoad]
public abstract class AsyncEditor : Editor
{
	private static List<MethodInfo> s_UpdateMethods = new List<MethodInfo>();
	private static List<MethodInfo> s_GuiMethods = new List<MethodInfo>();
	private static List<MethodInfo> s_HierachyChangedMethods = new List<MethodInfo>();

	static AsyncEditor()
	{
		Type type = typeof(AsyncEditor);
		List<Type> types = type.Assembly.GetTypes().Where(subType => subType.IsSubclassOf(type)).ToList();

		foreach (Type subType in types)
		{
			MethodInfo methodInfo = subType.GetMethod("OnAsyncUpdate", BindingFlags.NonPublic | BindingFlags.Static);
			if (null != methodInfo)
			{
				s_UpdateMethods.Add(methodInfo);
			}

			methodInfo = subType.GetMethod("OnAsyncSceneGUI", BindingFlags.NonPublic | BindingFlags.Static);
			if (null != methodInfo)
			{
				s_GuiMethods.Add(methodInfo);
			}

			methodInfo = subType.GetMethod("OnHierachyChange", BindingFlags.NonPublic | BindingFlags.Static);
			if (null != methodInfo)
			{
				s_HierachyChangedMethods.Add(methodInfo);
			}
		}

		EditorApplication.update += OnStaticUpdate;
		SceneView.onSceneGUIDelegate += OnStaticSceneGUI;
		EditorApplication.hierarchyWindowChanged += OnHierachyChange;
	}

	static void OnStaticUpdate()
	{
		foreach (MethodInfo method in s_UpdateMethods)
		{
			method.Invoke(null, null);
		}
	}

	static void OnStaticSceneGUI(SceneView sceneView)
	{
		foreach (MethodInfo method in s_GuiMethods)
		{
			method.Invoke(null, new object[] { sceneView });
		}
	}

	static void OnHierachyChange()
	{
		foreach (MethodInfo method in s_HierachyChangedMethods)
		{
			method.Invoke(null, null);
		}
	}
}
