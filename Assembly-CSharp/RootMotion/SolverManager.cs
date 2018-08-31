using System;
using UnityEngine;

namespace RootMotion
{
	public class SolverManager : MonoBehaviour
	{
		public void Disable()
		{
			Debug.Log("IK.Disable() is deprecated. Use enabled = false instead", base.transform);
			base.enabled = false;
		}

		protected virtual void InitiateSolver()
		{
		}

		protected virtual void UpdateSolver()
		{
		}

		protected virtual void FixTransforms()
		{
		}

		private void OnDisable()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			this.Initiate();
		}

		private void Start()
		{
			this.Initiate();
		}

		private bool animatePhysics
		{
			get
			{
				if (this.animator != null)
				{
					return this.animator.updateMode == AnimatorUpdateMode.AnimatePhysics;
				}
				return this.legacy != null && this.legacy.animatePhysics;
			}
		}

		private void Initiate()
		{
			if (this.componentInitiated)
			{
				return;
			}
			this.FindAnimatorRecursive(base.transform, true);
			this.InitiateSolver();
			this.componentInitiated = true;
		}

		private void Update()
		{
			if (this.skipSolverUpdate)
			{
				return;
			}
			if (this.animatePhysics)
			{
				return;
			}
			if (this.fixTransforms)
			{
				this.FixTransforms();
			}
		}

		private void FindAnimatorRecursive(Transform t, bool findInChildren)
		{
			if (this.isAnimated)
			{
				return;
			}
			this.animator = t.GetComponent<Animator>();
			this.legacy = t.GetComponent<Animation>();
			if (this.isAnimated)
			{
				return;
			}
			if (this.animator == null && findInChildren)
			{
				this.animator = t.GetComponentInChildren<Animator>();
			}
			if (this.legacy == null && findInChildren)
			{
				this.legacy = t.GetComponentInChildren<Animation>();
			}
			if (!this.isAnimated && t.parent != null)
			{
				this.FindAnimatorRecursive(t.parent, false);
			}
		}

		private bool isAnimated
		{
			get
			{
				return this.animator != null || this.legacy != null;
			}
		}

		private void FixedUpdate()
		{
			if (this.skipSolverUpdate)
			{
				this.skipSolverUpdate = false;
			}
			this.updateFrame = true;
			if (this.animatePhysics && this.fixTransforms)
			{
				this.FixTransforms();
			}
		}

		private void LateUpdate()
		{
			if (this.skipSolverUpdate)
			{
				return;
			}
			if (!this.animatePhysics)
			{
				this.updateFrame = true;
			}
			if (!this.updateFrame)
			{
				return;
			}
			this.updateFrame = false;
			this.UpdateSolver();
		}

		public void UpdateSolverExternal()
		{
			if (!base.enabled)
			{
				return;
			}
			this.skipSolverUpdate = true;
			this.UpdateSolver();
		}

		[Tooltip("If true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance. Not recommended for CCD and FABRIK solvers.")]
		public bool fixTransforms = true;

		private Animator animator;

		private Animation legacy;

		private bool updateFrame;

		private bool componentInitiated;

		private bool skipSolverUpdate;
	}
}
