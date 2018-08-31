using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;

namespace Cinemachine.PostFX
{
	[DocumentationSorting(101f, DocumentationSortingAttribute.Level.UserRef)]
	[SaveDuringPlay]
	[AddComponentMenu("")]
	[ExecuteInEditMode]
	public class CinemachinePostProcessing : CinemachineExtension
	{
		public PostProcessProfile Profile
		{
			get
			{
				return (!(this.mProfileCopy != null)) ? this.m_Profile : this.mProfileCopy;
			}
		}

		public bool IsValid
		{
			get
			{
				return this.m_Profile != null && this.m_Profile.settings.Count > 0;
			}
		}

		public void InvalidateCachedProfile()
		{
			this.mCachedProfileIsInvalid = true;
		}

		private void CreateProfileCopy()
		{
			this.DestroyProfileCopy();
			PostProcessProfile postProcessProfile = ScriptableObject.CreateInstance<PostProcessProfile>();
			if (this.m_Profile != null)
			{
				foreach (PostProcessEffectSettings original in this.m_Profile.settings)
				{
					PostProcessEffectSettings item = UnityEngine.Object.Instantiate<PostProcessEffectSettings>(original);
					postProcessProfile.settings.Add(item);
				}
			}
			this.mProfileCopy = postProcessProfile;
			this.mCachedProfileIsInvalid = false;
		}

		private void DestroyProfileCopy()
		{
			if (this.mProfileCopy != null)
			{
				UnityEngine.Object.DestroyImmediate(this.mProfileCopy);
			}
			this.mProfileCopy = null;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.DestroyProfileCopy();
		}

		protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
		{
			if (stage == CinemachineCore.Stage.Aim)
			{
				if (!this.IsValid)
				{
					this.DestroyProfileCopy();
				}
				else
				{
					if (!this.m_FocusTracksTarget || !state.HasLookAt)
					{
						this.DestroyProfileCopy();
					}
					else
					{
						if (this.mProfileCopy == null || this.mCachedProfileIsInvalid)
						{
							this.CreateProfileCopy();
						}
						DepthOfField depthOfField;
						if (this.mProfileCopy.TryGetSettings<DepthOfField>(out depthOfField))
						{
							depthOfField.focusDistance.value = (state.FinalPosition - state.ReferenceLookAt).magnitude + this.m_FocusOffset;
						}
					}
					state.AddCustomBlendable(new CameraState.CustomBlendable(this, 1f));
				}
			}
		}

		private static void OnCameraCut(CinemachineBrain brain)
		{
			PostProcessLayer postProcessLayer = brain.PostProcessingComponent as PostProcessLayer;
			if (postProcessLayer == null)
			{
				brain.PostProcessingComponent = null;
			}
			else
			{
				postProcessLayer.ResetHistory();
			}
		}

		private static void ApplyPostFX(CinemachineBrain brain)
		{
			PostProcessLayer component = brain.GetComponent<PostProcessLayer>();
			if (component == null || !component.enabled || component.volumeLayer == 0)
			{
				return;
			}
			CameraState currentCameraState = brain.CurrentCameraState;
			int numCustomBlendables = currentCameraState.NumCustomBlendables;
			List<PostProcessVolume> dynamicBrainVolumes = CinemachinePostProcessing.GetDynamicBrainVolumes(brain, component, numCustomBlendables);
			for (int i = 0; i < dynamicBrainVolumes.Count; i++)
			{
				dynamicBrainVolumes[i].weight = 0f;
				dynamicBrainVolumes[i].sharedProfile = null;
				dynamicBrainVolumes[i].profile = null;
			}
			for (int j = 0; j < numCustomBlendables; j++)
			{
				CameraState.CustomBlendable customBlendable = currentCameraState.GetCustomBlendable(j);
				CinemachinePostProcessing cinemachinePostProcessing = customBlendable.m_Custom as CinemachinePostProcessing;
				if (!(cinemachinePostProcessing == null))
				{
					PostProcessVolume postProcessVolume = dynamicBrainVolumes[j];
					postProcessVolume.sharedProfile = cinemachinePostProcessing.Profile;
					postProcessVolume.isGlobal = true;
					postProcessVolume.priority = float.MaxValue;
					postProcessVolume.weight = customBlendable.m_Weight;
				}
			}
		}

