using System;
using System.Collections.Generic;

namespace UltimateWater.Internal
{
	public class UInt64EqualityComparer : IEqualityComparer<ulong>
	{
		public static UInt64EqualityComparer Default
		{
			get
			{
				return (UInt64EqualityComparer._DefaultInstance == null) ? (UInt64EqualityComparer._DefaultInstance = new UInt64EqualityComparer()) : UInt64EqualityComparer._DefaultInstance;
			}
		}

		public bool Equals(ulong x, ulong y)
		{
			return x == y;
		}

		public int GetHashCode(ulong obj)
		{
			return (int)(obj ^ obj >> 32);
		}

		private static UInt64EqualityComparer _DefaultInstance;
	}
}
