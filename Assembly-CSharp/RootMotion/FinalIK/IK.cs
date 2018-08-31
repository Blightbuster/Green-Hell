using System;

namespace RootMotion.FinalIK
{
	public abstract class IK : SolverManager
	{
		public abstract IKSolver GetIKSolver();

		protected override void UpdateSolver()
		{
			if (!this.GetIKSolver().initiated)
			{
				this.InitiateSolver();
			}
			if (!this.GetIKSolver().initiated)
			{
				return;
			}
			this.GetIKSolver().Update();
		}

		protected override void InitiateSolver()
		{
			if (this.GetIKSolver().initiated)
			{
				return;
			}
			this.GetIKSolver().Initiate(base.transform);
		}

		protected override void FixTransforms()
		{
			if (!this.GetIKSolver().initiated)
			{
				return;
			}
			this.GetIKSolver().FixTransforms();
		}

		protected abstract void OpenUserManual();

		protected abstract void OpenScriptReference();
	}
}
