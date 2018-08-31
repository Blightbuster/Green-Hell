using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public abstract class Constraint
	{
		public bool isValid
		{
			get
			{
				return this.transform != null;
			}
		}

		public abstract void UpdateConstraint();

		public Transform transform;

		public float weight;
	}
}
