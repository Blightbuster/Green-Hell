using System;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Events;

namespace UltimateWater
{
	public sealed class WavesRendererFFT
	{
		public WavesRendererFFT(Water water, WindWaves windWaves, WavesRendererFFT.Data data)
		{
			this._Water = water;
			this._WindWaves = windWaves;
			this._Data = data;
			if (windWaves.LoopDuration != 0f)
			{
				this._NormalMapsCache = new RenderTexture[data.CachedFrameCount][];
				this._DisplacementMapsCache = new RenderTexture[data.CachedFrameCount][];
				this._IsCachedFrameValid = new bool[data.CachedFrameCount];
				water.ProfilesManager.Changed.AddListener(new UnityAction<Water>(this.OnProfilesChanged));
			}
			this.Validate();
		}

		public WavesRendererFFT.MapType RenderedMaps
		{
			get
			{
				return this._RenderedMaps;
			}
			set
			{
				this._RenderedMaps = value;
				if (this.Enabled && Application.isPlaying)
				{
					this.Dispose(false);
					this.ValidateResources();
				}
			}
		}

		public bool Enabled { get; private set; }

		public RenderTexture[] NormalMaps
		{
			get
			{
				return this._NormalMaps;
			}
		}

		public event Action ResourcesChanged;

		public void OnCopyModeChanged()
		{
			this._CopyModeDirty = true;
			if (this._LastCopyFrom != null)
			{
				this._LastCopyFrom.WindWaves.WaterWavesFFT.ResourcesChanged -= this.ValidateResources;
			}
			if (this._WindWaves.CopyFrom != null)
			{
				this._WindWaves.CopyFrom.WindWaves.WaterWavesFFT.ResourcesChanged += this.ValidateResources;
			}
			this._LastCopyFrom = this._WindWaves.CopyFrom;
			this.Dispose(false);
		}

		public Texture GetDisplacementMap(int index)
		{
			return (this._DisplacementMaps == null) ? null : this._DisplacementMaps[index];
		}

		public Texture GetNormalMap(int index)
		{
			return this._NormalMaps[index];
		}

		public void OnWaterRender(Camera camera)
		{
			if (this._FFTUtilitiesMaterial == null)
			{
				return;
			}
			this.ValidateWaveMaps();
		}

		private void RenderSpectra(float time, out Texture heightSpectrum, out Texture normalSpectrum, out Texture displacementSpectrum)
		{
			if (this._RenderedMaps == WavesRendererFFT.MapType.Normal)
			{
				heightSpectrum = null;
				displacementSpectrum = null;
				normalSpectrum = this._WindWaves.SpectrumResolver.RenderNormalsSpectrumAt(time);
			}
			else if ((this._RenderedMaps & WavesRendererFFT.MapType.Normal) == (WavesRendererFFT.MapType)0 || !this._FinalHighQualityNormalMaps)
			{
				normalSpectrum = null;
				this._WindWaves.SpectrumResolver.RenderDisplacementsSpectraAt(time, out heightSpectrum, out displacementSpectrum);
			}
			else
			{
				this._WindWaves.SpectrumResolver.RenderCompleteSpectraAt(time, out heightSpectrum, out normalSpectrum, out displacementSpectrum);
			}
		}

