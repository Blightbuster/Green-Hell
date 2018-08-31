using System;
using UnityEngine;

namespace CJTools
{
	public class Triangle
	{
		public bool Intersect(Ray ray, out Vector3 intersection_point)
		{
			Vector3 vector = this.p1 - this.p0;
			Vector3 vector2 = this.p2 - this.p0;
			Vector3 rhs = Vector3.Cross(ray.direction, vector2);
			float num = Vector3.Dot(vector, rhs);
			if (num > -Mathf.Epsilon && num < Mathf.Epsilon)
			{
				intersection_point = Vector3.zero;
				return false;
			}
			float num2 = 1f / num;
			Vector3 lhs = ray.origin - this.p0;
			float num3 = Vector3.Dot(lhs, rhs) * num2;
			if (num3 < 0f || num3 > 1f)
			{
				intersection_point = Vector3.zero;
				return false;
			}
			Vector3 rhs2 = Vector3.Cross(lhs, vector);
			float num4 = Vector3.Dot(ray.direction, rhs2) * num2;
			if (num4 < 0f || num3 + num4 > 1f)
			{
				intersection_point = Vector3.zero;
				return false;
			}
			float num5 = Vector3.Dot(vector2, rhs2) * num2;
			if (num5 > Mathf.Epsilon)
			{
				intersection_point = ray.origin + ray.direction.normalized * (num5 * ray.direction.magnitude);
				return true;
			}
			intersection_point = Vector3.zero;
			return false;
		}

		private static void Sort(ref Vector2 v)
		{
			if (v.x > v.y)
			{
				float x = v.x;
				v.x = v.y;
				v.y = x;
			}
		}

		private static bool EdgeEdgeTest(Vector3 v0, Vector3 v1, Vector3 u0, Vector3 u1, int i0, int i1)
		{
			float num = v1[i0] - v0[i0];
			float num2 = v1[i1] - v0[i1];
			float num3 = u0[i0] - u1[i0];
			float num4 = u0[i1] - u1[i1];
			float num5 = v0[i0] - u0[i0];
			float num6 = v0[i1] - u0[i1];
			float num7 = num2 * num3 - num * num4;
			float num8 = num4 * num5 - num3 * num6;
			if ((num7 > 0f && num8 >= 0f && num8 <= num7) || (num7 < 0f && num8 <= 0f && num8 >= num7))
			{
				float num9 = num * num6 - num2 * num5;
				if (num7 > 0f)
				{
					if (num9 >= 0f && num9 <= num7)
					{
						return true;
					}
				}
				else if (num9 <= 0f && num9 >= num7)
				{
					return true;
				}
			}
			return false;
		}

		private static bool EdgeAgainstTriEdges(Vector3 v0, Vector3 v1, Vector3 u0, Vector3 u1, Vector3 u2, short i0, short i1)
		{
			return Triangle.EdgeEdgeTest(v0, v1, u0, u1, (int)i0, (int)i1) || Triangle.EdgeEdgeTest(v0, v1, u1, u2, (int)i0, (int)i1) || Triangle.EdgeEdgeTest(v0, v1, u2, u0, (int)i0, (int)i1);
		}

		private static bool PointInTri(Vector3 v0, Vector3 u0, Vector3 u1, Vector3 u2, short i0, short i1)
		{
			float num = u1[(int)i1] - u0[(int)i1];
			float num2 = -(u1[(int)i0] - u0[(int)i0]);
			float num3 = -num * u0[(int)i0] - num2 * u0[(int)i1];
			float num4 = num * v0[(int)i0] + num2 * v0[(int)i1] + num3;
			num = u2[(int)i1] - u1[(int)i1];
			num2 = -(u2[(int)i0] - u1[(int)i0]);
			num3 = -num * u1[(int)i0] - num2 * u1[(int)i1];
			float num5 = num * v0[(int)i0] + num2 * v0[(int)i1] + num3;
			num = u0[(int)i1] - u2[(int)i1];
			num2 = -(u0[(int)i0] - u2[(int)i0]);
			num3 = -num * u2[(int)i0] - num2 * u2[(int)i1];
			float num6 = num * v0[(int)i0] + num2 * v0[(int)i1] + num3;
			return num4 * num5 > 0f && num4 * num6 > 0f;
		}

