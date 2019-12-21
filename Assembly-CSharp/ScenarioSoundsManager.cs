using System;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioSoundsManager : MonoBehaviour
{
	public void PlaySound(GameObject obj, string sound_name)
	{
		if (!obj)
		{
			DebugUtils.Assert("Can't play sound - " + sound_name + ". Object is not set!", true, DebugUtils.AssertType.Info);
			return;
		}
		AudioClip clip = Resources.Load<AudioClip>(sound_name);
		AudioSource audioSource = obj.AddComponent<AudioSource>();
		audioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
		audioSource.maxDistance = 15f;
		audioSource.spatialBlend = 1f;
		audioSource.clip = clip;
		audioSource.Play();
		this.m_AudioSources.Add(audioSource, Time.time);
		if (!base.enabled)
		{
			base.enabled = true;
		}
	}

	private void Update()
	{
		this.UpdateAudioSources();
	}

	private void UpdateAudioSources()
	{
		if (this.m_AudioSources.Count == 0)
		{
			base.enabled = false;
			return;
		}
		foreach (AudioSource audioSource in this.m_AudioSources.Keys)
		{
			if (Time.time - this.m_AudioSources[audioSource] >= audioSource.clip.length && !audioSource.isPlaying)
			{
				this.m_AudioSources.Remove(audioSource);
				UnityEngine.Object.Destroy(audioSource);
				break;
			}
		}
	}

	private Dictionary<AudioSource, float> m_AudioSources = new Dictionary<AudioSource, float>();
}
