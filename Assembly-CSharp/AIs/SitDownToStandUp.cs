using System;

namespace AIs
{
	public class SitDownToStandUp : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "SitDownToStandUp";
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}
	}
}
