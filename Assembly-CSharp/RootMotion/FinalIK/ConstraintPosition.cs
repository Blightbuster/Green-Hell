using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class ConstraintPosition : Constraint
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
			this.transform.position = Vector3.Lerp(this.transform.position, this.position, this.weight);
		}

		public ConstraintPosition()
		{
		}

		public ConstraintPosition(Transform transform)
		{
			this.transform = transform;
		}

		public Vector3 position;
	}
}
