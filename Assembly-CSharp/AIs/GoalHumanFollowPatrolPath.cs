using System;
using Enums;

namespace AIs
{
	public class GoalHumanFollowPatrolPath : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_HumanFollowPatrolPath = (base.CreateAction(typeof(HumanFollowPatrolPath)) as HumanFollowPatrolPath);
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_AI.m_MoveStyle = AIMoveStyle.Walk;
			base.StartAction(this.m_HumanFollowPatrolPath);
		}

		private HumanFollowPatrolPath m_HumanFollowPatrolPath;
	}
}
