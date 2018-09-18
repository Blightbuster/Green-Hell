using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UltimateWater
{
	[AddComponentMenu("Ultimate Water/Water (Base Component)", -1)]
	public sealed class Water : WaterCollider, ISerializationCallbackReceiver
	{
		public LayerMask CustomEffectsLayerMask
		{
			get
			{
				return this._DynamicWaterData.CustomEffectsLayerMask;
			}
			set
			{
				this._DynamicWaterData.CustomEffectsLayerMask = value;
			}
		}

		public ProfilesManager ProfilesManager
		{
			get
			{
				return this._ProfilesManager;
			}
		}

		public WaterMaterials Materials
		{
			get
			{
				return this._Materials;
			}
		}

		public WaterGeometry Geometry
		{
			get
			{
				return this._Geometry;
			}
		}

		public WaterRenderer Renderer
		{
			get
			{
				return this._WaterRenderer;
			}
		}

		public WaterVolume Volume
		{
			get
			{
				return this._Volume;
			}
			set
			{
				this._Volume = value;
			}
		}

		public WaterUvAnimator UVAnimator
		{
			get
			{
				return this._UVAnimator;
			}
		}

		public DynamicWater DynamicWater { get; private set; }

		public Foam Foam { get; private set; }

		public PlanarReflection PlanarReflection { get; private set; }

		public WindWaves WindWaves { get; private set; }

		public WaterSubsurfaceScattering SubsurfaceScattering
		{
			get
			{
				return this._SubsurfaceScattering;
			}
		}

		public ShaderSet ShaderSet
		{
			get
			{
				if (this._ShaderSet == null)
				{
					this._ShaderSet = (Resources.Load("Systems/Ultimate Water System/Shader Sets/Ocean (Fully featured)") as ShaderSet);
				}
				return this._ShaderSet;
			}
		}

		public bool RenderingEnabled
		{
			get
			{
				return this._RenderingEnabled;
			}
			set
			{
				if (this._RenderingEnabled == value)
				{
					return;
				}
				this._RenderingEnabled = value;
				if (this._RenderingEnabled)
				{
					if (base.enabled)
					{
						WaterSystem.Register(this);
					}
				}
				else
				{
					WaterSystem.Unregister(this);
				}
			}
		}

		public int ComputedSamplesCount { get; private set; }

		public float Density { get; private set; }

		public float Gravity { get; private set; }

		public float MaxHorizontalDisplacement { get; private set; }

		public float MaxVerticalDisplacement { get; private set; }

		public int Seed
		{
			get
			{
				return this._Seed;
			}
			set
			{
				this._Seed = value;
			}
		}

		public Vector2 SurfaceOffset
		{
			get
			{
				return (!float.IsNaN(this._SurfaceOffset.x)) ? this._SurfaceOffset : new Vector2(-base.transform.position.x, -base.transform.position.z);
			}
			set
			{
				this._SurfaceOffset = value;
			}
		}

		public float Time
		{
			get
			{
				return (this._Time != -1f) ? this._Time : UnityEngine.Time.time;
			}
			set
			{
				this._Time = value;
			}
		}

		public float UniformWaterScale
		{
			get
			{
				return base.transform.localScale.y;
			}
		}

		public int WaterId
		{
			get
			{
				return this._WaterId;
			}
		}

		public void ForceStartup()
		{
			this.CreateWaterComponents();
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			if (!Water._IsPlaying)
			{
				this._ComponentsCreated = false;
			}
		}

		public void ResetWater()
		{
			base.enabled = false;
			this.OnDestroy();
			this._ComponentsCreated = false;
			base.enabled = true;
		}

		public static Water CreateWater(string name, ShaderSet shaderCollection)
		{
			GameObject gameObject = new GameObject(name);
			Water water = gameObject.AddComponent<Water>();
			water._ShaderSet = shaderCollection;
			return water;
		}

		public static Water FindWater(Vector3 position, float radius)
		{
			bool flag;
			bool flag2;
			return Water.FindWater(position, radius, null, out flag, out flag2);
		}

		public static Water FindWater(Vector3 position, float radius, out bool isInsideSubtractiveVolume, out bool isInsideAdditiveVolume)
		{
			return Water.FindWater(position, radius, null, out isInsideSubtractiveVolume, out isInsideAdditiveVolume);
		}

		public static Water FindWater(Vector3 position, float radius, List<Water> allowedWaters, out bool isInsideSubtractiveVolume, out bool isInsideAdditiveVolume)
		{
			isInsideSubtractiveVolume = false;
			isInsideAdditiveVolume = false;
			int num = Physics.OverlapSphereNonAlloc(position, radius, Water._CollidersBuffer, 1 << WaterProjectSettings.Instance.WaterCollidersLayer, QueryTriggerInteraction.Collide);
			Water._PossibleWaters.Clear();
			Water._ExcludedWaters.Clear();
			for (int i = 0; i < num; i++)
			{
				WaterVolumeBase waterVolume = WaterVolumeBase.GetWaterVolume(Water._CollidersBuffer[i]);
				if (waterVolume != null)
				{
					if (waterVolume is WaterVolumeAdd)
					{
						isInsideAdditiveVolume = true;
						if (allowedWaters == null || allowedWaters.Contains(waterVolume.Water))
						{
							Water._PossibleWaters.Add(waterVolume.Water);
						}
					}
					else
					{
						isInsideSubtractiveVolume = true;
						Water._ExcludedWaters.Add(waterVolume.Water);
					}
				}
			}
			for (int j = 0; j < Water._PossibleWaters.Count; j++)
			{
				if (!Water._ExcludedWaters.Contains(Water._PossibleWaters[j]))
				{
					return Water._PossibleWaters[j];
				}
			}
			List<Water> boundlessWaters = ApplicationSingleton<WaterSystem>.Instance.BoundlessWaters;
			int count = boundlessWaters.Count;
			for (int k = 0; k < count; k++)
			{
				Water water = boundlessWaters[k];
				if ((allowedWaters == null || allowedWaters.Contains(water)) && water.Volume.IsPointInsideMainVolume(position, radius) && !Water._ExcludedWaters.Contains(water))
				{
					return water;
				}
			}
			return null;
		}

		private void Awake()
		{
			WaterQualitySettings.Instance.Changed -= this.OnQualitySettingsChanged;
			WaterQualitySettings.Instance.Changed += this.OnQualitySettingsChanged;
			this._Geometry.Awake(this);
			this._WaterRenderer.Awake(this);
			this._Materials.Awake(this);
			this._ProfilesManager.Awake(this);
		}

		private void OnEnable()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (this._FastEnableDisable && this._ComponentsCreated)
			{
				return;
			}
			Water._IsPlaying = Application.isPlaying;
			this.CreateWaterComponents();
			if (!this._ComponentsCreated)
			{
				return;
			}
			this._ProfilesManager.OnEnable();
			this._Geometry.OnEnable();
			this._Modules.ForEach(delegate(WaterModule x)
			{
				x.Enable();
			});
			if (this._RenderingEnabled)
			{
				WaterSystem.Register(this);
			}
		}

		private void OnDisable()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (this._FastEnableDisable)
			{
				return;
			}
			this._Modules.ForEach(delegate(WaterModule x)
			{
				x.Disable();
			});
			this._Geometry.OnDisable();
			this._ProfilesManager.OnDisable();
			WaterSystem.Unregister(this);
		}

		private void OnDestroy()
		{
			if (this._FastEnableDisable)
			{
				this._FastEnableDisable = false;
				this.OnDisable();
			}
			WaterQualitySettings.Instance.Changed -= this.OnQualitySettingsChanged;
			this._Modules.ForEach(delegate(WaterModule x)
			{
				x.Destroy();
			});
			this._Modules.Clear();
			this._Materials.OnDestroy();
			this._ProfilesManager.OnDestroy();
		}

		private void Update()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (!this._DontRotateUpwards)
			{
				base.transform.eulerAngles = new Vector3(0f, base.transform.eulerAngles.y, 0f);
			}
			this.UpdateStatisticalData();
			this._ProfilesManager.Update();
			this._Geometry.Update();
			for (int i = 0; i < this._Modules.Count; i++)
			{
				this._Modules[i].Update();
			}
		}

		public void OnValidate()
		{
			if (this._ComponentsCreated)
			{
				this._Modules.ForEach(delegate(WaterModule x)
				{
					x.Validate();
				});
				this._ProfilesManager.OnValidate();
				this._Materials.OnValidate();
				this._Geometry.OnValidate();
				this._WaterRenderer.OnValidate();
			}
		}

		public void OnDrawGizmos()
		{
			if (this._Geometry != null && this._Geometry.GeometryType == WaterGeometry.Type.CustomMeshes)
			{
				WaterCustomSurfaceMeshes customSurfaceMeshes = this._Geometry.CustomSurfaceMeshes;
				for (int i = 0; i < customSurfaceMeshes.Meshes.Length; i++)
				{
					Gizmos.matrix = base.transform.localToWorldMatrix;
					Gizmos.color = Color.cyan * 0.2f;
					Gizmos.DrawMesh(customSurfaceMeshes.Meshes[i]);
					Gizmos.color = Color.cyan * 0.4f;
					Gizmos.DrawWireMesh(customSurfaceMeshes.Meshes[i]);
				}
			}
		}

		internal int _WaterId
		{
			get
			{
				return this._WaterIdCache;
			}
			set
			{
				if (this._WaterIdCache == value)
				{
					return;
				}
				this._WaterIdCache = value;
				if (this.WaterIdChanged != null)
				{
					this.WaterIdChanged();
				}
			}
		}

		public event Action WaterIdChanged;

		internal void OnWaterRender(WaterCamera waterCamera)
		{
			if (!base.isActiveAndEnabled)
			{
				return;
			}
			this._Materials.OnWaterRender(waterCamera);
			for (int i = 0; i < this._Modules.Count; i++)
			{
				this._Modules[i].OnWaterRender(waterCamera);
			}
		}

		internal void OnWaterPostRender(WaterCamera waterCamera)
		{
			for (int i = 0; i < this._Modules.Count; i++)
			{
				this._Modules[i].OnWaterPostRender(waterCamera);
			}
		}

		internal void OnSamplingStarted()
		{
			this.ComputedSamplesCount++;
		}

		internal void OnSamplingStopped()
		{
			this.ComputedSamplesCount--;
		}

		private void CreateWaterComponents()
		{
			if (this._ComponentsCreated)
			{
				return;
			}
			this._ComponentsCreated = true;
			this._Modules.Clear();
			this._Modules.AddRange(new List<WaterModule>
			{
				this._UVAnimator,
				this._Volume,
				this._SubsurfaceScattering
			});
			for (int i = 0; i < this._Modules.Count; i++)
			{
				this._Modules[i].Start(this);
			}
			this._ProfilesManager.Changed.AddListener(new UnityAction<Water>(this.OnProfilesChanged));
			if (this._ShaderSet.LocalEffectsSupported)
			{
				this.DynamicWater = new DynamicWater(this, this._DynamicWaterData);
				this._Modules.Add(this.DynamicWater);
			}
			if (this._ShaderSet.PlanarReflections != PlanarReflectionsMode.Disabled)
			{
				this.PlanarReflection = new PlanarReflection(this, this._PlanarReflectionData);
				this._Modules.Add(this.PlanarReflection);
			}
			if (this._ShaderSet.WindWavesMode != WindWavesRenderMode.Disabled)
			{
				this.WindWaves = new WindWaves(this, this._WindWavesData);
				this._Modules.Add(this.WindWaves);
			}
			if (this._ShaderSet.Foam)
			{
				this.Foam = new Foam(this, this._FoamData);
				this._Modules.Add(this.Foam);
			}
		}

		internal void OnProfilesChanged(Water water)
		{
			Water.WeightedProfile[] profiles = water.ProfilesManager.Profiles;
			this.Density = 0f;
			this.Gravity = 0f;
			for (int i = 0; i < profiles.Length; i++)
			{
				WaterProfileData profile = profiles[i].Profile;
				float weight = profiles[i].Weight;
				this.Density += profile.Density * weight;
				this.Gravity -= profile.Gravity * weight;
			}
		}

		private void OnQualitySettingsChanged()
		{
			this.OnValidate();
		}

		private void UpdateStatisticalData()
		{
			this.MaxHorizontalDisplacement = 0f;
			this.MaxVerticalDisplacement = 0f;
			if (this.WindWaves != null)
			{
				this.MaxHorizontalDisplacement = this.WindWaves.MaxHorizontalDisplacement;
				this.MaxVerticalDisplacement = this.WindWaves.MaxVerticalDisplacement;
			}
		}

		public Vector3 GetDisplacementAt(float x, float z)
		{
			Vector3 result = default(Vector3);
			if (this.WindWaves != null)
			{
				this.CompensateHorizontalDisplacement(ref x, ref z, 0.045f);
				result = this.WindWaves.SpectrumResolver.GetDisplacementAt(x, z, this._Time);
			}
			return result;
		}

		public Vector3 GetDisplacementAt(float x, float z, float time)
		{
			Vector3 result = default(Vector3);
			if (this.WindWaves != null)
			{
				this.CompensateHorizontalDisplacement(ref x, ref z, 0.045f);
				result = this.WindWaves.SpectrumResolver.GetDisplacementAt(x, z, time);
			}
			return result;
		}

		public Vector3 GetUncompensatedDisplacementAt(float x, float z, float time)
		{
			return (this.WindWaves == null) ? default(Vector3) : this.WindWaves.SpectrumResolver.GetDisplacementAt(x, z, time);
		}

		public Vector2 GetHorizontalDisplacementAt(float x, float z)
		{
			Vector2 result = default(Vector2);
			if (this.WindWaves != null)
			{
				this.CompensateHorizontalDisplacement(ref x, ref z, 0.045f);
				result = this.WindWaves.SpectrumResolver.GetHorizontalDisplacementAt(x, z, this._Time);
			}
			return result;
		}

		public Vector2 GetHorizontalDisplacementAt(float x, float z, float time)
		{
			Vector2 result = default(Vector2);
			if (this.WindWaves != null)
			{
				this.CompensateHorizontalDisplacement(ref x, ref z, 0.045f);
				result = this.WindWaves.SpectrumResolver.GetHorizontalDisplacementAt(x, z, time);
			}
			return result;
		}

		public Vector2 GetUncompensatedHorizontalDisplacementAt(float x, float z, float time)
		{
			return (this.WindWaves == null) ? default(Vector2) : this.WindWaves.SpectrumResolver.GetHorizontalDisplacementAt(x, z, time);
		}

		public float GetHeightAt(float x, float z)
		{
			float result = 0f;
			if (this.WindWaves != null)
			{
				this.CompensateHorizontalDisplacement(ref x, ref z, 0.045f);
				result = this.WindWaves.SpectrumResolver.GetHeightAt(x, z, this._Time);
			}
			return result;
		}

		public float GetHeightAt(float x, float z, float time)
		{
			float result = 0f;
			if (this.WindWaves != null)
			{
				this.CompensateHorizontalDisplacement(ref x, ref z, 0.045f);
				result = this.WindWaves.SpectrumResolver.GetHeightAt(x, z, time);
			}
			return result;
		}

		public float GetUncompensatedHeightAt(float x, float z, float time)
		{
			return (this.WindWaves == null) ? 0f : this.WindWaves.SpectrumResolver.GetHeightAt(x, z, time);
		}

		public Vector4 GetHeightAndForcesAt(float x, float z)
		{
			Vector4 result = Vector4.zero;
			if (this.WindWaves != null)
			{
				this.CompensateHorizontalDisplacement(ref x, ref z, 0.045f);
				result = this.WindWaves.SpectrumResolver.GetForceAndHeightAt(x, z, this._Time);
			}
			return result;
		}

		public Vector4 GetHeightAndForcesAt(float x, float z, float time)
		{
			Vector4 result = Vector4.zero;
			if (this.WindWaves != null)
			{
				this.CompensateHorizontalDisplacement(ref x, ref z, 0.045f);
				result = this.WindWaves.SpectrumResolver.GetForceAndHeightAt(x, z, time);
			}
			return result;
		}

		public Vector4 GetUncompensatedHeightAndForcesAt(float x, float z, float time)
		{
			return (this.WindWaves == null) ? default(Vector4) : this.WindWaves.SpectrumResolver.GetForceAndHeightAt(x, z, time);
		}

		[Obsolete("Use this overload instead: GetDisplacementAt(float x, float z, float time).")]
		public Vector3 GetDisplacementAt(float x, float z, float spectrumStart, float spectrumEnd, float time)
		{
			Vector3 result = default(Vector3);
			if (this.WindWaves != null)
			{
				result = this.WindWaves.SpectrumResolver.GetDisplacementAt(x, z, time);
			}
			return result;
		}

		[Obsolete("Use this overload instead: GetHorizontalDisplacementAt(float x, float z, float time).")]
		public Vector2 GetHorizontalDisplacementAt(float x, float z, float spectrumStart, float spectrumEnd, float time)
		{
			Vector2 result = default(Vector2);
			if (this.WindWaves != null)
			{
				result = this.WindWaves.SpectrumResolver.GetHorizontalDisplacementAt(x, z, time);
			}
			return result;
		}

		[Obsolete("Use this overload instead: GetHeightAt(float x, float z, float time).")]
		public float GetHeightAt(float x, float z, float spectrumStart, float spectrumEnd, float time)
		{
			float result = 0f;
			if (this.WindWaves != null)
			{
				result = this.WindWaves.SpectrumResolver.GetHeightAt(x, z, time);
			}
			return result;
		}

		[Obsolete("Use this overload instead: GetHeightAndForcesAt(float x, float z, float time).")]
		public Vector4 GetHeightAndForcesAt(float x, float z, float spectrumStart, float spectrumEnd, float time)
		{
			Vector4 result = Vector4.zero;
			if (this.WindWaves != null)
			{
				result = this.WindWaves.SpectrumResolver.GetForceAndHeightAt(x, z, time);
			}
			return result;
		}

		public void CompensateHorizontalDisplacement(ref float x, ref float z, float errorTolerance = 0.045f)
		{
			float num = x;
			float num2 = z;
			SpectrumResolver spectrumResolver = this.WindWaves.SpectrumResolver;
			Vector2 horizontalDisplacementAt = spectrumResolver.GetHorizontalDisplacementAt(x, z, this._Time);
			x -= horizontalDisplacementAt.x;
			z -= horizontalDisplacementAt.y;
			if (horizontalDisplacementAt.x > errorTolerance || horizontalDisplacementAt.y > errorTolerance || horizontalDisplacementAt.x < -errorTolerance || horizontalDisplacementAt.y < -errorTolerance)
			{
				for (int i = 0; i < 14; i++)
				{
					horizontalDisplacementAt = spectrumResolver.GetHorizontalDisplacementAt(x, z, this._Time);
					float num3 = num - (x + horizontalDisplacementAt.x);
					float num4 = num2 - (z + horizontalDisplacementAt.y);
					x += num3 * Water._CompensationStepWeights[i];
					z += num4 * Water._CompensationStepWeights[i];
					if (num3 < errorTolerance && num4 < errorTolerance && num3 > -errorTolerance && num4 > -errorTolerance)
					{
						break;
					}
				}
			}
		}

		[Tooltip("Synchronizes dynamic preset with default water profile")]
		public bool Synchronize = true;

		public bool AskForWaterCamera = true;

		[FormerlySerializedAs("shaderSet")]
		[SerializeField]
		private ShaderSet _ShaderSet;

		[FormerlySerializedAs("seed")]
		[SerializeField]
		[Tooltip("Set it to anything else than 0 if your game has multiplayer functionality or you want your water to behave the same way each time your game is played (good for intro etc.).")]
		private int _Seed;

		[FormerlySerializedAs("materials")]
		[SerializeField]
		private WaterMaterials _Materials;

		[FormerlySerializedAs("profilesManager")]
		[SerializeField]
		private ProfilesManager _ProfilesManager;

		[FormerlySerializedAs("geometry")]
		[SerializeField]
		private WaterGeometry _Geometry;

		[FormerlySerializedAs("waterRenderer")]
		[SerializeField]
		private WaterRenderer _WaterRenderer;

		[FormerlySerializedAs("uvAnimator")]
		[SerializeField]
		private WaterUvAnimator _UVAnimator;

		[FormerlySerializedAs("volume")]
		[SerializeField]
		private WaterVolume _Volume;

		[FormerlySerializedAs("subsurfaceScattering")]
		[SerializeField]
		private WaterSubsurfaceScattering _SubsurfaceScattering;

		[FormerlySerializedAs("dynamicWaterData")]
		[SerializeField]
		private DynamicWater.Data _DynamicWaterData;

		[FormerlySerializedAs("foamData")]
		[SerializeField]
		private Foam.Data _FoamData;

		[FormerlySerializedAs("planarReflectionData")]
		[SerializeField]
		private PlanarReflection.Data _PlanarReflectionData;

		[FormerlySerializedAs("windWavesData")]
		[SerializeField]
		private WindWaves.Data _WindWavesData;

		[FormerlySerializedAs("dontRotateUpwards")]
		[SerializeField]
		private bool _DontRotateUpwards;

		[FormerlySerializedAs("fastEnableDisable")]
		[SerializeField]
		private bool _FastEnableDisable;

		[FormerlySerializedAs("version")]
		[SerializeField]
		private float _Version = 0.4f;

		private readonly List<WaterModule> _Modules = new List<WaterModule>();

		private bool _ComponentsCreated;

		private Vector2 _SurfaceOffset = new Vector2(float.NaN, float.NaN);

		private float _Time = -1f;

		private bool _RenderingEnabled = true;

		private int _WaterIdCache = -1;

		private static bool _IsPlaying;

		private static readonly Collider[] _CollidersBuffer = new Collider[30];

		private static readonly List<Water> _PossibleWaters = new List<Water>();

		private static readonly List<Water> _ExcludedWaters = new List<Water>();

		private static readonly float[] _CompensationStepWeights = new float[]
		{
			0.85f,
			0.75f,
			0.83f,
			0.77f,
			0.85f,
			0.75f,
			0.85f,
			0.75f,
			0.83f,
			0.77f,
			0.85f,
			0.75f,
			0.85f,
			0.75f
		};

		[Serializable]
		public struct WeightedProfile
		{
			public WeightedProfile(WaterProfile profile, float weight)
			{
				if (profile.Data.Spectrum == null)
				{
					Debug.Log("spectrum not created");
				}
				this.Profile = new WaterProfileData();
				this.Profile.Copy(profile.Data);
				this.Profile.TemplateProfile = profile;
				this.Weight = weight;
			}

			public WaterProfileData Profile;

			public float Weight;
		}

		[Serializable]
		public class WaterEvent : UnityEvent<Water>
		{
		}
	}
}
