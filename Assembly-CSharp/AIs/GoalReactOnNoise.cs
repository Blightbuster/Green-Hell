using System;

namespace AIs
{
	public class GoalReactOnNoise : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_ReactOnNoise = (base.CreateAction(typeof(ReactOnNoise)) as ReactOnNoise);
		}

		public override bool ShouldPerform()
		{
			if (this.m_Active)
			{
				return (float)base.GetPlanCount() > 0f;
			}
			return this.m_AI.m_HearingModule.m_LowNoise != null && (this.m_AI.m_GoalsModule.m_PrevGoal == null || this.m_AI.m_GoalsModule.m_PrevGoal.m_Type != AIGoalType.ReactOnNoise);
		}

		protected override void Prepare()
		{
			base.Prepare();
			base.AddToPlan(this.m_ReactOnNoise);
		}

		private ReactOnNoise m_ReactOnNoise;
	}
}
