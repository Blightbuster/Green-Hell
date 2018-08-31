using System;

namespace AIs
{
	public class Jump : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "Jump";
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}
	}
}
