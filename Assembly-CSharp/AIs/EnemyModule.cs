using System;
using UnityEngine;

namespace AIs
{
	public class EnemyModule : AIModule
	{
		public override void OnUpdate()
		{
			base.OnUpdate();
			this.UpdateEnemy();
		}

		private void OnDisable()
		{
			this.m_Enemy = null;
		}

		private void UpdateEnemy()
		{
			if (this.m_AI.IsHuman())
			{
				HumanAI humanAI = (HumanAI)this.m_AI;
				if (humanAI.GetState() == HumanAI.State.Attack || humanAI.GetState() == HumanAI.State.StartWave)
				{
					this.SetEnemy(true);
					return;
				}
			}
			if (this.m_AI.m_SightModule && this.m_AI.m_SightModule.m_PlayerVisible)
			{
				this.SetEnemy(true);
				return;
			}
			bool flag = this.m_AI.m_HearingModule && this.m_AI.m_HearingModule.m_Noise != null;
			if (flag)
			{
				this.SetEnemy(true);
				return;
			}
			if (this.m_AI.m_EnemySenseModule && this.m_AI.m_EnemySenseModule.m_Enemy)
			{
				this.SetEnemy(true);
				return;
			}
			if (this.m_Enemy)
			{
				this.m_SafeDuration += Time.deltaTime;
				if (this.m_SafeDuration >= this.m_TimeToLooseEnemy)
				{
					this.SetEnemy(false);
				}
			}
		}

		public override void OnTakeDamage(DamageInfo info)
		{
			base.OnTakeDamage(info);
			if (!base.enabled)
			{
				return;
			}
			if (this.m_Enemy)
			{
				return;
			}
			if (info.m_Damager.gameObject == Player.Get().gameObject)
			{
				this.SetEnemy(true);
			}
			else
			{
				Item component = info.m_Damager.gameObject.GetComponent<Item>();
				if (component && component.m_Thrower.gameObject == Player.Get().gameObject)
				{
					this.SetEnemy(true);
				}
			}
		}

		public void SetEnemy(bool set)
		{
			if ((set && this.m_Enemy) || (!set && !this.m_Enemy))
			{
				return;
			}
			this.m_Enemy = ((!set) ? null : Player.Get());
			if (this.m_Enemy)
			{
				this.m_SafeDuration = 0f;
			}
		}

		public Player m_Enemy;

		private float m_TimeToLooseEnemy = 5f;

		private float m_SafeDuration;
	}
}
