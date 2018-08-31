using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Terrain/Ramp Brush")]
public class RampBrush : MonoBehaviour
{
	public void OnDrawGizmos()
	{
		if (this.turnBrushOnVar)
		{
			Terrain x = (Terrain)base.GetComponent(typeof(Terrain));
			if (x == null)
			{
				return;
			}
			Gizmos.color = Color.cyan;
			float num = this.brushSize / 4f;
			Gizmos.DrawLine(this.brushPosition + new Vector3(-num, 0f, 0f), this.brushPosition + new Vector3(num, 0f, 0f));
			Gizmos.DrawLine(this.brushPosition + new Vector3(0f, -num, 0f), this.brushPosition + new Vector3(0f, num, 0f));
			Gizmos.DrawLine(this.brushPosition + new Vector3(0f, 0f, -num), this.brushPosition + new Vector3(0f, 0f, num));
			Gizmos.DrawWireCube(this.brushPosition, new Vector3(this.brushSize, 0f, this.brushSize));
			Gizmos.DrawWireSphere(this.brushPosition, this.brushSize / 2f);
			if (!this.multiPoint)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawWireSphere(this.beginRamp, this.brushSize / 2f);
			}
			else
			{
				Gizmos.color = Color.magenta;
				for (int i = 0; i < this.controlPoints.Count; i++)
				{
					Gizmos.DrawWireSphere(this.controlPoints[i], this.brushSize / 2f);
				}
				if (this.controlPoints.Count > 2)
				{
					double num2 = 1.0 / (((double)this.controlPoints.Count - 1.0) * 8.0) - 1E-14;
					this.calculateDistBetweenPoints(this.controlPoints);
					Ray ray = this.parameterizedLine(0f, this.controlPoints, null);
					double num3 = num2;
					int num4 = 0;
					while (num3 <= 1.0 && num4 < 1000)
					{
						Ray ray2 = this.parameterizedLine((float)num3, this.controlPoints, null);
						Gizmos.DrawLine(ray.origin, ray2.origin);
						ray = ray2;
						num3 += num2;
						num4++;
					}
				}
			}
		}
	}

	public int[] terrainCordsToBitmap(TerrainData terData, Vector3 v)
	{
		float num = (float)terData.heightmapWidth;
		float num2 = (float)terData.heightmapHeight;
		Vector3 size = terData.size;
		int num3 = (int)Mathf.Floor(num / size.x * v.x);
		int num4 = (int)Mathf.Floor(num2 / size.z * v.z);
		return new int[]
		{
			num4,
			num3
		};
	}

	public float[] bitmapCordsToTerrain(TerrainData terData, int x, int y)
	{
		int heightmapWidth = terData.heightmapWidth;
		int heightmapHeight = terData.heightmapHeight;
		Vector3 size = terData.size;
		float num = (float)x * (size.z / (float)heightmapHeight);
		float num2 = (float)y * (size.x / (float)heightmapWidth);
		return new float[]
		{
			num2,
			num
		};
	}

	public void toggleBrushOn()
	{
		if (this.turnBrushOnVar)
		{
			this.turnBrushOnVar = false;
		}
		else
		{
			this.turnBrushOnVar = true;
		}
	}

	public void rampBrush()
	{
		Terrain terrain = (Terrain)base.GetComponent(typeof(Terrain));
		if (terrain == null)
		{
			Debug.LogError("No terrain component on this GameObject");
			return;
		}
		try
		{
			TerrainData terrainData = terrain.terrainData;
			int heightmapWidth = terrainData.heightmapWidth;
			int heightmapHeight = terrainData.heightmapHeight;
			Vector3 size = terrainData.size;
			if (this.VERBOSE)
			{
				Debug.Log(string.Concat(new object[]
				{
					"terrainData heightmapHeight/heightmapWidth:",
					heightmapWidth,
					" ",
					heightmapWidth
				}));
			}
			if (this.VERBOSE)
			{
				Debug.Log("terrainData heightMapResolution:" + terrainData.heightmapResolution);
			}
			if (this.VERBOSE)
			{
				Debug.Log("terrainData size:" + terrainData.size);
			}
			Vector3 localScale = base.transform.localScale;
			base.transform.localScale = new Vector3(1f, 1f, 1f);
			Vector3 vector = this.beginRamp - base.transform.position;
			Vector3 vector2 = this.endRamp - base.transform.position;
			base.transform.localScale = localScale;
			int num = (int)Mathf.Floor((float)heightmapWidth / size.z * this.brushSize);
			int num2 = (int)Mathf.Floor((float)heightmapHeight / size.x * this.brushSize);
			int[] array = this.terrainCordsToBitmap(terrainData, vector);
			int[] array2 = this.terrainCordsToBitmap(terrainData, vector2);
			if (array[0] < 0 || array2[0] < 0 || array[1] < 0 || array2[1] < 0 || array[0] >= heightmapWidth || array2[0] >= heightmapWidth || array[1] >= heightmapHeight || array2[1] >= heightmapHeight)
			{
				Debug.LogError("The start point or the end point was out of bounds. Make sure the gizmo is over the terrain before setting the start and end points.Note: that sometimes Unity does not update the collider after changing settings in the 'Set Resolution' dialog. Entering play mode should reset the collider.");
			}
			else
			{
				double num3 = Math.Sqrt((double)((array2[0] - array[0]) * (array2[0] - array[0]) + (array2[1] - array[1]) * (array2[1] - array[1])));
				float[,] heights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);
				vector2.y = heights[array2[0], array2[1]];
				vector.y = heights[array[0], array[1]];
				Vector3 vector3 = vector2 - vector;
				Vector3 rhs = new Vector3(-vector3.z, 0f, vector3.x);
				Vector3 vector4 = Vector3.Cross(vector3, rhs);
				vector4.Normalize();
				Vector3 vector5 = new Vector3(vector3.x, 0f, vector3.z);
				float num4;
				if (this.brushSize < 15f)
				{
					num4 = this.brushSize / 6f / vector3.magnitude;
				}
				else
				{
					num4 = (float)(1.0 / num3 * (double)this.brushSampleDensity);
				}
				if (this.VERBOSE)
				{
					float[] array3 = this.bitmapCordsToTerrain(terrainData, array[0], array[1]);
					Debug.Log("Local Begin Pos:" + vector);
					Debug.Log(string.Concat(new object[]
					{
						"pixel begin coord:",
						array[0],
						" ",
						array[0]
					}));
					Debug.Log(string.Concat(new object[]
					{
						"Local begin Pos Rev Transformed:",
						array3[0],
						" ",
						array3[1]
					}));
					array3 = this.bitmapCordsToTerrain(terrainData, array2[0], array2[1]);
					Debug.Log("Local End Pos:" + vector2);
					Debug.Log(string.Concat(new object[]
					{
						"pixel End coord:",
						array2[0],
						" ",
						array2[1]
					}));
					Debug.Log(string.Concat(new object[]
					{
						"Local End Pos Rev Transformed:",
						array3[0],
						" ",
						array3[1]
					}));
					Debug.Log(string.Concat(new object[]
					{
						"Sample Width/height: ",
						num,
						" ",
						num2
					}));
					Debug.Log("Brush Width: " + num4);
				}
				for (float num5 = 0f; num5 <= 1f; num5 += num4)
				{
					Vector3 v = vector + num5 * vector3;
					int[] array4 = this.terrainCordsToBitmap(terrainData, v);
					int num6 = array4[0] - num / 2;
					int num7 = array4[1] - num2 / 2;
					float[,] array5 = new float[num, num2];
					for (int i = 0; i < num; i++)
					{
						for (int j = 0; j < num2; j++)
						{
							if (num6 + i >= 0 && num7 + j >= 0 && num6 + i < heightmapWidth && num7 + j < heightmapHeight)
							{
								array5[i, j] = heights[num6 + i, num7 + j];
							}
							else
							{
								array5[i, j] = 0f;
							}
						}
					}
					num = array5.GetLength(0);
					num2 = array5.GetLength(1);
					float[,] array6 = (float[,])array5.Clone();
					for (int k = 0; k < num; k++)
					{
						for (int l = 0; l < num2; l++)
						{
							float[] array7 = this.bitmapCordsToTerrain(terrainData, num6 + k, num7 + l);
							bool flag = false;
							if (vector5.x * (array7[0] - vector.x) + vector5.z * (array7[1] - vector.z) < 0f)
							{
								flag = true;
							}
							else if (-vector5.x * (array7[0] - vector2.x) - vector5.z * (array7[1] - vector2.z) < 0f)
							{
								flag = true;
							}
							if (!flag)
							{
								array6[k, l] = vector.y - (vector4.x * (array7[0] - vector.x) + vector4.z * (array7[1] - vector.z)) / vector4.y;
							}
						}
					}
					float num8 = (float)num / 2f;
					for (int m = 0; m < num; m++)
					{
						for (int n = 0; n < num2; n++)
						{
							float num9 = array6[m, n];
							float num10 = array5[m, n];
							float num11 = Vector2.Distance(new Vector2((float)m, (float)n), new Vector2(num8, num8));
							float num12 = 1f - (num11 - (num8 - num8 * this.brushSoftness)) / (num8 * this.brushSoftness);
							if (num12 < 0f)
							{
								num12 = 0f;
							}
							else if (num12 > 1f)
							{
								num12 = 1f;
							}
							num12 *= this.brushOpacity;
							float num13 = num9 * num12 + num10 * (1f - num12);
							array5[m, n] = num13;
						}
					}
					for (int num14 = 0; num14 < num; num14++)
					{
						for (int num15 = 0; num15 < num2; num15++)
						{
							if (num6 + num14 >= 0 && num7 + num15 >= 0 && num6 + num14 < heightmapWidth && num7 + num15 < heightmapHeight)
							{
								heights[num6 + num14, num7 + num15] = array5[num14, num15];
							}
						}
					}
				}
				terrainData.SetHeights(0, 0, heights);
			}
		}
		catch (Exception arg)
		{
			Debug.LogError("A brush error occurred: " + arg);
		}
	}

	public void StrokePath()
	{
		this._StrokePath();
	}

	public void _StrokePath()
	{
		Terrain terrain = (Terrain)base.GetComponent(typeof(Terrain));
		if (terrain == null)
		{
			Debug.LogError("No terrain component on this GameObject");
			return;
		}
		int num = 0;
		int num2 = 0;
		try
		{
			TerrainData terrainData = terrain.terrainData;
			int heightmapWidth = terrainData.heightmapWidth;
			int heightmapHeight = terrainData.heightmapHeight;
			Vector3 size = terrainData.size;
			if (this.VERBOSE)
			{
				Debug.Log(string.Concat(new object[]
				{
					"terrainData heightmapHeight/heightmapWidth:",
					heightmapWidth,
					" ",
					heightmapWidth
				}));
			}
			if (this.VERBOSE)
			{
				Debug.Log("terrainData heightMapResolution:" + terrainData.heightmapResolution);
			}
			if (this.VERBOSE)
			{
				Debug.Log("terrainData size:" + terrainData.size);
			}
			Vector3 localScale = base.transform.localScale;
			base.transform.localScale = new Vector3(1f, 1f, 1f);
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < this.controlPoints.Count; i++)
			{
				list.Add(this.controlPoints[i] - base.transform.position);
			}
			base.transform.localScale = localScale;
			for (int j = 0; j < list.Count; j++)
			{
				int[] array = this.terrainCordsToBitmap(terrainData, list[j]);
				if (array[0] < 0 || array[1] < 0 || array[0] >= heightmapWidth || array[1] >= heightmapHeight)
				{
					Debug.LogError("The start point or the end point was out of bounds. Make sure the gizmo is over the terrain before setting the start and end points.Note: that sometimes Unity does not update the collider after changing settings in the 'Set Resolution' dialog. Entering play mode should reset the collider.");
					return;
				}
			}
			int num3 = (int)Mathf.Floor((float)heightmapWidth / size.z * this.brushSize);
			int num4 = (int)Mathf.Floor((float)heightmapHeight / size.x * this.brushSize);
			float[,] heights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);
			for (int k = 0; k < list.Count; k++)
			{
				int[] array2 = this.terrainCordsToBitmap(terrainData, list[k]);
				Vector3 value = list[k];
				value.y = heights[array2[0], array2[1]];
				list[k] = value;
			}
			this.calculateDistBetweenPoints(list);
			this.calculateDistBetweenPointsInPixels(list, terrainData);
			float num5 = this.brushSampleDensity / this._totalLengthPixels;
			float num6 = this.brushSize / this._totalLengthPixels;
			Debug.Log(string.Concat(new object[]
			{
				"Sample w ",
				num3,
				" h ",
				num4
			}));
			Debug.Log("parameterized brush width " + num6);
			if (num6 > 0.5f)
			{
				num6 = 0.5f;
			}
			if (this.VERBOSE)
			{
				for (int l = 0; l < list.Count; l++)
				{
					int[] array3 = this.terrainCordsToBitmap(terrainData, list[l]);
					float[] array4 = this.bitmapCordsToTerrain(terrainData, array3[0], array3[1]);
					Debug.Log(l + " Local control Pos:" + list[l]);
					Debug.Log(string.Concat(new object[]
					{
						l,
						" pixel begin coord:",
						array3[0],
						" ",
						array3[0]
					}));
					Debug.Log(string.Concat(new object[]
					{
						l,
						" Local begin Pos Rev Transformed:",
						array4[0],
						" ",
						array4[1]
					}));
				}
				Debug.Log("parameterized brush width " + num6);
			}
			StringBuilder message = new StringBuilder();
			for (float num7 = 0f; num7 <= 1f; num7 += num5)
			{
				Ray ray = this.parameterizedLine(num7, list, null);
				Vector3 vector = Vector3.Cross(new Vector3(-ray.direction.z, 0f, ray.direction.x), ray.direction);
				vector.Normalize();
				if (this.spacingJitter > 0f)
				{
					float f = 6.28318548f * UnityEngine.Random.value;
					float num8 = UnityEngine.Random.value + UnityEngine.Random.value;
					float num9;
					if (num8 > 1f)
					{
						num9 = 2f - num8;
					}
					else
					{
						num9 = num8;
					}
					num9 *= this.spacingJitter * this.brushSize;
					Vector3 vector2 = new Vector3(num9 * Mathf.Cos(f), 0f, num9 * Mathf.Sin(f));
					if (this.VERBOSE)
					{
						Debug.Log(string.Concat(new object[]
						{
							"jittering by ",
							vector2,
							" dir ",
							ray.direction,
							" n ",
							vector
						}));
					}
					Plane plane = new Plane(vector, ray.origin);
					Ray ray2 = new Ray(ray.origin + vector2, Vector3.up);
					float d;
					if (plane.Raycast(ray2, out d))
					{
						Vector3 origin = ray2.origin + ray2.direction * d;
						ray.origin = origin;
					}
				}
				Plane plane2 = new Plane((list[0] - list[1]).normalized, list[0]);
				Plane plane3 = new Plane((list[list.Count - 1] - list[list.Count - 2]).normalized, list[list.Count - 1]);
				int[] array5 = this.terrainCordsToBitmap(terrainData, ray.origin);
				num = array5[0] - num3 / 2;
				num2 = array5[1] - num4 / 2;
				float[,] array6 = new float[num3, num4];
				for (int m = 0; m < num3; m++)
				{
					for (int n = 0; n < num4; n++)
					{
						if (num + m >= 0 && num2 + n >= 0 && num + m < heightmapWidth && num2 + n < heightmapHeight)
						{
							array6[m, n] = heights[num + m, num2 + n];
						}
						else
						{
							array6[m, n] = 0f;
						}
					}
				}
				float[,] array7 = (float[,])array6.Clone();
				for (int num10 = 0; num10 < num3; num10++)
				{
					for (int num11 = 0; num11 < num4; num11++)
					{
						float[] array8 = this.bitmapCordsToTerrain(terrainData, num + num10, num2 + num11);
						Vector3 vector3 = new Vector3(array8[0], 0f, array8[1]);
						bool flag = false;
						if (plane2.GetSide(vector3) && num7 < num6 / 2f)
						{
							flag = true;
						}
						else if (plane3.GetSide(vector3) && num7 > 1f - num6 / 2f)
						{
							flag = true;
						}
						if (!flag)
						{
							Plane plane4 = new Plane(vector, ray.origin);
							Ray ray3 = new Ray(vector3, Vector3.up);
							float num12;
							if (plane4.Raycast(ray3, out num12))
							{
								array7[num10, num11] = ray3.origin.y + ray3.direction.y * num12;
							}
						}
					}
				}
				float num13 = Mathf.Min((float)num4 / 2f, (float)num3 / 2f);
				for (int num14 = 0; num14 < num3; num14++)
				{
					for (int num15 = 0; num15 < num4; num15++)
					{
						float num16 = array7[num14, num15];
						float num17 = array6[num14, num15];
						float num18 = Vector2.Distance(new Vector2((float)num14, (float)num15), new Vector2(num13, num13));
						float num19 = (1f - num18 / num13) / this.brushSoftness;
						if (num19 < 0f)
						{
							num19 = 0f;
						}
						else if (num19 > 1f)
						{
							num19 = 1f;
						}
						num19 *= this.brushOpacity;
						float num20 = num16 * num19 + num17 * (1f - num19);
						array6[num14, num15] = num20;
					}
				}
				for (int num21 = 0; num21 < num3; num21++)
				{
					for (int num22 = 0; num22 < num4; num22++)
					{
						if (num + num21 >= 0 && num2 + num22 >= 0 && num + num21 < heightmapWidth && num2 + num22 < heightmapHeight)
						{
							heights[num + num21, num2 + num22] = array6[num21, num22];
						}
					}
				}
			}
			Debug.Log(message);
			terrainData.SetHeights(0, 0, heights);
		}
		catch (Exception arg)
		{
			Debug.LogError("A brush error occurred: " + arg);
		}
	}

	private void calculateDistBetweenPoints(List<Vector3> cps)
	{
		this._distBetweenPoints.Clear();
		this._totalLength = 0f;
		for (int i = 1; i < cps.Count; i++)
		{
			this._distBetweenPoints.Add((cps[i] - cps[i - 1]).magnitude);
			this._totalLength += this._distBetweenPoints[this._distBetweenPoints.Count - 1];
		}
	}

	private void calculateDistBetweenPointsInPixels(List<Vector3> cps, TerrainData terData)
	{
		this._totalLengthPixels = 0f;
		int[] array = this.terrainCordsToBitmap(terData, cps[0]);
		for (int i = 1; i < cps.Count; i++)
		{
			int[] array2 = this.terrainCordsToBitmap(terData, cps[i]);
			this._totalLengthPixels += Mathf.Sqrt((float)((array2[0] - array[0]) * (array2[0] - array[0]) + (array2[1] - array[1]) * (array2[1] - array[1])));
			array = array2;
		}
	}

	private Ray parameterizedLine(float t, List<Vector3> cps, StringBuilder sb = null)
	{
		if (cps.Count < 2)
		{
			Debug.LogError("Less than two control points.");
			return default(Ray);
		}
		if (t < 0f)
		{
			t = 0f;
		}
		if (t >= 1f)
		{
			t = 1f;
		}
		Vector3[] array = new Vector3[cps.Count + 2];
		for (int i = 0; i < cps.Count; i++)
		{
			array[i + 1] = cps[i];
		}
		array[0] = 2f * cps[0] - cps[1];
		array[array.Length - 1] = 2f * cps[cps.Count - 1] - cps[cps.Count - 2];
		float num = t * this._totalLength;
		int num2 = 0;
		float num3 = 0f;
		bool flag = false;
		float num4 = 0f;
		while (!flag)
		{
			if (num4 + this._distBetweenPoints[num2] < num)
			{
				num4 += this._distBetweenPoints[num2];
				if (num2 < this.controlPoints.Count - 2)
				{
					num2++;
				}
				else
				{
					flag = true;
					num3 = 1f;
				}
			}
			else
			{
				flag = true;
				num3 = (num - num4) / this._distBetweenPoints[num2];
			}
		}
		if (num2 >= this.controlPoints.Count - 1)
		{
			num2--;
		}
		if (num3 > 1f)
		{
			num3 = 1f;
		}
		num2++;
		if (num2 + 2 > array.Length - 1)
		{
			Debug.LogError("Off end=" + t);
		}
		if (sb != null)
		{
			sb.AppendFormat("t={0} cpIdx={1} nt={2}\n", t, num2, num3);
		}
		Vector3 a = array[num2 - 1];
		Vector3 a2 = array[num2];
		Vector3 vector = array[num2 + 1];
		Vector3 vector2 = array[num2 + 2];
		float d = num3 * num3;
		float d2 = num3 * num3 * num3;
		Vector3 origin = 0.5f * (2f * a2 + (-a + vector) * num3 + (2f * a - 5f * a2 + 4f * vector - vector2) * d + (-a + 3f * a2 - 3f * vector + vector2) * d2);
		Ray result = new Ray(origin, (0.5f * (-a + vector + num3 * (4f * a - 10f * a2 + 8f * vector - 2f * vector2) + d * (-3f * a + 9f * a2 - 9f * vector + 3f * vector2))).normalized);
		return result;
	}

	private bool VERBOSE;

	public bool brushOn;

	public bool turnBrushOnVar;

	public bool isBrushHidden;

	public Vector3 brushPosition;

	public Vector3 beginRamp;

	public Vector3 endRamp;

	public float brushSize = 50f;

	public float brushOpacity = 1f;

	public float brushSoftness = 0.5f;

	public float brushSampleDensity = 4f;

	public bool shiftProcessed = true;

	public Vector3 backupVector;

	public int numSubDivPerSeg = 10;

	public float spacingJitter;

	public float sizeJitter;

	public bool multiPoint;

	public List<Vector3> controlPoints = new List<Vector3>();

	private List<float> _distBetweenPoints = new List<float>();

	private float _totalLength;

	private float _totalLengthPixels;
}
