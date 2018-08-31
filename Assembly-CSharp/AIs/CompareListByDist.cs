using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class CompareListByDist : IComparer<RaycastHit>
	{
		public int Compare(RaycastHit i1, RaycastHit i2)
		{
			if (i1.point.y > i2.point.y)
			{
				return -1;
			}
			if (i1.point.y < i2.point.y)
			{
				return 1;
			}
			return 0;
		}
	}
}
