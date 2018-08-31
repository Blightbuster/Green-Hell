using System;
using UnityEngine;

namespace AIs
{
	public class HostileStateModule : AIModule
	{
		public override void OnUpdate()
		{
			base.OnUpdate();
			switch (this.m_State)
			{
			case HostileStateModule.State.Calm:
				if (this.m_AI.m_EnemyModule.m_Enemy != null)
				{
					this.m_State = HostileStateModule.State.Upset;
				}
				break;
			case HostileStateModule.State.Upset:
				if (this.m_AI.m_EnemyModule.m_Enemy == null)
				{
					this.m_State = HostileStateModule.State.Calm;
				}
				else if (Player.Get().IsRunning() || Vector3.Distance(Player.Get().transform.position, base.transform.position) < this.m_AI.m_Params.m_AttackRange * 2f)
				{
					this.m_State = HostileStateModule.State.Aggressive;
				}
				break;
			case HostileStateModule.State.Aggressive:
				if (this.m_AI.m_EnemyModule.m_Enemy == null)
				{
					this.m_State = HostileStateModule.State.Calm;
				}
				else if (this.m_AI.m_MoraleModule.m_Morale <= 0f)
				{
					this.m_State = HostileStateModule.State.Scared;
				}
				break;
			case HostileStateModule.State.Scared:
				if (this.m_AI.m_EnemyModule.m_Enemy == null)
				{
					this.m_State = HostileStateModule.State.Calm;
				}
				else if (this.m_AI.m_MoraleModule.m_Morale > 0.5f)
				{
					this.m_State = HostileStateModule.State.Upset;
				}
				break;
			}
		}

		public HostileStateModule.State m_State;

		public enum State
		{
			Calm,
			Upset,
			Aggressive,
			Scared
		}
	}
}
