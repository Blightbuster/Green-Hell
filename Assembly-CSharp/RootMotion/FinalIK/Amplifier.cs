using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class Amplifier : OffsetModifier
	{
		protected override void OnModifyOffset()
		{
			if (!this.ik.fixTransforms)
			{
				if (!Warning.logged)
				{
					Warning.Log("Amplifier needs the Fix Transforms option of the FBBIK to be set to true. Otherwise it might amplify to infinity, should the animator of the character stop because of culling.", base.transform, false);
				}
				return;
			}
			foreach (Amplifier.Body body in this.bodies)
			{
				body.Update(this.ik.solver, this.weight, base.deltaTime);
			}
		}

		[Tooltip("The amplified bodies.")]
		public Amplifier.Body[] bodies;

		[Serializable]
		public class Body
		{
			public void Update(IKSolverFullBodyBiped solver, float w, float deltaTime)
			{
				if (this.transform == null || this.relativeTo == null)
				{
					return;
				}
				Vector3 a = this.relativeTo.InverseTransformDirection(this.transform.position - this.relativeTo.position);
				if (this.firstUpdate)
				{
					this.lastRelativePos = a;
					this.firstUpdate = false;
				}
				Vector3 vector = (a - this.lastRelativePos) / deltaTime;
				this.smoothDelta = ((this.speed > 0f) ? Vector3.Lerp(this.smoothDelta, vector, deltaTime * this.speed) : vector);
				Vector3 v = this.relativeTo.TransformDirection(this.smoothDelta);
				Vector3 a2 = V3Tools.ExtractVertical(v, solver.GetRoot().up, this.verticalWeight) + V3Tools.ExtractHorizontal(v, solver.GetRoot().up, this.horizontalWeight);
				for (int i = 0; i < this.effectorLinks.Length; i++)
				{
					solver.GetEffector(this.effectorLinks[i].effector).positionOffset += a2 * w * this.effectorLinks[i].weight;
				}
				this.lastRelativePos = a;
			}

			private static Vector3 Multiply(Vector3 v1, Vector3 v2)
			{
				v1.x *= v2.x;
				v1.y *= v2.y;
				v1.z *= v2.z;
				return v1;
			}

			[Tooltip("The Transform that's motion we are reading.")]
			public Transform transform;

			[Tooltip("Amplify the 'transform's' position relative to this Transform.")]
			public Transform relativeTo;

			[Tooltip("Linking the body to effectors. One Body can be used to offset more than one effector.")]
			public Amplifier.Body.EffectorLink[] effectorLinks;

			[Tooltip("Amplification magnitude along the up axis of the character.")]
			public float verticalWeight = 1f;

			[Tooltip("Amplification magnitude along the horizontal axes of the character.")]
			public float horizontalWeight = 1f;

			[Tooltip("Speed of the amplifier. 0 means instant.")]
			public float speed = 3f;

			private Vector3 lastRelativePos;

			private Vector3 smoothDelta;

			private bool firstUpdate;

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
