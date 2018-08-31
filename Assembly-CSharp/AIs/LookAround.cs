using System;

namespace AIs
{
	public class LookAround : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "LookAround";
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}
	}
}
