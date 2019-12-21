using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class EnemyModule : AIModule
	{
		public override void OnUpdate()
		{
			base.OnUpdate();
			this.UpdateEnemy();
			if (this.m_Enemy)
			{
				Debug.DrawLine(base.transform.position, this.m_Enemy.transform.position, Color.red);
			}
		}

		private void OnDisable()
		{
			this.m_Enemy = null;
		}

		private void UpdateEnemy()
		{
			this.m_TempEnemyList.Clear();
			for (int i = 0; i < ReplicatedLogicalPlayer.s_AllLogicalPlayers.Count; i++)
			{
				Being component = ReplicatedLogicalPlayer.s_AllLogicalPlayers[i].GetComponent<Being>();
				if (this.CanSetEnemy(component))
				{
					this.m_TempEnemyList.Add(component);
				}
			}
			if (this.m_TempEnemyList.Count == 0)
			{
				return;
			}
			if (this.m_TempEnemyList.Count > 1)
			{
				float num = float.MaxValue;
				Being enemy = null;
				foreach (Being being in this.m_TempEnemyList)
				{
					float num2 = base.transform.position.Distance(being.transform.position);
					if (num2 < num)
					{
						enemy = being;
						num = num2;
					}
				}
				this.SetEnemy(enemy);
				return;
			}
			this.SetEnemy(this.m_TempEnemyList[0]);
		}

		private bool CanSetEnemy(Being being)
		{
			if (being.GetPlayerComponent<ReplicatedPlayerParams>().m_IsInSafeZone)
			{
				return false;
			}
			if (being.GetPlayerComponent<ReplicatedPlayerParams>().m_IsDead)
			{
				return false;
			}
			if (this.m_AI.IsHuman())
			{
				HumanAI humanAI = (HumanAI)this.m_AI;
				if ((humanAI.GetState() == HumanAI.State.Attack || humanAI.GetState() == HumanAI.State.StartWave) && this.m_AI.m_EnemyModule.m_Enemy == being)
				{
					return true;
				}
			}
			if (this.m_AI.m_BleedingDamage > 0f)
			{
				return true;
			}
			if (this.m_AI.m_SightModule && this.m_AI.m_SightModule.m_VisiblePlayers.Contains(being))
			{
				return true;
			}
			if (this.m_AI.m_HearingModule && this.m_AI.m_HearingModule.m_Noise != null)
			{
				return true;
			}
			if (this.m_AI.m_EnemySenseModule && this.m_AI.m_EnemySenseModule.m_Enemies.Contains(being))
			{
				return true;
			}
			if (this.m_Enemy)
			{
				this.m_SafeDuration += Time.deltaTime;
				float safeDuration = this.m_SafeDuration;
				float timeToLooseEnemy = this.m_TimeToLooseEnemy;
				return false;
			}
			return false;
		}

		public override void OnTakeDamage(DamageInfo info)
		{
			base.OnTakeDamage(info);
			if (!base.enabled || !info.m_Damager)
			{
				return;
			}
			if (info.m_Damager.gameObject.GetComponent<ReplicatedLogicalPlayer>())
			{
				this.SetEnemy(info.m_Damager.gameObject.GetComponent<Being>());
				return;
			}
			Item component = info.m_Damager.gameObject.GetComponent<Item>();
			if (component && component.m_Thrower && component.m_Thrower.gameObject.GetComponent<ReplicatedLogicalPlayer>())
			{
				this.SetEnemy(info.m_Damager.gameObject.GetComponent<Being>());
			}
		}

		public void SetEnemy(Being enemy)
		{
			if (this.m_Enemy == enemy)
			{
				return;
			}
			this.m_Enemy = enemy;
			if (this.m_Enemy)
			{
				this.m_SafeDuration = 0f;
			}
		}

		public Being m_Enemy;

		private float m_TimeToLooseEnemy = 5f;

		private float m_SafeDuration;

		private List<Being> m_TempEnemyList = new List<Being>();
	}
}
