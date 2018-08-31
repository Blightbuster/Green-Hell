using System;

namespace AIs
{
	public class GoalHunterBowPostAttack : GoalHunter
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_PostAttack = (base.CreateAction(typeof(PostAttack)) as PostAttack);
		}

		public override bool ShouldPerform()
		{
			if (this.m_HunterAI.m_WeaponType != HumanAI.WeaponType.Primary)
			{
				return false;
			}
			if (this.m_Active)
			{
				return base.GetDuration() < this.m_Length;
			}
			if (this.m_AI.m_GoalsModule.m_PrevGoal == null)
			{
				return true;
			}
			AIGoalType type = this.m_AI.m_GoalsModule.m_PrevGoal.m_Type;
			return type == AIGoalType.HunterBowAttack;
		}

		protected override void Prepare()
		{
			base.Prepare();
			base.AddToPlan(this.m_PostAttack);
			this.m_Length = this.m_HunterAI.m_BowAttackInterval;
		}

		private float m_Length;

		private PostAttack m_PostAttack;
	}
}
