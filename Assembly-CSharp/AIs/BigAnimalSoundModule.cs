using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class BigAnimalSoundModule : AISoundModule
	{
		public override void Initialize(Being being)
		{
			base.Initialize(being);
			this.m_IdleAudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_IdleAudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.AI);
			this.m_IdleAudioSource.spatialBlend = 1f;
			this.m_IdleAudioSource.rolloffMode = AudioRolloffMode.Linear;
			this.m_IdleAudioSource.minDistance = 2f;
			this.m_IdleAudioSource.maxDistance = 25f;
			this.m_IdleAudioSource.spatialize = true;
			this.m_IdleAudioSource.priority = 50;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			AIGoal activeGoal = this.m_AI.m_GoalsModule.m_ActiveGoal;
			bool flag = false;
			if (!MainLevel.Instance.IsPause() && activeGoal != null && activeGoal.m_Type == AIGoalType.MoveToEnemy && this.m_AI.m_HostileStateModule && this.m_AI.m_HostileStateModule.m_State == HostileStateModule.State.Upset)
			{
				flag = true;
			}
			if (flag)
			{
				if (!this.m_IdleAudioSource.isPlaying)
				{
					AudioClip clip = AISoundModule.s_IdleClips[(int)this.m_AI.m_ID][UnityEngine.Random.Range(0, AISoundModule.s_IdleClips[(int)this.m_AI.m_ID].Count)];
					this.m_IdleAudioSource.PlayOneShot(clip);
					return;
				}
			}
			else if (this.m_IdleAudioSource.isPlaying)
			{
				this.m_IdleAudioSource.Stop();
			}
		}

		public override void OnDie()
		{
			base.OnDie();
			this.m_IdleAudioSource.Stop();
		}

		private List<AudioClip> m_Clips = new List<AudioClip>();

		private AudioSource m_IdleAudioSource;
	}
}
