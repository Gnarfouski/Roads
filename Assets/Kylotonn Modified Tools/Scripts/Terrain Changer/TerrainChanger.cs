using UnityEngine;
using System.Collections.Generic;

public static class TerrainExtensions
{
	public static void SetWorldPosHeight(this Terrain terrain, Vector3 worldPos, float y, float radius = 1f, float startSmooth = 1f)
	{
		if (null != terrain)
		{
			TerrainData terrainData = terrain.terrainData;
			if (null != terrainData)
			{
				float minX = terrain.transform.position.x;
				float minZ = terrain.transform.position.z;
				float maxX = minX + terrainData.size.x;
				float maxZ = minZ + terrainData.size.z;

				bool inTerrain = worldPos.x >= minX && worldPos.x <= maxX && worldPos.z >= minZ && worldPos.z <= maxZ;
				if (inTerrain)
				{
					float fx = (worldPos.x - terrain.transform.position.x) / terrainData.size.x;
					float fz = (worldPos.z - terrain.transform.position.z) / terrainData.size.z;
					int x = Mathf.RoundToInt(fx * terrainData.heightmapResolution);
					int z = Mathf.RoundToInt(fz * terrainData.heightmapResolution);


					int w = Mathf.RoundToInt((radius * 2) / (terrainData.size.x / terrainData.heightmapResolution));
					int h = Mathf.RoundToInt((radius * 2) / (terrainData.size.z / terrainData.heightmapResolution));

					int tempX = x - w / 2;
					int tempZ = z - h / 2;
					int newX = System.Math.Max(0, tempX);
					int newZ = System.Math.Max(0, tempZ);
					int newW = System.Math.Min(terrainData.heightmapResolution, w - (tempX - newX));
					int newH = System.Math.Min(terrainData.heightmapResolution, h - (tempZ - newZ));

					float[,] oldHeights = terrainData.GetHeights(newX, newZ, newW, newH);
					float[,] heights = new float[newH, newW];
					float newHeight = (y - terrain.transform.position.y) / terrainData.size.y;
					for (int i = 0; i < newH; ++i)
					{
						for (int j = 0; j < newW; ++j)
						{
							float distX = ((newX + j) - x) / (float)w;
							float distZ = ((newZ + i) - z) / (float)h;
							float factor = 1f - Mathf.Sqrt(distX * distX + distZ * distZ);
							factor = Mathf.Clamp01(factor / startSmooth);
							factor = factor * factor * factor;
							factor = 1f;
							heights[i, j] = Mathf.Lerp(oldHeights[i,j], newHeight, factor);
						}
					}
					terrainData.SetHeights(newX, newZ, heights);
				}
			}
		}
	}
}

public class TerrainChanger
{
	class HeightData
	{
		List<float> heightValues = new List<float>();
		public float factor;
		public float GetHeight()
		{
			if (heightValues.Count > 0)
			{
				float height = 0;
				foreach (float value in heightValues)
				{
					height += value;
				}
				height /= heightValues.Count;
				return height;
			}
			return 0f;
		}

		public float GetMaxHeight()
		{
			if (heightValues.Count > 0)
			{
				float height = 0;
				foreach (float value in heightValues)
				{
					if (value > height)
					{
						height = value;
					}
				}
				return height;
			}
			return 0f;
		}

		public float GetMinHeight()
		{
			if (heightValues.Count > 0)
			{
				float height = Mathf.Infinity;
				foreach (float value in heightValues)
				{
					if (value < height)
					{
						height = value;
					}
				}
				return height;
			}
			return 0f;
		}

		public int GetHeightCount()
		{
			return heightValues.Count;
		}

		public void AddHeight(float height)
		{
			heightValues.Add(height);
		}

		public void ClearHeight()
		{
			heightValues.Clear();
		}
	}

	Terrain m_Terrain;
	TerrainData m_TerrainData;

	Dictionary<int, HeightData> m_Heights = new Dictionary<int, HeightData>();

