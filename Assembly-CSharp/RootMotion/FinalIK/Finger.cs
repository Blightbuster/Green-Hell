using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class Finger
	{
		public bool initiated { get; private set; }

		public Vector3 IKPosition
		{
			get
			{
				return this.solver.IKPosition;
			}
			set
			{
				this.solver.IKPosition = value;
			}
		}

		public Quaternion IKRotation
		{
			get
			{
				return this.solver.IKRotation;
			}
			set
			{
				this.solver.IKRotation = value;
			}
		}

		public bool IsValid(ref string errorMessage)
		{
			if (this.bone1 == null || this.bone2 == null || this.tip == null)
			{
				errorMessage = "One of the bones in the Finger Rig is null, can not initiate solvers.";
				return false;
			}
			return true;
		}

		public void Initiate(Transform hand, int index)
		{
			this.initiated = false;
			string empty = string.Empty;
			if (!this.IsValid(ref empty))
			{
				Warning.Log(empty, hand, false);
				return;
			}
			this.solver = new IKSolverLimb();
			this.solver.IKPositionWeight = this.weight;
			this.solver.bendModifier = IKSolverLimb.BendModifier.Target;
			this.solver.bendModifierWeight = 1f;
			this.IKPosition = this.tip.position;
			this.IKRotation = this.tip.rotation;
			if (this.bone3 != null)
			{
				this.bone3RelativeToTarget = Quaternion.Inverse(this.IKRotation) * this.bone3.rotation;
				this.bone3DefaultLocalPosition = this.bone3.localPosition;
				this.bone3DefaultLocalRotation = this.bone3.localRotation;
			}
			this.solver.SetChain(this.bone1, this.bone2, this.tip, hand);
			this.solver.Initiate(hand);
			this.initiated = true;
		}

		public void FixTransforms()
		{
			if (!this.initiated)
			{
				return;
			}
			this.solver.FixTransforms();
			if (this.bone3 != null)
			{
				this.bone3.localPosition = this.bone3DefaultLocalPosition;
				this.bone3.localRotation = this.bone3DefaultLocalRotation;
			}
		}

		public void Update(float masterWeight)
		{
			if (!this.initiated)
			{
				return;
			}
			float num = this.weight * masterWeight;
			if (num <= 0f)
			{
				return;
			}
			this.solver.target = this.target;
			if (this.target != null)
			{
				this.IKPosition = this.target.position;
				this.IKRotation = this.target.rotation;
			}
			if (this.bone3 != null)
			{
				if (num >= 1f)
				{
					this.bone3.rotation = this.IKRotation * this.bone3RelativeToTarget;
				}
				else
				{
					this.bone3.rotation = Quaternion.Lerp(this.bone3.rotation, this.IKRotation * this.bone3RelativeToTarget, num);
				}
			}
			this.solver.IKPositionWeight = num;
			this.solver.Update();
		}

		[Tooltip("Master Weight for the finger.")]
		[Range(0f, 1f)]
		public float weight = 1f;

		[Tooltip("The first bone of the finger.")]
		public Transform bone1;

		[Tooltip("The second bone of the finger.")]
		public Transform bone2;

		[Tooltip("The (optional) third bone of the finger. This can be ignored for thumbs.")]
		public Transform bone3;

		[Tooltip("The fingertip object. If your character doesn't have tip bones, you can create an empty GameObject and parent it to the last bone in the finger. Place it to the tip of the finger.")]
		public Transform tip;

		[Tooltip("The IK target (optional, can use IKPosition and IKRotation directly).")]
		public Transform target;

		private IKSolverLimb solver;

		private Quaternion bone3RelativeToTarget;

		private Vector3 bone3DefaultLocalPosition;

		private Quaternion bone3DefaultLocalRotation;
	}
}
