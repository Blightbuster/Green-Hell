﻿using System;
using UnityEngine;

[Serializable]
public class TOD_SunParameters
{
	[TOD_Min(0f)]
	[Tooltip("Diameter of the sun in degrees.\nThe diameter as seen from earth is 0.5 degrees.")]
	public float MeshSize = 1f;

	[Tooltip("Brightness of the sun.")]
	[TOD_Min(0f)]
	public float MeshBrightness = 2f;

	[Tooltip("Contrast of the sun.")]
	[TOD_Min(0f)]
	public float MeshContrast = 1f;
}
