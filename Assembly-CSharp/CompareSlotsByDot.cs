using System;
using System.Collections.Generic;

public class CompareSlotsByDot : IComparer<ConstructionSlotData>
{
	public int Compare(ConstructionSlotData i1, ConstructionSlotData i2)
	{
		float num = i1.dot * 10f + 1f / i1.dist;
		float num2 = i2.dot * 10f + 1f / i2.dist;
		if (num < num2)
		{
			return 1;
		}
		if (num > num2)
		{
			return -1;
		}
		return 0;
	}
}
