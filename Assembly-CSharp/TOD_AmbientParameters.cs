﻿using System;
using UnityEngine;

[Serializable]
public class TOD_AmbientParameters
{
	[Tooltip("Ambient light mode.")]
	public TOD_AmbientType Mode = TOD_AmbientType.Color;

	[Tooltip("Saturation of the ambient light.")]
	[TOD_Min(0f)]
	public float Saturation = 1f;

	[TOD_Min(0f)]
	[Tooltip("Refresh interval of the ambient light probe in seconds.")]
	public float UpdateInterval = 1f;
}