		private void RenderMaps(float time, RenderTexture[] displacementMaps, RenderTexture[] normalMaps)
		{
			Texture tex;
			Texture tex2;
			Texture tex3;
			this.RenderSpectra(time, out tex, out tex2, out tex3);
			if ((this._RenderedMaps & WavesRendererFFT.MapType.Displacement) != (WavesRendererFFT.MapType)0)
			{
				TemporaryRenderTexture temporary = this._SingleTargetCache.GetTemporary();
				TemporaryRenderTexture temporary2 = this._DoubleTargetCache.GetTemporary();
				this._HeightFFT.ComputeFFT(tex, temporary);
				this._DisplacementFFT.ComputeFFT(tex3, temporary2);
				this._FFTUtilitiesMaterial.SetTexture(ShaderVariables.HeightTex, temporary);
				this._FFTUtilitiesMaterial.SetTexture(ShaderVariables.DisplacementTex, temporary2);
				this._FFTUtilitiesMaterial.SetFloat(ShaderVariables.HorizontalDisplacementScale, this._Water.Materials.HorizontalDisplacementScale);
				for (int i = 0; i < 4; i++)
				{
					this._FFTUtilitiesMaterial.SetFloat(ShaderVariables.JacobianScale, this._Water.Materials.HorizontalDisplacementScale * 0.1f * (float)displacementMaps[i].width / this._WindWaves.TileSizes[i]);
					this._FFTUtilitiesMaterial.SetVector(ShaderVariables.Offset, WavesRendererFFT._Offsets[i]);
					Graphics.Blit(null, displacementMaps[i], this._FFTUtilitiesMaterial, 1);
				}
				temporary.Dispose();
				temporary2.Dispose();
			}
			if ((this._RenderedMaps & WavesRendererFFT.MapType.Normal) != (WavesRendererFFT.MapType)0)
			{
				if (!this._FinalHighQualityNormalMaps)
				{
					for (int j = 0; j < 2; j++)
					{
						int finalResolution = this._WindWaves.FinalResolution;
						this._FFTUtilitiesMaterial.SetFloat("_Intensity1", 0.58f * (float)finalResolution / this._WindWaves.TileSizes[j * 2]);
						this._FFTUtilitiesMaterial.SetFloat("_Intensity2", 0.58f * (float)finalResolution / this._WindWaves.TileSizes[j * 2 + 1]);
						this._FFTUtilitiesMaterial.SetTexture("_MainTex", displacementMaps[j << 1]);
						this._FFTUtilitiesMaterial.SetTexture("_SecondTex", displacementMaps[(j << 1) + 1]);
						this._FFTUtilitiesMaterial.SetFloat("_MainTex_Texel_Size", 1f / (float)displacementMaps[j << 1].width);
						Graphics.Blit(null, normalMaps[j], this._FFTUtilitiesMaterial, 0);
					}
				}
				else
				{
					TemporaryRenderTexture temporary3 = this._DoubleTargetCache.GetTemporary();
					this._NormalFFT.ComputeFFT(tex2, temporary3);
					for (int k = 0; k < 2; k++)
					{
						this._FFTUtilitiesMaterial.SetVector(ShaderVariables.Offset, WavesRendererFFT._OffsetsDual[k]);
						Graphics.Blit(temporary3, normalMaps[k], this._FFTUtilitiesMaterial, 3);
					}
					temporary3.Dispose();
				}
			}
		}

		private void RetrieveCachedFrame(int frameIndex, out RenderTexture[] displacementMaps, out RenderTexture[] normalMaps)
		{
			float time = (float)frameIndex / (float)this._Data.CachedFrameCount * this._WindWaves.LoopDuration;
			if (!this._IsCachedFrameValid[frameIndex])
			{
				this._IsCachedFrameValid[frameIndex] = true;
				if ((this._RenderedMaps & WavesRendererFFT.MapType.Displacement) != (WavesRendererFFT.MapType)0 && this._DisplacementMapsCache[frameIndex] == null)
				{
					this.CreateRenderTextures(ref this._DisplacementMapsCache[frameIndex], "[UWS] WavesRendererFFT - Water Displacement Map", RenderTextureFormat.ARGBHalf, 4, true);
				}
				if ((this._RenderedMaps & WavesRendererFFT.MapType.Normal) != (WavesRendererFFT.MapType)0 && this._NormalMapsCache[frameIndex] == null)
				{
					this.CreateRenderTextures(ref this._NormalMapsCache[frameIndex], "[UWS] WavesRendererFFT - Water Normal Map", RenderTextureFormat.ARGBHalf, 2, true);
				}
				this.RenderMaps(time, this._DisplacementMapsCache[frameIndex], this._NormalMapsCache[frameIndex]);
			}
			displacementMaps = this._DisplacementMapsCache[frameIndex];
			normalMaps = this._NormalMapsCache[frameIndex];
		}

		private void RenderMapsFromCache(float time, RenderTexture[] displacementMaps, RenderTexture[] normalMaps)
		{
			float num = (float)this._Data.CachedFrameCount * FastMath.FracAdditive(time / this._WindWaves.LoopDuration);
			int num2 = (int)num;
			float value = num - (float)num2;
			RenderTexture[] array;
			RenderTexture[] array2;
			this.RetrieveCachedFrame(num2, out array, out array2);
			int num3 = num2 + 1;
			if (num3 >= this._Data.CachedFrameCount)
			{
				num3 = 0;
			}
			RenderTexture[] array3;
			RenderTexture[] array4;
			this.RetrieveCachedFrame(num3, out array3, out array4);
			this._FFTUtilitiesMaterial.SetFloat("_BlendFactor", value);
			for (int i = 0; i < 4; i++)
			{
				this._FFTUtilitiesMaterial.SetTexture("_Texture1", array[i]);
				this._FFTUtilitiesMaterial.SetTexture("_Texture2", array3[i]);
				Graphics.Blit(null, displacementMaps[i], this._FFTUtilitiesMaterial, 6);
			}
			for (int j = 0; j < 2; j++)
			{
				this._FFTUtilitiesMaterial.SetTexture("_Texture1", array2[j]);
				this._FFTUtilitiesMaterial.SetTexture("_Texture2", array4[j]);
				Graphics.Blit(null, normalMaps[j], this._FFTUtilitiesMaterial, 6);
			}
		}

