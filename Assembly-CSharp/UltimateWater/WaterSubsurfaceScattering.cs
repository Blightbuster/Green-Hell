using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Events;

namespace UltimateWater
{
	[Serializable]
	public sealed class WaterSubsurfaceScattering : WaterModule
	{
		public float IsotropicScatteringIntensity
		{
			get
			{
				return this._ShaderParams.x;
			}
			set
			{
				this._ShaderParams.x = value;
			}
		}

		public float SubsurfaceScatteringContrast
		{
			get
			{
				return this._ShaderParams.y;
			}
			set
			{
				this._ShaderParams.y = value;
			}
		}

		internal override void OnWaterRender(WaterCamera waterCamera)
		{
			Camera cameraComponent = waterCamera.CameraComponent;
			Rect localMapsRect = waterCamera.LocalMapsRect;
			if (localMapsRect.width == 0f || !Application.isPlaying || this._Mode == WaterSubsurfaceScattering.SubsurfaceScatteringMode.Disabled)
			{
				return;
			}
			RenderTexture temporary = RenderTexture.GetTemporary(this._AmbientResolution, this._AmbientResolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
			temporary.filterMode = FilterMode.Bilinear;
			Camera effectsCamera = waterCamera.EffectsCamera;
			WaterCamera component = effectsCamera.GetComponent<WaterCamera>();
			component.enabled = true;
			component.GeometryType = WaterGeometryType.UniformGrid;
			WaterSubsurfaceScattering._CachedRenderList[0] = this._Water;
			component.SetCustomWaterRenderList(WaterSubsurfaceScattering._CachedRenderList);
			effectsCamera.stereoTargetEye = StereoTargetEyeMask.None;
			effectsCamera.enabled = false;
			effectsCamera.depthTextureMode = DepthTextureMode.None;
			effectsCamera.renderingPath = RenderingPath.Forward;
			effectsCamera.orthographic = true;
			effectsCamera.orthographicSize = localMapsRect.width * 0.5f;
			effectsCamera.cullingMask = 1 << this._LightingLayer;
			effectsCamera.farClipPlane = 2000f;
			effectsCamera.ResetProjectionMatrix();
			effectsCamera.clearFlags = CameraClearFlags.Nothing;
			effectsCamera.allowHDR = true;
			effectsCamera.transform.position = new Vector3(localMapsRect.center.x, 1000f, localMapsRect.center.y);
			effectsCamera.transform.rotation = Quaternion.LookRotation(new Vector3(0f, -1f, 0f), new Vector3(0f, 0f, 1f));
			effectsCamera.targetTexture = temporary;
			Shader.SetGlobalVector("_ScatteringParams", this._ShaderParams);
			Shader.SetGlobalVector("_WorldSpaceOriginalCameraPos", cameraComponent.transform.position);
			int pixelLightCount = 3;
			if (this._LightCount >= 0)
			{
				pixelLightCount = QualitySettings.pixelLightCount;
				QualitySettings.pixelLightCount = this._LightCount;
			}
			Shader shader = ShaderUtility.Instance.Get(ShaderList.CollectLight);
			this._Water.gameObject.layer = this._LightingLayer;
			effectsCamera.RenderWithShader(shader, "CustomType");
			this._Water.gameObject.layer = WaterProjectSettings.Instance.WaterLayer;
			if (this._LightCount >= 0)
			{
				QualitySettings.pixelLightCount = pixelLightCount;
			}
			component.GeometryType = WaterGeometryType.Auto;
			component.SetCustomWaterRenderList(null);
			RenderTexture temporary2 = RenderTexture.GetTemporary(this._AmbientResolution, this._AmbientResolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
			temporary2.filterMode = FilterMode.Point;
			Color parameterValue = this._Water.Materials.GetParameterValue(WaterMaterials.ColorParameter.AbsorptionColor);
			this._SubsurfaceScatteringBlur.BlurMaterial.SetVector("_ScatteringParams", this._ShaderParams);
			this._SubsurfaceScatteringBlur.Apply(temporary, temporary2, parameterValue, waterCamera.LocalMapsRect.width, this._IgnoredLightFraction);
			RenderTexture.ReleaseTemporary(temporary);
			Graphics.Blit(temporary2, this._ScatteringTex, this._SubsurfaceScatteringBlur.BlurMaterial, 1);
			RenderTexture.ReleaseTemporary(temporary2);
			this._Water.Renderer.PropertyBlock.SetTexture("_SubsurfaceScattering", this._ScatteringTex);
			Graphics.SetRenderTarget(null);
		}

		internal override void Start(Water water)
		{
			this._Water = water;
			water.ProfilesManager.Changed.AddListener(new UnityAction<Water>(this.ResolveProfileData));
		}

		internal override void Enable()
		{
			this.Validate();
			if (Application.isPlaying && this._Mode == WaterSubsurfaceScattering.SubsurfaceScatteringMode.TextureSpace)
			{
				this._ScatteringTex = new RenderTexture(this._AmbientResolution, this._AmbientResolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
				{
					name = "[UWS] WaterSubsurfaceScattering - Scattering Tex",
					hideFlags = HideFlags.DontSave,
					filterMode = FilterMode.Bilinear,
					useMipMap = WaterProjectSettings.Instance.AllowFloatingPointMipMaps,
					autoGenerateMips = WaterProjectSettings.Instance.AllowFloatingPointMipMaps
				};
			}
		}

		internal override void Disable()
		{
			if (this._ScatteringTex != null)
			{
				this._ScatteringTex.Destroy();
				this._ScatteringTex = null;
			}
			if (this._Water != null)
			{
				Texture2D texture2D = DefaultTextures.Get(Color.white);
				if (texture2D != null)
				{
					this._Water.Renderer.PropertyBlock.SetTexture("_SubsurfaceScattering", texture2D);
				}
			}
		}

		internal override void Destroy()
		{
			if (this._Water == null)
			{
				return;
			}
			this._Water.ProfilesManager.Changed.RemoveListener(new UnityAction<Water>(this.ResolveProfileData));
		}

		private void ResolveProfileData(Water water)
		{
			Water.WeightedProfile[] profiles = water.ProfilesManager.Profiles;
			this._ShaderParams.x = 0f;
			this._ShaderParams.y = 0f;
			for (int i = 0; i < profiles.Length; i++)
			{
				WaterProfileData profile = profiles[i].Profile;
				float weight = profiles[i].Weight;
				this._ShaderParams.x = this._ShaderParams.x + profile.IsotropicScatteringIntensity * weight;
				this._ShaderParams.y = this._ShaderParams.y + profile.SubsurfaceScatteringContrast * weight;
			}
			this._ShaderParams.x = this._ShaderParams.x * (1f + this._ShaderParams.y);
		}

		internal override void Validate()
		{
			if (this._SubsurfaceScatteringBlur == null)
			{
				this._SubsurfaceScatteringBlur = new BlurSSS();
			}
			this._SubsurfaceScatteringBlur.Validate("UltimateWater/Utilities/Blur (Subsurface Scattering)", "Systems/Ultimate Water System/Shaders/Blurs", 6);
		}

		[SerializeField]
		private WaterSubsurfaceScattering.SubsurfaceScatteringMode _Mode = WaterSubsurfaceScattering.SubsurfaceScatteringMode.TextureSpace;

		[SerializeField]
		private BlurSSS _SubsurfaceScatteringBlur;

		[Range(0f, 0.9f)]
		[SerializeField]
		private float _IgnoredLightFraction = 0.15f;

		[SerializeField]
		[Resolution(128, new int[]
		{
			64,
			128,
			256,
			512
		})]
		private int _AmbientResolution = 128;

		[SerializeField]
		[Range(-1f, 6f)]
		private int _LightCount = -1;

		[SerializeField]
		private int _LightingLayer = 22;

		private RenderTexture _ScatteringTex;

		private Vector4 _ShaderParams;

		private Water _Water;

		private static readonly List<Water> _CachedRenderList = new List<Water>
		{
			null
		};

		[Serializable]
		public enum SubsurfaceScatteringMode
		{
			Disabled,
			TextureSpace
		}
	}
}
