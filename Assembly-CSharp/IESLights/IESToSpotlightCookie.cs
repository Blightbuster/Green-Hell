using System;
using System.Linq;
using UnityEngine;

namespace IESLights
{
	[ExecuteInEditMode]
	public class IESToSpotlightCookie : MonoBehaviour
	{
		private void OnDestroy()
		{
			if (this._spotlightMaterial != null)
			{
				UnityEngine.Object.Destroy(this._spotlightMaterial);
			}
			if (this._fadeSpotlightEdgesMaterial != null)
			{
				UnityEngine.Object.Destroy(this._fadeSpotlightEdgesMaterial);
			}
			if (this._verticalFlipMaterial != null)
			{
				UnityEngine.Object.Destroy(this._verticalFlipMaterial);
			}
		}

		public void CreateSpotlightCookie(Texture2D iesTexture, IESData iesData, int resolution, bool applyVignette, bool flipVertically, out Texture2D cookie)
		{
			if (iesData.PhotometricType != PhotometricType.TypeA)
			{
				if (this._spotlightMaterial == null)
				{
					this._spotlightMaterial = new Material(Shader.Find("Hidden/IES/IESToSpotlightCookie"));
				}
				this.CalculateAndSetSpotHeight(iesData);
				this.SetShaderKeywords(iesData, applyVignette);
				cookie = this.CreateTexture(iesTexture, resolution, flipVertically);
				return;
			}
			if (this._fadeSpotlightEdgesMaterial == null)
			{
				this._fadeSpotlightEdgesMaterial = new Material(Shader.Find("Hidden/IES/FadeSpotlightCookieEdges"));
			}
			float verticalCenter = applyVignette ? this.CalculateCookieVerticalCenter(iesData) : 0f;
			Vector2 vector = applyVignette ? this.CalculateCookieFadeEllipse(iesData) : Vector2.zero;
			cookie = this.BlitToTargetSize(iesTexture, resolution, vector.x, vector.y, verticalCenter, applyVignette, flipVertically);
		}

		private float CalculateCookieVerticalCenter(IESData iesData)
		{
			float num = 1f - (float)iesData.PadBeforeAmount / (float)iesData.NormalizedValues[0].Count;
			float num2 = (float)(iesData.NormalizedValues[0].Count - iesData.PadBeforeAmount - iesData.PadAfterAmount) / (float)iesData.NormalizedValues.Count / 2f;
			return num - num2;
		}

		private Vector2 CalculateCookieFadeEllipse(IESData iesData)
		{
			if (iesData.HorizontalAngles.Count > iesData.VerticalAngles.Count)
			{
				return new Vector2(0.5f, 0.5f * ((float)(iesData.NormalizedValues[0].Count - iesData.PadBeforeAmount - iesData.PadAfterAmount) / (float)iesData.NormalizedValues[0].Count));
			}
			if (iesData.HorizontalAngles.Count < iesData.VerticalAngles.Count)
			{
				return new Vector2(0.5f * (iesData.HorizontalAngles.Max() - iesData.HorizontalAngles.Min()) / (iesData.VerticalAngles.Max() - iesData.VerticalAngles.Min()), 0.5f);
			}
			return new Vector2(0.5f, 0.5f);
		}

		private Texture2D CreateTexture(Texture2D iesTexture, int resolution, bool flipVertically)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
			temporary.filterMode = FilterMode.Trilinear;
			temporary.DiscardContents();
			RenderTexture.active = temporary;
			Graphics.Blit(iesTexture, this._spotlightMaterial);
			if (flipVertically)
			{
				RenderTexture temporary2 = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
				Graphics.Blit(temporary, temporary2);
				this.FlipVertically(temporary2, temporary);
				RenderTexture.ReleaseTemporary(temporary2);
			}
			Texture2D texture2D = new Texture2D(resolution, resolution, TextureFormat.RGBAFloat, false, true);
			texture2D.filterMode = FilterMode.Trilinear;
			texture2D.wrapMode = TextureWrapMode.Clamp;
			texture2D.ReadPixels(new Rect(0f, 0f, (float)resolution, (float)resolution), 0, 0);
			texture2D.Apply();
			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(temporary);
			return texture2D;
		}

