using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKConstraintBend
	{
		public bool IsValid(IKSolverFullBody solver, Warning.Logger logger)
		{
			if (this.bone1 == null || this.bone2 == null || this.bone3 == null)
			{
				if (logger != null)
				{
					logger("Bend Constraint contains a null reference.");
				}
				return false;
			}
			if (solver.GetPoint(this.bone1) == null)
			{
				if (logger != null)
				{
					logger("Bend Constraint is referencing to a bone '" + this.bone1.name + "' that does not excist in the Node Chain.");
				}
				return false;
			}
			if (solver.GetPoint(this.bone2) == null)
			{
				if (logger != null)
				{
					logger("Bend Constraint is referencing to a bone '" + this.bone2.name + "' that does not excist in the Node Chain.");
				}
				return false;
			}
			if (solver.GetPoint(this.bone3) == null)
			{
				if (logger != null)
				{
					logger("Bend Constraint is referencing to a bone '" + this.bone3.name + "' that does not excist in the Node Chain.");
				}
				return false;
			}
			return true;
		}

		public bool initiated { get; private set; }

		public IKConstraintBend()
		{
		}

		public IKConstraintBend(Transform bone1, Transform bone2, Transform bone3)
		{
			this.SetBones(bone1, bone2, bone3);
		}

		public void SetBones(Transform bone1, Transform bone2, Transform bone3)
		{
			this.bone1 = bone1;
			this.bone2 = bone2;
			this.bone3 = bone3;
		}

		public void Initiate(IKSolverFullBody solver)
		{
			solver.GetChainAndNodeIndexes(this.bone1, out this.chainIndex1, out this.nodeIndex1);
			solver.GetChainAndNodeIndexes(this.bone2, out this.chainIndex2, out this.nodeIndex2);
			solver.GetChainAndNodeIndexes(this.bone3, out this.chainIndex3, out this.nodeIndex3);
			this.direction = this.OrthoToBone1(solver, this.OrthoToLimb(solver, this.bone2.position - this.bone1.position));
			this.defaultLocalDirection = Quaternion.Inverse(this.bone1.rotation) * this.direction;
			Vector3 point = Vector3.Cross((this.bone3.position - this.bone1.position).normalized, this.direction);
			this.defaultChildDirection = Quaternion.Inverse(this.bone3.rotation) * point;
			this.initiated = true;
		}

		public void SetLimbOrientation(Vector3 upper, Vector3 lower, Vector3 last)
		{
			if (upper == Vector3.zero)
			{
				Debug.LogError("Attempting to set limb orientation to Vector3.zero axis");
			}
			if (lower == Vector3.zero)
			{
				Debug.LogError("Attempting to set limb orientation to Vector3.zero axis");
			}
			if (last == Vector3.zero)
			{
				Debug.LogError("Attempting to set limb orientation to Vector3.zero axis");
			}
			this.defaultLocalDirection = upper.normalized;
			this.defaultChildDirection = last.normalized;
		}

		public void LimitBend(float solverWeight, float positionWeight)
		{
			if (!this.initiated)
			{
				return;
			}
			Vector3 vector = this.bone1.rotation * -this.defaultLocalDirection;
			Vector3 fromDirection = this.bone3.position - this.bone2.position;
			bool flag = false;
			Vector3 toDirection = V3Tools.ClampDirection(fromDirection, vector, this.clampF * solverWeight, 0, out flag);
			Quaternion rotation = this.bone3.rotation;
			if (flag)
			{
				Quaternion lhs = Quaternion.FromToRotation(fromDirection, toDirection);
				this.bone2.rotation = lhs * this.bone2.rotation;
			}
			if (positionWeight > 0f)
			{
				Vector3 vector2 = this.bone2.position - this.bone1.position;
				Vector3 fromDirection2 = this.bone3.position - this.bone2.position;
				Vector3.OrthoNormalize(ref vector2, ref fromDirection2);
				Quaternion lhs2 = Quaternion.FromToRotation(fromDirection2, vector);
				this.bone2.rotation = Quaternion.Lerp(this.bone2.rotation, lhs2 * this.bone2.rotation, positionWeight * solverWeight);
			}
			if (flag || positionWeight > 0f)
			{
				this.bone3.rotation = rotation;
			}
		}

		public Vector3 GetDir(IKSolverFullBody solver)
		{
			if (!this.initiated)
			{
				return Vector3.zero;
			}
			float num = this.weight * solver.IKPositionWeight;
			if (this.bendGoal != null)
			{
				Vector3 lhs = this.bendGoal.position - solver.GetNode(this.chainIndex1, this.nodeIndex1).solverPosition;
				if (lhs != Vector3.zero)
				{
					this.direction = lhs;
				}
			}
			if (num >= 1f)
			{
				return this.direction.normalized;
			}
			Vector3 vector = solver.GetNode(this.chainIndex3, this.nodeIndex3).solverPosition - solver.GetNode(this.chainIndex1, this.nodeIndex1).solverPosition;
			Vector3 vector2 = Quaternion.FromToRotation(this.bone3.position - this.bone1.position, vector) * (this.bone2.position - this.bone1.position);
			if (solver.GetNode(this.chainIndex3, this.nodeIndex3).effectorRotationWeight > 0f)
			{
				Vector3 b = -Vector3.Cross(vector, solver.GetNode(this.chainIndex3, this.nodeIndex3).solverRotation * this.defaultChildDirection);
				vector2 = Vector3.Lerp(vector2, b, solver.GetNode(this.chainIndex3, this.nodeIndex3).effectorRotationWeight);
			}
			if (this.rotationOffset != Quaternion.identity)
			{
				vector2 = Quaternion.FromToRotation(this.rotationOffset * vector, vector) * this.rotationOffset * vector2;
			}
			if (num <= 0f)
			{
				return vector2;
			}
			return Vector3.Lerp(vector2, this.direction.normalized, num);
		}

		private Vector3 OrthoToLimb(IKSolverFullBody solver, Vector3 tangent)
		{
			Vector3 vector = solver.GetNode(this.chainIndex3, this.nodeIndex3).solverPosition - solver.GetNode(this.chainIndex1, this.nodeIndex1).solverPosition;
			Vector3.OrthoNormalize(ref vector, ref tangent);
			return tangent;
		}

		private Vector3 OrthoToBone1(IKSolverFullBody solver, Vector3 tangent)
		{
			Vector3 vector = solver.GetNode(this.chainIndex2, this.nodeIndex2).solverPosition - solver.GetNode(this.chainIndex1, this.nodeIndex1).solverPosition;
			Vector3.OrthoNormalize(ref vector, ref tangent);
			return tangent;
		}

		public Transform bone1;

		public Transform bone2;

		public Transform bone3;

		public Transform bendGoal;

		public Vector3 direction = Vector3.right;

		public Quaternion rotationOffset;

		[Range(0f, 1f)]
		public float weight;

		public Vector3 defaultLocalDirection;

		public Vector3 defaultChildDirection;

		[NonSerialized]
		public float clampF = 0.505f;

		private int chainIndex1;

		private int nodeIndex1;

		private int chainIndex2;

		private int nodeIndex2;

		private int chainIndex3;

		private int nodeIndex3;
	}
}
