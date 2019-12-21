using System;
using Enums;

namespace AIs
{
	public class GoalInTrap : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_SnareTrap = (base.CreateAction(typeof(SnareTrap)) as SnareTrap);
			this.m_Idle = (base.CreateAction(typeof(Idle)) as Idle);
		}

		public override bool ShouldPerform()
		{
			return this.m_AI.m_Trap != null;
		}

		protected override void Prepare()
		{
			base.Prepare();
			if (this.m_AI.m_Trap.m_Info.m_ID == ItemID.Snare_Trap)
			{
				this.m_SnareTrap.SetupParams();
				base.AddToPlan(this.m_SnareTrap);
				return;
			}
			this.m_Idle.SetupParams(float.MaxValue);
			base.AddToPlan(this.m_Idle);
		}

		private SnareTrap m_SnareTrap;

		private Idle m_Idle;
	}
}