		private void ValidateWaveMaps()
		{
			int frameCount = Time.frameCount;
			if (this._WaveMapsFrame == frameCount || !Application.isPlaying)
			{
				return;
			}
			if (this._LastCopyFrom != null)
			{
				if (this._CopyModeDirty)
				{
					this._CopyModeDirty = false;
					this.ValidateResources();
				}
				return;
			}
			this._WaveMapsFrame = frameCount;
			if (this._WindWaves.LoopDuration == 0f)
			{
				this.RenderMaps(this._Water.Time, this._DisplacementMaps, this._NormalMaps);
			}
			else
			{
				this.RenderMapsFromCache(this._Water.Time, this._DisplacementMaps, this._NormalMaps);
			}
		}

		private void OnResolutionChanged(WindWaves windWaves)
		{
			this.Dispose(false);
			this.ValidateResources();
		}

		private void Dispose(bool total)
		{
			this._WaveMapsFrame = -1;
			if (this._HeightFFT != null)
			{
				this._HeightFFT.Dispose();
				this._HeightFFT = null;
			}
			if (this._NormalFFT != null)
			{
				this._NormalFFT.Dispose();
				this._NormalFFT = null;
			}
			if (this._DisplacementFFT != null)
			{
				this._DisplacementFFT.Dispose();
				this._DisplacementFFT = null;
			}
			if (this._NormalMaps != null)
			{
				foreach (RenderTexture obj in this._NormalMaps)
				{
					obj.Destroy();
				}
				this._NormalMaps = null;
			}
			if (this._DisplacementMaps != null)
			{
				foreach (RenderTexture obj2 in this._DisplacementMaps)
				{
					obj2.Destroy();
				}
				this._DisplacementMaps = null;
			}
			if (this._NormalMapsCache != null)
			{
				for (int k = this._NormalMapsCache.Length - 1; k >= 0; k--)
				{
					RenderTexture[] array = this._NormalMapsCache[k];
					if (array != null)
					{
						for (int l = array.Length - 1; l >= 0; l--)
						{
							array[l].Destroy();
						}
						this._NormalMapsCache[k] = null;
					}
				}
			}
			if (this._DisplacementMapsCache != null)
			{
				for (int m = this._DisplacementMapsCache.Length - 1; m >= 0; m--)
				{
					RenderTexture[] array2 = this._DisplacementMapsCache[m];
					if (array2 != null)
					{
						for (int n = array2.Length - 1; n >= 0; n--)
						{
							array2[n].Destroy();
						}
						this._DisplacementMapsCache[m] = null;
					}
				}
			}
			if (this._IsCachedFrameValid != null)
			{
				for (int num = this._IsCachedFrameValid.Length - 1; num >= 0; num--)
				{
					this._IsCachedFrameValid[num] = false;
				}
			}
			if (total && this._FFTUtilitiesMaterial != null)
			{
				this._FFTUtilitiesMaterial.Destroy();
				this._FFTUtilitiesMaterial = null;
			}
		}

		private void OnProfilesChanged(Water water)
		{
			for (int i = this._IsCachedFrameValid.Length - 1; i >= 0; i--)
			{
				this._IsCachedFrameValid[i] = false;
			}
		}

		internal void Validate()
		{
			this._Dx11FFT = this._Water.ShaderSet.GetComputeShader("DX11 FFT");
			if (this._FFTShader == null)
			{
				this._FFTShader = Shader.Find("UltimateWater/Base/FFT");
			}
			if (this._FFTUtilitiesShader == null)
			{
				this._FFTUtilitiesShader = Shader.Find("UltimateWater/Utilities/FFT Utilities");
			}
			if (Application.isPlaying && this.Enabled)
			{
				this.ResolveFinalSettings(WaterQualitySettings.Instance.CurrentQualityLevel);
			}
		}

		internal void ResolveFinalSettings(WaterQualityLevel qualityLevel)
		{
			this._FinalHighQualityNormalMaps = this._Data.HighQualityNormalMaps;
			if (!qualityLevel.AllowHighQualityNormalMaps)
			{
				this._FinalHighQualityNormalMaps = false;
			}
			if ((this._RenderedMaps & WavesRendererFFT.MapType.Displacement) == (WavesRendererFFT.MapType)0)
			{
				this._FinalHighQualityNormalMaps = true;
			}
		}

