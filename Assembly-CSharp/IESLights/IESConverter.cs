using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace IESLights
{
	[RequireComponent(typeof(IESToCubemap))]
	[RequireComponent(typeof(IESToSpotlightCookie))]
	public class IESConverter : MonoBehaviour
	{
		public void ConvertIES(string filePath, string targetPath, bool createSpotlightCookies, bool rawImport, bool applyVignette, out Cubemap pointLightCookie, out Texture2D spotlightCookie, out EXRData exrData, out string targetFilename)
		{
			IESData iesdata = ParseIES.Parse(filePath, rawImport ? NormalizationMode.Linear : this.NormalizationMode);
			this._iesTexture = IESToTexture.ConvertIesData(iesdata);
			if (!rawImport)
			{
				exrData = default(EXRData);
				this.RegularImport(filePath, targetPath, createSpotlightCookies, applyVignette, out pointLightCookie, out spotlightCookie, out targetFilename, iesdata);
			}
			else
			{
				pointLightCookie = null;
				spotlightCookie = null;
				this.RawImport(iesdata, filePath, targetPath, createSpotlightCookies, out exrData, out targetFilename);
			}
			if (this._iesTexture != null)
			{
				UnityEngine.Object.Destroy(this._iesTexture);
			}
		}

		private void RegularImport(string filePath, string targetPath, bool createSpotlightCookies, bool applyVignette, out Cubemap pointLightCookie, out Texture2D spotlightCookie, out string targetFilename, IESData iesData)
		{
			if ((createSpotlightCookies && iesData.VerticalType != VerticalType.Full) || iesData.PhotometricType == PhotometricType.TypeA)
			{
				pointLightCookie = null;
				base.GetComponent<IESToSpotlightCookie>().CreateSpotlightCookie(this._iesTexture, iesData, this.Resolution, applyVignette, false, out spotlightCookie);
			}
			else
			{
				spotlightCookie = null;
				base.GetComponent<IESToCubemap>().CreateCubemap(this._iesTexture, iesData, this.Resolution, out pointLightCookie);
			}
			this.BuildTargetFilename(Path.GetFileNameWithoutExtension(filePath), targetPath, pointLightCookie != null, false, this.NormalizationMode, iesData, out targetFilename);
		}

		private void RawImport(IESData iesData, string filePath, string targetPath, bool createSpotlightCookie, out EXRData exrData, out string targetFilename)
		{
			if ((createSpotlightCookie && iesData.VerticalType != VerticalType.Full) || iesData.PhotometricType == PhotometricType.TypeA)
			{
				Texture2D texture2D = null;
				base.GetComponent<IESToSpotlightCookie>().CreateSpotlightCookie(this._iesTexture, iesData, this.Resolution, false, true, out texture2D);
				exrData = new EXRData(texture2D.GetPixels(), this.Resolution, this.Resolution);
				UnityEngine.Object.DestroyImmediate(texture2D);
			}
			else
			{
				exrData = new EXRData(base.GetComponent<IESToCubemap>().CreateRawCubemap(this._iesTexture, iesData, this.Resolution), this.Resolution * 6, this.Resolution);
			}
			this.BuildTargetFilename(Path.GetFileNameWithoutExtension(filePath), targetPath, false, true, NormalizationMode.Linear, iesData, out targetFilename);
		}

		private void BuildTargetFilename(string name, string folderHierarchy, bool isCubemap, bool isRaw, NormalizationMode normalizationMode, IESData iesData, out string targetFilePath)
		{
			if (!Directory.Exists(Path.Combine(Application.dataPath, string.Format("IES/Imports/{0}", folderHierarchy))))
			{
				Directory.CreateDirectory(Path.Combine(Application.dataPath, string.Format("IES/Imports/{0}", folderHierarchy)));
			}
			float num = 0f;
			if (iesData.PhotometricType == PhotometricType.TypeA)
			{
				num = iesData.HorizontalAngles.Max() - iesData.HorizontalAngles.Min();
			}
			else if (!isCubemap)
			{
				num = iesData.HalfSpotlightFov * 2f;
			}
			string text = "";
			if (normalizationMode == NormalizationMode.EqualizeHistogram)
			{
				text = "[H] ";
			}
			else if (normalizationMode == NormalizationMode.Logarithmic)
			{
				text = "[E] ";
			}
			string text2;
			if (isRaw)
			{
				text2 = "exr";
			}
			else
			{
				text2 = (isCubemap ? "cubemap" : "asset");
			}
			targetFilePath = Path.Combine(Path.Combine("Assets/IES/Imports/", folderHierarchy), string.Format("{0}{1}{2}.{3}", new object[]
			{
				text,
				(iesData.PhotometricType == PhotometricType.TypeA || !isCubemap) ? ("[FOV " + num + "] ") : "",
				name,
				text2
			}));
			if (File.Exists(targetFilePath))
			{
				File.Delete(targetFilePath);
			}
		}

		public int Resolution = 512;

		public NormalizationMode NormalizationMode;

		private Texture2D _iesTexture;
	}
}
