using System;
using UnityEngine;

namespace AIs
{
	public class HealthModule : AIModule
	{
		public override void Initialize()
		{
			base.Initialize();
			this.m_Health = this.m_AI.m_Params.m_Health;
		}

		public float GetHealth()
		{
			return this.m_Health;
		}

		public bool IsMaxHealth()
		{
			return this.m_Health == this.m_AI.m_Params.m_Health;
		}

		public void DecreaseHealth(float dec)
		{
			this.m_Health -= dec;
			this.m_Health = Mathf.Clamp(this.m_Health, 0f, this.m_AI.m_Params.m_Health);
		}

		private void IncreaseHealth(float dec)
		{
			this.m_Health += dec;
			this.m_Health = Mathf.Clamp(this.m_Health, 0f, this.m_AI.m_Params.m_Health);
		}

		public override void OnTakeDamage(DamageInfo info)
		{
			if (this.m_IsDead)
			{
				return;
			}
			base.OnTakeDamage(info);
			if (AI.IsTurtle(this.m_AI.m_ID))
			{
				if (this.m_AI.m_GoalsModule.m_ActiveGoal == null || this.m_AI.m_GoalsModule.m_ActiveGoal.m_Type != AIGoalType.Hide)
				{
					this.m_AI.m_GoalsModule.ActivateGoal(AIGoalType.Hide);
				}
				return;
			}
			this.DecreaseHealth(info.m_Damage);
			if (this.m_Health == 0f)
			{
				this.Die();
			}
			else
			{
				if (this.CanPunchBack())
				{
					this.m_AI.m_GoalsModule.ActivateGoal(AIGoalType.PunchBack);
				}
				else
				{
					this.m_AI.m_GoalsModule.ActivateGoal(AIGoalType.ReactOnHit);
				}
				if (AI.IsSnake(this.m_AI.m_ID) && info.m_Damager == Player.Get().gameObject && FistFightController.Get().IsActive())
				{
					this.m_AI.m_DamageModule.GivePlayerDamage();
					this.m_AI.m_SnakeSoundModule.PlayAttackSound();
				}
			}
		}

		private bool CanPunchBack()
		{
			if (this.m_AI.IsHuman() || this.m_AI.m_GoalsModule.m_PunchBackGoal == null)
			{
				return false;
			}
			float num = UnityEngine.Random.Range(0f, 1f);
			if (this.m_AI.m_GoalsModule.m_PunchBackGoal.m_Probability <= num)
			{
				return false;
			}
			float num2 = base.transform.position.Distance(Player.Get().transform.position);
			return num2 <= this.m_AI.m_Params.m_AttackRange;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			this.UpdateHealth();
		}

		private void UpdateHealth()
		{
			if (!this.m_IsDead && this.m_Health < this.m_AI.m_Params.m_Health)
			{
				this.IncreaseHealth(this.m_AI.m_Params.m_HealthRegeneration * MainLevel.s_GameTimeDelta);
			}
		}

		public void Die()
		{
			this.m_Health = 0f;
			this.m_IsDead = true;
			this.m_AI.OnDie();
		}

		private float m_Health;

		[HideInInspector]
		public bool m_IsDead;
	}
}
