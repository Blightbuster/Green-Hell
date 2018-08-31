using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class FBBIKArmBending : MonoBehaviour
	{
		private void LateUpdate()
		{
			if (this.ik == null)
			{
				return;
			}
			if (!this.initiated)
			{
				IKSolverFullBodyBiped solver = this.ik.solver;
				solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPostUpdate, new IKSolver.UpdateDelegate(this.OnPostFBBIK));
				this.initiated = true;
			}
			if (this.ik.solver.leftHandEffector.target != null)
			{
				Vector3 left = Vector3.left;
				this.ik.solver.leftArmChain.bendConstraint.direction = this.ik.solver.leftHandEffector.target.rotation * left + this.ik.solver.leftHandEffector.target.rotation * this.bendDirectionOffsetLeft + this.ik.transform.rotation * this.characterSpaceBendOffsetLeft;
				this.ik.solver.leftArmChain.bendConstraint.weight = 1f;
			}
			if (this.ik.solver.rightHandEffector.target != null)
			{
				Vector3 right = Vector3.right;
				this.ik.solver.rightArmChain.bendConstraint.direction = this.ik.solver.rightHandEffector.target.rotation * right + this.ik.solver.rightHandEffector.target.rotation * this.bendDirectionOffsetRight + this.ik.transform.rotation * this.characterSpaceBendOffsetRight;
				this.ik.solver.rightArmChain.bendConstraint.weight = 1f;
			}
		}

		private void OnPostFBBIK()
		{
			if (this.ik == null)
			{
				return;
			}
			if (this.ik.solver.leftHandEffector.target != null)
			{
				this.ik.references.leftHand.rotation = this.ik.solver.leftHandEffector.target.rotation;
			}
			if (this.ik.solver.rightHandEffector.target != null)
			{
				this.ik.references.rightHand.rotation = this.ik.solver.rightHandEffector.target.rotation;
			}
		}

		private void OnDestroy()
		{
			if (this.ik != null)
			{
				IKSolverFullBodyBiped solver = this.ik.solver;
				solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPostUpdate, new IKSolver.UpdateDelegate(this.OnPostFBBIK));
			}
		}

		public FullBodyBipedIK ik;

		public Vector3 bendDirectionOffsetLeft;

		public Vector3 bendDirectionOffsetRight;

		public Vector3 characterSpaceBendOffsetLeft;

		public Vector3 characterSpaceBendOffsetRight;

		private Quaternion leftHandTargetRotation;

		private Quaternion rightHandTargetRotation;

		private bool initiated;
	}
}
