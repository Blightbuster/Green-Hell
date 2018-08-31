using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateWater
{
	public class MaskModule : IRenderModule
	{
		public void OnEnable(WaterCamera waterCamera)
		{
			this._Commands = new CommandBuffer
			{
				name = "[Water] : Render Volumes and Masks"
			};
			this._SubtractiveMaskId = ShaderVariables.SubtractiveMask;
			this._AdditiveMaskId = ShaderVariables.AdditiveMask;
			this._VolumeFrontFastShader = ShaderUtility.Instance.Get(ShaderList.VolumesFrontSimple);
			this._VolumeFrontShader = ShaderUtility.Instance.Get(ShaderList.VolumesFront);
			this._VolumeBackShader = ShaderUtility.Instance.Get(ShaderList.VolumesBack);
		}

		public void OnDisable(WaterCamera waterCamera)
		{
			if (this._Commands != null)
			{
				Camera cameraComponent = waterCamera.CameraComponent;
				CameraEvent @event = MaskModule.GetEvent(waterCamera);
				cameraComponent.RemoveCommandBuffer(@event, this._Commands);
				this._Commands.Release();
				this._Commands = null;
			}
		}

		public void OnValidate(WaterCamera waterCamera)
		{
			ShaderUtility instance = ShaderUtility.Instance;
			instance.Use(ShaderList.VolumesFrontSimple);
			instance.Use(ShaderList.VolumesFront);
			instance.Use(ShaderList.VolumesBack);
		}

		public void Process(WaterCamera waterCamera)
		{
			if (!waterCamera.RenderVolumes)
			{
				return;
			}
			bool hasSubtractiveVolumes = false;
			bool hasAdditiveVolumes = false;
			bool hasFlatMasks = false;
			List<Water> waters = ApplicationSingleton<WaterSystem>.Instance.Waters;
			for (int i = waters.Count - 1; i >= 0; i--)
			{
				waters[i].Renderer.OnSharedSubtractiveMaskRender(ref hasSubtractiveVolumes, ref hasAdditiveVolumes, ref hasFlatMasks);
			}
			MaskModule.SetupCamera(waterCamera);
			this._Commands.Clear();
			this.SubtractiveMask(waterCamera, hasSubtractiveVolumes, hasFlatMasks);
			this.AdditiveMask(waterCamera, hasAdditiveVolumes);
			if (this._Commands.sizeInBytes != 0)
			{
				Camera cameraComponent = waterCamera.CameraComponent;
				CameraEvent @event = MaskModule.GetEvent(waterCamera);
				cameraComponent.RemoveCommandBuffer(@event, this._Commands);
				cameraComponent.AddCommandBuffer(@event, this._Commands);
			}
			for (int j = waters.Count - 1; j >= 0; j--)
			{
				waters[j].Renderer.OnSharedMaskPostRender();
			}
		}

		public void Render(WaterCamera waterCamera, RenderTexture source, RenderTexture destination)
		{
		}

		private void SubtractiveMask(WaterCamera waterCamera, bool hasSubtractiveVolumes, bool hasFlatMasks)
		{
			if (hasSubtractiveVolumes || hasFlatMasks)
			{
				this.RenderSubtractivePass(waterCamera, hasSubtractiveVolumes, hasFlatMasks);
			}
			else
			{
				Shader.SetGlobalTexture(this._SubtractiveMaskId, DefaultTextures.Get(Color.clear));
			}
		}

		private void AdditiveMask(WaterCamera waterCamera, bool hasAdditiveVolumes)
		{
			if (hasAdditiveVolumes)
			{
				this.RenderAdditivePass(waterCamera);
			}
			else
			{
				Shader.SetGlobalTexture(this._AdditiveMaskId, DefaultTextures.Get(Color.clear));
			}
		}

		private void RenderSubtractivePass(WaterCamera waterCamera, bool hasSubtractiveVolumes, bool hasFlatMasks)
		{
			int baseEffectWidth = waterCamera.BaseEffectWidth;
			int baseEffectHeight = waterCamera.BaseEffectHeight;
			FilterMode filter = (waterCamera.BaseEffectsQuality <= 0.98f) ? FilterMode.Bilinear : FilterMode.Point;
			Camera effectsCamera = waterCamera.EffectsCamera;
			WaterCamera component = effectsCamera.GetComponent<WaterCamera>();
			this._Commands.GetTemporaryRT(this._SubtractiveMaskId, baseEffectWidth, baseEffectHeight, 24, filter, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
			this._Commands.SetRenderTarget(this._SubtractiveMaskId);
			this._Commands.ClearRenderTarget(true, true, new Color(0f, 0f, 0f, 0f));
			if (hasSubtractiveVolumes)
			{
				Shader shader = (!waterCamera.IsInsideSubtractiveVolume) ? this._VolumeFrontFastShader : this._VolumeFrontShader;
				effectsCamera.cullingMask = 1 << WaterProjectSettings.Instance.WaterLayer;
				component.AddWaterRenderCommands(this._Commands, shader, false, true, waterCamera.IsInsideSubtractiveVolume);
				component.AddWaterRenderCommands(this._Commands, this._VolumeBackShader, false, true, false);
			}
			if (hasFlatMasks && waterCamera.RenderFlatMasks)
			{
				effectsCamera.cullingMask = 1 << WaterProjectSettings.Instance.WaterTempLayer;
				component.AddWaterMasksRenderCommands(this._Commands);
			}
		}

		private void RenderAdditivePass(WaterCamera waterCamera)
		{
			int baseEffectWidth = waterCamera.BaseEffectWidth;
			int baseEffectHeight = waterCamera.BaseEffectHeight;
			List<Water> waters = ApplicationSingleton<WaterSystem>.Instance.Waters;
			Camera effectsCamera = waterCamera.EffectsCamera;
			WaterCamera component = effectsCamera.GetComponent<WaterCamera>();
			Shader shader = (!waterCamera.IsInsideAdditiveVolume) ? this._VolumeFrontFastShader : this._VolumeFrontShader;
			FilterMode filter = (waterCamera.BaseEffectsQuality <= 0.98f) ? FilterMode.Bilinear : FilterMode.Point;
			for (int i = waters.Count - 1; i >= 0; i--)
			{
				waters[i].Renderer.OnSharedMaskAdditiveRender();
			}
			effectsCamera.cullingMask = 1 << WaterProjectSettings.Instance.WaterLayer;
			this._Commands.GetTemporaryRT(this._AdditiveMaskId, baseEffectWidth, baseEffectHeight, 24, filter, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
			this._Commands.SetRenderTarget(this._AdditiveMaskId);
			this._Commands.ClearRenderTarget(true, true, new Color(0f, 0f, 0f, 0f));
			component.AddWaterRenderCommands(this._Commands, shader, false, true, waterCamera.IsInsideAdditiveVolume);
			component.AddWaterRenderCommands(this._Commands, this._VolumeBackShader, false, true, false);
		}

		private static void SetupCamera(WaterCamera waterCamera)
		{
			Camera effectsCamera = waterCamera.EffectsCamera;
			effectsCamera.transform.position = waterCamera.transform.position;
			effectsCamera.transform.rotation = waterCamera.transform.rotation;
			effectsCamera.projectionMatrix = waterCamera.CameraComponent.projectionMatrix;
		}

		private static CameraEvent GetEvent(WaterCamera waterCamera)
		{
			Camera cameraComponent = waterCamera.CameraComponent;
			return (cameraComponent.actualRenderingPath != RenderingPath.Forward) ? CameraEvent.BeforeGBuffer : CameraEvent.BeforeForwardOpaque;
		}

		private CommandBuffer _Commands;

		private int _SubtractiveMaskId;

		private int _AdditiveMaskId;

		private Shader _VolumeFrontFastShader;

		private Shader _VolumeFrontShader;

		private Shader _VolumeBackShader;
	}
}
