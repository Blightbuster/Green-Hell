using System;
using UnityEngine;

[Serializable]
public class TOD_CloudParameters
{
	[TOD_Min(1f)]
	[Tooltip("Size of the clouds.")]
	public float Size = 2f;

	[TOD_Range(0f, 1f)]
	[Tooltip("Opacity of the clouds.")]
	public float Opacity = 1f;

	[TOD_Range(0f, 1f)]
	[Tooltip("How much sky is covered by clouds.")]
	public float Coverage = 0.3f;

	[TOD_Range(0f, 1f)]
	[Tooltip("Sharpness of the cloud to sky transition.")]
	public float Sharpness = 0.5f;

	[Tooltip("Amount of skylight that is blocked.")]
	[TOD_Range(0f, 1f)]
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