		private static bool TriTriCoplanar(Vector3 N, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 u0, Vector3 u1, Vector3 u2)
		{
			float[] array = new float[]
			{
				Mathf.Abs(N[0]),
				Mathf.Abs(N[1]),
				Mathf.Abs(N[2])
			};
			short i;
			short i2;
			if (array[0] > array[1])
			{
				if (array[0] > array[2])
				{
					i = 1;
					i2 = 2;
				}
				else
				{
					i = 0;
					i2 = 1;
				}
			}
			else if (array[2] > array[1])
			{
				i = 0;
				i2 = 1;
			}
			else
			{
				i = 0;
				i2 = 2;
			}
			return Triangle.EdgeAgainstTriEdges(v0, v1, u0, u1, u2, i, i2) || Triangle.EdgeAgainstTriEdges(v1, v2, u0, u1, u2, i, i2) || Triangle.EdgeAgainstTriEdges(v2, v0, u0, u1, u2, i, i2) || Triangle.PointInTri(v0, u0, u1, u2, i, i2) || Triangle.PointInTri(u0, v0, v1, v2, i, i2);
		}

		private static bool ComputeIntervals(float VV0, float VV1, float VV2, float D0, float D1, float D2, float D0D1, float D0D2, ref float A, ref float B, ref float C, ref float X0, ref float X1)
		{
			if (D0D1 > 0f)
			{
				A = VV2;
				B = (VV0 - VV2) * D2;
				C = (VV1 - VV2) * D2;
				X0 = D2 - D0;
				X1 = D2 - D1;
			}
			else if (D0D2 > 0f)
			{
				A = VV1;
				B = (VV0 - VV1) * D1;
				C = (VV2 - VV1) * D1;
				X0 = D1 - D0;
				X1 = D1 - D2;
			}
			else if (D1 * D2 > 0f || D0 != 0f)
			{
				A = VV0;
				B = (VV1 - VV0) * D0;
				C = (VV2 - VV0) * D0;
				X0 = D0 - D1;
				X1 = D0 - D2;
			}
			else if (D1 != 0f)
			{
				A = VV1;
				B = (VV0 - VV1) * D1;
				C = (VV2 - VV1) * D1;
				X0 = D1 - D0;
				X1 = D1 - D2;
			}
			else
			{
				if (D2 == 0f)
				{
					return true;
				}
				A = VV2;
				B = (VV0 - VV2) * D2;
				C = (VV1 - VV2) * D2;
				X0 = D2 - D0;
				X1 = D2 - D1;
			}
			return false;
		}

		public static bool TriTriIntersect(Triangle t0, Triangle t1)
		{
			return Triangle.TriTriIntersect(t0.p0, t0.p1, t0.p2, t1.p0, t1.p1, t1.p2);
		}

