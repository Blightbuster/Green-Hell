using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class BlackCaimanSoundModule : AISoundModule
	{
		public override void Initialize(Being being)
		{
			base.Initialize(being);
			this.m_AudioSourceSteps = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSourceSteps.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.AI);
			this.m_AudioSourceSteps.spatialBlend = 1f;
			this.m_AudioSourceSteps.rolloffMode = AudioRolloffMode.Linear;
			this.m_AudioSourceSteps.maxDistance = 22f;
			this.m_AudioSourceSteps.spatialize = true;
			this.m_AudioSourceSteps.priority = 2;
			this.m_AudioSourceAttack = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSourceAttack.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.AI);
			this.m_AudioSourceAttack.spatialBlend = 1f;
			this.m_AudioSourceAttack.rolloffMode = AudioRolloffMode.Linear;
			this.m_AudioSourceAttack.maxDistance = 22f;
			this.m_AudioSourceAttack.spatialize = true;
			this.m_AudioSourceAttack.priority = 2;
			this.m_AudioSourceGrowl = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSourceGrowl.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.AI);
			this.m_AudioSourceGrowl.spatialBlend = 1f;
			this.m_AudioSourceGrowl.rolloffMode = AudioRolloffMode.Linear;
			this.m_AudioSourceGrowl.maxDistance = 22f;
			this.m_AudioSourceGrowl.spatialize = true;
			this.m_AudioSourceGrowl.priority = 2;
			this.m_FootstepClips.Add(Resources.Load<AudioClip>("Sounds/AI/BlackCaiman/alligator_footstep_01"));
			this.m_FootstepClips.Add(Resources.Load<AudioClip>("Sounds/AI/BlackCaiman/alligator_footstep_02"));
			this.m_FootstepClips.Add(Resources.Load<AudioClip>("Sounds/AI/BlackCaiman/alligator_footstep_03"));
			this.m_FootstepClips.Add(Resources.Load<AudioClip>("Sounds/AI/BlackCaiman/alligator_footstep_04"));
			this.m_FootstepClips.Add(Resources.Load<AudioClip>("Sounds/AI/BlackCaiman/alligator_footstep_05"));
			this.m_FootstepClips.Add(Resources.Load<AudioClip>("Sounds/AI/BlackCaiman/alligator_footstep_06"));
			this.m_AttackClips.Add(Resources.Load<AudioClip>("Sounds/AI/BlackCaiman/alligator_attack_01"));
			this.m_AttackClips.Add(Resources.Load<AudioClip>("Sounds/AI/BlackCaiman/alligator_attack_02"));
			this.m_AttackClips.Add(Resources.Load<AudioClip>("Sounds/AI/BlackCaiman/alligator_attack_03"));
			this.m_AttackClips.Add(Resources.Load<AudioClip>("Sounds/AI/BlackCaiman/alligator_attack_04"));
			this.m_GrowlClips.Add(Resources.Load<AudioClip>("Sounds/AI/BlackCaiman/alligator_growl_01"));
			this.m_GrowlClips.Add(Resources.Load<AudioClip>("Sounds/AI/BlackCaiman/alligator_growl_02"));
			this.m_GrowlClips.Add(Resources.Load<AudioClip>("Sounds/AI/BlackCaiman/alligator_growl_03"));
			this.m_GrowlClips.Add(Resources.Load<AudioClip>("Sounds/AI/BlackCaiman/alligator_growl_04"));
			this.m_SwimmingClips.Add(Resources.Load<AudioClip>("Sounds/AI/BlackCaiman/alligator_swimming_water_splash"));
		}

		public override void OnDie()
		{
			base.OnDie();
			if (this.m_AudioSourceSteps)
			{
				this.m_AudioSourceSteps.Stop();
			}
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (!this.m_AudioSourceSteps.isPlaying && this.m_AI.m_Animator.velocity.magnitude >= 0.5f)
			{
				if (this.m_AI.IsSwimming())
				{
					this.m_AudioSourceSteps.clip = this.m_SwimmingClips[UnityEngine.Random.Range(0, this.m_SwimmingClips.Count)];
				}
				else if (Time.time - this.m_LastFootstepSoundTime > 1.2f)
				{
					this.m_LastFootstepSoundTime = Time.time;
					this.m_AudioSourceSteps.clip = this.m_FootstepClips[UnityEngine.Random.Range(0, this.m_FootstepClips.Count)];
				}
				this.m_AudioSourceSteps.Play();
			}
		}

		public override void PlayRandomGrowlSound()
		{
			AudioClip audioClip = this.m_GrowlClips[UnityEngine.Random.Range(0, this.m_GrowlClips.Count)];
			DebugUtils.Assert(audioClip, true);
			this.m_AudioSourceGrowl.clip = audioClip;
			this.m_AudioSourceGrowl.Play();
		}

		private void PlayRandomFootstepSound()
		{
			AudioClip audioClip = this.m_FootstepClips[UnityEngine.Random.Range(0, this.m_FootstepClips.Count)];
			DebugUtils.Assert(audioClip, true);
			this.m_AudioSourceSteps.clip = audioClip;
			this.m_AudioSourceSteps.Play();
		}

		public override void PlayAttackSound()
		{
			AudioClip audioClip = this.m_AttackClips[UnityEngine.Random.Range(0, this.m_AttackClips.Count)];
			DebugUtils.Assert(audioClip, true);
			this.m_AudioSourceAttack.clip = audioClip;
			this.m_AudioSourceAttack.Play();
		}

		private List<AudioClip> m_FootstepClips = new List<AudioClip>();

		private List<AudioClip> m_SwimmingClips = new List<AudioClip>();

		private List<AudioClip> m_AttackClips = new List<AudioClip>();

		private List<AudioClip> m_GrowlClips = new List<AudioClip>();

		private AudioSource m_AudioSourceSteps;

		private AudioSource m_AudioSourceAttack;

		private AudioSource m_AudioSourceGrowl;

		private float m_LastFootstepSoundTime = float.MinValue;
	}
}
