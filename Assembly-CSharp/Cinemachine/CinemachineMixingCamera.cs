using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinemachine
{
	[DocumentationSorting(20f, DocumentationSortingAttribute.Level.UserRef)]
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[AddComponentMenu("Cinemachine/CinemachineMixingCamera")]
	public class CinemachineMixingCamera : CinemachineVirtualCameraBase
	{
		public float GetWeight(int index)
		{
			switch (index)
			{
			case 0:
				return this.m_Weight0;
			case 1:
				return this.m_Weight1;
			case 2:
				return this.m_Weight2;
			case 3:
				return this.m_Weight3;
			case 4:
				return this.m_Weight4;
			case 5:
				return this.m_Weight5;
			case 6:
				return this.m_Weight6;
			case 7:
				return this.m_Weight7;
			default:
				Debug.LogError("CinemachineMixingCamera: Invalid index: " + index);
				return 0f;
			}
		}

		public void SetWeight(int index, float w)
		{
			switch (index)
			{
			case 0:
				this.m_Weight0 = w;
				return;
			case 1:
				this.m_Weight1 = w;
				return;
			case 2:
				this.m_Weight2 = w;
				return;
			case 3:
				this.m_Weight3 = w;
				return;
			case 4:
				this.m_Weight4 = w;
				return;
			case 5:
				this.m_Weight5 = w;
				return;
			case 6:
				this.m_Weight6 = w;
				return;
			case 7:
				this.m_Weight7 = w;
				return;
			default:
				Debug.LogError("CinemachineMixingCamera: Invalid index: " + index);
				return;
			}
		}

		public float GetWeight(CinemachineVirtualCameraBase vcam)
		{
			int index;
			if (this.m_indexMap.TryGetValue(vcam, out index))
			{
				return this.GetWeight(index);
			}
			Debug.LogError("CinemachineMixingCamera: Invalid child: " + ((vcam != null) ? vcam.Name : "(null)"));
			return 0f;
		}

		public void SetWeight(CinemachineVirtualCameraBase vcam, float w)
		{
			int index;
			if (this.m_indexMap.TryGetValue(vcam, out index))
			{
				this.SetWeight(index, w);
				return;
			}
			Debug.LogError("CinemachineMixingCamera: Invalid child: " + ((vcam != null) ? vcam.Name : "(null)"));
		}

		private ICinemachineCamera LiveChild { get; set; }

		public override CameraState State
		{
			get
			{
				return this.m_State;
			}
		}

		public override Transform LookAt { get; set; }

		public override Transform Follow { get; set; }

		public override ICinemachineCamera LiveChildOrSelf
		{
			get
			{
				return this.LiveChild;
			}
		}

		public override void RemovePostPipelineStageHook(CinemachineVirtualCameraBase.OnPostPipelineStageDelegate d)
		{
			base.RemovePostPipelineStageHook(d);
			this.ValidateListOfChildren();
			CinemachineVirtualCameraBase[] childCameras = this.m_ChildCameras;
			for (int i = 0; i < childCameras.Length; i++)
			{
				childCameras[i].RemovePostPipelineStageHook(d);
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			this.InvalidateListOfChildren();
		}

		public void OnTransformChildrenChanged()
		{
			this.InvalidateListOfChildren();
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			for (int i = 0; i < 8; i++)
			{
				this.SetWeight(i, Mathf.Max(0f, this.GetWeight(i)));
			}
		}

		public override bool IsLiveChild(ICinemachineCamera vcam)
		{
			CinemachineVirtualCameraBase[] childCameras = this.ChildCameras;
			int num = 0;
			while (num < 8 && num < childCameras.Length)
			{
				if (childCameras[num] == vcam)
				{
					return this.GetWeight(num) > 0.0001f && childCameras[num].isActiveAndEnabled;
				}
				num++;
			}
			return false;
		}

		public CinemachineVirtualCameraBase[] ChildCameras
		{
			get
			{
				this.ValidateListOfChildren();
				return this.m_ChildCameras;
			}
		}

		protected void InvalidateListOfChildren()
		{
			this.m_ChildCameras = null;
			this.m_indexMap = null;
			this.LiveChild = null;
		}

		protected void ValidateListOfChildren()
		{
			if (this.m_ChildCameras != null)
			{
				return;
			}
			this.m_indexMap = new Dictionary<CinemachineVirtualCameraBase, int>();
			List<CinemachineVirtualCameraBase> list = new List<CinemachineVirtualCameraBase>();
			foreach (CinemachineVirtualCameraBase cinemachineVirtualCameraBase in base.GetComponentsInChildren<CinemachineVirtualCameraBase>(true))
			{
				if (cinemachineVirtualCameraBase.transform.parent == base.transform)
				{
					int count = list.Count;
					list.Add(cinemachineVirtualCameraBase);
					if (count < 8)
					{
						this.m_indexMap.Add(cinemachineVirtualCameraBase, count);
					}
				}
			}
			this.m_ChildCameras = list.ToArray();
		}

		public override void UpdateCameraState(Vector3 worldUp, float deltaTime)
		{
			CinemachineVirtualCameraBase[] childCameras = this.ChildCameras;
			this.LiveChild = null;
			float num = 0f;
			float num2 = 0f;
			int num3 = 0;
			while (num3 < 8 && num3 < childCameras.Length)
			{
				CinemachineVirtualCameraBase cinemachineVirtualCameraBase = childCameras[num3];
				if (cinemachineVirtualCameraBase.isActiveAndEnabled)
				{
					float num4 = Mathf.Max(0f, this.GetWeight(num3));
					if (num4 > 0.0001f)
					{
						num2 += num4;
						if (num2 == num4)
						{
							this.m_State = cinemachineVirtualCameraBase.State;
						}
						else
						{
							this.m_State = CameraState.Lerp(this.m_State, cinemachineVirtualCameraBase.State, num4 / num2);
						}
						if (num4 > num)
						{
							num = num4;
							this.LiveChild = cinemachineVirtualCameraBase;
						}
					}
				}
				num3++;
			}
		}

		public const int MaxCameras = 8;

		[Tooltip("The weight of the first tracked camera")]
		public float m_Weight0 = 0.5f;

		[Tooltip("The weight of the second tracked camera")]
		public float m_Weight1 = 0.5f;

		[Tooltip("The weight of the third tracked camera")]
		public float m_Weight2 = 0.5f;

		[Tooltip("The weight of the fourth tracked camera")]
		public float m_Weight3 = 0.5f;

		[Tooltip("The weight of the fifth tracked camera")]
		public float m_Weight4 = 0.5f;

		[Tooltip("The weight of the sixth tracked camera")]
		public float m_Weight5 = 0.5f;

		[Tooltip("The weight of the seventh tracked camera")]
		public float m_Weight6 = 0.5f;

		[Tooltip("The weight of the eighth tracked camera")]
		public float m_Weight7 = 0.5f;

		private CameraState m_State = CameraState.Default;

		private CinemachineVirtualCameraBase[] m_ChildCameras;

		private Dictionary<CinemachineVirtualCameraBase, int> m_indexMap;
	}
}
