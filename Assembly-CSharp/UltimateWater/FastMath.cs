using System;
using UnityEngine;

namespace UltimateWater
{
	public class FastMath
	{
		static FastMath()
		{
			FastMath.PrecomputeFastSines();
		}

		private static void PrecomputeFastSines()
		{
			for (int i = 0; i < 2048; i++)
			{
				FastMath.Sines[i] = Mathf.Sin((float)i * 0.00306796166f);
			}
			for (int j = 0; j < 2048; j++)
			{
				FastMath.Cosines[j] = Mathf.Cos((float)j * 0.00306796166f);
			}
			for (int k = 0; k < 2048; k++)
			{
				FastMath.PositiveTanhSqrt[k] = (FastMath.PositiveTanhSqrtNoZero[k] = Mathf.Sqrt((float)Math.Tanh((double)((float)k * 0.001953125f))));
			}
			FastMath.PositiveTanhSqrtNoZero[0] = 2E-05f;
		}

		public static float Sin2048(float x)
		{
			int num = (int)(x * 325.949f) & 2047;
			return FastMath.Sines[num];
		}

		public static float Cos2048(float x)
		{
			int num = (int)(x * 325.949f) & 2047;
			return FastMath.Cosines[num];
		}

		public static void SinCos2048(float x, out float s, out float c)
		{
			int num = (int)(x * 325.949f) & 2047;
			s = FastMath.Sines[num];
			c = FastMath.Cosines[num];
		}

		public static float TanhSqrt2048Positive(float x)
		{
			int num = (int)(x * 512f);
			return (num < 2048) ? FastMath.PositiveTanhSqrt[num] : 1f;
		}

		public static float Pow2(float x)
		{
			return x * x;
		}

		public static float Pow4(float x)
		{
			float num = x * x;
			return num * num;
		}

		public static int FloorToInt(float f)
		{
			int num = (int)f;
			if ((float)num > f)
			{
				num--;
			}
			return num;
		}

		public static float Angle180(float f)
		{
			return f - (float)Math.Floor((double)((f + 180f) / 360f)) * 360f;
		}

		public static float Angle360(float f)
		{
			f %= 360f;
			return (f < 0f) ? (f + 360f) : f;
		}

		public static void Normalize(ref Vector3 a, ref Vector3 b)
		{
			double num = Math.Sqrt((double)(a.x * a.x + a.y * a.y + a.z * a.z));
			if (num > 9.99999974737875E-06)
			{
				float num2 = (float)(1.0 / num);
				b.x = a.x * num2;
				b.y = a.y * num2;
				b.z = a.z * num2;
			}
			else
			{
				b.x = 0f;
				b.y = 0f;
				b.z = 0f;
			}
		}

		public static void Normalize(ref Vector3 a)
		{
			double num = Math.Sqrt((double)(a.x * a.x + a.y * a.y + a.z * a.z));
			if (num > 9.99999974737875E-06)
			{
				float num2 = (float)(1.0 / num);
				a.x *= num2;
				a.y *= num2;
				a.z *= num2;
			}
			else
			{
				a.x = 0f;
				a.y = 0f;
				a.z = 0f;
			}
		}

		public static float Magnitude(ref Vector3 a)
		{
			return (float)Math.Sqrt((double)(a.x * a.x + a.y * a.y + a.z * a.z));
		}

		public static float TriangleArea(ref Vector3 a, ref Vector3 b, ref Vector3 c)
		{
			double num = (double)(c.x - a.x);
			double num2 = (double)(c.y - a.y);
			double num3 = (double)(c.z - a.z);
			double num4 = (double)(b.x - a.x);
			double num5 = (double)(b.y - a.y);
			double num6 = (double)(b.z - a.z);
			double num7 = num5 * num3 - num6 * num2;
			double num8 = num6 * num - num4 * num3;
			double num9 = num4 * num2 - num5 * num;
			return (float)(Math.Sqrt(num7 * num7 + num8 * num8 + num9 * num9) * 0.5);
		}

