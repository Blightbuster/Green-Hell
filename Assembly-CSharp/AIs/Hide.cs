using System;

namespace AIs
{
	public class Hide : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "Hide";
		}

		protected override bool ShouldFinish()
		{
			return this.m_Finish;
		}

		public bool m_Finish;
	}
}
