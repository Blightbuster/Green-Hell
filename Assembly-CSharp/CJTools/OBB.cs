using System;
using UnityEngine;

namespace CJTools
{
	public class OBB
	{
		public bool IntersectsLine(Vector3 s, Vector3 e)
		{
			Vector3 zero = Vector3.zero;
			return this.IntersectsLine(s, e, ref zero);
		}

		public bool IntersectsLine(Vector3 s, Vector3 e, ref Vector3 hit_point)
		{
			AABB aabb = new AABB();
			aabb.start = this.start;
			aabb.half_sizes = this.half_sizes;
			Vector3 s2 = this.transform.InverseTransformPoint(s);
			Vector3 e2 = this.transform.InverseTransformPoint(e);
			bool result = aabb.IntersectsLine(s2, e2, ref hit_point);
			hit_point = this.transform.TransformPoint(hit_point);
			return result;
		}

		public bool IsPointInside(Vector3 point)
		{
			AABB aabb = new AABB();
			aabb.half_sizes = this.half_sizes;
			Vector3 point2 = this.transform.InverseTransformPoint(point);
			return aabb.IsPointInside(point2);
		}

		public Vector3 start;

		public Vector3 half_sizes;

		public Transform transform;
	}
}
