using System;
using UnityEngine;

public class MusicJingle : MonoBehaviour
{
	private void Awake()
	{
		MusicJingle.s_Instance = this;
	}

	public static MusicJingle Get()
	{
		return MusicJingle.s_Instance;
	}

	private void InitSource()
	{
		if (this.m_Initialized)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			this.m_Source[i] = base.gameObject.AddComponent<AudioSource>();
			this.m_Source[i].outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Music);
			this.m_Source[i].spatialBlend = 0f;
			this.m_Initialized = true;
		}
	}

	public void Play(AudioClip clip, float volume = 1f, int track = 0)
	{
		if (!this.m_Source[track])
		{
			this.InitSource();
		}
		this.m_Source[track].clip = clip;
		this.m_Source[track].volume = volume;
		this.m_Source[track].Play();
	}

	public void PlayByName(string name, bool looped = false, float volume = 1f, int track = 0)
	{
		AudioClip audioClip = Resources.Load(Music.Get().GetPath(name)) as AudioClip;
		if (!audioClip)
		{
			DebugUtils.Assert("[Music:PlayByName] Can't find music " + name, true, DebugUtils.AssertType.Info);
			return;
		}
		if (!this.m_Source[track])
		{
			this.InitSource();
		}
		this.m_Source[track].clip = audioClip;
		this.m_Source[track].loop = looped;
		this.m_Source[track].volume = volume;
		this.m_Source[track].Play();
	}

	public void Stop(float fadeout = 0f, int track = 0)
	{
		AudioSource audio_source = this.m_Source[track];
		base.StartCoroutine(AudioFadeOut.FadeOut(audio_source, fadeout, 0f, null));
	}

	public void FadeOut(float target_volume, float time, int track = 0)
	{
		AudioSource audio_source = this.m_Source[track];
		base.StartCoroutine(AudioFadeOut.FadeOut(audio_source, time, target_volume, null));
	}

	public void FadeIn(float target_volume, float time, int track = 0)
	{
		AudioSource audio_source = this.m_Source[track];
		base.StartCoroutine(AudioFadeOut.FadeIn(audio_source, time, target_volume, null));
	}

	public void StoppAll()
	{
		for (int i = 0; i < this.m_Source.Length; i++)
		{
			if (this.m_Source[i])
			{
				this.m_Source[i].Stop();
			}
		}
	}

	private const int m_NumJingles = 2;

	[HideInInspector]
	public AudioSource[] m_Source = new AudioSource[2];

	private static MusicJingle s_Instance;

	private bool m_Initialized;
}
