using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinemachine
{
	[DocumentationSorting(23f, DocumentationSortingAttribute.Level.API)]
	public abstract class CinemachineExtension : MonoBehaviour
	{
		public CinemachineVirtualCameraBase VirtualCamera
		{
			get
			{
				if (this.m_vcamOwner == null)
				{
					this.m_vcamOwner = base.GetComponent<CinemachineVirtualCameraBase>();
				}
				return this.m_vcamOwner;
			}
		}

		protected virtual void Awake()
		{
			this.ConnectToVcam();
		}

		protected virtual void OnDestroy()
		{
			if (this.VirtualCamera != null)
			{
				this.VirtualCamera.RemovePostPipelineStageHook(new CinemachineVirtualCameraBase.OnPostPipelineStageDelegate(this.PostPipelineStageCallback));
			}
		}

		private void ConnectToVcam()
		{
			if (this.VirtualCamera == null)
			{
				Debug.LogError("CinemachineExtension requires a Cinemachine Virtual Camera component");
			}
			else
			{
				this.VirtualCamera.AddPostPipelineStageHook(new CinemachineVirtualCameraBase.OnPostPipelineStageDelegate(this.PostPipelineStageCallback));
			}
			this.mExtraState = null;
		}

		protected abstract void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime);

		protected T GetExtraState<T>(ICinemachineCamera vcam) where T : class, new()
		{
			if (this.mExtraState == null)
			{
				this.mExtraState = new Dictionary<ICinemachineCamera, object>();
			}
			object obj = null;
			if (!this.mExtraState.TryGetValue(vcam, out obj))
			{
				obj = (this.mExtraState[vcam] = Activator.CreateInstance<T>());
			}
			return obj as T;
		}

		protected List<T> GetAllExtraStates<T>() where T : class, new()
		{
			List<T> list = new List<T>();
			if (this.mExtraState != null)
			{
				foreach (KeyValuePair<ICinemachineCamera, object> keyValuePair in this.mExtraState)
				{
					list.Add(keyValuePair.Value as T);
				}
			}
			return list;
		}

		protected const float Epsilon = 0.0001f;

		private CinemachineVirtualCameraBase m_vcamOwner;

		private Dictionary<ICinemachineCamera, object> mExtraState;
	}
}
