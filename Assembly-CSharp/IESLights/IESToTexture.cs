using System;
using UnityEngine;

namespace IESLights
{
	public class IESToTexture : MonoBehaviour
	{
		public static Texture2D ConvertIesData(IESData data)
		{
			Texture2D texture2D = new Texture2D(data.NormalizedValues.Count, data.NormalizedValues[0].Count, TextureFormat.RGBAFloat, false, true)
			{
				filterMode = FilterMode.Trilinear,
				wrapMode = TextureWrapMode.Clamp
			};
			Color[] array = new Color[texture2D.width * texture2D.height];
			for (int i = 0; i < texture2D.width; i++)
			{
				for (int j = 0; j < texture2D.height; j++)
				{
					float num = data.NormalizedValues[i][j];
					array[i + j * texture2D.width] = new Color(num, num, num, num);
				}
			}
			texture2D.SetPixels(array);
			texture2D.Apply();
			return texture2D;
		}
	}
}
