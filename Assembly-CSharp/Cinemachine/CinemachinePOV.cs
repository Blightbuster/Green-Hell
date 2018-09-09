using System;
using UnityEngine;

namespace Cinemachine
{
	[AddComponentMenu("")]
	[DocumentationSorting(23f, DocumentationSortingAttribute.Level.UserRef)]
	[SaveDuringPlay]
	[RequireComponent(typeof(CinemachinePipeline))]
	public class CinemachinePOV : CinemachineComponentBase
	{
		public override bool IsValid
		{
			get
			{
				return base.enabled;
			}
		}

		public override CinemachineCore.Stage Stage
		{
			get
			{
				return CinemachineCore.Stage.Aim;
			}
		}

		private void OnValidate()
		{
			this.m_HorizontalAxis.Validate();
			this.m_VerticalAxis.Validate();
		}

		private void OnEnable()
		{
			this.m_HorizontalAxis.SetThresholds(-180f, 180f, true);
			this.m_VerticalAxis.SetThresholds(-90f, 90f, false);
		}

		public override void MutateCameraState(ref CameraState curState, float deltaTime)
		{
			if (!this.IsValid)
			{
				return;
			}
			if (deltaTime >= 0f || CinemachineCore.Instance.IsLive(base.VirtualCamera))
			{
				this.m_HorizontalAxis.Update(deltaTime);
				this.m_VerticalAxis.Update(deltaTime);
			}
			Quaternion quaternion = Quaternion.Euler(this.m_VerticalAxis.Value, this.m_HorizontalAxis.Value, 0f);
			quaternion *= Quaternion.FromToRotation(Vector3.up, curState.ReferenceUp);
			curState.OrientationCorrection *= quaternion;
		}

		[Tooltip("The Vertical axis.  Value is -90..90. Controls the vertical orientation")]
		public AxisState m_VerticalAxis = new AxisState(300f, 0.1f, 0.1f, 0f, "Mouse Y", true);

		[Tooltip("The Horizontal axis.  Value is -180..180.  Controls the horizontal orientation")]
		public AxisState m_HorizontalAxis = new AxisState(300f, 0.1f, 0.1f, 0f, "Mouse X", false);
	}
}
