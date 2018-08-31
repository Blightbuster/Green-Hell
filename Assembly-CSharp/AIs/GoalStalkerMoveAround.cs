using System;
using Enums;

namespace AIs
{
	public class GoalStalkerMoveAround : GoalMove
	{
		public override bool ShouldPerform()
		{
			return Stalker.Get().m_State == Stalker.State.MoveAround;
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_AI.m_PathModule.CalcPath(PathModule.PathType.StalkerMoveAround);
		}

		public override AIMoveStyle GetWantedMoveStyle()
		{
			return AIMoveStyle.Walk;
		}

		public override bool ShouldLookAtPlayer()
		{
			return true;
		}

		public override bool ShouldRotateToPlayer()
		{
			return true;
		}
	}
}
