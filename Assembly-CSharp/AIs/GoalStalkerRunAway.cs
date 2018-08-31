using System;
using Enums;

namespace AIs
{
	public class GoalStalkerRunAway : GoalMove
	{
		public override bool ShouldPerform()
		{
			return Stalker.Get().m_State == Stalker.State.RunAway;
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_AI.m_PathModule.CalcPath(PathModule.PathType.StalkerRunAway);
		}

		public override AIMoveStyle GetWantedMoveStyle()
		{
			return AIMoveStyle.Run;
		}
	}
}
