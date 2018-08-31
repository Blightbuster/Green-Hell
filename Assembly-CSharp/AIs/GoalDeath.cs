using System;

namespace AIs
{
	public class GoalDeath : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_Die = (base.CreateAction(typeof(Die)) as Die);
		}

		public override bool ShouldPerform()
		{
			return this.m_AI.IsDead();
		}

		protected override void Prepare()
		{
			base.Prepare();
			base.AddToPlan(this.m_Die);
		}

		private Die m_Die;
	}
}
