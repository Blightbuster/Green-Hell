using System;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace UltimateWater
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Ultimate Water/Underwater IME")]
	[RequireComponent(typeof(WaterCamera))]
	public sealed class UnderwaterIME : MonoBehaviour, IWaterImageEffect
	{
		public float Intensity
		{
			get
			{
				return this._Intensity;
			}
		}

		public bool EffectEnabled
		{
			get
			{
				return this._EffectEnabled;
			}
			set
			{
				this._EffectEnabled = value;
			}
		}

		public Water WaterOverride
		{
			get
			{
				return this._WaterOverride;
			}
			set
			{
				this._WaterOverride = value;
				this._HasWaterOverride = (value != null);
				this.OnSubmersionStateChanged(this._LocalWaterCamera);
			}
		}

		public void OnWaterCameraEnabled()
		{
			WaterCamera component = base.GetComponent<WaterCamera>();
			component.SubmersionStateChanged.AddListener(new UnityAction<WaterCamera>(this.OnSubmersionStateChanged));
		}

		public void OnWaterCameraPreCull()
		{
			if (!this._EffectEnabled)
			{
				base.enabled = false;
				return;
			}
			if (this._HasWaterOverride)
			{
				base.enabled = true;
				this._RenderUnderwaterMask = true;
				return;
			}
			SubmersionState submersionState = this._LocalWaterCamera.SubmersionState;
			if (submersionState != SubmersionState.None)
			{
				if (submersionState != SubmersionState.Partial)
				{
					if (submersionState == SubmersionState.Full)
					{
						base.enabled = true;
						this._RenderUnderwaterMask = false;
					}
				}
				else
				{
					base.enabled = true;
					this._RenderUnderwaterMask = true;
				}
			}
			else
			{
				base.enabled = false;
			}
			float num = this._LocalCamera.nearClipPlane * Mathf.Tan(this._LocalCamera.fieldOfView * 0.5f * 0.0174532924f);
			float num2 = base.transform.position.y - this._LocalWaterCamera.WaterLevel;
			float effectsIntensity = (-num2 + num) * 0.25f;
			this.SetEffectsIntensity(effectsIntensity);
		}

		private void Awake()
		{
			this._LocalCamera = base.GetComponent<Camera>();
			this._LocalWaterCamera = base.GetComponent<WaterCamera>();
			this.OnValidate();
			this._MaskMaterial = ShaderUtility.Instance.CreateMaterial(ShaderList.ScreenSpaceMask, HideFlags.DontSave);
			this._ImeMaterial = ShaderUtility.Instance.CreateMaterial(ShaderList.BaseIME, HideFlags.DontSave);
			this._NoiseMaterial = ShaderUtility.Instance.CreateMaterial(ShaderList.Noise, HideFlags.DontSave);
			this._ComposeUnderwaterMaskMaterial = ShaderUtility.Instance.CreateMaterial(ShaderList.ComposeUnderWaterMask, HideFlags.DontSave);
			this._ReverbFilter = base.GetComponent<AudioReverbFilter>();
			if (this._ReverbFilter == null && this._UnderwaterAudio)
			{
				this._ReverbFilter = base.gameObject.AddComponent<AudioReverbFilter>();
			}
		}

		private void OnDisable()
		{
			TextureUtility.Release(ref this._UnderwaterMask);
			if (this._MaskCommandBuffer != null)
			{
				this._MaskCommandBuffer.Clear();
			}
		}

		private void OnDestroy()
		{
			if (this._MaskCommandBuffer != null)
			{
				this._MaskCommandBuffer.Dispose();
				this._MaskCommandBuffer = null;
			}
			if (this._Blur != null)
			{
				this._Blur.Dispose();
				this._Blur = null;
			}
			this._MaskMaterial.Destroy();
			this._ImeMaterial.Destroy();
		}

		private void OnValidate()
		{
			ShaderUtility.Instance.Use(ShaderList.ScreenSpaceMask);
			ShaderUtility.Instance.Use(ShaderList.BaseIME);
			ShaderUtility.Instance.Use(ShaderList.Noise);
			ShaderUtility.Instance.Use(ShaderList.ComposeUnderWaterMask);
			if (this._Blur != null)
			{
				this._Blur.Validate("UltimateWater/Utilities/Blur (Underwater)", null, 0);
			}
		}

		private void OnPreCull()
		{
			this.RenderUnderwaterMask();
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			Water water = (!this._HasWaterOverride) ? this._LocalWaterCamera.ContainingWater : this._WaterOverride;
			if (!this._LocalWaterCamera.enabled || water == null)
			{
				Graphics.Blit(source, destination);
				return;
			}
			source.filterMode = FilterMode.Bilinear;
			TemporaryRenderTexture temporary = RenderTexturesCache.GetTemporary(source.width, source.height, 0, (!(destination != null)) ? source.format : destination.format, true, false, false);
			temporary.Texture.filterMode = FilterMode.Bilinear;
			temporary.Texture.wrapMode = TextureWrapMode.Clamp;
			this.RenderDepthScatter(source, temporary);
			this._Blur.TotalSize = water.Materials.UnderwaterBlurSize * this._CameraBlurScale;
			this._Blur.Apply(temporary);
			this.RenderDistortions(temporary, destination);
			temporary.Dispose();
		}

		private void RenderUnderwaterMask()
		{
			if (this._MaskCommandBuffer == null)
			{
				return;
			}
			this._MaskCommandBuffer.Clear();
			Water water = (!this._HasWaterOverride) ? this._LocalWaterCamera.ContainingWater : this._WaterOverride;
			Camera current = Camera.current;
			int underwaterMask = ShaderVariables.UnderwaterMask;
			int underwaterMask2 = ShaderVariables.UnderwaterMask2;
			if (this._UnderwaterMask == null)
			{
				int width = Mathf.RoundToInt((float)current.pixelWidth * this._MaskResolution);
				int height = Mathf.RoundToInt((float)current.pixelHeight * this._MaskResolution);
				this._UnderwaterMask = new RenderTexture(width, height, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear)
				{
					filterMode = FilterMode.Bilinear,
					name = "[UWS] UnderwaterIME - Mask"
				};
				this._UnderwaterMask.Create();
			}
			if (this._RenderUnderwaterMask || (water != null && water.Renderer.MaskCount > 0))
			{
				int width2 = Mathf.RoundToInt((float)current.pixelWidth * this._MaskResolution);
				int height2 = Mathf.RoundToInt((float)current.pixelHeight * this._MaskResolution);
				this._MaskCommandBuffer.GetTemporaryRT(underwaterMask, width2, height2, 0, FilterMode.Bilinear, RenderTextureFormat.R8, RenderTextureReadWrite.Linear, 1);
				this._MaskCommandBuffer.GetTemporaryRT(underwaterMask2, width2, height2, 0, FilterMode.Point, RenderTextureFormat.R8, RenderTextureReadWrite.Linear, 1);
			}
			else
			{
				this._MaskCommandBuffer.GetTemporaryRT(underwaterMask, 4, 4, 0, FilterMode.Point, RenderTextureFormat.R8, RenderTextureReadWrite.Linear, 1);
			}
			if (this._RenderUnderwaterMask && water != null)
			{
				this._MaskMaterial.CopyPropertiesFromMaterial(water.Materials.SurfaceMaterial);
				this._MaskCommandBuffer.SetRenderTarget(underwaterMask2);
				this._MaskCommandBuffer.ClearRenderTarget(false, true, Color.black);
				WaterGeometry geometry = water.Geometry;
				Matrix4x4 matrix;
				Mesh[] transformedMeshes = geometry.GetTransformedMeshes(this._LocalCamera, out matrix, (geometry.GeometryType != WaterGeometry.Type.ProjectionGrid) ? WaterGeometryType.Auto : WaterGeometryType.RadialGrid, true, geometry.ComputeVertexCountForCamera(current));
				for (int i = transformedMeshes.Length - 1; i >= 0; i--)
				{
					this._MaskCommandBuffer.DrawMesh(transformedMeshes[i], matrix, this._MaskMaterial, 0, 0, water.Renderer.PropertyBlock);
				}
				this._MaskCommandBuffer.SetRenderTarget(underwaterMask);
				this._MaskCommandBuffer.DrawMesh(Quads.BipolarXInversedY, Matrix4x4.identity, this._ImeMaterial, 0, 3, water.Renderer.PropertyBlock);
				this._MaskCommandBuffer.ReleaseTemporaryRT(underwaterMask2);
			}
			else
			{
				this._MaskCommandBuffer.SetRenderTarget(underwaterMask);
				this._MaskCommandBuffer.ClearRenderTarget(false, true, Color.white);
			}
			this._MaskCommandBuffer.Blit(underwaterMask, this._UnderwaterMask);
			Shader.SetGlobalTexture(underwaterMask, this._UnderwaterMask);
			if (water != null && water.Renderer.MaskCount != 0 && this._LocalWaterCamera.RenderVolumes)
			{
				this._MaskCommandBuffer.Blit("_SubtractiveMask", underwaterMask, this._ComposeUnderwaterMaskMaterial, 0);
			}
			CameraEvent evt = (this._LocalCamera.actualRenderingPath != RenderingPath.Forward) ? CameraEvent.BeforeLighting : ((!WaterProjectSettings.Instance.SinglePassStereoRendering) ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeForwardOpaque);
			this._LocalCamera.RemoveCommandBuffer(evt, this._MaskCommandBuffer);
			this._LocalCamera.AddCommandBuffer(evt, this._MaskCommandBuffer);
		}

		private void RenderDepthScatter(Texture source, RenderTexture target)
		{
			Water water = (!this._HasWaterOverride) ? this._LocalWaterCamera.ContainingWater : this._WaterOverride;
			this._ImeMaterial.CopyPropertiesFromMaterial(water.Materials.SurfaceMaterial);
			this._ImeMaterial.SetTexture("_UnderwaterAbsorptionGradient", water.Materials.UnderwaterAbsorptionColorByDepth);
			this._ImeMaterial.SetFloat("_UnderwaterLightFadeScale", water.Materials.UnderwaterLightFadeScale);
			this._ImeMaterial.SetMatrix("UNITY_MATRIX_VP_INVERSE", Matrix4x4.Inverse(this._LocalCamera.projectionMatrix * this._LocalCamera.worldToCameraMatrix));
			MaterialPropertyBlock propertyBlock = water.Renderer.PropertyBlock;
			GraphicsUtilities.Blit(source, target, this._ImeMaterial, 1, propertyBlock);
		}

		private void RenderDistortions(Texture source, RenderTexture target)
		{
			Water water = (!this._HasWaterOverride) ? this._LocalWaterCamera.ContainingWater : this._WaterOverride;
			float underwaterDistortionsIntensity = water.Materials.UnderwaterDistortionsIntensity;
			if (underwaterDistortionsIntensity > 0f)
			{
				int width = Camera.current.pixelWidth >> 2;
				int height = Camera.current.pixelHeight >> 2;
				TemporaryRenderTexture temporary = RenderTexturesCache.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, true, false, false);
				this.RenderDistortionMap(temporary);
				temporary.Texture.filterMode = FilterMode.Bilinear;
				this._ImeMaterial.SetTexture("_DistortionTex", temporary);
				this._ImeMaterial.SetFloat("_DistortionIntensity", underwaterDistortionsIntensity);
				GraphicsUtilities.Blit(source, target, this._ImeMaterial, 2, water.Renderer.PropertyBlock);
				temporary.Dispose();
			}
			else
			{
				Graphics.Blit(source, target);
			}
		}

		private void RenderDistortionMap(RenderTexture target)
		{
			Water water = (!this._HasWaterOverride) ? this._LocalWaterCamera.ContainingWater : this._WaterOverride;
			this._NoiseMaterial.SetVector("_Offset", new Vector4(0f, 0f, Time.time * water.Materials.UnderwaterDistortionAnimationSpeed, 0f));
			this._NoiseMaterial.SetVector("_Period", new Vector4(4f, 4f, 4f, 4f));
			Graphics.Blit(null, target, this._NoiseMaterial, 3);
		}

		private void OnSubmersionStateChanged(WaterCamera waterCamera)
		{
			if (waterCamera.SubmersionState != SubmersionState.None || this._HasWaterOverride)
			{
				if (this._MaskCommandBuffer == null)
				{
					this._MaskCommandBuffer = new CommandBuffer
					{
						name = "[UWS] UnderwaterIME - Render Underwater Mask"
					};
				}
			}
			else
			{
				if (this._MaskCommandBuffer == null)
				{
					return;
				}
				Camera component = base.GetComponent<Camera>();
				component.RemoveCommandBuffer((!WaterProjectSettings.Instance.SinglePassStereoRendering) ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeForwardOpaque, this._MaskCommandBuffer);
				component.RemoveCommandBuffer(CameraEvent.AfterLighting, this._MaskCommandBuffer);
			}
		}

		private void SetEffectsIntensity(float intensity)
		{
			if (this._LocalCamera == null)
			{
				return;
			}
			intensity = Mathf.Clamp01(intensity);
			if (this._Intensity == intensity)
			{
				return;
			}
			this._Intensity = intensity;
			if (this._ReverbFilter == null || !this._UnderwaterAudio)
			{
				return;
			}
			float num = (intensity <= 0.05f) ? intensity : Mathf.Clamp01(intensity + 0.7f);
			this._ReverbFilter.dryLevel = -2000f * num;
			this._ReverbFilter.room = -10000f * (1f - num);
			this._ReverbFilter.roomHF = Mathf.Lerp(-10000f, -4000f, num);
			this._ReverbFilter.decayTime = 1.6f * num;
			this._ReverbFilter.decayHFRatio = 0.1f * num;
			this._ReverbFilter.reflectionsLevel = -449f * num;
			this._ReverbFilter.reverbLevel = 1500f * num;
			this._ReverbFilter.reverbDelay = 0.0259f * num;
		}

		[SerializeField]
		private Blur _Blur;

		[SerializeField]
		private bool _UnderwaterAudio = true;

		[Tooltip("Individual camera blur scale. It's recommended to modify blur scale through water profiles. Use this one, only if some of your cameras need a clear view and some don't.")]
		[SerializeField]
		[Range(0f, 4f)]
		private float _CameraBlurScale = 1f;

		[SerializeField]
		[Range(0.1f, 1f)]
		private float _MaskResolution = 0.5f;

		private Material _MaskMaterial;

		private Material _ImeMaterial;

		private Material _NoiseMaterial;

		private Material _ComposeUnderwaterMaskMaterial;

		private Camera _LocalCamera;

		private WaterCamera _LocalWaterCamera;

		private AudioReverbFilter _ReverbFilter;

		private CommandBuffer _MaskCommandBuffer;

		private float _Intensity = float.NaN;

		private bool _RenderUnderwaterMask;

		private Water _WaterOverride;

		private bool _HasWaterOverride;

		private bool _EffectEnabled = true;

		private RenderTexture _UnderwaterMask;
	}
}
