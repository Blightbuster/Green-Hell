using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class OffsetPose : MonoBehaviour
	{
		public void Apply(IKSolverFullBodyBiped solver, float weight)
		{
			for (int i = 0; i < this.effectorLinks.Length; i++)
			{
				this.effectorLinks[i].Apply(solver, weight, solver.GetRoot().rotation);
			}
		}

		public void Apply(IKSolverFullBodyBiped solver, float weight, Quaternion rotation)
		{
			for (int i = 0; i < this.effectorLinks.Length; i++)
			{
				this.effectorLinks[i].Apply(solver, weight, rotation);
			}
		}

		public OffsetPose.EffectorLink[] effectorLinks = new OffsetPose.EffectorLink[0];

		[Serializable]
		public class EffectorLink
		{
			public void Apply(IKSolverFullBodyBiped solver, float weight, Quaternion rotation)
			{
				solver.GetEffector(this.effector).positionOffset += rotation * this.offset * weight;
				Vector3 a = solver.GetRoot().position + rotation * this.pin;
				Vector3 vector = a - solver.GetEffector(this.effector).bone.position;
				Vector3 vector2 = this.pinWeight * Mathf.Abs(weight);
				solver.GetEffector(this.effector).positionOffset = new Vector3(Mathf.Lerp(solver.GetEffector(this.effector).positionOffset.x, vector.x, vector2.x), Mathf.Lerp(solver.GetEffector(this.effector).positionOffset.y, vector.y, vector2.y), Mathf.Lerp(solver.GetEffector(this.effector).positionOffset.z, vector.z, vector2.z));
			}

			public FullBodyBipedEffector effector;

			public Vector3 offset;

			public Vector3 pin;

			public Vector3 pinWeight;
		}
	}
}
