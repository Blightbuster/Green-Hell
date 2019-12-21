using System;
using System.Collections.Generic;

namespace AIs
{
	public class EnemySenseModule : AIModule
	{
		public override void OnUpdate()
		{
			base.OnUpdate();
			this.UpdateEnemy();
		}

		private void OnDisable()
		{
			this.m_Enemies.Clear();
		}

		private void UpdateEnemy()
		{
			this.m_Enemies.Clear();
			for (int i = 0; i < ReplicatedLogicalPlayer.s_AllLogicalPlayers.Count; i++)
			{
				Being component = ReplicatedLogicalPlayer.s_AllLogicalPlayers[i].GetComponent<Being>();
				if (base.transform.position.Distance(component.transform.position) < this.m_AI.m_Params.m_EnemySenseRange)
				{
					this.m_Enemies.Add(component);
				}
			}
		}

		public List<Being> m_Enemies = new List<Being>();

		private List<Being> m_TempEnemyList = new List<Being>();
	}
}
