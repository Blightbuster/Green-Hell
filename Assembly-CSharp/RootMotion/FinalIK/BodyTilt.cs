using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class BodyTilt : OffsetModifier
	{
		protected override void Start()
		{
			base.Start();
			this.lastForward = base.transform.forward;
		}

		protected override void OnModifyOffset()
		{
			Quaternion quaternion = Quaternion.FromToRotation(this.lastForward, base.transform.forward);
			float num = 0f;
			Vector3 zero = Vector3.zero;
			quaternion.ToAngleAxis(out num, out zero);
			if (zero.y > 0f)
			{
				num = -num;
			}
			num *= this.tiltSensitivity * 0.01f;
			num /= base.deltaTime;
			num = Mathf.Clamp(num, -1f, 1f);
			this.tiltAngle = Mathf.Lerp(this.tiltAngle, num, base.deltaTime * this.tiltSpeed);
			float weight = Mathf.Abs(this.tiltAngle) / 1f;
			if (this.tiltAngle < 0f)
			{
				this.poseRight.Apply(this.ik.solver, weight);
			}
			else
			{
				this.poseLeft.Apply(this.ik.solver, weight);
			}
			this.lastForward = base.transform.forward;
		}

		[Tooltip("Speed of tilting")]
		public float tiltSpeed = 6f;

		[Tooltip("Sensitivity of tilting")]
		public float tiltSensitivity = 0.07f;

		[Tooltip("The OffsetPose components")]
		public OffsetPose poseLeft;

		[Tooltip("The OffsetPose components")]
		public OffsetPose poseRight;

		private float tiltAngle;

		private Vector3 lastForward;
	}
}
