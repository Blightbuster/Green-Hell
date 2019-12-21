using System;
using UnityEngine;

namespace AIs
{
	public class Stalker : AI
	{
		public static Stalker Get()
		{
			return Stalker.s_Instance;
		}

		protected override void Awake()
		{
			base.Awake();
			Stalker.s_Instance = this;
		}

		public override bool CheckActivityByDistance()
		{
			return false;
		}

		protected override void Update()
		{
			base.Update();
			this.UpdatePlayerApproaching();
			this.UpdateState();
		}

		private void UpdatePlayerApproaching()
		{
			float num = Vector3.Dot((base.transform.position - Player.Get().transform.position).normalized, Player.Get().transform.forward);
			this.m_PlayerApproaching = (Player.Get().IsWalking() || (Player.Get().IsRunning() && num > 0f));
			this.m_PrevPlayerPos = Player.Get().transform.position;
		}

		private void UpdateState()
		{
			switch (this.m_State)
			{
			case Stalker.State.MoveAround:
				if (StalkerManager.Get().CanAttack())
				{
					this.m_State = Stalker.State.Attack;
					StalkerManager.Get().OnStalkerStartAttack();
					return;
				}
				if (Vector3.Distance(Player.Get().transform.position, base.transform.position) < StalkerManager.Get().m_MoveAroundMinRange)
				{
					this.m_State = Stalker.State.RunAway;
				}
				PlayerSanityModule.Get().OnWhispersEvent(PlayerSanityModule.WhisperType.Stalker);
				return;
			case Stalker.State.RunAway:
				if (Vector3.Distance(Player.Get().transform.position, base.transform.position) > StalkerManager.Get().m_MoveAwayRange && !this.m_PlayerApproaching)
				{
					this.m_State = Stalker.State.MoveAround;
				}
				break;
			case Stalker.State.Attack:
				break;
			default:
				return;
			}
		}

		public override bool TakeDamage(DamageInfo info)
		{
			base.TakeDamage(info);
			ParticlesManager.Get().Spawn("Blood_Effect_Big", base.transform.position, Quaternion.identity, Vector3.zero, null, -1f, false);
			UnityEngine.Object.Destroy(base.gameObject, 0.2f);
			return true;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			StalkerManager.Get().OnStalkerDestroy();
		}

		public Stalker.State m_State;

		public bool m_PlayerApproaching;

		private Vector3 m_PrevPlayerPos = Vector3.zero;

		private static Stalker s_Instance;

		public enum State
		{
			MoveAround,
			RunAway,
			Attack
		}
	}
}