		private Texture2D BlitToTargetSize(Texture2D iesTexture, int resolution, float horizontalFadeDistance, float verticalFadeDistance, float verticalCenter, bool applyVignette, bool flipVertically)
		{
			if (applyVignette)
			{
				this._fadeSpotlightEdgesMaterial.SetFloat("_HorizontalFadeDistance", horizontalFadeDistance);
				this._fadeSpotlightEdgesMaterial.SetFloat("_VerticalFadeDistance", verticalFadeDistance);
				this._fadeSpotlightEdgesMaterial.SetFloat("_VerticalCenter", verticalCenter);
			}
			RenderTexture temporary = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
			temporary.filterMode = FilterMode.Trilinear;
			temporary.DiscardContents();
			if (applyVignette)
			{
				RenderTexture.active = temporary;
				Graphics.Blit(iesTexture, this._fadeSpotlightEdgesMaterial);
			}
			else if (flipVertically)
			{
				this.FlipVertically(iesTexture, temporary);
			}
			else
			{
				Graphics.Blit(iesTexture, temporary);
			}
			Texture2D texture2D = new Texture2D(resolution, resolution, TextureFormat.RGBAFloat, false, true);
			texture2D.filterMode = FilterMode.Trilinear;
			texture2D.wrapMode = TextureWrapMode.Clamp;
			texture2D.ReadPixels(new Rect(0f, 0f, (float)resolution, (float)resolution), 0, 0);
			texture2D.Apply();
			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(temporary);
			return texture2D;
		}

		private void FlipVertically(Texture iesTexture, RenderTexture renderTarget)
		{
			if (this._verticalFlipMaterial == null)
			{
				this._verticalFlipMaterial = new Material(Shader.Find("Hidden/IES/VerticalFlip"));
			}
			Graphics.Blit(iesTexture, renderTarget, this._verticalFlipMaterial);
		}

		private void CalculateAndSetSpotHeight(IESData iesData)
		{
			float value = 0.5f / Mathf.Tan(iesData.HalfSpotlightFov * 0.0174532924f);
			this._spotlightMaterial.SetFloat("_SpotHeight", value);
		}

		private void SetShaderKeywords(IESData iesData, bool applyVignette)
		{
			if (applyVignette)
			{
				this._spotlightMaterial.EnableKeyword("VIGNETTE");
			}
			else
			{
				this._spotlightMaterial.DisableKeyword("VIGNETTE");
			}
			if (iesData.VerticalType == VerticalType.Top)
			{
				this._spotlightMaterial.EnableKeyword("TOP_VERTICAL");
			}
			else
			{
				this._spotlightMaterial.DisableKeyword("TOP_VERTICAL");
			}
			if (iesData.HorizontalType == HorizontalType.None)
			{
				this._spotlightMaterial.DisableKeyword("QUAD_HORIZONTAL");
				this._spotlightMaterial.DisableKeyword("HALF_HORIZONTAL");
				this._spotlightMaterial.DisableKeyword("FULL_HORIZONTAL");
				return;
			}
			if (iesData.HorizontalType == HorizontalType.Quadrant)
			{
				this._spotlightMaterial.EnableKeyword("QUAD_HORIZONTAL");
				this._spotlightMaterial.DisableKeyword("HALF_HORIZONTAL");
				this._spotlightMaterial.DisableKeyword("FULL_HORIZONTAL");
				return;
			}
			if (iesData.HorizontalType == HorizontalType.Half)
			{
				this._spotlightMaterial.DisableKeyword("QUAD_HORIZONTAL");
				this._spotlightMaterial.EnableKeyword("HALF_HORIZONTAL");
				this._spotlightMaterial.DisableKeyword("FULL_HORIZONTAL");
				return;
			}
			if (iesData.HorizontalType == HorizontalType.Full)
			{
				this._spotlightMaterial.DisableKeyword("QUAD_HORIZONTAL");
				this._spotlightMaterial.DisableKeyword("HALF_HORIZONTAL");
				this._spotlightMaterial.EnableKeyword("FULL_HORIZONTAL");
			}
		}

		private Material _spotlightMaterial;

		private Material _fadeSpotlightEdgesMaterial;

		private Material _verticalFlipMaterial;
	}
}
