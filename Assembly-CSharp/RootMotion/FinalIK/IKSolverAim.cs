using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKSolverAim : IKSolverHeuristic
	{
		public float GetAngle()
		{
			return Vector3.Angle(this.transformAxis, this.IKPosition - this.transform.position);
		}

		public Vector3 transformAxis
		{
			get
			{
				return this.transform.rotation * this.axis;
			}
		}

		public Vector3 transformPoleAxis
		{
			get
			{
				return this.transform.rotation * this.poleAxis;
			}
		}

		protected override void OnInitiate()
		{
			if ((this.firstInitiation || !Application.isPlaying) && this.transform != null)
			{
				this.IKPosition = this.transform.position + this.transformAxis * 3f;
				this.polePosition = this.transform.position + this.transformPoleAxis * 3f;
			}
			for (int i = 0; i < this.bones.Length; i++)
			{
				if (this.bones[i].rotationLimit != null)
				{
					this.bones[i].rotationLimit.Disable();
				}
			}
			this.step = 1f / (float)this.bones.Length;
			if (Application.isPlaying)
			{
				this.axis = this.axis.normalized;
			}
		}

		protected override void OnUpdate()
		{
			if (this.axis == Vector3.zero)
			{
				if (!Warning.logged)
				{
					base.LogWarning("IKSolverAim axis is Vector3.zero.");
				}
				return;
			}
			if (this.poleAxis == Vector3.zero && this.poleWeight > 0f)
			{
				if (!Warning.logged)
				{
					base.LogWarning("IKSolverAim poleAxis is Vector3.zero.");
				}
				return;
			}
			if (this.target != null)
			{
				this.IKPosition = this.target.position;
			}
			if (this.poleTarget != null)
			{
				this.polePosition = this.poleTarget.position;
			}
			if (this.XY)
			{
				this.IKPosition.z = this.bones[0].transform.position.z;
			}
			if (this.IKPositionWeight <= 0f)
			{
				return;
			}
			this.IKPositionWeight = Mathf.Clamp(this.IKPositionWeight, 0f, 1f);
			if (this.transform != this.lastTransform)
			{
				this.transformLimit = this.transform.GetComponent<RotationLimit>();
				if (this.transformLimit != null)
				{
					this.transformLimit.enabled = false;
				}
				this.lastTransform = this.transform;
			}
			if (this.transformLimit != null)
			{
				this.transformLimit.Apply();
			}
			if (this.transform == null)
			{
				if (!Warning.logged)
				{
					base.LogWarning("Aim Transform unassigned in Aim IK solver. Please Assign a Transform (lineal descendant to the last bone in the spine) that you want to be aimed at IKPosition");
				}
				return;
			}
			this.clampWeight = Mathf.Clamp(this.clampWeight, 0f, 1f);
			this.clampedIKPosition = this.GetClampedIKPosition();
			Vector3 b = this.clampedIKPosition - this.transform.position;
			b = Vector3.Slerp(this.transformAxis * b.magnitude, b, this.IKPositionWeight);
			this.clampedIKPosition = this.transform.position + b;
			for (int i = 0; i < this.maxIterations; i++)
			{
				if (i >= 1 && this.tolerance > 0f && this.GetAngle() < this.tolerance)
				{
					break;
				}
				this.lastLocalDirection = this.localDirection;
				if (this.OnPreIteration != null)
				{
					this.OnPreIteration(i);
				}
				this.Solve();
			}
			this.lastLocalDirection = this.localDirection;
		}

		protected override int minBones
		{
			get
			{
				return 1;
			}
		}

		private void Solve()
		{
			for (int i = 0; i < this.bones.Length - 1; i++)
			{
				this.RotateToTarget(this.clampedIKPosition, this.bones[i], this.step * (float)(i + 1) * this.IKPositionWeight * this.bones[i].weight);
			}
			this.RotateToTarget(this.clampedIKPosition, this.bones[this.bones.Length - 1], this.IKPositionWeight * this.bones[this.bones.Length - 1].weight);
		}

		private Vector3 GetClampedIKPosition()
		{
			if (this.clampWeight <= 0f)
			{
				return this.IKPosition;
			}
			if (this.clampWeight >= 1f)
			{
				return this.transform.position + this.transformAxis * (this.IKPosition - this.transform.position).magnitude;
			}
			float num = Vector3.Angle(this.transformAxis, this.IKPosition - this.transform.position);
			float num2 = 1f - num / 180f;
			float num3 = (this.clampWeight <= 0f) ? 1f : Mathf.Clamp(1f - (this.clampWeight - num2) / (1f - num2), 0f, 1f);
			float num4 = (this.clampWeight <= 0f) ? 1f : Mathf.Clamp(num2 / this.clampWeight, 0f, 1f);
			for (int i = 0; i < this.clampSmoothing; i++)
			{
				float f = num4 * 3.14159274f * 0.5f;
				num4 = Mathf.Sin(f);
			}
			return this.transform.position + Vector3.Slerp(this.transformAxis * 10f, this.IKPosition - this.transform.position, num4 * num3);
		}

		private void RotateToTarget(Vector3 targetPosition, IKSolver.Bone bone, float weight)
		{
			if (this.XY)
			{
				if (weight >= 0f)
				{
					Vector3 transformAxis = this.transformAxis;
					Vector3 vector = targetPosition - this.transform.position;
					float current = Mathf.Atan2(transformAxis.x, transformAxis.y) * 57.29578f;
					float target = Mathf.Atan2(vector.x, vector.y) * 57.29578f;
					bone.transform.rotation = Quaternion.AngleAxis(Mathf.DeltaAngle(current, target), Vector3.back) * bone.transform.rotation;
				}
			}
			else
			{
				if (weight >= 0f)
				{
					Quaternion quaternion = Quaternion.FromToRotation(this.transformAxis, targetPosition - this.transform.position);
					if (weight >= 1f)
					{
						bone.transform.rotation = quaternion * bone.transform.rotation;
					}
					else
					{
						bone.transform.rotation = Quaternion.Lerp(Quaternion.identity, quaternion, weight) * bone.transform.rotation;
					}
				}
				if (this.poleWeight > 0f)
				{
					Vector3 vector2 = this.polePosition - this.transform.position;
					Vector3 toDirection = vector2;
					Vector3 transformAxis2 = this.transformAxis;
					Vector3.OrthoNormalize(ref transformAxis2, ref toDirection);
					Quaternion b = Quaternion.FromToRotation(this.transformPoleAxis, toDirection);
					bone.transform.rotation = Quaternion.Lerp(Quaternion.identity, b, weight * this.poleWeight) * bone.transform.rotation;
				}
			}
			if (this.useRotationLimits && bone.rotationLimit != null)
			{
				bone.rotationLimit.Apply();
			}
		}

		protected override Vector3 localDirection
		{
			get
			{
				return this.bones[0].transform.InverseTransformDirection(this.bones[this.bones.Length - 1].transform.forward);
			}
		}

		public Transform transform;

		public Vector3 axis = Vector3.forward;

		public Vector3 poleAxis = Vector3.up;

		public Vector3 polePosition;

		[Range(0f, 1f)]
		public float poleWeight;

		public Transform poleTarget;

		[Range(0f, 1f)]
		public float clampWeight = 0.1f;

		[Range(0f, 2f)]
		public int clampSmoothing = 2;

		public IKSolver.IterationDelegate OnPreIteration;

		private float step;

		private Vector3 clampedIKPosition;

		private RotationLimit transformLimit;

		private Transform lastTransform;
	}
}
