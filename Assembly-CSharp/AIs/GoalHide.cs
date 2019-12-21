using System;

namespace AIs
{
	public class GoalHide : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_Hide = (base.CreateAction(typeof(Hide)) as Hide);
			this.m_Show = (base.CreateAction(typeof(Show)) as Show);
		}

		public override bool ShouldPerform()
		{
			return this.m_Active || (AI.IsArmadillo(this.m_AI.m_ID) && this.m_AI.transform.position.Distance(Player.Get().transform.position) < 2f);
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_Hide.m_Finish = false;
			base.AddToPlan(this.m_Hide);
			base.AddToPlan(this.m_Show);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (this.m_AI.m_HealthModule.m_IsDead)
			{
				return;
			}
			if (this.m_Hide.IsInProgress())
			{
				if (AI.IsArmadillo(this.m_AI.m_ID))
				{
					if (this.m_AI.transform.position.Distance(Player.Get().transform.position) > 3f)
					{
						this.m_Hide.m_Finish = true;
						return;
					}
				}
				else if (base.GetDuration() >= 5f)
				{
					this.m_Hide.m_Finish = true;
				}
			}
		}

		private Hide m_Hide;

		private Show m_Show;
	}
}
