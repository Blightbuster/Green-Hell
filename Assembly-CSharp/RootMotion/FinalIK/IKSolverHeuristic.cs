using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKSolverHeuristic : IKSolver
	{
		public bool SetChain(Transform[] hierarchy, Transform root)
		{
			if (this.bones == null || this.bones.Length != hierarchy.Length)
			{
				this.bones = new IKSolver.Bone[hierarchy.Length];
			}
			for (int i = 0; i < hierarchy.Length; i++)
			{
				if (this.bones[i] == null)
				{
					this.bones[i] = new IKSolver.Bone();
				}
				this.bones[i].transform = hierarchy[i];
			}
			base.Initiate(root);
			return base.initiated;
		}

		public void AddBone(Transform bone)
		{
			Transform[] array = new Transform[this.bones.Length + 1];
			for (int i = 0; i < this.bones.Length; i++)
			{
				array[i] = this.bones[i].transform;
			}
			array[array.Length - 1] = bone;
			this.SetChain(array, this.root);
		}

		public override void StoreDefaultLocalState()
		{
			for (int i = 0; i < this.bones.Length; i++)
			{
				this.bones[i].StoreDefaultLocalState();
			}
		}

		public override void FixTransforms()
		{
			if (this.IKPositionWeight <= 0f)
			{
				return;
			}
			for (int i = 0; i < this.bones.Length; i++)
			{
				this.bones[i].FixTransform();
			}
		}

		public override bool IsValid(ref string message)
		{
			if (this.bones.Length == 0)
			{
				message = "IK chain has no Bones.";
				return false;
			}
			if (this.bones.Length < this.minBones)
			{
				message = "IK chain has less than " + this.minBones + " Bones.";
				return false;
			}
			IKSolver.Bone[] array = this.bones;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].transform == null)
				{
					message = "One of the Bones is null.";
					return false;
				}
			}
			Transform transform = IKSolver.ContainsDuplicateBone(this.bones);
			if (transform != null)
			{
				message = transform.name + " is represented multiple times in the Bones.";
				return false;
			}
			if (!this.allowCommonParent && !IKSolver.HierarchyIsValid(this.bones))
			{
				message = "Invalid bone hierarchy detected. IK requires for it's bones to be parented to each other in descending order.";
				return false;
			}
			if (!this.boneLengthCanBeZero)
			{
				for (int j = 0; j < this.bones.Length - 1; j++)
				{
					if ((this.bones[j].transform.position - this.bones[j + 1].transform.position).magnitude == 0f)
					{
						message = "Bone " + j + " length is zero.";
						return false;
					}
				}
			}
			return true;
		}

		public override IKSolver.Point[] GetPoints()
		{
			return this.bones;
		}

		public override IKSolver.Point GetPoint(Transform transform)
		{
			for (int i = 0; i < this.bones.Length; i++)
			{
				if (this.bones[i].transform == transform)
				{
					return this.bones[i];
				}
			}
			return null;
		}

		protected virtual int minBones
		{
			get
			{
				return 2;
			}
		}

		protected virtual bool boneLengthCanBeZero
		{
			get
			{
				return true;
			}
		}

		protected virtual bool allowCommonParent
		{
			get
			{
				return false;
			}
		}

		protected override void OnInitiate()
		{
		}

		protected override void OnUpdate()
		{
		}

		protected void InitiateBones()
		{
			this.chainLength = 0f;
			for (int i = 0; i < this.bones.Length; i++)
			{
				if (i < this.bones.Length - 1)
				{
					this.bones[i].length = (this.bones[i].transform.position - this.bones[i + 1].transform.position).magnitude;
					this.chainLength += this.bones[i].length;
					Vector3 position = this.bones[i + 1].transform.position;
					this.bones[i].axis = Quaternion.Inverse(this.bones[i].transform.rotation) * (position - this.bones[i].transform.position);
					if (this.bones[i].rotationLimit != null)
					{
						if (this.XY && !(this.bones[i].rotationLimit is RotationLimitHinge))
						{
							Warning.Log("Only Hinge Rotation Limits should be used on 2D IK solvers.", this.bones[i].transform, false);
						}
						this.bones[i].rotationLimit.Disable();
					}
				}
				else
				{
					this.bones[i].axis = Quaternion.Inverse(this.bones[i].transform.rotation) * (this.bones[this.bones.Length - 1].transform.position - this.bones[0].transform.position);
				}
			}
		}

		protected virtual Vector3 localDirection
		{
			get
			{
				return this.bones[0].transform.InverseTransformDirection(this.bones[this.bones.Length - 1].transform.position - this.bones[0].transform.position);
			}
		}

		protected float positionOffset
		{
			get
			{
				return Vector3.SqrMagnitude(this.localDirection - this.lastLocalDirection);
			}
		}

		protected Vector3 GetSingularityOffset()
		{
			if (!this.SingularityDetected())
			{
				return Vector3.zero;
			}
			Vector3 normalized = (this.IKPosition - this.bones[0].transform.position).normalized;
			Vector3 rhs = new Vector3(normalized.y, normalized.z, normalized.x);
			if (this.useRotationLimits && this.bones[this.bones.Length - 2].rotationLimit != null && this.bones[this.bones.Length - 2].rotationLimit is RotationLimitHinge)
			{
				rhs = this.bones[this.bones.Length - 2].transform.rotation * this.bones[this.bones.Length - 2].rotationLimit.axis;
			}
			return Vector3.Cross(normalized, rhs) * this.bones[this.bones.Length - 2].length * 0.5f;
		}

		private bool SingularityDetected()
		{
			if (!base.initiated)
			{
				return false;
			}
			Vector3 a = this.bones[this.bones.Length - 1].transform.position - this.bones[0].transform.position;
			Vector3 a2 = this.IKPosition - this.bones[0].transform.position;
			float magnitude = a.magnitude;
			float magnitude2 = a2.magnitude;
			return magnitude >= magnitude2 && magnitude >= this.chainLength - this.bones[this.bones.Length - 2].length * 0.1f && magnitude != 0f && magnitude2 != 0f && magnitude2 <= magnitude && Vector3.Dot(a / magnitude, a2 / magnitude2) >= 0.999f;
		}

		public Transform target;

		public float tolerance;

		public int maxIterations = 4;

		public bool useRotationLimits = true;

		public bool XY;

		public IKSolver.Bone[] bones = new IKSolver.Bone[0];

		protected Vector3 lastLocalDirection;

		protected float chainLength;
	}
}
