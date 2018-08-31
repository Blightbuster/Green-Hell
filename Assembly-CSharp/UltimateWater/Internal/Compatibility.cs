using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UltimateWater.Utils;
using UnityEngine;

namespace UltimateWater.Internal
{
	public class Compatibility
	{
		static Compatibility()
		{
			if (PlayerPrefs.GetInt("[Ultimate Water System]: compatibility check") == Versioning.Number)
			{
				return;
			}
			Compatibility.CheckFormats();
			PlayerPrefs.SetInt("[Ultimate Water System]: compatibility check", Versioning.Number);
		}

		public static RenderTextureFormat? GetFormat(RenderTextureFormat preferred, IEnumerable<RenderTextureFormat> fallback = null)
		{
			if (Compatibility.IsFormatSupported(preferred))
			{
				return new RenderTextureFormat?(preferred);
			}
			if (fallback == null)
			{
				WaterLogger.Error("Compatibility", "GetFormat", "preferred format not supported, and no fallback formats available for :" + preferred);
				return null;
			}
			foreach (RenderTextureFormat renderTextureFormat in fallback)
			{
				if (SystemInfo.SupportsRenderTextureFormat(renderTextureFormat))
				{
					WaterLogger.Warning("Compatibility", "GetFormat", "preferred format not supported, chosen fallback: " + renderTextureFormat);
					return new RenderTextureFormat?(renderTextureFormat);
				}
			}
			return null;
		}

		private static void CheckFormats()
		{
			RenderTextureFormat[] array = new RenderTextureFormat[8];
			RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.$field-4B90332EC190C288D28502C7CDE8E9207B8EBE8A).FieldHandle);
			RenderTextureFormat[] array2 = array;
			bool flag = true;
			foreach (RenderTextureFormat renderTextureFormat in array2)
			{
				if (!Compatibility.IsFormatSupported(renderTextureFormat))
				{
					WaterLogger.Info("Compatibility", "CheckFormats", "RenderTexture format not supported: " + renderTextureFormat);
					flag = false;
				}
			}
			if (flag)
			{
				WaterLogger.Info("Compatibility", "CheckFormats", "all necessary RenderTexture formats supported");
			}
			else
			{
				WaterLogger.Warning("Compatibility", "CheckFormats", "some of the necessary render texture formats not supported, \nsome features will not be available");
			}
		}

		private static bool IsFormatSupported(RenderTextureFormat format)
		{
			return SystemInfo.SupportsRenderTextureFormat(format);
		}
	}
}
