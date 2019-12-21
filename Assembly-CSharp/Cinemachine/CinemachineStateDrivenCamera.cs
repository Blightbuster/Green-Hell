using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinemachine
{
	[DocumentationSorting(13f, DocumentationSortingAttribute.Level.UserRef)]
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[AddComponentMenu("Cinemachine/CinemachineStateDrivenCamera")]
	public class CinemachineStateDrivenCamera : CinemachineVirtualCameraBase
	{
		public override string Description
		{
			get
			{
				ICinemachineCamera liveChild = this.LiveChild;
				if (this.mActiveBlend != null)
				{
					return this.mActiveBlend.Description;
				}
				if (liveChild == null)
				{
					return "(none)";
				}
				return "[" + liveChild.Name + "]";
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
			CinemachineVirtualCameraBase[] childCameras = this.m_ChildCameras;
			for (int i = 0; i < childCameras.Length; i++)
			{
				childCameras[i].RemovePostPipelineStageHook(d);
			}
		}

		public override void UpdateCameraState(Vector3 worldUp, float deltaTime)
		{
			if (!base.PreviousStateIsValid)
			{
				deltaTime = -1f;
			}
			this.UpdateListOfChildren();
			CinemachineVirtualCameraBase cinemachineVirtualCameraBase = this.ChooseCurrentCamera(deltaTime);
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
			ICinemachineCamera liveChild = this.LiveChild;
			this.LiveChild = cinemachineVirtualCameraBase;
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
				this.mActiveBlend.TimeInBlend += ((deltaTime >= 0f) ? deltaTime : this.mActiveBlend.Duration);
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

		public static string CreateFakeHashName(int parentHash, string stateName)
		{
			return parentHash.ToString() + "_" + stateName;
		}

		private void InvalidateListOfChildren()
		{
			this.m_ChildCameras = null;
			this.LiveChild = null;
		}

		private void UpdateListOfChildren()
		{
			if (this.m_ChildCameras != null && this.mInstructionDictionary != null && this.mStateParentLookup != null)
			{
				return;
			}
			List<CinemachineVirtualCameraBase> list = new List<CinemachineVirtualCameraBase>();
			foreach (CinemachineVirtualCameraBase cinemachineVirtualCameraBase in base.GetComponentsInChildren<CinemachineVirtualCameraBase>(true))
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
				this.m_Instructions = new CinemachineStateDrivenCamera.Instruction[0];
			}
			this.mInstructionDictionary = new Dictionary<int, int>();
			for (int i = 0; i < this.m_Instructions.Length; i++)
			{
				if (this.m_Instructions[i].m_VirtualCamera != null && this.m_Instructions[i].m_VirtualCamera.transform.parent != base.transform)
				{
					this.m_Instructions[i].m_VirtualCamera = null;
				}
				this.mInstructionDictionary[this.m_Instructions[i].m_FullHash] = i;
			}
			this.mStateParentLookup = new Dictionary<int, int>();
			if (this.m_ParentHash != null)
			{
				foreach (CinemachineStateDrivenCamera.ParentHash parentHash2 in this.m_ParentHash)
				{
					this.mStateParentLookup[parentHash2.m_Hash] = parentHash2.m_ParentHash;
				}
			}
			this.mActivationTime = (this.mPendingActivationTime = 0f);
			this.mActiveBlend = null;
		}

		private CinemachineVirtualCameraBase ChooseCurrentCamera(float deltaTime)
		{
			if (this.m_ChildCameras == null || this.m_ChildCameras.Length == 0)
			{
				this.mActivationTime = 0f;
				return null;
			}
			CinemachineVirtualCameraBase cinemachineVirtualCameraBase = this.m_ChildCameras[0];
			if (this.m_AnimatedTarget == null || !this.m_AnimatedTarget.gameObject.activeSelf || this.m_AnimatedTarget.runtimeAnimatorController == null || this.m_LayerIndex < 0 || this.m_LayerIndex >= this.m_AnimatedTarget.layerCount)
			{
				this.mActivationTime = 0f;
				return cinemachineVirtualCameraBase;
			}
			int num;
			if (this.m_AnimatedTarget.IsInTransition(this.m_LayerIndex))
			{
				AnimatorStateInfo nextAnimatorStateInfo = this.m_AnimatedTarget.GetNextAnimatorStateInfo(this.m_LayerIndex);
				num = nextAnimatorStateInfo.fullPathHash;
				if (this.m_AnimatedTarget.GetNextAnimatorClipInfoCount(this.m_LayerIndex) > 1)
				{
					this.m_AnimatedTarget.GetNextAnimatorClipInfo(this.m_LayerIndex, this.m_clipInfoList);
					num = this.GetClipHash(nextAnimatorStateInfo.fullPathHash, this.m_clipInfoList);
				}
			}
			else
			{
				AnimatorStateInfo currentAnimatorStateInfo = this.m_AnimatedTarget.GetCurrentAnimatorStateInfo(this.m_LayerIndex);
				num = currentAnimatorStateInfo.fullPathHash;
				if (this.m_AnimatedTarget.GetCurrentAnimatorClipInfoCount(this.m_LayerIndex) > 1)
				{
					this.m_AnimatedTarget.GetCurrentAnimatorClipInfo(this.m_LayerIndex, this.m_clipInfoList);
					num = this.GetClipHash(currentAnimatorStateInfo.fullPathHash, this.m_clipInfoList);
				}
			}
			while (num != 0 && !this.mInstructionDictionary.ContainsKey(num))
			{
				num = (this.mStateParentLookup.ContainsKey(num) ? this.mStateParentLookup[num] : 0);
			}
			float time = Time.time;
			if (this.mActivationTime != 0f)
			{
				if (this.mActiveInstruction.m_FullHash == num)
				{
					this.mPendingActivationTime = 0f;
					return this.mActiveInstruction.m_VirtualCamera;
				}
				if (deltaTime >= 0f && this.mPendingActivationTime != 0f && this.mPendingInstruction.m_FullHash == num)
				{
					if (time - this.mPendingActivationTime > this.mPendingInstruction.m_ActivateAfter && (time - this.mActivationTime > this.mActiveInstruction.m_MinDuration || this.mPendingInstruction.m_VirtualCamera.Priority > this.mActiveInstruction.m_VirtualCamera.Priority))
					{
						this.mActiveInstruction = this.mPendingInstruction;
						this.mActivationTime = time;
						this.mPendingActivationTime = 0f;
					}
					return this.mActiveInstruction.m_VirtualCamera;
				}
			}
			this.mPendingActivationTime = 0f;
			if (!this.mInstructionDictionary.ContainsKey(num))
			{
				if (this.mActivationTime != 0f)
				{
					return this.mActiveInstruction.m_VirtualCamera;
				}
				return cinemachineVirtualCameraBase;
			}
			else
			{
				CinemachineStateDrivenCamera.Instruction instruction = this.m_Instructions[this.mInstructionDictionary[num]];
				if (instruction.m_VirtualCamera == null)
				{
					instruction.m_VirtualCamera = cinemachineVirtualCameraBase;
				}
				if (deltaTime < 0f || this.mActivationTime <= 0f || (instruction.m_ActivateAfter <= 0f && (time - this.mActivationTime >= this.mActiveInstruction.m_MinDuration || instruction.m_VirtualCamera.Priority > this.mActiveInstruction.m_VirtualCamera.Priority)))
				{
					this.mActiveInstruction = instruction;
					this.mActivationTime = time;
					return this.mActiveInstruction.m_VirtualCamera;
				}
				this.mPendingInstruction = instruction;
				this.mPendingActivationTime = time;
				if (this.mActivationTime != 0f)
				{
					return this.mActiveInstruction.m_VirtualCamera;
				}
				return cinemachineVirtualCameraBase;
			}
		}

		private int GetClipHash(int hash, List<AnimatorClipInfo> clips)
		{
			if (clips.Count > 1)
			{
				int num = -1;
				for (int i = 0; i < clips.Count; i++)
				{
					if (num < 0 || clips[i].weight > clips[num].weight)
					{
						num = i;
					}
				}
				if (num >= 0 && clips[num].weight > 0f)
				{
					hash = Animator.StringToHash(CinemachineStateDrivenCamera.CreateFakeHashName(hash, clips[num].clip.name));
				}
			}
			return hash;
		}

		private AnimationCurve LookupBlendCurve(ICinemachineCamera fromKey, ICinemachineCamera toKey, out float duration)
		{
			AnimationCurve animationCurve = this.m_DefaultBlend.BlendCurve;
			if (this.m_CustomBlends != null)
			{
				string fromCameraName = (fromKey != null) ? fromKey.Name : string.Empty;
				string toCameraName = (toKey != null) ? toKey.Name : string.Empty;
				animationCurve = this.m_CustomBlends.GetBlendCurveForVirtualCameras(fromCameraName, toCameraName, animationCurve);
			}
			Keyframe[] keys = animationCurve.keys;
			duration = ((keys == null || keys.Length == 0) ? 0f : keys[keys.Length - 1].time);
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
				camA = new StaticPointVirtualCamera((activeBlend != null) ? activeBlend.State : this.State, (activeBlend != null) ? "Mid-blend" : "(none)");
			}
			return new CinemachineBlend(camA, camB, blendCurve, duration, 0f);
		}

		[Tooltip("Default object for the camera children to look at (the aim target), if not specified in a child camera.  May be empty if all of the children define targets of their own.")]
		[NoSaveDuringPlay]
		public Transform m_LookAt;

		[Tooltip("Default object for the camera children wants to move with (the body target), if not specified in a child camera.  May be empty if all of the children define targets of their own.")]
		[NoSaveDuringPlay]
		public Transform m_Follow;

		[Space]
		[Tooltip("The state machine whose state changes will drive this camera's choice of active child")]
		public Animator m_AnimatedTarget;

		[Tooltip("Which layer in the target state machine to observe")]
		public int m_LayerIndex;

		[Tooltip("When enabled, the current child camera and blend will be indicated in the game window, for debugging")]
		public bool m_ShowDebugText;

		[Tooltip("Force all child cameras to be enabled.  This is useful if animating them in Timeline, but consumes extra resources")]
		public bool m_EnableAllChildCameras;

		[SerializeField]
		[HideInInspector]
		[NoSaveDuringPlay]
		public CinemachineVirtualCameraBase[] m_ChildCameras;

		[Tooltip("The set of instructions associating virtual cameras with states.  These instructions are used to choose the live child at any given moment")]
		public CinemachineStateDrivenCamera.Instruction[] m_Instructions;

		[CinemachineBlendDefinitionProperty]
		[Tooltip("The blend which is used if you don't explicitly define a blend between two Virtual Camera children")]
		public CinemachineBlendDefinition m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 0.5f);

		[Tooltip("This is the asset which contains custom settings for specific child blends")]
		public CinemachineBlenderSettings m_CustomBlends;

		[HideInInspector]
		[SerializeField]
		public CinemachineStateDrivenCamera.ParentHash[] m_ParentHash;

		private CameraState m_State = CameraState.Default;

		private float mActivationTime;

		private CinemachineStateDrivenCamera.Instruction mActiveInstruction;

		private float mPendingActivationTime;

		private CinemachineStateDrivenCamera.Instruction mPendingInstruction;

		private CinemachineBlend mActiveBlend;

		private Dictionary<int, int> mInstructionDictionary;

		private Dictionary<int, int> mStateParentLookup;

		private List<AnimatorClipInfo> m_clipInfoList = new List<AnimatorClipInfo>();

		[Serializable]
		public struct Instruction
		{
			[Tooltip("The full hash of the animation state")]
			public int m_FullHash;

			[Tooltip("The virtual camera to activate whrn the animation state becomes active")]
			public CinemachineVirtualCameraBase m_VirtualCamera;

			[Tooltip("How long to wait (in seconds) before activating the virtual camera. This filters out very short state durations")]
			public float m_ActivateAfter;

			[Tooltip("The minimum length of time (in seconds) to keep a virtual camera active")]
			public float m_MinDuration;
		}

		[DocumentationSorting(13.2f, DocumentationSortingAttribute.Level.Undoc)]
		[Serializable]
		public struct ParentHash
		{
			public ParentHash(int h, int p)
			{
				this.m_Hash = h;
				this.m_ParentHash = p;
			}

			public int m_Hash;

			public int m_ParentHash;
		}
	}
}
