﻿using System;
using UnityEngine;

[Serializable]
public class TOD_NightParameters
{
	[Tooltip("Color of the moon mesh.\nLeft value: Sun at horizon.\nRight value: Sun opposite to zenith.")]
	public Gradient MoonColor = new Gradient
	{
		alphaKeys = new GradientAlphaKey[]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 0.5f),
			new GradientAlphaKey(1f, 1f)
		},
		colorKeys = new GradientColorKey[]
		{
			new GradientColorKey(new Color32(byte.MaxValue, 233, 200, byte.MaxValue), 0f),
			new GradientColorKey(new Color32(byte.MaxValue, 233, 200, byte.MaxValue), 0.5f),
			new GradientColorKey(new Color32(byte.MaxValue, 233, 200, byte.MaxValue), 1f)
		}
	};

	[Tooltip("Color of the light that hits the ground.\nLeft value: Sun at horizon.\nRight value: Sun opposite to zenith.")]
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
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 0f),
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 0.5f),
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 1f)
		}
	};

	[Tooltip("Color of the god rays.\nLeft value: Sun at horizon.\nRight value: Sun opposite to zenith.")]
	public Gradient RayColor = new Gradient
	{
		alphaKeys = new GradientAlphaKey[]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(0.2f, 0.5f),
			new GradientAlphaKey(0.2f, 1f)
		},
		colorKeys = new GradientColorKey[]
		{
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 0f),
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 0.5f),
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 1f)
		}
	};

	[Tooltip("Color of the light that hits the atmosphere.\nLeft value: Sun at horizon.\nRight value: Sun opposite to zenith.")]
	public Gradient SkyColor = new Gradient
	{
		alphaKeys = new GradientAlphaKey[]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(0.2f, 0.5f),
			new GradientAlphaKey(0.2f, 1f)
		},
		colorKeys = new GradientColorKey[]
		{
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 0f),
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 0.5f),
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 1f)
		}
	};

	[Tooltip("Color of the clouds.\nLeft value: Sun at horizon.\nRight value: Sun opposite to zenith.")]
	public Gradient CloudColor = new Gradient
	{
		alphaKeys = new GradientAlphaKey[]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(0.1f, 0.5f),
			new GradientAlphaKey(0.1f, 1f)
		},
		colorKeys = new GradientColorKey[]
		{
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 0f),
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 0.5f),
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 1f)
		}
	};

	[Tooltip("Color of the atmosphere fog.\nLeft value: Sun at horizon.\nRight value: Sun opposite to zenith.")]
	public Gradient FogColor = new Gradient
	{
		alphaKeys = new GradientAlphaKey[]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(0.2f, 0.5f),
			new GradientAlphaKey(0.2f, 1f)
		},
		colorKeys = new GradientColorKey[]
		{
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 0f),
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 0.5f),
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 1f)
		}
	};

	[Tooltip("Color of the ambient light.\nLeft value: Sun at horizon.\nRight value: Sun opposite to zenith.")]
	public Gradient AmbientColor = new Gradient
	{
		alphaKeys = new GradientAlphaKey[]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(0.2f, 0.5f),
			new GradientAlphaKey(0.2f, 1f)
		},
		colorKeys = new GradientColorKey[]
		{
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 0f),
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 0.5f),
			new GradientColorKey(new Color32(25, 40, 65, byte.MaxValue), 1f)
		}
	};

	[Tooltip("Intensity of the light source.")]
	[Range(0f, 8f)]
	public float LightIntensity = 0.1f;

	public float m_SanityLightIntensityMul = 1f;

	[Tooltip("Opacity of the shadows dropped by the light source.")]
	[Range(0f, 1f)]
	public float ShadowStrength = 1f;

	[Tooltip("Brightness multiplier of the ambient light.")]
	[Range(0f, 1f)]
	public float AmbientMultiplier = 1f;

	[Range(0f, 1f)]
	[Tooltip("Brightness multiplier of the reflection probe.")]
	public float ReflectionMultiplier = 1f;
}
