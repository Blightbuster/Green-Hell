using System;

namespace AIs
{
	public class StartCrouch : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "StartCrouch";
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}
	}
}
