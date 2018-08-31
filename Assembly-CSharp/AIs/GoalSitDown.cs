using System;
using UnityEngine;

namespace AIs
{
	public class GoalSitDown : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_SitDown = (base.CreateAction(typeof(SitDown)) as SitDown);
			this.m_SitDownIdle = (base.CreateAction(typeof(SitDownIdle)) as SitDownIdle);
			this.m_SitDownToStandUp = (base.CreateAction(typeof(SitDownToStandUp)) as SitDownToStandUp);
		}

		public override bool ShouldPerform()
		{
			return true;
		}

		protected override void Prepare()
		{
			base.Prepare();
			base.AddToPlan(this.m_SitDown);
			this.m_SitDownIdle.SetupParams(UnityEngine.Random.Range(this.m_MinDuration, this.m_MaxDuration));
			base.AddToPlan(this.m_SitDownIdle);
			base.AddToPlan(this.m_SitDownToStandUp);
		}

		private float m_MinDuration = 3f;

		private float m_MaxDuration = 10f;

		private SitDown m_SitDown;

		private SitDownIdle m_SitDownIdle;

		private SitDownToStandUp m_SitDownToStandUp;
	}
}
