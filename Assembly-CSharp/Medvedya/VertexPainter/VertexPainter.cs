using System;
using System.Collections.Generic;
using UnityEngine;

namespace Medvedya.VertexPainter
{
	[RequireComponent(typeof(MeshFilter))]
	[AddComponentMenu("Vertex painter/Vertex painter")]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(MeshRenderer))]
	[ExecuteInEditMode]
	public class VertexPainter : MonoBehaviour, IPainting
	{
		public MeshFilter meshFilter
		{
			get
			{
				if (this._meshFilter == null)
				{
					this._meshFilter = base.GetComponent<MeshFilter>();
				}
				return this._meshFilter;
			}
		}

		public bool isLoadedMeshData
		{
			get
			{
				return this._isLoadedMeshData;
			}
			private set
			{
				this._isLoadedMeshData = value;
			}
		}

		public bool isInitMesh { get; private set; }

		public ModifyInfo modifyInfo
		{
			get
			{
				return this._modifyInfo;
			}
		}

		public void LoadDataFromAnother(VertexPainter vp)
		{
			this.subMeshs.Clear();
			foreach (VertexPainter.SubMesh item in vp.subMeshs)
			{
				this.subMeshs.Add(item);
			}
			this.verteces = (Vector3[])vp.verteces.Clone();
			this.colors = (Color32[])vp.colors.Clone();
			this.normals = (Vector3[])vp.normals.Clone();
			if (vp.hasUV)
			{
				this.hasUV = true;
				this.uv = vp.uv;
			}
			this.referensMesh = vp.referensMesh;
			this.bounds = vp.bounds;
		}

		public void HardNormal(int vertecsIndex)
		{
			int triangleByVertecsIndex = VertexPainter.GetTriangleByVertecsIndex(vertecsIndex);
			Vector3 vector = Vector3.Cross(this.verteces[triangleByVertecsIndex], this.verteces[triangleByVertecsIndex + 1]);
			this.normals[vertecsIndex] = vector;
		}

		public static int GetTriangleByVertecsIndex(int vertecsIndex)
		{
			return vertecsIndex - vertecsIndex % 3;
		}

		private void OnDrawGizmos()
		{
			this.wireMesh.Do(this);
		}

		private void Awake()
		{
			if (this.modifyInfo == null || this.modifyInfo.vertexPainter == null)
			{
				this._modifyInfo = new ModifyInfo(this, base.transform);
			}
			if (!this.isLoadedMeshData && this.meshFilter.sharedMesh != null)
			{
				this.LoadMeshDataFromBaseMesh(this.meshFilter.sharedMesh);
				this.meshFilter.sharedMesh = null;
			}
			this.Init();
		}

		public void ApplyStandratMaterial()
		{
		}

		public void Init()
		{
			if (!this.isLoadedMeshData || this.isInitMesh)
			{
				return;
			}
			this.isInitMesh = true;
			if (this.meshFilter.sharedMesh != null)
			{
			}
			this.meshFilter.sharedMesh = new Mesh();
			this.meshFilter.sharedMesh.name = base.gameObject.name;
			this.FillFullMesh();
			this.FillColorMesh();
		}

		private void OnValidate()
		{
			if (this.meshFilter.sharedMesh == null && this.isLoadedMeshData)
			{
				this.isInitMesh = false;
			}
			this.Init();
		}

		private void OnDestroy()
		{
			if (this.meshFilter.sharedMesh != null)
			{
				UnityEngine.Object.DestroyImmediate(this.meshFilter.sharedMesh);
			}
		}

		private void FillFullMesh()
		{
			this.meshFilter.sharedMesh.vertices = this.verteces;
			this.meshFilter.sharedMesh.subMeshCount = this.subMeshs.Count;
			for (int i = 0; i < this.subMeshs.Count; i++)
			{
				this.meshFilter.sharedMesh.SetTriangles(this.subMeshs[i].triangles, i);
			}
			this.meshFilter.sharedMesh.normals = this.normals;
			if (this.hasUV)
			{
				this.meshFilter.sharedMesh.uv = this.uv;
			}
			this.meshFilter.sharedMesh.RecalculateBounds();
		}

		public void FillColorMesh()
		{
			if (this.meshFilter.sharedMesh == null)
			{
				return;
			}
			this.meshFilter.sharedMesh.colors32 = this.colors;
		}

		public void FillNormalMesh()
		{
			if (this.meshFilter.sharedMesh == null)
			{
				return;
			}
			this.meshFilter.sharedMesh.normals = this.normals;
		}

		public void LoadMeshDataFromBaseMesh(Mesh baseMesh)
		{
			this.ApplyStandratMaterial();
			this.referensMesh = baseMesh;
			MeshCollider component = base.GetComponent<MeshCollider>();
			if (component != null)
			{
				component.sharedMesh = baseMesh;
			}
			Vector3[] vertices = baseMesh.vertices;
			Vector3[] array = baseMesh.normals;
			Color[] array2 = baseMesh.colors;
			Vector2[] array3 = baseMesh.uv;
			List<int[]> list = new List<int[]>();
			int num = 0;
			this.subMeshs.Clear();
			for (int i = 0; i < baseMesh.subMeshCount; i++)
			{
				int[] triangles = baseMesh.GetTriangles(i);
				list.Add(triangles);
				num += triangles.Length;
			}
			this.verteces = new Vector3[num];
			this.colors = new Color32[num];
			this.normals = new Vector3[num];
			bool flag = array != null && array.Length > 0;
			bool flag2 = array2 != null && array2.Length > 0;
			this.hasUV = (array3 != null && array3.Length > 0);
			if (this.hasUV)
			{
				this.uv = new Vector2[num];
			}
			else
			{
				this.uv = null;
			}
			int num2 = 0;
			for (int j = 0; j < list.Count; j++)
			{
				int[] array4 = list[j];
				VertexPainter.SubMesh subMesh = new VertexPainter.SubMesh(array4.Length);
				this.subMeshs.Add(subMesh);
				for (int k = 0; k < array4.Length; k++)
				{
					int num3 = array4[k];
					int num4 = num2 + k;
					this.verteces[num4] = vertices[num3];
					subMesh.triangles[k] = num4;
					if (flag)
					{
						this.normals[num4] = array[num3];
					}
					if (flag2)
					{
						this.colors[num4] = array2[num3];
					}
					if (this.hasUV)
					{
						this.uv[num4] = array3[num3];
					}
				}
				num2 += array4.Length;
			}
			if (!flag2)
			{
				Color32 color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				for (int l = 0; l < this.verteces.Length; l++)
				{
					this.colors[l] = color;
				}
			}
			if (!flag)
			{
				for (int m = 0; m < this.subMeshs.Count; m++)
				{
					int num5 = 0;
					for (int n = 0; n < this.subMeshs[m].triangles.Length; n += 3)
					{
						int num6 = this.subMeshs[m].triangles[num5 + n];
						Vector3 b = this.verteces[num6];
						Vector3 a = this.verteces[num6 + 1];
						Vector3 a2 = this.verteces[num6 + 2];
						Vector3 lhs = a - b;
						Vector3 rhs = a2 - b;
						Vector3 vector = Vector3.Cross(lhs, rhs);
						this.normals[num6] = vector;
						this.normals[num6 + 1] = vector;
						this.normals[num6 + 2] = vector;
					}
					num5 += this.subMeshs[m].triangles.Length;
				}
			}
			this.bounds = baseMesh.bounds;
			this.isLoadedMeshData = true;
			this.isInitMesh = false;
			this.ApplyStandratMaterial();
		}

		public List<int> GetComparePoints(int point)
		{
			List<int> list = new List<int>();
			Vector3 rhs = this.verteces[point];
			for (int i = 0; i < this.verteces.Length; i++)
			{
				Vector3 lhs = this.verteces[i];
				if (lhs == rhs)
				{
					list.Add(i);
				}
			}
			return list;
		}

		public void SetSoftNormal(int pointIndex)
		{
			List<int> comparePoints = this.GetComparePoints(pointIndex);
			Vector3 vector = Vector3.zero;
			foreach (int point in comparePoints)
			{
				vector += this.GetHardNormal(point);
			}
			vector /= (float)comparePoints.Count;
			vector.Normalize();
			foreach (int num in comparePoints)
			{
				this.normals[num] = vector;
			}
		}

		public void SetHardNormal(int pointIndex)
		{
			this.normals[pointIndex] = this.GetHardNormal(pointIndex);
		}

		public Vector3 GetHardNormal(int point)
		{
			int num = point - point % 3;
			Vector3 lhs = this.verteces[num] - this.verteces[num + 1];
			Vector3 rhs = this.verteces[num] - this.verteces[num + 2];
			return Vector3.Cross(lhs, rhs);
		}

		public void OptemizeMesh()
		{
		}

		[SerializeField]
		public List<VertexPainter.SubMesh> subMeshs = new List<VertexPainter.SubMesh>();

		[SerializeField]
		public Vector3[] verteces;

		[SerializeField]
		public Color32[] colors;

		[SerializeField]
		public Vector3[] normals;

		[SerializeField]
		private bool hasUV;

		[SerializeField]
		private Vector2[] uv;

		[SerializeField]
		private Bounds bounds;

		[SerializeField]
		private MeshFilter _meshFilter;

		[SerializeField]
		public WireMesh wireMesh = new WireMesh();

		[SerializeField]
		private bool _isLoadedMeshData;

		[SerializeField]
		private ModifyInfo _modifyInfo;

		public Mesh referensMesh;

		[Serializable]
		public class SubMesh
		{
			public SubMesh(int length)
			{
				this.triangles = new int[length];
			}

			public SubMesh(int[] triangles)
			{
				this.triangles = (int[])triangles.Clone();
			}

			public VertexPainter.SubMesh Clone()
			{
				return new VertexPainter.SubMesh(this.triangles);
			}

			public int[] triangles;
		}

		public struct NearTriangleInfo
		{
			public int p1;

			public int p2;

			public int p3;

			public int cp;
		}
	}
}
