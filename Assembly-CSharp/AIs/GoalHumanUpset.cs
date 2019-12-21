using System;

namespace AIs
{
	public class GoalHumanUpset : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_StopCrouch = (base.CreateAction(typeof(StopCrouch)) as StopCrouch);
			this.m_LookAround = (base.CreateAction(typeof(LookAround)) as LookAround);
		}

		protected override void Prepare()
		{
			base.Prepare();
			if (this.m_AI.m_GoalsModule.m_PreviousAction != null && this.m_AI.m_GoalsModule.m_PreviousAction.GetType() == typeof(CrouchIdle))
			{
				base.StartAction(this.m_StopCrouch);
				return;
			}
			base.StartAction(this.m_LookAround);
		}

		public override void OnStopAction(AIAction action)
		{
			base.OnStopAction(action);
			if (action.GetType() == typeof(StopCrouch))
			{
				base.StartAction(this.m_LookAround);
				return;
			}
			if (action.GetType() == typeof(LookAround))
			{
				this.m_HumanAI.SetState(HumanAI.State.Rest);
			}
		}

		private StopCrouch m_StopCrouch;

		private LookAround m_LookAround;
	}
}
