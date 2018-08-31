using System;
using UnityEngine;

namespace Cinemachine
{
	[DocumentationSorting(24f, DocumentationSortingAttribute.Level.API)]
	public abstract class CinemachineComponentBase : MonoBehaviour
	{
		public CinemachineVirtualCameraBase VirtualCamera
		{
			get
			{
				if (this.m_vcamOwner == null)
				{
					this.m_vcamOwner = base.gameObject.transform.parent.gameObject.GetComponent<CinemachineVirtualCameraBase>();
				}
				return this.m_vcamOwner;
			}
		}

		public Transform FollowTarget
		{
			get
			{
				CinemachineVirtualCameraBase virtualCamera = this.VirtualCamera;
				return (!(virtualCamera == null)) ? virtualCamera.Follow : null;
			}
		}

		public Transform LookAtTarget
		{
			get
			{
				CinemachineVirtualCameraBase virtualCamera = this.VirtualCamera;
				return (!(virtualCamera == null)) ? virtualCamera.LookAt : null;
			}
		}

		public CameraState VcamState
		{
			get
			{
				CinemachineVirtualCameraBase virtualCamera = this.VirtualCamera;
				return (!(virtualCamera == null)) ? virtualCamera.State : CameraState.Default;
			}
		}

		public abstract bool IsValid { get; }

		public virtual void PrePipelineMutateCameraState(ref CameraState state)
		{
		}

		public abstract CinemachineCore.Stage Stage { get; }

		public abstract void MutateCameraState(ref CameraState curState, float deltaTime);

		public virtual void OnPositionDragged(Vector3 delta)
		{
		}

		protected const float Epsilon = 0.0001f;

		private CinemachineVirtualCameraBase m_vcamOwner;
	}
}
