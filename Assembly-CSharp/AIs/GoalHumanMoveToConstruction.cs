using System;
using Enums;

namespace AIs
{
	public class GoalHumanMoveToConstruction : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_HumanMoveTo = (base.CreateAction(typeof(HumanMoveTo)) as HumanMoveTo);
		}

		public override bool ShouldPerform()
		{
			if (!this.m_HumanAI.m_SelectedConstruction)
			{
				return false;
			}
			float num = this.m_HumanAI.m_SelectedConstruction.transform.position.Distance(this.m_AI.transform.position);
			return num > this.m_AI.m_Params.m_AttackRange && (this.m_Active || (this.m_HumanAI.m_SelectedConstruction != null && this.m_AI.m_PathModule.CalcPath(PathModule.PathType.MoveToConstruction)));
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_AI.m_MoveStyle = AIMoveStyle.Run;
			base.StartAction(this.m_HumanMoveTo);
		}

		private HumanMoveTo m_HumanMoveTo;
	}
}
