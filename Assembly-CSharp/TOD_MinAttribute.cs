using System;
using UnityEngine;

public class TOD_MinAttribute : PropertyAttribute
{
	public TOD_MinAttribute(float min)
	{
		this.min = min;
	}

	public float min;
}
