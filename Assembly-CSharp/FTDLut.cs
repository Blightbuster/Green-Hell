using System;
using UnityEngine;

public class FTDLut
{
	public bool Validate2DLut(Texture2D texture)
	{
		return texture && texture.height == Mathf.FloorToInt(Mathf.Sqrt((float)texture.width));
	}

	public Texture3D Allocate3DLut(Texture2D source)
	{
		int height = source.height;
		return new Texture3D(height, height, height, TextureFormat.ARGB32, false);
	}

	public void SetNeutralLUT(Texture2D destination)
	{
		int height = destination.height;
		Color[] array = new Color[height * height * height];
		float num = 1f / (1f * (float)height - 1f);
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < height; j++)
			{
				for (int k = 0; k < height; k++)
				{
					int num2 = height - j - 1;
					array[k * height + i + num2 * height * height] = new Color((float)i * 1f * num, (float)j * 1f * num, (float)k * 1f * num, 1f);
				}
			}
		}
		destination.SetPixels(array);
		destination.Apply();
	}

	public bool ConvertLUT(Texture2D source, Texture3D destination)
	{
		int height = source.height;
		if (!this.Validate2DLut(source))
		{
			Debug.LogWarning("The given 2D texture " + source.name + " cannot be used as a 3D LUT.");
			return false;
		}
		Color[] pixels = source.GetPixels();
		Color[] array = new Color[pixels.Length];
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < height; j++)
			{
				for (int k = 0; k < height; k++)
				{
					int num = height - j - 1;
					array[i + j * height + k * height * height] = pixels[k * height + i + num * height * height];
				}
			}
		}
		destination.SetPixels(array);
		destination.Apply();
		return true;
	}
}
