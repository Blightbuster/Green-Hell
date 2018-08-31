using System;
using UnityEngine;

namespace UltimateWater
{
	public static class ColliderExtensions
	{
		public static float ComputeVolume(this Collider that)
		{
			BoxCollider boxCollider = that as BoxCollider;
			if (boxCollider != null)
			{
				return boxCollider.ComputeVolume();
			}
			SphereCollider sphereCollider = that as SphereCollider;
			if (sphereCollider != null)
			{
				return sphereCollider.ComputeVolume();
			}
			MeshCollider meshCollider = that as MeshCollider;
			if (meshCollider != null)
			{
				return meshCollider.ComputeVolume();
			}
			CapsuleCollider capsuleCollider = that as CapsuleCollider;
			if (capsuleCollider != null)
			{
				return capsuleCollider.ComputeVolume();
			}
			throw new NotImplementedException("UltimateWater: Unknown collider type.");
		}

		public static float ComputeVolume(this BoxCollider that)
		{
			Vector3 size = that.size;
			Vector3 lossyScale = that.transform.lossyScale;
			return size.x * lossyScale.x * size.y * lossyScale.y * size.z * lossyScale.z;
		}

		public static float ComputeVolume(this SphereCollider that)
		{
			float radius = that.radius;
			Vector3 lossyScale = that.transform.lossyScale;
			return 4.18879032f * radius * radius * radius * lossyScale.x * lossyScale.y * lossyScale.z;
		}

		public static float ComputeVolume(this MeshCollider that)
		{
			float num = 0f;
			Mesh sharedMesh = that.sharedMesh;
			Vector3[] vertices = sharedMesh.vertices;
			int[] triangles = sharedMesh.triangles;
			int num2 = triangles.Length;
			Vector3 b = that.transform.InverseTransformPoint(that.bounds.center);
			int i = 0;
			while (i < num2)
			{
				Vector3 p = vertices[triangles[i++]] - b;
				Vector3 p2 = vertices[triangles[i++]] - b;
				Vector3 p3 = vertices[triangles[i++]] - b;
				num += ColliderExtensions.SignedVolumeOfTriangle(p, p2, p3);
			}
			Vector3 lossyScale = that.transform.lossyScale;
			return Mathf.Abs(num) * lossyScale.x * lossyScale.y * lossyScale.z;
		}

		public static float ComputeVolume(this CapsuleCollider that)
		{
			float radius = that.radius;
			float num = 4.18879032f * radius * radius * radius;
			float num2 = 3.14159274f * radius * radius * that.height;
			Vector3 lossyScale = that.transform.lossyScale;
			return (num2 + num) * lossyScale.x * lossyScale.y * lossyScale.z;
		}

		public static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
		{
			float num = p3.x * p2.y * p1.z;
			float num2 = p2.x * p3.y * p1.z;
			float num3 = p3.x * p1.y * p2.z;
			float num4 = p1.x * p3.y * p2.z;
			float num5 = p2.x * p1.y * p3.z;
			float num6 = p1.x * p2.y * p3.z;
			return 0.166666672f * (-num + num2 + num3 - num4 - num5 + num6);
		}

		public static float ComputeArea(this Collider that)
		{
			MeshCollider meshCollider = that as MeshCollider;
			if (meshCollider != null)
			{
				return meshCollider.ComputeArea();
			}
			BoxCollider boxCollider = that as BoxCollider;
			if (boxCollider != null)
			{
				return boxCollider.ComputeArea();
			}
			SphereCollider sphereCollider = that as SphereCollider;
			if (sphereCollider != null)
			{
				return sphereCollider.ComputeArea();
			}
			CapsuleCollider capsuleCollider = that as CapsuleCollider;
			if (capsuleCollider != null)
			{
				return capsuleCollider.ComputeArea();
			}
			throw new NotImplementedException("UltimateWater: Unknown collider type.");
		}

