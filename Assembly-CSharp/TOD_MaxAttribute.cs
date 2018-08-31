using System;
using UnityEngine;

public class TOD_MaxAttribute : PropertyAttribute
{
	public TOD_MaxAttribute(float max)
	{
		this.max = max;
	}

	public float max;
}
