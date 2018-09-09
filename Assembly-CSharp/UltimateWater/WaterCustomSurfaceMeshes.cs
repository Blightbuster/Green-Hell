using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	[Serializable]
	public class WaterCustomSurfaceMeshes
	{
		public Mesh[] VolumeMeshes
		{
			get
			{
				if (this._VolumeMeshes == null)
				{
					Mesh[] usedMeshes = this._UsedMeshes;
					List<Mesh> list = new List<Mesh>();
					foreach (Mesh mesh in usedMeshes)
					{
						list.Add(mesh);
						list.Add(this.CreateBoundaryMesh(mesh));
					}
					this._VolumeMeshes = list.ToArray();
				}
				return this._VolumeMeshes;
			}
		}

		public bool Triangular
		{
			get
			{
				return this._CustomMeshes == null || this._UsedMeshes.Length == 0 || this._UsedMeshes[0].GetTopology(0) == MeshTopology.Triangles;
			}
		}

		public Mesh[] Meshes
		{
			get
			{
				return this._CustomMeshes;
			}
			set
			{
				this._CustomMeshes = value;
				this._UsedMeshesCache = null;
				this._VolumeMeshes = null;
			}
		}

		public Mesh[] GetTransformedMeshes(Camera camera, out Matrix4x4 matrix, bool volume)
		{
			matrix = this._Water.transform.localToWorldMatrix;
			if (volume)
			{
				return this.VolumeMeshes;
			}
			return this._UsedMeshes;
		}

		public void Dispose()
		{
			if (this._VolumeMeshes != null)
			{
				for (int i = 1; i < this._VolumeMeshes.Length; i += 2)
				{
					this._VolumeMeshes[i].Destroy();
				}
				this._VolumeMeshes = null;
			}
			this._UsedMeshesCache = null;
		}

		private Mesh[] _UsedMeshes
		{
			get
			{
				if (this._UsedMeshesCache == null)
				{
					List<Mesh> list = new List<Mesh>();
					foreach (Mesh mesh in this._CustomMeshes)
					{
						if (mesh != null)
						{
							list.Add(mesh);
						}
					}
					this._UsedMeshesCache = list.ToArray();
				}
				return this._UsedMeshesCache;
			}
		}

		private Mesh CreateBoundaryMesh(Mesh sourceMesh)
		{
			Mesh mesh = new Mesh
			{
				hideFlags = HideFlags.DontSave
			};
			Vector3[] vertices = sourceMesh.vertices;
			List<Vector3> list = new List<Vector3>();
			List<int> list2 = new List<int>();
			WaterCustomSurfaceMeshes.Edge[] array = WaterCustomSurfaceMeshes.BuildManifoldEdges(sourceMesh);
			Vector3 vector = default(Vector3);
			int item = array.Length * 4;
			for (int i = 0; i < array.Length; i++)
			{
				int count = list.Count;
				Vector3 vector2 = vertices[array[i].VertexIndex0];
				Vector3 vector3 = vertices[array[i].VertexIndex1];
				list.Add(vector2);
				list.Add(vector3);
				vector2.y -= 1000f;
				vector3.y -= 1000f;
				list.Add(vector2);
				list.Add(vector3);
				list2.Add(count + 3);
				list2.Add(count + 2);
				list2.Add(count);
				list2.Add(count + 3);
				list2.Add(count);
				list2.Add(count + 1);
				list2.Add(count + 3);
				list2.Add(count + 2);
				list2.Add(item);
				vector += vector2;
				vector += vector3;
			}
			int num = list.Count / 2;
			vector /= (float)num;
			list.Add(vector);
			mesh.vertices = list.ToArray();
			mesh.SetIndices(list2.ToArray(), MeshTopology.Triangles, 0);
			return mesh;
		}

		private static WaterCustomSurfaceMeshes.Edge[] BuildManifoldEdges(Mesh mesh)
		{
			WaterCustomSurfaceMeshes.Edge[] array = WaterCustomSurfaceMeshes.BuildEdges(mesh.vertexCount, mesh.triangles);
			List<WaterCustomSurfaceMeshes.Edge> list = new List<WaterCustomSurfaceMeshes.Edge>();
			foreach (WaterCustomSurfaceMeshes.Edge item in array)
			{
				if (item.FaceIndex0 == item.FaceIndex1)
				{
					list.Add(item);
				}
			}
			return list.ToArray();
		}

		private static WaterCustomSurfaceMeshes.Edge[] BuildEdges(int vertexCount, int[] triangleArray)
		{
			int num = triangleArray.Length;
			int[] array = new int[vertexCount + num];
			int num2 = triangleArray.Length / 3;
			for (int i = 0; i < vertexCount; i++)
			{
				array[i] = -1;
			}
			WaterCustomSurfaceMeshes.Edge[] array2 = new WaterCustomSurfaceMeshes.Edge[num];
			int num3 = 0;
			for (int j = 0; j < num2; j++)
			{
				int num4 = triangleArray[j * 3 + 2];
				for (int k = 0; k < 3; k++)
				{
					int num5 = triangleArray[j * 3 + k];
					if (num4 < num5)
					{
						WaterCustomSurfaceMeshes.Edge edge = new WaterCustomSurfaceMeshes.Edge
						{
							VertexIndex0 = num4,
							VertexIndex1 = num5,
							FaceIndex0 = j,
							FaceIndex1 = j
						};
						array2[num3] = edge;
						int num6 = array[num4];
						if (num6 == -1)
						{
							array[num4] = num3;
						}
						else
						{
							for (;;)
							{
								int num7 = array[vertexCount + num6];
								if (num7 == -1)
								{
									break;
								}
								num6 = num7;
							}
							array[vertexCount + num6] = num3;
						}
						array[vertexCount + num3] = -1;
						num3++;
					}
					num4 = num5;
				}
			}
			for (int l = 0; l < num2; l++)
			{
				int num8 = triangleArray[l * 3 + 2];
				for (int m = 0; m < 3; m++)
				{
					int num9 = triangleArray[l * 3 + m];
					if (num8 > num9)
					{
						bool flag = false;
						for (int num10 = array[num9]; num10 != -1; num10 = array[vertexCount + num10])
						{
							WaterCustomSurfaceMeshes.Edge edge2 = array2[num10];
							if (edge2.VertexIndex1 == num8 && edge2.FaceIndex0 == edge2.FaceIndex1)
							{
								array2[num10].FaceIndex1 = l;
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							WaterCustomSurfaceMeshes.Edge edge3 = new WaterCustomSurfaceMeshes.Edge
							{
								VertexIndex0 = num8,
								VertexIndex1 = num9,
								FaceIndex0 = l,
								FaceIndex1 = l
							};
							array2[num3] = edge3;
							num3++;
						}
					}
					num8 = num9;
				}
			}
			WaterCustomSurfaceMeshes.Edge[] array3 = new WaterCustomSurfaceMeshes.Edge[num3];
			for (int n = 0; n < num3; n++)
			{
				array3[n] = array2[n];
			}
			return array3;
		}

		internal virtual void OnEnable(Water water)
		{
			this._Water = water;
		}

		internal virtual void OnDisable()
		{
			this.Dispose();
		}

		[SerializeField]
		[FormerlySerializedAs("customMeshes")]
		private Mesh[] _CustomMeshes;

		private Water _Water;

		private Mesh[] _UsedMeshesCache;

		private Mesh[] _VolumeMeshes;

		private struct Edge
		{
			public int VertexIndex0;

			public int VertexIndex1;

			public int FaceIndex0;

			public int FaceIndex1;
		}
	}
}
