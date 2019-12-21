using System;
using UnityEngine;

public class MipTexMap : MonoBehaviour
{
	private static void BuildMipFilterTex(int size)
	{
		size = Mathf.ClosestPowerOfTwo(size);
		Texture2D texture2D = new Texture2D(size, size, TextureFormat.Alpha8, true);
		texture2D.anisoLevel = 3;
		texture2D.filterMode = FilterMode.Trilinear;
		texture2D.mipMapBias = 0f;
		for (int i = 0; i < texture2D.mipmapCount; i++)
		{
			int num = size * size;
			Color[] array = new Color[num];
			float num2 = 1f * (float)i / (float)(texture2D.mipmapCount - 1);
			Color color = new Color(num2, num2, num2, num2);
			for (int j = 0; j < num; j++)
			{
				array[j] = color;
			}
			texture2D.SetPixels(array, i);
		}
		texture2D.Apply(false, true);
		if (size <= 256)
		{
			if (size == 64)
			{
				MipTexMap.mipFilterTex64 = texture2D;
				return;
			}
			if (size == 128)
			{
				MipTexMap.mipFilterTex128 = texture2D;
				return;
			}
			if (size == 256)
			{
				MipTexMap.mipFilterTex256 = texture2D;
				return;
			}
		}
		else
		{
			if (size == 512)
			{
				MipTexMap.mipFilterTex512 = texture2D;
				return;
			}
			if (size == 1024)
			{
				MipTexMap.mipFilterTex1024 = texture2D;
				return;
			}
			if (size == 2048)
			{
				MipTexMap.mipFilterTex2048 = texture2D;
				return;
			}
		}
		MipTexMap.mipFilterTex512 = texture2D;
	}

	public static Texture2D GetTex(int size)
	{
		size = Mathf.ClosestPowerOfTwo(size);
		if (size <= 256)
		{
			if (size != 64)
			{
				if (size != 128)
				{
					if (size == 256)
					{
						if (MipTexMap.mipFilterTex256)
						{
							return MipTexMap.mipFilterTex256;
						}
						MipTexMap.BuildMipFilterTex(size);
						return MipTexMap.mipFilterTex256;
					}
				}
				else
				{
					if (MipTexMap.mipFilterTex128)
					{
						return MipTexMap.mipFilterTex128;
					}
					MipTexMap.BuildMipFilterTex(size);
					return MipTexMap.mipFilterTex128;
				}
			}
			else
			{
				if (MipTexMap.mipFilterTex64)
				{
					return MipTexMap.mipFilterTex64;
				}
				MipTexMap.BuildMipFilterTex(size);
				return MipTexMap.mipFilterTex64;
			}
		}
		else if (size != 512)
		{
			if (size != 1024)
			{
				if (size == 2048)
				{
					if (MipTexMap.mipFilterTex2048)
					{
						return MipTexMap.mipFilterTex2048;
					}
					MipTexMap.BuildMipFilterTex(size);
					return MipTexMap.mipFilterTex2048;
				}
			}
			else
			{
				if (MipTexMap.mipFilterTex1024)
				{
					return MipTexMap.mipFilterTex1024;
				}
				MipTexMap.BuildMipFilterTex(size);
				return MipTexMap.mipFilterTex1024;
			}
		}
		else
		{
			if (MipTexMap.mipFilterTex512)
			{
				return MipTexMap.mipFilterTex512;
			}
			MipTexMap.BuildMipFilterTex(size);
			return MipTexMap.mipFilterTex512;
		}
		if (MipTexMap.mipFilterTex512)
		{
			return MipTexMap.mipFilterTex512;
		}
		MipTexMap.BuildMipFilterTex(size);
		return MipTexMap.mipFilterTex512;
	}

	private static Texture2D mipFilterTex64;

	private static Texture2D mipFilterTex128;

	private static Texture2D mipFilterTex256;

	private static Texture2D mipFilterTex512;

	private static Texture2D mipFilterTex1024;

	private static Texture2D mipFilterTex2048;
}
