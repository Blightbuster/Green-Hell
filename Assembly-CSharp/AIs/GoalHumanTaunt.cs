using System;
using UnityEngine;

namespace AIs
{
	public class GoalHumanTaunt : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_StopCrouch = (base.CreateAction(typeof(StopCrouch)) as StopCrouch);
			this.m_Taunt = (base.CreateAction(typeof(Taunt)) as Taunt);
		}

		protected override void Prepare()
		{
			base.Prepare();
			if (this.m_AI.m_GoalsModule.m_CurrentAction.GetType() == typeof(CrouchIdle))
			{
				base.StartAction(this.m_StopCrouch);
				return;
			}
			this.SetupAction();
		}

		private void SetupAction()
		{
			if (!this.m_AI.m_EnemyModule.m_Enemy)
			{
				base.StartAction(this.m_Taunt);
				return;
			}
			if (Vector3.Angle((this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).GetNormalized2D(), this.m_AI.transform.forward.GetNormalized2D()) >= 10f)
			{
				HumanRotateTo humanRotateTo = base.CreateAction(typeof(HumanRotateTo)) as HumanRotateTo;
				humanRotateTo.SetupParams(this.m_AI.m_EnemyModule.m_Enemy.transform.position, 10f);
				base.StartAction(humanRotateTo);
			}
		}

		public override void OnStopAction(AIAction action)
		{
			base.OnStopAction(action);
			if (action.GetType() == typeof(StopCrouch) || action.GetType() == typeof(HumanRotateTo))
			{
				this.SetupAction();
			}
		}

		private const float MAX_ANGLE = 10f;

		private StopCrouch m_StopCrouch;

		private Taunt m_Taunt;
	}
}