		public static float ComputeArea(this MeshCollider that)
		{
			float num = 0f;
			Mesh sharedMesh = that.sharedMesh;
			Vector3[] vertices = sharedMesh.vertices;
			int[] triangles = sharedMesh.triangles;
			int num2 = triangles.Length;
			Vector3 lossyScale = that.transform.lossyScale;
			int i = 0;
			while (i < num2)
			{
				Vector3 b = vertices[triangles[i++]];
				Vector3 lhs = vertices[triangles[i++]] - b;
				Vector3 rhs = vertices[triangles[i++]] - b;
				lhs.Scale(lossyScale);
				rhs.Scale(lossyScale);
				num += Vector3.Cross(lhs, rhs).magnitude;
			}
			return num * 0.5f;
		}

		public static float ComputeArea(this BoxCollider that)
		{
			Vector3 size = that.size;
			size.Scale(that.transform.lossyScale);
			return 2f * (size.x * size.y + size.y * size.z + size.x * size.z);
		}

		public static float ComputeArea(this SphereCollider that)
		{
			float magnitude = that.transform.lossyScale.magnitude;
			float num = that.radius * magnitude;
			return 12.566371f * num * num;
		}

		public static float ComputeArea(this CapsuleCollider that)
		{
			Vector3 lossyScale = that.transform.lossyScale;
			float num = that.radius * lossyScale.magnitude;
			float num2 = that.height;
			switch (that.direction)
			{
			case 0:
				num2 *= lossyScale.x;
				break;
			case 1:
				num2 *= lossyScale.y;
				break;
			case 2:
				num2 *= lossyScale.z;
				break;
			default:
				throw new InvalidOperationException();
			}
			return 6.28318548f * num * (2f * num + num2);
		}

		public static Vector3 RandomPoint(this Collider that)
		{
			MeshCollider meshCollider = that as MeshCollider;
			if (meshCollider != null)
			{
				return meshCollider.RandomPoint();
			}
			BoxCollider boxCollider = that as BoxCollider;
			if (boxCollider != null)
			{
				return boxCollider.RandomPoint();
			}
			CapsuleCollider capsuleCollider = that as CapsuleCollider;
			if (capsuleCollider != null)
			{
				return capsuleCollider.RandomPoint();
			}
			SphereCollider sphereCollider = that as SphereCollider;
			if (sphereCollider != null)
			{
				return sphereCollider.RandomPoint();
			}
			throw new NotImplementedException("UltimateWater: Unknown collider type.");
		}

		public static Vector3 RandomPoint(this MeshCollider that)
		{
			Bounds bounds = that.sharedMesh.bounds;
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;
			Vector3 vector = max - min;
			Vector3 vector2 = default(Vector3);
			for (int i = 0; i < 40; i++)
			{
				vector2.x = min.x + UnityEngine.Random.value * vector.x;
				vector2.y = min.y + UnityEngine.Random.value * vector.y;
				vector2.z = min.z + UnityEngine.Random.value * vector.z;
				if (that.IsPointInside(that.transform.TransformPoint(vector2)))
				{
					break;
				}
			}
			return vector2;
		}

		public static Vector3 RandomPoint(this BoxCollider that)
		{
			Vector3 center = that.center;
			Vector3 vector = that.size * 0.5f;
			float x = center.x + UnityEngine.Random.Range(-vector.x, vector.x);
			float y = center.y + UnityEngine.Random.Range(-vector.y, vector.y);
			float z = center.z + UnityEngine.Random.Range(-vector.z, vector.z);
			return new Vector3(x, y, z);
		}

		public static Vector3 RandomPoint(this CapsuleCollider that)
		{
			float radius = that.radius;
			float height = that.height;
			float num = 3.14159274f * radius * radius * height;
			float num2 = 4.18879032f * radius * radius * radius;
			float num3 = UnityEngine.Random.Range(0f, num + num2);
			Vector3 result;
			if (num3 < num)
			{
				result = ColliderExtensions.RandomPointInCircle(radius);
				result.z = result.y;
				result.y = UnityEngine.Random.Range(-height * 0.5f, height * 0.5f);
			}
			else
			{
				result = ColliderExtensions.RandomPointInSphere(radius);
				if (result.y < 0f)
				{
					result.y -= height * 0.5f;
				}
				else
				{
					result.y += height * 0.5f;
				}
			}
			int direction = that.direction;
			if (direction != 0)
			{
				if (direction == 2)
				{
					float y = result.y;
					result.y = result.z;
					result.z = y;
				}
			}
			else
			{
				float y2 = result.y;
				result.y = result.x;
				result.x = y2;
			}
			return result;
		}

