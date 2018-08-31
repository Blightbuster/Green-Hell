using System;
using Cinemachine.Utility;
using UnityEngine;

namespace Cinemachine
{
	[ExecuteInEditMode]
	[DocumentationSorting(16f, DocumentationSortingAttribute.Level.UserRef)]
	[SaveDuringPlay]
	[AddComponentMenu("")]
	public class CinemachineFollowZoom : CinemachineExtension
	{
		private void OnValidate()
		{
			this.m_Width = Mathf.Max(0f, this.m_Width);
			this.m_MaxFOV = Mathf.Clamp(this.m_MaxFOV, 1f, 179f);
			this.m_MinFOV = Mathf.Clamp(this.m_MinFOV, 1f, this.m_MaxFOV);
		}

		protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
		{
			CinemachineFollowZoom.VcamExtraState extraState = base.GetExtraState<CinemachineFollowZoom.VcamExtraState>(vcam);
			if (!base.enabled || deltaTime < 0f)
			{
				extraState.m_previousFrameZoom = state.Lens.FieldOfView;
			}
			if (stage == CinemachineCore.Stage.Body)
			{
				float num = Mathf.Max(this.m_Width, 0f);
				float value = 179f;
				float num2 = Vector3.Distance(state.CorrectedPosition, state.ReferenceLookAt);
				if (num2 > 0.0001f)
				{
					float min = num2 * 2f * Mathf.Tan(this.m_MinFOV * 0.0174532924f / 2f);
					float max = num2 * 2f * Mathf.Tan(this.m_MaxFOV * 0.0174532924f / 2f);
					num = Mathf.Clamp(num, min, max);
					if (deltaTime >= 0f && this.m_Damping > 0f)
					{
						float num3 = num2 * 2f * Mathf.Tan(extraState.m_previousFrameZoom * 0.0174532924f / 2f);
						float num4 = num - num3;
						num4 = Damper.Damp(num4, this.m_Damping, deltaTime);
						num = num3 + num4;
					}
					value = 2f * Mathf.Atan(num / (2f * num2)) * 57.29578f;
				}
				LensSettings lens = state.Lens;
				lens.FieldOfView = (extraState.m_previousFrameZoom = Mathf.Clamp(value, this.m_MinFOV, this.m_MaxFOV));
				state.Lens = lens;
			}
		}

		[Tooltip("The shot width to maintain, in world units, at target distance.")]
		public float m_Width = 2f;

		[Tooltip("Increase this value to soften the aggressiveness of the follow-zoom.  Small numbers are more responsive, larger numbers give a more heavy slowly responding camera.")]
		[Range(0f, 20f)]
		public float m_Damping = 1f;

		[Range(1f, 179f)]
		[Tooltip("Lower limit for the FOV that this behaviour will generate.")]
		public float m_MinFOV = 3f;

		[Range(1f, 179f)]
		[Tooltip("Upper limit for the FOV that this behaviour will generate.")]
		public float m_MaxFOV = 60f;

		private class VcamExtraState
		{
			public float m_previousFrameZoom;
		}
	}
}
