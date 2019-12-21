using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class KidRunnerSoundModule : AIModule
	{
		public override void Initialize(Being being)
		{
			base.Initialize(being);
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.AI);
			this.m_AudioSource.spatialBlend = 1f;
			this.m_AudioSource.rolloffMode = AudioRolloffMode.Linear;
			this.m_AudioSource.minDistance = 2f;
			this.m_AudioSource.maxDistance = 30f;
			this.m_AudioSource.spatialize = true;
			this.m_IdleClips = new List<AudioClip>(Resources.LoadAll<AudioClip>("Sounds/AI/KidRunner/KidRunner_Idle"));
			this.m_RunClips = new List<AudioClip>(Resources.LoadAll<AudioClip>("Sounds/AI/KidRunner/KidRunner_Laugh"));
			this.m_KidRunner = (KidRunnerAI)being;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (this.m_KidRunner.m_KidState == KidRunnerAI.KidState.Finish)
			{
				return;
			}
			if (this.m_AudioSource.isPlaying)
			{
				this.m_LastSoundTime = Time.time;
				return;
			}
			if (Time.time - this.m_LastSoundTime >= this.m_SoundInterval)
			{
				this.PlaySound();
			}
		}

		private void PlaySound()
		{
			AudioClip audioClip = null;
			while (audioClip == null || audioClip == this.m_LastClip)
			{
				KidRunnerAI.KidState kidState = this.m_KidRunner.m_KidState;
				if (kidState != KidRunnerAI.KidState.Run)
				{
					if (kidState == KidRunnerAI.KidState.Play)
					{
						audioClip = this.m_IdleClips[UnityEngine.Random.Range(0, this.m_IdleClips.Count)];
					}
				}
				else
				{
					audioClip = this.m_RunClips[UnityEngine.Random.Range(0, this.m_RunClips.Count)];
				}
			}
			this.m_AudioSource.PlayOneShot(audioClip);
			this.m_SoundInterval = UnityEngine.Random.Range(1f, 2f);
			this.m_LastClip = audioClip;
		}

		private List<AudioClip> m_IdleClips;

		private List<AudioClip> m_RunClips;

		private AudioSource m_AudioSource;

		private float m_LastSoundTime;

		private float m_SoundInterval;

		private KidRunnerAI m_KidRunner;

		private AudioClip m_LastClip;
	}
}