		public static Vector3 RandomPoint(this SphereCollider that)
		{
			return ColliderExtensions.RandomPointInSphere(that.radius);
		}

		public static Vector3 RandomPointInSphere(float radius)
		{
			float f = UnityEngine.Random.Range(-1f, 1f);
			float f2 = Mathf.Asin(f);
			float f3 = 6.28318548f * UnityEngine.Random.Range(0f, 1f);
			float num = 3f * Mathf.Pow(UnityEngine.Random.Range(0f, 1f), 0.333333343f);
			float num2 = Mathf.Sin(f2);
			return new Vector3(num * num2 * Mathf.Cos(f3), num * num2 * Mathf.Sin(f3), num * Mathf.Cos(f2));
		}

		public static Vector2 RandomPointInCircle(float radius)
		{
			float f = 6.28318548f * UnityEngine.Random.Range(0f, 1f);
			float num = UnityEngine.Random.Range(0f, 1f) + UnityEngine.Random.Range(0f, 1f);
			float num2 = ((num <= 1f) ? num : (2f - num)) * radius;
			return new Vector2(num2 * Mathf.Cos(f), num2 * Mathf.Sin(f));
		}

		public static void GetLocalMinMax(Collider collider, out Vector3 min, out Vector3 max)
		{
			MeshCollider meshCollider = collider as MeshCollider;
			if (meshCollider != null)
			{
				Bounds bounds = meshCollider.sharedMesh.bounds;
				min = bounds.min;
				max = bounds.max;
				return;
			}
			BoxCollider boxCollider = collider as BoxCollider;
			if (boxCollider != null)
			{
				BoxCollider boxCollider2 = boxCollider;
				min = boxCollider2.center - boxCollider2.size * 0.5f;
				max = boxCollider2.center + boxCollider2.size * 0.5f;
				return;
			}
			SphereCollider sphereCollider = collider as SphereCollider;
			if (sphereCollider != null)
			{
				SphereCollider sphereCollider2 = sphereCollider;
				Vector3 center = sphereCollider2.center;
				float num = sphereCollider2.radius * 0.5f;
				min = new Vector3(center.x - num, center.y - num, center.z - num);
				max = new Vector3(center.x + num, center.y + num, center.z + num);
				return;
			}
			CapsuleCollider capsuleCollider = collider as CapsuleCollider;
			if (capsuleCollider != null)
			{
				CapsuleCollider capsuleCollider2 = capsuleCollider;
				Vector3 center2 = capsuleCollider2.center;
				float num2 = capsuleCollider2.radius * 0.5f;
				float num3 = capsuleCollider2.height * 0.5f + num2;
				switch (capsuleCollider2.direction)
				{
				case 0:
					min = new Vector3(center2.x - num3, center2.y - num2, center2.z - num2);
					max = new Vector3(center2.x + num3, center2.y + num2, center2.z + num2);
					break;
				case 1:
					min = new Vector3(center2.x - num2, center2.y - num3, center2.z - num2);
					max = new Vector3(center2.x + num2, center2.y + num3, center2.z + num2);
					break;
				case 2:
					min = new Vector3(center2.x - num2, center2.y - num2, center2.z - num3);
					max = new Vector3(center2.x + num2, center2.y + num2, center2.z + num3);
					break;
				default:
					throw new InvalidOperationException();
				}
				return;
			}
			throw new InvalidOperationException();
		}

		public static bool IsPointInside(this Collider convex, Vector3 point)
		{
			Bounds bounds = convex.bounds;
			if (!bounds.Contains(point))
			{
				return false;
			}
			Vector3 direction = bounds.center - point;
			float magnitude = direction.magnitude;
			RaycastHit raycastHit;
			return magnitude < 1E-05f || !convex.Raycast(new Ray(point, direction), out raycastHit, magnitude);
		}
	}
}
