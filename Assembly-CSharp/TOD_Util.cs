using System;
using UnityEngine;

public static class TOD_Util
{
	public static Color MulRGB(Color color, float multiplier)
	{
		if (multiplier == 1f)
		{
			return color;
		}
		return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier, color.a);
	}

	public static Color MulRGBA(Color color, float multiplier)
	{
		if (multiplier == 1f)
		{
			return color;
		}
		return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier, color.a * multiplier);
	}

	public static Color PowRGB(Color color, float power)
	{
		if (power == 1f)
		{
			return color;
		}
		return new Color(Mathf.Pow(color.r, power), Mathf.Pow(color.g, power), Mathf.Pow(color.b, power), color.a);
	}

	public static Color PowRGBA(Color color, float power)
	{
		if (power == 1f)
		{
			return color;
		}
		return new Color(Mathf.Pow(color.r, power), Mathf.Pow(color.g, power), Mathf.Pow(color.b, power), Mathf.Pow(color.a, power));
	}

	public static Color ApplyAlpha(Color color)
	{
		return new Color(color.r * color.a, color.g * color.a, color.b * color.a, 1f);
	}

	public static Color ChangeSaturation(Color color, float change)
	{
		float grayscale = color.grayscale;
		color.r = grayscale + (color.r - grayscale) * change;
		color.g = grayscale + (color.g - grayscale) * change;
		color.b = grayscale + (color.b - grayscale) * change;
		return color;
	}
}
