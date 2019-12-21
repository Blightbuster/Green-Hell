using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class Piranha : Fish
	{
		protected override void Start()
		{
			base.Start();
			this.SetupCheckSwimToPlayerInterval();
		}

		protected override void CheckNoise()
		{
		}

		protected override float GetMinSpeed()
		{
			if (this.m_FishState == Fish.State.SwimToPlayer)
			{
				return 0.5f;
			}
			return base.GetMinSpeed();
		}

		protected override float GetRotationSpeed()
		{
			if (this.m_FishState == Fish.State.SwimToPlayer)
			{
				return base.GetRotationSpeed() * 1.5f;
			}
			return base.GetRotationSpeed();
		}

		protected override void UpdateState()
		{
			Fish.State fishState = this.m_FishState;
			if (fishState != Fish.State.SwimToPlayer)
			{
				if (fishState == Fish.State.BitePlayer)
				{
					this.UpdateBitePlayer();
				}
			}
			else
			{
				this.UpdateSwimToPlayer();
			}
			base.UpdateState();
		}

		private void SetupCheckSwimToPlayerInterval()
		{
			this.m_CheckSwimToPlayerInterval = UnityEngine.Random.Range(this.m_SwimToPlayerCheckMinInterval, this.m_SwimToPlayerCheckMaxInterval);
		}

		protected override void UpdateSwimState()
		{
			base.UpdateSwimState();
			if (Player.Get().IsOnBoat())
			{
				return;
			}
			if (Time.time - this.m_CheckSwimToPlayerInterval < this.m_LastCheckSwimToPlayerTime)
			{
				return;
			}
			this.m_LastCheckSwimToPlayerTime = Time.time;
			this.SetupCheckSwimToPlayerInterval();
			if (Time.time - this.m_LastBiteTime < this.m_BiteInterval)
			{
				return;
			}
			if (UnityEngine.Random.Range(0f, 1f) > this.m_SwimToPlayerChance)
			{
				return;
			}
			if (!this.m_Tank.IsPointInside(Player.Get().transform.position))
			{
				return;
			}
			base.SetState(Fish.State.SwimToPlayer);
		}

		private void UpdateSwimToPlayer()
		{
			if (!this.m_Tank.IsPointInside(Player.Get().transform.position))
			{
				base.SetState(Fish.State.Swim);
				return;
			}
			if (Vector3.Distance(this.m_Mouth ? this.m_Mouth.transform.position : base.transform.position, Player.Get().transform.position) < this.m_BiteDist)
			{
				base.SetState(Fish.State.BitePlayer);
			}
		}

		private void UpdateBitePlayer()
		{
			Vector3 hit_dir = Player.Get().transform.position + Vector3.up * Player.Get().GetComponent<CharacterControllerProxy>().height * 0.5f - base.transform.position;
			Player.Get().GiveDamage(base.gameObject, null, this.m_Damage, hit_dir, DamageType.None, 0, false);
			this.m_LastBiteTime = Time.time;
			base.SetState(Fish.State.Swim);
		}

		private float m_BiteInterval = 3f;

		private float m_LastBiteTime;

		private float m_SwimToPlayerCheckMinInterval = 1f;

		private float m_SwimToPlayerCheckMaxInterval = 3f;

		private float m_CheckSwimToPlayerInterval;

		private float m_LastCheckSwimToPlayerTime;

		private float m_SwimToPlayerChance = 0.2f;

		private float m_BiteDist = 0.5f;

		public float m_Damage = 10f;
	}
}
