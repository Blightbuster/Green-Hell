using System;
using System.Collections.Generic;
using AIs;
using CJTools;
using UnityEngine;

public class PlayerSanityModule : PlayerModule
{
	public static PlayerSanityModule Get()
	{
		return PlayerSanityModule.s_Instance;
	}

	private void Awake()
	{
		PlayerSanityModule.s_Instance = this;
	}

	public override void Initialize()
	{
		base.Initialize();
		for (int i = 0; i < 24; i++)
		{
			this.m_EventsMap.Add(i, new SanityEventData());
		}
		this.LoadScript();
		this.m_DisappearAudioSource = new GameObject("DissappearSound")
		{
			transform = 
			{
				parent = base.transform,
				localRotation = Quaternion.identity,
				localPosition = Vector3.zero
			}
		}.AddComponent<AudioSource>();
		this.m_DisappearAudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		for (int j = 0; j < this.m_MaxWhispersQueue; j++)
		{
			AudioSource audioSource = base.gameObject.AddComponent<AudioSource>();
			audioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
			audioSource.panStereo = -1f;
			this.m_AudioSources.Add(audioSource);
			audioSource = base.gameObject.AddComponent<AudioSource>();
			audioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
			audioSource.panStereo = 1f;
			this.m_AudioSources.Add(audioSource);
			audioSource = base.gameObject.AddComponent<AudioSource>();
			audioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
			audioSource.panStereo = 1f;
			this.m_AudioSources.Add(audioSource);
		}
	}