		private GpuFFT ChooseBestFFTAlgorithm(bool twoChannels)
		{
			int finalResolution = this._WindWaves.FinalResolution;
			GpuFFT gpuFFT;
			if (!this._Data.ForcePixelShader && this._Dx11FFT != null && SystemInfo.supportsComputeShaders && finalResolution <= 512)
			{
				gpuFFT = new Dx11FFT(this._Dx11FFT, finalResolution, this._WindWaves.FinalHighPrecision || finalResolution >= 2048, twoChannels);
			}
			else
			{
				gpuFFT = new PixelShaderFFT(this._FFTShader, finalResolution, this._WindWaves.FinalHighPrecision || finalResolution >= 2048, twoChannels);
			}
			gpuFFT.SetupMaterials();
			return gpuFFT;
		}

		private void ValidateFFT(ref GpuFFT fft, bool present, bool twoChannels)
		{
			if (present)
			{
				if (fft == null)
				{
					fft = this.ChooseBestFFTAlgorithm(twoChannels);
				}
			}
			else if (fft != null)
			{
				fft.Dispose();
				fft = null;
			}
		}

		private RenderTexture CreateRenderTexture(string name, RenderTextureFormat format, bool mipMaps)
		{
			RenderTexture renderTexture = new RenderTexture(this._WindWaves.FinalResolution, this._WindWaves.FinalResolution, 0, format, RenderTextureReadWrite.Linear)
			{
				name = name,
				hideFlags = HideFlags.DontSave,
				wrapMode = TextureWrapMode.Repeat
			};
			if (mipMaps && WaterProjectSettings.Instance.AllowFloatingPointMipMaps)
			{
				renderTexture.filterMode = FilterMode.Trilinear;
				renderTexture.useMipMap = true;
				renderTexture.autoGenerateMips = true;
			}
			else
			{
				renderTexture.filterMode = FilterMode.Bilinear;
			}
			return renderTexture;
		}

		private void CreateRenderTextures(ref RenderTexture[] renderTextures, string name, RenderTextureFormat format, int count, bool mipMaps)
		{
			renderTextures = new RenderTexture[count];
			for (int i = 0; i < count; i++)
			{
				renderTextures[i] = this.CreateRenderTexture(name, format, mipMaps);
			}
		}

		private void ValidateResources()
		{
			if (this._WindWaves.CopyFrom == null)
			{
				this.ValidateFFT(ref this._HeightFFT, (this._RenderedMaps & WavesRendererFFT.MapType.Displacement) != (WavesRendererFFT.MapType)0, false);
				this.ValidateFFT(ref this._DisplacementFFT, (this._RenderedMaps & WavesRendererFFT.MapType.Displacement) != (WavesRendererFFT.MapType)0, true);
				this.ValidateFFT(ref this._NormalFFT, (this._RenderedMaps & WavesRendererFFT.MapType.Normal) != (WavesRendererFFT.MapType)0, true);
			}
			if (this._DisplacementMaps == null || this._NormalMaps == null)
			{
				RenderTexture[] displacementMaps;
				RenderTexture[] normalMaps;
				if (this._WindWaves.CopyFrom == null)
				{
					int finalResolution = this._WindWaves.FinalResolution;
					int num = finalResolution << 1;
					this._SingleTargetCache = RenderTexturesCache.GetCache(num, num, 0, RenderTextureFormat.RHalf, true, this._HeightFFT is Dx11FFT, false);
					this._DoubleTargetCache = RenderTexturesCache.GetCache(num, num, 0, RenderTextureFormat.RGHalf, true, this._DisplacementFFT is Dx11FFT, false);
					if (this._DisplacementMaps == null && (this._RenderedMaps & WavesRendererFFT.MapType.Displacement) != (WavesRendererFFT.MapType)0)
					{
						this.CreateRenderTextures(ref this._DisplacementMaps, "[UWS] WavesRendererFFT - Water Displacement Map", RenderTextureFormat.ARGBHalf, 4, true);
					}
					if (this._NormalMaps == null && (this._RenderedMaps & WavesRendererFFT.MapType.Normal) != (WavesRendererFFT.MapType)0)
					{
						this.CreateRenderTextures(ref this._NormalMaps, "[UWS] WavesRendererFFT - Water Normal Map", RenderTextureFormat.ARGBHalf, 2, true);
					}
					displacementMaps = this._DisplacementMaps;
					normalMaps = this._NormalMaps;
				}
				else
				{
					Water copyFrom = this._WindWaves.CopyFrom;
					if (copyFrom.WindWaves.WaterWavesFFT._WindWaves == null)
					{
						copyFrom.WindWaves.ResolveFinalSettings(WaterQualitySettings.Instance.CurrentQualityLevel);
					}
					copyFrom.WindWaves.WaterWavesFFT.ValidateResources();
					displacementMaps = copyFrom.WindWaves.WaterWavesFFT._DisplacementMaps;
					normalMaps = copyFrom.WindWaves.WaterWavesFFT._NormalMaps;
				}
				for (int i = 0; i < 4; i++)
				{
					string str = (i == 0) ? string.Empty : i.ToString();
					if (displacementMaps != null)
					{
						string name = "_GlobalDisplacementMap" + str;
						this._Water.Renderer.PropertyBlock.SetTexture(name, displacementMaps[i]);
					}
					if (i < 2 && normalMaps != null)
					{
						string name2 = "_GlobalNormalMap" + str;
						this._Water.Renderer.PropertyBlock.SetTexture(name2, normalMaps[i]);
					}
				}
				if (this.ResourcesChanged != null)
				{
					this.ResourcesChanged();
				}
			}
		}

