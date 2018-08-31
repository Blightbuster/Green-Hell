using System;
using UnityEngine;

namespace Cinemachine
{
	public interface ICinemachineCamera
	{
		string Name { get; }

		string Description { get; }

		int Priority { get; set; }

		Transform LookAt { get; set; }

		Transform Follow { get; set; }

		CameraState State { get; }

		GameObject VirtualCameraGameObject { get; }

		ICinemachineCamera LiveChildOrSelf { get; }

		ICinemachineCamera ParentCamera { get; }

		bool IsLiveChild(ICinemachineCamera vcam);

		void UpdateCameraState(Vector3 worldUp, float deltaTime);

		void OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime);
	}
}
