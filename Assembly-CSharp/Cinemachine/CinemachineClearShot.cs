using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinemachine
{
	[ExecuteInEditMode]
	[AddComponentMenu("Cinemachine/CinemachineClearShot")]
	[DisallowMultipleComponent]
	[DocumentationSorting(12f, DocumentationSortingAttribute.Level.UserRef)]
	public class CinemachineClearShot : CinemachineVirtualCameraBase
	{
		public override string Description
		{
			get
			{
				ICinemachineCamera liveChild = this.LiveChild;
				if (this.mActiveBlend == null)
				{
					return (liveChild == null) ? "(none)" : ("[" + liveChild.Name + "]");
				}
				return this.mActiveBlend.Description;
			}
		}

		public ICinemachineCamera LiveChild { get; set; }

		public override CameraState State
		{
			get
			{
				return this.m_State;
			}
		}

		public override ICinemachineCamera LiveChildOrSelf
		{
			get
			{
				return this.LiveChild;
			}
		}

		public override bool IsLiveChild(ICinemachineCamera vcam)
		{
			return vcam == this.LiveChild || (this.mActiveBlend != null && (vcam == this.mActiveBlend.CamA || vcam == this.mActiveBlend.CamB));
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

		public override void RemovePostPipelineStageHook(CinemachineVirtualCameraBase.OnPostPipelineStageDelegate d)
		{
			base.RemovePostPipelineStageHook(d);
			this.UpdateListOfChildren();
			foreach (CinemachineVirtualCameraBase cinemachineVirtualCameraBase in this.m_ChildCameras)
			{
				cinemachineVirtualCameraBase.RemovePostPipelineStageHook(d);
			}
		}

		public override void UpdateCameraState(Vector3 worldUp, float deltaTime)
		{
			if (!base.PreviousStateIsValid)
			{
				deltaTime = -1f;
			}
			this.UpdateListOfChildren();
			ICinemachineCamera liveChild = this.LiveChild;
			this.LiveChild = this.ChooseCurrentCamera(worldUp, deltaTime);
			if (liveChild != null && this.LiveChild != null && liveChild != this.LiveChild)
			{
				float duration = 0f;
				AnimationCurve blendCurve = this.LookupBlendCurve(liveChild, this.LiveChild, out duration);
				this.mActiveBlend = this.CreateBlend(liveChild, this.LiveChild, blendCurve, duration, this.mActiveBlend, deltaTime);
				this.LiveChild.OnTransitionFromCamera(liveChild, worldUp, deltaTime);
				CinemachineCore.Instance.GenerateCameraActivationEvent(this.LiveChild);
				if (this.mActiveBlend == null)
				{
					CinemachineCore.Instance.GenerateCameraCutEvent(this.LiveChild);
				}
			}
			if (this.mActiveBlend != null)
			{
				this.mActiveBlend.TimeInBlend += ((deltaTime < 0f) ? this.mActiveBlend.Duration : deltaTime);
				if (this.mActiveBlend.IsComplete)
				{
					this.mActiveBlend = null;
				}
			}
			if (this.mActiveBlend != null)
			{
				this.mActiveBlend.UpdateCameraState(worldUp, deltaTime);
				this.m_State = this.mActiveBlend.State;
			}
			else if (this.LiveChild != null)
			{
				this.m_State = this.LiveChild.State;
			}
			base.PreviousStateIsValid = true;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			this.InvalidateListOfChildren();
			this.mActiveBlend = null;
		}

		public void OnTransformChildrenChanged()
		{
			this.InvalidateListOfChildren();
		}

		public bool IsBlending
		{
			get
			{
				return this.mActiveBlend != null;
			}
		}

		public CinemachineVirtualCameraBase[] ChildCameras
		{
			get
			{
				this.UpdateListOfChildren();
				return this.m_ChildCameras;
			}
		}

		private void InvalidateListOfChildren()
		{
			this.m_ChildCameras = null;
			this.m_RandomizedChilden = null;
			this.LiveChild = null;
		}

		public void ResetRandomization()
		{
			this.m_RandomizedChilden = null;
			this.mRandomizeNow = true;
		}

		private void UpdateListOfChildren()
		{
			if (this.m_ChildCameras != null)
			{
				return;
			}
			List<CinemachineVirtualCameraBase> list = new List<CinemachineVirtualCameraBase>();
			CinemachineVirtualCameraBase[] componentsInChildren = base.GetComponentsInChildren<CinemachineVirtualCameraBase>(true);
			foreach (CinemachineVirtualCameraBase cinemachineVirtualCameraBase in componentsInChildren)
			{
				if (cinemachineVirtualCameraBase.transform.parent == base.transform)
				{
					list.Add(cinemachineVirtualCameraBase);
				}
			}
			this.m_ChildCameras = list.ToArray();
			this.mActivationTime = (this.mPendingActivationTime = 0f);
			this.mPendingCamera = null;
			this.LiveChild = null;
			this.mActiveBlend = null;
		}

		private ICinemachineCamera ChooseCurrentCamera(Vector3 worldUp, float deltaTime)
		{
			if (this.m_ChildCameras == null || this.m_ChildCameras.Length == 0)
			{
				this.mActivationTime = 0f;
				return null;
			}
			CinemachineVirtualCameraBase[] array = this.m_ChildCameras;
			if (!this.m_RandomizeChoice)
			{
				this.m_RandomizedChilden = null;
			}
			else if (this.m_ChildCameras.Length > 1)
			{
				if (this.m_RandomizedChilden == null)
				{
					this.m_RandomizedChilden = this.Randomize(this.m_ChildCameras);
				}
				array = this.m_RandomizedChilden;
			}
			if (this.LiveChild != null && !this.LiveChild.VirtualCameraGameObject.activeSelf)
			{
				this.LiveChild = null;
			}
			ICinemachineCamera cinemachineCamera = this.LiveChild;
			foreach (CinemachineVirtualCameraBase cinemachineVirtualCameraBase in array)
			{
				if (cinemachineVirtualCameraBase != null && cinemachineVirtualCameraBase.VirtualCameraGameObject.activeInHierarchy && (cinemachineCamera == null || cinemachineVirtualCameraBase.State.ShotQuality > cinemachineCamera.State.ShotQuality || (cinemachineVirtualCameraBase.State.ShotQuality == cinemachineCamera.State.ShotQuality && cinemachineVirtualCameraBase.Priority > cinemachineCamera.Priority) || (this.m_RandomizeChoice && this.mRandomizeNow && cinemachineVirtualCameraBase != this.LiveChild && cinemachineVirtualCameraBase.State.ShotQuality == cinemachineCamera.State.ShotQuality && cinemachineVirtualCameraBase.Priority == cinemachineCamera.Priority)))
				{
					cinemachineCamera = cinemachineVirtualCameraBase;
				}
			}
			this.mRandomizeNow = false;
			float time = Time.time;
			if (this.mActivationTime != 0f)
			{
				if (this.LiveChild == cinemachineCamera)
				{
					this.mPendingActivationTime = 0f;
					this.mPendingCamera = null;
					return cinemachineCamera;
				}
				if (deltaTime >= 0f && this.mPendingActivationTime != 0f && this.mPendingCamera == cinemachineCamera)
				{
					if (time - this.mPendingActivationTime > this.m_ActivateAfter && time - this.mActivationTime > this.m_MinDuration)
					{
						this.m_RandomizedChilden = null;
						this.mActivationTime = time;
						this.mPendingActivationTime = 0f;
						this.mPendingCamera = null;
						return cinemachineCamera;
					}
					return this.LiveChild;
				}
			}
			this.mPendingActivationTime = 0f;
			this.mPendingCamera = null;
			if (deltaTime >= 0f && this.mActivationTime > 0f && (this.m_ActivateAfter > 0f || time - this.mActivationTime < this.m_MinDuration))
			{
				this.mPendingCamera = cinemachineCamera;
				this.mPendingActivationTime = time;
				return this.LiveChild;
			}
			this.m_RandomizedChilden = null;
			this.mActivationTime = time;
			return cinemachineCamera;
		}

		private CinemachineVirtualCameraBase[] Randomize(CinemachineVirtualCameraBase[] src)
		{
			List<CinemachineClearShot.Pair> list = new List<CinemachineClearShot.Pair>();
			for (int i = 0; i < src.Length; i++)
			{
				list.Add(new CinemachineClearShot.Pair
				{
					a = i,
					b = UnityEngine.Random.Range(0f, 1000f)
				});
			}
			list.Sort((CinemachineClearShot.Pair p1, CinemachineClearShot.Pair p2) => (int)p1.b - (int)p2.b);
			CinemachineVirtualCameraBase[] array = new CinemachineVirtualCameraBase[src.Length];
			CinemachineClearShot.Pair[] array2 = list.ToArray();
			for (int j = 0; j < src.Length; j++)
			{
				array[j] = src[array2[j].a];
			}
			return array;
		}

		private AnimationCurve LookupBlendCurve(ICinemachineCamera fromKey, ICinemachineCamera toKey, out float duration)
		{
			AnimationCurve animationCurve = this.m_DefaultBlend.BlendCurve;
			if (this.m_CustomBlends != null)
			{
				string fromCameraName = (fromKey == null) ? string.Empty : fromKey.Name;
				string toCameraName = (toKey == null) ? string.Empty : toKey.Name;
				animationCurve = this.m_CustomBlends.GetBlendCurveForVirtualCameras(fromCameraName, toCameraName, animationCurve);
			}
			Keyframe[] keys = animationCurve.keys;
			duration = ((keys != null && keys.Length != 0) ? keys[keys.Length - 1].time : 0f);
			return animationCurve;
		}

		private CinemachineBlend CreateBlend(ICinemachineCamera camA, ICinemachineCamera camB, AnimationCurve blendCurve, float duration, CinemachineBlend activeBlend, float deltaTime)
		{
			if (blendCurve == null || duration <= 0f || (camA == null && camB == null))
			{
				return null;
			}
			if (camA == null || activeBlend != null)
			{
				CameraState state = (activeBlend == null) ? this.State : activeBlend.State;
				camA = new StaticPointVirtualCamera(state, (activeBlend == null) ? "(none)" : "Mid-blend");
			}
			return new CinemachineBlend(camA, camB, blendCurve, duration, 0f);
		}

		public override void OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
		{
			base.OnTransitionFromCamera(fromCam, worldUp, deltaTime);
			if (this.m_RandomizeChoice && this.mActiveBlend == null)
			{
				this.m_RandomizedChilden = null;
				this.LiveChild = null;
				this.UpdateCameraState(worldUp, deltaTime);
			}
		}

		[NoSaveDuringPlay]
		[Tooltip("Default object for the camera children to look at (the aim target), if not specified in a child camera.  May be empty if all children specify targets of their own.")]
		public Transform m_LookAt;

		[NoSaveDuringPlay]
		[Tooltip("Default object for the camera children wants to move with (the body target), if not specified in a child camera.  May be empty if all children specify targets of their own.")]
		public Transform m_Follow;

		[NoSaveDuringPlay]
		[Tooltip("When enabled, the current child camera and blend will be indicated in the game window, for debugging")]
		public bool m_ShowDebugText;

		[HideInInspector]
		[NoSaveDuringPlay]
		[SerializeField]
		public CinemachineVirtualCameraBase[] m_ChildCameras;

		[Tooltip("Wait this many seconds before activating a new child camera")]
		public float m_ActivateAfter;

		[Tooltip("An active camera must be active for at least this many seconds")]
		public float m_MinDuration;

		[Tooltip("If checked, camera choice will be randomized if multiple cameras are equally desirable.  Otherwise, child list order and child camera priority will be used.")]
		public bool m_RandomizeChoice;

		[CinemachineBlendDefinitionProperty]
		[Tooltip("The blend which is used if you don't explicitly define a blend between two Virtual Cameras")]
		public CinemachineBlendDefinition m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);

		[HideInInspector]
		public CinemachineBlenderSettings m_CustomBlends;

		private CameraState m_State = CameraState.Default;

		private float mActivationTime;

		private float mPendingActivationTime;

		private ICinemachineCamera mPendingCamera;

		private CinemachineBlend mActiveBlend;

		private bool mRandomizeNow;

		private CinemachineVirtualCameraBase[] m_RandomizedChilden;

		private struct Pair
		{
			public int a;

			public float b;
		}
	}
}
