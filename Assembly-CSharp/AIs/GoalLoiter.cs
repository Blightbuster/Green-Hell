using System;
using Enums;

namespace AIs
{
	public class GoalLoiter : GoalMove
	{
		public override bool ShouldPerform()
		{
			return (!this.m_AI.m_EnemyModule || !(this.m_AI.m_EnemyModule.m_Enemy != null)) && (this.m_AI.m_Params.m_CanWalk || this.m_AI.m_Params.m_CanTrot || this.m_AI.m_Params.m_CanRun);
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_AI.m_PathModule.CalcPath(PathModule.PathType.Loiter);
		}

		public override AIMoveStyle GetWantedMoveStyle()
		{
			return AIMoveStyle.Walk;
		}
	}
}
