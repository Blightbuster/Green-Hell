using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[HelpURL("http://www.root-motion.com/finalikdox/html/page12.html")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Rotation Limits/Rotation Limit Hinge")]
	public class RotationLimitHinge : RotationLimit
	{
		[ContextMenu("User Manual")]
		private void OpenUserManual()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page12.html");
		}

		[ContextMenu("Scrpt Reference")]
		private void OpenScriptReference()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_rotation_limit_hinge.html");
		}

		[ContextMenu("Support Group")]
		private void SupportGroup()
		{
			Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
		}

		[ContextMenu("Asset Store Thread")]
		private void ASThread()
		{
			Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
		}

		protected override Quaternion LimitRotation(Quaternion rotation)
		{
			this.lastRotation = this.LimitHinge(rotation);
			return this.lastRotation;
		}

		private Quaternion LimitHinge(Quaternion rotation)
		{
			if (this.min == 0f && this.max == 0f && this.useLimits)
			{
				return Quaternion.AngleAxis(0f, this.axis);
			}
			Quaternion quaternion = RotationLimit.Limit1DOF(rotation, this.axis);
			if (!this.useLimits)
			{
				return quaternion;
			}
			Quaternion quaternion2 = quaternion * Quaternion.Inverse(this.lastRotation);
			float num = Quaternion.Angle(Quaternion.identity, quaternion2);
			Vector3 vector = new Vector3(this.axis.z, this.axis.x, this.axis.y);
			Vector3 rhs = Vector3.Cross(vector, this.axis);
			if (Vector3.Dot(quaternion2 * vector, rhs) > 0f)
			{
				num = -num;
			}
			this.lastAngle = Mathf.Clamp(this.lastAngle + num, this.min, this.max);
			return Quaternion.AngleAxis(this.lastAngle, this.axis);
		}

		public bool useLimits = true;

		public float min = -45f;

		public float max = 90f;

		[HideInInspector]
		public float zeroAxisDisplayOffset;

		private Quaternion lastRotation = Quaternion.identity;

		private float lastAngle;
	}
}