	public TerrainChanger(Terrain terrain)
	{
		s_AreaPoints.Clear();
		s_LinePoints.Clear();
		if (null != terrain)
		{
			m_Terrain = terrain;
			m_TerrainData = terrain.terrainData;
		}else{
			throw new System.Exception("terrain is null");
		}
	}

	public float GetTerrainHeight(float y)
	{
		if (null != m_TerrainData)
		{
			return (y - m_Terrain.transform.position.y) / m_TerrainData.size.y;
		}
		return 0f;
	}

	public float GetWorldHeight(float height)
	{
		if (null != m_TerrainData)
		{
			return m_Terrain.transform.position.y + height * m_TerrainData.size.y;
		}
		return 0f;
	}

	public bool GetTerrainCoord(Vector3 worldPos, out int x, out int z)
	{
		if (null != m_TerrainData)
		{
			float minX = m_Terrain.transform.position.x;
			float minZ = m_Terrain.transform.position.z;
			float maxX = minX + m_TerrainData.size.x;
			float maxZ = minZ + m_TerrainData.size.z;

			bool inTerrain = worldPos.x >= minX && worldPos.x <= maxX && worldPos.z >= minZ && worldPos.z <= maxZ;
			float fx = (worldPos.x - m_Terrain.transform.position.x) / m_TerrainData.size.x;
			float fz = (worldPos.z - m_Terrain.transform.position.z) / m_TerrainData.size.z;
			x = Mathf.RoundToInt(fx * m_TerrainData.heightmapResolution);
			z = Mathf.RoundToInt(fz * m_TerrainData.heightmapResolution);
			return inTerrain;
		}
		x = 0;
		z = 0;
		return false;
	}

	public bool PointOnLine2D(Vector3 point, Vector3 startLine, Vector3 endLine, out float distance, out Vector3 clampPoint)
	{
		Vector3 dir = endLine - startLine;
		dir.y = 0;
		Vector3 dirNorm = dir.normalized;
		Vector3 diff = point - startLine;
		diff.y = 0f;
		distance = Vector3.Cross(dirNorm, diff).magnitude;

		if (Vector3.Dot(dirNorm, diff.normalized) > 0)
		{
			float sqrLen = diff.sqrMagnitude - distance * distance;
			if (sqrLen <= dir.sqrMagnitude)
			{
				clampPoint = startLine + dirNorm * Mathf.Sqrt(sqrLen);

				if (Utils.LineLineIntersection(startLine, endLine, clampPoint - Vector3.up * 1000f, clampPoint + Vector3.up * 1000f, out clampPoint, true, 1000f))
				{
					return true;
				}
				
			}
		}
		clampPoint = Vector3.zero;
		return false;
	}

	public Vector3 GetWorldCoord(int x, int z)
	{
		if (null != m_TerrainData 
			&& x >= 0 && x < m_TerrainData.heightmapResolution
			&& z >= 0 && z < m_TerrainData.heightmapResolution)
		{
			return new Vector3(
					m_Terrain.transform.position.x + m_TerrainData.size.x * (x / (float)(m_TerrainData.heightmapResolution - 1)),
					m_Terrain.transform.position.y + /*m_TerrainData.size.y **/ m_TerrainData.GetHeight(x, z),
					m_Terrain.transform.position.z + m_TerrainData.size.z * (z / (float)(m_TerrainData.heightmapResolution - 1))
				);
		}
		throw new System.Exception("Out of range x:" + x + " z:" + z);
	}

	public void SetHeight(int x, int z, float height, float factor)
	{
		if (null != m_TerrainData && IsCoordInTerrain(x,z))
		{
			s_LinePoints.Add(GetWorldCoord(x, z));
			int hash = GetCoordToHash(x, z);
			if (m_Heights.ContainsKey(hash))
			{
				HeightData heightData;
				m_Heights.TryGetValue(hash, out heightData);
				if (factor > heightData.factor)
				{
					heightData.factor = factor;
					heightData.ClearHeight();
					heightData.AddHeight(height);
				}
				else if (factor == heightData.factor)
				{
					heightData.AddHeight(height);
				}
			}
			else
			{
				HeightData heightData = new HeightData();
				heightData.AddHeight(height);
				heightData.factor = factor;
				m_Heights.Add(hash, heightData);
			}
		}
	}

