using System;

namespace AIs
{
	public class Eat : Idle
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "Eat";
		}
	}
}
