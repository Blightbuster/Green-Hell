﻿using System;
using UnityEngine;

[Serializable]
public class TOD_WorldParameters
{
	[Tooltip("Latitude of the current location in degrees.")]
	[Range(-90f, 90f)]
	public float Latitude;

	[Range(-180f, 180f)]
	[Tooltip("Longitude of the current location in degrees.")]
	public float Longitude;

	[Range(-14f, 14f)]
	[Tooltip("UTC/GMT time zone of the current location in hours.")]
	public float UTC;
}
