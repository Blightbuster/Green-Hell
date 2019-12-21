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
			this.m_RotateTo = (base.CreateAction(typeof(RotateTo)) as RotateTo);
		}

		public override bool ShouldPerform()
		{
			if (!this.m_AI.m_EnemyModule || !this.m_AI.m_EnemyModule.m_Enemy)
			{
				return false;
			}
			if (this.m_AI.transform.position.Distance(this.m_AI.m_EnemyModule.m_Enemy.transform.position) > this.m_AI.m_Params.m_AttackRange * 2f)
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
			if (this.m_AI.IsCat() && this.m_AI.m_EnemyModule.m_Enemy)
			{
				Vector3 normalized2D = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).GetNormalized2D();
				Vector3 normalized2D2 = this.m_AI.transform.forward.GetNormalized2D();
				if (Vector3.Angle(normalized2D, normalized2D2) > 75f)
				{
					this.m_RotateTo.SetupParams(this.m_AI.m_EnemyModule.m_Enemy.gameObject, true);
					base.StartAction(this.m_RotateTo);
				}
			}
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

		private RotateTo m_RotateTo;
	}
}
