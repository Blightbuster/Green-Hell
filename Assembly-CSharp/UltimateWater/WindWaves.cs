using System;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Events;

namespace UltimateWater
{
	public sealed class WindWaves : WaterModule
	{
		public WindWaves(Water water, WindWaves.Data data)
		{
			this._Water = water;
			this._Data = data;
			this._RuntimeCopyFrom = data.CopyFrom;
			this._IsClone = (this._RuntimeCopyFrom != null);
			this.CheckSupport();
			this.Validate();
			Shader spectrumShader = Shader.Find("UltimateWater/Spectrum/Water Spectrum");
			if (this._SpectrumResolver == null)
			{
				this._SpectrumResolver = new SpectrumResolver(water, this, spectrumShader);
			}
			if (data.WindDirectionChanged == null)
			{
				data.WindDirectionChanged = new WindWaves.WindWavesEvent();
			}
			this.CreateObjects();
			this.ResolveFinalSettings(WaterQualitySettings.Instance.CurrentQualityLevel);
			if (!Application.isPlaying)
			{
				return;
			}
			water.ProfilesManager.Changed.AddListener(new UnityAction<Water>(this.OnProfilesChanged));
			this.OnProfilesChanged(water);
		}

		public Water CopyFrom
		{
			get
			{
				return this._RuntimeCopyFrom;
			}
			set
			{
				if (this._Data.CopyFrom != value || this._RuntimeCopyFrom != value)
				{
					this._Data.CopyFrom = value;
					this._RuntimeCopyFrom = value;
					this._IsClone = (value != null);
					this._DynamicSmoothness.OnCopyModeChanged();
					this._WaterWavesFFT.OnCopyModeChanged();
				}
			}
		}

		public SpectrumResolver SpectrumResolver
		{
			get
			{
				return (!(this._Data.CopyFrom == null)) ? this._Data.CopyFrom.WindWaves._SpectrumResolver : this._SpectrumResolver;
			}
		}

		public WavesRendererFFT WaterWavesFFT
		{
			get
			{
				return this._WaterWavesFFT;
			}
		}

		public WavesRendererGerstner WaterWavesGerstner
		{
			get
			{
				return this._WaterWavesGerstner;
			}
		}

		public DynamicSmoothness DynamicSmoothness
		{
			get
			{
				return this._DynamicSmoothness;
			}
		}

		public WaveSpectrumRenderMode FinalRenderMode
		{
			get
			{
				return this._FinalRenderMode;
			}
		}

		public Vector4 TileSizes
		{
			get
			{
				return this._TileSizes;
			}
		}

		public Vector4 TileSizesInv
		{
			get
			{
				return this._TileSizesInv;
			}
		}

		public Vector4 UnscaledTileSizes
		{
			get
			{
				return this._UnscaledTileSizes;
			}
		}

		public Vector2 WindSpeed
		{
			get
			{
				return this._WindSpeed;
			}
		}

		public bool WindSpeedChanged
		{
			get
			{
				return this._WindSpeedChanged;
			}
		}

		public Vector2 WindDirection
		{
			get
			{
				return this._WindDirection;
			}
		}

		public Transform WindDirectionPointer
		{
			get
			{
				return this._Data.WindDirectionPointer;
			}
		}

		public WindWaves.WindWavesEvent WindDirectionChanged
		{
			get
			{
				return this._Data.WindDirectionChanged;
			}
		}

		public WindWaves.WindWavesEvent ResolutionChanged
		{
			get
			{
				return (this._Data.ResolutionChanged == null) ? (this._Data.ResolutionChanged = new WindWaves.WindWavesEvent()) : this._Data.ResolutionChanged;
			}
		}

		public int Resolution
		{
			get
			{
				return this._Data.Resolution;
			}
			set
			{
				if (this._Data.Resolution == value)
				{
					return;
				}
				this._Data.Resolution = value;
				this.ResolveFinalSettings(WaterQualitySettings.Instance.CurrentQualityLevel);
			}
		}

		public int FinalResolution
		{
			get
			{
				return this._FinalResolution;
			}
		}

		public bool FinalHighPrecision
		{
			get
			{
				return this._FinalHighPrecision;
			}
		}

		public bool HighPrecision
		{
			get
			{
				return this._Data.HighPrecision;
			}
		}

		public float CpuDesiredStandardError
		{
			get
			{
				return this._Data.CpuDesiredStandardError;
			}
		}

		public float LoopDuration
		{
			get
			{
				return this._Data.LoopDuration;
			}
		}

		public Vector4 TileSizeScales
		{
			get
			{
				return this._TileSizeScales;
			}
		}

		public float MaxVerticalDisplacement
		{
			get
			{
				return this._SpectrumResolver.MaxVerticalDisplacement;
			}
		}

