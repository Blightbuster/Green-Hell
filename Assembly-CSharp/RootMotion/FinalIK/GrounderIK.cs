using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder IK")]
	[HelpURL("http://www.root-motion.com/finalikdox/html/page11.html")]
	public class GrounderIK : Grounder
	{
		[ContextMenu("User Manual")]
		protected override void OpenUserManual()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page11.html");
		}

		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_i_k.html");
		}

		public override void ResetPosition()
		{
			this.solver.Reset();
		}

		private bool IsReadyToInitiate()
		{
			if (this.pelvis == null)
			{
				return false;
			}
			if (this.legs.Length == 0)
			{
				return false;
			}
			foreach (IK ik in this.legs)
			{
				if (ik == null)
				{
					return false;
				}
				if (ik is FullBodyBipedIK)
				{
					base.LogWarning("GrounderIK does not support FullBodyBipedIK, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead. If you want to use FullBodyBipedIK, use the GrounderFBBIK component.");
					return false;
				}
				if (ik is FABRIKRoot)
				{
					base.LogWarning("GrounderIK does not support FABRIKRoot, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead.");
					return false;
				}
				if (ik is AimIK)
				{
					base.LogWarning("GrounderIK does not support AimIK, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead.");
					return false;
				}
			}
			return true;
		}

		private void OnDisable()
		{
			if (!this.initiated)
			{
				return;
			}
			for (int i = 0; i < this.legs.Length; i++)
			{
				if (this.legs[i] != null)
				{
					this.legs[i].GetIKSolver().IKPositionWeight = 0f;
				}
			}
		}

		private void Update()
		{
			this.weight = Mathf.Clamp(this.weight, 0f, 1f);
			if (this.weight <= 0f)
			{
				return;
			}
			this.solved = false;
			if (this.initiated)
			{
				this.rootRotationWeight = Mathf.Clamp(this.rootRotationWeight, 0f, 1f);
				this.rootRotationSpeed = Mathf.Clamp(this.rootRotationSpeed, 0f, this.rootRotationSpeed);
				if (this.characterRoot != null && this.rootRotationSpeed > 0f && this.rootRotationWeight > 0f)
				{
					Vector3 vector = this.solver.GetLegsPlaneNormal();
					if (this.rootRotationWeight < 1f)
					{
						vector = Vector3.Slerp(Vector3.up, vector, this.rootRotationWeight);
					}
					Quaternion from = Quaternion.FromToRotation(base.transform.up, Vector3.up) * this.characterRoot.rotation;
					Quaternion b = Quaternion.RotateTowards(from, Quaternion.FromToRotation(base.transform.up, vector) * this.characterRoot.rotation, this.maxRootRotationAngle);
					this.characterRoot.rotation = Quaternion.Lerp(this.characterRoot.rotation, b, Time.deltaTime * this.rootRotationSpeed);
				}
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
			this.feet = new Transform[this.legs.Length];
			this.footRotations = new Quaternion[this.legs.Length];
			for (int i = 0; i < this.feet.Length; i++)
			{
				this.footRotations[i] = Quaternion.identity;
			}
			for (int j = 0; j < this.legs.Length; j++)
			{
				IKSolver.Point[] points = this.legs[j].GetIKSolver().GetPoints();
				this.feet[j] = points[points.Length - 1].transform;
				IKSolver iksolver = this.legs[j].GetIKSolver();
				iksolver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iksolver.OnPreUpdate, new IKSolver.UpdateDelegate(this.OnSolverUpdate));
				IKSolver iksolver2 = this.legs[j].GetIKSolver();
				iksolver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iksolver2.OnPostUpdate, new IKSolver.UpdateDelegate(this.OnPostSolverUpdate));
			}
			this.animatedPelvisLocalPosition = this.pelvis.localPosition;
			this.solver.Initiate(base.transform, this.feet);
			for (int k = 0; k < this.legs.Length; k++)
			{
				if (this.legs[k] is LegIK)
				{
					this.solver.legs[k].invertFootCenter = true;
				}
			}
			this.initiated = true;
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
			if (this.solved)
			{
				return;
			}
			if (this.OnPreGrounder != null)
			{
				this.OnPreGrounder();
			}
			if (this.pelvis.localPosition != this.solvedPelvisLocalPosition)
			{
				this.animatedPelvisLocalPosition = this.pelvis.localPosition;
			}
			else
			{
				this.pelvis.localPosition = this.animatedPelvisLocalPosition;
			}
			this.solver.Update();
			for (int i = 0; i < this.legs.Length; i++)
			{
				this.SetLegIK(i);
			}
			this.pelvis.position += this.solver.pelvis.IKOffset * this.weight;
			this.solved = true;
			this.solvedFeet = 0;
			if (this.OnPostGrounder != null)
			{
				this.OnPostGrounder();
			}
		}

		private void SetLegIK(int index)
		{
			this.footRotations[index] = this.feet[index].rotation;
			if (this.legs[index] is LegIK)
			{
				(this.legs[index].GetIKSolver() as IKSolverLeg).IKRotation = Quaternion.Slerp(Quaternion.identity, this.solver.legs[index].rotationOffset, this.weight) * this.footRotations[index];
				(this.legs[index].GetIKSolver() as IKSolverLeg).IKRotationWeight = 1f;
			}
			this.legs[index].GetIKSolver().IKPosition = this.solver.legs[index].IKPosition;
			this.legs[index].GetIKSolver().IKPositionWeight = this.weight;
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
			this.solvedFeet++;
			if (this.solvedFeet < this.feet.Length)
			{
				return;
			}
			for (int i = 0; i < this.feet.Length; i++)
			{
				this.feet[i].rotation = Quaternion.Slerp(Quaternion.identity, this.solver.legs[i].rotationOffset, this.weight) * this.footRotations[i];
			}
			this.solvedPelvisLocalPosition = this.pelvis.localPosition;
		}

		private void OnDestroy()
		{
			if (this.initiated)
			{
				foreach (IK ik in this.legs)
				{
					if (ik != null)
					{
						IKSolver iksolver = ik.GetIKSolver();
						iksolver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(iksolver.OnPreUpdate, new IKSolver.UpdateDelegate(this.OnSolverUpdate));
						IKSolver iksolver2 = ik.GetIKSolver();
						iksolver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(iksolver2.OnPostUpdate, new IKSolver.UpdateDelegate(this.OnPostSolverUpdate));
					}
				}
			}
		}

		public IK[] legs;

		[Tooltip("The pelvis transform. Common ancestor of all the legs.")]
		public Transform pelvis;

		[Tooltip("The root Transform of the character, with the rigidbody and the collider.")]
		public Transform characterRoot;

		[Range(0f, 1f)]
		[Tooltip("The weight of rotating the character root to the ground normal (range: 0 - 1).")]
		public float rootRotationWeight;

		[Tooltip("The speed of rotating the character root to the ground normal (range: 0 - inf).")]
		public float rootRotationSpeed = 5f;

		[Tooltip("The maximum angle of root rotation (range: 0 - 90).")]
		public float maxRootRotationAngle = 45f;

		private Transform[] feet = new Transform[0];

		private Quaternion[] footRotations = new Quaternion[0];

		private Vector3 animatedPelvisLocalPosition;

		private Vector3 solvedPelvisLocalPosition;

		private int solvedFeet;

		private bool solved;

		private float lastWeight;
	}
}
