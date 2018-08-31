using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder Full Body Biped")]
	[HelpURL("https://www.youtube.com/watch?v=9MiZiaJorws&index=6&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6")]
	public class GrounderFBBIK : Grounder
	{
		[ContextMenu("TUTORIAL VIDEO")]
		private void OpenTutorial()
		{
			Application.OpenURL("https://www.youtube.com/watch?v=9MiZiaJorws&index=6&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
		}

		[ContextMenu("User Manual")]
		protected override void OpenUserManual()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page11.html");
		}

		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_f_b_b_i_k.html");
		}

		public override void ResetPosition()
		{
			this.solver.Reset();
			this.spineOffset = Vector3.zero;
		}

		private bool IsReadyToInitiate()
		{
			return !(this.ik == null) && this.ik.solver.initiated;
		}

		private void Update()
		{
			this.firstSolve = true;
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

		private void FixedUpdate()
		{
			this.firstSolve = true;
		}

		private void LateUpdate()
		{
			this.firstSolve = true;
		}

		private void Initiate()
		{
			this.ik.solver.leftLegMapping.maintainRotationWeight = 1f;
			this.ik.solver.rightLegMapping.maintainRotationWeight = 1f;
			this.feet = new Transform[2];
			this.feet[0] = this.ik.solver.leftFootEffector.bone;
			this.feet[1] = this.ik.solver.rightFootEffector.bone;
			IKSolverFullBodyBiped solver = this.ik.solver;
			solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPreUpdate, new IKSolver.UpdateDelegate(this.OnSolverUpdate));
			this.solver.Initiate(this.ik.references.root, this.feet);
			this.initiated = true;
		}

		private void OnSolverUpdate()
		{
			if (!this.firstSolve)
			{
				return;
			}
			this.firstSolve = false;
			if (!base.enabled)
			{
				return;
			}
			if (this.weight <= 0f)
			{
				return;
			}
			if (this.OnPreGrounder != null)
			{
				this.OnPreGrounder();
			}
			this.solver.Update();
			this.ik.references.pelvis.position += this.solver.pelvis.IKOffset * this.weight;
			this.SetLegIK(this.ik.solver.leftFootEffector, this.solver.legs[0]);
			this.SetLegIK(this.ik.solver.rightFootEffector, this.solver.legs[1]);
			if (this.spineBend != 0f)
			{
				this.spineSpeed = Mathf.Clamp(this.spineSpeed, 0f, this.spineSpeed);
				Vector3 a = base.GetSpineOffsetTarget() * this.weight;
				this.spineOffset = Vector3.Lerp(this.spineOffset, a * this.spineBend, Time.deltaTime * this.spineSpeed);
				Vector3 a2 = this.ik.references.root.up * this.spineOffset.magnitude;
				for (int i = 0; i < this.spine.Length; i++)
				{
					this.ik.solver.GetEffector(this.spine[i].effectorType).positionOffset += this.spineOffset * this.spine[i].horizontalWeight + a2 * this.spine[i].verticalWeight;
				}
			}
			if (this.OnPostGrounder != null)
			{
				this.OnPostGrounder();
			}
		}

		private void SetLegIK(IKEffector effector, Grounding.Leg leg)
		{
			effector.positionOffset += (leg.IKPosition - effector.bone.position) * this.weight;
			effector.bone.rotation = Quaternion.Slerp(Quaternion.identity, leg.rotationOffset, this.weight) * effector.bone.rotation;
		}

		private void OnDrawGizmosSelected()
		{
			if (this.ik == null)
			{
				this.ik = base.GetComponent<FullBodyBipedIK>();
			}
			if (this.ik == null)
			{
				this.ik = base.GetComponentInParent<FullBodyBipedIK>();
			}
			if (this.ik == null)
			{
				this.ik = base.GetComponentInChildren<FullBodyBipedIK>();
			}
		}

		private void OnDestroy()
		{
			if (this.initiated && this.ik != null)
			{
				IKSolverFullBodyBiped solver = this.ik.solver;
				solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPreUpdate, new IKSolver.UpdateDelegate(this.OnSolverUpdate));
			}
		}

		[Tooltip("Reference to the FBBIK componet.")]
		public FullBodyBipedIK ik;

		[Tooltip("The amount of spine bending towards upward slopes.")]
		public float spineBend = 2f;

		[Tooltip("The interpolation speed of spine bending.")]
		public float spineSpeed = 3f;

		public GrounderFBBIK.SpineEffector[] spine = new GrounderFBBIK.SpineEffector[0];

		private Transform[] feet = new Transform[2];

		private Vector3 spineOffset;

		private bool firstSolve;

		[Serializable]
		public class SpineEffector
		{
			[Tooltip("The type of the effector.")]
			public FullBodyBipedEffector effectorType;

			[Tooltip("The weight of horizontal bend offset towards the slope.")]
			public float horizontalWeight = 1f;

			[Tooltip("The vertical bend offset weight.")]
			public float verticalWeight;
		}
	}
}
