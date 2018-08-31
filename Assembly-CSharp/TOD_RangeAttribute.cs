using System;
using UnityEngine;

public class TOD_RangeAttribute : PropertyAttribute
{
	public TOD_RangeAttribute(float min, float max)
	{
		this.min = min;
		this.max = max;
	}

	public float min;

	public float max;
}