		private static List<PostProcessVolume> GetDynamicBrainVolumes(CinemachineBrain brain, PostProcessLayer ppLayer, int minVolumes)
		{
			GameObject gameObject = null;
			Transform transform = brain.transform;
			int childCount = transform.childCount;
			CinemachinePostProcessing.sVolumes.Clear();
			int num = 0;
			while (gameObject == null && num < childCount)
			{
				GameObject gameObject2 = transform.GetChild(num).gameObject;
				if (gameObject2.hideFlags == HideFlags.HideAndDontSave)
				{
					gameObject2.GetComponents<PostProcessVolume>(CinemachinePostProcessing.sVolumes);
					if (CinemachinePostProcessing.sVolumes.Count > 0)
					{
						gameObject = gameObject2;
					}
				}
				num++;
			}
			if (minVolumes > 0)
			{
				if (gameObject == null)
				{
					gameObject = new GameObject(CinemachinePostProcessing.sVolumeOwnerName);
					gameObject.hideFlags = HideFlags.HideAndDontSave;
					gameObject.transform.parent = transform;
				}
				int value = ppLayer.volumeLayer.value;
				for (int i = 0; i < 32; i++)
				{
					if ((value & 1 << i) != 0)
					{
						gameObject.layer = i;
						break;
					}
				}
				while (CinemachinePostProcessing.sVolumes.Count < minVolumes)
				{
					CinemachinePostProcessing.sVolumes.Add(gameObject.gameObject.AddComponent<PostProcessVolume>());
				}
			}
			return CinemachinePostProcessing.sVolumes;
		}

		[RuntimeInitializeOnLoadMethod]
		public static void InitializeModule()
		{
			UnityEvent<CinemachineBrain> sPostProcessingHandler = CinemachineBrain.sPostProcessingHandler;
			if (CinemachinePostProcessing.<>f__mg$cache0 == null)
			{
				CinemachinePostProcessing.<>f__mg$cache0 = new UnityAction<CinemachineBrain>(CinemachinePostProcessing.StaticPostFXHandler);
			}
			sPostProcessingHandler.RemoveListener(CinemachinePostProcessing.<>f__mg$cache0);
			UnityEvent<CinemachineBrain> sPostProcessingHandler2 = CinemachineBrain.sPostProcessingHandler;
			if (CinemachinePostProcessing.<>f__mg$cache1 == null)
			{
				CinemachinePostProcessing.<>f__mg$cache1 = new UnityAction<CinemachineBrain>(CinemachinePostProcessing.StaticPostFXHandler);
			}
			sPostProcessingHandler2.AddListener(CinemachinePostProcessing.<>f__mg$cache1);
		}

		private static void StaticPostFXHandler(CinemachineBrain brain)
		{
			PostProcessLayer x = brain.PostProcessingComponent as PostProcessLayer;
			if (x == null)
			{
				brain.PostProcessingComponent = brain.GetComponent<PostProcessLayer>();
				x = (brain.PostProcessingComponent as PostProcessLayer);
				if (x != null)
				{
					UnityEvent<CinemachineBrain> cameraCutEvent = brain.m_CameraCutEvent;
					if (CinemachinePostProcessing.<>f__mg$cache2 == null)
					{
						CinemachinePostProcessing.<>f__mg$cache2 = new UnityAction<CinemachineBrain>(CinemachinePostProcessing.OnCameraCut);
					}
					cameraCutEvent.AddListener(CinemachinePostProcessing.<>f__mg$cache2);
				}
			}
			if (x != null)
			{
				CinemachinePostProcessing.ApplyPostFX(brain);
			}
		}

		[Tooltip("If checked, then the Focus Distance will be set to the distance between the camera and the LookAt target.  Requires DepthOfField effect in the Profile")]
		public bool m_FocusTracksTarget;

		[Tooltip("Offset from target distance, to be used with Focus Tracks Target.  Offsets the sharpest point away from the LookAt target.")]
		public float m_FocusOffset;

		[Tooltip("This Post-Processing profile will be applied whenever this virtual camera is live")]
		public PostProcessProfile m_Profile;

		private bool mCachedProfileIsInvalid = true;

		private PostProcessProfile mProfileCopy;

		private static string sVolumeOwnerName = "__CMVolumes";

		private static List<PostProcessVolume> sVolumes = new List<PostProcessVolume>();

		[CompilerGenerated]
		private static UnityAction<CinemachineBrain> <>f__mg$cache0;

		[CompilerGenerated]
		private static UnityAction<CinemachineBrain> <>f__mg$cache1;

		[CompilerGenerated]
		private static UnityAction<CinemachineBrain> <>f__mg$cache2;
	}
}
