using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	[Serializable]
	public class WaterProfileData
	{
		public WaterProfileData Template
		{
			get
			{
				return this._Template.Data;
			}
		}

		public WaterProfile TemplateProfile
		{
			set
			{
				this._Template = value;
			}
		}

		public bool IsTemplate
		{
			get
			{
				return this._Template == null;
			}
		}

		public WaterProfileData.WaterSpectrumType SpectrumType
		{
			get
			{
				return this._SpectrumType;
			}
		}

		public WaterWavesSpectrum Spectrum
		{
			get
			{
				if (this._Spectrum == null)
				{
					this.CreateSpectrum();
				}
				return this._Spectrum;
			}
		}

		public float WindSpeed
		{
			get
			{
				return this._WindSpeed;
			}
		}

		public float TileSize
		{
			get
			{
				return this._TileSize;
			}
		}

		public float TileScale
		{
			get
			{
				return this._TileScale;
			}
		}

		public float HorizontalDisplacementScale
		{
			get
			{
				return this._HorizontalDisplacementScale;
			}
			set
			{
				this._HorizontalDisplacementScale = value;
				this.Dirty = true;
			}
		}

		public float Gravity
		{
			get
			{
				return this._Gravity;
			}
			set
			{
				this._Gravity = value;
				this.Dirty = true;
			}
		}

		public float Directionality
		{
			get
			{
				return this._Directionality;
			}
			set
			{
				this._Directionality = value;
				this.Dirty = true;
			}
		}

		public Color AbsorptionColor
		{
			get
			{
				return this._AbsorptionColor;
			}
			set
			{
				this._AbsorptionColor = value;
				this.Dirty = true;
			}
		}

		public Color DiffuseColor
		{
			get
			{
				return this._DiffuseColor;
			}
			set
			{
				this._DiffuseColor = value;
				this.Dirty = true;
			}
		}

		public Color SpecularColor
		{
			get
			{
				return this._SpecularColor;
			}
			set
			{
				this._SpecularColor = value;
				this.Dirty = true;
			}
		}

		public Color DepthColor
		{
			get
			{
				return this._DepthColor;
			}
			set
			{
				this._DepthColor = value;
				this.Dirty = true;
			}
		}

		public Color EmissionColor
		{
			get
			{
				return this._EmissionColor;
			}
			set
			{
				this._EmissionColor = value;
				this.Dirty = true;
			}
		}

		public Color ReflectionColor
		{
			get
			{
				return this._ReflectionColor;
			}
			set
			{
				this._ReflectionColor = value;
				this.Dirty = true;
			}
		}

		public float Smoothness
		{
			get
			{
				return this._Smoothness;
			}
			set
			{
				this._Smoothness = value;
				this.Dirty = true;
			}
		}

		public bool CustomAmbientSmoothness
		{
			get
			{
				return this._CustomAmbientSmoothness;
			}
			set
			{
				this._CustomAmbientSmoothness = value;
				this.Dirty = true;
			}
		}

		public float AmbientSmoothness
		{
			get
			{
				return (!this._CustomAmbientSmoothness) ? this._Smoothness : this._AmbientSmoothness;
			}
			set
			{
				this._AmbientSmoothness = value;
				this.Dirty = true;
			}
		}

		public float IsotropicScatteringIntensity
		{
			get
			{
				return this._IsotropicScatteringIntensity;
			}
			set
			{
				this._IsotropicScatteringIntensity = value;
				this.Dirty = true;
			}
		}

		public float ForwardScatteringIntensity
		{
			get
			{
				return this._ForwardScatteringIntensity;
			}
			set
			{
				this._ForwardScatteringIntensity = value;
				this.Dirty = true;
			}
		}

		public float SubsurfaceScatteringContrast
		{
			get
			{
				return this._SubsurfaceScatteringContrast;
			}
			set
			{
				this._SubsurfaceScatteringContrast = value;
				this.Dirty = true;
			}
		}

		public Color SubsurfaceScatteringShoreColor
		{
			get
			{
				return this._SubsurfaceScatteringShoreColor;
			}
			set
			{
				this._SubsurfaceScatteringShoreColor = value;
				this.Dirty = true;
			}
		}

		public float RefractionDistortion
		{
			get
			{
				return this._RefractionDistortion;
			}
			set
			{
				this._RefractionDistortion = value;
				this.Dirty = true;
			}
		}

		public float FresnelBias
		{
			get
			{
				return this._FresnelBias;
			}
			set
			{
				this._FresnelBias = value;
				this.Dirty = true;
			}
		}

		public float DetailFadeDistance
		{
			get
			{
				return this._DetailFadeDistance * this._DetailFadeDistance;
			}
			set
			{
				this._DetailFadeDistance = value;
				this.Dirty = true;
			}
		}

		public float DisplacementNormalsIntensity
		{
			get
			{
				return this._DisplacementNormalsIntensity;
			}
			set
			{
				this._DisplacementNormalsIntensity = value;
				this.Dirty = true;
			}
		}

		public float PlanarReflectionIntensity
		{
			get
			{
				return this._PlanarReflectionIntensity;
			}
			set
			{
				this._PlanarReflectionIntensity = value;
				this.Dirty = true;
			}
		}

		public float PlanarReflectionFlatten
		{
			get
			{
				return this._PlanarReflectionFlatten;
			}
			set
			{
				this._PlanarReflectionFlatten = value;
				this.Dirty = true;
			}
		}

		public float PlanarReflectionVerticalOffset
		{
			get
			{
				return this._PlanarReflectionVerticalOffset;
			}
			set
			{
				this._WindSpeed = value;
				this.Dirty = true;
			}
		}

		public float EdgeBlendFactor
		{
			get
			{
				return this._EdgeBlendFactor;
			}
			set
			{
				this._EdgeBlendFactor = value;
				this.Dirty = true;
			}
		}

		public float DirectionalWrapSSS
		{
			get
			{
				return this._DirectionalWrapSss;
			}
			set
			{
				this._DirectionalWrapSss = value;
				this.Dirty = true;
			}
		}

		public float PointWrapSSS
		{
			get
			{
				return this._PointWrapSss;
			}
			set
			{
				this._PointWrapSss = value;
				this.Dirty = true;
			}
		}

		public float DynamicSmoothnessIntensity
		{
			get
			{
				return this._DynamicSmoothnessIntensity;
			}
			set
			{
				this._DynamicSmoothnessIntensity = value;
				this.Dirty = true;
			}
		}

		public float Density
		{
			get
			{
				return this._Density;
			}
			set
			{
				this._Density = value;
				this.Dirty = true;
			}
		}

		public float UnderwaterBlurSize
		{
			get
			{
				return this._UnderwaterBlurSize;
			}
			set
			{
				this._UnderwaterBlurSize = value;
				this.Dirty = true;
			}
		}

		public float UnderwaterLightFadeScale
		{
			get
			{
				return this._UnderwaterLightFadeScale;
			}
			set
			{
				this._UnderwaterLightFadeScale = value;
				this.Dirty = true;
			}
		}

		public float UnderwaterDistortionsIntensity
		{
			get
			{
				return this._UnderwaterDistortionsIntensity;
			}
			set
			{
				this._UnderwaterDistortionsIntensity = value;
				this.Dirty = true;
			}
		}

		public float UnderwaterDistortionAnimationSpeed
		{
			get
			{
				return this._UnderwaterDistortionAnimationSpeed;
			}
			set
			{
				this._UnderwaterDistortionAnimationSpeed = value;
				this.Dirty = true;
			}
		}

		public NormalMapAnimation NormalMapAnimation1
		{
			get
			{
				return this._NormalMapAnimation1;
			}
			set
			{
				this._NormalMapAnimation1 = value;
				this.Dirty = true;
			}
		}

		public NormalMapAnimation NormalMapAnimation2
		{
			get
			{
				return this._NormalMapAnimation2;
			}
			set
			{
				this._NormalMapAnimation2 = value;
				this.Dirty = true;
			}
		}

		public float FoamIntensity
		{
			get
			{
				return this._FoamIntensity;
			}
			set
			{
				this._FoamIntensity = value;
				this.Dirty = true;
			}
		}

		public float FoamThreshold
		{
			get
			{
				return this._FoamThreshold;
			}
			set
			{
				this._FoamThreshold = value;
				this.Dirty = true;
			}
		}

		public float FoamFadingFactor
		{
			get
			{
				return this._FoamFadingFactor;
			}
			set
			{
				this._FoamFadingFactor = value;
				this.Dirty = true;
			}
		}

		public float FoamShoreIntensity
		{
			get
			{
				return this._FoamShoreIntensity;
			}
			set
			{
				this._FoamShoreIntensity = value;
				this.Dirty = true;
			}
		}

		public float FoamShoreExtent
		{
			get
			{
				return this._FoamShoreExtent;
			}
			set
			{
				this._FoamShoreExtent = value;
				this.Dirty = true;
			}
		}

		public float FoamNormalScale
		{
			get
			{
				return this._FoamNormalScale;
			}
			set
			{
				this._FoamNormalScale = value;
				this.Dirty = true;
			}
		}

		public Color FoamDiffuseColor
		{
			get
			{
				return this._FoamDiffuseColor;
			}
			set
			{
				this._FoamDiffuseColor = value;
				this.Dirty = true;
			}
		}

		public Color FoamSpecularColor
		{
			get
			{
				return this._FoamSpecularColor;
			}
			set
			{
				this._FoamSpecularColor = value;
				this.Dirty = true;
			}
		}

		public float SprayThreshold
		{
			get
			{
				return this._SprayThreshold;
			}
			set
			{
				this._SprayThreshold = value;
				this.Dirty = true;
			}
		}

		public float SpraySkipRatio
		{
			get
			{
				return this._SpraySkipRatio;
			}
			set
			{
				this._SpraySkipRatio = value;
				this.Dirty = true;
			}
		}

		public float SpraySize
		{
			get
			{
				return this._SpraySize;
			}
			set
			{
				this._SpraySize = value;
				this.Dirty = true;
			}
		}

		public Texture2D NormalMap
		{
			get
			{
				return this._NormalMap;
			}
			set
			{
				this._NormalMap = value;
				this.Dirty = true;
			}
		}

		public Texture2D FoamDiffuseMap
		{
			get
			{
				return this._FoamDiffuseMap;
			}
			set
			{
				this._FoamDiffuseMap = value;
				this.Dirty = true;
			}
		}

		public Texture2D FoamNormalMap
		{
			get
			{
				return this._FoamNormalMap;
			}
			set
			{
				this._FoamNormalMap = value;
				this.Dirty = true;
			}
		}

		public Vector2 FoamTiling
		{
			get
			{
				return this._FoamTiling;
			}
			set
			{
				this._FoamTiling = value;
				this.Dirty = true;
			}
		}

		public float WavesFrequencyScale
		{
			get
			{
				return this._WavesFrequencyScale;
			}
			set
			{
				this._WavesFrequencyScale = value;
				this.Dirty = true;
			}
		}

		public Gradient AbsorptionColorByDepth
		{
			get
			{
				return (!this._CustomUnderwaterAbsorptionColor) ? this._AbsorptionColorByDepthFlatGradient : this._AbsorptionColorByDepth;
			}
			set
			{
				this._AbsorptionColorByDepth = value;
				this.Dirty = true;
			}
		}

		public void Synchronize()
		{
			this.Copy(this.Template);
		}

		public void Copy(WaterProfileData other)
		{
			this._Spectrum = other._Spectrum;
			this._SpectrumType = other._SpectrumType;
			this._WindSpeed = other._WindSpeed;
			this._TileSize = other._TileSize;
			this._TileScale = other._TileScale;
			this._WavesAmplitude = other._WavesAmplitude;
			this._WavesFrequencyScale = other._WavesFrequencyScale;
			this._HorizontalDisplacementScale = other._HorizontalDisplacementScale;
			this._PhillipsCutoffFactor = other._PhillipsCutoffFactor;
			this._Gravity = other._Gravity;
			this._Fetch = other._Fetch;
			this._Directionality = other._Directionality;
			this._AbsorptionColor = other._AbsorptionColor;
			this._CustomUnderwaterAbsorptionColor = other._CustomUnderwaterAbsorptionColor;
			this._AbsorptionColorByDepth = other._AbsorptionColorByDepth;
			this._AbsorptionColorByDepthFlatGradient = other._AbsorptionColorByDepthFlatGradient;
			this._DiffuseColor = other._DiffuseColor;
			this._SpecularColor = other._SpecularColor;
			this._DepthColor = other._DepthColor;
			this._EmissionColor = other._EmissionColor;
			this._ReflectionColor = other._ReflectionColor;
			this._Smoothness = other._Smoothness;
			this._CustomAmbientSmoothness = other._CustomAmbientSmoothness;
			this._AmbientSmoothness = other._AmbientSmoothness;
			this._IsotropicScatteringIntensity = other._IsotropicScatteringIntensity;
			this._ForwardScatteringIntensity = other._ForwardScatteringIntensity;
			this._SubsurfaceScatteringContrast = other._SubsurfaceScatteringContrast;
			this._SubsurfaceScatteringShoreColor = other._SubsurfaceScatteringShoreColor;
			this._RefractionDistortion = other._RefractionDistortion;
			this._FresnelBias = other._FresnelBias;
			this._DetailFadeDistance = other._DetailFadeDistance;
			this._DisplacementNormalsIntensity = other._DisplacementNormalsIntensity;
			this._PlanarReflectionIntensity = other._PlanarReflectionIntensity;
			this._PlanarReflectionFlatten = other._PlanarReflectionFlatten;
			this._PlanarReflectionVerticalOffset = other._PlanarReflectionVerticalOffset;
			this._EdgeBlendFactor = other._EdgeBlendFactor;
			this._DirectionalWrapSss = other._DirectionalWrapSss;
			this._PointWrapSss = other._PointWrapSss;
			this._Density = other._Density;
			this._UnderwaterBlurSize = other._UnderwaterBlurSize;
			this._UnderwaterLightFadeScale = other._UnderwaterLightFadeScale;
			this._UnderwaterDistortionsIntensity = other._UnderwaterDistortionsIntensity;
			this._UnderwaterDistortionAnimationSpeed = other._UnderwaterDistortionAnimationSpeed;
			this._DynamicSmoothnessIntensity = other._DynamicSmoothnessIntensity;
			this._NormalMapAnimation1 = other._NormalMapAnimation1;
			this._NormalMapAnimation2 = other._NormalMapAnimation2;
			this._NormalMap = other._NormalMap;
			this._FoamIntensity = other._FoamIntensity;
			this._FoamThreshold = other._FoamThreshold;
			this._FoamFadingFactor = other._FoamFadingFactor;
			this._FoamShoreIntensity = other._FoamShoreIntensity;
			this._FoamShoreExtent = other._FoamShoreExtent;
			this._FoamNormalScale = other._FoamNormalScale;
			this._FoamDiffuseColor = other._FoamDiffuseColor;
			this._FoamSpecularColor = other._FoamSpecularColor;
			this._SprayThreshold = other._SprayThreshold;
			this._SpraySkipRatio = other._SpraySkipRatio;
			this._SpraySize = other._SpraySize;
			this._FoamDiffuseMap = other._FoamDiffuseMap;
			this._FoamNormalMap = other._FoamNormalMap;
			this._FoamTiling = other._FoamTiling;
		}

		public void CreateSpectrum()
		{
			WaterProfileData.WaterSpectrumType spectrumType = this._SpectrumType;
			if (spectrumType != WaterProfileData.WaterSpectrumType.Unified)
			{
				if (spectrumType == WaterProfileData.WaterSpectrumType.Phillips)
				{
					this._Spectrum = new PhillipsSpectrum(this._TileSize, -this._Gravity, this._WindSpeed, this._WavesAmplitude, this._PhillipsCutoffFactor);
				}
			}
			else
			{
				this._Spectrum = new UnifiedSpectrum(this._TileSize, -this._Gravity, this._WindSpeed, this._WavesAmplitude, this._WavesFrequencyScale, this._Fetch);
			}
		}

		[SerializeField]
		[FormerlySerializedAs("spectrumType")]
		private WaterProfileData.WaterSpectrumType _SpectrumType = WaterProfileData.WaterSpectrumType.Unified;

		[SerializeField]
		[FormerlySerializedAs("windSpeed")]
		private float _WindSpeed = 22f;

		[Tooltip("Tile size in world units of all water maps including heightmap. High values lower overall quality, but low values make the water pattern noticeable.")]
		[SerializeField]
		[FormerlySerializedAs("tileSize")]
		private float _TileSize = 180f;

		[FormerlySerializedAs("tileScale")]
		[SerializeField]
		private float _TileScale = 1f;

		[Tooltip("Setting it to something else than 1.0 will make the spectrum less physically correct, but still may be useful at times.")]
		[FormerlySerializedAs("wavesAmplitude")]
		[SerializeField]
		private float _WavesAmplitude = 1f;

		[FormerlySerializedAs("wavesFrequencyScale")]
		[SerializeField]
		private float _WavesFrequencyScale = 1f;

		[FormerlySerializedAs("horizontalDisplacementScale")]
		[SerializeField]
		[Range(0f, 4f)]
		private float _HorizontalDisplacementScale = 1f;

		[FormerlySerializedAs("phillipsCutoffFactor")]
		[SerializeField]
		private float _PhillipsCutoffFactor = 2000f;

		[FormerlySerializedAs("gravity")]
		[SerializeField]
		private float _Gravity = -9.81f;

		[FormerlySerializedAs("fetch")]
		[SerializeField]
		[Tooltip("It is the length of water in meters over which a wind has blown. Usually a distance to the closest land in the direction opposite to the wind.")]
		private float _Fetch = 100000f;

		[Range(0f, 1f)]
		[Tooltip("Eliminates waves moving against the wind.")]
		[FormerlySerializedAs("directionality")]
		[SerializeField]
		private float _Directionality;

		[ColorUsage(false, true, 0f, 10f, 0f, 10f)]
		[SerializeField]
		[FormerlySerializedAs("absorptionColor")]
		private Color _AbsorptionColor = new Color(0.35f, 0.04f, 0.001f, 1f);

		[SerializeField]
		[FormerlySerializedAs("customUnderwaterAbsorptionColor")]
		private bool _CustomUnderwaterAbsorptionColor = true;

		[FormerlySerializedAs("absorptionColorByDepth")]
		[SerializeField]
		[Tooltip("Absorption color used by the underwater camera image-effect. Gradient describes color at each depth starting with 0 and ending on 600 units.")]
		private Gradient _AbsorptionColorByDepth;

		[SerializeField]
		[FormerlySerializedAs("absorptionColorByDepthFlatGradient")]
		private Gradient _AbsorptionColorByDepthFlatGradient;

		[ColorUsage(false, true, 0f, 10f, 0f, 10f)]
		[SerializeField]
		[FormerlySerializedAs("diffuseColor")]
		private Color _DiffuseColor = new Color(0.1176f, 0.2196f, 0.2666f);

		[SerializeField]
		[ColorUsage(false)]
		[FormerlySerializedAs("specularColor")]
		private Color _SpecularColor = new Color(0.0353f, 0.0471f, 0.0549f);

		[FormerlySerializedAs("depthColor")]
		[ColorUsage(false)]
		[SerializeField]
		private Color _DepthColor = new Color(0f, 0f, 0f);

		[SerializeField]
		[FormerlySerializedAs("emissionColor")]
		[ColorUsage(false)]
		private Color _EmissionColor = new Color(0f, 0f, 0f);

		[ColorUsage(false)]
		[FormerlySerializedAs("reflectionColor")]
		[SerializeField]
		private Color _ReflectionColor = new Color(1f, 1f, 1f);

		[FormerlySerializedAs("smoothness")]
		[Range(0f, 1f)]
		[SerializeField]
		private float _Smoothness = 0.94f;

		[FormerlySerializedAs("customAmbientSmoothness")]
		[SerializeField]
		private bool _CustomAmbientSmoothness;

		[Range(0f, 1f)]
		[SerializeField]
		[FormerlySerializedAs("ambientSmoothness")]
		private float _AmbientSmoothness = 0.94f;

		[FormerlySerializedAs("isotropicScatteringIntensity")]
		[Range(0f, 6f)]
		[SerializeField]
		private float _IsotropicScatteringIntensity = 1f;

		[SerializeField]
		[Range(0f, 6f)]
		[FormerlySerializedAs("forwardScatteringIntensity")]
		private float _ForwardScatteringIntensity = 1f;

		[FormerlySerializedAs("subsurfaceScatteringContrast")]
		[SerializeField]
		[Range(0f, 1f)]
		private float _SubsurfaceScatteringContrast;

		[FormerlySerializedAs("subsurfaceScatteringShoreColor")]
		[SerializeField]
		[ColorUsage(false, true, 1f, 8f, 1f, 8f)]
		private Color _SubsurfaceScatteringShoreColor = new Color(1.4f, 3f, 3f);

		[FormerlySerializedAs("refractionDistortion")]
		[SerializeField]
		[Range(0f, 1f)]
		private float _RefractionDistortion = 0.55f;

		[FormerlySerializedAs("fresnelBias")]
		[SerializeField]
		private float _FresnelBias = 0.02040781f;

		[Range(0.5f, 20f)]
		[FormerlySerializedAs("detailFadeDistance")]
		[SerializeField]
		private float _DetailFadeDistance = 4.5f;

		[FormerlySerializedAs("displacementNormalsIntensity")]
		[SerializeField]
		[Range(0.1f, 10f)]
		private float _DisplacementNormalsIntensity = 2f;

		[FormerlySerializedAs("planarReflectionIntensity")]
		[Tooltip("Planar reflections are very good solution for calm weather, but you should fade them out for profiles with big waves (storms etc.) as they get completely incorrect then.")]
		[Range(0f, 1f)]
		[SerializeField]
		private float _PlanarReflectionIntensity = 0.6f;

		[Range(1f, 10f)]
		[SerializeField]
		[FormerlySerializedAs("planarReflectionFlatten")]
		private float _PlanarReflectionFlatten = 6f;

		[FormerlySerializedAs("planarReflectionVerticalOffset")]
		[Range(0f, 0.008f)]
		[SerializeField]
		[Tooltip("Fixes some artifacts produced by planar reflections at grazing angles.")]
		private float _PlanarReflectionVerticalOffset = 0.0015f;

		[FormerlySerializedAs("edgeBlendFactor")]
		[SerializeField]
		private float _EdgeBlendFactor = 0.15f;

		[FormerlySerializedAs("directionalWrapSSS")]
		[SerializeField]
		private float _DirectionalWrapSss = 0.2f;

		[SerializeField]
		[FormerlySerializedAs("pointWrapSSS")]
		private float _PointWrapSss = 0.5f;

		[Tooltip("Used by the physics.")]
		[FormerlySerializedAs("density")]
		[SerializeField]
		private float _Density = 998.6f;

		[FormerlySerializedAs("underwaterBlurSize")]
		[SerializeField]
		[Range(0f, 0.03f)]
		private float _UnderwaterBlurSize = 0.003f;

		[Range(0f, 2f)]
		[FormerlySerializedAs("underwaterLightFadeScale")]
		[SerializeField]
		private float _UnderwaterLightFadeScale = 0.8f;

		[FormerlySerializedAs("underwaterDistortionsIntensity")]
		[SerializeField]
		[Range(0f, 0.4f)]
		private float _UnderwaterDistortionsIntensity = 0.05f;

		[FormerlySerializedAs("underwaterDistortionAnimationSpeed")]
		[SerializeField]
		[Range(0.02f, 0.5f)]
		private float _UnderwaterDistortionAnimationSpeed = 0.1f;

		[Range(1f, 64f)]
		[SerializeField]
		[FormerlySerializedAs("dynamicSmoothnessIntensity")]
		private float _DynamicSmoothnessIntensity = 1f;

		[SerializeField]
		[FormerlySerializedAs("normalMapAnimation1")]
		private NormalMapAnimation _NormalMapAnimation1 = new NormalMapAnimation(1f, -10f, 1f, new Vector2(1f, 1f));

		[FormerlySerializedAs("normalMapAnimation2")]
		[SerializeField]
		private NormalMapAnimation _NormalMapAnimation2 = new NormalMapAnimation(-0.55f, 20f, 0.74f, new Vector2(1.5f, 1.5f));

		[FormerlySerializedAs("normalMap")]
		[SerializeField]
		private Texture2D _NormalMap;

		[FormerlySerializedAs("foamIntensity")]
		[SerializeField]
		private float _FoamIntensity = 1f;

		[FormerlySerializedAs("foamThreshold")]
		[SerializeField]
		private float _FoamThreshold = 1f;

		[Tooltip("Determines how fast foam will fade out.")]
		[FormerlySerializedAs("foamFadingFactor")]
		[Range(0f, 1f)]
		[SerializeField]
		private float _FoamFadingFactor = 0.85f;

		[SerializeField]
		[FormerlySerializedAs("foamShoreIntensity")]
		[Range(0f, 5f)]
		private float _FoamShoreIntensity = 1f;

		[FormerlySerializedAs("foamShoreExtent")]
		[Range(0f, 5f)]
		[SerializeField]
		private float _FoamShoreExtent = 1f;

		[FormerlySerializedAs("foamNormalScale")]
		[SerializeField]
		private float _FoamNormalScale = 2.2f;

		[FormerlySerializedAs("foamDiffuseColor")]
		[SerializeField]
		[ColorUsage(false)]
		private Color _FoamDiffuseColor = new Color(0.8f, 0.8f, 0.8f);

		[FormerlySerializedAs("foamSpecularColor")]
		[SerializeField]
		[Tooltip("Alpha component is PBR smoothness.")]
		private Color _FoamSpecularColor = new Color(1f, 1f, 1f, 0f);

		[Range(0f, 4f)]
		[SerializeField]
		[FormerlySerializedAs("sprayThreshold")]
		private float _SprayThreshold = 1f;

		[Range(0f, 0.999f)]
		[FormerlySerializedAs("spraySkipRatio")]
		[SerializeField]
		private float _SpraySkipRatio = 0.9f;

		[FormerlySerializedAs("spraySize")]
		[SerializeField]
		[Range(0.25f, 4f)]
		private float _SpraySize = 1f;

		[SerializeField]
		[FormerlySerializedAs("foamDiffuseMap")]
		private Texture2D _FoamDiffuseMap;

		[FormerlySerializedAs("foamNormalMap")]
		[SerializeField]
		private Texture2D _FoamNormalMap;

		[SerializeField]
		[FormerlySerializedAs("foamTiling")]
		private Vector2 _FoamTiling = new Vector2(5.4f, 5.4f);

		[HideInInspector]
		[SerializeField]
		private WaterProfile _Template;

		private WaterWavesSpectrum _Spectrum;

		public bool Dirty;

		public enum WaterSpectrumType
		{
			Phillips,
			Unified
		}
	}
}
