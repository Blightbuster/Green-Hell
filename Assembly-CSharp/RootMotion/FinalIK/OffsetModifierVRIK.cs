using System;
using System.Collections;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public abstract class OffsetModifierVRIK : MonoBehaviour
	{
		protected float deltaTime
		{
			get
			{
				return Time.time - this.lastTime;
			}
		}

		protected abstract void OnModifyOffset();

		protected virtual void Start()
		{
			base.StartCoroutine(this.Initiate());
		}

		private IEnumerator Initiate()
		{
			while (this.ik == null)
			{
				yield return null;
			}
			IKSolverVR solver = this.ik.solver;
			solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPreUpdate, new IKSolver.UpdateDelegate(this.ModifyOffset));
			this.lastTime = Time.time;
			yield break;
		}

		private void ModifyOffset()
		{
			if (!base.enabled)
			{
				return;
			}
			if (this.weight <= 0f)
			{
				return;
			}
			if (this.deltaTime <= 0f)
			{
				return;
			}
			if (this.ik == null)
			{
				return;
			}
			this.weight = Mathf.Clamp(this.weight, 0f, 1f);
			this.OnModifyOffset();
			this.lastTime = Time.time;
		}

		protected virtual void OnDestroy()
		{
			if (this.ik != null)
			{
				IKSolverVR solver = this.ik.solver;
				solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPreUpdate, new IKSolver.UpdateDelegate(this.ModifyOffset));
			}
		}

		[Tooltip("The master weight")]
		public float weight = 1f;

		[Tooltip("Reference to the VRIK component")]
		public VRIK ik;

		private float lastTime;
	}
}
