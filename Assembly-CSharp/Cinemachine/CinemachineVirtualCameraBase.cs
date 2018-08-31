using System;
using UnityEngine;

namespace Cinemachine
{
	[SaveDuringPlay]
	public abstract class CinemachineVirtualCameraBase : MonoBehaviour, ICinemachineCamera
	{
		public int ValidatingStreamVersion
		{
			get
			{
				return (!this.m_OnValidateCalled) ? CinemachineCore.kStreamingVersion : this.m_ValidatingStreamVersion;
			}
			private set
			{
				this.m_ValidatingStreamVersion = value;
			}
		}

		public virtual void AddPostPipelineStageHook(CinemachineVirtualCameraBase.OnPostPipelineStageDelegate d)
		{
			this.OnPostPipelineStage = (CinemachineVirtualCameraBase.OnPostPipelineStageDelegate)Delegate.Remove(this.OnPostPipelineStage, d);
			this.OnPostPipelineStage = (CinemachineVirtualCameraBase.OnPostPipelineStageDelegate)Delegate.Combine(this.OnPostPipelineStage, d);
		}

		public virtual void RemovePostPipelineStageHook(CinemachineVirtualCameraBase.OnPostPipelineStageDelegate d)
		{
			this.OnPostPipelineStage = (CinemachineVirtualCameraBase.OnPostPipelineStageDelegate)Delegate.Remove(this.OnPostPipelineStage, d);
		}

		protected void InvokePostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState newState, float deltaTime)
		{
			if (this.OnPostPipelineStage != null)
			{
				this.OnPostPipelineStage(vcam, stage, ref newState, deltaTime);
			}
			CinemachineVirtualCameraBase cinemachineVirtualCameraBase = this.ParentCamera as CinemachineVirtualCameraBase;
			if (cinemachineVirtualCameraBase != null)
			{
				cinemachineVirtualCameraBase.InvokePostPipelineStageCallback(vcam, stage, ref newState, deltaTime);
			}
		}

		public string Name
		{
			get
			{
				return base.name;
			}
		}

		public virtual string Description
		{
			get
			{
				return string.Empty;
			}
		}

		public int Priority
		{
			get
			{
				return this.m_Priority;
			}
			set
			{
				this.m_Priority = value;
			}
		}

		public GameObject VirtualCameraGameObject
		{
			get
			{
				if (this == null)
				{
					return null;
				}
				return base.gameObject;
			}
		}

		public abstract CameraState State { get; }

		public virtual ICinemachineCamera LiveChildOrSelf
		{
			get
			{
				return this;
			}
		}

		public ICinemachineCamera ParentCamera
		{
			get
			{
				if (!this.mSlaveStatusUpdated || !Application.isPlaying)
				{
					this.UpdateSlaveStatus();
				}
				return this.m_parentVcam;
			}
		}

		public virtual bool IsLiveChild(ICinemachineCamera vcam)
		{
			return false;
		}

		public abstract Transform LookAt { get; set; }

		public abstract Transform Follow { get; set; }

		public bool PreviousStateIsValid
		{
			get
			{
				if (this.LookAt != this.m_previousLookAtTarget)
				{
					this.m_previousLookAtTarget = this.LookAt;
					this.m_previousStateIsValid = false;
				}
				if (this.Follow != this.m_previousFollowTarget)
				{
					this.m_previousFollowTarget = this.Follow;
					this.m_previousStateIsValid = false;
				}
				return this.m_previousStateIsValid;
			}
			set
			{
				this.m_previousStateIsValid = value;
			}
		}

		public abstract void UpdateCameraState(Vector3 worldUp, float deltaTime);

		public virtual void OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
		{
			if (!base.gameObject.activeInHierarchy)
			{
				this.PreviousStateIsValid = false;
			}
		}

		protected virtual void Start()
		{
		}

		protected virtual void OnDestroy()
		{
			CinemachineCore.Instance.RemoveActiveCamera(this);
		}