		public static bool TriTriIntersect(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 u0, Vector3 u1, Vector3 u2)
		{
			Vector2 zero = Vector2.zero;
			Vector2 zero2 = Vector2.zero;
			Vector3 lhs = v1 - v0;
			Vector3 rhs = v2 - v0;
			Vector3 vector = Vector3.Cross(lhs, rhs);
			float num = -Vector3.Dot(vector, v0);
			float num2 = Vector3.Dot(vector, u0) + num;
			float num3 = Vector3.Dot(vector, u1) + num;
			float num4 = Vector3.Dot(vector, u2) + num;
			if (Mathf.Abs(num2) < Mathf.Epsilon)
			{
				num2 = 0f;
			}
			if (Mathf.Abs(num3) < Mathf.Epsilon)
			{
				num3 = 0f;
			}
			if (Mathf.Abs(num4) < Mathf.Epsilon)
			{
				num4 = 0f;
			}
			float num5 = num2 * num3;
			float num6 = num2 * num4;
			if (num5 > 0f && num6 > 0f)
			{
				return false;
			}
			lhs = u1 - u0;
			rhs = u2 - u0;
			Vector3 vector2 = Vector3.Cross(lhs, rhs);
			float num7 = -Vector3.Dot(vector2, u0);
			float num8 = Vector3.Dot(vector2, v0) + num7;
			float num9 = Vector3.Dot(vector2, v1) + num7;
			float num10 = Vector3.Dot(vector2, v2) + num7;
			if (Mathf.Abs(num8) < Mathf.Epsilon)
			{
				num8 = 0f;
			}
			if (Mathf.Abs(num9) < Mathf.Epsilon)
			{
				num9 = 0f;
			}
			if (Mathf.Abs(num10) < Mathf.Epsilon)
			{
				num10 = 0f;
			}
			float num11 = num8 * num9;
			float num12 = num8 * num10;
			if (num11 > 0f && num12 > 0f)
			{
				return false;
			}
			Vector3 vector3 = Vector3.Cross(vector, vector2);
			float num13 = Mathf.Abs(vector3[0]);
			short index = 0;
			float num14 = Mathf.Abs(vector3[1]);
			float num15 = Mathf.Abs(vector3[2]);
			if (num14 > num13)
			{
				num13 = num14;
				index = 1;
			}
			if (num15 > num13)
			{
				index = 2;
			}
			float vv = v0[(int)index];
			float vv2 = v1[(int)index];
			float vv3 = v2[(int)index];
			float vv4 = u0[(int)index];
			float vv5 = u1[(int)index];
			float vv6 = u2[(int)index];
			float num16 = 0f;
			float num17 = 0f;
			float num18 = 0f;
			float num19 = 0f;
			float num20 = 0f;
			if (Triangle.ComputeIntervals(vv, vv2, vv3, num8, num9, num10, num11, num12, ref num16, ref num17, ref num18, ref num19, ref num20))
			{
				return Triangle.TriTriCoplanar(vector, v0, v1, v2, u0, u1, u2);
			}
			float num21 = 0f;
			float num22 = 0f;
			float num23 = 0f;
			float num24 = 0f;
			float num25 = 0f;
			if (Triangle.ComputeIntervals(vv4, vv5, vv6, num2, num3, num4, num5, num6, ref num21, ref num22, ref num23, ref num24, ref num25))
			{
				return Triangle.TriTriCoplanar(vector, v0, v1, v2, u0, u1, u2);
			}
			float num26 = num19 * num20;
			float num27 = num24 * num25;
			float num28 = num26 * num27;
			float num29 = num16 * num28;
			zero[0] = num29 + num17 * num20 * num27;
			zero[1] = num29 + num18 * num19 * num27;
			num29 = num21 * num28;
			zero2[0] = num29 + num22 * num26 * num25;
			zero2[1] = num29 + num23 * num26 * num24;
			Triangle.Sort(ref zero);
			Triangle.Sort(ref zero2);
			return zero[1] >= zero2[0] && zero2[1] >= zero[0];
		}

		public static bool TriOBBIntersect(Triangle triangle, OBB box)
		{
			return Triangle.TriAABBIntersect(new Triangle
			{
				p0 = box.transform.InverseTransformPoint(triangle.p0),
				p1 = box.transform.InverseTransformPoint(triangle.p1),
				p2 = box.transform.InverseTransformPoint(triangle.p2)
			}, new AABB
			{
				half_sizes = box.half_sizes,
				start = box.start
			});
		}

