using System;
using UnityEngine;

[Serializable]
public class TOD_StarParameters
{
	[TOD_Min(0f)]
	[Tooltip("Size of the stars.")]
	public float Size = 1f;

	[TOD_Min(0f)]
	[Tooltip("Brightness of the stars.")]
	public float Brightness = 1f;

	[Tooltip("Type of the stars position calculation.")]
	public TOD_StarsPositionType Position = TOD_StarsPositionType.Rotating;
}
