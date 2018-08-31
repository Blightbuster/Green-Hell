using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class SnakeSoundModule : AIModule
	{
		public override void Initialize()
		{
			base.Initialize();
			AudioSource[] components = base.GetComponents<AudioSource>();
			if (components.Length > 0)
			{
				this.m_LifeAS = components[0];
				this.m_LifeAS.clip = this.m_Life;
				this.m_LifeAS.loop = true;
				this.m_LifeAS.spatialize = true;
				this.m_LifeAS.Play();
			}
			if (components.Length > 1)
			{
				this.m_AttackAS = components[1];
				this.m_AttackAS.clip = this.m_Attack;
				this.m_AttackAS.volume = 0.2f;
				this.m_AttackAS.loop = false;
				this.m_AttackAS.spatialize = true;
			}
		}

		public override void OnAnimEvent(AnimEventID id)
		{
			base.OnAnimEvent(id);
			if (this.m_AI.m_HealthModule && this.m_AI.m_HealthModule.m_IsDead)
			{
				return;
			}
			if (id == AnimEventID.RattlesnakeAttack)
			{
				this.PlayAttackSound();
			}
		}

		public void PlayAttackSound()
		{
			this.m_AttackAS.Play();
		}

		public override void OnDie()
		{
			base.OnDie();
			if (this.m_LifeAS)
			{
				this.m_LifeAS.Stop();
			}
			if (this.m_AttackAS)
			{
				this.m_AttackAS.Stop();
			}
		}

		public AudioClip m_Life;

		public AudioClip m_Attack;

		private AudioSource m_LifeAS;

		private AudioSource m_AttackAS;
	}
}