		public static bool TriAABBIntersect(Triangle triangle, AABB box)
		{
			Vector3[] array = new Vector3[]
			{
				new Vector3(1f, 0f, 0f),
				new Vector3(0f, 1f, 0f),
				new Vector3(0f, 0f, 1f)
			};
			for (int i = 0; i < 3; i++)
			{
				double num;
				double num2;
				Triangle.Project(triangle, array[i], out num, out num2);
				if (i == 0 && (num2 < (double)(box.start.x - box.half_sizes.x) || num > (double)(box.start.x + box.half_sizes.x)))
				{
					return false;
				}
				if (i == 1 && (num2 < (double)(box.start.y - box.half_sizes.y) || num > (double)(box.start.y + box.half_sizes.y)))
				{
					return false;
				}
				if (i == 2 && (num2 < (double)(box.start.z - box.half_sizes.z) || num > (double)(box.start.z + box.half_sizes.z)))
				{
					return false;
				}
			}
			Vector3 vector = Vector3.Cross(triangle.p0 - triangle.p1, triangle.p2 - triangle.p1);
			double num3 = (double)Vector3.Dot(vector, triangle.p0);
			double num4;
			double num5;
			Triangle.Project(box, vector, out num4, out num5);
			if (num5 < num3 || num4 > num3)
			{
				return false;
			}
			Vector3[] array2 = new Vector3[]
			{
				triangle.p0 - triangle.p1,
				triangle.p1 - triangle.p2,
				triangle.p2 - triangle.p0
			};
			for (int j = 0; j < 3; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					Vector3 axis = Vector3.Cross(array2[j], array[k]);
					Triangle.Project(box, axis, out num4, out num5);
					double num;
					double num2;
					Triangle.Project(triangle, axis, out num, out num2);
					if (num5 <= num || num4 >= num2)
					{
						return false;
					}
				}
			}
			return true;
		}

		private static void Project(Triangle tri, Vector3 axis, out double min, out double max)
		{
			min = double.PositiveInfinity;
			max = double.NegativeInfinity;
			for (int i = 0; i < 3; i++)
			{
				Vector3 rhs = default(Vector3);
				if (i == 0)
				{
					rhs = tri.p0;
				}
				else if (i == 1)
				{
					rhs = tri.p1;
				}
				else if (i == 2)
				{
					rhs = tri.p2;
				}
				double num = (double)Vector3.Dot(axis, rhs);
				if (num < min)
				{
					min = num;
				}
				if (num > max)
				{
					max = num;
				}
			}
		}

		private static void Project(AABB box, Vector3 axis, out double min, out double max)
		{
			min = double.PositiveInfinity;
			max = double.NegativeInfinity;
			for (int i = 0; i < 8; i++)
			{
				Vector3 rhs = default(Vector3);
				if (i == 0)
				{
					rhs = box.start - Vector3.left * box.half_sizes.x + Vector3.up * box.half_sizes.y - Vector3.forward * box.half_sizes.z;
				}
				else if (i == 1)
				{
					rhs = box.start + Vector3.left * box.half_sizes.x + Vector3.up * box.half_sizes.y - Vector3.forward * box.half_sizes.z;
				}
				else if (i == 2)
				{
					rhs = box.start - Vector3.left * box.half_sizes.x - Vector3.up * box.half_sizes.y - Vector3.forward * box.half_sizes.z;
				}
				else if (i == 3)
				{
					rhs = box.start + Vector3.left * box.half_sizes.x - Vector3.up * box.half_sizes.y - Vector3.forward * box.half_sizes.z;
				}
				else if (i == 4)
				{
					rhs = box.start - Vector3.left * box.half_sizes.x + Vector3.up * box.half_sizes.y + Vector3.forward * box.half_sizes.z;
				}
				else if (i == 5)
				{
					rhs = box.start + Vector3.left * box.half_sizes.x + Vector3.up * box.half_sizes.y + Vector3.forward * box.half_sizes.z;
				}
				else if (i == 6)
				{
					rhs = box.start + Vector3.left * box.half_sizes.x - Vector3.up * box.half_sizes.y + Vector3.forward * box.half_sizes.z;
				}
				else if (i == 7)
				{
					rhs = box.start - Vector3.left * box.half_sizes.x - Vector3.up * box.half_sizes.y + Vector3.forward * box.half_sizes.z;
				}
				double num = (double)Vector3.Dot(axis, rhs);
				if (num < min)
				{
					min = num;
				}
				if (num > max)
				{
					max = num;
				}
			}
		}

		public Vector3 p0;

		public Vector3 p1;

		public Vector3 p2;
	}
}
