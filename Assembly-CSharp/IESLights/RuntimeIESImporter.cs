using System;
using System.IO;
using UnityEngine;

namespace IESLights
{
	public class RuntimeIESImporter : MonoBehaviour
	{
		public static void Import(string path, out Texture2D spotlightCookie, out Cubemap pointLightCookie, int resolution = 128, bool enhancedImport = false, bool applyVignette = true)
		{
			spotlightCookie = null;
			pointLightCookie = null;
			if (!RuntimeIESImporter.IsFileValid(path))
			{
				return;
			}
			GameObject obj;
			IESConverter iesConverter;
			RuntimeIESImporter.GetIESConverterAndCubeSphere(enhancedImport, resolution, out obj, out iesConverter);
			RuntimeIESImporter.ImportIES(path, iesConverter, true, applyVignette, out spotlightCookie, out pointLightCookie);
			UnityEngine.Object.Destroy(obj);
		}

		public static Texture2D ImportSpotlightCookie(string path, int resolution = 128, bool enhancedImport = false, bool applyVignette = true)
		{
			if (!RuntimeIESImporter.IsFileValid(path))
			{
				return null;
			}
			GameObject obj;
			IESConverter iesConverter;
			RuntimeIESImporter.GetIESConverterAndCubeSphere(enhancedImport, resolution, out obj, out iesConverter);
			Texture2D result;
			Cubemap cubemap;
			RuntimeIESImporter.ImportIES(path, iesConverter, true, applyVignette, out result, out cubemap);
			UnityEngine.Object.Destroy(obj);
			return result;
		}

		public static Cubemap ImportPointLightCookie(string path, int resolution = 128, bool enhancedImport = false)
		{
			if (!RuntimeIESImporter.IsFileValid(path))
			{
				return null;
			}
			GameObject obj;
			IESConverter iesConverter;
			RuntimeIESImporter.GetIESConverterAndCubeSphere(enhancedImport, resolution, out obj, out iesConverter);
			Texture2D texture2D;
			Cubemap result;
			RuntimeIESImporter.ImportIES(path, iesConverter, false, false, out texture2D, out result);
			UnityEngine.Object.Destroy(obj);
			return result;
		}

		private static void GetIESConverterAndCubeSphere(bool logarithmicNormalization, int resolution, out GameObject cubemapSphere, out IESConverter iesConverter)
		{
			UnityEngine.Object original = Resources.Load("IES cubemap sphere");
			cubemapSphere = (GameObject)UnityEngine.Object.Instantiate(original);
			iesConverter = cubemapSphere.GetComponent<IESConverter>();
			iesConverter.NormalizationMode = (logarithmicNormalization ? NormalizationMode.Logarithmic : NormalizationMode.Linear);
			iesConverter.Resolution = resolution;
		}

		private static void ImportIES(string path, IESConverter iesConverter, bool allowSpotlightCookies, bool applyVignette, out Texture2D spotlightCookie, out Cubemap pointlightCookie)
		{
			string text = null;
			spotlightCookie = null;
			pointlightCookie = null;
			try
			{
				EXRData exrdata;
				iesConverter.ConvertIES(path, "", allowSpotlightCookies, false, applyVignette, out pointlightCookie, out spotlightCookie, out exrdata, out text);
			}
			catch (IESParseException ex)
			{
				Debug.LogError(string.Format("[IES] Encountered invalid IES data in {0}. Error message: {1}", path, ex.Message));
			}
			catch (Exception ex2)
			{
				Debug.LogError(string.Format("[IES] Error while parsing {0}. Please contact me through the forums or thomasmountainborn.com. Error message: {1}", path, ex2.Message));
			}
		}

		private static bool IsFileValid(string path)
		{
			if (!File.Exists(path))
			{
				Debug.LogWarningFormat("[IES] The file \"{0}\" does not exist.", new object[]
				{
					path
				});
				return false;
			}
			if (Path.GetExtension(path).ToLower() != ".ies")
			{
				Debug.LogWarningFormat("[IES] The file \"{0}\" is not an IES file.", new object[]
				{
					path
				});
				return false;
			}
			return true;
		}
	}
}