		public static void MultiplyVector(ref Matrix4x4 matrix, ref Vector3 a, ref Vector3 b)
		{
			b.x = matrix.m00 * a.x + matrix.m01 * a.y + matrix.m02 * a.z;
			b.y = matrix.m10 * a.x + matrix.m11 * a.y + matrix.m12 * a.z;
			b.z = matrix.m20 * a.x + matrix.m21 * a.y + matrix.m22 * a.z;
		}

		public static void MultiplyPoint(ref Matrix4x4 matrix, ref Vector3 a, ref Vector3 b)
		{
			b.x = matrix.m00 * a.x + matrix.m01 * a.y + matrix.m02 * a.z + matrix.m03;
			b.y = matrix.m10 * a.x + matrix.m11 * a.y + matrix.m12 * a.z + matrix.m13;
			b.z = matrix.m20 * a.x + matrix.m21 * a.y + matrix.m22 * a.z + matrix.m23;
		}

		public static Vector2 ProjectOntoLine(Vector2 a, Vector2 b, Vector2 target)
		{
			Vector2 vector = b - a;
			Vector2 rhs = target - a;
			return a + Vector2.Dot(vector, rhs) * vector / vector.sqrMagnitude;
		}

		public static float DistanceToLine(Vector3 a, Vector3 b, Vector3 target)
		{
			Vector3 vector = b - a;
			Vector3 rhs = target - a;
			Vector3 a2 = a + Vector3.Dot(vector, rhs) * vector / vector.sqrMagnitude;
			return Vector3.Distance(a2, target);
		}

		public static float DistanceToSegment(Vector3 a, Vector3 b, Vector3 target)
		{
			Vector3 vector = b - a;
			Vector3 rhs = target - a;
			Vector3 vector2 = a + Vector3.Dot(vector, rhs) * vector / vector.sqrMagnitude;
			if (Vector3.Dot((a - vector2).normalized, (b - vector2).normalized) < 0f)
			{
				return Vector3.Distance(vector2, target);
			}
			return Mathf.Min(rhs.magnitude, Vector3.Distance(b, target));
		}

		public static float DistanceToSegment(Vector2 a, Vector2 b, Vector2 target)
		{
			Vector2 vector = b - a;
			Vector2 rhs = target - a;
			Vector2 vector2 = a + Vector2.Dot(vector, rhs) * vector / vector.sqrMagnitude;
			if (Vector2.Dot((a - vector2).normalized, (b - vector2).normalized) < 0f)
			{
				return Vector2.Distance(vector2, target);
			}
			return Mathf.Min(rhs.magnitude, Vector2.Distance(b, target));
		}

		public static Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 target)
		{
			Vector2 vector = b - a;
			Vector2 rhs = target - a;
			Vector2 vector2 = a + Vector2.Dot(vector, rhs) * vector / vector.sqrMagnitude;
			if (Vector2.Dot((a - vector2).normalized, (b - vector2).normalized) < 0f)
			{
				return vector2;
			}
			if (Vector2.Distance(a, target) < Vector2.Distance(b, target))
			{
				return a;
			}
			return b;
		}

