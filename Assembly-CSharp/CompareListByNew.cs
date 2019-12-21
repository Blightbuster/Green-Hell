using System;
using System.Collections.Generic;

public class CompareListByNew : IComparer<HUDSelectDialogElemData>
{
	public int Compare(HUDSelectDialogElemData d1, HUDSelectDialogElemData d2)
	{
		if (d1.m_CounterObject.activeSelf && !d2.m_CounterObject.activeSelf)
		{
			return -1;
		}
		if (!d1.m_CounterObject.activeSelf && d2.m_CounterObject.activeSelf)
		{
			return 1;
		}
		return 0;
	}
}
