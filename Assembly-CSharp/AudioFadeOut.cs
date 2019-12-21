using System;
using System.Collections;
using UnityEngine;

public static class AudioFadeOut
{
	public static IEnumerator FadeOut(AudioSource audio_source, float fade_time, float target_volume = 0f, Action callback = null)
	{
		if (audio_source != null)
		{
			float start_volume = audio_source.volume;
			while (audio_source.volume > target_volume)
			{
				audio_source.volume -= start_volume * Time.deltaTime / fade_time;
				yield return null;
			}
			audio_source.Stop();
			audio_source.volume = start_volume;
			if (callback != null)
			{
				callback();
			}
		}
		yield break;
	}

	public static IEnumerator FadeIn(AudioSource audio_source, float fade_time, float target_volume, Action callback = null)
	{
		if (!audio_source.isPlaying)
		{
			audio_source.Play();
		}
		while (audio_source.volume < target_volume)
		{
			audio_source.volume += target_volume * Time.deltaTime / fade_time;
			yield return null;
		}
		audio_source.volume = target_volume;
		if (callback != null)
		{
			callback();
		}
		yield break;
	}
}
