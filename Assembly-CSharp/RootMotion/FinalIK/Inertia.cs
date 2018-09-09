using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class Inertia : OffsetModifier
	{
		public void ResetBodies()
		{
			this.lastTime = Time.time;
			foreach (Inertia.Body body in this.bodies)
			{
				body.Reset();
			}
		}

		protected override void OnModifyOffset()
		{
			foreach (Inertia.Body body in this.bodies)
			{
				body.Update(this.ik.solver, this.weight, base.deltaTime);
			}
			base.ApplyLimits(this.limits);
		}

		[Tooltip("The array of Bodies")]
		public Inertia.Body[] bodies;

		[Tooltip("The array of OffsetLimits")]
		public OffsetModifier.OffsetLimits[] limits;

		[Serializable]
		public class Body
		{
			public void Reset()
			{
				if (this.transform == null)
				{
					return;
				}
				this.lazyPoint = this.transform.position;
				this.lastPosition = this.transform.position;
				this.direction = Vector3.zero;
			}

			public void Update(IKSolverFullBodyBiped solver, float weight, float deltaTime)
			{
				if (this.transform == null)
				{
					return;
				}
				if (this.firstUpdate)
				{
					this.Reset();
					this.firstUpdate = false;
				}
				this.direction = Vector3.Lerp(this.direction, (this.transform.position - this.lazyPoint) / deltaTime * 0.01f, deltaTime * this.acceleration);
				this.lazyPoint += this.direction * deltaTime * this.speed;
				this.delta = this.transform.position - this.lastPosition;
				this.lazyPoint += this.delta * this.matchVelocity;
				this.lazyPoint.y = this.lazyPoint.y + this.gravity * deltaTime;
				foreach (Inertia.Body.EffectorLink effectorLink in this.effectorLinks)
				{
					solver.GetEffector(effectorLink.effector).positionOffset += (this.lazyPoint - this.transform.position) * effectorLink.weight * weight;
				}
				this.lastPosition = this.transform.position;
			}

			[Tooltip("The Transform to follow, can be any bone of the character")]
			public Transform transform;

			[Tooltip("Linking the body to effectors. One Body can be used to offset more than one effector")]
			public Inertia.Body.EffectorLink[] effectorLinks;

			[Tooltip("The speed to follow the Transform")]
			public float speed = 10f;

			[Tooltip("The acceleration, smaller values means lazyer following")]
			public float acceleration = 3f;

			[Range(0f, 1f)]
			[Tooltip("Matching target velocity")]
			public float matchVelocity;

			[Tooltip("gravity applied to the Body")]
			public float gravity;

			private Vector3 delta;

			private Vector3 lazyPoint;

			private Vector3 direction;

			private Vector3 lastPosition;

			private bool firstUpdate = true;

			[Serializable]
			public class EffectorLink
			{
				[Tooltip("Type of the FBBIK effector to use")]
				public FullBodyBipedEffector effector;

				[Tooltip("Weight of using this effector")]
				public float weight;
			}
		}
	}
}
