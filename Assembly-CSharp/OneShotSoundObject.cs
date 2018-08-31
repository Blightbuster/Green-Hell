using System;
using UnityEngine;

public class OneShotSoundObject : MonoBehaviour
{
	private void Start()
	{
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
		}
		AudioClip audioClip = (AudioClip)Resources.Load(this.m_SoundNameWithPath);
		if (audioClip != null)
		{
			this.m_AudioSource.PlayOneShot(audioClip);
		}
	}

	private void Update()
	{
		if (!this.m_AudioSource.isPlaying)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public string m_SoundNameWithPath = string.Empty;

	private AudioSource m_AudioSource;
}
