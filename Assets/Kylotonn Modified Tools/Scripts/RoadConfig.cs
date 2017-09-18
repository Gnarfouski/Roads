using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class RoadConfig : MonoBehaviour
{
	[System.Serializable]
	public class RoadTemplate
	{
		public string m_Name;
		public bool m_NotRoad = false;
		public RoadMesh.EWayType m_Type = RoadMesh.EWayType.None;
		public float m_Width = 3.5f;
		public Material m_Material;
		public bool m_TurnUv = false;
		public float m_UvLen = 1f;
		public Vector2 m_UvRange = new Vector2(0f, 1f);

		public RoadTemplate()
		{}
		public RoadTemplate(RoadMesh.Road road)
		{
			m_Name = "New Template";
			m_NotRoad = road.m_NotRoad;
			m_Type = road.m_Type;
			m_Width = road.m_Width;
			m_Material = road.m_Material;
			m_TurnUv = road.m_TurnUv;
			m_UvLen = road.m_UvLen;
			m_UvRange = road.m_UvRange;
		}

		public bool Equals(RoadMesh.Road road)
		{
			return m_NotRoad == road.m_NotRoad
				&& m_Type == road.m_Type
				&& m_Width == road.m_Width
				&& m_Material == road.m_Material
				&& m_TurnUv == road.m_TurnUv
				&& m_UvLen == road.m_UvLen
				&& (m_UvRange == road.m_UvRange || (m_UvRange.x == road.m_UvRange.y && m_UvRange.y == road.m_UvRange.x));
		}

		public bool HasUVSwitch(RoadMesh.Road road)
		{
			return (m_UvRange.x == road.m_UvRange.y && m_UvRange.y == road.m_UvRange.x);
		}
	}

	[System.Serializable]
	public class RoadLineTemplate
	{
		public string m_Name;
		public bool m_Show;
		public float m_Width;
		public float m_Length;
		public float m_Space;
		public float m_Height;
		public Color m_Color;
		public Material m_Material;

		public bool Equals(RoadMesh.RoadLine line)
		{
            return m_Show == line.m_Show
                && m_Width == line.m_Width
                && m_Length == line.m_Length
                && m_Space == line.m_Space
                && m_Height == line.m_Height
                && m_Color == line.m_Color
                && m_Material == line.m_Material;
		}
	}

	[System.Serializable]
	public class RayCloneTemplate
	{
		public string m_Name;
		public float m_Space;
	}

	[System.Serializable]
	public enum EWayDirection
	{
		Normal,
		Reverse
	}

	[System.Serializable]
	public enum ELineAlignement
	{
		Default,
		Left,
		Middle,
		Right
	}

	[System.Serializable]
	public class FullRoadTemplate
	{
		public string m_Name;

		public string[] m_RoadTemplate;
		public EWayDirection[] m_RoadDirection;
		public bool[] m_SwitchUVs;
		public string[] m_LineTemplate;
		public ELineAlignement[] m_LineAlignement;

		public void CopyToRoadMesh(RoadConfig roadConfig, RoadMesh roadMesh)
		{
			if (null != roadMesh && null != m_RoadTemplate && m_RoadTemplate.Length > 0)
			{
				roadMesh.m_RoadCount = m_RoadTemplate.Length;
				for (int i = 0; i < roadMesh.m_RoadCount; ++i)
				{
					string roadTemplate = m_RoadTemplate[i];
					int roadTemplateIndex = roadConfig.GetRoadIndex(roadTemplate);
					if (roadTemplateIndex >= 0)
					{
						roadMesh.m_Roads[i].CopyTemplate(roadConfig.m_RoadTemplates[roadTemplateIndex]);
						roadMesh.m_Roads[i].m_Reverse = m_RoadDirection[i] == EWayDirection.Reverse;
						if (m_SwitchUVs[i])
						{
							float tempUv = roadMesh.m_Roads[i].m_UvRange.x;
							roadMesh.m_Roads[i].m_UvRange.x = roadMesh.m_Roads[i].m_UvRange.y;
							roadMesh.m_Roads[i].m_UvRange.y = tempUv;
						}
					}
				}
				for (int i = 0; i <= roadMesh.m_RoadCount && i < m_LineTemplate.Length; ++i)
				{
                    //Debug.Log("Checking " + i + " on " + m_LineTemplate.Length);
					string lineTemplate = m_LineTemplate[i];
					int lineTemplateIndex = roadConfig.GetLineIndex(lineTemplate);
					if (lineTemplateIndex >= 0)
					{
						roadMesh.m_RoadLines[i].CopyTemplate(roadConfig.m_RoadLinesTemplates[lineTemplateIndex]);
					}

					if (m_LineAlignement[i] == ELineAlignement.Left)
					{
						roadMesh.m_RoadLines[i].m_Center = RoadMesh.ELineCenter.Left;
					}
					else if (m_LineAlignement[i] == ELineAlignement.Middle)
					{
						roadMesh.m_RoadLines[i].m_Center = RoadMesh.ELineCenter.Middle;
					}
					else if (m_LineAlignement[i] == ELineAlignement.Right)
					{
						roadMesh.m_RoadLines[i].m_Center = RoadMesh.ELineCenter.Right;
					}
				}
			}
		}
	}

	public RoadTemplate[] m_RoadTemplates;

	public RoadLineTemplate[] m_RoadLinesTemplates;

	public FullRoadTemplate[] m_FullRoadTemplate;

	static RoadConfig s_Instance;
	public static RoadConfig GetInstance()
	{
        GameObject[] rcs = GameObject.FindGameObjectsWithTag("RoadConfig");
        s_Instance = rcs[0].GetComponent<RoadConfig>();
        return s_Instance;
    }

    public RoadConfig()
	{
		//s_Instance = this;
	}

	public void OnDestroy()
	{
		s_Instance = null;
	}

	public int GetRoadIndex(string name)
	{
		if (null != m_RoadTemplates)
		{
			for (int i = 0; i < m_RoadTemplates.Length; ++i)
			{
				if (null != m_RoadTemplates[i] && m_RoadTemplates[i].m_Name == name)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public RoadTemplate GetRoadTemplate(RoadMesh.Road road)
	{
		if (null != m_RoadTemplates)
		{
			for (int i = 0; i < m_RoadTemplates.Length; ++i)
			{
				if (null != m_RoadTemplates[i] && m_RoadTemplates[i].Equals(road))
				{
					return m_RoadTemplates[i];
				}
			}
		}
		return null;
	}

	public int GetLineIndex(string name)
	{
		if (null != m_RoadLinesTemplates)
		{
			for (int i = 0; i < m_RoadLinesTemplates.Length; ++i)
			{
				if (null != m_RoadLinesTemplates[i] && m_RoadLinesTemplates[i].m_Name == name)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public RoadLineTemplate GetLineTemplate(RoadMesh.RoadLine line)
	{
		if (null != m_RoadLinesTemplates)
		{
			for (int i = 0; i < m_RoadLinesTemplates.Length; ++i)
			{
				if (null != m_RoadLinesTemplates[i] && m_RoadLinesTemplates[i].Equals(line))
				{
					return m_RoadLinesTemplates[i];
				}
			}
		}
		return null;
	}
}