	public void SetHeight(Vector3 worldPos, float height, float factor)
	{
		if (null != m_TerrainData)
		{
			int x, z;
			if (GetTerrainCoord(worldPos, out x, out z))
			{
				float newHeight = (height - m_Terrain.transform.position.y) / m_TerrainData.size.y;
				//newHeight = GetStepHeight(newHeight);

				SetHeight(x, z, newHeight, factor);
			}
		}
	}

	public void SetHeight(Vector3 worldPos, float height, float radius, float safeArea, AnimationCurve radiusCurve)
	{
		if (null != m_TerrainData)
		{
			int x, z;
			if (GetTerrainCoord(worldPos, out x, out z))
			{
				int w = Mathf.RoundToInt((radius * 2) / (m_TerrainData.size.x / m_TerrainData.heightmapResolution));
				int h = Mathf.RoundToInt((radius * 2) / (m_TerrainData.size.z / m_TerrainData.heightmapResolution));

				w = System.Math.Max(1, w);
				h = System.Math.Max(1, h);

				int tempX = x - w / 2;
				int tempZ = z - h / 2;
				int newX = System.Math.Max(0, tempX);
				int newZ = System.Math.Max(0, tempZ);
				int newW = System.Math.Min(m_TerrainData.heightmapResolution, w - (tempX - newX));
				int newH = System.Math.Min(m_TerrainData.heightmapResolution, h - (tempZ - newZ));

				//float[,] oldHeights = m_TerrainData.GetHeights(newX, newZ, newW, newH);
				float newHeight = (height - m_Terrain.transform.position.y) / m_TerrainData.size.y;
				//newHeight = GetStepHeight(newHeight);
				for (int i = 0; i < newH; ++i)
				{
					for (int j = 0; j < newW; ++j)
					{
						float distX = ((newX + j) - x);
						float distZ = ((newZ + i) - z);
						float dist = Mathf.Sqrt(distX * distX + distZ * distZ);
						//distX /= (w / 2f);
						//distZ /= (h / 2f);
						float factor = 1f;
						if (dist <= safeArea)
						{
							factor = 1f;
						}else{
							factor = 1f - Mathf.Clamp01((dist - safeArea) / (radius - safeArea));
							factor *= 0.9999f;
							//factor = 1f - Mathf.Sqrt(distX * distX + distZ * distZ);
						}

						if (factor > 0f)
						{
							if (null != radiusCurve)
							{
								factor = radiusCurve.Evaluate(factor);
							}
							SetHeight(j + newX, i + newZ, newHeight, factor);
						}
					}
				}
			}
		}
	}

