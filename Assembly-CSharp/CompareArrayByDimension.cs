using System;
using System.Collections.Generic;

public class CompareArrayByDimension : IComparer<int>
{
	public int Compare(int i1, int i2)
	{
		if (i1 < i2)
		{
			return 1;
		}
		if (i1 > i2)
		{
			return -1;
		}
		return 0;
	}
}
