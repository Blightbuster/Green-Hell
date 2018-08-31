using System;
using Enums;

namespace AIs
{
	public class GoalMove : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_MoveTo = (base.CreateAction(typeof(MoveTo)) as MoveTo);
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_MoveTo.SetupParams(this);
			base.AddToPlan(this.m_MoveTo);
		}

		public virtual AIMoveStyle GetWantedMoveStyle()
		{
			return AIMoveStyle.Walk;
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
		}

		private MoveTo m_MoveTo;
	}
}
