using System;
using UnityEngine;

namespace CJTools
{
	public static class SpringTools
	{
		public static float Update(float current, float target, float omega)
		{
			return target - (target - current) * Mathf.Exp(-omega * Time.deltaTime);
		}

		public static float UpdateAngle(float current, float target, float omega)
		{
			return target - Mathf.DeltaAngle(current, target) * Mathf.Exp(-omega * Time.deltaTime);
		}

		public static Vector3 Update(Vector3 current, Vector3 target, float omega)
		{
			return Vector3.Lerp(target, current, Mathf.Exp(-omega * Time.deltaTime));
		}

		public static Quaternion Update(Quaternion current, Quaternion target, float omega)
		{
			if (current == target)
			{
				return target;
			}
			return Quaternion.Lerp(target, current, Mathf.Exp(-omega * Time.deltaTime));
		}
	}
}
