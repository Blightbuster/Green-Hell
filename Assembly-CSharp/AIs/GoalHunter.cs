using System;

namespace AIs
{
	public class GoalHunter : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_HunterAI = (HunterAI)ai;
			DebugUtils.Assert(this.m_HunterAI, true);
		}

		protected HunterAI m_HunterAI;
	}
}
