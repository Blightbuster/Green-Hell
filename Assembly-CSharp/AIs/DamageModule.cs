using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class DamageModule : AIModule
	{
		public override void OnAnimEvent(AnimEventID id)
		{
			base.OnAnimEvent(id);
			if (this.m_AI.m_HealthModule && this.m_AI.m_HealthModule.m_IsDead)
			{
				return;
			}
			if (id == AnimEventID.GiveDamage && !this.GivePlayerDamage())
			{
				this.GiveConstructionDamage();
			}
		}

		public bool GivePlayerDamage()
		{
			bool result = false;
			float num = (!Player.Get()) ? float.MaxValue : Player.Get().transform.position.Distance(this.m_AI.transform.position);
			float num2 = this.m_AI.m_Params.m_AttackRange;
			if (AI.IsSnake(this.m_AI.m_ID))
			{
				num2 *= 1.5f;
			}
			if (num <= num2 || this.m_AI.m_ID == AI.AIID.BrasilianWanderingSpider)
			{
				Vector3 hit_dir = Player.Get().transform.position + Vector3.up * Player.Get().GetComponent<CharacterController>().height * 0.5f - this.m_AI.transform.position;
				float damage = this.m_AI.m_Params.m_Damage;
				DamageType damageType = this.m_AI.m_Params.m_DamageType;
				HumanAI humanAI = (!this.m_AI.IsHuman()) ? null : ((HumanAI)this.m_AI);
				if (humanAI)
				{
					Weapon weapon = (Weapon)humanAI.m_CurrentWeapon;
					if (weapon)
					{
						damage = ((WeaponInfo)weapon.m_Info).m_PlayerDamage;
						damageType = weapon.GetDamageType();
					}
				}
				float num3 = UnityEngine.Random.Range(0f, 1f);
				bool critical_hit = num3 < 0.05f * (PlayerConditionModule.Get().GetHP() / PlayerConditionModule.Get().GetMaxHP());
				Player.Get().GiveDamage(base.gameObject, null, damage, hit_dir, damageType, this.m_AI.m_Params.m_PoisonLevel, critical_hit);
				result = true;
			}
			this.m_AI.m_GoalsModule.m_ActiveGoal.m_WasDamage = true;
			return result;
		}

		private void GiveConstructionDamage()
		{
			if (!this.m_AI.IsHuman())
			{
				return;
			}
			HumanAI humanAI = (HumanAI)this.m_AI;
			if (!humanAI.m_SelectedConstruction)
			{
				return;
			}
			float num = humanAI.m_SelectedConstruction.m_BoxCollider.ClosestPointOnBounds(this.m_AI.transform.position).Distance(this.m_AI.transform.position);
			if (num <= this.m_AI.m_Params.m_AttackRange)
			{
				Vector3 forward = humanAI.transform.forward;
				float num2 = this.m_AI.m_Params.m_Damage;
				DamageType damageType = this.m_AI.m_Params.m_DamageType;
				Weapon weapon = (Weapon)humanAI.m_CurrentWeapon;
				if (weapon)
				{
					num2 = ((WeaponInfo)weapon.m_Info).m_PlayerDamage;
					damageType = weapon.GetDamageType();
				}
				DamageInfo damageInfo = new DamageInfo();
				damageInfo.m_Damage = 10f;
				humanAI.m_SelectedConstruction.TakeDamage(damageInfo);
			}
		}
	}
}
