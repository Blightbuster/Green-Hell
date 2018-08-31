using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class ConstraintPosition : Constraint
	{
		public ConstraintPosition()
		{
		}

		public ConstraintPosition(Transform transform)
		{
			this.transform = transform;
		}

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
			this.transform.position = Vector3.Lerp(this.transform.position, this.position, this.weight);
		}

		public Vector3 position;
	}
}
