using System;

namespace AIs
{
	public class GoalHuman : AIGoal
	{
		public override bool ShouldPerform()
		{
			return this.m_Active;
		}

		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_HumanAI = (HumanAI)ai;
			DebugUtils.Assert(this.m_HumanAI, true);
		}

		protected virtual bool IsAction(Type type)
		{
			return base.GetPlanCount() != 0 && base.GetAction(0).GetType() == type;
		}

		protected HumanAI m_HumanAI;
	}
}
