using System;
using UnityEngine;

namespace AIs
{
	public class HostileStateModule : AIModule
	{
		private void SetState(HostileStateModule.State state)
		{
			this.m_State = state;
			this.m_EnterStateTime = Time.time;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			switch (this.m_State)
			{
			case HostileStateModule.State.Calm:
				if (this.m_AI.m_EnemyModule.m_Enemy != null)
				{
					this.SetState(HostileStateModule.State.Upset);
					return;
				}
				break;
			case HostileStateModule.State.Upset:
				if (this.m_AI.m_EnemyModule.m_Enemy == null)
				{
					this.SetState(HostileStateModule.State.Calm);
					return;
				}
				if (Player.Get().IsRunning() || Vector3.Distance(Player.Get().transform.position, base.transform.position) < this.m_AI.m_Params.m_AttackRange * 3f)
				{
					this.SetState(HostileStateModule.State.Aggressive);
					return;
				}
				break;
			case HostileStateModule.State.Aggressive:
				if (this.m_AI.m_EnemyModule.m_Enemy == null)
				{
					this.SetState(HostileStateModule.State.Calm);
					return;
				}
				if (this.m_AI.m_MoraleModule.m_Morale <= 0f)
				{
					this.SetState(HostileStateModule.State.Scared);
					return;
				}
				break;
			case HostileStateModule.State.Scared:
				if (this.m_AI.m_EnemyModule.m_Enemy == null)
				{
					this.SetState(HostileStateModule.State.Calm);
					return;
				}
				if (this.m_AI.m_MoraleModule.m_Morale > 0.5f)
				{
					this.SetState(HostileStateModule.State.Upset);
				}
				break;
			default:
				return;
			}
		}

		public override void OnTakeDamage(DamageInfo info)
		{
			base.OnTakeDamage(info);
			this.SetState(HostileStateModule.State.Aggressive);
		}

		public HostileStateModule.State m_State;

		private float m_EnterStateTime;

		public enum State
		{
			Calm,
			Upset,
			Aggressive,
			Scared
		}
	}
}
