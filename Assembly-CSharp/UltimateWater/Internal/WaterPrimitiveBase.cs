using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateWater.Internal
{
	[Serializable]
	public abstract class WaterPrimitiveBase
	{
		public virtual Mesh[] GetTransformedMeshes(Camera camera, out Matrix4x4 matrix, int vertexCount, bool volume)
		{
			matrix = ((!(camera != null)) ? Matrix4x4.identity : this.GetMatrix(camera));
			int num = vertexCount;
			if (volume)
			{
				num = -num;
			}
			WaterPrimitiveBase.CachedMeshSet cachedMeshSet;
			if (!this._Cache.TryGetValue(num, out cachedMeshSet))
			{
				cachedMeshSet = (this._Cache[num] = new WaterPrimitiveBase.CachedMeshSet(this.CreateMeshes(vertexCount, volume)));
			}
			else
			{
				cachedMeshSet.Update();
			}
			return cachedMeshSet.Meshes;
		}

		public void Dispose()
		{
			foreach (WaterPrimitiveBase.CachedMeshSet cachedMeshSet in this._Cache.Values)
			{
				foreach (Mesh obj in cachedMeshSet.Meshes)
				{
					if (Application.isPlaying)
					{
						UnityEngine.Object.Destroy(obj);
					}
					else
					{
						UnityEngine.Object.DestroyImmediate(obj);
					}
				}
			}
			this._Cache.Clear();
		}

		protected abstract Matrix4x4 GetMatrix(Camera camera);

		protected abstract Mesh[] CreateMeshes(int vertexCount, bool volume);

		protected Mesh CreateMesh(Vector3[] vertices, int[] indices, string name, bool triangular = false)
		{
			Mesh mesh = new Mesh
			{
				hideFlags = HideFlags.DontSave,
				name = name,
				vertices = vertices
			};
			mesh.SetIndices(indices, (!triangular) ? MeshTopology.Quads : MeshTopology.Triangles, 0);
			mesh.RecalculateBounds();
			mesh.UploadMeshData(true);
			return mesh;
		}

		internal void Update()
		{
			int frameCount = Time.frameCount;
			if (this._KeysToRemove == null)
			{
				this._KeysToRemove = new List<int>();
			}
			Dictionary<int, WaterPrimitiveBase.CachedMeshSet>.Enumerator enumerator = this._Cache.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<int, WaterPrimitiveBase.CachedMeshSet> keyValuePair = enumerator.Current;
				if (frameCount - keyValuePair.Value.LastFrameUsed > 27)
				{
					this._KeysToRemove.Add(keyValuePair.Key);
					foreach (Mesh obj in keyValuePair.Value.Meshes)
					{
						if (Application.isPlaying)
						{
							UnityEngine.Object.Destroy(obj);
						}
						else
						{
							UnityEngine.Object.DestroyImmediate(obj);
						}
					}
				}
			}
			enumerator.Dispose();
			for (int j = 0; j < this._KeysToRemove.Count; j++)
			{
				this._Cache.Remove(this._KeysToRemove[j]);
			}
			this._KeysToRemove.Clear();
		}

		internal virtual void OnEnable(Water water)
		{
			this._Water = water;
		}

		internal virtual void OnDisable()
		{
			this.Dispose();
		}

		internal virtual void AddToMaterial(Water water)
		{
		}

		internal virtual void RemoveFromMaterial(Water water)
		{
		}

		protected Water _Water;

		protected Dictionary<int, WaterPrimitiveBase.CachedMeshSet> _Cache = new Dictionary<int, WaterPrimitiveBase.CachedMeshSet>(Int32EqualityComparer.Default);

		private List<int> _KeysToRemove;

		protected class CachedMeshSet
		{
			public CachedMeshSet(Mesh[] meshes)
			{
				this.Meshes = meshes;
				this.Update();
			}

			public void Update()
			{
				this.LastFrameUsed = Time.frameCount;
			}

			public Mesh[] Meshes;

			public int LastFrameUsed;
		}
	}
}
