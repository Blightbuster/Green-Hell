using System;
using Enums;

namespace AIs
{
	public class GoalMoveAwayFromEnemy : GoalMove
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_StartMove = (base.CreateAction(typeof(StartMove)) as StartMove);
		}

		public override bool ShouldPerform()
		{
			return !(this.m_AI.m_EnemyModule.m_Enemy == null) && (!this.m_AI.m_MoraleModule || this.m_AI.m_MoraleModule.m_Morale == 0f);
		}

		protected override void Prepare()
		{
			AIMoveStyle moveStyle = this.m_AI.m_MoveStyle;
			this.m_AI.m_MoveStyle = this.GetWantedMoveStyle();
			this.m_AI.m_PathModule.CalcPath(PathModule.PathType.MoveAwayFromEnemy);
			if (this.m_AI.m_ID != AI.AIID.Peccary && this.m_AI.m_ID != AI.AIID.Capybara)
			{
				base.Prepare();
				return;
			}
			if ((moveStyle == AIMoveStyle.Walk && this.m_AI.m_MoveStyle == AIMoveStyle.Run) || this.m_AI.m_GoalsModule.m_PreviousAction == null || (this.m_AI.m_GoalsModule.m_PreviousAction.GetType() != typeof(MoveTo) && this.m_AI.m_GoalsModule.m_PreviousAction.GetType() != typeof(StartMove)))
			{
				base.StartAction(this.m_StartMove);
				return;
			}
			base.Prepare();
		}

		public override AIMoveStyle GetWantedMoveStyle()
		{
			if (this.m_AI.m_EnemyModule.m_Enemy)
			{
				if (this.m_AI.m_Params.m_CanRun)
				{
					return AIMoveStyle.Run;
				}
				if (this.m_AI.m_Params.m_CanTrot)
				{
					return AIMoveStyle.Trot;
				}
			}
			return AIMoveStyle.Walk;
		}

		public override void OnStopAction(AIAction action)
		{
			base.OnStopAction(action);
			if (action.GetType() == typeof(StartMove))
			{
				base.Prepare();
				this.m_AI.m_AnimationModule.m_TransitionDuration = 0f;
				return;
			}
			if (action.GetType() == typeof(MoveTo))
			{
				this.Prepare();
			}
		}

		private StartMove m_StartMove;
	}
}
