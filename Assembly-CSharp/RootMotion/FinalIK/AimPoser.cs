using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class AimPoser : MonoBehaviour
	{
		public AimPoser.Pose GetPose(Vector3 localDirection)
		{
			if (this.poses.Length == 0)
			{
				return null;
			}
			for (int i = 0; i < this.poses.Length - 1; i++)
			{
				if (this.poses[i].IsInDirection(localDirection))
				{
					return this.poses[i];
				}
			}
			return this.poses[this.poses.Length - 1];
		}

		public void SetPoseActive(AimPoser.Pose pose)
		{
			for (int i = 0; i < this.poses.Length; i++)
			{
				this.poses[i].SetAngleBuffer((this.poses[i] != pose) ? 0f : this.angleBuffer);
			}
		}

		public float angleBuffer = 5f;

		public AimPoser.Pose[] poses = new AimPoser.Pose[0];

		[Serializable]
		public class Pose
		{
			public bool IsInDirection(Vector3 d)
			{
				if (this.direction == Vector3.zero)
				{
					return false;
				}
				if (this.yaw <= 0f || this.pitch <= 0f)
				{
					return false;
				}
				if (this.yaw < 180f)
				{
					Vector3 forward = new Vector3(this.direction.x, 0f, this.direction.z);
					if (forward == Vector3.zero)
					{
						forward = Vector3.forward;
					}
					Vector3 from = new Vector3(d.x, 0f, d.z);
					float num = Vector3.Angle(from, forward);
					if (num > this.yaw + this.angleBuffer)
					{
						return false;
					}
				}
				if (this.pitch >= 180f)
				{
					return true;
				}
				float num2 = Vector3.Angle(Vector3.up, this.direction);
				float num3 = Vector3.Angle(Vector3.up, d);
				return Mathf.Abs(num3 - num2) < this.pitch + this.angleBuffer;
			}

			public void SetAngleBuffer(float value)
			{
				this.angleBuffer = value;
			}

			public bool visualize = true;

			public string name;

			public Vector3 direction;

			public float yaw = 75f;

			public float pitch = 45f;

			private float angleBuffer;
		}
	}
}
