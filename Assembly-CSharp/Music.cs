using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Music : MonoBehaviour, ISaveLoad
{
	public static Music Get()
	{
		return Music.s_Instance;
	}

	private void Awake()
	{
		Music.s_Instance = this;
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("MusicsMap.txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			this.m_MusicsMap[key.GetVariable(0).SValue] = key.GetVariable(1).SValue;
		}
	}

	private void InitSources()
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
		}
		this.m_Initialized = true;
	}

	public void Play(AudioClip clip, float volume = 1f, bool looped = false, int track = 0)
	{
		if (!this.m_Source[track])
		{
			this.InitSources();
		}
		this.m_Source[track].clip = clip;
		this.m_Source[track].volume = volume;
		this.m_Source[track].loop = looped;
		this.m_Source[track].Play();
	}

	public string GetPath(string name)
	{
		if (!this.m_MusicsMap.ContainsKey(name.ToLower()))
		{
			return string.Empty;
		}
		string text = this.m_MusicsMap[name.ToLower()];
		return text.Remove(text.LastIndexOf('.')).Remove(0, 17);
	}

	public void PlayByName(string name, bool looped = false, float volume = 1f, int track = 0)
	{
		string path = this.GetPath(name);
		AudioClip audioClip = Resources.Load(path) as AudioClip;
		if (!audioClip)
		{
			DebugUtils.Assert("[Music:PlayByName] Can't find music " + path, true, DebugUtils.AssertType.Info);
			return;
		}
		if (!this.m_Source[track])
		{
			this.InitSources();
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

	public bool IsMusicPlaying(int track = 0)
	{
		return this.m_Source[track].isPlaying && this.m_Source[track].volume > 0f;
	}

	public bool IsMusicPlayingAndIsNoPause(int track = 0)
	{
		return this.IsMusicPlaying(track) || MainLevel.Instance.IsPause();
	}

	public void Save()
	{
		for (int i = 0; i < 2; i++)
		{
			if (this.m_Source[i])
			{
				SaveGame.SaveVal("MusicPlaying" + i.ToString(), this.m_Source[i].isPlaying);
				if (this.m_Source[i].isPlaying)
				{
					SaveGame.SaveVal("Music" + i.ToString(), this.m_Source[i].clip.name);
				}
			}
			else
			{
				SaveGame.SaveVal("MusicPlaying" + i.ToString(), false);
			}
		}
	}

	public void Load()
	{
		for (int i = 0; i < 2; i++)
		{
			if (SaveGame.LoadBVal("MusicPlaying" + i.ToString()))
			{
				this.PlayByName(SaveGame.LoadSVal("Music" + i.ToString()), false, 1f, i);
			}
		}
	}

	public void FadeOut(float target_volume, float time, int track)
	{
		AudioSource audio_source = this.m_Source[track];
		base.StartCoroutine(AudioFadeOut.FadeOut(audio_source, time, target_volume, null));
	}

	public void FadeIn(float target_volume, float time, int track)
	{
		AudioSource audio_source = this.m_Source[track];
		base.StartCoroutine(AudioFadeOut.FadeIn(audio_source, time, target_volume, null));
	}

	public void Schedule(string clip_name, int track = 0, bool loop = false)
	{
		if (this.m_Scheduled.ContainsKey(track))
		{
			if (this.m_Scheduled[track] == null)
			{
				this.m_Scheduled[track] = new MusicScheduleData();
			}
			this.m_Scheduled[track].m_ClipName = clip_name;
		}
		else
		{
			MusicScheduleData musicScheduleData = new MusicScheduleData();
			musicScheduleData.m_ClipName = clip_name;
			this.m_Scheduled.Add(track, musicScheduleData);
		}
		if (this.m_Source[track].clip != null)
		{
			this.m_Scheduled[track].m_PlayTime = Time.time + (this.m_Source[track].clip.length - this.m_Source[track].time);
		}
		else
		{
			this.m_Scheduled[track].m_PlayTime = Time.time;
		}
		this.m_Scheduled[track].m_Loop = loop;
	}

	private void Update()
	{
		int key = -1;
		for (int i = 0; i < 2; i++)
		{
			foreach (KeyValuePair<int, MusicScheduleData> keyValuePair in this.m_Scheduled)
			{
				int key2 = keyValuePair.Key;
				Dictionary<int, MusicScheduleData>.Enumerator enumerator;
				keyValuePair = enumerator.Current;
				string clipName = keyValuePair.Value.m_ClipName;
				keyValuePair = enumerator.Current;
				bool loop = keyValuePair.Value.m_Loop;
				if (clipName.Length > 0)
				{
					float time = Time.time;
					keyValuePair = enumerator.Current;
					if (time >= keyValuePair.Value.m_PlayTime)
					{
						this.PlayByName(clipName, loop, 1f, key2);
						key = key2;
						break;
					}
				}
			}
			this.m_Scheduled.Remove(key);
		}
	}

	public void StopAll()
	{
		this.m_Scheduled.Clear();
		for (int i = 0; i < this.m_Source.Count<AudioSource>(); i++)
		{
			AudioSource audioSource = this.m_Source[i];
			if (audioSource != null)
			{
				audioSource.Stop();
			}
		}
	}

	public void StopAllOnTrack(int track, float time)
	{
		if (this.m_Scheduled.ContainsKey(track))
		{
			this.m_Scheduled.Remove(track);
		}
		this.FadeOut(0f, time, track);
	}

	private const int m_NumMusicTracks = 2;

	[HideInInspector]
	public AudioSource[] m_Source = new AudioSource[2];

	private static Music s_Instance;

	private bool m_Initialized;

	private Dictionary<string, string> m_MusicsMap = new Dictionary<string, string>();

	private Dictionary<int, MusicScheduleData> m_Scheduled = new Dictionary<int, MusicScheduleData>();
}
