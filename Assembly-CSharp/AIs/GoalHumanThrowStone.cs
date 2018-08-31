using System;
using UnityEngine;

namespace AIs
{
	public class GoalHumanThrowStone : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_ThrowStone = (base.CreateAction(typeof(ThrowStone)) as ThrowStone);
		}

		public override bool ShouldPerform()
		{
			if (this.m_Active)
			{
				return true;
			}
			Being enemy = this.m_AI.m_EnemyModule.m_Enemy;
			if (!enemy)
			{
				return false;
			}
			if (this.m_Active)
			{
				return true;
			}
			float num = Player.Get().transform.position.Distance(this.m_AI.transform.position);
			return num < 15f && num > 6f && (this.m_AI.m_GoalsModule.m_PrevGoal == null || this.m_AI.m_GoalsModule.m_PrevGoal != this);
		}

		protected override void Prepare()
		{
			base.Prepare();
			Vector3 normalized2D = (Player.Get().transform.position - this.m_AI.transform.position).GetNormalized2D();
			float num = Vector3.Angle(normalized2D, this.m_AI.transform.forward.GetNormalized2D());
			if (num >= 10f)
			{
				HumanRotateTo humanRotateTo = base.CreateAction(typeof(HumanRotateTo)) as HumanRotateTo;
				humanRotateTo.SetupParams(Player.Get().transform.position, 10f);
				base.StartAction(humanRotateTo);
			}
			else
			{
				base.StartAction(this.m_ThrowStone);
			}
		}

		public override void OnStopAction(AIAction action)
		{
			base.OnStopAction(action);
			if (action.GetType() == typeof(HumanRotateTo))
			{
				base.StartAction(this.m_ThrowStone);
			}
		}

		private const float MAX_ANGLE = 10f;

		private ThrowStone m_ThrowStone;
	}
}
