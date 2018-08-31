using System;
using Cinemachine.Utility;
using UnityEngine;

namespace Cinemachine
{
	[RequireComponent(typeof(Camera))]
	[DocumentationSorting(14f, DocumentationSortingAttribute.Level.UserRef)]
	[DisallowMultipleComponent]
	[ExecuteInEditMode]
	[AddComponentMenu("Cinemachine/CinemachineExternalCamera")]
	public class CinemachineExternalCamera : CinemachineVirtualCameraBase
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
				return this.m_LookAt;
			}
			set
			{
				this.m_LookAt = value;
			}
		}

		public override Transform Follow { get; set; }

		public override void UpdateCameraState(Vector3 worldUp, float deltaTime)
		{
			if (this.m_Camera == null)
			{
				this.m_Camera = base.GetComponent<Camera>();
			}
			this.m_State = CameraState.Default;
			this.m_State.RawPosition = base.transform.position;
			this.m_State.RawOrientation = base.transform.rotation;
			this.m_State.ReferenceUp = this.m_State.RawOrientation * Vector3.up;
			if (this.m_Camera != null)
			{
				this.m_State.Lens = LensSettings.FromCamera(this.m_Camera);
			}
			if (this.m_LookAt != null)
			{
				this.m_State.ReferenceLookAt = this.m_LookAt.transform.position;
				Vector3 vector = this.m_State.ReferenceLookAt - this.State.RawPosition;
				if (!vector.AlmostZero())
				{
					this.m_State.ReferenceLookAt = this.m_State.RawPosition + Vector3.Project(vector, this.State.RawOrientation * Vector3.forward);
				}
			}
		}

		[NoSaveDuringPlay]
		[Tooltip("The object that the camera is looking at.  Setting this will improve the quality of the blends to and from this camera")]
		public Transform m_LookAt;

		private Camera m_Camera;

		private CameraState m_State = CameraState.Default;
	}
}