	private void LoadScript()
	{
		TextAsset textAsset = Resources.Load(this.m_SanityScript) as TextAsset;
		if (!textAsset)
		{
			DebugUtils.Assert("Can't load Sanity script - " + this.m_SanityScript, true, DebugUtils.AssertType.Info);
			return;
		}
		TextAssetParser textAssetParser = new TextAssetParser(textAsset);
		for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
		{
			Key key = textAssetParser.GetKey(i);
			if (key.GetName() == "ItemHallucinationsSanityLevel")
			{
				this.m_ItemHallucinationsSanityLevel = key.GetVariable(0).IValue;
			}
			else if (key.GetName() == "ItemHallucinationMinInterval")
			{
				this.m_ItemHallucinationMinInterval = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "ItemHallucinationMaxInterval")
			{
				this.m_ItemHallucinationMaxInterval = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "ItemHallucinationsMinCount")
			{
				this.m_ItemHallucinationsMinCount = key.GetVariable(0).IValue;
			}
			else if (key.GetName() == "ItemHallucinationsMaxCount")
			{
				this.m_ItemHallucinationsMaxCount = key.GetVariable(0).IValue;
			}
			else if (key.GetName() == "AIHallucinationsSanityLevel")
			{
				this.m_AIHallucinationsSanityLevel = key.GetVariable(0).IValue;
			}
			else if (key.GetName() == "AIHallucinationMinInterval")
			{
				this.m_AIHallucinationMinInterval = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "AIHallucinationMaxInterval")
			{
				this.m_AIHallucinationMaxInterval = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "AIHallucinationsMinCount")
			{
				this.m_AIHallucinationsMinCount = key.GetVariable(0).IValue;
			}
			else if (key.GetName() == "AIHallucinationsMaxCount")
			{
				this.m_AIHallucinationsMaxCount = key.GetVariable(0).IValue;
			}
			else if (key.GetName() == "StartEffectSanityLevel")
			{
				this.m_StartEffectSanityLevel = key.GetVariable(0).IValue;
			}
			else if (key.GetName() == "DisappearChatterChance")
			{
				this.m_DisappearChatterChance = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "MinSpawnStalkerInterval")
			{
				this.m_MinSpawnStalkerInterval = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "MaxSpawnStalkerInterval")
			{
				this.m_MaxSpawnStalkerInterval = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "WhispersSanityLevel")
			{
				this.m_WhispersSanityLevel = key.GetVariable(0).IValue;
			}
			else if (key.GetName() == "MinWhispersInterval")
			{
				this.m_MinWhispersInterval = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "MaxWhispersInterval")
			{
				this.m_MaxWhispersInterval = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "MinRandomWhispersInterval")
			{
				this.m_MinRandomWhispersInterval = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "MaxRandomWhispersInterval")
			{
				this.m_MaxRandomWhispersInterval = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "MaxWhispersQueue")
			{
				this.m_MaxWhispersQueue = key.GetVariable(0).IValue;
			}
			else if (key.GetName() == "Event")
			{
				string svalue = key.GetVariable(0).SValue;
				if (Enum.IsDefined(typeof(PlayerSanityModule.SanityEventType), svalue))
				{
					PlayerSanityModule.SanityEventType key2 = (PlayerSanityModule.SanityEventType)Enum.Parse(typeof(PlayerSanityModule.SanityEventType), svalue);
					this.m_EventsMap[(int)key2].m_SanityChange = key.GetVariable(1).IValue;
					this.m_EventsMap[(int)key2].m_Interval = key.GetVariable(2).FValue;
					this.m_EventsMap[(int)key2].m_TextID = key.GetVariable(3).SValue;
				}
			}
			else if (key.GetName() == "StalkerStalkingSanityLevel")
			{
				this.m_StalkerStalkingSanityLevel = key.GetVariable(0).IValue;
			}
			else if (key.GetName() == "StalkerAttackSanityLevel")
			{
				this.m_StalkerAttackSanityLevel = key.GetVariable(0).IValue;
			}
			else if (key.GetName() == "Whisper")
			{
				PlayerSanityModule.WhisperType key3 = (PlayerSanityModule.WhisperType)Enum.Parse(typeof(PlayerSanityModule.WhisperType), key.GetVariable(0).SValue);
				for (int j = 0; j < key.GetKeysCount(); j++)
				{
					Key key4 = key.GetKey(j);
					if (key4.GetName() == "Clip")
					{
						AudioClip audioClip = Resources.Load<AudioClip>("Sounds/Chatters/Whispers/" + key4.GetVariable(0).SValue);
						if (!audioClip)
						{
							audioClip = Resources.Load<AudioClip>("Sounds/Chatters/Temp_Whispers/" + key4.GetVariable(0).SValue);
						}
						if (!audioClip)
						{
							DebugUtils.Assert("Can't find clip - " + key4.GetVariable(0).SValue, true, DebugUtils.AssertType.Info);
						}
						if (!this.m_WhispersMap.ContainsKey((int)key3))
						{
							List<AudioClip> value = new List<AudioClip>();
							this.m_WhispersMap.Add((int)key3, value);
						}
						this.m_WhispersMap[(int)key3].Add(audioClip);
					}
				}
			}
			else if (key.GetName() == "DisappearItemSounds")
			{
				for (int k = 0; k < key.GetKeysCount(); k++)
				{
					Key key5 = key.GetKey(k);
					if (key5.GetName() == "Clip")
					{
						AudioClip audioClip2 = Resources.Load<AudioClip>("Sounds/Chatters/Whispers/" + key5.GetVariable(0).SValue);
						if (!audioClip2)
						{
							audioClip2 = Resources.Load<AudioClip>("Sounds/Chatters/Temp_Whispers/" + key5.GetVariable(0).SValue);
						}
						if (!audioClip2)
						{
							DebugUtils.Assert("Can't find clip - " + key5.GetVariable(0).SValue, true, DebugUtils.AssertType.Info);
						}
						if (!this.m_DisappearItem.Contains(audioClip2))
						{
							this.m_DisappearItem.Add(audioClip2);
						}
					}
				}
			}
			else if (key.GetName() == "DisappearAISounds")
			{
				for (int l = 0; l < key.GetKeysCount(); l++)
				{
					Key key6 = key.GetKey(l);
					if (key6.GetName() == "Clip")
					{
						AudioClip audioClip3 = Resources.Load<AudioClip>("Sounds/Chatters/Whispers/" + key6.GetVariable(0).SValue);
						if (!audioClip3)
						{
							audioClip3 = Resources.Load<AudioClip>("Sounds/Chatters/Temp_Whispers/" + key6.GetVariable(0).SValue);
						}
						if (!audioClip3)
						{
							DebugUtils.Assert("Can't find clip - " + key6.GetVariable(0).SValue, true, DebugUtils.AssertType.Info);
						}
						if (!this.m_DisappearAI.Contains(audioClip3))
						{
							this.m_DisappearAI.Add(audioClip3);
						}
					}
				}
			}
			else if (key.GetName() == "LowEnegryWhispersLevel")
			{
				this.m_LowEnegryWhispersLevel = key.GetVariable(0).FValue;
			}
		}
		Resources.UnloadAsset(textAsset);
	}

