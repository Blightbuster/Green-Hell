using System;
using UnityEngine;

namespace AIs
{
	public class ArachnidSoundModule : AIModule
	{
		public override void Initialize()
		{
			base.Initialize();
			this.m_AudioSource = base.gameObject.GetComponent<AudioSource>();
			this.m_AudioSource.clip = this.m_Clip;
			this.m_AudioSource.loop = true;
			this.m_AudioSource.spatialize = true;
			this.m_AudioSource.Play();
		}

		public override void OnDie()
		{
			base.OnDie();
			if (this.m_AudioSource)
			{
				this.m_AudioSource.Stop();
			}
		}

		public AudioClip m_Clip;

		private AudioSource m_AudioSource;
	}
}
