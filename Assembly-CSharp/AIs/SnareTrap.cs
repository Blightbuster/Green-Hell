using System;

namespace AIs
{
	public class SnareTrap : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "SnareTrap";
		}
	}
}
