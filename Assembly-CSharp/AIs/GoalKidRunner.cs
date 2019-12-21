using System;
using Enums;

namespace AIs
{
	public class GoalKidRunner : GoalMove
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_AI.m_MoveStyle = AIMoveStyle.Run;
			this.m_MoveTo = (base.CreateAction(typeof(KidRunnerMoveTo)) as KidRunnerMoveTo);
			this.m_StartPlay = (base.CreateAction(typeof(StartPlay)) as StartPlay);
			this.m_PlayIdle = (base.CreateAction(typeof(PlayIdle)) as PlayIdle);
			this.m_StopPlay = (base.CreateAction(typeof(StopPlay)) as StopPlay);
			this.m_KidRunner = (KidRunnerAI)this.m_AI;
		}

		public override bool ShouldPerform()
		{
			return true;
		}

		protected override void Prepare()
		{
			base.StartAction(this.m_MoveTo);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			this.UpdateActions();
		}

		private void UpdateActions()
		{
			KidRunnerAI.KidState kidState = this.m_KidRunner.m_KidState;
			if (kidState != KidRunnerAI.KidState.Run)
			{
				if (kidState - KidRunnerAI.KidState.Play <= 1 && this.m_CurrentAction == this.m_MoveTo)
				{
					base.StartAction(this.m_StartPlay);
					return;
				}
			}
			else if (this.m_CurrentAction == this.m_PlayIdle)
			{
				base.StartAction(this.m_MoveTo);
			}
		}

		public override void OnStopAction(AIAction action)
		{
			base.OnStopAction(action);
			if (action.GetType() == typeof(StartPlay))
			{
				base.StartAction(this.m_PlayIdle);
			}
		}

		public override AIMoveStyle GetWantedMoveStyle()
		{
			return AIMoveStyle.Run;
		}

		private StartPlay m_StartPlay;

		private PlayIdle m_PlayIdle;

		private StopPlay m_StopPlay;

		private KidRunnerMoveTo m_MoveTo;

		private KidRunnerAI m_KidRunner;
	}
}
