using System;
using UnityEngine;

namespace Cinemachine
{
	public class CinemachineBlend
	{
		public ICinemachineCamera CamA { get; set; }

		public ICinemachineCamera CamB { get; set; }

		public AnimationCurve BlendCurve { get; set; }

		public float TimeInBlend { get; set; }

		public float BlendWeight
		{
			get
			{
				if (this.BlendCurve == null)
				{
					return 0f;
				}
				return this.BlendCurve.Evaluate(this.TimeInBlend);
			}
		}

		public bool IsValid
		{
			get
			{
				return this.CamA != null || this.CamB != null;
			}
		}

		public float Duration { get; set; }

		public bool IsComplete
		{
			get
			{
				return this.TimeInBlend >= this.Duration;
			}
		}

		public string Description
		{
			get
			{
				string arg = (this.CamA != null) ? ("[" + this.CamA.Name + "]") : "(none)";
				string arg2 = (this.CamB != null) ? ("[" + this.CamB.Name + "]") : "(none)";
				int num = (int)(this.BlendWeight * 100f);
				return string.Format("{0} {1}% from {2}", arg2, num, arg);
			}
		}

		public bool Uses(ICinemachineCamera cam)
		{
			if (cam == this.CamA || cam == this.CamB)
			{
				return true;
			}
			BlendSourceVirtualCamera blendSourceVirtualCamera = this.CamA as BlendSourceVirtualCamera;
			if (blendSourceVirtualCamera != null && blendSourceVirtualCamera.Blend.Uses(cam))
			{
				return true;
			}
			blendSourceVirtualCamera = (this.CamB as BlendSourceVirtualCamera);
			return blendSourceVirtualCamera != null && blendSourceVirtualCamera.Blend.Uses(cam);
		}

		public CinemachineBlend(ICinemachineCamera a, ICinemachineCamera b, AnimationCurve curve, float duration, float t)
		{
			if (a == null || b == null)
			{
				throw new ArgumentException("Blend cameras cannot be null");
			}
			this.CamA = a;
			this.CamB = b;
			this.BlendCurve = curve;
			this.TimeInBlend = t;
			this.Duration = duration;
		}

		public void UpdateCameraState(Vector3 worldUp, float deltaTime)
		{
			CinemachineCore.Instance.UpdateVirtualCamera(this.CamA, worldUp, deltaTime);
			CinemachineCore.Instance.UpdateVirtualCamera(this.CamB, worldUp, deltaTime);
		}

		public CameraState State
		{
			get
			{
				return CameraState.Lerp(this.CamA.State, this.CamB.State, this.BlendWeight);
			}
		}
	}
}
