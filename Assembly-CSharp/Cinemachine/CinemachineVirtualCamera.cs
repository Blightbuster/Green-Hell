using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cinemachine
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Cinemachine/CinemachineVirtualCamera")]
	[DocumentationSorting(1f, DocumentationSortingAttribute.Level.UserRef)]
	[ExecuteInEditMode]
	public class CinemachineVirtualCamera : CinemachineVirtualCameraBase
	{
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

		public override void UpdateCameraState(Vector3 worldUp, float deltaTime)
		{
			if (!base.PreviousStateIsValid)
			{
				deltaTime = -1f;
			}
			if (deltaTime < 0f)
			{
				this.m_State = this.PullStateFromVirtualCamera(worldUp);
			}
			this.m_State = this.CalculateNewState(worldUp, deltaTime);
			if (!this.UserIsDragging)
			{
				if (this.Follow != null)
				{
					base.transform.position = this.State.RawPosition;
				}
				if (this.LookAt != null)
				{
					base.transform.rotation = this.State.RawOrientation;
				}
			}
			base.PreviousStateIsValid = true;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			this.InvalidateComponentPipeline();
			if (base.ValidatingStreamVersion < 20170927)
			{
				if (this.Follow != null && this.GetCinemachineComponent(CinemachineCore.Stage.Body) == null)
				{
					this.AddCinemachineComponent<CinemachineHardLockToTarget>();
				}
				if (this.LookAt != null && this.GetCinemachineComponent(CinemachineCore.Stage.Aim) == null)
				{
					this.AddCinemachineComponent<CinemachineHardLookAt>();
				}
			}
		}

		protected override void OnDestroy()
		{
			IEnumerator enumerator = base.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					if (transform.GetComponent<CinemachinePipeline>() != null)
					{
						transform.gameObject.hideFlags &= ~(HideFlags.HideInHierarchy | HideFlags.HideInInspector);
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
			base.OnDestroy();
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			this.m_Lens.Validate();
		}

		private void OnTransformChildrenChanged()
		{
			this.InvalidateComponentPipeline();
		}

		private void Reset()
		{
			this.DestroyPipeline();
		}

		private void DestroyPipeline()
		{
			List<Transform> list = new List<Transform>();
			IEnumerator enumerator = base.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					if (transform.GetComponent<CinemachinePipeline>() != null)
					{
						list.Add(transform);
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
			foreach (Transform transform2 in list)
			{
				if (CinemachineVirtualCamera.DestroyPipelineOverride != null)
				{
					CinemachineVirtualCamera.DestroyPipelineOverride(transform2.gameObject);
				}
				else
				{
					UnityEngine.Object.Destroy(transform2.gameObject);
				}
			}
			this.m_ComponentOwner = null;
			base.PreviousStateIsValid = false;
		}

		private Transform CreatePipeline(CinemachineVirtualCamera copyFrom)
		{
			CinemachineComponentBase[] array = null;
			if (copyFrom != null)
			{
				copyFrom.InvalidateComponentPipeline();
				array = copyFrom.GetComponentPipeline();
			}
			Transform result;
			if (CinemachineVirtualCamera.CreatePipelineOverride != null)
			{
				result = CinemachineVirtualCamera.CreatePipelineOverride(this, "cm", array);
			}
			else
			{
				GameObject gameObject = new GameObject("cm");
				gameObject.transform.parent = base.transform;
				gameObject.AddComponent<CinemachinePipeline>();
				result = gameObject.transform;
				if (array != null)
				{
					foreach (CinemachineComponentBase component in array)
					{
						ReflectionHelpers.CopyFields(component, gameObject.AddComponent(component.GetType()), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					}
				}
			}
			base.PreviousStateIsValid = false;
			return result;
		}

		public void InvalidateComponentPipeline()
		{
			this.m_ComponentPipeline = null;
		}

		public Transform GetComponentOwner()
		{
			this.UpdateComponentPipeline();
			return this.m_ComponentOwner;
		}

		public CinemachineComponentBase[] GetComponentPipeline()
		{
			this.UpdateComponentPipeline();
			return this.m_ComponentPipeline;
		}

		public CinemachineComponentBase GetCinemachineComponent(CinemachineCore.Stage stage)
		{
			CinemachineComponentBase[] componentPipeline = this.GetComponentPipeline();
			if (componentPipeline != null)
			{
				foreach (CinemachineComponentBase cinemachineComponentBase in componentPipeline)
				{
					if (cinemachineComponentBase.Stage == stage)
					{
						return cinemachineComponentBase;
					}
				}
			}
			return null;
		}

		public T GetCinemachineComponent<T>() where T : CinemachineComponentBase
		{
			CinemachineComponentBase[] componentPipeline = this.GetComponentPipeline();
			if (componentPipeline != null)
			{
				foreach (CinemachineComponentBase cinemachineComponentBase in componentPipeline)
				{
					if (cinemachineComponentBase is T)
					{
						return cinemachineComponentBase as T;
					}
				}
			}
			return (T)((object)null);
		}

		public T AddCinemachineComponent<T>() where T : CinemachineComponentBase
		{
			Transform componentOwner = this.GetComponentOwner();
			CinemachineComponentBase[] components = componentOwner.GetComponents<CinemachineComponentBase>();
			T t = componentOwner.gameObject.AddComponent<T>();
			if (t != null && components != null)
			{
				CinemachineCore.Stage stage = t.Stage;
				for (int i = components.Length - 1; i >= 0; i--)
				{
					if (components[i].Stage == stage)
					{
						components[i].enabled = false;
						UnityEngine.Object.DestroyImmediate(components[i]);
					}
				}
			}
			this.InvalidateComponentPipeline();
			return t;
		}

		public void DestroyCinemachineComponent<T>() where T : CinemachineComponentBase
		{
			CinemachineComponentBase[] componentPipeline = this.GetComponentPipeline();
			if (componentPipeline != null)
			{
				foreach (CinemachineComponentBase cinemachineComponentBase in componentPipeline)
				{
					if (cinemachineComponentBase is T)
					{
						cinemachineComponentBase.enabled = false;
						UnityEngine.Object.DestroyImmediate(cinemachineComponentBase);
						this.InvalidateComponentPipeline();
					}
				}
			}
		}

		public bool UserIsDragging { get; set; }

		public void OnPositionDragged(Vector3 delta)
		{
			CinemachineComponentBase[] componentPipeline = this.GetComponentPipeline();
			if (componentPipeline != null)
			{
				for (int i = 0; i < componentPipeline.Length; i++)
				{
					componentPipeline[i].OnPositionDragged(delta);
				}
			}
		}

		private void UpdateComponentPipeline()
		{
			if (this.m_ComponentOwner != null && this.m_ComponentOwner.parent != base.transform)
			{
				CinemachineVirtualCamera copyFrom = (!(this.m_ComponentOwner.parent != null)) ? null : this.m_ComponentOwner.parent.gameObject.GetComponent<CinemachineVirtualCamera>();
				this.DestroyPipeline();
				this.m_ComponentOwner = this.CreatePipeline(copyFrom);
			}
			if (this.m_ComponentOwner != null && this.m_ComponentPipeline != null)
			{
				return;
			}
			this.m_ComponentOwner = null;
			List<CinemachineComponentBase> list = new List<CinemachineComponentBase>();
			IEnumerator enumerator = base.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					if (transform.GetComponent<CinemachinePipeline>() != null)
					{
						this.m_ComponentOwner = transform;
						CinemachineComponentBase[] components = transform.GetComponents<CinemachineComponentBase>();
						foreach (CinemachineComponentBase item in components)
						{
							list.Add(item);
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
			if (this.m_ComponentOwner == null)
			{
				this.m_ComponentOwner = this.CreatePipeline(null);
			}
			if (CinemachineCore.sShowHiddenObjects)
			{
				this.m_ComponentOwner.gameObject.hideFlags &= ~(HideFlags.HideInHierarchy | HideFlags.HideInInspector);
			}
			else
			{
				this.m_ComponentOwner.gameObject.hideFlags |= (HideFlags.HideInHierarchy | HideFlags.HideInInspector);
			}
			list.Sort((CinemachineComponentBase c1, CinemachineComponentBase c2) => c1.Stage - c2.Stage);
			this.m_ComponentPipeline = list.ToArray();
		}

		private CameraState CalculateNewState(Vector3 worldUp, float deltaTime)
		{
			CameraState result = this.PullStateFromVirtualCamera(worldUp);
			if (this.LookAt != null)
			{
				result.ReferenceLookAt = this.LookAt.position;
			}
			CinemachineCore.Stage curStage = CinemachineCore.Stage.Body;
			this.UpdateComponentPipeline();
			if (this.m_ComponentPipeline != null)
			{
				for (int i = 0; i < this.m_ComponentPipeline.Length; i++)
				{
					this.m_ComponentPipeline[i].PrePipelineMutateCameraState(ref result);
				}
				for (int j = 0; j < this.m_ComponentPipeline.Length; j++)
				{
					curStage = this.AdvancePipelineStage(ref result, deltaTime, curStage, (int)this.m_ComponentPipeline[j].Stage);
					this.m_ComponentPipeline[j].MutateCameraState(ref result, deltaTime);
				}
			}
			int maxStage = 3;
			this.AdvancePipelineStage(ref result, deltaTime, curStage, maxStage);
			return result;
		}

		private CinemachineCore.Stage AdvancePipelineStage(ref CameraState state, float deltaTime, CinemachineCore.Stage curStage, int maxStage)
		{
			while (curStage < (CinemachineCore.Stage)maxStage)
			{
				base.InvokePostPipelineStageCallback(this, curStage, ref state, deltaTime);
				curStage++;
			}
			return curStage;
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

		internal void SetStateRawPosition(Vector3 pos)
		{
			this.m_State.RawPosition = pos;
		}

		[NoSaveDuringPlay]
		[Tooltip("The object that the camera wants to look at (the Aim target).  If this is null, then the vcam's Transform orientation will define the camera's orientation.")]
		public Transform m_LookAt;

		[Tooltip("The object that the camera wants to move with (the Body target).  If this is null, then the vcam's Transform position will define the camera's position.")]
		[NoSaveDuringPlay]
		public Transform m_Follow;

		[FormerlySerializedAs("m_LensAttributes")]
		[Tooltip("Specifies the lens properties of this Virtual Camera.  This generally mirrors the Unity Camera's lens settings, and will be used to drive the Unity camera when the vcam is active.")]
		[LensSettingsProperty]
		public LensSettings m_Lens = LensSettings.Default;

		public const string PipelineName = "cm";

		public static CinemachineVirtualCamera.CreatePipelineDelegate CreatePipelineOverride;

		public static CinemachineVirtualCamera.DestroyPipelineDelegate DestroyPipelineOverride;

		private CameraState m_State = CameraState.Default;

		private CinemachineComponentBase[] m_ComponentPipeline;

		[SerializeField]
		[HideInInspector]
		private Transform m_ComponentOwner;

		public delegate Transform CreatePipelineDelegate(CinemachineVirtualCamera vcam, string name, CinemachineComponentBase[] copyFrom);

		public delegate void DestroyPipelineDelegate(GameObject pipeline);
	}
}