		public float MaxHorizontalDisplacement
		{
			get
			{
				return this._SpectrumResolver.MaxHorizontalDisplacement;
			}
		}

		public float SpectrumDirectionality
		{
			get
			{
				return this._SpectrumDirectionality;
			}
		}

		public Vector2 GetHorizontalDisplacementAt(float x, float z, float time)
		{
			return this._SpectrumResolver.GetHorizontalDisplacementAt(x, z, time);
		}

		public float GetHeightAt(float x, float z, float time)
		{
			return this._SpectrumResolver.GetHeightAt(x, z, time);
		}

		public Vector4 GetForceAndHeightAt(float x, float z, float time)
		{
			return this._SpectrumResolver.GetForceAndHeightAt(x, z, time);
		}

		public Vector3 GetDisplacementAt(float x, float z, float time)
		{
			return this._SpectrumResolver.GetDisplacementAt(x, z, time);
		}

		internal override void Validate()
		{
			if (Application.isPlaying)
			{
				this.CopyFrom = this._Data.CopyFrom;
			}
			if (this._Data.CpuDesiredStandardError < 1E-05f)
			{
				this._Data.CpuDesiredStandardError = 1E-05f;
			}
			this._HasWindDirectionPointer = (this._Data.WindDirectionPointer != null);
			if (this._SpectrumResolver != null)
			{
				this.ResolveFinalSettings(WaterQualitySettings.Instance.CurrentQualityLevel);
				this._WaterWavesFFT.Validate();
				this._WaterWavesGerstner.OnValidate(this);
			}
			if (this._Water != null && this._TileSize != 0f)
			{
				this.UpdateShaderParams();
			}
			if (this._WaterWavesFFT != null && this._WaterWavesFFT.NormalMaps != null && this._WaterWavesFFT.NormalMaps.Length != 0)
			{
				this._WaterWavesFFT.GetNormalMap(0).mipMapBias = this._Data.MipBias;
				this._WaterWavesFFT.GetNormalMap(1).mipMapBias = this._Data.MipBias;
			}
		}

		internal override void Update()
		{
			this.UpdateWind();
			if (this._IsClone)
			{
				this._TileSize = this._RuntimeCopyFrom.WindWaves._TileSize;
				this.UpdateShaderParams();
				return;
			}
			if (!Application.isPlaying)
			{
				return;
			}
			this._SpectrumResolver.Update();
			this._DynamicSmoothness.Update();
			this.UpdateShaderParams();
		}

