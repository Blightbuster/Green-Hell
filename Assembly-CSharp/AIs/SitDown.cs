using System;

namespace AIs
{
	public class SitDown : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "SitDown";
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}
	}
}
