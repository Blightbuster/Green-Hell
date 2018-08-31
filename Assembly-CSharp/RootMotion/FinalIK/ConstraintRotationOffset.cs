using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class ConstraintRotationOffset : Constraint
	{
		public ConstraintRotationOffset()
		{
		}

		public ConstraintRotationOffset(Transform transform)
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
				this.defaultLocalRotation = this.transform.localRotation;
				this.lastLocalRotation = this.transform.localRotation;
				this.initiated = true;
			}
			if (this.rotationChanged)
			{
				this.defaultLocalRotation = this.transform.localRotation;
			}
			this.transform.localRotation = this.defaultLocalRotation;
			this.transform.rotation = Quaternion.Slerp(this.transform.rotation, this.offset, this.weight);
			this.lastLocalRotation = this.transform.localRotation;
		}

		private bool rotationChanged
		{
			get
			{
				return this.transform.localRotation != this.lastLocalRotation;
			}
		}

		public Quaternion offset;

		private Quaternion defaultRotation;

		private Quaternion defaultLocalRotation;

		private Quaternion lastLocalRotation;

		private Quaternion defaultTargetLocalRotation;

		private bool initiated;
	}
}
