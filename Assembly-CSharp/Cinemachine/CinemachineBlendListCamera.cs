using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinemachine
{
	[ExecuteInEditMode]
	[DocumentationSorting(13f, DocumentationSortingAttribute.Level.UserRef)]
	[AddComponentMenu("Cinemachine/CinemachineBlendListCamera")]
	[DisallowMultipleComponent]
	public class CinemachineBlendListCamera : CinemachineVirtualCameraBase
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

		public override void RemovePostPipelineStageHook(CinemachineVirtualCameraBase.OnPostPipelineStageDelegate d)
		{
			base.RemovePostPipelineStageHook(d);
			this.UpdateListOfChildren();
			foreach (CinemachineVirtualCameraBase cinemachineVirtualCameraBase in this.m_ChildCameras)
			{
				cinemachineVirtualCameraBase.RemovePostPipelineStageHook(d);
			}
		}

		public override void OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
		{
			base.OnTransitionFromCamera(fromCam, worldUp, deltaTime);
			this.mActivationTime = Time.time;
			this.mCurrentInstruction = -1;
			this.LiveChild = null;
			this.mActiveBlend = null;
			this.UpdateCameraState(worldUp, deltaTime);
		}

		public override void UpdateCameraState(Vector3 worldUp, float deltaTime)
		{
			if (!base.PreviousStateIsValid)
			{
				deltaTime = -1f;
			}
			this.UpdateListOfChildren();
			this.AdvanceCurrentInstruction();
			CinemachineVirtualCameraBase cinemachineVirtualCameraBase = null;
			if (this.mCurrentInstruction >= 0 && this.mCurrentInstruction < this.m_Instructions.Length)
			{
				cinemachineVirtualCameraBase = this.m_Instructions[this.mCurrentInstruction].m_VirtualCamera;
			}
			if (this.m_ChildCameras != null)
			{
				for (int i = 0; i < this.m_ChildCameras.Length; i++)
				{
					CinemachineVirtualCameraBase cinemachineVirtualCameraBase2 = this.m_ChildCameras[i];
					if (cinemachineVirtualCameraBase2 != null)
					{
						bool flag = this.m_EnableAllChildCameras || cinemachineVirtualCameraBase2 == cinemachineVirtualCameraBase;
						if (flag != cinemachineVirtualCameraBase2.VirtualCameraGameObject.activeInHierarchy)
						{
							cinemachineVirtualCameraBase2.gameObject.SetActive(flag);
							if (flag)
							{
								CinemachineCore.Instance.UpdateVirtualCamera(cinemachineVirtualCameraBase2, worldUp, deltaTime);
							}
						}
					}
				}
			}
			if (cinemachineVirtualCameraBase != null)
			{
				ICinemachineCamera liveChild = this.LiveChild;
				this.LiveChild = cinemachineVirtualCameraBase;
				if (liveChild != null && this.LiveChild != null && liveChild != this.LiveChild && this.mCurrentInstruction > 0)
				{
					this.mActiveBlend = this.CreateBlend(liveChild, this.LiveChild, this.m_Instructions[this.mCurrentInstruction].m_Blend.BlendCurve, this.m_Instructions[this.mCurrentInstruction].m_Blend.m_Time, this.mActiveBlend, deltaTime);
					this.LiveChild.OnTransitionFromCamera(liveChild, worldUp, deltaTime);
					CinemachineCore.Instance.GenerateCameraActivationEvent(this.LiveChild);
					if (this.mActiveBlend == null)
					{
						CinemachineCore.Instance.GenerateCameraCutEvent(this.LiveChild);
					}
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

		public CinemachineVirtualCameraBase[] ChildCameras
		{
			get
			{
				this.UpdateListOfChildren();
				return this.m_ChildCameras;
			}
		}

		public bool IsBlending
		{
			get
			{
				return this.mActiveBlend != null;
			}
		}

		private void InvalidateListOfChildren()
		{
			this.m_ChildCameras = null;
			this.LiveChild = null;
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
			this.ValidateInstructions();
		}

		public void ValidateInstructions()
		{
			if (this.m_Instructions == null)
			{
				this.m_Instructions = new CinemachineBlendListCamera.Instruction[0];
			}
			for (int i = 0; i < this.m_Instructions.Length; i++)
			{
				if (this.m_Instructions[i].m_VirtualCamera != null && this.m_Instructions[i].m_VirtualCamera.transform.parent != base.transform)
				{
					this.m_Instructions[i].m_VirtualCamera = null;
				}
			}
			this.mActiveBlend = null;
		}

		private void AdvanceCurrentInstruction()
		{
			if (this.m_ChildCameras == null || this.m_ChildCameras.Length == 0 || this.mActivationTime < 0f || this.m_Instructions.Length == 0)
			{
				this.mActivationTime = -1f;
				this.mCurrentInstruction = -1;
				this.mActiveBlend = null;
			}
			else if (this.mCurrentInstruction >= this.m_Instructions.Length - 1)
			{
				this.mCurrentInstruction = this.m_Instructions.Length - 1;
			}
			else
			{
				float time = Time.time;
				if (this.mCurrentInstruction < 0)
				{
					this.mActivationTime = time;
					this.mCurrentInstruction = 0;
				}
				else if (time - this.mActivationTime > Mathf.Max(0f, this.m_Instructions[this.mCurrentInstruction].m_Hold))
				{
					this.mActivationTime = time;
					this.mCurrentInstruction++;
				}
			}
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

		[NoSaveDuringPlay]
		[Tooltip("Default object for the camera children to look at (the aim target), if not specified in a child camera.  May be empty if all of the children define targets of their own.")]
		public Transform m_LookAt;

		[Tooltip("Default object for the camera children wants to move with (the body target), if not specified in a child camera.  May be empty if all of the children define targets of their own.")]
		[NoSaveDuringPlay]
		public Transform m_Follow;

		[Tooltip("When enabled, the current child camera and blend will be indicated in the game window, for debugging")]
		public bool m_ShowDebugText;

		[Tooltip("Force all child cameras to be enabled.  This is useful if animating them in Timeline, but consumes extra resources")]
		public bool m_EnableAllChildCameras;

		[SerializeField]
		[NoSaveDuringPlay]
		[HideInInspector]
		public CinemachineVirtualCameraBase[] m_ChildCameras;

		[Tooltip("The set of instructions for enabling child cameras.")]
		public CinemachineBlendListCamera.Instruction[] m_Instructions;

		private CameraState m_State = CameraState.Default;

		private float mActivationTime = -1f;

		private int mCurrentInstruction;

		private CinemachineBlend mActiveBlend;

		[Serializable]
		public struct Instruction
		{
			[Tooltip("The virtual camera to activate when this instruction becomes active")]
			public CinemachineVirtualCameraBase m_VirtualCamera;

			[Tooltip("How long to wait (in seconds) before activating the next virtual camera in the list (if any)")]
			public float m_Hold;

			[Tooltip("How to blend to the next virtual camera in the list (if any)")]
			[CinemachineBlendDefinitionProperty]
			public CinemachineBlendDefinition m_Blend;
		}
	}
}
