using System;
using System.Collections.Generic;
using UnityEngine;

public class ChatterManager : MonoBehaviour
{
	public static ChatterManager Get()
	{
		return ChatterManager.s_Instance;
	}

	private void Awake()
	{
		ChatterManager.s_Instance = this;
	}

	private void Start()
	{
		AudioClip[] array = Resources.LoadAll<AudioClip>("Sounds/TempSounds/Chatters/");
		for (int i = 0; i < array.Length; i++)
		{
			this.AddClip(array[i]);
		}
		array = Resources.LoadAll<AudioClip>("Sounds/Chatters/");
		for (int j = 0; j < array.Length; j++)
		{
			this.AddClip(array[j]);
		}
		this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
		this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Chatter);
	}

	private void AddClip(AudioClip clip)
	{
		ChatterData chatterData = new ChatterData();
		chatterData.m_Clip = clip;
		DebugUtils.Assert(chatterData.m_Clip, true);
		chatterData.m_Name = chatterData.m_Clip.name;
		chatterData.m_TextID = "Chatter_" + chatterData.m_Clip.name;
		this.m_Chatters.Add(chatterData);
	}

	public int GetChattersCount()
	{
		return this.m_Chatters.Count;
	}

	public ChatterData GetChatter(int index)
	{
		if (index < 0 || index >= this.GetChattersCount())
		{
			return null;
		}
		return this.m_Chatters[index];
	}

	public ChatterData FindChatterByName(string name)
	{
		for (int i = 0; i < this.m_Chatters.Count; i++)
		{
			if (this.m_Chatters[i].m_Name == name)
			{
				return this.m_Chatters[i];
			}
		}
		return null;
	}

	public bool IsPlaying(string chatter_name)
	{
		return this.m_AudioSource.isPlaying && this.m_AudioSource.clip.name == chatter_name;
	}

	public void PlayRandom(string chatter_name, float pan_stereo = 0f)
	{
		string[] array = chatter_name.Split(new char[]
		{
			';'
		});
		string chatter_name2 = array[UnityEngine.Random.Range(0, array.Length)];
		this.Play(chatter_name2, pan_stereo);
	}

	public void Play(string chatter_name, float pan_stereo = 0f)
	{
		ChatterData chatterData = this.FindChatterByName(chatter_name);
		if (chatterData == null || this.m_CurrentChatter == chatterData)
		{
			return;
		}
		chatterData.m_PanStereo = pan_stereo;
		this.m_Queue.Add(chatterData);
	}

	private void Update()
	{
		this.UpdateCurrentChatter();
		this.UpdateQueue();
		if (Input.GetKeyDown(KeyCode.Space) && this.m_AudioSource.isPlaying)
		{
			this.m_AudioSource.Stop();
		}
	}

	private void UpdateCurrentChatter()
	{
		if (this.m_CurrentChatter != null && !this.m_AudioSource.isPlaying)
		{
			this.m_CurrentChatter = null;
		}
	}

	private void UpdateQueue()
	{
		if (this.m_Queue.Count == 0)
		{
			return;
		}
		if (this.m_CurrentChatter != null)
		{
			return;
		}
		ChatterData chatterData = this.m_Queue[0];
		this.m_Queue.RemoveAt(0);
		if (!chatterData.m_Clip)
		{
			return;
		}
		this.m_AudioSource.panStereo = chatterData.m_PanStereo;
		this.m_AudioSource.clip = chatterData.m_Clip;
		this.m_AudioSource.Play();
		this.m_CurrentChatter = chatterData;
	}

	public bool IsAnyChatterPlaying()
	{
		return this.m_AudioSource.isPlaying;
	}

	private List<ChatterData> m_Chatters = new List<ChatterData>();

	public ChatterData m_CurrentChatter;

	private List<ChatterData> m_Queue = new List<ChatterData>();

	private AudioSource m_AudioSource;

	private static ChatterManager s_Instance;
}
