using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class BigAnimalSoundModule : AISoundModule
	{
		public override void Initialize()
		{
			base.Initialize();
			this.m_AudioSourceSteps = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSourceSteps.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.AI);
			this.m_AudioSourceSteps.spatialBlend = 1f;
			this.m_AudioSourceSteps.rolloffMode = AudioRolloffMode.Linear;
			this.m_AudioSourceSteps.maxDistance = 12f;
			this.m_AudioSourceSteps.spatialize = true;
			this.m_Clips.Add(Resources.Load<AudioClip>("Sounds/TempSounds/Player/Footstep01"));
			this.m_Clips.Add(Resources.Load<AudioClip>("Sounds/TempSounds/Player/Footstep02"));
			this.m_Clips.Add(Resources.Load<AudioClip>("Sounds/TempSounds/Player/Footstep03"));
			this.m_Clips.Add(Resources.Load<AudioClip>("Sounds/TempSounds/Player/Footstep04"));
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (!this.m_AudioSourceSteps.isPlaying && this.m_AI.m_Animator.velocity.magnitude >= 0.5f)
			{
				this.m_AudioSourceSteps.clip = this.m_Clips[UnityEngine.Random.Range(0, this.m_Clips.Count)];
				this.m_AudioSourceSteps.Play();
			}
		}

		public override void OnDie()
		{
			base.OnDie();
			if (this.m_AudioSourceSteps)
			{
				this.m_AudioSourceSteps.Stop();
			}
		}

		private List<AudioClip> m_Clips = new List<AudioClip>();

		private AudioSource m_AudioSourceSteps;
	}
}
