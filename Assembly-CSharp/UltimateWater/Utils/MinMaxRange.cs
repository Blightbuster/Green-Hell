using System;
using UnityEngine;

namespace UltimateWater.Utils
{
	[Serializable]
	public class MinMaxRange
	{
		public float Random()
		{
			return UnityEngine.Random.Range(this.MinValue, this.MaxValue);
		}

		public float MinValue;

		public float MaxValue;
	}
}