		internal void Enable()
		{
			if (this.Enabled)
			{
				return;
			}
			this.Enabled = true;
			this.OnCopyModeChanged();
			if (Application.isPlaying)
			{
				if (this._LastCopyFrom == null)
				{
					this.ValidateResources();
				}
				this._WindWaves.ResolutionChanged.AddListener(new UnityAction<WindWaves>(this.OnResolutionChanged));
			}
			this._FFTUtilitiesMaterial = new Material(this._FFTUtilitiesShader)
			{
				hideFlags = HideFlags.DontSave
			};
		}

		internal void Disable()
		{
			if (!this.Enabled)
			{
				return;
			}
			this.Enabled = false;
			this.Dispose(false);
		}

		internal void OnDestroy()
		{
			this.Dispose(true);
		}

		private Shader _FFTShader;

		private Shader _FFTUtilitiesShader;

		private readonly Water _Water;

		private readonly WindWaves _WindWaves;

		private readonly WavesRendererFFT.Data _Data;

		private readonly RenderTexture[][] _NormalMapsCache;

		private readonly RenderTexture[][] _DisplacementMapsCache;

		private readonly bool[] _IsCachedFrameValid;

		private RenderTexture[] _NormalMaps;

		private RenderTexture[] _DisplacementMaps;

		private RenderTexturesCache _SingleTargetCache;

		private RenderTexturesCache _DoubleTargetCache;

		private GpuFFT _HeightFFT;

		private GpuFFT _NormalFFT;

		private GpuFFT _DisplacementFFT;

		private Material _FFTUtilitiesMaterial;

		private ComputeShader _Dx11FFT;

		private WavesRendererFFT.MapType _RenderedMaps;

		private bool _FinalHighQualityNormalMaps;

		private bool _CopyModeDirty;

		private int _WaveMapsFrame;

		private Water _LastCopyFrom;

		private static readonly Vector4[] _Offsets = new Vector4[]
		{
			new Vector4(0f, 0f, 0f, 0f),
			new Vector4(0.5f, 0f, 0f, 0f),
			new Vector4(0f, 0.5f, 0f, 0f),
			new Vector4(0.5f, 0.5f, 0f, 0f)
		};

		private static readonly Vector4[] _OffsetsDual = new Vector4[]
		{
			new Vector4(0f, 0f, 0.5f, 0f),
			new Vector4(0f, 0.5f, 0.5f, 0.5f)
		};

		[Serializable]
		public sealed class Data
		{
			[Tooltip("Determines if GPU partial derivatives or Fast Fourier Transform (high quality) should be used to compute normal map (Recommended: on). Works only if displacement map rendering is enabled.")]
			public bool HighQualityNormalMaps = true;

			[Tooltip("Check this option, if your water is flat or game crashes instantly on a DX11 GPU (in editor or build). Compute shaders are very fast, so use this as a last resort.")]
			public bool ForcePixelShader;

			[Tooltip("Fixes crest artifacts during storms, but lowers overall quality. Enabled by default when used with additive water volumes as it is actually needed and disabled in all other cases.")]
			public WavesRendererFFT.FlattenMode FlattenMode;

			[Tooltip("Sea state will be cached in the specified frame count for extra performance, if LoopLength on WindWaves is set to a value greater than zero.")]
			public int CachedFrameCount = 180;
		}

		public enum SpectrumType
		{
			Phillips,
			Unified
		}

		[Flags]
		public enum MapType
		{
			Displacement = 1,
			Normal = 2
		}

		public enum FlattenMode
		{
			Auto,
			ForcedOn,
			ForcedOff
		}
	}
}
