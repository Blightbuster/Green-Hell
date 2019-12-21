using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[HelpURL("http://www.root-motion.com/finalikdox/html/page11.html")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder Biped")]
	public class GrounderBipedIK : Grounder
	{
		[ContextMenu("User Manual")]
		protected override void OpenUserManual()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page11.html");
		}

		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_biped_i_k.html");
		}

		public override void ResetPosition()
		{
			this.solver.Reset();
			this.spineOffset = Vector3.zero;
		}

		private bool IsReadyToInitiate()
		{
			return !(this.ik == null) && this.ik.solvers.leftFoot.initiated && this.ik.solvers.rightFoot.initiated;
		}

		private void Update()
		{
			this.weight = Mathf.Clamp(this.weight, 0f, 1f);
			if (this.weight <= 0f)
			{
				return;
			}
			if (this.initiated)
			{
				return;
			}
			if (!this.IsReadyToInitiate())
			{
				return;
			}
			this.Initiate();
		}

		private void Initiate()
		{
			this.feet = new Transform[2];
			this.footRotations = new Quaternion[2];
			this.feet[0] = this.ik.references.leftFoot;
			this.feet[1] = this.ik.references.rightFoot;
			this.footRotations[0] = Quaternion.identity;
			this.footRotations[1] = Quaternion.identity;
			IKSolverFABRIK spine = this.ik.solvers.spine;
			spine.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(spine.OnPreUpdate, new IKSolver.UpdateDelegate(this.OnSolverUpdate));
			IKSolverLimb rightFoot = this.ik.solvers.rightFoot;
			rightFoot.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(rightFoot.OnPostUpdate, new IKSolver.UpdateDelegate(this.OnPostSolverUpdate));
			this.animatedPelvisLocalPosition = this.ik.references.pelvis.localPosition;
			this.solver.Initiate(this.ik.references.root, this.feet);
			this.initiated = true;
		}

		private void OnDisable()
		{
			if (!this.initiated)
			{
				return;
			}
			this.ik.solvers.leftFoot.IKPositionWeight = 0f;
			this.ik.solvers.rightFoot.IKPositionWeight = 0f;
		}

		private void OnSolverUpdate()
		{
			if (!base.enabled)
			{
				return;
			}
			if (this.weight <= 0f)
			{
				if (this.lastWeight <= 0f)
				{
					return;
				}
				this.OnDisable();
			}
			this.lastWeight = this.weight;
			if (this.OnPreGrounder != null)
			{
				this.OnPreGrounder();
			}
			if (this.ik.references.pelvis.localPosition != this.solvedPelvisLocalPosition)
			{
				this.animatedPelvisLocalPosition = this.ik.references.pelvis.localPosition;
			}
			else
			{
				this.ik.references.pelvis.localPosition = this.animatedPelvisLocalPosition;
			}
			this.solver.Update();
			this.ik.references.pelvis.position += this.solver.pelvis.IKOffset * this.weight;
			this.SetLegIK(this.ik.solvers.leftFoot, 0);
			this.SetLegIK(this.ik.solvers.rightFoot, 1);
			if (this.spineBend != 0f && this.ik.references.spine.Length != 0)
			{
				this.spineSpeed = Mathf.Clamp(this.spineSpeed, 0f, this.spineSpeed);
				Vector3 a = base.GetSpineOffsetTarget() * this.weight;
				this.spineOffset = Vector3.Lerp(this.spineOffset, a * this.spineBend, Time.deltaTime * this.spineSpeed);
				Quaternion rotation = this.ik.references.leftUpperArm.rotation;
				Quaternion rotation2 = this.ik.references.rightUpperArm.rotation;
				Vector3 up = this.solver.up;
				Quaternion lhs = Quaternion.FromToRotation(up, up + this.spineOffset);
				this.ik.references.spine[0].rotation = lhs * this.ik.references.spine[0].rotation;
				this.ik.references.leftUpperArm.rotation = rotation;
				this.ik.references.rightUpperArm.rotation = rotation2;
			}
			if (this.OnPostGrounder != null)
			{
				this.OnPostGrounder();
			}
		}

		private void SetLegIK(IKSolverLimb limb, int index)
		{
			this.footRotations[index] = this.feet[index].rotation;
			limb.IKPosition = this.solver.legs[index].IKPosition;
			limb.IKPositionWeight = this.weight;
		}

		private void OnPostSolverUpdate()
		{
			if (this.weight <= 0f)
			{
				return;
			}
			if (!base.enabled)
			{
				return;
			}
			for (int i = 0; i < this.feet.Length; i++)
			{
				this.feet[i].rotation = Quaternion.Slerp(Quaternion.identity, this.solver.legs[i].rotationOffset, this.weight) * this.footRotations[i];
			}
			this.solvedPelvisLocalPosition = this.ik.references.pelvis.localPosition;
		}

		private void OnDestroy()
		{
			if (this.initiated && this.ik != null)
			{
				IKSolverFABRIK spine = this.ik.solvers.spine;
				spine.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(spine.OnPreUpdate, new IKSolver.UpdateDelegate(this.OnSolverUpdate));
				IKSolverLimb rightFoot = this.ik.solvers.rightFoot;
				rightFoot.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(rightFoot.OnPostUpdate, new IKSolver.UpdateDelegate(this.OnPostSolverUpdate));
			}
		}

		[Tooltip("The BipedIK componet.")]
		public BipedIK ik;

		[Tooltip("The amount of spine bending towards upward slopes.")]
		public float spineBend = 7f;

		[Tooltip("The interpolation speed of spine bending.")]
		public float spineSpeed = 3f;

		private Transform[] feet = new Transform[2];

		private Quaternion[] footRotations = new Quaternion[2];

		private Vector3 animatedPelvisLocalPosition;

		private Vector3 solvedPelvisLocalPosition;

		private Vector3 spineOffset;

		private float lastWeight;
	}
}
