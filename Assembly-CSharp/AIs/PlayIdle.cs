using System;

namespace AIs
{
	public class PlayIdle : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "PlayIdle";
		}

		protected override bool ShouldFinish()
		{
			return false;
		}
	}
}
