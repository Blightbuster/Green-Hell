using System;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding.Voxels
{
	public class RasterizationMesh
	{
		public RasterizationMesh()
		{
		}

		public RasterizationMesh(Vector3[] vertices, int[] triangles, Bounds bounds)
		{
			this.matrix = Matrix4x4.identity;
			this.vertices = vertices;
			this.numVertices = vertices.Length;
			this.triangles = triangles;
			this.numTriangles = triangles.Length;
			this.bounds = bounds;
			this.original = null;
			this.area = 0;
		}

		public RasterizationMesh(Vector3[] vertices, int[] triangles, Bounds bounds, Matrix4x4 matrix)
		{
			this.matrix = matrix;
			this.vertices = vertices;
			this.numVertices = vertices.Length;
			this.triangles = triangles;
			this.numTriangles = triangles.Length;
			this.bounds = bounds;
			this.original = null;
			this.area = 0;
		}

		public void RecalculateBounds()
		{
			Bounds bounds = new Bounds(this.matrix.MultiplyPoint3x4(this.vertices[0]), Vector3.zero);
			for (int i = 1; i < this.numVertices; i++)
			{
				bounds.Encapsulate(this.matrix.MultiplyPoint3x4(this.vertices[i]));
			}
			this.bounds = bounds;
		}

		public void Pool()
		{
			if (this.pool)
			{
				ArrayPool<int>.Release(ref this.triangles, false);
				ArrayPool<Vector3>.Release(ref this.vertices, false);
			}
		}

		public MeshFilter original;

		public int area;

		public Vector3[] vertices;

		public int[] triangles;

		public int numVertices;

		public int numTriangles;

		public Bounds bounds;

		public Matrix4x4 matrix;

		public bool pool;
	}
}
