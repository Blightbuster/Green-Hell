using System;
using System.Collections.Generic;

namespace UltimateWater.Internal
{
	public class FloatEqualityComparer : IEqualityComparer<float>
	{
		public bool Equals(float x, float y)
		{
			return x == y;
		}

		public int GetHashCode(float obj)
		{
			return (int)BitConverter.DoubleToInt64Bits((double)obj);
		}
	}
}
