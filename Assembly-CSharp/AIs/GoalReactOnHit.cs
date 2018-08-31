using System;

namespace AIs
{
	public class GoalReactOnHit : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_HitReaction = (base.CreateAction(typeof(HitReaction)) as HitReaction);
		}

		public override bool ShouldPerform()
		{
			return this.m_Active && (float)base.GetPlanCount() > 0f;
		}

		protected override void Prepare()
		{
			base.Prepare();
			base.AddToPlan(this.m_HitReaction);
		}

		private HitReaction m_HitReaction;
	}
}
