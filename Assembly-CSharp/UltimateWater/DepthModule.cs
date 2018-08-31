using System;
using System.Runtime.CompilerServices;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateWater
{
	public class DepthModule : IRenderModule
	{
		public void OnEnable(WaterCamera waterCamera)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			RenderTextureFormat preferred = RenderTextureFormat.Depth;
			RenderTextureFormat[] array = new RenderTextureFormat[3];
			RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.$field-2BEF801ABDD507D9EA15BD732E2FD447518DF1B3).FieldHandle);
			RenderTextureFormat? format = Compatibility.GetFormat(preferred, array);
			RenderTextureFormat? format2 = Compatibility.GetFormat(RenderTextureFormat.RFloat, new RenderTextureFormat[]
			{
				RenderTextureFormat.RHalf
			});
			if (format == null || format2 == null)
			{
				return;
			}
			this._Commands = new CommandBuffer
			{
				name = "[UWS] DepthModule - Render Depth"
			};
			this._CameraDepthTextureId = ShaderVariables.CameraDepthTexture2;
			this._WaterDepthTextureId = ShaderVariables.WaterDepthTexture;
			this._WaterlessDepthId = ShaderVariables.WaterlessDepthTexture;
			this._DepthFormat = format.Value;
			this._BlendedDepthFormat = format.Value;
			if (SystemInfo.graphicsShaderLevel < 50)
			{
				this._BlendedDepthFormat = format2.Value;
				if (this._BlendedDepthFormat == RenderTextureFormat.RFloat && waterCamera.BaseEffectsQuality < 0.2f)
				{
					RenderTextureFormat? format3 = Compatibility.GetFormat(RenderTextureFormat.RHalf, null);
					if (format3 == null)
					{
						return;
					}
					this._BlendedDepthFormat = format3.Value;
				}
			}
		}

		public void OnDisable(WaterCamera waterCamera)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			this.Unbind(waterCamera);
			if (this._DepthBlitCache != null)
			{
				this._DepthBlitCache.Destroy();
				this._DepthBlitCache = null;
			}
			if (this._Commands != null)
			{
				this._Commands.Release();
				this._Commands = null;
			}
		}

		public void OnValidate(WaterCamera waterCamera)
		{
			ShaderUtility.Instance.Use(ShaderList.WaterDepth);
		}

		public void Process(WaterCamera waterCamera)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (!waterCamera.RenderWaterDepth && waterCamera.RenderMode != WaterRenderMode.ImageEffectDeferred)
			{
				return;
			}
			int baseEffectWidth = waterCamera.BaseEffectWidth;
			int baseEffectHeight = waterCamera.BaseEffectHeight;
			FilterMode filter = (waterCamera.BaseEffectsQuality <= 0.98f) ? FilterMode.Bilinear : FilterMode.Point;
			Material depthBlit = this._DepthBlit;
			if (this._Commands == null)
			{
				this._Commands = new CommandBuffer
				{
					name = "[Ultimate Water] Render Depth"
				};
			}
			this._Commands.Clear();
			this._Commands.GetTemporaryRT(this._WaterDepthTextureId, baseEffectWidth, baseEffectHeight, (this._DepthFormat != RenderTextureFormat.Depth) ? 16 : 32, filter, this._DepthFormat, RenderTextureReadWrite.Linear);
			this._Commands.SetRenderTarget(this._WaterDepthTextureId);
			this._Commands.ClearRenderTarget(true, true, Color.white);
			waterCamera.AddWaterRenderCommands(this._Commands, ShaderUtility.Instance.Get(ShaderList.WaterDepth), true, true, false);
			this._Commands.GetTemporaryRT(this._WaterlessDepthId, baseEffectWidth, baseEffectHeight, (this._BlendedDepthFormat != RenderTextureFormat.Depth) ? 0 : 32, FilterMode.Point, this._BlendedDepthFormat, RenderTextureReadWrite.Linear);
			this._Commands.SetRenderTarget(this._WaterlessDepthId);
			this._Commands.DrawMesh(Quads.BipolarXInversedY, Matrix4x4.identity, depthBlit, 0, (this._BlendedDepthFormat != RenderTextureFormat.Depth) ? 0 : 3);
			this._Commands.GetTemporaryRT(this._CameraDepthTextureId, baseEffectWidth, baseEffectHeight, (this._BlendedDepthFormat != RenderTextureFormat.Depth) ? 0 : 32, FilterMode.Point, this._BlendedDepthFormat, RenderTextureReadWrite.Linear);
			this._Commands.SetRenderTarget(this._CameraDepthTextureId);
			this._Commands.ClearRenderTarget(true, true, Color.white);
			this._Commands.DrawMesh(Quads.BipolarXInversedY, Matrix4x4.identity, depthBlit, 0, (this._BlendedDepthFormat != RenderTextureFormat.Depth) ? 1 : 4);
			this._Commands.SetGlobalTexture("_CameraDepthTexture", this._CameraDepthTextureId);
			this.Unbind(waterCamera);
			this.Bind(waterCamera);
		}

		public void Render(WaterCamera waterCamera, RenderTexture source, RenderTexture destination)
		{
		}

		private Material _DepthBlit
		{
			get
			{
				return (!(this._DepthBlitCache != null)) ? (this._DepthBlitCache = ShaderUtility.Instance.CreateMaterial(ShaderList.DepthCopy, HideFlags.DontSave)) : this._DepthBlitCache;
			}
		}

		private void Bind(WaterCamera waterCamera)
		{
			Camera cameraComponent = waterCamera.CameraComponent;
			bool singlePassStereoRendering = WaterProjectSettings.Instance.SinglePassStereoRendering;
			cameraComponent.AddCommandBuffer((cameraComponent.actualRenderingPath != RenderingPath.Forward) ? CameraEvent.BeforeLighting : ((!singlePassStereoRendering) ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeForwardOpaque), this._Commands);
		}

		private void Unbind(WaterCamera waterCamera)
		{
			Camera cameraComponent = waterCamera.CameraComponent;
			bool singlePassStereoRendering = WaterProjectSettings.Instance.SinglePassStereoRendering;
			cameraComponent.RemoveCommandBuffer((!singlePassStereoRendering) ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeForwardOpaque, this._Commands);
			cameraComponent.RemoveCommandBuffer(CameraEvent.BeforeLighting, this._Commands);
		}

		private CommandBuffer _Commands;

		private RenderTextureFormat _DepthFormat;

		private RenderTextureFormat _BlendedDepthFormat;

		private Material _DepthBlitCache;

		private int _WaterDepthTextureId;

		private int _WaterlessDepthId;

		private int _CameraDepthTextureId;
	}
}
