using System;
using CJTools;
using UnityEngine;

namespace AIs
{
	public class GoalCaimanAttack : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_Attack = (base.CreateAction(typeof(Attack)) as Attack);
		}

		public override bool ShouldPerform()
		{
			Being enemy = this.m_AI.m_EnemyModule.m_Enemy;
			if (!enemy)
			{
				return false;
			}
			if (enemy.IsDead())
			{
				return false;
			}
			if (this.m_Active)
			{
				return true;
			}
			float num = enemy.transform.position.Distance(this.m_AI.transform.position);
			return num <= this.m_AI.m_Params.m_AttackRange;
		}

		protected override void Prepare()
		{
			base.Prepare();
			base.AddToPlan(this.m_Attack);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			this.UpdateBlend();
		}

		private void UpdateBlend()
		{
			Vector3 normalized2D = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.GetHeadTransform().position).GetNormalized2D();
			float b = normalized2D.AngleSigned(this.m_AI.transform.forward.GetNormalized2D(), Vector3.up);
			float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, b, 45f, -45f);
			this.m_AI.m_AnimationModule.SetWantedAttackBlend(proportionalClamp);
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			this.m_AI.m_AnimationModule.ResetWantedBlend();
		}

		public override bool ShouldLookAtPlayer()
		{
			return true;
		}

		public override bool ShouldRotateToPlayer()
		{
			return true;
		}

		private Attack m_Attack;
	}
}