	public float GetEventInterval(PlayerSanityModule.SanityEventType evn)
	{
		SanityEventData sanityEventData = this.m_EventsMap[(int)evn];
		return sanityEventData.m_Interval;
	}

	public void ResetEventCooldown(PlayerSanityModule.SanityEventType evn)
	{
		SanityEventData sanityEventData = this.m_EventsMap[(int)evn];
		sanityEventData.m_LastEventTime = Time.time;
	}

	public void OnEat(int sanity_change)
	{
		if (MainLevel.Instance.m_Tutorial)
		{
			this.m_Sanity = 100;
			return;
		}
		if (!base.enabled || sanity_change == 0)
		{
			return;
		}
		int num = Mathf.Clamp(this.m_Sanity + sanity_change, 0, 100);
		int num2 = num - this.m_Sanity;
		if (num2 == 0)
		{
			return;
		}
		this.m_Sanity = num;
		this.OnChangeSanity((float)num2, string.Empty);
	}

	public void OnEvent(PlayerSanityModule.SanityEventType evn, int mul = 1)
	{
		if (MainLevel.Instance.m_Tutorial)
		{
			this.m_Sanity = 100;
			return;
		}
		if (!base.enabled)
		{
			return;
		}
		SanityEventData sanityEventData = this.m_EventsMap[(int)evn];
		if (sanityEventData.m_LastEventTime == 0f || Time.time - sanityEventData.m_LastEventTime >= sanityEventData.m_Interval)
		{
			int num = Mathf.Clamp(this.m_Sanity + sanityEventData.m_SanityChange * mul, 0, 100);
			int num2 = num - this.m_Sanity;
			if (num2 == 0)
			{
				return;
			}
			this.m_Sanity = num;
			sanityEventData.m_LastEventTime = Time.time;
			this.OnChangeSanity((float)num2, sanityEventData.m_TextID);
		}
	}

	private void OnChangeSanity(float diff, string text_id)
	{
		HUDMessages hudmessages = (HUDMessages)HUDManager.Get().GetHUD(typeof(HUDMessages));
		string text = GreenHellGame.Instance.GetLocalization().Get("HUD_Sanity") + ((diff <= 0f) ? string.Empty : "+") + diff.ToString();
		hudmessages.AddMessage(text, null, HUDMessageIcon.None, string.Empty);
		if (text_id != string.Empty)
		{
			text = GreenHellGame.Instance.GetLocalization().Get(text_id);
			hudmessages.AddMessage(text, null, HUDMessageIcon.None, string.Empty);
		}
		if (this.m_Sanity <= this.m_SanityLevelToPlaySounds)
		{
			PlayerAudioModule.Get().PlaySanityLossSound(1f);
		}
		HUDSanity.Get().OnChangeSanity(diff);
		if ((float)this.m_Sanity < 10f)
		{
			if (GreenHellGame.Instance.GetCurrentSnapshot() != AudioMixerSnapshotGame.LowSanity && !SleepController.Get().IsActive())
			{
				GreenHellGame.Instance.SetSnapshot(AudioMixerSnapshotGame.LowSanity, 0.5f);
			}
		}
		else if (GreenHellGame.Instance.GetCurrentSnapshot() != AudioMixerSnapshotGame.Default && !SleepController.Get().IsActive())
		{
			GreenHellGame.Instance.SetSnapshot(AudioMixerSnapshotGame.Default, 0.5f);
		}
	}

