using System;
using UnityEngine;

namespace AIs
{
	public class GoalIdle : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_Idle = (base.CreateAction(typeof(Idle)) as Idle);
			this.m_SpecificIdle = (base.CreateAction(typeof(SpecificIdle)) as SpecificIdle);
		}

		public override bool ShouldPerform()
		{
			return true;
		}

		protected override void Prepare()
		{
			base.Prepare();
			if (this.m_AI.m_AnimationModule.m_HasSpecificIdle && UnityEngine.Random.Range(0f, 1f) <= this.m_SpecificIdleChance)
			{
				base.AddToPlan(this.m_SpecificIdle);
			}
			this.m_Idle.SetupParams(UnityEngine.Random.Range(this.m_MinDuration, this.m_MaxDuration));
			base.AddToPlan(this.m_Idle);
		}

		private float m_MinDuration = 3f;

		private float m_MaxDuration = 10f;

		private Idle m_Idle;

		private SpecificIdle m_SpecificIdle;

		private float m_SpecificIdleChance = 0.2f;
	}
}