	public static HashSet<Vector3> s_LinePoints = new HashSet<Vector3>();
	public static HashSet<Vector3> s_AreaPoints = new HashSet<Vector3>();
	public void SetHeightOnLine(Vector3 start, Vector3 end, float width, float smooth)
	{
		if (null != m_TerrainData)
		{
			Vector3 dirUp = start - end;
			dirUp.y = 0f;
			Vector3 tan = Vector3.Cross(dirUp.normalized, Vector3.up).normalized;
			int minX = int.MaxValue;
			int minZ = int.MaxValue;
			int maxX = int.MinValue;
			int maxZ = int.MinValue;

			int x, z;

			if (GetTerrainCoord(start + tan * width / 2f, out x, out z))
			{
				minX = System.Math.Min(minX, x);
				minZ = System.Math.Min(minZ, z);
				maxX = System.Math.Max(maxX, x);
				maxZ = System.Math.Max(maxZ, z);
			}
			if (GetTerrainCoord(start - tan * width / 2f, out x, out z))
			{
				minX = System.Math.Min(minX, x);
				minZ = System.Math.Min(minZ, z);
				maxX = System.Math.Max(maxX, x);
				maxZ = System.Math.Max(maxZ, z);
			}

			if (GetTerrainCoord(end + tan * width / 2f, out x, out z))
			{
				minX = System.Math.Min(minX, x);
				minZ = System.Math.Min(minZ, z);
				maxX = System.Math.Max(maxX, x);
				maxZ = System.Math.Max(maxZ, z);
			}
			if (GetTerrainCoord(end - tan * width / 2f, out x, out z))
			{
				minX = System.Math.Min(minX, x);
				minZ = System.Math.Min(minZ, z);
				maxX = System.Math.Max(maxX, x);
				maxZ = System.Math.Max(maxZ, z);
			}

			--minX;
			--minZ;
			++maxX;
			++maxZ;

			minX = System.Math.Min(m_TerrainData.heightmapResolution, System.Math.Max(0, minX));
			minZ = System.Math.Min(m_TerrainData.heightmapResolution, System.Math.Max(0, minZ));
			maxX = System.Math.Min(m_TerrainData.heightmapResolution, System.Math.Max(0, maxX));
			maxZ = System.Math.Min(m_TerrainData.heightmapResolution, System.Math.Max(0, maxZ));
            
			for (int i = minX; i <= maxX; ++i)
			{
				for (int j = minZ; j <= maxZ; ++j)
				{
					Vector3 worldPos = GetWorldCoord(i, j);
					worldPos.y = 0f;
					float dist;
					Vector3 clampPoint;
					s_AreaPoints.Add(worldPos);
					if (PointOnLine2D(worldPos, start, end, out dist, out clampPoint))
					{
						Vector3 tempDist = worldPos - clampPoint;
						tempDist.y = 0f;
						//if (tempDist.sqrMagnitude <= sqrWidth)
						{
							float newHeight = GetTerrainHeight((float)clampPoint.y);
							//newHeight = GetStepHeight(newHeight);
							worldPos.y = newHeight;
							//s_LinePoints.Add(worldPos);
							SetHeight(i, j, newHeight, 1f);

							/*SetHeight(i - 1, j, newHeight, 0.999f);
							SetHeight(i + 1, j, newHeight, 0.999f);
							SetHeight(i, j - 1, newHeight, 0.999f);
							SetHeight(i, j + 1, newHeight, 0.999f);*/
						}
					}
					else
					{
						//linePoints.Add(worldPos);
					}
				}
			}
		}
	}

	public void SetHeightOnCollider(Collider collider, float offset)
	{
		if (null != m_TerrainData && null != collider)
		{
			int minX, minZ;
			int maxX, maxZ;
			GetTerrainCoord(collider.bounds.min, out minX, out minZ);
			GetTerrainCoord(collider.bounds.max, out maxX, out maxZ);

			ClampTerrainPos(ref minX);
			ClampTerrainPos(ref minZ);
			ClampTerrainPos(ref maxX);
			ClampTerrainPos(ref maxZ);

			for (int x = minX; x <= maxX; ++x)
			{
				for (int z = minZ; z <= maxZ; ++z)
				{
					Vector3 worldPos = GetWorldCoord(x,z);
					RaycastHit hitInfo;
					bool found = false;
					if (collider.Raycast(new Ray(worldPos - Vector3.up * 1000f, Vector3.up), out hitInfo, Mathf.Infinity))
					{
						found = true;
					}
					else if (collider.Raycast(new Ray(worldPos + Vector3.up * 1000f, -Vector3.up), out hitInfo, Mathf.Infinity))
					{
						/*RaycastHit[] hitInfos = Physics.RaycastAll(new Ray(worldPos + Vector3.up * 1000f, -Vector3.up), Mathf.Infinity);
						if (null != hitInfos)
						{
							foreach (RaycastHit newHitInfo in hitInfos)
							{
								if (newHitInfo.collider == collider)
								{
									if (newHitInfo.point.y < hitInfo.point.y)
									{
										hitInfo = newHitInfo;
									}
								}
							}
						}*/
						/*RaycastHit newHitInfo = hitInfo;
						while (collider.Raycast(new Ray(newHitInfo.point - Vector3.up * 0.1f, -Vector3.up), out newHitInfo, Mathf.Infinity))
						{
							hitInfo = newHitInfo;
						}*/
						found = true;
					}
					if (found)
					{
						s_LinePoints.Add(hitInfo.point);

						float height = GetTerrainHeight(hitInfo.point.y - offset); 
						SetHeight(x, z, height, 1f);

						for (int i = -1; i <= 1; ++i)
						{
							for (int j = -1; j <= 1; ++j)
							{
								if (IsCoordInTerrain(x + i, z + j))
								{
									s_LinePoints.Add(GetWorldCoord(x + i, z + j));
									SetHeight(x + i, z + j, height, 0.999f);
								}
							}
						}
					}
					else
					{
						s_AreaPoints.Add(worldPos);
					}
				}
			}
		}
	}

