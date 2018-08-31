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
			Type type = this.m_AI.m_GoalsModule.m_CurrentAction.GetType();
			if (type == typeof(CrouchIdle))
			{
				base.StartAction(this.m_StopCrouch);
			}
			else
			{
				this.SetupAction();
			}
		}

		private void SetupAction()
		{
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
				base.StartAction(this.m_Taunt);
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
