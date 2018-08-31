using System;

namespace AIs
{
	public class GoalRoar : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_Roar = (base.CreateAction(typeof(Roar)) as Roar);
		}

		public override bool ShouldPerform()
		{
			return this.m_AI.m_GoalsModule.m_PrevGoal != null && this.m_AI.m_GoalsModule.m_PrevGoal.m_Type == AIGoalType.Attack;
		}

		protected override void Prepare()
		{
			base.Prepare();
			base.AddToPlan(this.m_Roar);
		}

		public override bool ShouldLookAtPlayer()
		{
			return true;
		}

		public override bool ShouldRotateToPlayer()
		{
			return true;
		}

		private Roar m_Roar;
	}
}
