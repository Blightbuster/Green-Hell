using System;
using UnityEngine;

namespace CJTools
{
	public class Math
	{
		public static float GetProportional(float a1, float a2, float b, float b1, float b2)
		{
			float num = (b1 == b2) ? 0f : ((b - b1) / (b2 - b1));
			return a1 + (a2 - a1) * num;
		}

		public static float GetProportionalClamp(float a1, float a2, float b, float b1, float b2)
		{
			if (b1 <= b2)
			{
				if (b <= b1)
				{
					return a1;
				}
				if (b >= b2)
				{
					return a2;
				}
			}
			else
			{
				if (b >= b1)
				{
					return a1;
				}
				if (b <= b2)
				{
					return a2;
				}
			}
			return Math.GetProportional(a1, a2, b, b1, b2);
		}

		public static Vector3 ProjectPointOnLine(Vector3 line_point, Vector3 line_dir, Vector3 point)
		{
			line_dir.Normalize();
			Vector3 lhs = point - line_point;
			float d = Vector3.Dot(lhs, line_dir);
			return line_point + line_dir * d;
		}

		public static Vector3 ProjectPointOnSegment(Vector3 seg_start, Vector3 seg_end, Vector3 point)
		{
			float num = 0f;
			return Math.ProjectPointOnSegment(seg_start, seg_end, point, out num);
		}

		public static Vector3 ProjectPointOnSegment(Vector3 seg_start, Vector3 seg_end, Vector3 point, out float progress)
		{
			Vector3 normalized = (seg_end - seg_start).normalized;
			Vector3 lhs = point - seg_start;
			float num = Vector3.Dot(lhs, normalized);
			progress = num / Vector3.Distance(seg_start, seg_end);
			return seg_start + normalized * num;
		}

		public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
		{
			Quaternion result = default(Quaternion);
			result.w = Mathf.Sqrt(Mathf.Max(0f, 1f + m[0, 0] + m[1, 1] + m[2, 2])) / 2f;
			result.x = Mathf.Sqrt(Mathf.Max(0f, 1f + m[0, 0] - m[1, 1] - m[2, 2])) / 2f;
			result.y = Mathf.Sqrt(Mathf.Max(0f, 1f - m[0, 0] + m[1, 1] - m[2, 2])) / 2f;
			result.z = Mathf.Sqrt(Mathf.Max(0f, 1f - m[0, 0] - m[1, 1] + m[2, 2])) / 2f;
			result.x *= Mathf.Sign(result.x * (m[2, 1] - m[1, 2]));
			result.y *= Mathf.Sign(result.y * (m[0, 2] - m[2, 0]));
			result.z *= Mathf.Sign(result.z * (m[1, 0] - m[0, 1]));
			return result;
		}
	}
}
