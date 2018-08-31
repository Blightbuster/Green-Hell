using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
public class RamSpline : MonoBehaviour
{
	public void Start()
	{
		this.GenerateSpline(null);
	}

	public static RamSpline CreateSpline(Material splineMaterial = null, List<Vector4> positions = null)
	{
		GameObject gameObject = new GameObject("RamSpline");
		RamSpline ramSpline = gameObject.AddComponent<RamSpline>();
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.receiveShadows = false;
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		if (splineMaterial != null)
		{
			meshRenderer.sharedMaterial = splineMaterial;
		}
		if (positions != null)
		{
			for (int i = 0; i < positions.Count; i++)
			{
				ramSpline.AddPoint(positions[i]);
			}
		}
		return ramSpline;
	}

	public void AddPoint(Vector4 position)
	{
		if (position.w == 0f)
		{
			if (this.controlPoints.Count > 0)
			{
				position.w = this.controlPoints[this.controlPoints.Count - 1].w;
			}
			else
			{
				position.w = this.width;
			}
		}
		this.controlPointsRotations.Add(Quaternion.identity);
		this.controlPoints.Add(position);
		this.controlPointsSnap.Add(0f);
		this.controlPointsMeshCurves.Add(new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 0f)
		}));
	}

	public void AddPointAfter(int i)
	{
		Vector4 vector = this.controlPoints[i];
		if (i < this.controlPoints.Count - 1 && this.controlPoints.Count > i + 1)
		{
			Vector4 vector2 = this.controlPoints[i + 1];
			if (Vector3.Distance(vector2, vector) > 0f)
			{
				vector = (vector + vector2) * 0.5f;
			}
			else
			{
				vector.x += 1f;
			}
		}
		else if (this.controlPoints.Count > 1 && i == this.controlPoints.Count - 1)
		{
			Vector4 vector3 = this.controlPoints[i - 1];
			if (Vector3.Distance(vector3, vector) > 0f)
			{
				vector += vector - vector3;
			}
			else
			{
				vector.x += 1f;
			}
		}
		else
		{
			vector.x += 1f;
		}
		this.controlPoints.Insert(i + 1, vector);
		this.controlPointsRotations.Insert(i + 1, Quaternion.identity);
		this.controlPointsSnap.Insert(i + 1, 0f);
		this.controlPointsMeshCurves.Insert(i + 1, new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 0f)
		}));
	}

	public void ChangePointPosition(int i, Vector3 position)
	{
		this.ChangePointPosition(i, new Vector4(position.x, position.y, position.z, 0f));
	}

	public void ChangePointPosition(int i, Vector4 position)
	{
		Vector4 vector = this.controlPoints[i];
		if (position.w == 0f)
		{
			position.w = vector.w;
		}
		this.controlPoints[i] = position;
	}

	public void RemovePoint(int i)
	{
		if (i < this.controlPoints.Count)
		{
			this.controlPoints.RemoveAt(i);
			this.controlPointsRotations.RemoveAt(i);
			this.controlPointsMeshCurves.RemoveAt(i);
			this.controlPointsSnap.RemoveAt(i);
		}
	}

	public void RemovePoints(int fromID = -1)
	{
		int num = this.controlPoints.Count - 1;
		for (int i = num; i > fromID; i--)
		{
			this.RemovePoint(i);
		}
	}

	public void GenerateBeginningParentBased()
	{
		this.vertsInShape = (int)Mathf.Round((float)(this.beginningSpline.vertsInShape - 1) * (this.beginningMaxWidth - this.beginningMinWidth) + 1f);
		if (this.vertsInShape < 1)
		{
			this.vertsInShape = 1;
		}
		this.beginningConnectionID = this.beginningSpline.points.Count - 1;
		float num = this.beginningSpline.controlPoints[this.beginningSpline.controlPoints.Count - 1].w;
		num *= this.beginningMaxWidth - this.beginningMinWidth;
		Vector4 value = Vector3.Lerp(this.beginningSpline.pointsDown[this.beginningConnectionID], this.beginningSpline.pointsUp[this.beginningConnectionID], this.beginningMinWidth + (this.beginningMaxWidth - this.beginningMinWidth) * 0.5f) + this.beginningSpline.transform.position - base.transform.position;
		value.w = num;
		this.controlPoints[0] = value;
		if (!this.uvScaleOverride)
		{
			this.uvScale = this.beginningSpline.uvScale;
		}
	}

	public void GenerateEndingParentBased()
	{
		if (this.beginningSpline == null)
		{
			this.vertsInShape = (int)Mathf.Round((float)(this.endingSpline.vertsInShape - 1) * (this.endingMaxWidth - this.endingMinWidth) + 1f);
			if (this.vertsInShape < 1)
			{
				this.vertsInShape = 1;
			}
		}
		this.endingConnectionID = 0;
		float num = this.endingSpline.controlPoints[0].w;
		num *= this.endingMaxWidth - this.endingMinWidth;
		Vector4 value = Vector3.Lerp(this.endingSpline.pointsDown[this.endingConnectionID], this.endingSpline.pointsUp[this.endingConnectionID], this.endingMinWidth + (this.endingMaxWidth - this.endingMinWidth) * 0.5f) + this.endingSpline.transform.position - base.transform.position;
		value.w = num;
		this.controlPoints[this.controlPoints.Count - 1] = value;
	}

	public void GenerateSpline(List<RamSpline> generatedSplines = null)
	{
		generatedSplines = new List<RamSpline>();
		if (this.beginningSpline)
		{
			this.GenerateBeginningParentBased();
		}
		if (this.endingSpline)
		{
			this.GenerateEndingParentBased();
		}
		List<Vector4> list = new List<Vector4>();
		for (int i = 0; i < this.controlPoints.Count; i++)
		{
			if (i > 0)
			{
				if (Vector3.Distance(this.controlPoints[i], this.controlPoints[i - 1]) > 0f)
				{
					list.Add(this.controlPoints[i]);
				}
			}
			else
			{
				list.Add(this.controlPoints[i]);
			}
		}
		Mesh mesh = new Mesh();
		this.meshfilter = base.GetComponent<MeshFilter>();
		if (list.Count < 2)
		{
			mesh.Clear();
			this.meshfilter.mesh = mesh;
			return;
		}
		this.controlPointsOrientation = new List<Quaternion>();
		this.lerpValues.Clear();
		this.snaps.Clear();
		this.points.Clear();
		this.pointsUp.Clear();
		this.pointsDown.Clear();
		this.orientations.Clear();
		this.tangents.Clear();
		this.normalsList.Clear();
		this.widths.Clear();
		this.controlPointsUp.Clear();
		this.controlPointsDown.Clear();
		this.verticesBeginning.Clear();
		this.verticesEnding.Clear();
		this.normalsBeginning.Clear();
		this.normalsEnding.Clear();
		if (this.beginningSpline != null && this.beginningSpline.controlPointsRotations.Count > 0)
		{
			this.controlPointsRotations[0] = Quaternion.identity;
		}
		if (this.endingSpline != null && this.endingSpline.controlPointsRotations.Count > 0)
		{
			this.controlPointsRotations[this.controlPointsRotations.Count - 1] = Quaternion.identity;
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (j <= list.Count - 2)
			{
				this.CalculateCatmullRomSideSplines(list, j);
			}
		}
		if (this.beginningSpline != null && this.beginningSpline.controlPointsRotations.Count > 0)
		{
			this.controlPointsRotations[0] = Quaternion.Inverse(this.controlPointsOrientation[0]) * this.beginningSpline.controlPointsOrientation[this.beginningSpline.controlPointsOrientation.Count - 1];
		}
		if (this.endingSpline != null && this.endingSpline.controlPointsRotations.Count > 0)
		{
			this.controlPointsRotations[this.controlPointsRotations.Count - 1] = Quaternion.Inverse(this.controlPointsOrientation[this.controlPointsOrientation.Count - 1]) * this.endingSpline.controlPointsOrientation[0];
		}
		this.controlPointsOrientation = new List<Quaternion>();
		this.controlPointsUp.Clear();
		this.controlPointsDown.Clear();
		for (int k = 0; k < list.Count; k++)
		{
			if (k <= list.Count - 2)
			{
				this.CalculateCatmullRomSideSplines(list, k);
			}
		}
		for (int l = 0; l < list.Count; l++)
		{
			if (l <= list.Count - 2)
			{
				this.CalculateCatmullRomSplineParameters(list, l, false);
			}
		}
		for (int m = 0; m < this.controlPointsUp.Count; m++)
		{
			if (m <= this.controlPointsUp.Count - 2)
			{
				this.CalculateCatmullRomSpline(this.controlPointsUp, m, ref this.pointsUp);
			}
		}
		for (int n = 0; n < this.controlPointsDown.Count; n++)
		{
			if (n <= this.controlPointsDown.Count - 2)
			{
				this.CalculateCatmullRomSpline(this.controlPointsDown, n, ref this.pointsDown);
			}
		}
		this.GenerateMesh(ref mesh);
		if (generatedSplines != null)
		{
			generatedSplines.Add(this);
			foreach (RamSpline ramSpline in this.beginnigChildSplines)
			{
				if (ramSpline != null && !generatedSplines.Contains(ramSpline) && (ramSpline.beginningSpline == this || ramSpline.endingSpline == this))
				{
					ramSpline.GenerateSpline(generatedSplines);
				}
			}
			foreach (RamSpline ramSpline2 in this.endingChildSplines)
			{
				if (ramSpline2 != null && !generatedSplines.Contains(ramSpline2) && (ramSpline2.beginningSpline == this || ramSpline2.endingSpline == this))
				{
					ramSpline2.GenerateSpline(generatedSplines);
				}
			}
		}
		this.GenerateMeshConvexColliderTrigger();
	}

	private void CalculateCatmullRomSideSplines(List<Vector4> controlPoints, int pos)
	{
		Vector3 p = controlPoints[pos];
		Vector3 p2 = controlPoints[pos];
		Vector3 p3 = controlPoints[this.ClampListPos(pos + 1)];
		Vector3 p4 = controlPoints[this.ClampListPos(pos + 1)];
		if (pos > 0)
		{
			p = controlPoints[this.ClampListPos(pos - 1)];
		}
		if (pos < controlPoints.Count - 2)
		{
			p4 = controlPoints[this.ClampListPos(pos + 2)];
		}
		int num = 0;
		if (pos == controlPoints.Count - 2)
		{
			num = 1;
		}
		for (int i = 0; i <= num; i++)
		{
			Vector3 catmullRomPosition = this.GetCatmullRomPosition((float)i, p, p2, p3, p4);
			Vector3 normalized = this.GetCatmullRomTangent((float)i, p, p2, p3, p4).normalized;
			Vector3 normalized2 = this.CalculateNormal(normalized, Vector3.up).normalized;
			Quaternion quaternion;
			if (normalized2 == normalized && normalized2 == Vector3.zero)
			{
				quaternion = Quaternion.identity;
			}
			else
			{
				quaternion = Quaternion.LookRotation(normalized, normalized2);
			}
			quaternion *= Quaternion.Lerp(this.controlPointsRotations[pos], this.controlPointsRotations[this.ClampListPos(pos + 1)], (float)i);
			this.controlPointsOrientation.Add(quaternion);
			Vector3 item = catmullRomPosition + quaternion * (0.5f * controlPoints[pos + i].w * Vector3.right);
			Vector3 item2 = catmullRomPosition + quaternion * (0.5f * controlPoints[pos + i].w * Vector3.left);
			this.controlPointsUp.Add(item);
			this.controlPointsDown.Add(item2);
		}
	}

	private void CalculateCatmullRomSplineParameters(List<Vector4> controlPoints, int pos, bool initialPoints = false)
	{
		Vector3 p = controlPoints[pos];
		Vector3 p2 = controlPoints[pos];
		Vector3 p3 = controlPoints[this.ClampListPos(pos + 1)];
		Vector3 p4 = controlPoints[this.ClampListPos(pos + 1)];
		if (pos > 0)
		{
			p = controlPoints[this.ClampListPos(pos - 1)];
		}
		if (pos < controlPoints.Count - 2)
		{
			p4 = controlPoints[this.ClampListPos(pos + 2)];
		}
		int num = Mathf.FloorToInt(1f / this.traingleDensity);
		float num2 = 0f;
		if (pos > 0)
		{
			num2 = 1f;
		}
		float num3;
		for (num3 = num2; num3 <= (float)num; num3 += 1f)
		{
			float t = num3 * this.traingleDensity;
			this.CalculatePointParameters(controlPoints, pos, p, p2, p3, p4, t);
		}
		if (num3 < (float)num)
		{
			num3 = (float)num;
			float t2 = num3 * this.traingleDensity;
			this.CalculatePointParameters(controlPoints, pos, p, p2, p3, p4, t2);
		}
	}

	private void CalculateCatmullRomSpline(List<Vector3> controlPoints, int pos, ref List<Vector3> points)
	{
		Vector3 p = controlPoints[pos];
		Vector3 p2 = controlPoints[pos];
		Vector3 p3 = controlPoints[this.ClampListPos(pos + 1)];
		Vector3 p4 = controlPoints[this.ClampListPos(pos + 1)];
		if (pos > 0)
		{
			p = controlPoints[this.ClampListPos(pos - 1)];
		}
		if (pos < controlPoints.Count - 2)
		{
			p4 = controlPoints[this.ClampListPos(pos + 2)];
		}
		int num = Mathf.FloorToInt(1f / this.traingleDensity);
		float num2 = 0f;
		if (pos > 0)
		{
			num2 = 1f;
		}
		float num3;
		for (num3 = num2; num3 <= (float)num; num3 += 1f)
		{
			float t = num3 * this.traingleDensity;
			this.CalculatePointPosition(controlPoints, pos, p, p2, p3, p4, t, ref points);
		}
		if (num3 < (float)num)
		{
			num3 = (float)num;
			float t2 = num3 * this.traingleDensity;
			this.CalculatePointPosition(controlPoints, pos, p, p2, p3, p4, t2, ref points);
		}
	}

	private void CalculatePointPosition(List<Vector3> controlPoints, int pos, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, ref List<Vector3> points)
	{
		Vector3 catmullRomPosition = this.GetCatmullRomPosition(t, p0, p1, p2, p3);
		points.Add(catmullRomPosition);
		Vector3 normalized = this.GetCatmullRomTangent(t, p0, p1, p2, p3).normalized;
		Vector3 normalized2 = this.CalculateNormal(normalized, Vector3.up).normalized;
	}

	private void CalculatePointParameters(List<Vector4> controlPoints, int pos, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		Vector3 catmullRomPosition = this.GetCatmullRomPosition(t, p0, p1, p2, p3);
		this.widths.Add(Mathf.Lerp(controlPoints[pos].w, controlPoints[this.ClampListPos(pos + 1)].w, t));
		if (this.controlPointsSnap.Count > pos + 1)
		{
			this.snaps.Add(Mathf.Lerp(this.controlPointsSnap[pos], this.controlPointsSnap[this.ClampListPos(pos + 1)], t));
		}
		else
		{
			this.snaps.Add(0f);
		}
		this.lerpValues.Add((float)pos + t);
		this.points.Add(catmullRomPosition);
		Vector3 normalized = this.GetCatmullRomTangent(t, p0, p1, p2, p3).normalized;
		Vector3 vector = this.CalculateNormal(normalized, Vector3.up).normalized;
		Quaternion quaternion;
		if (vector == normalized && vector == Vector3.zero)
		{
			quaternion = Quaternion.identity;
		}
		else
		{
			quaternion = Quaternion.LookRotation(normalized, vector);
		}
		quaternion *= Quaternion.Lerp(this.controlPointsRotations[pos], this.controlPointsRotations[this.ClampListPos(pos + 1)], t);
		this.orientations.Add(quaternion);
		this.tangents.Add(normalized);
		if (this.normalsList.Count > 0 && Vector3.Angle(this.normalsList[this.normalsList.Count - 1], vector) > 90f)
		{
			vector *= -1f;
		}
		this.normalsList.Add(vector);
	}

	private int ClampListPos(int pos)
	{
		if (pos < 0)
		{
			pos = this.controlPoints.Count - 1;
		}
		if (pos > this.controlPoints.Count)
		{
			pos = 1;
		}
		else if (pos > this.controlPoints.Count - 1)
		{
			pos = 0;
		}
		return pos;
	}

	private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		Vector3 a = 2f * p1;
		Vector3 a2 = p2 - p0;
		Vector3 a3 = 2f * p0 - 5f * p1 + 4f * p2 - p3;
		Vector3 a4 = -p0 + 3f * p1 - 3f * p2 + p3;
		return 0.5f * (a + a2 * t + a3 * t * t + a4 * t * t * t);
	}

	private Vector3 GetCatmullRomTangent(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		return 0.5f * (-p0 + p2 + 2f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t + 3f * (-p0 + 3f * p1 - 3f * p2 + p3) * t * t);
	}

	private Vector3 CalculateNormal(Vector3 tangent, Vector3 up)
	{
		Vector3 rhs = Vector3.Cross(up, tangent);
		return Vector3.Cross(tangent, rhs);
	}

	private void GenerateMesh(ref Mesh mesh)
	{
		foreach (Transform transform in this.meshesPartTransforms)
		{
			if (transform != null)
			{
				UnityEngine.Object.DestroyImmediate(transform.gameObject);
			}
		}
		int num = this.points.Count - 1;
		int count = this.points.Count;
		int num2 = this.vertsInShape * count;
		List<int> list = new List<int>();
		Vector3[] array = new Vector3[num2];
		Vector3[] array2 = new Vector3[num2];
		Vector2[] array3 = new Vector2[num2];
		Vector2[] array4 = new Vector2[num2];
		Vector2[] array5 = new Vector2[num2];
		if (this.colors == null || this.colors.Length != num2)
		{
			this.colors = new Color[num2];
			for (int i = 0; i < this.colors.Length; i++)
			{
				this.colors[i] = Color.black;
			}
		}
		if (this.colorsFlowMap.Count != num2)
		{
			this.colorsFlowMap.Clear();
		}
		this.length = 0f;
		this.fulllength = 0f;
		if (this.beginningSpline != null)
		{
			this.length = this.beginningSpline.length;
		}
		this.minMaxWidth = 1f;
		this.uvWidth = 1f;
		this.uvBeginning = 0f;
		if (this.beginningSpline != null)
		{
			this.minMaxWidth = this.beginningMaxWidth - this.beginningMinWidth;
			this.uvWidth = this.minMaxWidth * this.beginningSpline.uvWidth;
			this.uvBeginning = this.beginningSpline.uvWidth * this.beginningMinWidth + this.beginningSpline.uvBeginning;
		}
		else if (this.endingSpline != null)
		{
			this.minMaxWidth = this.endingMaxWidth - this.endingMinWidth;
			this.uvWidth = this.minMaxWidth * this.endingSpline.uvWidth;
			this.uvBeginning = this.endingSpline.uvWidth * this.endingMinWidth + this.endingSpline.uvBeginning;
		}
		for (int j = 0; j < this.pointsDown.Count; j++)
		{
			float num3 = this.widths[j];
			if (j > 0)
			{
				this.fulllength += this.uvWidth * Vector3.Distance(this.pointsDown[j], this.pointsDown[j - 1]) / (this.uvScale * num3);
			}
		}
		float num4 = Mathf.Round(this.fulllength);
		for (int k = 0; k < this.pointsDown.Count; k++)
		{
			float num5 = this.widths[k];
			int num6 = k * this.vertsInShape;
			if (k > 0)
			{
				this.length += this.uvWidth * Vector3.Distance(this.pointsDown[k], this.pointsDown[k - 1]) / (this.uvScale * num5) / this.fulllength * num4;
			}
			float num7 = 0f;
			float num8 = 0f;
			for (int l = 0; l < this.vertsInShape; l++)
			{
				int num9 = num6 + l;
				float num10 = (float)l / (float)(this.vertsInShape - 1);
				if (num10 < 0.5f)
				{
					num10 *= this.minVal * 2f;
				}
				else
				{
					num10 = ((num10 - 0.5f) * (1f - this.maxVal) + 0.5f * this.maxVal) * 2f;
				}
				if (k == 0 && this.beginningSpline != null && this.beginningSpline.verticesEnding != null && this.beginningSpline.normalsEnding != null)
				{
					int num11 = (int)((float)this.beginningSpline.vertsInShape * this.beginningMinWidth);
					array[num9] = this.beginningSpline.verticesEnding[Mathf.Clamp(l + num11, 0, this.beginningSpline.verticesEnding.Count - 1)] + this.beginningSpline.transform.position - base.transform.position;
				}
				else if (k == this.pointsDown.Count - 1 && this.endingSpline != null && this.endingSpline.verticesBeginning != null && this.endingSpline.normalsBeginning != null)
				{
					int num12 = (int)((float)this.endingSpline.vertsInShape * this.endingMinWidth);
					array[num9] = this.endingSpline.verticesBeginning[Mathf.Clamp(l + num12, 0, this.endingSpline.verticesBeginning.Count - 1)] + this.endingSpline.transform.position - base.transform.position;
				}
				else
				{
					array[num9] = Vector3.Lerp(this.pointsDown[k], this.pointsUp[k], num10);
					RaycastHit raycastHit;
					if (Physics.Raycast(array[num9] + base.transform.position + Vector3.up * 5f, Vector3.down, out raycastHit, 1000f, this.snapMask.value))
					{
						array[num9] = Vector3.Lerp(array[num9], raycastHit.point - base.transform.position + new Vector3(0f, 0.1f, 0f), (Mathf.Sin(3.14159274f * this.snaps[k] - 1.57079637f) + 1f) * 0.5f);
					}
					RaycastHit raycastHit2;
					if (this.normalFromRaycast && Physics.Raycast(this.points[k] + base.transform.position + Vector3.up * 5f, Vector3.down, out raycastHit2, 1000f, this.snapMask.value))
					{
						array2[num9] = raycastHit2.normal;
					}
					Vector3[] array6 = array;
					int num13 = num9;
					array6[num13].y = array6[num13].y + Mathf.Lerp(this.controlPointsMeshCurves[Mathf.FloorToInt(this.lerpValues[k])].Evaluate(num10), this.controlPointsMeshCurves[Mathf.CeilToInt(this.lerpValues[k])].Evaluate(num10), this.lerpValues[k] - Mathf.Floor(this.lerpValues[k]));
				}
				if (k > 0 && k < 5 && this.beginningSpline != null && this.beginningSpline.verticesEnding != null)
				{
					array[num9].y = (array[num9].y + array[num9 - this.vertsInShape].y) * 0.5f;
				}
				if (k == this.pointsDown.Count - 1 && this.endingSpline != null && this.endingSpline.verticesBeginning != null)
				{
					for (int m = 1; m < 5; m++)
					{
						array[num9 - this.vertsInShape * m].y = (array[num9 - this.vertsInShape * (m - 1)].y + array[num9 - this.vertsInShape * m].y) * 0.5f;
					}
				}
				if (k == 0)
				{
					this.verticesBeginning.Add(array[num9]);
				}
				if (k == this.pointsDown.Count - 1)
				{
					this.verticesEnding.Add(array[num9]);
				}
				if (!this.normalFromRaycast)
				{
					array2[num9] = this.orientations[k] * Vector3.up;
				}
				if (k == 0)
				{
					this.normalsBeginning.Add(array2[num9]);
				}
				if (k == this.pointsDown.Count - 1)
				{
					this.normalsEnding.Add(array2[num9]);
				}
				if (l > 0)
				{
					num7 = num10 * this.uvWidth;
					num8 = num10;
				}
				if (this.beginningSpline != null || this.endingSpline != null)
				{
					num7 += this.uvBeginning;
				}
				num7 /= this.uvScale;
				float num14 = this.FlowCalculate(num8, array2[num9].y);
				int num15 = 10;
				if (this.beginnigChildSplines.Count > 0 && k <= num15)
				{
					float num16 = 0f;
					int n = 0;
					while (n < this.endingChildSplines.Count)
					{
						RamSpline ramSpline = this.endingChildSplines[n];
						if (ramSpline == null)
						{
							this.endingChildSplines.RemoveAt(n);
						}
						else
						{
							n++;
							if (Mathf.CeilToInt(ramSpline.endingMaxWidth * (float)(this.vertsInShape - 1)) >= l && l >= Mathf.CeilToInt(ramSpline.endingMinWidth * (float)(this.vertsInShape - 1)))
							{
								num16 = (float)(l - Mathf.CeilToInt(ramSpline.endingMinWidth * (float)(this.vertsInShape - 1))) / (float)(Mathf.CeilToInt(ramSpline.endingMaxWidth * (float)(this.vertsInShape - 1)) - Mathf.CeilToInt(ramSpline.endingMinWidth * (float)(this.vertsInShape - 1)));
								num16 = this.FlowCalculate(num16, array2[num9].y);
							}
						}
					}
					if (k > 0)
					{
						num14 = Mathf.Lerp(num14, num16, 1f - (float)k / (float)num15);
					}
					else
					{
						num14 = num16;
					}
				}
				if (k >= this.pointsDown.Count - num15 - 1 && this.endingChildSplines.Count > 0)
				{
					float num17 = 0f;
					int num18 = 0;
					while (num18 < this.endingChildSplines.Count)
					{
						RamSpline ramSpline2 = this.endingChildSplines[num18];
						if (ramSpline2 == null)
						{
							this.endingChildSplines.RemoveAt(num18);
						}
						else
						{
							num18++;
							if (Mathf.CeilToInt(ramSpline2.beginningMaxWidth * (float)(this.vertsInShape - 1)) >= l && l >= Mathf.CeilToInt(ramSpline2.beginningMinWidth * (float)(this.vertsInShape - 1)))
							{
								num17 = (float)(l - Mathf.CeilToInt(ramSpline2.beginningMinWidth * (float)(this.vertsInShape - 1))) / (float)(Mathf.CeilToInt(ramSpline2.beginningMaxWidth * (float)(this.vertsInShape - 1)) - Mathf.CeilToInt(ramSpline2.beginningMinWidth * (float)(this.vertsInShape - 1)));
								num17 = this.FlowCalculate(num17, array2[num9].y);
							}
						}
					}
					if (k < this.pointsDown.Count - 1)
					{
						num14 = Mathf.Lerp(num14, num17, (float)(k - (this.pointsDown.Count - num15 - 1)) / (float)num15);
					}
					else
					{
						num14 = num17;
					}
				}
				float num19 = -(num8 - 0.5f) * 0.01f;
				if (this.uvRotation)
				{
					if (!this.invertUVDirection)
					{
						array3[num9] = new Vector2(1f - this.length, num7);
						array4[num9] = new Vector2(1f - this.length / this.fulllength, num8);
						array5[num9] = new Vector2(num14, num19);
					}
					else
					{
						array3[num9] = new Vector2(1f + this.length, num7);
						array4[num9] = new Vector2(1f + this.length / this.fulllength, num8);
						array5[num9] = new Vector2(num14, num19);
					}
				}
				else if (!this.invertUVDirection)
				{
					array3[num9] = new Vector2(num7, 1f - this.length);
					array4[num9] = new Vector2(num8, 1f - this.length / this.fulllength);
					array5[num9] = new Vector2(num19, num14);
				}
				else
				{
					array3[num9] = new Vector2(num7, 1f + this.length);
					array4[num9] = new Vector2(num8, 1f + this.length / this.fulllength);
					array5[num9] = new Vector2(num19, num14);
				}
				if (this.colorsFlowMap.Count <= num9)
				{
					this.colorsFlowMap.Add(array5[num9]);
				}
				else if (!this.overrideFlowMap)
				{
					this.colorsFlowMap[num9] = array5[num9];
				}
			}
		}
		for (int num20 = 0; num20 < num; num20++)
		{
			int num21 = num20 * this.vertsInShape;
			for (int num22 = 0; num22 < this.vertsInShape - 1; num22++)
			{
				int item = num21 + num22;
				int item2 = num21 + num22 + this.vertsInShape;
				int item3 = num21 + num22 + 1 + this.vertsInShape;
				int item4 = num21 + num22 + 1;
				list.Add(item);
				list.Add(item2);
				list.Add(item3);
				list.Add(item3);
				list.Add(item4);
				list.Add(item);
			}
		}
		mesh = new Mesh();
		mesh.Clear();
		mesh.vertices = array;
		mesh.normals = array2;
		mesh.uv = array3;
		mesh.uv3 = array4;
		mesh.uv4 = this.colorsFlowMap.ToArray();
		mesh.triangles = list.ToArray();
		mesh.colors = this.colors;
		mesh.RecalculateTangents();
		this.meshfilter.mesh = mesh;
		base.GetComponent<MeshRenderer>().enabled = true;
		if (this.generateMeshParts)
		{
			this.GenerateMeshParts(mesh);
		}
	}

	public void GenerateMeshParts(Mesh baseMesh)
	{
		foreach (Transform transform in this.meshesPartTransforms)
		{
			if (transform != null)
			{
				UnityEngine.Object.DestroyImmediate(transform.gameObject);
			}
		}
		Vector3[] vertices = baseMesh.vertices;
		Vector3[] normals = baseMesh.normals;
		Vector2[] uv = baseMesh.uv;
		Vector2[] uv2 = baseMesh.uv3;
		base.GetComponent<MeshRenderer>().enabled = false;
		int num = Mathf.RoundToInt((float)(vertices.Length / this.vertsInShape) / (float)this.meshPartsCount);
		int num2 = num * this.vertsInShape;
		for (int i = 0; i < this.meshPartsCount; i++)
		{
			GameObject gameObject = new GameObject(base.gameObject.name + "- Mesh part " + i);
			gameObject.transform.SetParent(base.gameObject.transform, false);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localEulerAngles = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			this.meshesPartTransforms.Add(gameObject.transform);
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = base.GetComponent<MeshRenderer>().sharedMaterial;
			meshRenderer.receiveShadows = false;
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			Mesh mesh = new Mesh();
			mesh.Clear();
			List<Vector3> list = new List<Vector3>();
			List<Vector3> list2 = new List<Vector3>();
			List<Vector2> list3 = new List<Vector2>();
			List<Vector2> list4 = new List<Vector2>();
			List<Vector2> list5 = new List<Vector2>();
			List<Color> list6 = new List<Color>();
			List<int> list7 = new List<int>();
			int num3 = num2 * i + ((i <= 0) ? 0 : (-this.vertsInShape));
			while ((num3 < num2 * (i + 1) && num3 < vertices.Length) || (i == this.meshPartsCount - 1 && num3 < vertices.Length))
			{
				list.Add(vertices[num3]);
				list2.Add(normals[num3]);
				list3.Add(uv[num3]);
				list4.Add(uv2[num3]);
				list5.Add(this.colorsFlowMap[num3]);
				list6.Add(this.colors[num3]);
				num3++;
			}
			if (list.Count > 0)
			{
				Vector3 b = list[0];
				for (int j = 0; j < list.Count; j++)
				{
					list[j] -= b;
				}
				for (int k = 0; k < list.Count / this.vertsInShape - 1; k++)
				{
					int num4 = k * this.vertsInShape;
					for (int l = 0; l < this.vertsInShape - 1; l++)
					{
						int item = num4 + l;
						int item2 = num4 + l + this.vertsInShape;
						int item3 = num4 + l + 1 + this.vertsInShape;
						int item4 = num4 + l + 1;
						list7.Add(item);
						list7.Add(item2);
						list7.Add(item3);
						list7.Add(item3);
						list7.Add(item4);
						list7.Add(item);
					}
				}
				gameObject.transform.position += b;
				mesh.vertices = list.ToArray();
				mesh.triangles = list7.ToArray();
				mesh.normals = list2.ToArray();
				mesh.uv = list3.ToArray();
				mesh.uv3 = list4.ToArray();
				mesh.uv4 = list5.ToArray();
				mesh.colors = list6.ToArray();
				mesh.RecalculateTangents();
				meshFilter.mesh = mesh;
			}
		}
	}

	private float FlowCalculate(float u, float normalY)
	{
		return Mathf.Lerp(this.flowWaterfall.Evaluate(u), this.flowFlat.Evaluate(u), Mathf.Clamp(normalY, 0f, 1f));
	}

	private void GenerateMeshConvexColliderTrigger()
	{
		if (!this.m_GenerateMeshConvexColliderTrigger)
		{
			return;
		}
		List<MeshCollider> componentsDeepChild = General.GetComponentsDeepChild<MeshCollider>(base.gameObject);
		for (int i = 0; i < componentsDeepChild.Count; i++)
		{
			if (componentsDeepChild[i] != null)
			{
				UnityEngine.Object.DestroyImmediate(componentsDeepChild[i]);
			}
		}
		MeshCollider meshCollider = base.gameObject.AddComponent<MeshCollider>();
		meshCollider.convex = true;
		meshCollider.isTrigger = true;
		meshCollider.sharedMesh = this.meshfilter.sharedMesh;
	}

	public SplineProfile currentProfile;

	public SplineProfile oldProfile;

	public List<RamSpline> beginnigChildSplines = new List<RamSpline>();

	public List<RamSpline> endingChildSplines = new List<RamSpline>();

	public RamSpline beginningSpline;

	public RamSpline endingSpline;

	public int beginningConnectionID;

	public int endingConnectionID;

	public float beginningMinWidth = 0.5f;

	public float beginningMaxWidth = 1f;

	public float endingMinWidth = 0.5f;

	public float endingMaxWidth = 1f;

	public int toolbarInt;

	public bool invertUVDirection;

	public bool uvRotation = true;

	public MeshFilter meshfilter;

	public List<Vector4> controlPoints = new List<Vector4>();

	public List<Quaternion> controlPointsRotations = new List<Quaternion>();

	public List<Quaternion> controlPointsOrientation = new List<Quaternion>();

	public List<Vector3> controlPointsUp = new List<Vector3>();

	public List<Vector3> controlPointsDown = new List<Vector3>();

	public List<float> controlPointsSnap = new List<float>();

	public AnimationCurve meshCurve = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 0f),
		new Keyframe(1f, 0f)
	});

	public List<AnimationCurve> controlPointsMeshCurves = new List<AnimationCurve>();

	public bool normalFromRaycast;

	public bool snapToTerrain;

	public LayerMask snapMask = 1;

	public List<Vector3> points = new List<Vector3>();

	public List<Vector3> pointsUp = new List<Vector3>();

	public List<Vector3> pointsDown = new List<Vector3>();

	public List<Vector3> points2 = new List<Vector3>();

	public List<Vector3> verticesBeginning = new List<Vector3>();

	public List<Vector3> verticesEnding = new List<Vector3>();

	public List<Vector3> normalsBeginning = new List<Vector3>();

	public List<Vector3> normalsEnding = new List<Vector3>();

	public List<float> widths = new List<float>();

	public List<float> snaps = new List<float>();

	public List<float> lerpValues = new List<float>();

	public List<Quaternion> orientations = new List<Quaternion>();

	public List<Vector3> tangents = new List<Vector3>();

	public List<Vector3> normalsList = new List<Vector3>();

	public Color[] colors;

	public List<Vector2> colorsFlowMap = new List<Vector2>();

	public float minVal = 0.5f;

	public float maxVal = 0.5f;

	public float width = 4f;

	public int vertsInShape = 3;

	public float traingleDensity = 0.2f;

	public float uvScale = 3f;

	public Material oldMaterial;

	public bool showVertexColors;

	public bool showFlowMap;

	public bool overrideFlowMap;

	public bool drawOnMesh;

	public bool drawOnMeshFlowMap;

	public bool uvScaleOverride;

	public bool debug;

	public Color drawColor = Color.black;

	public bool drawOnMultiple;

	public float flowSpeed = 1f;

	public float flowDirection;

	public AnimationCurve flowFlat = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 0.025f),
		new Keyframe(0.5f, 0.05f),
		new Keyframe(1f, 0.025f)
	});

	public AnimationCurve flowWaterfall = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 0.25f),
		new Keyframe(1f, 0.25f)
	});

	public float opacity = 0.1f;

	public float drawSize = 1f;

	public float length;

	public float fulllength;

	public float minMaxWidth;

	public float uvWidth;

	public float uvBeginning;

	public bool m_GenerateMeshConvexColliderTrigger;

	public bool generateMeshParts;

	public int meshPartsCount = 3;

	public List<Transform> meshesPartTransforms = new List<Transform>();

	public float simulatedRiverLength = 100f;

	public int simulatedRiverPoints = 10;

	public float simulationMinStepSize = 1f;

	public AnimationCurve terrainCurve = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 0f),
		new Keyframe(1f, 0f)
	});

	public int detailTerrain = 100;

	public int detailTerrainForward = 100;

	public float terrainDepthHeight;

	public float terrainDepthMultiplier = 1f;

	public float terrainAdditionalWidth = 2f;

	public float terrainSmoothMultiplier = 1f;
}
