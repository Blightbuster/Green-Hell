using System;
using UnityEngine;

namespace AIs
{
	public class GoalLieDown : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_LieDown = (base.CreateAction(typeof(LieDown)) as LieDown);
			this.m_LieIdle = (base.CreateAction(typeof(LieIdle)) as LieIdle);
			this.m_LieDownToStandUp = (base.CreateAction(typeof(LieDownToStandUp)) as LieDownToStandUp);
		}

		public override bool ShouldPerform()
		{
			return true;
		}

		protected override void Prepare()
		{
			base.Prepare();
			base.AddToPlan(this.m_LieDown);
			this.m_LieIdle.SetupParams(UnityEngine.Random.Range(this.m_MinDuration, this.m_MaxDuration));
			base.AddToPlan(this.m_LieIdle);
			base.AddToPlan(this.m_LieDownToStandUp);
		}

		private float m_MinDuration = 60f;

		private float m_MaxDuration = 120f;

		private LieDown m_LieDown;

		private LieIdle m_LieIdle;

		private LieDownToStandUp m_LieDownToStandUp;
	}
}
