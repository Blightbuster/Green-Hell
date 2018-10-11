using System;

namespace AIs
{
	public class Show : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "Show";
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}
	}
}
