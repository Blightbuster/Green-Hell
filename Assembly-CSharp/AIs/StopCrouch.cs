using System;

namespace AIs
{
	public class StopCrouch : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "StopCrouch";
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}
	}
}
