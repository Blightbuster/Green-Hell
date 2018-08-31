using System;
using System.Collections.Generic;

namespace UltimateWater.Internal
{
	public class Int32EqualityComparer : IEqualityComparer<int>
	{
		public static Int32EqualityComparer Default
		{
			get
			{
				return (Int32EqualityComparer._DefaultInstance == null) ? (Int32EqualityComparer._DefaultInstance = new Int32EqualityComparer()) : Int32EqualityComparer._DefaultInstance;
			}
		}

		public bool Equals(int x, int y)
		{
			return x == y;
		}

		public int GetHashCode(int obj)
		{
			return obj;
		}

		private static Int32EqualityComparer _DefaultInstance;
	}
}
