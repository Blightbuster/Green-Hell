using System;

namespace AIs
{
	public class StartPlay : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "StartPlay";
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}
	}
}
