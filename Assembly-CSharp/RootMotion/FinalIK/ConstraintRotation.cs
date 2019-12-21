using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class ConstraintRotation : Constraint
	{
		public override void UpdateConstraint()
		{
			if (this.weight <= 0f)
			{
				return;
			}
			if (!base.isValid)
			{
				return;
			}
			this.transform.rotation = Quaternion.Slerp(this.transform.rotation, this.rotation, this.weight);
		}

		public ConstraintRotation()
		{
		}

		public ConstraintRotation(Transform transform)
		{
			this.transform = transform;
		}

		public Quaternion rotation;
	}
}
