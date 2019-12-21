using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class HumanAISoundModule : AIModule
	{
		public override void Initialize(Being being)
		{
			base.Initialize(being);
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.AI);
			this.m_AudioSource.spatialBlend = 1f;
			this.m_AudioSource.rolloffMode = AudioRolloffMode.Linear;
			this.m_AudioSource.minDistance = 2f;
			this.m_AudioSource.maxDistance = 25f;
			this.m_AudioSource.spatialize = true;
			this.m_AudioSource.priority = 50;
			this.m_AudioSource.playOnAwake = false;
			switch (this.m_AI.m_SoundPreset)
			{
			case AI.SoundPreset.Tribe0:
				this.m_Sounds = AIManager.Get().m_Tribe0Sounds;
				break;
			case AI.SoundPreset.Tribe1:
				this.m_Sounds = AIManager.Get().m_Tribe1Sounds;
				break;
			case AI.SoundPreset.Tribe2:
				this.m_Sounds = AIManager.Get().m_Tribe2Sounds;
				break;
			}
			this.m_HumanAI = (HumanAI)this.m_AI;
		}

		public AudioClip PlaySound(HumanAISoundModule.SoundType type)
		{
			if (!this.m_AI || !this.m_AI.m_StartExecuted)
			{
				return null;
			}
			AudioClip clip = this.m_Sounds[(int)type][UnityEngine.Random.Range(0, this.m_Sounds[(int)type].Count)];
			return this.PlaySound(clip);
		}

		public AudioClip PlaySound(AudioClip clip)
		{
			this.m_AudioSource.Stop();
			this.m_AudioSource.clip = clip;
			this.m_AudioSource.Play();
			return clip;
		}

		public void StopSound(AudioClip clip)
		{
			if (this.m_AudioSource == null)
			{
				return;
			}
			if (this.m_AudioSource.clip == clip)
			{
				this.m_AudioSource.Stop();
			}
		}

		public override void OnDie()
		{
			base.OnDie();
			this.PlaySound(HumanAISoundModule.SoundType.Death);
		}

		private Dictionary<int, List<AudioClip>> m_Sounds = new Dictionary<int, List<AudioClip>>();

		private AudioSource m_AudioSource;

		private HumanAI m_HumanAI;

		public enum SoundType
		{
			Death,
			Sing,
			Speak,
			Scream
		}
	}
}
