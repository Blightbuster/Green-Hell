using System;

namespace AIs
{
	public class PrepareToAttack : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "PrepareToAttack";
		}
	}
}
