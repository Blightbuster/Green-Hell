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
			this.m_SwimAttack = (base.CreateAction(typeof(SwimAttack)) as SwimAttack);
		}

		public override bool ShouldPerform()
		{
			Being enemy = this.m_AI.m_EnemyModule.m_Enemy;
			return enemy && !enemy.IsDead() && (this.m_Active || (enemy.GetHeadTransform().position.To2D().Distance(this.m_AI.GetHeadTransform().position.To2D()) <= (this.m_AI.IsSwimming() ? 1.3f : this.m_AI.m_Params.m_AttackRange) && (!this.m_AI.IsSwimming() || Vector3.Dot((this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).GetNormalized2D(), this.m_AI.transform.forward.GetNormalized2D()) >= 0.5f)));
		}

		protected override void Prepare()
		{
			base.Prepare();
			if (this.m_AI.IsSwimming())
			{
				base.AddToPlan(this.m_SwimAttack);
				return;
			}
			base.AddToPlan(this.m_Attack);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			this.UpdateBlend();
		}

		private void UpdateBlend()
		{
			float b = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.GetHeadTransform().position).GetNormalized2D().AngleSigned(this.m_AI.transform.forward.GetNormalized2D(), Vector3.up);
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

		private SwimAttack m_SwimAttack;
	}
}
