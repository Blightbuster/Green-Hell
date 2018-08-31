using System;

namespace AIs
{
	public class CrouchIdle : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "CrouchIdle";
		}

		protected override bool ShouldFinish()
		{
			return false;
		}
	}
}
