using System;

namespace AIs
{
	public class JumpBack : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "JumpBack";
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}

		protected override void SetupTransitionDuration()
		{
			this.m_AI.m_AnimationModule.m_TransitionDuration = 0.1f;
		}
	}
}
