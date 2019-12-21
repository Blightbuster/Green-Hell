﻿using System;
using UnityEngine;

[Serializable]
public class TOD_DayParameters
{
	[Tooltip("Color of the sun spot.\nLeft value: Sun at zenith.\nRight value: Sun at horizon.")]
	public Gradient SunColor = new Gradient
	{
		alphaKeys = new GradientAlphaKey[]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 0.5f),
			new GradientAlphaKey(1f, 1f)
		},
		colorKeys = new GradientColorKey[]
		{
			new GradientColorKey(new Color32(253, 171, 50, byte.MaxValue), 0f),
			new GradientColorKey(new Color32(253, 171, 50, byte.MaxValue), 0.5f),
			new GradientColorKey(new Color32(253, 171, 50, byte.MaxValue), 1f)
		}
	};

	[Tooltip("Color of the light that hits the ground.\nLeft value: Sun at zenith.\nRight value: Sun at horizon.")]
	public Gradient LightColor = new Gradient
	{
		alphaKeys = new GradientAlphaKey[]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 0.5f),
			new GradientAlphaKey(1f, 1f)
		},
		colorKeys = new GradientColorKey[]
		{
			new GradientColorKey(new Color32(byte.MaxValue, 243, 234, byte.MaxValue), 0f),
			new GradientColorKey(new Color32(byte.MaxValue, 243, 234, byte.MaxValue), 0.5f),
			new GradientColorKey(new Color32(byte.MaxValue, 154, 0, byte.MaxValue), 1f)
		}
	};

	[Tooltip("Color of the god rays.\nLeft value: Sun at zenith.\nRight value: Sun at horizon.")]
	public Gradient RayColor = new Gradient
	{
		alphaKeys = new GradientAlphaKey[]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 0.5f),
			new GradientAlphaKey(1f, 1f)
		},
		colorKeys = new GradientColorKey[]
		{
			new GradientColorKey(new Color32(byte.MaxValue, 243, 234, byte.MaxValue), 0f),
			new GradientColorKey(new Color32(byte.MaxValue, 243, 234, byte.MaxValue), 0.5f),
			new GradientColorKey(new Color32(byte.MaxValue, 154, 0, byte.MaxValue), 1f)
		}
	};

	[Tooltip("Color of the light that hits the atmosphere.\nLeft value: Sun at zenith.\nRight value: Sun at horizon.")]
	public Gradient SkyColor = new Gradient
	{
		alphaKeys = new GradientAlphaKey[]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 0.5f),
			new GradientAlphaKey(1f, 1f)
		},
		colorKeys = new GradientColorKey[]
		{
			new GradientColorKey(new Color32(byte.MaxValue, 243, 234, byte.MaxValue), 0f),
			new GradientColorKey(new Color32(byte.MaxValue, 243, 234, byte.MaxValue), 0.5f),
			new GradientColorKey(new Color32(byte.MaxValue, 243, 234, byte.MaxValue), 1f)
		}
	};

	[Tooltip("Color of the clouds.\nLeft value: Sun at zenith.\nRight value: Sun at horizon.")]
	public Gradient CloudColor = new Gradient
	{
		alphaKeys = new GradientAlphaKey[]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 0.5f),
			new GradientAlphaKey(1f, 1f)
		},
		colorKeys = new GradientColorKey[]
		{
			new GradientColorKey(new Color32(224, 235, byte.MaxValue, byte.MaxValue), 0f),
			new GradientColorKey(new Color32(224, 235, byte.MaxValue, byte.MaxValue), 0.5f),
			new GradientColorKey(new Color32(byte.MaxValue, 195, 145, byte.MaxValue), 1f)
		}
	};

	[Tooltip("Color of the atmosphere fog.\nLeft value: Sun at horizon.\nRight value: Sun opposite to zenith.")]
	public Gradient FogColor = new Gradient
	{
		alphaKeys = new GradientAlphaKey[]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 0.5f),
			new GradientAlphaKey(1f, 1f)
		},
		colorKeys = new GradientColorKey[]
		{
			new GradientColorKey(new Color32(191, 191, 191, byte.MaxValue), 0f),
			new GradientColorKey(new Color32(191, 191, 191, byte.MaxValue), 0.5f),
			new GradientColorKey(new Color32(127, 127, 127, byte.MaxValue), 1f)
		}
	};

	[Tooltip("Color of the ambient light.\nLeft value: Sun at zenith.\nRight value: Sun at horizon.")]
	public Gradient AmbientColor = new Gradient
	{
		alphaKeys = new GradientAlphaKey[]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 0.5f),
			new GradientAlphaKey(1f, 1f)
		},
		colorKeys = new GradientColorKey[]
		{
			new GradientColorKey(new Color32(94, 89, 87, byte.MaxValue), 0f),
			new GradientColorKey(new Color32(94, 89, 87, byte.MaxValue), 0.5f),
			new GradientColorKey(new Color32(94, 89, 87, byte.MaxValue), 1f)
		}
	};

	[Tooltip("Intensity of the light source.")]
	[Range(0f, 8f)]
	public float LightIntensity = 1f;

	public float m_SanityLightIntensityMul = 1f;

	[Tooltip("Opacity of the shadows dropped by the light source.")]
	[Range(0f, 1f)]
	public float ShadowStrength = 1f;

	[Tooltip("Brightness multiplier of the ambient light.")]
	[Range(0f, 1f)]
	public float AmbientMultiplier = 1f;

	[Tooltip("Brightness multiplier of the reflection probe.")]
	[Range(0f, 1f)]
	public float ReflectionMultiplier = 1f;
}
