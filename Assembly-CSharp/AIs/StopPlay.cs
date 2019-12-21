using System;

namespace AIs
{
	public class StopPlay : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "StopPlay";
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}
	}
}
