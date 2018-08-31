using System;

namespace AIs
{
	public class ReactOnNoise : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "ReactOnNoise";
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}
	}
}
