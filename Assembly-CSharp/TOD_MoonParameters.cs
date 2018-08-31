using System;
using UnityEngine;

[Serializable]
public class TOD_MoonParameters
{
	[TOD_Min(0f)]
	[Tooltip("Diameter of the moon in degrees.\nThe diameter as seen from earth is 0.5 degrees.")]
	public float MeshSize = 1f;

	[TOD_Min(0f)]
	[Tooltip("Brightness of the moon.")]
	public float MeshBrightness = 2f;

	[TOD_Min(0f)]
	[Tooltip("Contrast of the moon.")]
	public float MeshContrast = 1f;

	[TOD_Min(0f)]
	[Tooltip("Size of the moon halo.")]
	public float HaloSize = 0.1f;

	[TOD_Min(0f)]
	[Tooltip("Brightness of the moon halo.")]
	public float HaloBrightness = 1f;

	[Tooltip("Type of the moon position calculation.")]
	public TOD_MoonPositionType Position = TOD_MoonPositionType.Realistic;
}
