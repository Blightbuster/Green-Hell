using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKMappingSpine : IKMapping
	{
		public IKMappingSpine()
		{
		}

		public IKMappingSpine(Transform[] spineBones, Transform leftUpperArmBone, Transform rightUpperArmBone, Transform leftThighBone, Transform rightThighBone)
		{
			this.SetBones(spineBones, leftUpperArmBone, rightUpperArmBone, leftThighBone, rightThighBone);
		}

		public override bool IsValid(IKSolver solver, ref string message)
		{
			if (!base.IsValid(solver, ref message))
			{
				return false;
			}
			foreach (Transform x in this.spineBones)
			{
				if (x == null)
				{
					message = "Spine bones contains a null reference.";
					return false;
				}
			}
			int num = 0;
			for (int j = 0; j < this.spineBones.Length; j++)
			{
				if (solver.GetPoint(this.spineBones[j]) != null)
				{
					num++;
				}
			}
			if (num == 0)
			{
				message = "IKMappingSpine does not contain any nodes.";
				return false;
			}
			if (this.leftUpperArmBone == null)
			{
				message = "IKMappingSpine is missing the left upper arm bone.";
				return false;
			}
			if (this.rightUpperArmBone == null)
			{
				message = "IKMappingSpine is missing the right upper arm bone.";
				return false;
			}
			if (this.leftThighBone == null)
			{
				message = "IKMappingSpine is missing the left thigh bone.";
				return false;
			}
			if (this.rightThighBone == null)
			{
				message = "IKMappingSpine is missing the right thigh bone.";
				return false;
			}
			if (solver.GetPoint(this.leftUpperArmBone) == null)
			{
				message = "Full Body IK is missing the left upper arm node.";
				return false;
			}
			if (solver.GetPoint(this.rightUpperArmBone) == null)
			{
				message = "Full Body IK is missing the right upper arm node.";
				return false;
			}
			if (solver.GetPoint(this.leftThighBone) == null)
			{
				message = "Full Body IK is missing the left thigh node.";
				return false;
			}
			if (solver.GetPoint(this.rightThighBone) == null)
			{
				message = "Full Body IK is missing the right thigh node.";
				return false;
			}
			return true;
		}

		public void SetBones(Transform[] spineBones, Transform leftUpperArmBone, Transform rightUpperArmBone, Transform leftThighBone, Transform rightThighBone)
		{
			this.spineBones = spineBones;
			this.leftUpperArmBone = leftUpperArmBone;
			this.rightUpperArmBone = rightUpperArmBone;
			this.leftThighBone = leftThighBone;
			this.rightThighBone = rightThighBone;
		}

		public void StoreDefaultLocalState()
		{
			for (int i = 0; i < this.spine.Length; i++)
			{
				this.spine[i].StoreDefaultLocalState();
			}
		}

		public void FixTransforms()
		{
			for (int i = 0; i < this.spine.Length; i++)
			{
				this.spine[i].FixTransform(i == 0 || i == this.spine.Length - 1);
			}
		}

		public override void Initiate(IKSolverFullBody solver)
		{
			if (this.iterations <= 0)
			{
				this.iterations = 3;
			}
			if (this.spine == null || this.spine.Length != this.spineBones.Length)
			{
				this.spine = new IKMapping.BoneMap[this.spineBones.Length];
			}
			this.rootNodeIndex = -1;
			for (int i = 0; i < this.spineBones.Length; i++)
			{
				if (this.spine[i] == null)
				{
					this.spine[i] = new IKMapping.BoneMap();
				}
				this.spine[i].Initiate(this.spineBones[i], solver);
				if (this.spine[i].isNodeBone)
				{
					this.rootNodeIndex = i;
				}
			}
			if (this.leftUpperArm == null)
			{
				this.leftUpperArm = new IKMapping.BoneMap();
			}
			if (this.rightUpperArm == null)
			{
				this.rightUpperArm = new IKMapping.BoneMap();
			}
			if (this.leftThigh == null)
			{
				this.leftThigh = new IKMapping.BoneMap();
			}
			if (this.rightThigh == null)
			{
				this.rightThigh = new IKMapping.BoneMap();
			}
			this.leftUpperArm.Initiate(this.leftUpperArmBone, solver);
			this.rightUpperArm.Initiate(this.rightUpperArmBone, solver);
			this.leftThigh.Initiate(this.leftThighBone, solver);
			this.rightThigh.Initiate(this.rightThighBone, solver);
			for (int j = 0; j < this.spine.Length; j++)
			{
				this.spine[j].SetIKPosition();
			}
			this.spine[0].SetPlane(solver, this.spine[this.rootNodeIndex].transform, this.leftThigh.transform, this.rightThigh.transform);
			for (int k = 0; k < this.spine.Length - 1; k++)
			{
				this.spine[k].SetLength(this.spine[k + 1]);
				this.spine[k].SetLocalSwingAxis(this.spine[k + 1]);
				this.spine[k].SetLocalTwistAxis(this.leftUpperArm.transform.position - this.rightUpperArm.transform.position, this.spine[k + 1].transform.position - this.spine[k].transform.position);
			}
			this.spine[this.spine.Length - 1].SetPlane(solver, this.spine[this.rootNodeIndex].transform, this.leftUpperArm.transform, this.rightUpperArm.transform);
			this.spine[this.spine.Length - 1].SetLocalSwingAxis(this.leftUpperArm, this.rightUpperArm);
			this.useFABRIK = this.UseFABRIK();
		}

		private bool UseFABRIK()
		{
			return this.spine.Length > 3 || this.rootNodeIndex != 1;
		}

		public void ReadPose()
		{
			this.spine[0].UpdatePlane(true, true);
			for (int i = 0; i < this.spine.Length - 1; i++)
			{
				this.spine[i].SetLength(this.spine[i + 1]);
				this.spine[i].SetLocalSwingAxis(this.spine[i + 1]);
				this.spine[i].SetLocalTwistAxis(this.leftUpperArm.transform.position - this.rightUpperArm.transform.position, this.spine[i + 1].transform.position - this.spine[i].transform.position);
			}
			this.spine[this.spine.Length - 1].UpdatePlane(true, true);
			this.spine[this.spine.Length - 1].SetLocalSwingAxis(this.leftUpperArm, this.rightUpperArm);
		}

		public void WritePose(IKSolverFullBody solver)
		{
			Vector3 planePosition = this.spine[0].GetPlanePosition(solver);
			Vector3 solverPosition = solver.GetNode(this.spine[this.rootNodeIndex].chainIndex, this.spine[this.rootNodeIndex].nodeIndex).solverPosition;
			Vector3 planePosition2 = this.spine[this.spine.Length - 1].GetPlanePosition(solver);
			if (this.useFABRIK)
			{
				Vector3 b = solver.GetNode(this.spine[this.rootNodeIndex].chainIndex, this.spine[this.rootNodeIndex].nodeIndex).solverPosition - this.spine[this.rootNodeIndex].transform.position;
				for (int i = 0; i < this.spine.Length; i++)
				{
					this.spine[i].ikPosition = this.spine[i].transform.position + b;
				}
				for (int j = 0; j < this.iterations; j++)
				{
					this.ForwardReach(planePosition2);
					this.BackwardReach(planePosition);
					this.spine[this.rootNodeIndex].ikPosition = solverPosition;
				}
			}
			else
			{
				this.spine[0].ikPosition = planePosition;
				this.spine[this.rootNodeIndex].ikPosition = solverPosition;
			}
			this.spine[this.spine.Length - 1].ikPosition = planePosition2;
			this.MapToSolverPositions(solver);
		}

		public void ForwardReach(Vector3 position)
		{
			this.spine[this.spineBones.Length - 1].ikPosition = position;
			for (int i = this.spine.Length - 2; i > -1; i--)
			{
				this.spine[i].ikPosition = base.SolveFABRIKJoint(this.spine[i].ikPosition, this.spine[i + 1].ikPosition, this.spine[i].length);
			}
		}

		private void BackwardReach(Vector3 position)
		{
			this.spine[0].ikPosition = position;
			for (int i = 1; i < this.spine.Length; i++)
			{
				this.spine[i].ikPosition = base.SolveFABRIKJoint(this.spine[i].ikPosition, this.spine[i - 1].ikPosition, this.spine[i - 1].length);
			}
		}

		private void MapToSolverPositions(IKSolverFullBody solver)
		{
			this.spine[0].SetToIKPosition();
			this.spine[0].RotateToPlane(solver, 1f);
			for (int i = 1; i < this.spine.Length - 1; i++)
			{
				this.spine[i].Swing(this.spine[i + 1].ikPosition, 1f);
				if (this.twistWeight > 0f)
				{
					float num = (float)i / ((float)this.spine.Length - 2f);
					Vector3 solverPosition = solver.GetNode(this.leftUpperArm.chainIndex, this.leftUpperArm.nodeIndex).solverPosition;
					Vector3 solverPosition2 = solver.GetNode(this.rightUpperArm.chainIndex, this.rightUpperArm.nodeIndex).solverPosition;
					this.spine[i].Twist(solverPosition - solverPosition2, this.spine[i + 1].ikPosition - this.spine[i].transform.position, num * this.twistWeight);
				}
			}
			this.spine[this.spine.Length - 1].SetToIKPosition();
			this.spine[this.spine.Length - 1].RotateToPlane(solver, 1f);
		}

		public Transform[] spineBones;

		public Transform leftUpperArmBone;

		public Transform rightUpperArmBone;

		public Transform leftThighBone;

		public Transform rightThighBone;

		[Range(1f, 3f)]
		public int iterations = 3;

		[Range(0f, 1f)]
		public float twistWeight = 1f;

		private int rootNodeIndex;

		private IKMapping.BoneMap[] spine = new IKMapping.BoneMap[0];

		private IKMapping.BoneMap leftUpperArm = new IKMapping.BoneMap();

		private IKMapping.BoneMap rightUpperArm = new IKMapping.BoneMap();

		private IKMapping.BoneMap leftThigh = new IKMapping.BoneMap();

		private IKMapping.BoneMap rightThigh = new IKMapping.BoneMap();

		private bool useFABRIK;
	}
}
