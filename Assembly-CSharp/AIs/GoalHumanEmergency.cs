using System;
using UnityEngine;

namespace AIs
{
	public class GoalHumanEmergency : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_Idle = (base.CreateAction(typeof(Idle)) as Idle);
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_Idle.SetupParams(UnityEngine.Random.Range(this.m_MinDuration, this.m_MaxDuration));
			base.AddToPlan(this.m_Idle);
			this.m_AI.m_PerformEmergency = false;
		}

		private float m_MinDuration = 2f;

		private float m_MaxDuration = 3f;

		private Idle m_Idle;
	}
}
