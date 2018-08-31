using System;

namespace AIs
{
	public class GoalReturnToSpawner : GoalMove
	{
		public override bool ShouldPerform()
		{
			return this.m_AI.m_Spawner != null && !this.m_AI.m_Spawner.IsInside(this.m_AI.transform.position);
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_AI.m_PathModule.CalcPath(PathModule.PathType.ReturnToSpawner);
		}
	}
}
