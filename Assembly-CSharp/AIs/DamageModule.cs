using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class DamageModule : AIModule
	{
		public static Vector3 GetHitDirLocal(AnimEventID event_id)
		{
			Vector3 result = Vector3.left;
			if (event_id == AnimEventID.GiveDamageRArm)
			{
				result = Vector3.right;
			}
			else if (event_id == AnimEventID.GiveDamageLLeg)
			{
				result.Set(-0.1f, 1f, 0f);
				result.Normalize();
			}
			else if (event_id == AnimEventID.GiveDamageRLeg)
			{
				result.Set(0.1f, 1f, 0f);
				result.Normalize();
			}
			return result;
		}

		public override void OnAnimEvent(AnimEventID id)
		{
			base.OnAnimEvent(id);
			if (this.m_AI.m_HealthModule && this.m_AI.m_HealthModule.m_IsDead)
			{
				return;
			}
			if (id == AnimEventID.GiveDamage || id == AnimEventID.GiveDamageLArm || id == AnimEventID.GiveDamageRArm || id == AnimEventID.GiveDamageLLeg || id == AnimEventID.GiveDamageRLeg)
			{
				if (this.m_BlockDamage)
				{
					this.m_BlockDamage = false;
					return;
				}
				Vector3 world_hit_dir = this.m_AI.transform.TransformVector(DamageModule.GetHitDirLocal(id));
				if (!this.GivePlayerDamage(world_hit_dir))
				{
					this.GiveConstructionDamage();
				}
			}
		}

		public bool GivePlayerDamage(Vector3 world_hit_dir)
		{
			bool result = false;
			float num = Player.Get() ? Player.Get().transform.position.Distance(this.m_AI.transform.position) : float.MaxValue;
			float num2 = this.m_AI.m_Params.m_AttackRange;
			if (AI.IsSnake(this.m_AI.m_ID))
			{
				num2 *= 1.5f;
			}
			if (this.m_AI.m_ID == AI.AIID.BlackCaiman)
			{
				num = Player.Get().transform.position.Distance(this.m_AI.GetHeadTransform().position);
				if (!this.m_AI.IsSwimming() && (this.m_AI.GetHeadTransform().position.y < Player.Get().transform.position.y - 1f || this.m_AI.GetHeadTransform().position.y > Player.Get().GetHeadTransform().position.y + 0.75f))
				{
					if (this.m_AI.m_GoalsModule.m_ActiveGoal != null)
					{
						this.m_AI.m_GoalsModule.m_ActiveGoal.m_WasDamage = true;
					}
					return result;
				}
			}
			if (num <= num2 || this.m_AI.m_ID == AI.AIID.BrasilianWanderingSpider)
			{
				float damage = this.m_AI.m_Params.m_Damage;
				DamageType damageType = this.m_AI.m_Params.m_DamageType;
				HumanAI humanAI = this.m_AI.IsHuman() ? ((HumanAI)this.m_AI) : null;
				if (humanAI)
				{
					Weapon weapon = (Weapon)humanAI.m_CurrentWeapon;
					if (weapon)
					{
						damage = ((WeaponInfo)weapon.m_Info).m_PlayerDamage;
						damageType = weapon.GetDamageType();
					}
				}
				bool critical_hit = UnityEngine.Random.Range(0f, 1f) < 0.05f * (PlayerConditionModule.Get().GetHP() / PlayerConditionModule.Get().GetMaxHP());
				Player.Get().GiveDamage(base.gameObject, null, damage, this.m_AI.IsHumanAI() ? UnityEngine.Random.insideUnitSphere : world_hit_dir, damageType, this.m_AI.m_Params.m_PoisonLevel, critical_hit);
				result = true;
			}
			if (this.m_AI.m_GoalsModule.m_ActiveGoal != null)
			{
				this.m_AI.m_GoalsModule.m_ActiveGoal.m_WasDamage = true;
			}
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
			Vector3 forward = humanAI.transform.forward;
			float damage = this.m_AI.m_Params.m_Damage;
			DamageType damageType = this.m_AI.m_Params.m_DamageType;
			Weapon weapon = (Weapon)humanAI.m_CurrentWeapon;
			if (weapon)
			{
				float playerDamage = ((WeaponInfo)weapon.m_Info).m_PlayerDamage;
				weapon.GetDamageType();
			}
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.m_Damage = 10f;
			humanAI.m_SelectedConstruction.TakeDamage(damageInfo);
		}

		[HideInInspector]
		public bool m_BlockDamage;
	}
}
