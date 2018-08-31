using System;

namespace AIs
{
	public class GoalDontMove : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_Idle = (base.CreateAction(typeof(Idle)) as Idle);
		}

		public override bool ShouldPerform()
		{
			return this.m_AI.m_Trap != null;
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_Idle.SetupParams(float.MaxValue);
			base.AddToPlan(this.m_Idle);
		}

		private Idle m_Idle;
	}
}
