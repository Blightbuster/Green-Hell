using System;
using System.Collections.Generic;
using UnityEngine;

public class BIWoundSlotComparer : IComparer<BIWoundSlot>
{
	public int Compare(BIWoundSlot c1, BIWoundSlot c2)
	{
		Camera main = Camera.main;
		Vector3 vector = main.WorldToScreenPoint(c1.m_Wound.transform.position);
		Vector3 vector2 = main.WorldToScreenPoint(c2.m_Wound.transform.position);
		if (vector.y > vector2.y)
		{
			return 1;
		}
		if (vector.y < vector2.y)
		{
			return -1;
		}
		return 0;
	}
}