	public int GetCoordToHash(int x, int z)
	{
		return z * m_TerrainData.heightmapResolution + x;
	}

	public void GetHashToCoord(int hash, out int x, out int z)
	{
		z = Mathf.FloorToInt(hash / m_TerrainData.heightmapResolution);
		x = hash - z * m_TerrainData.heightmapResolution;
	}

	public void Smooth(int smooth)
	{
		if (smooth <= 0) return;

		int posX, posZ;
		int minX = int.MaxValue;
		int minZ = int.MaxValue;
		int maxX = int.MinValue;
		int maxZ = int.MinValue;

		foreach (KeyValuePair<int,HeightData> changer in m_Heights)
		{
			GetHashToCoord(changer.Key, out posX, out posZ);
			minX = System.Math.Min(minX, posX);
			minZ = System.Math.Min(minZ, posZ);
			maxX = System.Math.Max(maxX, posX);
			maxZ = System.Math.Max(maxZ, posZ);
		}

		int startX = minX - smooth;
		int startZ = minZ - smooth;
		int w = smooth * 2 + 1 + maxX - minX;
		int h = smooth * 2 + 1 + maxZ - minZ;

		ClampTerrainPos(ref startX);
		ClampTerrainPos(ref startZ);
		ClampTerrainPos(ref w);
		ClampTerrainPos(ref h);

		float[,] factors = new float[w, h];
		float[,] heights = new float[w, h];

		float ignoreFactor = 1f;
		foreach (KeyValuePair<int,HeightData> changer in m_Heights)
		{
			GetHashToCoord(changer.Key, out posX, out posZ);
			int x = posX - startX;
			int z = posZ - startZ;
			if (x >= 0 && x < w && z >= 0 && z < h)
			{
				factors[posX - startX, posZ - startZ] = changer.Value.factor;
				heights[posX - startX, posZ - startZ] = changer.Value.GetHeight();
				ignoreFactor = System.Math.Min(ignoreFactor, changer.Value.factor);
			}
		}
		
		for (int s = 0; s < smooth; ++s)
		{
			for (int x = 0; x < w; ++x)
			{
				for (int z = 0; z < h; ++z)
				{
					if (factors[x, z] < ignoreFactor)
					{
						int fromX = System.Math.Max(0, x - 1);
						int toX = System.Math.Min(w - 1, x + 1);
						int fromZ = System.Math.Max(0, z - 1);
						int toZ = System.Math.Min(h - 1, z + 1);

						int factorCount = 0;
						int heightCount = 0;
						float newFactor = 0f;
						float newHeight = 0f;
						for (int i = fromX; i <= toX; ++i)
						{
							for (int j = fromZ; j <= toZ; ++j)
							{
								if (factors[i,j] > 0f) 
								{
									newHeight += heights[i, j];
									++heightCount;
								}
								newFactor += factors[i, j];
								++factorCount;
							}
						}
						if (heightCount > 0)
						{
							factors[x, z] = newFactor / factorCount;
							//heights[x, z] = GetStepHeight(newHeight / heightCount);
							heights[x, z] = Mathf.Clamp01(newHeight / heightCount);
						}
					}
				}
			}
		}

		for (int x = 0; x < w; ++x)
		{
			for (int z = 0; z < h; ++z)
			{
				if (factors[x,z] > 0f)
				{
					SetHeight(x + startX, z + startZ, heights[x, z], factors[x, z]);
				}
			}
		}
	}

