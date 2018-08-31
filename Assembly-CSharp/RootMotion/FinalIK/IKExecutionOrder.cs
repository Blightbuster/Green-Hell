using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class IKExecutionOrder : MonoBehaviour
	{
		private bool animatePhysics
		{
			get
			{
				return !(this.animator == null) && this.animator.updateMode == AnimatorUpdateMode.AnimatePhysics;
			}
		}

		private void Start()
		{
			for (int i = 0; i < this.IKComponents.Length; i++)
			{
				this.IKComponents[i].enabled = false;
			}
		}

		private void Update()
		{
			if (this.animatePhysics)
			{
				return;
			}
			this.FixTransforms();
		}

		private void FixedUpdate()
		{
			this.fixedFrame = true;
			if (this.animatePhysics)
			{
				this.FixTransforms();
			}
		}

		private void LateUpdate()
		{
			if (!this.animatePhysics || this.fixedFrame)
			{
				for (int i = 0; i < this.IKComponents.Length; i++)
				{
					this.IKComponents[i].GetIKSolver().Update();
				}
				this.fixedFrame = false;
			}
		}

		private void FixTransforms()
		{
			for (int i = 0; i < this.IKComponents.Length; i++)
			{
				if (this.IKComponents[i].fixTransforms)
				{
					this.IKComponents[i].GetIKSolver().FixTransforms();
				}
			}
		}

		[Tooltip("The IK components, assign in the order in which you wish to update them.")]
		public IK[] IKComponents;

		[Tooltip("Optional. Assign it if you are using 'Animate Physics' as the Update Mode.")]
		public Animator animator;

		private bool fixedFrame;
	}
}
