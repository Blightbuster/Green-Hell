using System;
using UnityEngine;

namespace CJTools
{
	public class AABB
	{
		private bool Line_AABB_1d(float start, float dir, float min, float max, ref float enter, ref float exit)
		{
			if ((double)Mathf.Abs(dir) < 1E-08)
			{
				return start >= min && start <= max;
			}
			float num = 1f / dir;
			float num2 = (min - start) * num;
			float num3 = (max - start) * num;
			if (num2 > num3)
			{
				float num4 = num2;
				num2 = num3;
				num3 = num4;
			}
			if (num2 > exit || num3 < enter)
			{
				return false;
			}
			if (num2 > enter)
			{
				enter = num2;
			}
			if (num3 < exit)
			{
				exit = num3;
			}
			return true;
		}

		public bool IntersectsLine(Vector3 s, Vector3 e, ref Vector3 hit_point)
		{
			float d = 0f;
			float num = 1f;
			Vector3 vector = e - s;
			Vector3 vector2 = -this.half_sizes + this.start;
			Vector3 vector3 = this.half_sizes + this.start;
			if (!this.Line_AABB_1d(s.x, vector.x, vector2.x, vector3.x, ref d, ref num))
			{
				return false;
			}
			if (!this.Line_AABB_1d(s.y, vector.y, vector2.y, vector3.y, ref d, ref num))
			{
				return false;
			}
			if (!this.Line_AABB_1d(s.z, vector.z, vector2.z, vector3.z, ref d, ref num))
			{
				return false;
			}
			hit_point = s + vector * d;
			return true;
		}

		public bool IsPointInside(Vector3 point)
		{
			return point.x > -this.half_sizes.x && point.x < this.half_sizes.x && point.y > -this.half_sizes.y && point.y < this.half_sizes.y && point.z > -this.half_sizes.z && point.z < this.half_sizes.z;
		}

		public Vector3 start;

		public Vector3 half_sizes;
	}
}
