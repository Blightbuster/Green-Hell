using System;
using UnityEngine;

namespace LuxWater
{
	[RequireComponent(typeof(Camera))]
	public class LuxWater_UnderWaterBlur : MonoBehaviour
	{
		private void OnEnable()
		{
			this.blurMaterial = new Material(Shader.Find("Lux Water/BlurEffectConeTap"));
			this.blitMaterial = new Material(Shader.Find("Lux Water/UnderWaterPost"));
			base.Invoke("GetWaterrendermanagerInstance", 0f);
		}

		private void OnDisable()
		{
			if (this.blurMaterial)
			{
				UnityEngine.Object.DestroyImmediate(this.blurMaterial);
			}
			if (this.blitMaterial)
			{
				UnityEngine.Object.DestroyImmediate(this.blitMaterial);
			}
		}

		private void GetWaterrendermanagerInstance()
		{
			this.waterrendermanager = LuxWater_UnderWaterRendering.instance;
		}

		private void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			this.doBlur = (this.waterrendermanager.activeWaterVolume > -1);
			if (this.doBlur)
			{
				int width = src.width / this.blurDownSample;
				int height = src.height / this.blurDownSample;
				RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 0);
				this.DownSample(src, renderTexture);
				for (int i = 0; i < this.blurIterations; i++)
				{
					RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0);
					this.FourTapCone(renderTexture, temporary, i);
					RenderTexture.ReleaseTemporary(renderTexture);
					renderTexture = temporary;
				}
				RenderTexture temporary2 = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.DefaultHDR);
				Graphics.Blit(renderTexture, temporary2);
				RenderTexture.ReleaseTemporary(renderTexture);
				Shader.SetGlobalTexture("_BlurredWaterTex", temporary2);
				Graphics.Blit(src, dest, this.blitMaterial, 1);
				RenderTexture.ReleaseTemporary(temporary2);
				return;
			}
			Graphics.Blit(src, dest);
		}

		private void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
		{
			float num = 0.5f + (float)iteration * this.blurSpread;
			this.m_offsets[0].x = -num;
			this.m_offsets[0].y = -num;
			this.m_offsets[1].x = -num;
			this.m_offsets[1].y = num;
			this.m_offsets[2].x = num;
			this.m_offsets[2].y = num;
			this.m_offsets[3].x = num;
			this.m_offsets[3].y = -num;
			if (iteration == 0)
			{
				Graphics.BlitMultiTap(source, dest, this.blurMaterial, this.m_offsets);
				return;
			}
			Graphics.BlitMultiTap(source, dest, this.blurMaterial, this.m_offsets);
		}

		private void DownSample(RenderTexture source, RenderTexture dest)
		{
			float num = 1f;
			this.m_offsets[0].x = -num;
			this.m_offsets[0].y = -num;
			this.m_offsets[1].x = -num;
			this.m_offsets[1].y = num;
			this.m_offsets[2].x = num;
			this.m_offsets[2].y = num;
			this.m_offsets[3].x = num;
			this.m_offsets[3].y = -num;
			Graphics.BlitMultiTap(source, dest, this.blurMaterial, this.m_offsets);
		}

		public float blurSpread = 0.6f;

		public int blurDownSample = 4;

		public int blurIterations = 4;

		private Vector2[] m_offsets = new Vector2[4];

		private Material blurMaterial;

		private Material blitMaterial;

		private LuxWater_UnderWaterRendering waterrendermanager;

		private bool doBlur;
	}
}
