using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class ConstraintPositionOffset : Constraint
	{
		public ConstraintPositionOffset()
		{
		}

		public ConstraintPositionOffset(Transform transform)
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
			if (!this.initiated)
			{
				this.defaultLocalPosition = this.transform.localPosition;
				this.lastLocalPosition = this.transform.localPosition;
				this.initiated = true;
			}
			if (this.positionChanged)
			{
				this.defaultLocalPosition = this.transform.localPosition;
			}
			this.transform.localPosition = this.defaultLocalPosition;
			this.transform.position += this.offset * this.weight;
			this.lastLocalPosition = this.transform.localPosition;
		}

		private bool positionChanged
		{
			get
			{
				return this.transform.localPosition != this.lastLocalPosition;
			}
		}

		public Vector3 offset;

		private Vector3 defaultLocalPosition;

		private Vector3 lastLocalPosition;

		private bool initiated;
	}
}
