using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace UltimateWater
{
	[Serializable]
	public class ShaderSet : ScriptableObject
	{
		public WaterTransparencyMode TransparencyMode
		{
			get
			{
				return this._TransparencyMode;
			}
			set
			{
				this._TransparencyMode = value;
			}
		}

		public ReflectionProbeUsage ReflectionProbeUsage
		{
			get
			{
				return this._ReflectionProbeUsage;
			}
			set
			{
				this._ReflectionProbeUsage = value;
			}
		}

		public bool ReceiveShadows
		{
			get
			{
				return this._ReceiveShadows;
			}
			set
			{
				this._ReceiveShadows = value;
			}
		}

		public PlanarReflectionsMode PlanarReflections
		{
			get
			{
				return this._PlanarReflections;
			}
			set
			{
				this._PlanarReflections = value;
			}
		}

		public WindWavesRenderMode WindWavesMode
		{
			get
			{
				return this._WindWavesMode;
			}
			set
			{
				this._WindWavesMode = value;
			}
		}

		public Shader[] SurfaceShaders
		{
			get
			{
				return this._SurfaceShaders;
			}
		}

		public Shader[] VolumeShaders
		{
			get
			{
				return this._VolumeShaders;
			}
		}

		public bool LocalEffectsSupported
		{
			get
			{
				return this._LocalEffectsSupported;
			}
		}

		public bool Foam
		{
			get
			{
				return this._Foam;
			}
		}

		public bool LocalEffectsDebug
		{
			get
			{
				return this._LocalEffectsDebug;
			}
		}

		public bool CustomTriangularGeometry
		{
			get
			{
				return this._CustomTriangularGeometry;
			}
		}

		public bool ProjectionGrid
		{
			get
			{
				return this._ProjectionGrid;
			}
		}

		public DynamicSmoothnessMode SmoothnessMode
		{
			get
			{
				return this._DynamicSmoothnessMode;
			}
		}

		public static Shader GetRuntimeShaderVariant(string keywordsString, bool volume)
		{
			Shader shader = Shader.Find("UltimateWater/Variations/Water " + ((!volume) ? string.Empty : "Volume ") + keywordsString);
			if (shader == null && !ShaderSet._ErrorDisplayed && Application.isPlaying)
			{
				Debug.LogError("Could not find proper water shader variation. Select your water and click \"Rebuild shaders\" from its context menu to build proper shaders. Missing shader: \"UltimateWater/Variations/Water " + ((!volume) ? string.Empty : "Volume ") + keywordsString + "\"");
				ShaderSet._ErrorDisplayed = true;
			}
			return shader;
		}

		public Shader GetShaderVariant(string[] localKeywords, string[] sharedKeywords, string additionalCode, string keywordsString, bool volume)
		{
			Array.Sort<string>(localKeywords);
			Array.Sort<string>(sharedKeywords);
			string str = ((!volume) ? string.Empty : "Volume ") + keywordsString;
			return Shader.Find("UltimateWater/Variations/Water " + str);
		}

		public void FindBestShaders(out Shader surfaceShader, out Shader volumeShader)
		{
			ShaderVariant shaderVariant = new ShaderVariant();
			this.BuildShaderVariant(shaderVariant, WaterQualitySettings.Instance.CurrentQualityLevel);
			string[] array = shaderVariant.GetKeywordsString().Split(new char[]
			{
				' '
			});
			surfaceShader = null;
			volumeShader = null;
			if (this._SurfaceShaders != null)
			{
				for (int i = 0; i < this._SurfaceShaders.Length; i++)
				{
					if (!(this._SurfaceShaders[i] == null))
					{
						string name = this._SurfaceShaders[i].name;
						for (int j = 0; j < array.Length; j++)
						{
							if (name.Contains(array[j]))
							{
								surfaceShader = this._SurfaceShaders[i];
								break;
							}
						}
						if (surfaceShader != null)
						{
							break;
						}
					}
				}
			}
			if (this._VolumeShaders != null)
			{
				for (int k = 0; k < this._VolumeShaders.Length; k++)
				{
					if (!(this._VolumeShaders[k] == null))
					{
						string name2 = this._VolumeShaders[k].name;
						for (int l = 0; l < array.Length; l++)
						{
							if (name2.Contains(array[l]))
							{
								volumeShader = this._VolumeShaders[k];
								break;
							}
						}
						if (volumeShader != null)
						{
							break;
						}
					}
				}
			}
		}

		[ContextMenu("Rebuild shaders")]
		public void Build()
		{
		}

		public bool ContainsShaderVariant(string keywordsString)
		{
			if (this._SurfaceShaders != null)
			{
				for (int i = this._SurfaceShaders.Length - 1; i >= 0; i--)
				{
					Shader shader = this._SurfaceShaders[i];
					if (shader != null && shader.name.EndsWith(keywordsString))
					{
						return true;
					}
				}
			}
			if (this._VolumeShaders != null)
			{
				for (int j = this._VolumeShaders.Length - 1; j >= 0; j--)
				{
					Shader shader2 = this._VolumeShaders[j];
					if (shader2 != null && shader2.name.EndsWith(keywordsString))
					{
						return true;
					}
				}
			}
			return false;
		}

		public ComputeShader GetComputeShader(string shaderName)
		{
			for (int i = 0; i < this._ComputeShaders.Length; i++)
			{
				if (this._ComputeShaders[i].name.Contains(shaderName))
				{
					return this._ComputeShaders[i];
				}
			}
			return null;
		}

		private static void ValidateWaterObjects()
		{
			Water[] array = UnityEngine.Object.FindObjectsOfType<Water>();
			for (int i = array.Length - 1; i >= 0; i--)
			{
				array[i].ResetWater();
			}
		}

		private static void SetProgress(float progress)
		{
		}

		private void AddShader(Shader shader, bool volumeShader)
		{
			if (volumeShader)
			{
				if (this._VolumeShaders != null)
				{
					Array.Resize<Shader>(ref this._VolumeShaders, this._VolumeShaders.Length + 1);
					this._VolumeShaders[this._VolumeShaders.Length - 1] = shader;
				}
				else
				{
					this._VolumeShaders = new Shader[]
					{
						shader
					};
				}
			}
			else if (this._SurfaceShaders != null)
			{
				Array.Resize<Shader>(ref this._SurfaceShaders, this._SurfaceShaders.Length + 1);
				this._SurfaceShaders[this._SurfaceShaders.Length - 1] = shader;
			}
			else
			{
				this._SurfaceShaders = new Shader[]
				{
					shader
				};
			}
		}

		private void BuildShaderVariant(ShaderVariant variant, WaterQualityLevel qualityLevel)
		{
			bool flag = this._TransparencyMode == WaterTransparencyMode.Refractive && qualityLevel.AllowAlphaBlending;
			variant.SetWaterKeyword("_WATER_REFRACTION", flag);
			variant.SetWaterKeyword("_CUBEMAP_REFLECTIONS", this._ReflectionProbeUsage != ReflectionProbeUsage.Off);
			variant.SetWaterKeyword("_WATER_RECEIVE_SHADOWS", this._ReceiveShadows);
			variant.SetWaterKeyword("_ALPHABLEND_ON", flag);
			variant.SetWaterKeyword("_ALPHAPREMULTIPLY_ON", !flag);
			variant.SetUnityKeyword("_TRIANGLES", this._CustomTriangularGeometry);
			if (this._ProjectionGrid)
			{
				variant.SetAdditionalSurfaceCode("_PROJECTION_GRID", "\t\t\t#pragma multi_compile _PROJECTION_GRID_OFF _PROJECTION_GRID");
			}
			variant.SetUnityKeyword("_WATER_OVERLAYS", this._LocalEffectsSupported);
			variant.SetUnityKeyword("_LOCAL_MAPS_DEBUG", this._LocalEffectsSupported && this._LocalEffectsDebug);
			WindWavesRenderMode windWavesRenderMode = this.BuildWindWavesVariant(variant, qualityLevel);
			variant.SetWaterKeyword("_WATER_FOAM_WS", this._Foam && !this._LocalEffectsSupported && windWavesRenderMode == WindWavesRenderMode.FullFFT);
			variant.SetUnityKeyword("_BOUNDED_WATER", this._DisplayOnlyInAdditiveVolumes);
			variant.SetUnityKeyword("_WAVES_ALIGN", this._WavesAlign);
			variant.SetWaterKeyword("_NORMALMAP", this._NormalMappingMode == NormalMappingMode.Always || (this._NormalMappingMode == NormalMappingMode.Auto && windWavesRenderMode > WindWavesRenderMode.GerstnerAndFFTNormals));
			variant.SetWaterKeyword("_EMISSION", this._SupportEmission);
			variant.SetWaterKeyword("_PLANAR_REFLECTIONS", this._PlanarReflections == PlanarReflectionsMode.Normal);
			variant.SetWaterKeyword("_PLANAR_REFLECTIONS_HQ", this._PlanarReflections == PlanarReflectionsMode.HighQuality);
		}

		private WindWavesRenderMode BuildWindWavesVariant(ShaderVariant variant, WaterQualityLevel qualityLevel)
		{
			WaterWavesMode wavesMode = qualityLevel.WavesMode;
			WindWavesRenderMode windWavesRenderMode;
			if (this._WindWavesMode == WindWavesRenderMode.Disabled || wavesMode == WaterWavesMode.DisallowAll)
			{
				windWavesRenderMode = WindWavesRenderMode.Disabled;
			}
			else if (this._WindWavesMode == WindWavesRenderMode.FullFFT && wavesMode == WaterWavesMode.AllowAll)
			{
				windWavesRenderMode = WindWavesRenderMode.FullFFT;
			}
			else if (this._WindWavesMode <= WindWavesRenderMode.GerstnerAndFFTNormals && wavesMode <= WaterWavesMode.AllowNormalFFT)
			{
				windWavesRenderMode = WindWavesRenderMode.GerstnerAndFFTNormals;
			}
			else
			{
				windWavesRenderMode = WindWavesRenderMode.Gerstner;
			}
			if (windWavesRenderMode != WindWavesRenderMode.FullFFT)
			{
				if (windWavesRenderMode != WindWavesRenderMode.GerstnerAndFFTNormals)
				{
					if (windWavesRenderMode == WindWavesRenderMode.Gerstner)
					{
						variant.SetUnityKeyword("_WAVES_GERSTNER", true);
					}
				}
				else
				{
					variant.SetWaterKeyword("_WAVES_FFT_NORMAL", true);
					variant.SetUnityKeyword("_WAVES_GERSTNER", true);
				}
			}
			else
			{
				variant.SetUnityKeyword("_WAVES_FFT", true);
			}
			if (this._DynamicSmoothnessMode == DynamicSmoothnessMode.Physical)
			{
				variant.SetWaterKeyword("_INCLUDE_SLOPE_VARIANCE", true);
			}
			return windWavesRenderMode;
		}

		[FormerlySerializedAs("transparencyMode")]
		[Header("Reflection & Refraction")]
		[SerializeField]
		private WaterTransparencyMode _TransparencyMode = WaterTransparencyMode.Refractive;

		[FormerlySerializedAs("reflectionProbeUsage")]
		[SerializeField]
		private ReflectionProbeUsage _ReflectionProbeUsage = ReflectionProbeUsage.BlendProbesAndSkybox;

		[FormerlySerializedAs("planarReflections")]
		[SerializeField]
		private PlanarReflectionsMode _PlanarReflections = PlanarReflectionsMode.Normal;

		[FormerlySerializedAs("receiveShadows")]
		[SerializeField]
		[Tooltip("Affects direct light specular and diffuse components. Shadows currently work only for main directional light and you need to attach WaterShadowCastingLight script to it. Also it doesn't work at all on mobile platforms.")]
		private bool _ReceiveShadows;

		[FormerlySerializedAs("windWavesMode")]
		[SerializeField]
		[Header("Waves")]
		private WindWavesRenderMode _WindWavesMode;

		[FormerlySerializedAs("dynamicSmoothnessMode")]
		[SerializeField]
		private DynamicSmoothnessMode _DynamicSmoothnessMode = DynamicSmoothnessMode.Physical;

		[FormerlySerializedAs("localEffectsSupported")]
		[SerializeField]
		private bool _LocalEffectsSupported = true;

		[FormerlySerializedAs("localEffectsDebug")]
		[SerializeField]
		private bool _LocalEffectsDebug;

		[FormerlySerializedAs("foam")]
		[SerializeField]
		private bool _Foam = true;

		[FormerlySerializedAs("forwardRenderMode")]
		[SerializeField]
		[Header("Render Modes")]
		private bool _ForwardRenderMode;

		[FormerlySerializedAs("deferredRenderMode")]
		[SerializeField]
		private bool _DeferredRenderMode;

		[FormerlySerializedAs("projectionGrid")]
		[SerializeField]
		[Header("Geometries Support")]
		private bool _ProjectionGrid;

		[FormerlySerializedAs("customTriangularGeometry")]
		[SerializeField]
		private bool _CustomTriangularGeometry;

		[Header("Volumes")]
		[SerializeField]
		[FormerlySerializedAs("displayOnlyInAdditiveVolumes")]
		private bool _DisplayOnlyInAdditiveVolumes;

		[FormerlySerializedAs("wavesAlign")]
		[SerializeField]
		private bool _WavesAlign;

		[Header("Surface")]
		[SerializeField]
		[FormerlySerializedAs("normalMappingMode")]
		private NormalMappingMode _NormalMappingMode = NormalMappingMode.Auto;

		[FormerlySerializedAs("supportEmission")]
		[SerializeField]
		private bool _SupportEmission;

		[SerializeField]
		[FormerlySerializedAs("surfaceShaders")]
		[Header("Generated Shaders")]
		private Shader[] _SurfaceShaders;

		[FormerlySerializedAs("volumeShaders")]
		[SerializeField]
		private Shader[] _VolumeShaders;

		[FormerlySerializedAs("utilityShaders")]
		[SerializeField]
		private Shader[] _UtilityShaders;

		[FormerlySerializedAs("computeShaders")]
		[SerializeField]
		private ComputeShader[] _ComputeShaders;

		private bool _Rebuilding;

		private static bool _ErrorDisplayed;
	}
}
