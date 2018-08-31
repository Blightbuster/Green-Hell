using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public abstract class Poser : SolverManager
	{
		public abstract void AutoMapping();

		protected abstract void InitiatePoser();

		protected abstract void UpdatePoser();

		protected abstract void FixPoserTransforms();

		protected override void UpdateSolver()
		{
			if (!this.initiated)
			{
				this.InitiateSolver();
			}
			if (!this.initiated)
			{
				return;
			}
			this.UpdatePoser();
		}

		protected override void InitiateSolver()
		{
			if (this.initiated)
			{
				return;
			}
			this.InitiatePoser();
			this.initiated = true;
		}

		protected override void FixTransforms()
		{
			if (!this.initiated)
			{
				return;
			}
			this.FixPoserTransforms();
		}

		public Transform poseRoot;

		[Range(0f, 1f)]
		public float weight = 1f;

		[Range(0f, 1f)]
		public float localRotationWeight = 1f;

		[Range(0f, 1f)]
		public float localPositionWeight;

		private bool initiated;
	}
}
