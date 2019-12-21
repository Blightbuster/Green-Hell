using System;
using UnityEngine;

namespace Pathfinding.Util
{
	public class GraphTransform : IMovementPlane, ITransform
	{
		public GraphTransform(Matrix4x4 matrix)
		{
			this.matrix = matrix;
			this.inverseMatrix = matrix.inverse;
			this.identity = matrix.isIdentity;
			this.onlyTranslational = GraphTransform.MatrixIsTranslational(matrix);
			this.up = matrix.MultiplyVector(Vector3.up).normalized;
			this.translation = matrix.MultiplyPoint3x4(Vector3.zero);
			this.i3translation = (Int3)this.translation;
			this.rotation = Quaternion.LookRotation(this.TransformVector(Vector3.forward), this.TransformVector(Vector3.up));
			this.inverseRotation = Quaternion.Inverse(this.rotation);
			this.isXY = (this.rotation == Quaternion.Euler(-90f, 0f, 0f));
			this.isXZ = (this.rotation == Quaternion.Euler(0f, 0f, 0f));
		}

		public Vector3 WorldUpAtGraphPosition(Vector3 p)
		{
			return this.up;
		}

		private static bool MatrixIsTranslational(Matrix4x4 m)
		{
			return m.GetColumn(0) == new Vector4(1f, 0f, 0f, 0f) && m.GetColumn(1) == new Vector4(0f, 1f, 0f, 0f) && m.GetColumn(2) == new Vector4(0f, 0f, 1f, 0f) && m.m33 == 1f;
		}

		public Vector3 Transform(Vector3 p)
		{
			if (this.onlyTranslational)
			{
				return p + this.translation;
			}
			return this.matrix.MultiplyPoint3x4(p);
		}

		public Vector3 TransformVector(Vector3 p)
		{
			if (this.onlyTranslational)
			{
				return p;
			}
			return this.matrix.MultiplyVector(p);
		}

		public void Transform(Int3[] arr)
		{
			if (this.onlyTranslational)
			{
				for (int i = arr.Length - 1; i >= 0; i--)
				{
					arr[i] += this.i3translation;
				}
				return;
			}
			for (int j = arr.Length - 1; j >= 0; j--)
			{
				arr[j] = (Int3)this.matrix.MultiplyPoint3x4((Vector3)arr[j]);
			}
		}

		public void Transform(Vector3[] arr)
		{
			if (this.onlyTranslational)
			{
				for (int i = arr.Length - 1; i >= 0; i--)
				{
					arr[i] += this.translation;
				}
				return;
			}
			for (int j = arr.Length - 1; j >= 0; j--)
			{
				arr[j] = this.matrix.MultiplyPoint3x4(arr[j]);
			}
		}

		public Vector3 InverseTransform(Vector3 p)
		{
			if (this.onlyTranslational)
			{
				return p - this.translation;
			}
			return this.inverseMatrix.MultiplyPoint3x4(p);
		}

		public Int3 InverseTransform(Int3 p)
		{
			if (this.onlyTranslational)
			{
				return p - this.i3translation;
			}
			return (Int3)this.inverseMatrix.MultiplyPoint3x4((Vector3)p);
		}

		public void InverseTransform(Int3[] arr)
		{
			for (int i = arr.Length - 1; i >= 0; i--)
			{
				arr[i] = (Int3)this.inverseMatrix.MultiplyPoint3x4((Vector3)arr[i]);
			}
		}

		public static GraphTransform operator *(GraphTransform lhs, Matrix4x4 rhs)
		{
			return new GraphTransform(lhs.matrix * rhs);
		}

		public static GraphTransform operator *(Matrix4x4 lhs, GraphTransform rhs)
		{
			return new GraphTransform(lhs * rhs.matrix);
		}

