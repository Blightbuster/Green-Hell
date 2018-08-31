using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class Recoil : OffsetModifier
	{
		public bool isFinished
		{
			get
			{
				return Time.time > this.endTime;
			}
		}

		public void SetHandRotations(Quaternion leftHandRotation, Quaternion rightHandRotation)
		{
			if (this.handedness == Recoil.Handedness.Left)
			{
				this.primaryHandRotation = leftHandRotation;
			}
			else
			{
				this.primaryHandRotation = rightHandRotation;
			}
			this.handRotationsSet = true;
		}

		public void Fire(float magnitude)
		{
			float num = magnitude * UnityEngine.Random.value * this.magnitudeRandom;
			this.magnitudeMlp = magnitude + num;
			this.randomRotation = Quaternion.Euler(this.rotationRandom * UnityEngine.Random.value);
			foreach (Recoil.RecoilOffset recoilOffset in this.offsets)
			{
				recoilOffset.Start();
			}
			if (Time.time < this.endTime)
			{
				this.blendWeight = 0f;
			}
			else
			{
				this.blendWeight = 1f;
			}
			Keyframe[] keys = this.recoilWeight.keys;
			this.length = keys[keys.Length - 1].time;
			this.endTime = Time.time + this.length;
		}

		protected override void OnModifyOffset()
		{
			if (this.aimIK != null)
			{
				this.aimIKAxis = this.aimIK.solver.axis;
			}
			if (Time.time >= this.endTime)
			{
				this.rotationOffset = Quaternion.identity;
				return;
			}
			if (!this.initiated && this.ik != null)
			{
				this.initiated = true;
				IKSolverFullBodyBiped solver = this.ik.solver;
				solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPostUpdate, new IKSolver.UpdateDelegate(this.AfterFBBIK));
				if (this.aimIK != null)
				{
					IKSolverAim solver2 = this.aimIK.solver;
					solver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver2.OnPostUpdate, new IKSolver.UpdateDelegate(this.AfterAimIK));
				}
			}
			this.blendTime = Mathf.Max(this.blendTime, 0f);
			if (this.blendTime > 0f)
			{
				this.blendWeight = Mathf.Min(this.blendWeight + Time.deltaTime * (1f / this.blendTime), 1f);
			}
			else
			{
				this.blendWeight = 1f;
			}
			float b = this.recoilWeight.Evaluate(this.length - (this.endTime - Time.time)) * this.magnitudeMlp;
			this.w = Mathf.Lerp(this.w, b, this.blendWeight);
			Quaternion quaternion = (!(this.aimIK != null) || this.aimIKSolvedLast) ? this.ik.references.root.rotation : Quaternion.LookRotation(this.aimIK.solver.IKPosition - this.aimIK.solver.transform.position, this.ik.references.root.up);
			quaternion = this.randomRotation * quaternion;
			foreach (Recoil.RecoilOffset recoilOffset in this.offsets)
			{
				recoilOffset.Apply(this.ik.solver, quaternion, this.w, this.length, this.endTime - Time.time);
			}
			if (!this.handRotationsSet)
			{
				this.primaryHandRotation = this.primaryHand.rotation;
			}
			this.handRotationsSet = false;
			this.rotationOffset = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(this.randomRotation * this.primaryHandRotation * this.handRotationOffset), this.w);
			this.handRotation = this.rotationOffset * this.primaryHandRotation;
			if (this.twoHanded)
			{
				Vector3 point = Quaternion.Inverse(this.primaryHand.rotation) * (this.secondaryHand.position - this.primaryHand.position);
				this.secondaryHandRelativeRotation = Quaternion.Inverse(this.primaryHand.rotation) * this.secondaryHand.rotation;
				Vector3 a = this.primaryHand.position + this.primaryHandEffector.positionOffset;
				Vector3 a2 = a + this.handRotation * point;
				this.secondaryHandEffector.positionOffset += a2 - (this.secondaryHand.position + this.secondaryHandEffector.positionOffset);
			}
			if (this.aimIK != null && this.aimIKSolvedLast)
			{
				this.aimIK.solver.axis = Quaternion.Inverse(this.ik.references.root.rotation) * Quaternion.Inverse(this.rotationOffset) * this.aimIKAxis;
			}
		}

		private void AfterFBBIK()
		{
			if (Time.time >= this.endTime)
			{
				return;
			}
			this.primaryHand.rotation = this.handRotation;
			if (this.twoHanded)
			{
				this.secondaryHand.rotation = this.primaryHand.rotation * this.secondaryHandRelativeRotation;
			}
		}

		private void AfterAimIK()
		{
			if (this.aimIKSolvedLast)
			{
				this.aimIK.solver.axis = this.aimIKAxis;
			}
		}

		private IKEffector primaryHandEffector
		{
			get
			{
				if (this.handedness == Recoil.Handedness.Right)
				{
					return this.ik.solver.rightHandEffector;
				}
				return this.ik.solver.leftHandEffector;
			}
		}

		private IKEffector secondaryHandEffector
		{
			get
			{
				if (this.handedness == Recoil.Handedness.Right)
				{
					return this.ik.solver.leftHandEffector;
				}
				return this.ik.solver.rightHandEffector;
			}
		}

		private Transform primaryHand
		{
			get
			{
				return this.primaryHandEffector.bone;
			}
		}

		private Transform secondaryHand
		{
			get
			{
				return this.secondaryHandEffector.bone;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.ik != null && this.initiated)
			{
				IKSolverFullBodyBiped solver = this.ik.solver;
				solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPostUpdate, new IKSolver.UpdateDelegate(this.AfterFBBIK));
				if (this.aimIK != null)
				{
					IKSolverAim solver2 = this.aimIK.solver;
					solver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver2.OnPostUpdate, new IKSolver.UpdateDelegate(this.AfterAimIK));
				}
			}
		}

		[Tooltip("Reference to the AimIK component. Optional, only used to getting the aiming direction.")]
		public AimIK aimIK;

		[Tooltip("Set this true if you are using IKExecutionOrder.cs or a custom script to force AimIK solve after FBBIK.")]
		public bool aimIKSolvedLast;

		[Tooltip("Which hand is holding the weapon?")]
		public Recoil.Handedness handedness;

		[Tooltip("Check for 2-handed weapons.")]
		public bool twoHanded = true;

		[Tooltip("Weight curve for the recoil offsets. Recoil procedure is as long as this curve.")]
		public AnimationCurve recoilWeight;

		[Tooltip("How much is the magnitude randomized each time Recoil is called?")]
		public float magnitudeRandom = 0.1f;

		[Tooltip("How much is the rotation randomized each time Recoil is called?")]
		public Vector3 rotationRandom;

		[Tooltip("Rotating the primary hand bone for the recoil (in local space).")]
		public Vector3 handRotationOffset;

		[Tooltip("Time of blending in another recoil when doing automatic fire.")]
		public float blendTime;

		[Tooltip("FBBIK effector position offsets for the recoil (in aiming direction space).")]
		[Space(10f)]
		public Recoil.RecoilOffset[] offsets;

		[HideInInspector]
		public Quaternion rotationOffset = Quaternion.identity;

		private float magnitudeMlp = 1f;

		private float endTime = -1f;

		private Quaternion handRotation;

		private Quaternion secondaryHandRelativeRotation;

		private Quaternion randomRotation;

		private float length = 1f;

		private bool initiated;

		private float blendWeight;

		private float w;

		private Quaternion primaryHandRotation = Quaternion.identity;

		private bool handRotationsSet;

		private Vector3 aimIKAxis;

		[Serializable]
		public class RecoilOffset
		{
			public void Start()
			{
				if (this.additivity <= 0f)
				{
					return;
				}
				this.additiveOffset = Vector3.ClampMagnitude(this.lastOffset * this.additivity, this.maxAdditiveOffsetMag);
			}

			public void Apply(IKSolverFullBodyBiped solver, Quaternion rotation, float masterWeight, float length, float timeLeft)
			{
				this.additiveOffset = Vector3.Lerp(Vector3.zero, this.additiveOffset, timeLeft / length);
				this.lastOffset = rotation * (this.offset * masterWeight) + rotation * this.additiveOffset;
				foreach (Recoil.RecoilOffset.EffectorLink effectorLink in this.effectorLinks)
				{
					solver.GetEffector(effectorLink.effector).positionOffset += this.lastOffset * effectorLink.weight;
				}
			}

			[Tooltip("Offset vector for the associated effector when doing recoil.")]
			public Vector3 offset;

			[Tooltip("When firing before the last recoil has faded, how much of the current recoil offset will be maintained?")]
			[Range(0f, 1f)]
			public float additivity = 1f;

			[Tooltip("Max additive recoil for automatic fire.")]
			public float maxAdditiveOffsetMag = 0.2f;

			[Tooltip("Linking this recoil offset to FBBIK effectors.")]
			public Recoil.RecoilOffset.EffectorLink[] effectorLinks;

			private Vector3 additiveOffset;

			private Vector3 lastOffset;

			[Serializable]
			public class EffectorLink
			{
				[Tooltip("Type of the FBBIK effector to use")]
				public FullBodyBipedEffector effector;

				[Tooltip("Weight of using this effector")]
				public float weight;
			}
		}

		[Serializable]
		public enum Handedness
		{
			Right,
			Left
		}
	}
}
