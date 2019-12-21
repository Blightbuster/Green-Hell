using System;
using System.Collections.Generic;

public class CompareListByChance : IComparer<BSItemData>
{
	public int Compare(BSItemData i1, BSItemData i2)
	{
		if (i1.m_Chance < i2.m_Chance)
		{
			return 1;
		}
		if (i1.m_Chance > i2.m_Chance)
		{
			return -1;
		}
		return 0;
	}
}