		public Bounds Transform(Bounds b)
		{
			if (this.onlyTranslational)
			{
				return new Bounds(b.center + this.translation, b.size);
			}
			Vector3[] array = ArrayPool<Vector3>.Claim(8);
			Vector3 extents = b.extents;
			array[0] = this.Transform(b.center + new Vector3(extents.x, extents.y, extents.z));
			array[1] = this.Transform(b.center + new Vector3(extents.x, extents.y, -extents.z));
			array[2] = this.Transform(b.center + new Vector3(extents.x, -extents.y, extents.z));
			array[3] = this.Transform(b.center + new Vector3(extents.x, -extents.y, -extents.z));
			array[4] = this.Transform(b.center + new Vector3(-extents.x, extents.y, extents.z));
			array[5] = this.Transform(b.center + new Vector3(-extents.x, extents.y, -extents.z));
			array[6] = this.Transform(b.center + new Vector3(-extents.x, -extents.y, extents.z));
			array[7] = this.Transform(b.center + new Vector3(-extents.x, -extents.y, -extents.z));
			Vector3 vector = array[0];
			Vector3 vector2 = array[0];
			for (int i = 1; i < 8; i++)
			{
				vector = Vector3.Min(vector, array[i]);
				vector2 = Vector3.Max(vector2, array[i]);
			}
			ArrayPool<Vector3>.Release(ref array, false);
			return new Bounds((vector + vector2) * 0.5f, vector2 - vector);
		}

		public Bounds InverseTransform(Bounds b)
		{
			if (this.onlyTranslational)
			{
				return new Bounds(b.center - this.translation, b.size);
			}
			Vector3[] array = ArrayPool<Vector3>.Claim(8);
			Vector3 extents = b.extents;
			array[0] = this.InverseTransform(b.center + new Vector3(extents.x, extents.y, extents.z));
			array[1] = this.InverseTransform(b.center + new Vector3(extents.x, extents.y, -extents.z));
			array[2] = this.InverseTransform(b.center + new Vector3(extents.x, -extents.y, extents.z));
			array[3] = this.InverseTransform(b.center + new Vector3(extents.x, -extents.y, -extents.z));
			array[4] = this.InverseTransform(b.center + new Vector3(-extents.x, extents.y, extents.z));
			array[5] = this.InverseTransform(b.center + new Vector3(-extents.x, extents.y, -extents.z));
			array[6] = this.InverseTransform(b.center + new Vector3(-extents.x, -extents.y, extents.z));
			array[7] = this.InverseTransform(b.center + new Vector3(-extents.x, -extents.y, -extents.z));
			Vector3 vector = array[0];
			Vector3 vector2 = array[0];
			for (int i = 1; i < 8; i++)
			{
				vector = Vector3.Min(vector, array[i]);
				vector2 = Vector3.Max(vector2, array[i]);
			}
			ArrayPool<Vector3>.Release(ref array, false);
			return new Bounds((vector + vector2) * 0.5f, vector2 - vector);
		}

		Vector2 IMovementPlane.ToPlane(Vector3 p)
		{
			if (this.isXY)
			{
				return new Vector2(p.x, p.y);
			}
			if (!this.isXZ)
			{
				p = this.inverseRotation * p;
			}
			return new Vector2(p.x, p.z);
		}

		Vector2 IMovementPlane.ToPlane(Vector3 p, out float elevation)
		{
			if (!this.isXZ)
			{
				p = this.inverseRotation * p;
			}
			elevation = p.y;
			return new Vector2(p.x, p.z);
		}

		Vector3 IMovementPlane.ToWorld(Vector2 p, float elevation)
		{
			return this.rotation * new Vector3(p.x, elevation, p.y);
		}

		public readonly bool identity;

		public readonly bool onlyTranslational;

		private readonly bool isXY;

		private readonly bool isXZ;

		private readonly Matrix4x4 matrix;

		private readonly Matrix4x4 inverseMatrix;

		private readonly Vector3 up;

		private readonly Vector3 translation;

		private readonly Int3 i3translation;

		private readonly Quaternion rotation;

		private readonly Quaternion inverseRotation;

		public static readonly GraphTransform identityTransform = new GraphTransform(Matrix4x4.identity);
	}
}
