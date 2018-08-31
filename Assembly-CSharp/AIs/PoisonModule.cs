using System;
using UnityEngine;

namespace AIs
{
	public class PoisonModule : AIModule
	{
		public override void OnUpdate()
		{
			base.OnUpdate();
			if (this.m_LastAttackTime == 0f || Time.time - this.m_LastAttackTime >= this.m_PoisoningInterval)
			{
				float num = Player.Get().m_LFoot.position.Distance(this.m_AI.transform.position);
				float num2 = Player.Get().m_RFoot.position.Distance(this.m_AI.transform.position);
				float num3 = (!this.m_AI.m_Visible) ? (this.m_AI.m_Params.m_AttackRange * 2f) : this.m_AI.m_Params.m_AttackRange;
				if (num <= num3 || num2 <= num3)
				{
					this.Attack();
				}
			}
		}

		private void Attack()
		{
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.m_Damager = base.gameObject;
			damageInfo.m_Damage = this.m_AI.m_Params.m_Damage;
			damageInfo.m_HitDir = Player.Get().transform.position + Vector3.up * Player.Get().GetComponent<CharacterController>().height * 0.5f - base.transform.position;
			damageInfo.m_PoisonLevel = this.m_AI.m_Params.m_PoisonLevel;
			Player.Get().TakeDamage(damageInfo);
			this.m_LastAttackTime = Time.time;
		}

		public float m_PoisoningInterval = 10f;

		private float m_LastAttackTime;
	}
}
