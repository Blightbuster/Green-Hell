using System;

namespace AIs
{
	public class Roar : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "Roar";
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}
	}
}
