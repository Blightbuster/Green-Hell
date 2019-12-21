using System;
using Enums;

namespace AIs
{
	public class GoalSpearmanStrafe : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_SpearmanStrafe = (base.CreateAction(typeof(SpearmanStrafe)) as SpearmanStrafe);
		}

		public override bool ShouldPerform()
		{
			return this.m_Active || (this.m_AI.m_GoalsModule.m_PrevGoal != null && this.m_AI.m_GoalsModule.m_PrevGoal.m_Type == AIGoalType.HumanAttack);
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_AI.m_MoveStyle = AIMoveStyle.Walk;
			this.SetupAction();
		}

		private void SetupAction()
		{
			base.StartAction(this.m_SpearmanStrafe);
		}

		protected Direction m_Direction;

		private SpearmanStrafe m_SpearmanStrafe;
	}
}