		internal void ResolveFinalSettings(WaterQualityLevel quality)
		{
			this.CreateObjects();
			WaterWavesMode wavesMode = quality.WavesMode;
			if (wavesMode == WaterWavesMode.DisallowAll)
			{
				this._WaterWavesFFT.Disable();
				this._WaterWavesGerstner.Disable();
				return;
			}
			bool flag = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat) || SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
			int num = Mathf.Min(new int[]
			{
				this._Data.Resolution,
				quality.MaxSpectrumResolution,
				SystemInfo.maxTextureSize
			});
			bool flag2 = this._Data.HighPrecision && quality.AllowHighPrecisionTextures && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat);
			WindWavesRenderMode windWavesMode = this._Water.ShaderSet.WindWavesMode;
			if (windWavesMode == WindWavesRenderMode.FullFFT && wavesMode == WaterWavesMode.AllowAll && flag)
			{
				this._FinalRenderMode = WaveSpectrumRenderMode.FullFFT;
			}
			else if (windWavesMode <= WindWavesRenderMode.GerstnerAndFFTNormals && wavesMode <= WaterWavesMode.AllowNormalFFT && flag)
			{
				this._FinalRenderMode = WaveSpectrumRenderMode.GerstnerAndFFTNormals;
			}
			else
			{
				this._FinalRenderMode = WaveSpectrumRenderMode.Gerstner;
			}
			if (this._FinalResolution != num)
			{
				lock (this)
				{
					this._FinalResolution = num;
					this._FinalHighPrecision = flag2;
					if (this._SpectrumResolver != null)
					{
						this._SpectrumResolver.OnMapsFormatChanged(true);
					}
					if (this.ResolutionChanged != null)
					{
						this.ResolutionChanged.Invoke(this);
					}
				}
			}
			else if (this._FinalHighPrecision != flag2)
			{
				lock (this)
				{
					this._FinalHighPrecision = flag2;
					if (this._SpectrumResolver != null)
					{
						this._SpectrumResolver.OnMapsFormatChanged(false);
					}
				}
			}
			WaveSpectrumRenderMode finalRenderMode = this._FinalRenderMode;
			if (finalRenderMode != WaveSpectrumRenderMode.FullFFT)
			{
				if (finalRenderMode != WaveSpectrumRenderMode.GerstnerAndFFTNormals)
				{
					if (finalRenderMode == WaveSpectrumRenderMode.Gerstner)
					{
						this._WaterWavesFFT.Disable();
						this._WaterWavesGerstner.Enable();
					}
				}
				else
				{
					this._WaterWavesFFT.RenderedMaps = WavesRendererFFT.MapType.Normal;
					this._WaterWavesFFT.Enable();
					this._WaterWavesGerstner.Enable();
				}
			}
			else
			{
				this._WaterWavesFFT.RenderedMaps = (WavesRendererFFT.MapType.Displacement | WavesRendererFFT.MapType.Normal);
				this._WaterWavesFFT.Enable();
				this._WaterWavesGerstner.Disable();
			}
		}

		private void UpdateShaderParams()
		{
			float uniformWaterScale = this._Water.UniformWaterScale;
			if (this._LastTileSize == this._TileSize && this._LastUniformWaterScale == uniformWaterScale)
			{
				return;
			}
			MaterialPropertyBlock propertyBlock = this._Water.Renderer.PropertyBlock;
			float num = this._TileSize * uniformWaterScale;
			this._TileSizes.x = num * this._TileSizeScales.x;
			this._TileSizes.y = num * this._TileSizeScales.y;
			this._TileSizes.z = num * this._TileSizeScales.z;
			this._TileSizes.w = num * this._TileSizeScales.w;
			propertyBlock.SetVector(ShaderVariables.WaterTileSize, this._TileSizes);
			this._TileSizesInv.x = 1f / this._TileSizes.x;
			this._TileSizesInv.y = 1f / this._TileSizes.y;
			this._TileSizesInv.z = 1f / this._TileSizes.z;
			this._TileSizesInv.w = 1f / this._TileSizes.w;
			propertyBlock.SetVector(ShaderVariables.WaterTileSizeInv, this._TileSizesInv);
			this._LastUniformWaterScale = uniformWaterScale;
			this._LastTileSize = this._TileSize;
		}

		private void OnProfilesChanged(Water water)
		{
			this._TileSize = 0f;
			this._WindSpeedMagnitude = 0f;
			this._SpectrumDirectionality = 0f;
			Water.WeightedProfile[] profiles = water.ProfilesManager.Profiles;
			for (int i = 0; i < profiles.Length; i++)
			{
				WaterProfileData profile = profiles[i].Profile;
				float weight = profiles[i].Weight;
				this._TileSize += profile.TileSize * profile.TileScale * weight;
				this._WindSpeedMagnitude += profile.WindSpeed * weight;
				this._SpectrumDirectionality += profile.Directionality * weight;
			}
			WaterQualitySettings instance = WaterQualitySettings.Instance;
			this._TileSize *= instance.TileSizeScale;
			this._UnscaledTileSizes = this._TileSize * this._TileSizeScales;
			this.UpdateShaderParams();
			MaterialPropertyBlock propertyBlock = water.Renderer.PropertyBlock;
			propertyBlock.SetVector(ShaderVariables.WaterTileSizeScales, new Vector4(this._TileSizeScales.x / this._TileSizeScales.y, this._TileSizeScales.x / this._TileSizeScales.z, this._TileSizeScales.x / this._TileSizeScales.w, 0f));
			this._SpectrumResolver.OnProfilesChanged();
			propertyBlock.SetFloat(ShaderVariables.MaxDisplacement, this._SpectrumResolver.MaxHorizontalDisplacement);
		}

		internal override void Destroy()
		{
			if (this._SpectrumResolver != null)
			{
				this._SpectrumResolver.OnDestroy();
				this._SpectrumResolver = null;
			}
			if (this._WaterWavesFFT != null)
			{
				this._WaterWavesFFT.OnDestroy();
			}
		}

		private void UpdateWind()
		{
			Vector2 windDirection;
			if (this._HasWindDirectionPointer)
			{
				Vector3 forward = this._Data.WindDirectionPointer.forward;
				float num = Mathf.Sqrt(forward.x * forward.x + forward.z * forward.z);
				windDirection = new Vector2(forward.x / num, forward.z / num);
			}
			else
			{
				windDirection = new Vector2(1f, 0f);
			}
			Vector2 windSpeed = new Vector2(windDirection.x * this._WindSpeedMagnitude, windDirection.y * this._WindSpeedMagnitude);
			if (this._WindSpeed.x != windSpeed.x || this._WindSpeed.y != windSpeed.y)
			{
				this._WindDirection = windDirection;
				this._WindSpeed = windSpeed;
				this._WindSpeedChanged = true;
				this._SpectrumResolver.SetWindDirection(this._WindDirection);
			}
			else
			{
				this._WindSpeedChanged = false;
			}
		}

		private void CreateObjects()
		{
			if (this._WaterWavesFFT == null)
			{
				this._WaterWavesFFT = new WavesRendererFFT(this._Water, this, this._Data.WavesRendererFFTData);
			}
			if (this._WaterWavesGerstner == null)
			{
				this._WaterWavesGerstner = new WavesRendererGerstner(this._Water, this, this._Data.WavesRendererGerstnerData);
			}
			if (this._DynamicSmoothness == null)
			{
				this._DynamicSmoothness = new DynamicSmoothness(this._Water, this);
			}
		}

		private void CheckSupport()
		{
			if (this._Data.HighPrecision && (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGFloat) || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat)))
			{
				this._FinalHighPrecision = false;
			}
			if (!this._Data.HighPrecision && (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf) || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf)))
			{
				if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGFloat))
				{
					this._FinalHighPrecision = true;
				}
				else if (this._Water.ShaderSet.WindWavesMode == WindWavesRenderMode.FullFFT)
				{
					this._FinalRenderMode = WaveSpectrumRenderMode.Gerstner;
				}
			}
		}

		internal override void OnWaterRender(WaterCamera waterCamera)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			Camera cameraComponent = waterCamera.CameraComponent;
			if (this._WaterWavesFFT.Enabled)
			{
				this._WaterWavesFFT.OnWaterRender(cameraComponent);
			}
			if (this._WaterWavesGerstner.Enabled)
			{
				this._WaterWavesGerstner.OnWaterRender(cameraComponent);
			}
		}

		internal override void Enable()
		{
			this.UpdateWind();
			this.ResolveFinalSettings(WaterQualitySettings.Instance.CurrentQualityLevel);
		}

		internal override void Disable()
		{
			if (this._WaterWavesFFT != null)
			{
				this._WaterWavesFFT.Disable();
			}
			if (this._WaterWavesGerstner != null)
			{
				this._WaterWavesGerstner.Disable();
			}
			if (this._DynamicSmoothness != null)
			{
				this._DynamicSmoothness.FreeResources();
			}
		}

		private readonly Water _Water;

		private readonly WindWaves.Data _Data;

		private Vector4 _TileSizeScales = new Vector4(0.79241f, 0.163151f, 3.175131f, 13.731513f);

		private int _FinalResolution;

		private bool _FinalHighPrecision;

		private float _WindSpeedMagnitude;

		private float _SpectrumDirectionality;

		private float _TileSize;

		private float _LastTileSize = float.NaN;

		private float _LastUniformWaterScale = float.NaN;

		private Vector4 _TileSizes;

		private Vector4 _TileSizesInv;

		private Vector4 _UnscaledTileSizes;

		private Vector2 _WindDirection;

		private Vector2 _WindSpeed;

		private WaveSpectrumRenderMode _FinalRenderMode;

		private SpectrumResolver _SpectrumResolver;

		private Water _RuntimeCopyFrom;

		private bool _IsClone;

		private bool _WindSpeedChanged;

		private bool _HasWindDirectionPointer;

		private WavesRendererFFT _WaterWavesFFT;

		private WavesRendererGerstner _WaterWavesGerstner;

		private DynamicSmoothness _DynamicSmoothness;

		[Serializable]
		public class WindWavesEvent : UnityEvent<WindWaves>
		{
		}

		[Serializable]
		public sealed class Data
		{
			public Transform WindDirectionPointer;

			[SerializeField]
			[Tooltip("Higher values increase quality, but also decrease performance. Directly controls quality of waves, foam and spray.")]
			public int Resolution = 256;

			[SerializeField]
			[Tooltip("Determines if 32-bit precision buffers should be used for computations (Default: off). Not supported on most mobile devices. This setting has impact on performance, even on PCs.\n\nTips:\n 1024 and higher - The difference is clearly visible, use this option on higher quality settings.\n 512 or lower - Keep it disabled.")]
			public bool HighPrecision = true;

			[Tooltip("What error in world units is acceptable for elevation sampling used by physics and custom scripts? Lower values mean better precision, but higher CPU usage.")]
			public float CpuDesiredStandardError = 0.12f;

			[Tooltip("Copying wave spectrum from other fluid will make this instance a lot faster.")]
			public Water CopyFrom;

			[Tooltip("Setting this property to any value greater than zero will loop the water spectra in that time. A good value is 10 seconds. Set to 0 to resolve sea state at each frame without any looping (best quality).")]
			public float LoopDuration;

			public WindWaves.WindWavesEvent WindDirectionChanged;

			public WindWaves.WindWavesEvent ResolutionChanged;

			public float MipBias;

			public WavesRendererFFT.Data WavesRendererFFTData;

			public WavesRendererGerstner.Data WavesRendererGerstnerData;
		}
	}
}
