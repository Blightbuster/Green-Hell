using System;
using UnityEngine;

namespace Cinemachine
{
	internal class StaticPointVirtualCamera : ICinemachineCamera
	{
		public StaticPointVirtualCamera(CameraState state, string name)
		{
			this.State = state;
			this.Name = name;
		}

		public void SetState(CameraState state)
		{
			this.State = state;
		}

		public string Name { get; private set; }

		public string Description
		{
			get
			{
				return "";
			}
		}

		public int Priority { get; set; }

		public Transform LookAt { get; set; }

		public Transform Follow { get; set; }

		public CameraState State { get; private set; }

		public GameObject VirtualCameraGameObject
		{
			get
			{
				return null;
			}
		}

		public ICinemachineCamera LiveChildOrSelf
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
				return null;
			}
		}

		public bool IsLiveChild(ICinemachineCamera vcam)
		{
			return false;
		}

		public void UpdateCameraState(Vector3 worldUp, float deltaTime)
		{
		}

		public void OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
		{
		}
	}
}
