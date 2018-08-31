using System;

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
			this.m_Enemy = null;
		}

		private void UpdateEnemy()
		{
			if (Player.Get().transform.position.Distance(this.m_AI.transform.position) < this.m_AI.m_Params.m_EnemySenseRange)
			{
				this.m_Enemy = Player.Get();
			}
			else
			{
				this.m_Enemy = null;
			}
		}

		public Player m_Enemy;
	}
}
