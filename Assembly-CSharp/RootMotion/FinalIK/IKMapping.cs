using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKMapping
	{
		public virtual bool IsValid(IKSolver solver, ref string message)
		{
			return true;
		}

		public virtual void Initiate(IKSolverFullBody solver)
		{
		}

		protected bool BoneIsValid(Transform bone, IKSolver solver, ref string message, Warning.Logger logger = null)
		{
			if (bone == null)
			{
				message = "IKMappingLimb contains a null reference.";
				if (logger != null)
				{
					logger(message);
				}
				return false;
			}
			if (solver.GetPoint(bone) == null)
			{
				message = "IKMappingLimb is referencing to a bone '" + bone.name + "' that does not excist in the Node Chain.";
				if (logger != null)
				{
					logger(message);
				}
				return false;
			}
			return true;
		}

		protected Vector3 SolveFABRIKJoint(Vector3 pos1, Vector3 pos2, float length)
		{
			return pos2 + (pos1 - pos2).normalized * length;
		}

		[Serializable]
		public class BoneMap
		{
			public void Initiate(Transform transform, IKSolverFullBody solver)
			{
				this.transform = transform;
				solver.GetChainAndNodeIndexes(transform, out this.chainIndex, out this.nodeIndex);
			}

			public Vector3 swingDirection
			{
				get
				{
					return this.transform.rotation * this.localSwingAxis;
				}
			}

			public void StoreDefaultLocalState()
			{
				this.defaultLocalPosition = this.transform.localPosition;
				this.defaultLocalRotation = this.transform.localRotation;
			}

			public void FixTransform(bool position)
			{
				if (position)
				{
					this.transform.localPosition = this.defaultLocalPosition;
				}
				this.transform.localRotation = this.defaultLocalRotation;
			}

			public bool isNodeBone
			{
				get
				{
					return this.nodeIndex != -1;
				}
			}

			public void SetLength(IKMapping.BoneMap nextBone)
			{
				this.length = Vector3.Distance(this.transform.position, nextBone.transform.position);
			}

			public void SetLocalSwingAxis(IKMapping.BoneMap swingTarget)
			{
				this.SetLocalSwingAxis(swingTarget, this);
			}

			public void SetLocalSwingAxis(IKMapping.BoneMap bone1, IKMapping.BoneMap bone2)
			{
				this.localSwingAxis = Quaternion.Inverse(this.transform.rotation) * (bone1.transform.position - bone2.transform.position);
			}

			public void SetLocalTwistAxis(Vector3 twistDirection, Vector3 normalDirection)
			{
				Vector3.OrthoNormalize(ref normalDirection, ref twistDirection);
				this.localTwistAxis = Quaternion.Inverse(this.transform.rotation) * twistDirection;
			}

			public void SetPlane(IKSolverFullBody solver, Transform planeBone1, Transform planeBone2, Transform planeBone3)
			{
				this.planeBone1 = planeBone1;
				this.planeBone2 = planeBone2;
				this.planeBone3 = planeBone3;
				solver.GetChainAndNodeIndexes(planeBone1, out this.plane1ChainIndex, out this.plane1NodeIndex);
				solver.GetChainAndNodeIndexes(planeBone2, out this.plane2ChainIndex, out this.plane2NodeIndex);
				solver.GetChainAndNodeIndexes(planeBone3, out this.plane3ChainIndex, out this.plane3NodeIndex);
				this.UpdatePlane(true, true);
			}

			public void UpdatePlane(bool rotation, bool position)
			{
				Quaternion lastAnimatedTargetRotation = this.lastAnimatedTargetRotation;
				if (rotation)
				{
					this.defaultLocalTargetRotation = QuaTools.RotationToLocalSpace(this.transform.rotation, lastAnimatedTargetRotation);
				}
				if (position)
				{
					this.planePosition = Quaternion.Inverse(lastAnimatedTargetRotation) * (this.transform.position - this.planeBone1.position);
				}
			}

			public void SetIKPosition()
			{
				this.ikPosition = this.transform.position;
			}

			public void MaintainRotation()
			{
				this.maintainRotation = this.transform.rotation;
			}

			public void SetToIKPosition()
			{
				this.transform.position = this.ikPosition;
			}

			public void FixToNode(IKSolverFullBody solver, float weight, IKSolver.Node fixNode = null)
			{
				if (fixNode == null)
				{
					fixNode = solver.GetNode(this.chainIndex, this.nodeIndex);
				}
				if (weight >= 1f)
				{
					this.transform.position = fixNode.solverPosition;
					return;
				}
				this.transform.position = Vector3.Lerp(this.transform.position, fixNode.solverPosition, weight);
			}

			public Vector3 GetPlanePosition(IKSolverFullBody solver)
			{
				return solver.GetNode(this.plane1ChainIndex, this.plane1NodeIndex).solverPosition + this.GetTargetRotation(solver) * this.planePosition;
			}

			public void PositionToPlane(IKSolverFullBody solver)
			{
				this.transform.position = this.GetPlanePosition(solver);
			}

			public void RotateToPlane(IKSolverFullBody solver, float weight)
			{
				Quaternion quaternion = this.GetTargetRotation(solver) * this.defaultLocalTargetRotation;
				if (weight >= 1f)
				{
					this.transform.rotation = quaternion;
					return;
				}
				this.transform.rotation = Quaternion.Lerp(this.transform.rotation, quaternion, weight);
			}

			public void Swing(Vector3 swingTarget, float weight)
			{
				this.Swing(swingTarget, this.transform.position, weight);
			}

			public void Swing(Vector3 pos1, Vector3 pos2, float weight)
			{
				Quaternion quaternion = Quaternion.FromToRotation(this.transform.rotation * this.localSwingAxis, pos1 - pos2) * this.transform.rotation;
				if (weight >= 1f)
				{
					this.transform.rotation = quaternion;
					return;
				}
				this.transform.rotation = Quaternion.Lerp(this.transform.rotation, quaternion, weight);
			}

			public void Twist(Vector3 twistDirection, Vector3 normalDirection, float weight)
			{
				Vector3.OrthoNormalize(ref normalDirection, ref twistDirection);
				Quaternion quaternion = Quaternion.FromToRotation(this.transform.rotation * this.localTwistAxis, twistDirection) * this.transform.rotation;
				if (weight >= 1f)
				{
					this.transform.rotation = quaternion;
					return;
				}
				this.transform.rotation = Quaternion.Lerp(this.transform.rotation, quaternion, weight);
			}

			public void RotateToMaintain(float weight)
			{
				if (weight <= 0f)
				{
					return;
				}
				this.transform.rotation = Quaternion.Lerp(this.transform.rotation, this.maintainRotation, weight);
			}

			public void RotateToEffector(IKSolverFullBody solver, float weight)
			{
				if (!this.isNodeBone)
				{
					return;
				}
				float num = weight * solver.GetNode(this.chainIndex, this.nodeIndex).effectorRotationWeight;
				if (num <= 0f)
				{
					return;
				}
				if (num >= 1f)
				{
					this.transform.rotation = solver.GetNode(this.chainIndex, this.nodeIndex).solverRotation;
					return;
				}
				this.transform.rotation = Quaternion.Lerp(this.transform.rotation, solver.GetNode(this.chainIndex, this.nodeIndex).solverRotation, num);
			}

			private Quaternion GetTargetRotation(IKSolverFullBody solver)
			{
				Vector3 solverPosition = solver.GetNode(this.plane1ChainIndex, this.plane1NodeIndex).solverPosition;
				Vector3 solverPosition2 = solver.GetNode(this.plane2ChainIndex, this.plane2NodeIndex).solverPosition;
				Vector3 solverPosition3 = solver.GetNode(this.plane3ChainIndex, this.plane3NodeIndex).solverPosition;
				if (solverPosition == solverPosition3)
				{
					return Quaternion.identity;
				}
				return Quaternion.LookRotation(solverPosition2 - solverPosition, solverPosition3 - solverPosition);
			}

			private Quaternion lastAnimatedTargetRotation
			{
				get
				{
					if (this.planeBone1.position == this.planeBone3.position)
					{
						return Quaternion.identity;
					}
					return Quaternion.LookRotation(this.planeBone2.position - this.planeBone1.position, this.planeBone3.position - this.planeBone1.position);
				}
			}

			public Transform transform;

			public int chainIndex = -1;

			public int nodeIndex = -1;

			public Vector3 defaultLocalPosition;

			public Quaternion defaultLocalRotation;

			public Vector3 localSwingAxis;

			public Vector3 localTwistAxis;

			public Vector3 planePosition;

			public Vector3 ikPosition;

			public Quaternion defaultLocalTargetRotation;

			private Quaternion maintainRotation;

			public float length;

			public Quaternion animatedRotation;

			private Transform planeBone1;

			private Transform planeBone2;

			private Transform planeBone3;

			private int plane1ChainIndex = -1;

			private int plane1NodeIndex = -1;

			private int plane2ChainIndex = -1;

			private int plane2NodeIndex = -1;

			private int plane3ChainIndex = -1;

			private int plane3NodeIndex = -1;
		}
	}
}
