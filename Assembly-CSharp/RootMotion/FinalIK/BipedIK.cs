using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[HelpURL("http://www.root-motion.com/finalikdox/html/page2.html")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/IK/Biped IK")]
	public class BipedIK : SolverManager
	{
		[ContextMenu("User Manual")]
		private void OpenUserManual()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page2.html");
		}

		[ContextMenu("Scrpt Reference")]
		private void OpenScriptReference()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_biped_i_k.html");
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

		public float GetIKPositionWeight(AvatarIKGoal goal)
		{
			return this.GetGoalIK(goal).GetIKPositionWeight();
		}

		public float GetIKRotationWeight(AvatarIKGoal goal)
		{
			return this.GetGoalIK(goal).GetIKRotationWeight();
		}

		public void SetIKPositionWeight(AvatarIKGoal goal, float weight)
		{
			this.GetGoalIK(goal).SetIKPositionWeight(weight);
		}

		public void SetIKRotationWeight(AvatarIKGoal goal, float weight)
		{
			this.GetGoalIK(goal).SetIKRotationWeight(weight);
		}

		public void SetIKPosition(AvatarIKGoal goal, Vector3 IKPosition)
		{
			this.GetGoalIK(goal).SetIKPosition(IKPosition);
		}

		public void SetIKRotation(AvatarIKGoal goal, Quaternion IKRotation)
		{
			this.GetGoalIK(goal).SetIKRotation(IKRotation);
		}

		public Vector3 GetIKPosition(AvatarIKGoal goal)
		{
			return this.GetGoalIK(goal).GetIKPosition();
		}

		public Quaternion GetIKRotation(AvatarIKGoal goal)
		{
			return this.GetGoalIK(goal).GetIKRotation();
		}

		public void SetLookAtWeight(float weight, float bodyWeight, float headWeight, float eyesWeight, float clampWeight, float clampWeightHead, float clampWeightEyes)
		{
			this.solvers.lookAt.SetLookAtWeight(weight, bodyWeight, headWeight, eyesWeight, clampWeight, clampWeightHead, clampWeightEyes);
		}

		public void SetLookAtPosition(Vector3 lookAtPosition)
		{
			this.solvers.lookAt.SetIKPosition(lookAtPosition);
		}

		public void SetSpinePosition(Vector3 spinePosition)
		{
			this.solvers.spine.SetIKPosition(spinePosition);
		}

		public void SetSpineWeight(float weight)
		{
			this.solvers.spine.SetIKPositionWeight(weight);
		}

		public IKSolverLimb GetGoalIK(AvatarIKGoal goal)
		{
			switch (goal)
			{
			case AvatarIKGoal.LeftFoot:
				return this.solvers.leftFoot;
			case AvatarIKGoal.RightFoot:
				return this.solvers.rightFoot;
			case AvatarIKGoal.LeftHand:
				return this.solvers.leftHand;
			case AvatarIKGoal.RightHand:
				return this.solvers.rightHand;
			default:
				return null;
			}
		}

		public void InitiateBipedIK()
		{
			this.InitiateSolver();
		}

		public void UpdateBipedIK()
		{
			this.UpdateSolver();
		}

		public void SetToDefaults()
		{
			foreach (IKSolverLimb iksolverLimb in this.solvers.limbs)
			{
				iksolverLimb.SetIKPositionWeight(0f);
				iksolverLimb.SetIKRotationWeight(0f);
				iksolverLimb.bendModifier = IKSolverLimb.BendModifier.Animation;
				iksolverLimb.bendModifierWeight = 1f;
			}
			this.solvers.leftHand.maintainRotationWeight = 0f;
			this.solvers.rightHand.maintainRotationWeight = 0f;
			this.solvers.spine.SetIKPositionWeight(0f);
			this.solvers.spine.tolerance = 0f;
			this.solvers.spine.maxIterations = 2;
			this.solvers.spine.useRotationLimits = false;
			this.solvers.aim.SetIKPositionWeight(0f);
			this.solvers.aim.tolerance = 0f;
			this.solvers.aim.maxIterations = 2;
			this.SetLookAtWeight(0f, 0.5f, 1f, 1f, 0.5f, 0.7f, 0.5f);
		}

		protected override void FixTransforms()
		{
			this.solvers.lookAt.FixTransforms();
			for (int i = 0; i < this.solvers.limbs.Length; i++)
			{
				this.solvers.limbs[i].FixTransforms();
			}
		}

		protected override void InitiateSolver()
		{
			string empty = string.Empty;
			if (BipedReferences.SetupError(this.references, ref empty))
			{
				Warning.Log(empty, this.references.root, false);
				return;
			}
			this.solvers.AssignReferences(this.references);
			if (this.solvers.spine.bones.Length > 1)
			{
				this.solvers.spine.Initiate(base.transform);
			}
			this.solvers.lookAt.Initiate(base.transform);
			this.solvers.aim.Initiate(base.transform);
			foreach (IKSolverLimb iksolverLimb in this.solvers.limbs)
			{
				iksolverLimb.Initiate(base.transform);
			}
			this.solvers.pelvis.Initiate(this.references.pelvis);
		}

		protected override void UpdateSolver()
		{
			for (int i = 0; i < this.solvers.limbs.Length; i++)
			{
				this.solvers.limbs[i].MaintainBend();
				this.solvers.limbs[i].MaintainRotation();
			}
			this.solvers.pelvis.Update();
			if (this.solvers.spine.bones.Length > 1)
			{
				this.solvers.spine.Update();
			}
			this.solvers.aim.Update();
			this.solvers.lookAt.Update();
			for (int j = 0; j < this.solvers.limbs.Length; j++)
			{
				this.solvers.limbs[j].Update();
			}
		}

		public void LogWarning(string message)
		{
			Warning.Log(message, base.transform, false);
		}

		public BipedReferences references = new BipedReferences();

		public BipedIKSolvers solvers = new BipedIKSolvers();
	}
}
