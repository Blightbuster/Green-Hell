using System;

namespace AIs
{
	public class LieDownToStandUp : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "LieDownToStandUp";
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}
	}
}
