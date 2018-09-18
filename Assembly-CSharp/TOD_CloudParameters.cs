using System;
using UnityEngine;

[Serializable]
public class TOD_CloudParameters
{
	[Tooltip("Size of the clouds.")]
	[TOD_Min(1f)]
	public float Size = 2f;

	[TOD_Range(0f, 1f)]
	[Tooltip("Opacity of the clouds.")]
	public float Opacity = 1f;

	[Tooltip("How much sky is covered by clouds.")]
	[TOD_Range(0f, 1f)]
	public float Coverage = 0.3f;

	[Tooltip("Sharpness of the cloud to sky transition.")]
	[TOD_Range(0f, 1f)]
	public float Sharpness = 0.5f;

	[TOD_Range(0f, 1f)]
	[Tooltip("Amount of skylight that is blocked.")]
	public float Attenuation = 0.5f;

	[Tooltip("Amount of sunlight that is blocked.\nOnly affects the highest cloud quality setting.")]
	[TOD_Range(0f, 1f)]
	public float Saturation = 0.5f;

	[TOD_Min(0f)]
	[Tooltip("Intensity of the cloud translucency glow.\nOnly affects the highest cloud quality setting.")]
	public float Scattering = 1f;

	[TOD_Min(0f)]
	[Tooltip("Brightness of the clouds.")]
	public float Brightness = 1.5f;
}
