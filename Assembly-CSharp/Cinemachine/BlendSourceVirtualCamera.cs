using System;
using UnityEngine;

namespace Cinemachine
{
	internal class BlendSourceVirtualCamera : ICinemachineCamera
	{
		public BlendSourceVirtualCamera(CinemachineBlend blend, float deltaTime)
		{
			this.Blend = blend;
			this.UpdateCameraState(blend.CamA.State.ReferenceUp, deltaTime);
		}

		public CinemachineBlend Blend { get; private set; }

		public string Name
		{
			get
			{
				return "Blend";
			}
		}

		public string Description
		{
			get
			{
				return this.Blend.Description;
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
				return this.Blend.CamB;
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
			return vcam == this.Blend.CamA || vcam == this.Blend.CamB;
		}

		public CameraState CalculateNewState(float deltaTime)
		{
			return this.State;
		}

		public void UpdateCameraState(Vector3 worldUp, float deltaTime)
		{
			this.Blend.UpdateCameraState(worldUp, deltaTime);
			this.State = this.Blend.State;
		}

		public void OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
		{
		}
	}
}
