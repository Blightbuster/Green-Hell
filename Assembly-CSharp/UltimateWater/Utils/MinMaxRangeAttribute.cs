using System;
using UnityEngine;

namespace UltimateWater.Utils
{
	public class MinMaxRangeAttribute : PropertyAttribute
	{
		public MinMaxRangeAttribute(float minValue, float maxValue)
		{
			this.MinValue = minValue;
			this.MaxValue = maxValue;
		}

		public float MinValue;

		public float MaxValue;
	}
}
