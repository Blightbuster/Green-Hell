using System;
using UnityEngine;

[Serializable]
public class TOD_SunParameters
{
	[Tooltip("Diameter of the sun in degrees.\nThe diameter as seen from earth is 0.5 degrees.")]
	[TOD_Min(0f)]
	public float MeshSize = 1f;

	[TOD_Min(0f)]
	[Tooltip("Brightness of the sun.")]
	public float MeshBrightness = 2f;

	[TOD_Min(0f)]
	[Tooltip("Contrast of the sun.")]
	public float MeshContrast = 1f;
}
