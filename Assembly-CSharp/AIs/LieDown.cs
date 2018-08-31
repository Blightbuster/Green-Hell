using System;

namespace AIs
{
	public class LieDown : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "LieDown";
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}
	}
}
