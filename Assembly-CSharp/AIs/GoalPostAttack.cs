using System;
using UnityEngine;

namespace AIs
{
	public class GoalPostAttack : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_PostAttack = (base.CreateAction(typeof(PostAttack)) as PostAttack);
		}

		public override bool ShouldPerform()
		{
			float num = this.m_AI.transform.position.Distance(Player.Get().transform.position);
			if (num > this.m_AI.m_Params.m_AttackRange * 2f)
			{
				return false;
			}
			if (this.m_Active)
			{
				return base.GetDuration() < this.m_Length;
			}
			return this.m_AI.m_GoalsModule.m_PrevGoal != null && (this.m_AI.m_GoalsModule.m_PrevGoal.m_Type == AIGoalType.Attack || this.m_AI.m_GoalsModule.m_PrevGoal.m_Type == AIGoalType.CaimanAttack || this.m_AI.m_GoalsModule.m_PrevGoal.m_Type == AIGoalType.ComboAttack);
		}

		protected override void Prepare()
		{
			base.Prepare();
			base.AddToPlan(this.m_PostAttack);
			this.m_Length = UnityEngine.Random.Range(this.m_MinDuration, this.m_MaxDuration);
		}

		public override bool ShouldLookAtPlayer()
		{
			return true;
		}

		public override bool ShouldRotateToPlayer()
		{
			return true;
		}

		private float m_MinDuration = 0.5f;

		private float m_MaxDuration = 1.5f;

		private float m_Length;

		private PostAttack m_PostAttack;
	}
}
