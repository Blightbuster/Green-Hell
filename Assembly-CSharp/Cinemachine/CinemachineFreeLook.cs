using System;
using System.Collections;
using System.Reflection;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cinemachine
{
	[ExecuteInEditMode]
	[DocumentationSorting(11f, DocumentationSortingAttribute.Level.UserRef)]
	[AddComponentMenu("Cinemachine/CinemachineFreeLook")]
	[DisallowMultipleComponent]
	public class CinemachineFreeLook : CinemachineVirtualCameraBase
	{
		protected override void OnValidate()
		{
			base.OnValidate();
			if (this.m_LegacyHeadingBias != 3.40282347E+38f)
			{
				this.m_Heading.m_HeadingBias = this.m_LegacyHeadingBias;
				this.m_LegacyHeadingBias = float.MaxValue;
				this.m_RecenterToTargetHeading.LegacyUpgrade(ref this.m_Heading.m_HeadingDefinition, ref this.m_Heading.m_VelocityFilterStrength);
				this.mUseLegacyRigDefinitions = true;
			}
			this.m_YAxis.Validate();
			this.m_XAxis.Validate();
			this.m_RecenterToTargetHeading.Validate();
			this.m_Lens.Validate();
			this.InvalidateRigCache();
		}

		public CinemachineVirtualCamera GetRig(int i)
		{
			this.UpdateRigCache();
			return (i >= 0 && i <= 2) ? this.m_Rigs[i] : null;
		}

		public static string[] RigNames
		{
			get
			{
				return new string[]
				{
					"TopRig",
					"MiddleRig",
					"BottomRig"
				};
			}
		}

		protected override void OnEnable()
		{
			this.mIsDestroyed = false;
			base.OnEnable();
			this.InvalidateRigCache();
		}

		protected override void OnDestroy()
		{
			if (this.m_Rigs != null)
			{
				foreach (CinemachineVirtualCamera cinemachineVirtualCamera in this.m_Rigs)
				{
					if (cinemachineVirtualCamera != null && cinemachineVirtualCamera.gameObject != null)
					{
						cinemachineVirtualCamera.gameObject.hideFlags &= ~(HideFlags.HideInHierarchy | HideFlags.HideInInspector);
					}
				}
			}
			this.mIsDestroyed = true;
			base.OnDestroy();
		}

		private void OnTransformChildrenChanged()
		{
			this.InvalidateRigCache();
		}

		private void Reset()
		{
			this.DestroyRigs();
		}

		public override CameraState State
		{
			get
			{
				return this.m_State;
			}
		}

		public override Transform LookAt
		{
			get
			{
				return base.ResolveLookAt(this.m_LookAt);
			}
			set
			{
				this.m_LookAt = value;
			}
		}

		public override Transform Follow
		{
			get
			{
				return base.ResolveFollow(this.m_Follow);
			}
			set
			{
				this.m_Follow = value;
			}
		}

		public override ICinemachineCamera LiveChildOrSelf
		{
			get
			{
				if (this.m_Rigs == null || this.m_Rigs.Length != 3)
				{
					return this;
				}
				if (this.m_YAxis.Value < 0.33f)
				{
					return this.m_Rigs[2];
				}
				if (this.m_YAxis.Value > 0.66f)
				{
					return this.m_Rigs[0];
				}
				return this.m_Rigs[1];
			}
		}

		public override bool IsLiveChild(ICinemachineCamera vcam)
		{
			if (this.m_Rigs == null || this.m_Rigs.Length != 3)
			{
				return false;
			}
			if (this.m_YAxis.Value < 0.33f)
			{
				return vcam == this.m_Rigs[2];
			}
			if (this.m_YAxis.Value > 0.66f)
			{
				return vcam == this.m_Rigs[0];
			}
			return vcam == this.m_Rigs[1];
		}

		public override void RemovePostPipelineStageHook(CinemachineVirtualCameraBase.OnPostPipelineStageDelegate d)
		{
			base.RemovePostPipelineStageHook(d);
			this.UpdateRigCache();
			if (this.m_Rigs != null)
			{
				foreach (CinemachineVirtualCamera cinemachineVirtualCamera in this.m_Rigs)
				{
					if (cinemachineVirtualCamera != null)
					{
						cinemachineVirtualCamera.RemovePostPipelineStageHook(d);
					}
				}
			}
		}

		public override void UpdateCameraState(Vector3 worldUp, float deltaTime)
		{
			if (!base.PreviousStateIsValid)
			{
				deltaTime = -1f;
			}
			this.UpdateRigCache();
			if (deltaTime < 0f)
			{
				this.m_State = this.PullStateFromVirtualCamera(worldUp);
			}
			this.m_State = this.CalculateNewState(worldUp, deltaTime);
			if (this.Follow != null)
			{
				Vector3 b = this.State.RawPosition - base.transform.position;
				base.transform.position = this.State.RawPosition;
				this.m_Rigs[0].transform.position -= b;
				this.m_Rigs[1].transform.position -= b;
				this.m_Rigs[2].transform.position -= b;
			}
			base.PreviousStateIsValid = true;
			bool flag = deltaTime >= 0f || CinemachineCore.Instance.IsLive(this);
			if (flag)
			{
				this.m_YAxis.Update(deltaTime);
			}
			this.PushSettingsToRigs();
		}

		public override void OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
		{
			base.OnTransitionFromCamera(fromCam, worldUp, deltaTime);
			if (fromCam != null && fromCam is CinemachineFreeLook)
			{
				CinemachineFreeLook cinemachineFreeLook = fromCam as CinemachineFreeLook;
				if (cinemachineFreeLook.Follow == this.Follow)
				{
					this.m_XAxis.Value = cinemachineFreeLook.m_XAxis.Value;
					this.m_YAxis.Value = cinemachineFreeLook.m_YAxis.Value;
					this.UpdateCameraState(worldUp, deltaTime);
				}
			}
		}

		private void InvalidateRigCache()
		{
			this.mOrbitals = null;
		}

		private void DestroyRigs()
		{
			CinemachineVirtualCamera[] array = new CinemachineVirtualCamera[CinemachineFreeLook.RigNames.Length];
			for (int i = 0; i < CinemachineFreeLook.RigNames.Length; i++)
			{
				IEnumerator enumerator = base.transform.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform transform = (Transform)obj;
						if (transform.gameObject.name == CinemachineFreeLook.RigNames[i])
						{
							array[i] = transform.GetComponent<CinemachineVirtualCamera>();
						}
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] != null)
				{
					if (CinemachineFreeLook.DestroyRigOverride != null)
					{
						CinemachineFreeLook.DestroyRigOverride(array[j].gameObject);
					}
					else
					{
						UnityEngine.Object.Destroy(array[j].gameObject);
					}
				}
			}
			this.m_Rigs = null;
			this.mOrbitals = null;
		}

		private CinemachineVirtualCamera[] CreateRigs(CinemachineVirtualCamera[] copyFrom)
		{
			this.mOrbitals = null;
			float[] array = new float[]
			{
				0.5f,
				0.55f,
				0.6f
			};
			CinemachineVirtualCamera[] array2 = new CinemachineVirtualCamera[3];
			for (int i = 0; i < CinemachineFreeLook.RigNames.Length; i++)
			{
				CinemachineVirtualCamera cinemachineVirtualCamera = null;
				if (copyFrom != null && copyFrom.Length > i)
				{
					cinemachineVirtualCamera = copyFrom[i];
				}
				if (CinemachineFreeLook.CreateRigOverride != null)
				{
					array2[i] = CinemachineFreeLook.CreateRigOverride(this, CinemachineFreeLook.RigNames[i], cinemachineVirtualCamera);
				}
				else
				{
					array2[i] = new GameObject(CinemachineFreeLook.RigNames[i])
					{
						transform = 
						{
							parent = base.transform
						}
					}.AddComponent<CinemachineVirtualCamera>();
					if (cinemachineVirtualCamera != null)
					{
						ReflectionHelpers.CopyFields(cinemachineVirtualCamera, array2[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					}
					else
					{
						GameObject gameObject = array2[i].GetComponentOwner().gameObject;
						gameObject.AddComponent<CinemachineOrbitalTransposer>();
						gameObject.AddComponent<CinemachineComposer>();
					}
				}
				array2[i].InvalidateComponentPipeline();
				CinemachineOrbitalTransposer cinemachineOrbitalTransposer = array2[i].GetCinemachineComponent<CinemachineOrbitalTransposer>();
				if (cinemachineOrbitalTransposer == null)
				{
					cinemachineOrbitalTransposer = array2[i].AddCinemachineComponent<CinemachineOrbitalTransposer>();
				}
				if (cinemachineVirtualCamera == null)
				{
					cinemachineOrbitalTransposer.m_YawDamping = 0f;
					CinemachineComposer cinemachineComponent = array2[i].GetCinemachineComponent<CinemachineComposer>();
					if (cinemachineComponent != null)
					{
						cinemachineComponent.m_HorizontalDamping = (cinemachineComponent.m_VerticalDamping = 0f);
						cinemachineComponent.m_ScreenX = 0.5f;
						cinemachineComponent.m_ScreenY = array[i];
						cinemachineComponent.m_DeadZoneWidth = (cinemachineComponent.m_DeadZoneHeight = 0.1f);
						cinemachineComponent.m_SoftZoneWidth = (cinemachineComponent.m_SoftZoneHeight = 0.8f);
						cinemachineComponent.m_BiasX = (cinemachineComponent.m_BiasY = 0f);
					}
				}
			}
			return array2;
		}

		private void UpdateRigCache()
		{
			if (this.mIsDestroyed)
			{
				return;
			}
			if (this.m_Rigs != null && this.m_Rigs.Length == 3 && this.m_Rigs[0] != null && this.m_Rigs[0].transform.parent != base.transform)
			{
				this.DestroyRigs();
				this.m_Rigs = this.CreateRigs(this.m_Rigs);
			}
			if (this.mOrbitals != null && this.mOrbitals.Length == 3)
			{
				return;
			}
			if (this.LocateExistingRigs(CinemachineFreeLook.RigNames, false) != 3)
			{
				this.DestroyRigs();
				this.m_Rigs = this.CreateRigs(null);
				this.LocateExistingRigs(CinemachineFreeLook.RigNames, true);
			}
			foreach (CinemachineVirtualCamera cinemachineVirtualCamera in this.m_Rigs)
			{
				CinemachineVirtualCameraBase cinemachineVirtualCameraBase = cinemachineVirtualCamera;
				string[] excludedPropertiesInInspector;
				if (this.m_CommonLens)
				{
					string[] array = new string[6];
					array[0] = "m_Script";
					array[1] = "Header";
					array[2] = "Extensions";
					array[3] = "m_Priority";
					array[4] = "m_Follow";
					excludedPropertiesInInspector = array;
					array[5] = "m_Lens";
				}
				else
				{
					string[] array2 = new string[5];
					array2[0] = "m_Script";
					array2[1] = "Header";
					array2[2] = "Extensions";
					array2[3] = "m_Priority";
					excludedPropertiesInInspector = array2;
					array2[4] = "m_Follow";
				}
				cinemachineVirtualCameraBase.m_ExcludedPropertiesInInspector = excludedPropertiesInInspector;
				cinemachineVirtualCamera.m_LockStageInInspector = new CinemachineCore.Stage[1];
			}
			this.mBlendA = new CinemachineBlend(this.m_Rigs[1], this.m_Rigs[0], AnimationCurve.Linear(0f, 0f, 1f, 1f), 1f, 0f);
			this.mBlendB = new CinemachineBlend(this.m_Rigs[2], this.m_Rigs[1], AnimationCurve.Linear(0f, 0f, 1f, 1f), 1f, 0f);
			this.m_XAxis.SetThresholds(0f, 360f, true);
			this.m_YAxis.SetThresholds(0f, 1f, false);
		}

		private int LocateExistingRigs(string[] rigNames, bool forceOrbital)
		{
			this.mOrbitals = new CinemachineOrbitalTransposer[rigNames.Length];
			this.m_Rigs = new CinemachineVirtualCamera[rigNames.Length];
			int num = 0;
			IEnumerator enumerator = base.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					CinemachineVirtualCamera component = transform.GetComponent<CinemachineVirtualCamera>();
					if (component != null)
					{
						GameObject gameObject = transform.gameObject;
						for (int i = 0; i < rigNames.Length; i++)
						{
							if (this.mOrbitals[i] == null && gameObject.name == rigNames[i])
							{
								this.mOrbitals[i] = component.GetCinemachineComponent<CinemachineOrbitalTransposer>();
								if (this.mOrbitals[i] == null && forceOrbital)
								{
									this.mOrbitals[i] = component.AddCinemachineComponent<CinemachineOrbitalTransposer>();
								}
								if (this.mOrbitals[i] != null)
								{
									this.mOrbitals[i].m_HeadingIsSlave = true;
									if (i == 0)
									{
										this.mOrbitals[i].HeadingUpdater = ((CinemachineOrbitalTransposer orbital, float deltaTime, Vector3 up) => orbital.UpdateHeading(deltaTime, up, ref this.m_XAxis));
									}
									this.m_Rigs[i] = component;
									num++;
								}
							}
						}
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			return num;
		}

		private void PushSettingsToRigs()
		{
			this.UpdateRigCache();
			for (int i = 0; i < this.m_Rigs.Length; i++)
			{
				if (!(this.m_Rigs[i] == null))
				{
					if (this.m_CommonLens)
					{
						this.m_Rigs[i].m_Lens = this.m_Lens;
					}
					if (this.mUseLegacyRigDefinitions)
					{
						this.mUseLegacyRigDefinitions = false;
						this.m_Orbits[i].m_Height = this.mOrbitals[i].m_FollowOffset.y;
						this.m_Orbits[i].m_Radius = -this.mOrbitals[i].m_FollowOffset.z;
						if (this.m_Rigs[i].Follow != null)
						{
							this.Follow = this.m_Rigs[i].Follow;
						}
					}
					this.m_Rigs[i].Follow = null;
					if (CinemachineCore.sShowHiddenObjects)
					{
						this.m_Rigs[i].gameObject.hideFlags &= ~(HideFlags.HideInHierarchy | HideFlags.HideInInspector);
					}
					else
					{
						this.m_Rigs[i].gameObject.hideFlags |= (HideFlags.HideInHierarchy | HideFlags.HideInInspector);
					}
					this.mOrbitals[i].m_FollowOffset = this.GetLocalPositionForCameraFromInput(this.m_YAxis.Value);
					this.mOrbitals[i].m_BindingMode = this.m_BindingMode;
					this.mOrbitals[i].m_Heading = this.m_Heading;
					this.mOrbitals[i].m_XAxis = this.m_XAxis;
					this.mOrbitals[i].m_RecenterToTargetHeading = this.m_RecenterToTargetHeading;
					if (i > 0)
					{
						this.mOrbitals[i].m_RecenterToTargetHeading.m_enabled = false;
					}
					if (this.m_BindingMode == CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp)
					{
						this.m_Rigs[i].SetStateRawPosition(this.State.RawPosition);
					}
				}
			}
		}

		private CameraState CalculateNewState(Vector3 worldUp, float deltaTime)
		{
			CameraState result = this.PullStateFromVirtualCamera(worldUp);
			float value = this.m_YAxis.Value;
			if (value > 0.5f)
			{
				if (this.mBlendA != null)
				{
					this.mBlendA.TimeInBlend = (value - 0.5f) * 2f;
					this.mBlendA.UpdateCameraState(worldUp, deltaTime);
					result = this.mBlendA.State;
				}
			}
			else if (this.mBlendB != null)
			{
				this.mBlendB.TimeInBlend = value * 2f;
				this.mBlendB.UpdateCameraState(worldUp, deltaTime);
				result = this.mBlendB.State;
			}
			return result;
		}

		private CameraState PullStateFromVirtualCamera(Vector3 worldUp)
		{
			CameraState @default = CameraState.Default;
			@default.RawPosition = base.transform.position;
			@default.RawOrientation = base.transform.rotation;
			@default.ReferenceUp = worldUp;
			CinemachineBrain cinemachineBrain = CinemachineCore.Instance.FindPotentialTargetBrain(this);
			this.m_Lens.Aspect = ((!(cinemachineBrain != null)) ? 1f : cinemachineBrain.OutputCamera.aspect);
			this.m_Lens.Orthographic = (cinemachineBrain != null && cinemachineBrain.OutputCamera.orthographic);
			@default.Lens = this.m_Lens;
			return @default;
		}

		public Vector3 GetLocalPositionForCameraFromInput(float t)
		{
			if (this.mOrbitals == null)
			{
				return Vector3.zero;
			}
			this.UpdateCachedSpline();
			int num = 1;
			if (t > 0.5f)
			{
				t -= 0.5f;
				num = 2;
			}
			return SplineHelpers.Bezier3(t * 2f, this.m_CachedKnots[num], this.m_CachedCtrl1[num], this.m_CachedCtrl2[num], this.m_CachedKnots[num + 1]);
		}

		private void UpdateCachedSpline()
		{
			bool flag = this.m_CachedOrbits != null && this.m_CachedTension == this.m_SplineCurvature;
			int num = 0;
			while (num < 3 && flag)
			{
				flag = (this.m_CachedOrbits[num].m_Height == this.m_Orbits[num].m_Height && this.m_CachedOrbits[num].m_Radius == this.m_Orbits[num].m_Radius);
				num++;
			}
			if (!flag)
			{
				float splineCurvature = this.m_SplineCurvature;
				this.m_CachedKnots = new Vector4[5];
				this.m_CachedCtrl1 = new Vector4[5];
				this.m_CachedCtrl2 = new Vector4[5];
				this.m_CachedKnots[1] = new Vector4(0f, this.m_Orbits[2].m_Height, -this.m_Orbits[2].m_Radius, 0f);
				this.m_CachedKnots[2] = new Vector4(0f, this.m_Orbits[1].m_Height, -this.m_Orbits[1].m_Radius, 0f);
				this.m_CachedKnots[3] = new Vector4(0f, this.m_Orbits[0].m_Height, -this.m_Orbits[0].m_Radius, 0f);
				this.m_CachedKnots[0] = Vector4.Lerp(this.m_CachedKnots[1], Vector4.zero, splineCurvature);
				this.m_CachedKnots[4] = Vector4.Lerp(this.m_CachedKnots[3], Vector4.zero, splineCurvature);
				SplineHelpers.ComputeSmoothControlPoints(ref this.m_CachedKnots, ref this.m_CachedCtrl1, ref this.m_CachedCtrl2);
				this.m_CachedOrbits = new CinemachineFreeLook.Orbit[3];
				for (int i = 0; i < 3; i++)
				{
					this.m_CachedOrbits[i] = this.m_Orbits[i];
				}
				this.m_CachedTension = this.m_SplineCurvature;
			}
		}

		[NoSaveDuringPlay]
		[Tooltip("Object for the camera children to look at (the aim target).")]
		public Transform m_LookAt;

		[NoSaveDuringPlay]
		[Tooltip("Object for the camera children wants to move with (the body target).")]
		public Transform m_Follow;

		[Tooltip("If enabled, this lens setting will apply to all three child rigs, otherwise the child rig lens settings will be used")]
		[FormerlySerializedAs("m_UseCommonLensSetting")]
		public bool m_CommonLens = true;

		[Tooltip("Specifies the lens properties of this Virtual Camera.  This generally mirrors the Unity Camera's lens settings, and will be used to drive the Unity camera when the vcam is active")]
		[FormerlySerializedAs("m_LensAttributes")]
		[LensSettingsProperty]
		public LensSettings m_Lens = LensSettings.Default;

		[Header("Axis Control")]
		[Tooltip("The Vertical axis.  Value is 0..1.  Chooses how to blend the child rigs")]
		public AxisState m_YAxis = new AxisState(2f, 0.2f, 0.1f, 0.5f, "Mouse Y", false);

		[Tooltip("The Horizontal axis.  Value is 0..359.  This is passed on to the rigs' OrbitalTransposer component")]
		public AxisState m_XAxis = new AxisState(300f, 0.1f, 0.1f, 0f, "Mouse X", true);

		[Tooltip("The definition of Forward.  Camera will follow behind.")]
		public CinemachineOrbitalTransposer.Heading m_Heading = new CinemachineOrbitalTransposer.Heading(CinemachineOrbitalTransposer.Heading.HeadingDefinition.TargetForward, 4, 0f);

		[Tooltip("Controls how automatic recentering of the X axis is accomplished")]
		public CinemachineOrbitalTransposer.Recentering m_RecenterToTargetHeading = new CinemachineOrbitalTransposer.Recentering(false, 1f, 2f);

		[Header("Orbits")]
		[Tooltip("The coordinate space to use when interpreting the offset from the target.  This is also used to set the camera's Up vector, which will be maintained when aiming the camera.")]
		public CinemachineTransposer.BindingMode m_BindingMode = CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp;

		[FormerlySerializedAs("m_SplineTension")]
		[Tooltip("Controls how taut is the line that connects the rigs' orbits, which determines final placement on the Y axis")]
		[Range(0f, 1f)]
		public float m_SplineCurvature = 0.2f;

		[Tooltip("The radius and height of the three orbiting rigs.")]
		public CinemachineFreeLook.Orbit[] m_Orbits = new CinemachineFreeLook.Orbit[]
		{
			new CinemachineFreeLook.Orbit(4.5f, 1.75f),
			new CinemachineFreeLook.Orbit(2.5f, 3f),
			new CinemachineFreeLook.Orbit(0.4f, 1.3f)
		};

		[FormerlySerializedAs("m_HeadingBias")]
		[HideInInspector]
		[SerializeField]
		private float m_LegacyHeadingBias = float.MaxValue;

		private bool mUseLegacyRigDefinitions;

		private bool mIsDestroyed;

		private CameraState m_State = CameraState.Default;

		[HideInInspector]
		[NoSaveDuringPlay]
		[SerializeField]
		private CinemachineVirtualCamera[] m_Rigs = new CinemachineVirtualCamera[3];

		private CinemachineOrbitalTransposer[] mOrbitals;

		private CinemachineBlend mBlendA;

		private CinemachineBlend mBlendB;

		public static CinemachineFreeLook.CreateRigDelegate CreateRigOverride;

		public static CinemachineFreeLook.DestroyRigDelegate DestroyRigOverride;

		private CinemachineFreeLook.Orbit[] m_CachedOrbits;

		private float m_CachedTension;

		private Vector4[] m_CachedKnots;

		private Vector4[] m_CachedCtrl1;

		private Vector4[] m_CachedCtrl2;

		[Serializable]
		public struct Orbit
		{
			public Orbit(float h, float r)
			{
				this.m_Height = h;
				this.m_Radius = r;
			}

			public float m_Height;

			public float m_Radius;
		}

		public delegate CinemachineVirtualCamera CreateRigDelegate(CinemachineFreeLook vcam, string name, CinemachineVirtualCamera copyFrom);

		public delegate void DestroyRigDelegate(GameObject rig);
	}
}
