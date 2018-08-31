using System;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace UltimateWater
{
	[Serializable]
	public class DynamicSmoothness
	{
		public DynamicSmoothness(Water water, WindWaves windWaves)
		{
			this._Water = water;
			this._WindWaves = windWaves;
			this._Supported = DynamicSmoothness.CheckSupport();
			this._VarianceShader = water.ShaderSet.GetComputeShader("Spectral Variances");
			this.OnCopyModeChanged();
		}

		public bool Enabled
		{
			get
			{
				return this._Water.ShaderSet.SmoothnessMode == DynamicSmoothnessMode.Physical;
			}
		}

		public Texture VarianceTexture
		{
			get
			{
				return this._VarianceTexture;
			}
		}

		public ComputeShader ComputeShader
		{
			get
			{
				return this._VarianceShader;
			}
			set
			{
				this._VarianceShader = value;
			}
		}

		public void FreeResources()
		{
			if (this._VarianceTexture != null)
			{
				this._VarianceTexture.Destroy();
				this._VarianceTexture = null;
			}
		}

		public void OnCopyModeChanged()
		{
			if (this._WindWaves == null || this._WindWaves.CopyFrom == null)
			{
				return;
			}
			this._WindWaves.CopyFrom.ForceStartup();
			this.FreeResources();
			WindWaves windWaves = this._WindWaves.CopyFrom.WindWaves;
			windWaves.DynamicSmoothness.ValidateVarianceTextures();
			this._Water.Renderer.PropertyBlock.SetTexture("_SlopeVariance", windWaves.DynamicSmoothness._VarianceTexture);
		}

		public static bool CheckSupport()
		{
			RenderTextureFormat? format = Compatibility.GetFormat(RenderTextureFormat.RGHalf, new RenderTextureFormat[]
			{
				RenderTextureFormat.RGFloat
			});
			if (!SystemInfo.supportsComputeShaders || !SystemInfo.supports3DTextures || format == null)
			{
				WaterLogger.Warning("Dynamic Smoothness", "Check Support", "Dynamic Smoothness not supported");
				if (!SystemInfo.supportsComputeShaders)
				{
					WaterLogger.Warning("Dynamic Smoothness", "Check Support", " - compute shaders not supported");
				}
				if (!SystemInfo.supports3DTextures)
				{
					WaterLogger.Warning("Dynamic Smoothness", "Check Support", " - 3D textures not supported");
				}
				if (format == null)
				{
					WaterLogger.Warning("Dynamic Smoothness", "Check Support", " - necessary RenderTexture formats not found");
				}
				return false;
			}
			DynamicSmoothness._Format = format.Value;
			return true;
		}

		public void Update()
		{
			if (this._Water.ShaderSet.SmoothnessMode != DynamicSmoothnessMode.Physical || !this._Supported)
			{
				return;
			}
			if (!this._Initialized)
			{
				this.InitializeVariance();
			}
			this.ValidateVarianceTextures();
			if (!this._Finished)
			{
				this.RenderNextPixel();
			}
		}

		private void InitializeVariance()
		{
			this._Initialized = true;
			this._Water.ProfilesManager.Changed.AddListener(new UnityAction<Water>(this.OnProfilesChanged));
			this._WindWaves.WindDirectionChanged.AddListener(new UnityAction<WindWaves>(this.OnWindDirectionChanged));
			this.OnProfilesChanged(this._Water);
		}

		private void ValidateVarianceTextures()
		{
			if (this._VarianceTexture == null)
			{
				this._VarianceTexture = DynamicSmoothness.CreateVarianceTexture(DynamicSmoothness._Format);
				this.ResetComputations();
			}
			if (!this._VarianceTexture.IsCreated())
			{
				this._VarianceTexture.Create();
				this._Water.Renderer.PropertyBlock.SetTexture("_SlopeVariance", this._VarianceTexture);
				this._LastResetIndex = 0;
				this._CurrentIndex = 0;
			}
		}

		private void RenderNextPixel()
		{
			this._VarianceShader.SetInt("_FFTSize", this._WindWaves.FinalResolution);
			this._VarianceShader.SetInt("_FFTSizeHalf", this._WindWaves.FinalResolution >> 1);
			this._VarianceShader.SetFloat("_VariancesSize", (float)this._VarianceTexture.width);
			this._VarianceShader.SetFloat("_IntensityScale", this._DynamicSmoothnessIntensity);
			this._VarianceShader.SetVector("_TileSizes", this._WindWaves.TileSizes);
			this._VarianceShader.SetVector("_Coordinates", new Vector4((float)(this._CurrentIndex % 4), (float)((this._CurrentIndex >> 2) % 4), (float)(this._CurrentIndex >> 4), 0f));
			this._VarianceShader.SetTexture(0, "_Spectrum", this._WindWaves.SpectrumResolver.GetRawDirectionalSpectrum());
			this._VarianceShader.SetTexture(0, "_Variance", this._VarianceTexture);
			this._VarianceShader.Dispatch(0, 1, 1, 1);
			this._CurrentIndex++;
			if (this._CurrentIndex >= 64)
			{
				this._CurrentIndex = 0;
			}
			if (this._CurrentIndex == this._LastResetIndex)
			{
				this._Finished = true;
			}
		}

		private void ResetComputations()
		{
			this._LastResetIndex = this._CurrentIndex;
			this._Finished = false;
		}

		private void OnProfilesChanged(Water water)
		{
			this.ResetComputations();
			this._DynamicSmoothnessIntensity = 0f;
			Water.WeightedProfile[] profiles = water.ProfilesManager.Profiles;
			for (int i = profiles.Length - 1; i >= 0; i--)
			{
				this._DynamicSmoothnessIntensity += profiles[i].Profile.DynamicSmoothnessIntensity * profiles[i].Weight;
			}
		}

		private void OnWindDirectionChanged(WindWaves windWaves)
		{
			this.ResetComputations();
		}

		private static RenderTexture CreateVarianceTexture(RenderTextureFormat format)
		{
			return new RenderTexture(4, 4, 0, format, RenderTextureReadWrite.Linear)
			{
				name = "[UWS] DynamicSmoothness - Variance Tex",
				hideFlags = HideFlags.DontSave,
				volumeDepth = 4,
				enableRandomWrite = true,
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Bilinear,
				autoGenerateMips = false,
				useMipMap = false,
				dimension = TextureDimension.Tex3D
			};
		}

		private readonly Water _Water;

		private readonly WindWaves _WindWaves;

		private readonly bool _Supported;

		private static RenderTextureFormat _Format;

		private ComputeShader _VarianceShader;

		private RenderTexture _VarianceTexture;

		private int _LastResetIndex;

		private int _CurrentIndex;

		private bool _Finished;

		private bool _Initialized;

		private float _DynamicSmoothnessIntensity;
	}
}
