using System;
using CJTools;
using Enums;
using UnityEngine;

namespace AIs
{
	public class HealthModule : AIModule, IReplicatedBehaviour
	{
		public override void Initialize(Being being)
		{
			base.Initialize(being);
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
			if (this.m_Health == 0f)
			{
				this.Die();
			}
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
			GameObject damager = info.m_Damager;
			if (damager != null && damager.IsPlayer())
			{
				this.ReplRequestOwnership(false);
			}
			base.OnTakeDamage(info);
			if (!AI.IsTurtle(this.m_AI.m_ID))
			{
				this.DecreaseHealth(info.m_Damage);
				if (this.m_Health > 0f)
				{
					if (this.CanPunchBack())
					{
						this.m_AI.m_GoalsModule.ActivateGoal(AIGoalType.PunchBack);
					}
					else if (this.m_AI.m_GoalsModule.m_ActiveGoal == null || this.m_AI.m_GoalsModule.m_ActiveGoal.m_Type != AIGoalType.Hide)
					{
						this.m_AI.m_GoalsModule.ActivateGoal(AIGoalType.ReactOnHit);
					}
					if (AI.IsSnake(this.m_AI.m_ID) && info.m_Damager.IsPlayer() && FistFightController.Get().IsActive())
					{
						int num = UnityEngine.Random.Range(0, 2);
						AnimEventID event_id = AnimEventID.GiveDamageLLeg;
						if (num == 1)
						{
							event_id = AnimEventID.GiveDamageRLeg;
						}
						Vector3 world_hit_dir = this.m_AI.transform.TransformVector(DamageModule.GetHitDirLocal(event_id));
						this.m_AI.m_DamageModule.GivePlayerDamage(world_hit_dir);
						this.m_AI.m_SnakeSoundModule.PlayAttackSound();
					}
				}
				return;
			}
			if (this.m_AI.m_GoalsModule.m_ActiveGoal == null || this.m_AI.m_GoalsModule.m_ActiveGoal.m_Type != AIGoalType.Hide)
			{
				this.m_AI.m_GoalsModule.ActivateGoal(AIGoalType.Hide);
				return;
			}
			if (this.m_AI.m_GoalsModule.m_ActiveGoal.m_Type == AIGoalType.Hide)
			{
				this.m_AI.m_GoalsModule.m_ActiveGoal.ResetDuration();
			}
		}

		private bool CanPunchBack()
		{
			if (this.m_AI.IsHuman() || this.m_AI.m_GoalsModule.m_PunchBackGoal == null)
			{
				return false;
			}
			float num = UnityEngine.Random.Range(0f, 1f);
			return this.m_AI.m_GoalsModule.m_PunchBackGoal.m_Probability > num && base.transform.position.Distance(Player.Get().transform.position) <= this.m_AI.m_Params.m_AttackRange;
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

		public void ReplOnChangedOwner(bool was_owner)
		{
		}

		public void ReplOnSpawned()
		{
		}

		public void OnReplicationPrepare()
		{
			float health = this.GetHealth();
			if (!CJTools.Math.FloatsEqual(this.m_ReplHealth, health, 2))
			{
				this.m_ReplHealth = health;
				this.ReplSetDirty();
			}
		}

		public void OnReplicationSerialize(P2PNetworkWriter writer, bool initial_state)
		{
			writer.Write(this.m_ReplHealth);
		}

		public void OnReplicationDeserialize(P2PNetworkReader reader, bool initial_state)
		{
			this.m_ReplHealth = reader.ReadFloat();
		}

		public void OnReplicationResolve()
		{
			if (this.m_ReplHealth < this.m_Health)
			{
				this.DecreaseHealth(this.m_Health - this.m_ReplHealth);
				return;
			}
			this.m_Health = this.m_ReplHealth;
		}

		private float m_Health;

		[HideInInspector]
		public bool m_IsDead;

		private float m_ReplHealth = -1f;
	}
}
