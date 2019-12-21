using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKEffector
	{
		public IKSolver.Node GetNode(IKSolverFullBody solver)
		{
			return solver.chain[this.chainIndex].nodes[this.nodeIndex];
		}

		public bool isEndEffector { get; private set; }

		public void PinToBone(float positionWeight, float rotationWeight)
		{
			this.position = this.bone.position;
			this.positionWeight = Mathf.Clamp(positionWeight, 0f, 1f);
			this.rotation = this.bone.rotation;
			this.rotationWeight = Mathf.Clamp(rotationWeight, 0f, 1f);
		}

		public IKEffector()
		{
		}

		public IKEffector(Transform bone, Transform[] childBones)
		{
			this.bone = bone;
			this.childBones = childBones;
		}

		public bool IsValid(IKSolver solver, ref string message)
		{
			if (this.bone == null)
			{
				message = "IK Effector bone is null.";
				return false;
			}
			if (solver.GetPoint(this.bone) == null)
			{
				message = "IK Effector is referencing to a bone '" + this.bone.name + "' that does not excist in the Node Chain.";
				return false;
			}
			Transform[] array = this.childBones;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == null)
				{
					message = "IK Effector contains a null reference.";
					return false;
				}
			}
			foreach (Transform transform in this.childBones)
			{
				if (solver.GetPoint(transform) == null)
				{
					message = "IK Effector is referencing to a bone '" + transform.name + "' that does not excist in the Node Chain.";
					return false;
				}
			}
			if (this.planeBone1 != null && solver.GetPoint(this.planeBone1) == null)
			{
				message = "IK Effector is referencing to a bone '" + this.planeBone1.name + "' that does not excist in the Node Chain.";
				return false;
			}
			if (this.planeBone2 != null && solver.GetPoint(this.planeBone2) == null)
			{
				message = "IK Effector is referencing to a bone '" + this.planeBone2.name + "' that does not excist in the Node Chain.";
				return false;
			}
			if (this.planeBone3 != null && solver.GetPoint(this.planeBone3) == null)
			{
				message = "IK Effector is referencing to a bone '" + this.planeBone3.name + "' that does not excist in the Node Chain.";
				return false;
			}
			return true;
		}

		public void Initiate(IKSolverFullBody solver)
		{
			this.position = this.bone.position;
			this.rotation = this.bone.rotation;
			this.animatedPlaneRotation = Quaternion.identity;
			solver.GetChainAndNodeIndexes(this.bone, out this.chainIndex, out this.nodeIndex);
			this.childChainIndexes = new int[this.childBones.Length];
			this.childNodeIndexes = new int[this.childBones.Length];
			for (int i = 0; i < this.childBones.Length; i++)
			{
				solver.GetChainAndNodeIndexes(this.childBones[i], out this.childChainIndexes[i], out this.childNodeIndexes[i]);
			}
			this.localPositions = new Vector3[this.childBones.Length];
			this.usePlaneNodes = false;
			if (this.planeBone1 != null)
			{
				solver.GetChainAndNodeIndexes(this.planeBone1, out this.plane1ChainIndex, out this.plane1NodeIndex);
				if (this.planeBone2 != null)
				{
					solver.GetChainAndNodeIndexes(this.planeBone2, out this.plane2ChainIndex, out this.plane2NodeIndex);
					if (this.planeBone3 != null)
					{
						solver.GetChainAndNodeIndexes(this.planeBone3, out this.plane3ChainIndex, out this.plane3NodeIndex);
						this.usePlaneNodes = true;
					}
				}
				this.isEndEffector = true;
				return;
			}
			this.isEndEffector = false;
		}

		public void ResetOffset(IKSolverFullBody solver)
		{
			solver.GetNode(this.chainIndex, this.nodeIndex).offset = Vector3.zero;
			for (int i = 0; i < this.childChainIndexes.Length; i++)
			{
				solver.GetNode(this.childChainIndexes[i], this.childNodeIndexes[i]).offset = Vector3.zero;
			}
		}

		public void SetToTarget()
		{
			if (this.target == null)
			{
				return;
			}
			this.position = this.target.position;
			this.rotation = this.target.rotation;
		}

		public void OnPreSolve(IKSolverFullBody solver)
		{
			this.positionWeight = Mathf.Clamp(this.positionWeight, 0f, 1f);
			this.rotationWeight = Mathf.Clamp(this.rotationWeight, 0f, 1f);
			this.maintainRelativePositionWeight = Mathf.Clamp(this.maintainRelativePositionWeight, 0f, 1f);
			this.posW = this.positionWeight * solver.IKPositionWeight;
			this.rotW = this.rotationWeight * solver.IKPositionWeight;
			solver.GetNode(this.chainIndex, this.nodeIndex).effectorPositionWeight = this.posW;
			solver.GetNode(this.chainIndex, this.nodeIndex).effectorRotationWeight = this.rotW;
			solver.GetNode(this.chainIndex, this.nodeIndex).solverRotation = this.rotation;
			if (float.IsInfinity(this.positionOffset.x) || float.IsInfinity(this.positionOffset.y) || float.IsInfinity(this.positionOffset.z))
			{
				Debug.LogError("Invalid IKEffector.positionOffset (contains Infinity)! Please make sure not to set IKEffector.positionOffset to infinite values.", this.bone);
			}
			if (float.IsNaN(this.positionOffset.x) || float.IsNaN(this.positionOffset.y) || float.IsNaN(this.positionOffset.z))
			{
				Debug.LogError("Invalid IKEffector.positionOffset (contains NaN)! Please make sure not to set IKEffector.positionOffset to NaN values.", this.bone);
			}
			if (this.positionOffset.sqrMagnitude > 1E+10f)
			{
				Debug.LogError("Additive effector positionOffset detected in Full Body IK (extremely large value). Make sure you are not circularily adding to effector positionOffset each frame.", this.bone);
			}
			if (float.IsInfinity(this.position.x) || float.IsInfinity(this.position.y) || float.IsInfinity(this.position.z))
			{
				Debug.LogError("Invalid IKEffector.position (contains Infinity)!");
			}
			solver.GetNode(this.chainIndex, this.nodeIndex).offset += this.positionOffset * solver.IKPositionWeight;
			if (this.effectChildNodes && solver.iterations > 0)
			{
				for (int i = 0; i < this.childBones.Length; i++)
				{
					this.localPositions[i] = this.childBones[i].transform.position - this.bone.transform.position;
					solver.GetNode(this.childChainIndexes[i], this.childNodeIndexes[i]).offset += this.positionOffset * solver.IKPositionWeight;
				}
			}
			if (this.usePlaneNodes && this.maintainRelativePositionWeight > 0f)
			{
				this.animatedPlaneRotation = Quaternion.LookRotation(this.planeBone2.position - this.planeBone1.position, this.planeBone3.position - this.planeBone1.position);
			}
			this.firstUpdate = true;
		}

		public void OnPostWrite()
		{
			this.positionOffset = Vector3.zero;
		}

		private Quaternion GetPlaneRotation(IKSolverFullBody solver)
		{
			Vector3 solverPosition = solver.GetNode(this.plane1ChainIndex, this.plane1NodeIndex).solverPosition;
			Vector3 solverPosition2 = solver.GetNode(this.plane2ChainIndex, this.plane2NodeIndex).solverPosition;
			Vector3 solverPosition3 = solver.GetNode(this.plane3ChainIndex, this.plane3NodeIndex).solverPosition;
			Vector3 vector = solverPosition2 - solverPosition;
			Vector3 upwards = solverPosition3 - solverPosition;
			if (vector == Vector3.zero)
			{
				Warning.Log("Make sure you are not placing 2 or more FBBIK effectors of the same chain to exactly the same position.", this.bone, false);
				return Quaternion.identity;
			}
			return Quaternion.LookRotation(vector, upwards);
		}

		public void Update(IKSolverFullBody solver)
		{
			if (this.firstUpdate)
			{
				this.animatedPosition = this.bone.position + solver.GetNode(this.chainIndex, this.nodeIndex).offset;
				this.firstUpdate = false;
			}
			solver.GetNode(this.chainIndex, this.nodeIndex).solverPosition = Vector3.Lerp(this.GetPosition(solver, out this.planeRotationOffset), this.position, this.posW);
			if (!this.effectChildNodes)
			{
				return;
			}
			for (int i = 0; i < this.childBones.Length; i++)
			{
				solver.GetNode(this.childChainIndexes[i], this.childNodeIndexes[i]).solverPosition = Vector3.Lerp(solver.GetNode(this.childChainIndexes[i], this.childNodeIndexes[i]).solverPosition, solver.GetNode(this.chainIndex, this.nodeIndex).solverPosition + this.localPositions[i], this.posW);
			}
		}

		private Vector3 GetPosition(IKSolverFullBody solver, out Quaternion planeRotationOffset)
		{
			planeRotationOffset = Quaternion.identity;
			if (!this.isEndEffector)
			{
				return solver.GetNode(this.chainIndex, this.nodeIndex).solverPosition;
			}
			if (this.maintainRelativePositionWeight <= 0f)
			{
				return this.animatedPosition;
			}
			Vector3 a = this.bone.position;
			Vector3 point = a - this.planeBone1.position;
			planeRotationOffset = this.GetPlaneRotation(solver) * Quaternion.Inverse(this.animatedPlaneRotation);
			a = solver.GetNode(this.plane1ChainIndex, this.plane1NodeIndex).solverPosition + planeRotationOffset * point;
			planeRotationOffset = Quaternion.Lerp(Quaternion.identity, planeRotationOffset, this.maintainRelativePositionWeight);
			return Vector3.Lerp(this.animatedPosition, a + solver.GetNode(this.chainIndex, this.nodeIndex).offset, this.maintainRelativePositionWeight);
		}

		public Transform bone;

		public Transform target;

		[Range(0f, 1f)]
		public float positionWeight;

		[Range(0f, 1f)]
		public float rotationWeight;

		public Vector3 position = Vector3.zero;

		public Quaternion rotation = Quaternion.identity;

		public Vector3 positionOffset;

		public bool effectChildNodes = true;

		[Range(0f, 1f)]
		public float maintainRelativePositionWeight;

		public Transform[] childBones = new Transform[0];

		public Transform planeBone1;

		public Transform planeBone2;

		public Transform planeBone3;

		public Quaternion planeRotationOffset = Quaternion.identity;

		private float posW;

		private float rotW;

		private Vector3[] localPositions = new Vector3[0];

		private bool usePlaneNodes;

		private Quaternion animatedPlaneRotation = Quaternion.identity;

		private Vector3 animatedPosition;

		private bool firstUpdate;

		private int chainIndex = -1;

		private int nodeIndex = -1;

		private int plane1ChainIndex;

		private int plane1NodeIndex = -1;

		private int plane2ChainIndex = -1;

		private int plane2NodeIndex = -1;

		private int plane3ChainIndex = -1;

		private int plane3NodeIndex = -1;

		private int[] childChainIndexes = new int[0];

		private int[] childNodeIndexes = new int[0];
	}
}
