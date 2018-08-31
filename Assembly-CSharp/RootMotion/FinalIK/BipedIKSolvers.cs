using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class BipedIKSolvers
	{
		public IKSolverLimb[] limbs
		{
			get
			{
				if (this._limbs == null || (this._limbs != null && this._limbs.Length != 4))
				{
					this._limbs = new IKSolverLimb[]
					{
						this.leftFoot,
						this.rightFoot,
						this.leftHand,
						this.rightHand
					};
				}
				return this._limbs;
			}
		}

		public IKSolver[] ikSolvers
		{
			get
			{
				if (this._ikSolvers == null || (this._ikSolvers != null && this._ikSolvers.Length != 7))
				{
					this._ikSolvers = new IKSolver[]
					{
						this.leftFoot,
						this.rightFoot,
						this.leftHand,
						this.rightHand,
						this.spine,
						this.lookAt,
						this.aim
					};
				}
				return this._ikSolvers;
			}
		}

		public void AssignReferences(BipedReferences references)
		{
			this.leftHand.SetChain(references.leftUpperArm, references.leftForearm, references.leftHand, references.root);
			this.rightHand.SetChain(references.rightUpperArm, references.rightForearm, references.rightHand, references.root);
			this.leftFoot.SetChain(references.leftThigh, references.leftCalf, references.leftFoot, references.root);
			this.rightFoot.SetChain(references.rightThigh, references.rightCalf, references.rightFoot, references.root);
			this.spine.SetChain(references.spine, references.root);
			this.lookAt.SetChain(references.spine, references.head, references.eyes, references.root);
			this.aim.SetChain(references.spine, references.root);
			this.leftFoot.goal = AvatarIKGoal.LeftFoot;
			this.rightFoot.goal = AvatarIKGoal.RightFoot;
			this.leftHand.goal = AvatarIKGoal.LeftHand;
			this.rightHand.goal = AvatarIKGoal.RightHand;
		}

		public IKSolverLimb leftFoot = new IKSolverLimb(AvatarIKGoal.LeftFoot);

		public IKSolverLimb rightFoot = new IKSolverLimb(AvatarIKGoal.RightFoot);

		public IKSolverLimb leftHand = new IKSolverLimb(AvatarIKGoal.LeftHand);

		public IKSolverLimb rightHand = new IKSolverLimb(AvatarIKGoal.RightHand);

		public IKSolverFABRIK spine = new IKSolverFABRIK();

		public IKSolverLookAt lookAt = new IKSolverLookAt();

		public IKSolverAim aim = new IKSolverAim();

		public Constraints pelvis = new Constraints();

		private IKSolverLimb[] _limbs;

		private IKSolver[] _ikSolvers;
	}
}