	public override void Update()
	{
		base.Update();
		if (MainLevel.Instance.IsPause())
		{
			return;
		}
		this.UpdateEffects();
		this.UpdateHeartBeatSound();
		this.UpdateAIHallucinations();
		this.UpdateItemsHallucinations();
		this.UpdateRandomWhispers();
		this.UpdateDebug();
	}

	private void UpdateEffects()
	{
		float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, (float)this.m_Sanity, (float)this.m_StartEffectSanityLevel, 1f);
		PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Sanity, proportionalClamp);
	}

	private void UpdateHeartBeatSound()
	{
		float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, (float)this.m_Sanity, 1f, 0f);
		this.m_HeartBeatVolume += (proportionalClamp - this.m_HeartBeatVolume) * Time.deltaTime * 0.2f;
		if (this.m_HeartBeatVolume <= 0f)
		{
			return;
		}
		float proportionalClamp2 = CJTools.Math.GetProportionalClamp(1.5f, 0.2f, (float)this.m_Sanity, 1f, 0.1f);
		if (Time.time - this.m_LastHeartSoundTime >= proportionalClamp2 && !PlayerAudioModule.Get().IsHeartBeatSoundPlaying())
		{
			PlayerAudioModule.Get().PlayHeartBeatSound(this.m_HeartBeatVolume, false);
			this.m_LastHeartSoundTime = Time.time;
		}
	}

	private void UpdateDebug()
	{
		if (!GreenHellGame.DEBUG)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.PageUp))
		{
			this.OnEvent(PlayerSanityModule.SanityEventType.DebugPositive, 1);
		}
		if (Input.GetKeyDown(KeyCode.PageDown))
		{
			this.OnEvent(PlayerSanityModule.SanityEventType.DebugNegative, 1);
		}
	}

	private void UpdateAIHallucinations()
	{
		if (this.m_Sanity > this.m_AIHallucinationsSanityLevel)
		{
			return;
		}
		if (this.m_CurrentHallucination != null)
		{
			return;
		}
		float proportionalClamp = CJTools.Math.GetProportionalClamp(this.m_AIHallucinationMaxInterval, this.m_AIHallucinationMinInterval, (float)this.m_Sanity, (float)this.m_AIHallucinationsSanityLevel, 1f);
		if (this.m_LastAIHallucinationTime == 0f || Time.time - this.m_LastAIHallucinationTime >= proportionalClamp)
		{
			this.SpawnAIHallucination();
		}
	}

	private void SpawnAIHallucination()
	{
		int count = Mathf.FloorToInt(CJTools.Math.GetProportionalClamp((float)this.m_AIHallucinationsMinCount, (float)this.m_AIHallucinationsMaxCount, (float)this.m_Sanity, (float)this.m_AIHallucinationsSanityLevel, 1f));
		this.m_CurrentHallucination = AIWavesManager.Get().SpawnWave(count, true);
		this.m_LastAIHallucinationTime = Time.time;
	}

	public void OnDeactivateHallucination()
	{
		this.m_CurrentHallucination = null;
		this.m_LastAIHallucinationTime = Time.time;
	}

	private void UpdateItemsHallucinations()
	{
		if (this.m_Sanity > this.m_ItemHallucinationsSanityLevel)
		{
			this.DisableItemHallucinations();
			return;
		}
		if (!this.m_ItemHallucinationsEnabled)
		{
			float proportionalClamp = CJTools.Math.GetProportionalClamp(this.m_ItemHallucinationMaxInterval, this.m_ItemHallucinationMinInterval, (float)this.m_Sanity, (float)this.m_ItemHallucinationsSanityLevel, 1f);
			if (this.m_LastItemHallucinationTime == 0f || Time.time - this.m_LastItemHallucinationTime >= proportionalClamp)
			{
				this.EnbaleItemHallucinations();
			}
		}
	}

	private void EnbaleItemHallucinations()
	{
		if (this.m_ItemHallucinationsEnabled)
		{
			return;
		}
		this.m_ItemHallucinationsEnabled = true;
		this.m_ColleectedItemHallucinations = 0;
	}

	private void DisableItemHallucinations()
	{
		if (!this.m_ItemHallucinationsEnabled)
		{
			return;
		}
		this.m_ItemHallucinationsEnabled = false;
		foreach (Item item in Item.s_AllItems)
		{
			if (item.m_Hallucination)
			{
				item.Disappear(false);
			}
		}
		this.m_LastItemHallucinationTime = Time.time;
	}

	public int GetWantedItemsHallucinationsCount()
	{
		return Mathf.FloorToInt(CJTools.Math.GetProportionalClamp((float)this.m_ItemHallucinationsMinCount, (float)this.m_ItemHallucinationsMaxCount, (float)this.m_Sanity, (float)this.m_ItemHallucinationsSanityLevel, 1f));
	}

	public void OnObjectDisappear(CJObject obj, bool play_disappear_chatter)
	{
		if (!this.m_ItemHallucinationsEnabled && !obj.IsAI())
		{
			return;
		}
		if (!obj.IsAI())
		{
			this.m_ColleectedItemHallucinations++;
			if (this.m_ColleectedItemHallucinations > 5)
			{
				this.DisableItemHallucinations();
			}
		}
		if (play_disappear_chatter)
		{
			AudioClip clip;
			if (obj.IsAI())
			{
				clip = this.m_DisappearAI[UnityEngine.Random.Range(0, this.m_DisappearAI.Count)];
			}
			else
			{
				clip = this.m_DisappearItem[UnityEngine.Random.Range(0, this.m_DisappearItem.Count)];
			}
			this.m_DisappearAudioSource.clip = clip;
			this.m_DisappearAudioSource.Play();
		}
	}

	private void UpdateRandomWhispers()
	{
		if (this.m_LastWhisperTime != 0f)
		{
			float proportionalClamp = CJTools.Math.GetProportionalClamp(this.m_MaxRandomWhispersInterval, this.m_MinRandomWhispersInterval, (float)this.m_Sanity, (float)this.m_WhispersSanityLevel, 1f);
			if (Time.time - this.m_LastWhisperTime < proportionalClamp)
			{
				return;
			}
		}
		this.OnWhispersEvent(PlayerSanityModule.WhisperType.Random);
	}

	public void OnWhispersEvent(PlayerSanityModule.WhisperType type)
	{
		if (this.m_Sanity > this.m_WhispersSanityLevel)
		{
			return;
		}
		if (this.m_LastWhisperTime != 0f)
		{
			float proportionalClamp = CJTools.Math.GetProportionalClamp(this.m_MaxWhispersInterval, this.m_MinWhispersInterval, (float)this.m_Sanity, (float)this.m_WhispersSanityLevel, 1f);
			if (Time.time - this.m_LastWhisperTime < proportionalClamp)
			{
				return;
			}
		}
		this.m_WhispersQueue.Clear();
		List<AudioClip> list = this.m_WhispersMap[(int)type];
		int num = UnityEngine.Random.Range(1, this.m_MaxWhispersQueue + 1);
		for (int i = 0; i < num; i++)
		{
			AudioSource audioSource = this.m_AudioSources[UnityEngine.Random.Range(0, this.m_AudioSources.Count)];
			this.m_AudioSources.Remove(audioSource);
			audioSource.clip = list[UnityEngine.Random.Range(0, list.Count)];
			this.m_WhispersQueue.Add(audioSource);
		}
		foreach (AudioSource item in this.m_WhispersQueue)
		{
			this.m_AudioSources.Add(item);
		}
		this.PlayWhisper();
		this.m_LastWhisperTime = Time.time;
	}

	private void PlayWhisper()
	{
		if (this.m_WhispersQueue.Count == 0)
		{
			return;
		}
		AudioSource audioSource = this.m_WhispersQueue[0];
		audioSource.Play();
		this.m_WhispersQueue.RemoveAt(0);
		if (this.m_WhispersQueue.Count > 0)
		{
			base.Invoke("PlayWhisper", UnityEngine.Random.Range(0.5f, 1.2f));
		}
	}

	public const int MAX_SANITY = 100;

	[Range(0f, 100f)]
	public int m_Sanity = 100;

	private Dictionary<int, SanityEventData> m_EventsMap = new Dictionary<int, SanityEventData>();

	public string m_SanityScript = "Scripts/Player/PlayerSanity";

	private float m_LastHeartSoundTime;

	private float m_HeartBeatVolume;

	[HideInInspector]
	public int m_StalkerStalkingSanityLevel;

	[HideInInspector]
	public int m_StalkerAttackSanityLevel;

	[HideInInspector]
	public float m_MinSpawnStalkerInterval;

	[HideInInspector]
	public float m_MaxSpawnStalkerInterval;

	private int m_ItemHallucinationsSanityLevel = 50;

	private float m_ItemHallucinationMinInterval = 60f;

	private float m_ItemHallucinationMaxInterval = 300f;

	private int m_ItemHallucinationsMinCount = 4;

	private int m_ItemHallucinationsMaxCount = 10;

	[HideInInspector]
	public bool m_ItemHallucinationsEnabled;

	private float m_LastItemHallucinationTime;

	private int m_ColleectedItemHallucinations;

	private int m_AIHallucinationsSanityLevel = 50;

	private float m_AIHallucinationMinInterval = 60f;

	private float m_AIHallucinationMaxInterval = 300f;

	private int m_AIHallucinationsMinCount = 1;

	private int m_AIHallucinationsMaxCount = 6;

	private HumanAIWave m_CurrentHallucination;

	private float m_LastAIHallucinationTime;

	private int m_StartEffectSanityLevel = 50;

	private float m_DisappearChatterChance = 0.5f;

	private int m_MaxWhispersQueue;

	private int m_WhispersSanityLevel;

	private float m_LastWhisperTime;

	private float m_MinWhispersInterval;

	private float m_MaxWhispersInterval;

	private float m_MinRandomWhispersInterval;

	private float m_MaxRandomWhispersInterval;

	private AudioSource m_DisappearAudioSource;

	private List<AudioClip> m_DisappearAI = new List<AudioClip>();

	private List<AudioClip> m_DisappearItem = new List<AudioClip>();

	private List<AudioSource> m_AudioSources = new List<AudioSource>();

	private List<AudioSource> m_WhispersQueue = new List<AudioSource>();

	private Dictionary<int, List<AudioClip>> m_WhispersMap = new Dictionary<int, List<AudioClip>>();

	[HideInInspector]
	public float m_LowEnegryWhispersLevel;

	public int m_SanityLevelToPlaySounds = 20;

	private static PlayerSanityModule s_Instance;

	public enum SanityEventType
	{
		None,
		GroundSleep,
		BedSleep,
		StoryPositive,
		StoryNegative,
		Firecamp,
		WormMinigameSuccess,
		WormMinigameFail,
		Disease,
		Rain,
		SmallWoundAbrassion,
		SmallWoundScratch,
		Laceration,
		LacerationCat,
		Rash,
		Worm,
		WormHole,
		Leech,
		LeechHole,
		VenomBite,
		SnakeBite,
		PlannedAction,
		DebugPositive,
		DebugNegative,
		Count
	}

	public enum WhisperType
	{
		Random,
		StartFire,
		Aim,
		TakeItemGood,
		TakeItemBad,
		Stalker,
		LowEnergy,
		GhostSlot,
		Inspection,
		AISight,
		AIDamage
	}
}
