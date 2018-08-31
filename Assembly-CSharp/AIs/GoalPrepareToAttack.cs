using System;

namespace AIs
{
	public class GoalPrepareToAttack : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_PrepareToAttack = (base.CreateAction(typeof(PrepareToAttack)) as PrepareToAttack);
		}

		public override bool ShouldPerform()
		{
			Being enemy = this.m_AI.m_EnemyModule.m_Enemy;
			return enemy;
		}

		protected override void Prepare()
		{
			base.Prepare();
			base.AddToPlan(this.m_PrepareToAttack);
		}

		private PrepareToAttack m_PrepareToAttack;
	}
}
