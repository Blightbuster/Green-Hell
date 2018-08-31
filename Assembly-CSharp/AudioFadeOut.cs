using System;
using System.Collections;
using UnityEngine;

public static class AudioFadeOut
{
	public static IEnumerator FadeOut(AudioSource audio_source, float fade_time)
	{
		if (audio_source != null)
		{
			float start_volume = audio_source.volume;
			while (audio_source.volume > 0f)
			{
				audio_source.volume -= start_volume * Time.deltaTime / fade_time;
				yield return null;
			}
			audio_source.Stop();
			audio_source.volume = start_volume;
		}
		yield break;
	}

	public static IEnumerator FadeIn(AudioSource audio_source, float fade_time, float target_volume)
	{
		while (audio_source.volume < target_volume)
		{
			audio_source.volume += target_volume * Time.deltaTime / fade_time;
			yield return null;
		}
		audio_source.volume = target_volume;
		yield break;
	}
}
