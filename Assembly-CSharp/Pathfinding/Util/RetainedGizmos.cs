using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Util
{
	public class RetainedGizmos
	{
		public GraphGizmoHelper GetSingleFrameGizmoHelper(AstarPath active)
		{
			RetainedGizmos.Hasher hasher = default(RetainedGizmos.Hasher);
			hasher.AddHash(Time.realtimeSinceStartup.GetHashCode());
			this.Draw(hasher);
			return this.GetGizmoHelper(active, hasher);
		}

		public GraphGizmoHelper GetGizmoHelper(AstarPath active, RetainedGizmos.Hasher hasher)
		{
			GraphGizmoHelper graphGizmoHelper = ObjectPool<GraphGizmoHelper>.Claim();
			graphGizmoHelper.Init(active, hasher, this);
			return graphGizmoHelper;
		}

		private void PoolMesh(Mesh mesh)
		{
			mesh.Clear();
			this.cachedMeshes.Push(mesh);
		}

		private Mesh GetMesh()
		{
			if (this.cachedMeshes.Count > 0)
			{
				return this.cachedMeshes.Pop();
			}
			return new Mesh
			{
				hideFlags = HideFlags.DontSave
			};
		}

		public bool HasCachedMesh(RetainedGizmos.Hasher hasher)
		{
			return this.existingHashes.Contains(hasher.Hash);
		}

		public bool Draw(RetainedGizmos.Hasher hasher)
		{
			this.usedHashes.Add(hasher.Hash);
			return this.HasCachedMesh(hasher);
		}

		public void DrawExisting()
		{
			for (int i = 0; i < this.meshes.Count; i++)
			{
				this.usedHashes.Add(this.meshes[i].hash);
			}
		}

		public void FinalizeDraw()
		{
			this.RemoveUnusedMeshes(this.meshes);
			Camera current = Camera.current;
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(current);
			if (this.surfaceMaterial == null || this.lineMaterial == null)
			{
				return;
			}
			for (int i = 0; i <= 1; i++)
			{
				Material material = (i != 0) ? this.lineMaterial : this.surfaceMaterial;
				for (int j = 0; j < material.passCount; j++)
				{
					material.SetPass(j);
					for (int k = 0; k < this.meshes.Count; k++)
					{
						if (this.meshes[k].lines == (material == this.lineMaterial) && GeometryUtility.TestPlanesAABB(planes, this.meshes[k].mesh.bounds))
						{
							Graphics.DrawMeshNow(this.meshes[k].mesh, Matrix4x4.identity);
						}
					}
				}
			}
			this.usedHashes.Clear();
		}

		public void ClearCache()
		{
			this.usedHashes.Clear();
			this.RemoveUnusedMeshes(this.meshes);
			while (this.cachedMeshes.Count > 0)
			{
				UnityEngine.Object.DestroyImmediate(this.cachedMeshes.Pop());
			}
		}

		private void RemoveUnusedMeshes(List<RetainedGizmos.MeshWithHash> meshList)
		{
			int i = 0;
			int num = 0;
			while (i < meshList.Count)
			{
				if (num == meshList.Count)
				{
					num--;
					meshList.RemoveAt(num);
				}
				else if (this.usedHashes.Contains(meshList[num].hash))
				{
					meshList[i] = meshList[num];
					i++;
					num++;
				}
				else
				{
					this.PoolMesh(meshList[num].mesh);
					this.existingHashes.Remove(meshList[num].hash);
					num++;
				}
			}
		}

		private List<RetainedGizmos.MeshWithHash> meshes = new List<RetainedGizmos.MeshWithHash>();

		private HashSet<ulong> usedHashes = new HashSet<ulong>();

		private HashSet<ulong> existingHashes = new HashSet<ulong>();

		private Stack<Mesh> cachedMeshes = new Stack<Mesh>();

		public Material surfaceMaterial;

		public Material lineMaterial;

		public struct Hasher
		{
			public Hasher(AstarPath active)
			{
				this.hash = 0UL;
				this.debugData = active.debugPathData;
				this.includePathSearchInfo = (this.debugData != null && (active.debugMode == GraphDebugMode.F || active.debugMode == GraphDebugMode.G || active.debugMode == GraphDebugMode.H || active.showSearchTree));
				this.AddHash((int)active.debugMode);
				this.AddHash(active.debugFloor.GetHashCode());
				this.AddHash(active.debugRoof.GetHashCode());
			}

			public void AddHash(int hash)
			{
				this.hash = (1572869UL * this.hash ^ (ulong)((long)hash));
			}

			public void HashNode(GraphNode node)
			{
				this.AddHash(node.GetGizmoHashCode());
				if (this.includePathSearchInfo)
				{
					PathNode pathNode = this.debugData.GetPathNode(node.NodeIndex);
					this.AddHash((int)pathNode.pathID);
					this.AddHash((pathNode.pathID != this.debugData.PathID) ? 0 : 1);
					this.AddHash((int)pathNode.F);
				}
			}

			public ulong Hash
			{
				get
				{
					return this.hash;
				}
			}

			private ulong hash;

			private bool includePathSearchInfo;

			private PathHandler debugData;
		}

		public class Builder : IAstarPooledObject
		{
			public void DrawMesh(RetainedGizmos gizmos, Vector3[] vertices, List<int> triangles, Color[] colors)
			{
				Mesh mesh = gizmos.GetMesh();
				mesh.vertices = vertices;
				mesh.SetTriangles(triangles, 0);
				mesh.colors = colors;
				mesh.UploadMeshData(true);
				this.meshes.Add(mesh);
			}

			public void DrawWireCube(GraphTransform tr, Bounds bounds, Color color)
			{
				Vector3 min = bounds.min;
				Vector3 max = bounds.max;
				this.DrawLine(tr.Transform(new Vector3(min.x, min.y, min.z)), tr.Transform(new Vector3(max.x, min.y, min.z)), color);
				this.DrawLine(tr.Transform(new Vector3(max.x, min.y, min.z)), tr.Transform(new Vector3(max.x, min.y, max.z)), color);
				this.DrawLine(tr.Transform(new Vector3(max.x, min.y, max.z)), tr.Transform(new Vector3(min.x, min.y, max.z)), color);
				this.DrawLine(tr.Transform(new Vector3(min.x, min.y, max.z)), tr.Transform(new Vector3(min.x, min.y, min.z)), color);
				this.DrawLine(tr.Transform(new Vector3(min.x, max.y, min.z)), tr.Transform(new Vector3(max.x, max.y, min.z)), color);
				this.DrawLine(tr.Transform(new Vector3(max.x, max.y, min.z)), tr.Transform(new Vector3(max.x, max.y, max.z)), color);
				this.DrawLine(tr.Transform(new Vector3(max.x, max.y, max.z)), tr.Transform(new Vector3(min.x, max.y, max.z)), color);
				this.DrawLine(tr.Transform(new Vector3(min.x, max.y, max.z)), tr.Transform(new Vector3(min.x, max.y, min.z)), color);
				this.DrawLine(tr.Transform(new Vector3(min.x, min.y, min.z)), tr.Transform(new Vector3(min.x, max.y, min.z)), color);
				this.DrawLine(tr.Transform(new Vector3(max.x, min.y, min.z)), tr.Transform(new Vector3(max.x, max.y, min.z)), color);
				this.DrawLine(tr.Transform(new Vector3(max.x, min.y, max.z)), tr.Transform(new Vector3(max.x, max.y, max.z)), color);
				this.DrawLine(tr.Transform(new Vector3(min.x, min.y, max.z)), tr.Transform(new Vector3(min.x, max.y, max.z)), color);
			}

			public void DrawLine(Vector3 start, Vector3 end, Color color)
			{
				this.lines.Add(start);
				this.lines.Add(end);
				Color32 item = color;
				this.lineColors.Add(item);
				this.lineColors.Add(item);
			}

			public void Submit(RetainedGizmos gizmos, RetainedGizmos.Hasher hasher)
			{
				this.SubmitLines(gizmos, hasher.Hash);
				this.SubmitMeshes(gizmos, hasher.Hash);
			}

			private void SubmitMeshes(RetainedGizmos gizmos, ulong hash)
			{
				for (int i = 0; i < this.meshes.Count; i++)
				{
					gizmos.meshes.Add(new RetainedGizmos.MeshWithHash
					{
						hash = hash,
						mesh = this.meshes[i],
						lines = false
					});
					gizmos.existingHashes.Add(hash);
				}
			}

			private void SubmitLines(RetainedGizmos gizmos, ulong hash)
			{
				int num = (this.lines.Count + 32766 - 1) / 32766;
				for (int i = 0; i < num; i++)
				{
					int num2 = 32766 * i;
					int num3 = Mathf.Min(num2 + 32766, this.lines.Count);
					int num4 = num3 - num2;
					List<Vector3> list = ListPool<Vector3>.Claim(num4 * 2);
					List<Color32> list2 = ListPool<Color32>.Claim(num4 * 2);
					List<Vector3> list3 = ListPool<Vector3>.Claim(num4 * 2);
					List<Vector2> list4 = ListPool<Vector2>.Claim(num4 * 2);
					List<int> list5 = ListPool<int>.Claim(num4 * 3);
					for (int j = num2; j < num3; j++)
					{
						Vector3 item = this.lines[j];
						list.Add(item);
						list.Add(item);
						Color32 item2 = this.lineColors[j];
						list2.Add(item2);
						list2.Add(item2);
						list4.Add(new Vector2(0f, 0f));
						list4.Add(new Vector2(1f, 0f));
					}
					for (int k = num2; k < num3; k += 2)
					{
						Vector3 item3 = this.lines[k + 1] - this.lines[k];
						list3.Add(item3);
						list3.Add(item3);
						list3.Add(item3);
						list3.Add(item3);
					}
					int l = 0;
					int num5 = 0;
					while (l < num4 * 3)
					{
						list5.Add(num5);
						list5.Add(num5 + 1);
						list5.Add(num5 + 2);
						list5.Add(num5 + 1);
						list5.Add(num5 + 3);
						list5.Add(num5 + 2);
						l += 6;
						num5 += 4;
					}
					Mesh mesh = gizmos.GetMesh();
					mesh.SetVertices(list);
					mesh.SetTriangles(list5, 0);
					mesh.SetColors(list2);
					mesh.SetNormals(list3);
					mesh.SetUVs(0, list4);
					mesh.UploadMeshData(true);
					ListPool<Vector3>.Release(list);
					ListPool<Color32>.Release(list2);
					ListPool<Vector3>.Release(list3);
					ListPool<Vector2>.Release(list4);
					ListPool<int>.Release(list5);
					gizmos.meshes.Add(new RetainedGizmos.MeshWithHash
					{
						hash = hash,
						mesh = mesh,
						lines = true
					});
					gizmos.existingHashes.Add(hash);
				}
			}

			void IAstarPooledObject.OnEnterPool()
			{
				this.lines.Clear();
				this.lineColors.Clear();
				this.meshes.Clear();
			}

			private List<Vector3> lines = new List<Vector3>();

			private List<Color32> lineColors = new List<Color32>();

			private List<Mesh> meshes = new List<Mesh>();
		}

		private struct MeshWithHash
		{
			public ulong hash;

			public Mesh mesh;

			public bool lines;
		}
	}
}
