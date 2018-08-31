using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Rotation Limits/Rotation Limit Angle")]
	[HelpURL("http://www.root-motion.com/finalikdox/html/page12.html")]
	public class RotationLimitAngle : RotationLimit
	{
		[ContextMenu("User Manual")]
		private void OpenUserManual()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page12.html");
		}

		[ContextMenu("Scrpt Reference")]
		private void OpenScriptReference()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_rotation_limit_angle.html");
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
			Quaternion rotation2 = this.LimitSwing(rotation);
			return RotationLimit.LimitTwist(rotation2, this.axis, base.secondaryAxis, this.twistLimit);
		}

		private Quaternion LimitSwing(Quaternion rotation)
		{
			if (this.axis == Vector3.zero)
			{
				return rotation;
			}
			if (rotation == Quaternion.identity)
			{
				return rotation;
			}
			if (this.limit >= 180f)
			{
				return rotation;
			}
			Vector3 vector = rotation * this.axis;
			Quaternion to = Quaternion.FromToRotation(this.axis, vector);
			Quaternion rotation2 = Quaternion.RotateTowards(Quaternion.identity, to, this.limit);
			Quaternion lhs = Quaternion.FromToRotation(vector, rotation2 * this.axis);
			return lhs * rotation;
		}

		[Range(0f, 180f)]
		public float limit = 45f;

		[Range(0f, 180f)]
		public float twistLimit = 180f;
	}
}
