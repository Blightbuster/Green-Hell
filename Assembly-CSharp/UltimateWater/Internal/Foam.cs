using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace UltimateWater.Internal
{
	public sealed class Foam : WaterModule
	{
		public Foam(Water water, Foam.Data data)
		{
			this._Water = water;
			this._WindWaves = water.WindWaves;
			this._Overlays = water.DynamicWater;
			this._Data = data;
			this.Validate();
			this._WindWaves.ResolutionChanged.AddListener(new UnityAction<WindWaves>(this.OnResolutionChanged));
			this._Resolution = Mathf.RoundToInt((float)this._WindWaves.FinalResolution * data.Supersampling);
			this._GlobalFoamSimulationMaterial = new Material(this._GlobalFoamSimulationShader)
			{
				hideFlags = HideFlags.DontSave
			};
			this._FirstFrame = true;
		}

		public float FoamIntensity
		{
			get
			{
				return this._FoamIntensity;
			}
			set
			{
				if (float.IsNaN(value))
				{
					this._FoamIntensityOverriden = false;
					this.OnProfilesChanged(this._Water);
				}
				else
				{
					this._FoamIntensityOverriden = true;
					this._FoamIntensity = value;
					if (this._GlobalFoamSimulationMaterial != null)
					{
						float y = this._FoamThreshold * (float)this._Resolution / 2048f * 0.5f;
						this._GlobalFoamSimulationMaterial.SetVector(ShaderVariables.FoamParameters, new Vector4(this._FoamIntensity * 0.6f, y, 0f, this._FoamFadingFactor));
					}
					MaterialPropertyBlock propertyBlock = this._Water.Renderer.PropertyBlock;
					float y2 = this._FoamThreshold * (float)this._Resolution / 2048f * 0.5f;
					propertyBlock.SetVector(ShaderVariables.FoamParameters, new Vector4(this._FoamIntensity * 0.6f, y2, 150f / (this._FoamShoreExtent * this._FoamShoreExtent), this._FoamFadingFactor));
				}
			}
		}

		public Texture FoamMap
		{
			get
			{
				return this._FoamMapA;
			}
		}

		public void RenderOverlays(DynamicWaterCameraData overlays)
		{
			if (!Application.isPlaying || !this.CheckPreresquisites())
			{
				return;
			}
			WaterCamera camera = overlays.Camera;
			if (camera.Type != WaterCamera.CameraType.Normal)
			{
				return;
			}
			int layer = this._Water.gameObject.layer;
			Foam.CameraRenderData cameraRenderData;
			if (!Foam._LayerUpdateFrames.TryGetValue(camera, out cameraRenderData))
			{
				cameraRenderData = (Foam._LayerUpdateFrames[camera] = new Foam.CameraRenderData());
				WaterCamera waterCamera = camera;
				if (Foam.<>f__mg$cache0 == null)
				{
					Foam.<>f__mg$cache0 = new Action<WaterCamera>(Foam.OnCameraDestroyed);
				}
				waterCamera.Destroyed += Foam.<>f__mg$cache0;
			}
			int frameCount = Time.frameCount;
			if (cameraRenderData.RenderFramePerLayer[layer] < frameCount)
			{
				cameraRenderData.RenderFramePerLayer[layer] = frameCount;
				if (this._Water.WindWaves.FinalRenderMode == WaveSpectrumRenderMode.FullFFT)
				{
					RenderTexture[] displacementDeltaMaps = this.GetDisplacementDeltaMaps();
					float y = this._FoamThreshold * (float)this._Resolution / 2048f * 0.5f;
					this._GlobalFoamSimulationMaterial.SetVector(ShaderVariables.FoamParameters, new Vector4(this._FoamIntensity * 0.6f, y, 0f, this._FoamFadingFactor));
					for (int i = 0; i < 4; i++)
					{
						Texture displacementMap = this._Water.WindWaves.WaterWavesFFT.GetDisplacementMap(i);
						RenderTexture dest = displacementDeltaMaps[i];
						this._GlobalFoamSimulationMaterial.SetFloat(ShaderVariables.WaterTileSizeInvSrt, this._Water.WindWaves.TileSizesInv[i]);
						Graphics.Blit(displacementMap, dest, this._GlobalFoamSimulationMaterial, 1);
					}
					Shader.SetGlobalTexture("_FoamMapPrevious", overlays.FoamMapPrevious);
					Shader.SetGlobalVector("_WaterOffsetDelta", this._Water.SurfaceOffset - cameraRenderData.LastSurfaceOffset);
					cameraRenderData.LastSurfaceOffset = this._Water.SurfaceOffset;
					Camera planeProjectorCamera = camera.PlaneProjectorCamera;
					planeProjectorCamera.cullingMask = 1 << layer;
					planeProjectorCamera.GetComponent<WaterCamera>().RenderWaterWithShader("[PW Water] Foam", overlays.FoamMap, this._LocalFoamSimulationShader, this._Water);
				}
			}
			this._Water.Renderer.PropertyBlock.SetTexture("_FoamMap", overlays.FoamMap);
		}

		internal override void Enable()
		{
			this._Water.ProfilesManager.Changed.AddListener(new UnityAction<Water>(this.OnProfilesChanged));
			this.OnProfilesChanged(this._Water);
		}

		internal override void Disable()
		{
			this._Water.ProfilesManager.Changed.RemoveListener(new UnityAction<Water>(this.OnProfilesChanged));
		}

		private void SetupFoamMaterials()
		{
			if (this._GlobalFoamSimulationMaterial != null)
			{
				float num = this._FoamThreshold * (float)this._Resolution / 2048f * 0.5f;
				float num2 = num * 220f;
				this._GlobalFoamSimulationMaterial.SetVector(ShaderVariables.FoamParameters, new Vector4(this._FoamIntensity * 0.6f, num, 0f, this._FoamFadingFactor));
				this._GlobalFoamSimulationMaterial.SetVector(ShaderVariables.FoamIntensity, new Vector4(num2 / this._WindWaves.TileSizes.x, num2 / this._WindWaves.TileSizes.y, num2 / this._WindWaves.TileSizes.z, num2 / this._WindWaves.TileSizes.w));
			}
		}

		internal override void Validate()
		{
			if (this._GlobalFoamSimulationShader == null)
			{
				this._GlobalFoamSimulationShader = Shader.Find("UltimateWater/Foam/Global");
			}
			if (this._LocalFoamSimulationShader == null)
			{
				this._LocalFoamSimulationShader = Shader.Find("UltimateWater/Foam/Local");
			}
			this._Data.Supersampling = (float)Mathf.ClosestPowerOfTwo(Mathf.RoundToInt(this._Data.Supersampling * 4096f)) / 4096f;
		}

		internal override void Destroy()
		{
			if (this._FoamMapA != null)
			{
				this._FoamMapA.Destroy();
				this._FoamMapB.Destroy();
				this._FoamMapA = null;
				this._FoamMapB = null;
			}
			if (this._DisplacementDeltaMaps != null)
			{
				for (int i = 0; i < this._DisplacementDeltaMaps.Length; i++)
				{
					this._DisplacementDeltaMaps[i].Destroy();
				}
				this._DisplacementDeltaMaps = null;
			}
		}

		internal override void Update()
		{
			if (!this._FirstFrame && this._Overlays == null)
			{
				this.UpdateFoamTiled();
			}
			else
			{
				this._FirstFrame = false;
			}
		}

		private void CheckTilesFoamResources()
		{
			if (this._FoamMapA == null)
			{
				this._FoamMapA = this.CreateRt(0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear, FilterMode.Trilinear, TextureWrapMode.Repeat);
				this._FoamMapA.name = "[UWS] Foam - Map A";
				this._FoamMapB = this.CreateRt(0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear, FilterMode.Trilinear, TextureWrapMode.Repeat);
				this._FoamMapB.name = "[UWS] Foam - Map B";
				RenderTexture.active = null;
			}
		}

		private RenderTexture CreateRt(int depth, RenderTextureFormat format, RenderTextureReadWrite readWrite, FilterMode filterMode, TextureWrapMode wrapMode)
		{
			bool allowFloatingPointMipMaps = WaterProjectSettings.Instance.AllowFloatingPointMipMaps;
			RenderTexture renderTexture = new RenderTexture(this._Resolution, this._Resolution, depth, format, readWrite)
			{
				name = "[UWS] Foam",
				hideFlags = HideFlags.DontSave,
				filterMode = filterMode,
				wrapMode = wrapMode,
				useMipMap = allowFloatingPointMipMaps,
				autoGenerateMips = allowFloatingPointMipMaps
			};
			RenderTexture.active = renderTexture;
			GL.Clear(false, true, new Color(0f, 0f, 0f, 0f));
			return renderTexture;
		}

		private void UpdateFoamTiled()
		{
			if (!this.CheckPreresquisites())
			{
				return;
			}
			this.CheckTilesFoamResources();
			this.SetupFoamMaterials();
			WavesRendererFFT waterWavesFFT = this._WindWaves.WaterWavesFFT;
			this._GlobalFoamSimulationMaterial.SetTexture("_DisplacementMap0", waterWavesFFT.GetDisplacementMap(0));
			this._GlobalFoamSimulationMaterial.SetTexture("_DisplacementMap1", waterWavesFFT.GetDisplacementMap(1));
			this._GlobalFoamSimulationMaterial.SetTexture("_DisplacementMap2", waterWavesFFT.GetDisplacementMap(2));
			this._GlobalFoamSimulationMaterial.SetTexture("_DisplacementMap3", waterWavesFFT.GetDisplacementMap(3));
			Graphics.Blit(this._FoamMapA, this._FoamMapB, this._GlobalFoamSimulationMaterial, 0);
			this._Water.Renderer.PropertyBlock.SetTexture("_FoamMap", this._FoamMapB);
			this.SwapRenderTargets();
		}

		private void OnResolutionChanged(WindWaves windWaves)
		{
			this._Resolution = Mathf.RoundToInt((float)windWaves.FinalResolution * this._Data.Supersampling);
			this.Destroy();
		}

		private bool CheckPreresquisites()
		{
			return this._WindWaves != null && this._WindWaves.FinalRenderMode == WaveSpectrumRenderMode.FullFFT;
		}

		private void OnProfilesChanged(Water water)
		{
			Water.WeightedProfile[] profiles = water.ProfilesManager.Profiles;
			float num = 0f;
			this._FoamThreshold = 0f;
			this._FoamFadingFactor = 0f;
			this._FoamShoreExtent = 0f;
			float num2 = 0f;
			float num3 = 0f;
			if (profiles != null)
			{
				for (int i = profiles.Length - 1; i >= 0; i--)
				{
					Water.WeightedProfile weightedProfile = profiles[i];
					WaterProfileData profile = weightedProfile.Profile;
					float weight = weightedProfile.Weight;
					num += profile.FoamIntensity * weight;
					this._FoamThreshold += profile.FoamThreshold * weight;
					this._FoamFadingFactor += profile.FoamFadingFactor * weight;
					this._FoamShoreExtent += profile.FoamShoreExtent * weight;
					num2 += profile.FoamShoreIntensity * weight;
					num3 += profile.FoamNormalScale * weight;
				}
			}
			if (!this._FoamIntensityOverriden)
			{
				this._FoamIntensity = num;
			}
			MaterialPropertyBlock propertyBlock = water.Renderer.PropertyBlock;
			propertyBlock.SetFloat("_FoamNormalScale", num3);
			if (this._FoamShoreExtent < 0.001f)
			{
				this._FoamShoreExtent = 0.001f;
			}
			float y = this._FoamThreshold * (float)this._Resolution / 2048f * 0.5f;
			propertyBlock.SetVector(ShaderVariables.FoamParameters, new Vector4(num * 0.6f, y, 150f / (this._FoamShoreExtent * this._FoamShoreExtent), this._FoamFadingFactor));
			propertyBlock.SetFloat(ShaderVariables.FoamShoreIntensity, num2);
		}

		private void SwapRenderTargets()
		{
			RenderTexture foamMapA = this._FoamMapA;
			this._FoamMapA = this._FoamMapB;
			this._FoamMapB = foamMapA;
		}

		private RenderTexture[] GetDisplacementDeltaMaps()
		{
			if (this._DisplacementDeltaMaps == null)
			{
				this._DisplacementDeltaMaps = new RenderTexture[4];
				bool allowFloatingPointMipMaps = WaterProjectSettings.Instance.AllowFloatingPointMipMaps;
				for (int i = 0; i < 4; i++)
				{
					this._DisplacementDeltaMaps[i] = new RenderTexture(this._Resolution, this._Resolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
					{
						name = "[UWS] Foam - Displacement Delta Map [" + i + "]",
						useMipMap = allowFloatingPointMipMaps,
						autoGenerateMips = allowFloatingPointMipMaps,
						wrapMode = TextureWrapMode.Repeat,
						filterMode = ((!allowFloatingPointMipMaps) ? FilterMode.Bilinear : FilterMode.Trilinear)
					};
					this._Water.Renderer.PropertyBlock.SetTexture(ShaderVariables.DisplacementDeltaMaps[i], this._DisplacementDeltaMaps[i]);
				}
			}
			return this._DisplacementDeltaMaps;
		}

		private static void OnCameraDestroyed(WaterCamera waterCamera)
		{
			Foam._LayerUpdateFrames.Remove(waterCamera);
		}

		private readonly Water _Water;

		private readonly WindWaves _WindWaves;

		private readonly Foam.Data _Data;

		private float _FoamIntensity = 1f;

		private float _FoamThreshold = 1f;

		private float _FoamFadingFactor = 0.85f;

		private float _FoamShoreExtent;

		private bool _FoamIntensityOverriden;

		private Shader _LocalFoamSimulationShader;

		private Shader _GlobalFoamSimulationShader;

		private RenderTexture _FoamMapA;

		private RenderTexture _FoamMapB;

		private RenderTexture[] _DisplacementDeltaMaps;

		private int _Resolution;

		private bool _FirstFrame;

		private readonly DynamicWater _Overlays;

		private readonly Material _GlobalFoamSimulationMaterial;

		private static readonly Dictionary<WaterCamera, Foam.CameraRenderData> _LayerUpdateFrames = new Dictionary<WaterCamera, Foam.CameraRenderData>();

		[CompilerGenerated]
		private static Action<WaterCamera> <>f__mg$cache0;

		[Serializable]
		public class Data
		{
			[Tooltip("Foam map supersampling in relation to the waves simulator resolution. Has to be a power of two (0.25, 0.5, 1, 2, etc.)")]
			public float Supersampling = 1f;
		}

		private class CameraRenderData
		{
			public readonly int[] RenderFramePerLayer = new int[32];

			public Vector2 LastSurfaceOffset;
		}
	}
}