	public void Apply(bool removeTree = false)
	{
		if (null != m_TerrainData)
		{
			float[,] heights = m_TerrainData.GetHeights(0, 0, m_TerrainData.heightmapResolution, m_TerrainData.heightmapResolution);
			int x, z;

			Dictionary<int, HeightData> importantHeights = new Dictionary<int, HeightData>();

			foreach (KeyValuePair<int,HeightData> changer in m_Heights)
			{
				GetHashToCoord(changer.Key, out x, out z);
				float fixedheight = changer.Value.GetHeight();
				//fixedheight = GetStepHeight(fixedheight);

				if (changer.Value.factor >= 0.9f)
				{
					importantHeights.Add(changer.Key, changer.Value);
				}
				
				float newHeight = Mathf.Lerp(heights[z, x], fixedheight, changer.Value.factor);
				//newHeight = GetStepHeight(newHeight);
				heights[z, x] = newHeight;

				//m_TerrainData.treeInstances

				//heights[z, x] = changer.Value.GetHeightCount() / 10f; // Debug
				//heights[z, x] = changer.Value.GetHeight(); // Debug
				//heights[z, x] = changer.Value.factor; // Debug
			}

			if (removeTree)
			{
				List<TreeInstance> newTreeInstances = new List<TreeInstance>();

				foreach (TreeInstance treeInstance in m_TerrainData.treeInstances)
				{
					int treeX, treeZ;
					GetTerrainCoord(treeInstance.position.Mult(m_TerrainData.size) + m_Terrain.transform.position, out treeX, out treeZ);
					bool onImportant = false;
					int keyX, keyZ;
					foreach (KeyValuePair<int, HeightData> changer in importantHeights)
					{
						GetHashToCoord(changer.Key, out keyX, out keyZ);
						if (((keyX - 1) <= treeX)
							&& ((keyX + 1) >= treeX)
							&& ((keyZ - 1) <= treeZ)
							&& ((keyZ + 1) >= treeZ))
						{
							onImportant = true;
							break;
						}
					}
					if (!onImportant)
					{
						newTreeInstances.Add(treeInstance);
					}
				}
				m_TerrainData.treeInstances = newTreeInstances.ToArray();
			}

			
			m_TerrainData.SetHeights(0,0, heights);
		}
	}

	public float GetStepHeight(float height)
	{
		float step = 1f / m_TerrainData.heightmapHeight;
		return Mathf.Clamp01((Mathf.Floor(height / step)) * step);
	}

	public void ClampTerrainPos(ref int XOrZ)
	{
		XOrZ = System.Math.Min(m_TerrainData.heightmapResolution - 1, System.Math.Max(0, XOrZ));
	}

	public bool IsCoordInTerrain(int x, int z)
	{
		return x >= 0 && x < m_TerrainData.heightmapResolution && z >= 0 && z < m_TerrainData.heightmapResolution;
	}

	public static void OptimizeHeight(TerrainData terrainData, float safe = 0.1f)
	{
		float maxHeight = 0f;

		float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
		
		for (int x = 0; x < terrainData.heightmapResolution; ++x)
		{
			for (int z = 0; z < terrainData.heightmapResolution; ++z)
			{
				if (heights[x,z] > maxHeight)
				{
					maxHeight = heights[x, z];
				}
			}
		}

		float newHeight = Mathf.Clamp01(maxHeight + 0.1f) * terrainData.size.y;
		float scaleRatio = terrainData.size.y / newHeight;

		if (newHeight != terrainData.heightmapHeight)
		{
			for (int x = 0; x < terrainData.heightmapResolution; ++x)
			{
				for (int z = 0; z < terrainData.heightmapResolution; ++z)
				{
					heights[x, z] = heights[x, z] * scaleRatio;
				}
			}
			terrainData.size = new Vector3(terrainData.size.x, newHeight, terrainData.size.z);
			terrainData.SetHeights(0, 0, heights);
		}
	}
}
