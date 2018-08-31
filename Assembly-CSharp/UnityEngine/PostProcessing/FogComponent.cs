using System;
using UnityEngine.Rendering;

namespace UnityEngine.PostProcessing
{
	public sealed class FogComponent : PostProcessingComponentCommandBuffer<FogModel>
	{
		public override bool active
		{
			get
			{
				return base.model.enabled && this.context.isGBufferAvailable && RenderSettings.fog && !this.context.interrupted;
			}
		}

		public override string GetName()
		{
			return "Fog";
		}

		public override DepthTextureMode GetCameraFlags()
		{
			return DepthTextureMode.Depth;
		}

		public override CameraEvent GetCameraEvent()
		{
			return CameraEvent.AfterImageEffectsOpaque;
		}

		public override void PopulateCommandBuffer(CommandBuffer cb)
		{
			FogModel.Settings settings = base.model.settings;
			Material material = this.context.materialFactory.Get("Hidden/Post FX/Fog");
			material.shaderKeywords = null;
			Color value = (!GraphicsUtils.isLinearColorSpace) ? RenderSettings.fogColor : RenderSettings.fogColor.linear;
			material.SetColor(FogComponent.Uniforms._FogColor, value);
			material.SetFloat(FogComponent.Uniforms._Density, RenderSettings.fogDensity);
			material.SetFloat(FogComponent.Uniforms._Start, RenderSettings.fogStartDistance);
			material.SetFloat(FogComponent.Uniforms._End, RenderSettings.fogEndDistance);
			FogMode fogMode = RenderSettings.fogMode;
			if (fogMode != FogMode.Linear)
			{
				if (fogMode != FogMode.Exponential)
				{
					if (fogMode == FogMode.ExponentialSquared)
					{
						material.EnableKeyword("FOG_EXP2");
					}
				}
				else
				{
					material.EnableKeyword("FOG_EXP");
				}
			}
			else
			{
				material.EnableKeyword("FOG_LINEAR");
			}
			RenderTextureFormat format = (!this.context.isHdr) ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR;
			cb.GetTemporaryRT(FogComponent.Uniforms._TempRT, this.context.width, this.context.height, 24, FilterMode.Bilinear, format);
			cb.Blit(BuiltinRenderTextureType.CameraTarget, FogComponent.Uniforms._TempRT);
			cb.Blit(FogComponent.Uniforms._TempRT, BuiltinRenderTextureType.CameraTarget, material, (!settings.excludeSkybox) ? 0 : 1);
			cb.ReleaseTemporaryRT(FogComponent.Uniforms._TempRT);
		}

		private const string k_ShaderString = "Hidden/Post FX/Fog";

		private static class Uniforms
		{
			internal static readonly int _FogColor = Shader.PropertyToID("_FogColor");

			internal static readonly int _Density = Shader.PropertyToID("_Density");

			internal static readonly int _Start = Shader.PropertyToID("_Start");

			internal static readonly int _End = Shader.PropertyToID("_End");

			internal static readonly int _TempRT = Shader.PropertyToID("_TempRT");
		}
	}
}
