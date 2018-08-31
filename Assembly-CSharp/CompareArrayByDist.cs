using System;
using System.Collections.Generic;
using UnityEngine;

public class CompareArrayByDist : IComparer<RaycastHit>
{
	public int Compare(RaycastHit i1, RaycastHit i2)
	{
		float num = Vector3.Distance(TriggerController.s_CrosshairOrigin, i1.point);
		float num2 = Vector3.Distance(TriggerController.s_CrosshairOrigin, i2.point);
		if (num > num2)
		{
			return 1;
		}
		if (num < num2)
		{
			return -1;
		}
		return 0;
	}
}
