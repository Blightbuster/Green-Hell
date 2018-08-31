using System;
using UnityEngine;

namespace Medvedya.VertexPainter
{
	[Serializable]
	public class ModifyInfo
	{
		public ModifyInfo(VertexPainter vertexPainter, Transform transform)
		{
			this.transform = transform;
			this.vertexPainter = vertexPainter;
		}

		public bool RayCast(Ray ray, out int triangleIndex, out float distance)
		{
			triangleIndex = 0;
			distance = float.MaxValue;
			if (this.vertexPainter == null)
			{
				return false;
			}
			if (this.vertexPainter.meshFilter.sharedMesh == null)
			{
				return false;
			}
			if (!this.vertexPainter.isLoadedMeshData)
			{
				return false;
			}
			Ray ray2 = new Ray(this.transform.InverseTransformPoint(ray.origin), this.transform.InverseTransformDirection(ray.direction));
			if (!this.vertexPainter.meshFilter.sharedMesh.bounds.IntersectRay(ray2))
			{
				return false;
			}
			bool flag = false;
			float num = float.MaxValue;
			for (int i = 0; i < this.vertexPainter.verteces.Length; i += 3)
			{
				Vector3 vector = this.vertexPainter.verteces[i];
				Vector3 vector2 = this.vertexPainter.verteces[i + 1];
				Vector3 vector3 = this.vertexPainter.verteces[i + 2];
				if (ModifyInfo.IntersectTriangle(vector, vector2, vector3, ray2))
				{
					Plane plane = new Plane(vector, vector2, vector3);
					float num2;
					if (plane.Raycast(ray2, out num2) && num2 < num)
					{
						num = num2;
						triangleIndex = i;
						flag = true;
					}
				}
			}
			if (flag)
			{
				distance = Vector3.Distance(this.transform.TransformPoint(ray2.GetPoint(num)), ray.origin);
			}
			return flag;
		}

		private static bool IntersectTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Ray ray)
		{
			Vector3 vector = p2 - p1;
			Vector3 vector2 = p3 - p1;
			Vector3 rhs = Vector3.Cross(ray.direction, vector2);
			float num = Vector3.Dot(vector, rhs);
			if (num > -Mathf.Epsilon && num < Mathf.Epsilon)
			{
				return false;
			}
			float num2 = 1f / num;
			Vector3 lhs = ray.origin - p1;
			float num3 = Vector3.Dot(lhs, rhs) * num2;
			if (num3 < 0f || num3 > 1f)
			{
				return false;
			}
			Vector3 rhs2 = Vector3.Cross(lhs, vector);
			float num4 = Vector3.Dot(ray.direction, rhs2) * num2;
			return num4 >= 0f && num3 + num4 <= 1f && Vector3.Dot(vector2, rhs2) * num2 > Mathf.Epsilon;
		}

		public Transform transform;

		public VertexPainter vertexPainter;
	}
}
