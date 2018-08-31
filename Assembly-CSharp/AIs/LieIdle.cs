using System;

namespace AIs
{
	public class LieIdle : Idle
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "LieIdle";
		}
	}
}
