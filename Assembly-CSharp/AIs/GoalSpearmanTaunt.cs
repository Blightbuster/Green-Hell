using System;

namespace AIs
{
	public class GoalSpearmanTaunt : GoalHumanTaunt
	{
		public override bool ShouldPerform()
		{
			if (this.m_Active)
			{
				return true;
			}
			if (this.m_AI.m_GoalsModule.m_PrevGoal == null)
			{
				return false;
			}
			AIGoalType type = this.m_AI.m_GoalsModule.m_PrevGoal.m_Type;
			if (type != AIGoalType.HumanAttack)
			{
				bool flag = type == AIGoalType.HumanJumpAttack;
			}
			return (type == AIGoalType.HumanAttack || type == AIGoalType.HumanJumpAttack) && type != AIGoalType.HumanMoveToEnemy;
		}
	}
}
