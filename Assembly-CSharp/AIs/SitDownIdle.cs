using System;

namespace AIs
{
	public class SitDownIdle : Idle
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "SitDownIdle";
		}
	}
}
