using System;
using System.Collections.Generic;
using UnityEngine;

public class CompareListByDot : IComparer<Trigger>
{
	public int Compare(Trigger i1, Trigger i2)
	{
		float num = Vector3.Dot(TriggerController.s_CrosshairDir, (i1.transform.position - TriggerController.s_CrosshairOrigin).normalized);
		float num2 = Vector3.Dot(TriggerController.s_CrosshairDir, (i2.transform.position - TriggerController.s_CrosshairOrigin).normalized);
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