		protected virtual void OnValidate()
		{
			this.m_OnValidateCalled = true;
			this.ValidatingStreamVersion = this.m_StreamingVersion;
			this.m_StreamingVersion = CinemachineCore.kStreamingVersion;
		}

		protected virtual void OnEnable()
		{
			CinemachineVirtualCameraBase[] components = base.GetComponents<CinemachineVirtualCameraBase>();
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i].enabled && components[i] != this)
				{
					Debug.LogError(this.Name + " has multiple CinemachineVirtualCameraBase-derived components.  Disabling " + base.GetType().Name + ".");
					base.enabled = false;
				}
			}
			this.UpdateSlaveStatus();
			this.UpdateVcamPoolStatus();
			this.PreviousStateIsValid = false;
		}

		protected virtual void OnDisable()
		{
			this.UpdateVcamPoolStatus();
		}

		protected virtual void Update()
		{
			if (this.m_Priority != this.m_QueuePriority)
			{
				this.UpdateVcamPoolStatus();
			}
		}

		protected virtual void OnTransformParentChanged()
		{
			this.UpdateSlaveStatus();
			this.UpdateVcamPoolStatus();
		}

		private void UpdateSlaveStatus()
		{
			this.mSlaveStatusUpdated = true;
			this.m_parentVcam = null;
			Transform parent = base.transform.parent;
			if (parent != null)
			{
				this.m_parentVcam = parent.GetComponent<CinemachineVirtualCameraBase>();
			}
		}

		protected Transform ResolveLookAt(Transform localLookAt)
		{
			Transform transform = localLookAt;
			if (transform == null && this.ParentCamera != null)
			{
				transform = this.ParentCamera.LookAt;
			}
			return transform;
		}

		protected Transform ResolveFollow(Transform localFollow)
		{
			Transform transform = localFollow;
			if (transform == null && this.ParentCamera != null)
			{
				transform = this.ParentCamera.Follow;
			}
			return transform;
		}

		private void UpdateVcamPoolStatus()
		{
			this.m_QueuePriority = int.MaxValue;
			CinemachineCore.Instance.RemoveActiveCamera(this);
			CinemachineCore.Instance.RemoveChildCamera(this);
			if (this.m_parentVcam == null)
			{
				if (base.isActiveAndEnabled)
				{
					CinemachineCore.Instance.AddActiveCamera(this);
					this.m_QueuePriority = this.m_Priority;
				}
			}
			else if (base.isActiveAndEnabled)
			{
				CinemachineCore.Instance.AddChildCamera(this);
			}
		}

		public void MoveToTopOfPrioritySubqueue()
		{
			this.UpdateVcamPoolStatus();
		}

		[NoSaveDuringPlay]
		[HideInInspector]
		public Action CinemachineGUIDebuggerCallback;

		[HideInInspector]
		[SerializeField]
		[NoSaveDuringPlay]
		public string[] m_ExcludedPropertiesInInspector = new string[]
		{
			"m_Script"
		};

		[SerializeField]
		[NoSaveDuringPlay]
		[HideInInspector]
		public CinemachineCore.Stage[] m_LockStageInInspector;

		private int m_ValidatingStreamVersion;

		private bool m_OnValidateCalled;

		[SerializeField]
		[NoSaveDuringPlay]
		[HideInInspector]
		private int m_StreamingVersion;

		[NoSaveDuringPlay]
		[Tooltip("The priority will determine which camera becomes active based on the state of other cameras and this camera.  Higher numbers have greater priority.")]
		public int m_Priority = 10;

		protected CinemachineVirtualCameraBase.OnPostPipelineStageDelegate OnPostPipelineStage;

		private bool m_previousStateIsValid;

		private Transform m_previousLookAtTarget;

		private Transform m_previousFollowTarget;

		private bool mSlaveStatusUpdated;

		private CinemachineVirtualCameraBase m_parentVcam;

		private int m_QueuePriority = int.MaxValue;

		public delegate void OnPostPipelineStageDelegate(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState newState, float deltaTime);
	}
}
