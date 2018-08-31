using System;
using UnityEngine;

public class Music : MonoBehaviour, ISaveLoad
{
	private void Awake()
	{
		Music.s_Instance = this;
	}

	public static Music Get()
	{
		return Music.s_Instance;
	}

	private void InitSource()
	{
		if (this.m_Initialized)
		{
			return;
		}
		this.m_Source = base.gameObject.AddComponent<AudioSource>();
		this.m_Source.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Music);
		this.m_Source.spatialBlend = 0f;
		this.m_Initialized = true;
	}

	public void Play(AudioClip clip)
	{
		if (!this.m_Source)
		{
			this.InitSource();
		}
		this.m_Source.clip = clip;
		this.m_Source.volume = this.m_Volume;
		this.m_Source.Play();
	}

	public void PlayByName(string name, bool looped = false)
	{
		string text = "Music/Jingle/" + name;
		AudioClip audioClip = Resources.Load(text) as AudioClip;
		if (!audioClip)
		{
			DebugUtils.Assert("[Music:PlayByName] Can't find music " + text, true, DebugUtils.AssertType.Info);
			return;
		}
		if (!this.m_Source)
		{
			this.InitSource();
		}
		this.m_Source.clip = audioClip;
		this.m_Source.loop = looped;
		this.m_Source.volume = this.m_Volume;
		this.m_Source.Play();
	}

	public void Stop(float fadeout = 0f)
	{
		base.StartCoroutine(AudioFadeOut.FadeOut(this.m_Source, fadeout));
	}

	public void Save()
	{
		if (this.m_Source)
		{
			SaveGame.SaveVal("MusicPlaying", this.m_Source.isPlaying);
			if (this.m_Source.isPlaying)
			{
				SaveGame.SaveVal("Music", this.m_Source.clip.name);
			}
		}
		else
		{
			SaveGame.SaveVal("MusicPlaying", false);
		}
	}

	public void Load()
	{
		bool flag = SaveGame.LoadBVal("MusicPlaying");
		if (flag)
		{
			this.PlayByName(SaveGame.LoadSVal("Music"), false);
		}
	}

	public void SetVolume(float volume)
	{
		if (!this.m_Source)
		{
			this.InitSource();
		}
		this.m_Volume = volume;
		this.m_Source.volume = volume;
	}

	[HideInInspector]
	public AudioSource m_Source;

	private static Music s_Instance;

	private bool m_Initialized;

	private float m_Volume = 1f;
}