		public static bool IsPointInsideTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 target)
		{
			float num = target.x - a.x;
			float num2 = target.y - a.y;
			bool flag = (b.x - a.x) * num2 - (b.y - a.y) * num > 0f;
			return (c.x - a.x) * num2 - (c.y - a.y) * num > 0f != flag && (c.x - b.x) * (target.y - b.y) - (c.y - b.y) * (target.x - b.x) > 0f == flag;
		}

		public static float Gauss01()
		{
			return Mathf.Sqrt(-2f * Mathf.Log(UnityEngine.Random.Range(1E-06f, 1f))) * Mathf.Sin(6.28318548f * UnityEngine.Random.value);
		}

		public static float Gauss(float mean, float stdDev)
		{
			return mean + stdDev * Mathf.Sqrt(-2f * Mathf.Log(UnityEngine.Random.Range(1E-06f, 1f))) * Mathf.Sin(6.28318548f * UnityEngine.Random.value);
		}

		public static float Gauss01(float u1, float u2)
		{
			return Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(6.28318548f * u2);
		}

		public static float FracAdditive(float value)
		{
			return value - (float)((int)value);
		}

		public static Vector2 Rotate(Vector2 vector, float angle)
		{
			float num;
			float num2;
			FastMath.SinCos2048(angle, out num, out num2);
			return new Vector2(vector.x * num2 + vector.y * num, vector.x * num + vector.y * num2);
		}

		public static float Interpolate(float a0, float a1, float a2, float a3, float b0, float b1, float b2, float b3, float fx, float invFx, float fy, float invFy, float t)
		{
			float num = a0 * fx + a1 * invFx;
			float num2 = a2 * fx + a3 * invFx;
			float num3 = num * fy + num2 * invFy;
			float num4 = b0 * fx + b1 * invFx;
			float num5 = b2 * fx + b3 * invFx;
			float num6 = num4 * fy + num5 * invFy;
			return num3 * (1f - t) + num6 * t;
		}

		public static Vector2 Interpolate(ref Vector2 a0, ref Vector2 a1, ref Vector2 a2, ref Vector2 a3, ref Vector2 b0, ref Vector2 b1, ref Vector2 b2, ref Vector2 b3, float fx, float invFx, float fy, float invFy, float t)
		{
			if (fx != 0f)
			{
				float num = invFx / fx;
				float num2 = a0.x + a1.x * num;
				float num3 = a0.y + a1.y * num;
				float num4 = a2.x + a3.x * num;
				float num5 = a2.y + a3.y * num;
				float num6 = num2 * fy + num4 * invFy;
				float num7 = num3 * fy + num5 * invFy;
				float num8 = b0.x + b1.x * num;
				float num9 = b0.y + b1.y * num;
				float num10 = b2.x + b3.x * num;
				float num11 = b2.y + b3.y * num;
				float num12 = num8 * fy + num10 * invFy;
				float num13 = num9 * fy + num11 * invFy;
				float num14 = (1f - t) * fx;
				t *= fx;
				return new Vector2(num6 * num14 + num12 * t, num7 * num14 + num13 * t);
			}
			float num15 = a1.x * fy + a3.x * invFy;
			float num16 = a1.y * fy + a3.y * invFy;
			float num17 = b1.x * fy + b3.x * invFy;
			float num18 = b1.y * fy + b3.y * invFy;
			float num19 = 1f - t;
			return new Vector2(num15 * num19 + num17 * t, num16 * num19 + num18 * t);
		}

		public static Vector2 Interpolate(Vector2 a0, Vector2 a1, Vector2 a2, Vector2 a3, Vector2 b0, Vector2 b1, Vector2 b2, Vector2 b3, float fx, float invFx, float fy, float invFy, float t)
		{
			Vector2 a4 = a0 * fx + a1 * invFx;
			Vector2 a5 = a2 * fx + a3 * invFx;
			Vector2 a6 = a4 * fy + a5 * invFy;
			Vector2 a7 = b0 * fx + b1 * invFx;
			Vector2 a8 = b2 * fx + b3 * invFx;
			Vector2 a9 = a7 * fy + a8 * invFy;
			return a6 * (1f - t) + a9 * t;
		}

		public static Vector3 Interpolate(Vector3 a0, Vector3 a1, Vector3 a2, Vector3 a3, Vector3 b0, Vector3 b1, Vector3 b2, Vector3 b3, float fx, float invFx, float fy, float invFy, float t)
		{
			Vector3 a4 = a0 * fx + a1 * invFx;
			Vector3 a5 = a2 * fx + a3 * invFx;
			Vector3 a6 = a4 * fy + a5 * invFy;
			Vector3 a7 = b0 * fx + b1 * invFx;
			Vector3 a8 = b2 * fx + b3 * invFx;
			Vector3 a9 = a7 * fy + a8 * invFy;
			return a6 * (1f - t) + a9 * t;
		}

		public static Vector4 Interpolate(ref Vector4 a0, ref Vector4 a1, ref Vector4 a2, ref Vector4 a3, ref Vector4 b0, ref Vector4 b1, ref Vector4 b2, ref Vector4 b3, float fx, float invFx, float fy, float invFy, float t)
		{
			if (fx != 0f)
			{
				float num = invFx / fx;
				float num2 = a0.x + a1.x * num;
				float num3 = a0.y + a1.y * num;
				float num4 = a0.z + a1.z * num;
				float num5 = a0.w + a1.w * num;
				float num6 = a2.x + a3.x * num;
				float num7 = a2.y + a3.y * num;
				float num8 = a2.z + a3.z * num;
				float num9 = a2.w + a3.w * num;
				float num10 = num2 * fy + num6 * invFy;
				float num11 = num3 * fy + num7 * invFy;
				float num12 = num4 * fy + num8 * invFy;
				float num13 = num5 * fy + num9 * invFy;
				float num14 = b0.x + a1.x * num;
				float num15 = b0.y + a1.y * num;
				float num16 = b0.z + a1.z * num;
				float num17 = b0.w + a1.w * num;
				float num18 = b2.x + a3.x * num;
				float num19 = b2.y + a3.y * num;
				float num20 = b2.z + a3.z * num;
				float num21 = b2.w + a3.w * num;
				float num22 = num14 * fy + num18 * invFy;
				float num23 = num15 * fy + num19 * invFy;
				float num24 = num16 * fy + num20 * invFy;
				float num25 = num17 * fy + num21 * invFy;
				float num26 = (1f - t) * fx;
				t *= fx;
				return new Vector4(num10 * num26 + num22 * t, num11 * num26 + num23 * t, num12 * num26 + num24 * t, num13 * num26 + num25 * t);
			}
			float num27 = a1.x * fy + a3.x * invFy;
			float num28 = a1.y * fy + a3.y * invFy;
			float num29 = a1.z * fy + a3.z * invFy;
			float num30 = a1.w * fy + a3.w * invFy;
			float num31 = b1.x * fy + b3.x * invFy;
			float num32 = b1.y * fy + b3.y * invFy;
			float num33 = b1.z * fy + b3.z * invFy;
			float num34 = b1.w * fy + b3.w * invFy;
			float num35 = (1f - t) * fx;
			t *= fx;
			return new Vector4(num27 * num35 + num31 * t, num28 * num35 + num32 * t, num29 * num35 + num33 * t, num30 * num35 + num34 * t);
		}

		public static Vector4 Interpolate(Vector4 a0, Vector4 a1, Vector4 a2, Vector4 a3, Vector4 b0, Vector4 b1, Vector4 b2, Vector4 b3, float fx, float invFx, float fy, float invFy, float t)
		{
			if (fx != 0f)
			{
				float d = invFx / fx;
				Vector4 a4 = a0 + a1 * d;
				Vector4 a5 = a2 + a3 * d;
				Vector4 a6 = a4 * fy + a5 * invFy;
				Vector4 a7 = b0 + b1 * d;
				Vector4 a8 = b2 + b3 * d;
				Vector4 a9 = a7 * fy + a8 * invFy;
				return a6 * ((1f - t) * fx) + a9 * (t * fx);
			}
			Vector4 a10 = a1 * fy + a3 * invFy;
			Vector4 a11 = b1 * fy + b3 * invFy;
			return a10 * (1f - t) + a11 * t;
		}

		public static readonly float[] Sines = new float[2048];

		public static readonly float[] Cosines = new float[2048];

		public static readonly float[] PositiveTanhSqrt = new float[2048];

		public static readonly float[] PositiveTanhSqrtNoZero = new float[2048];

		private const float _PIx2 = 6.28318548f;
	}
}
