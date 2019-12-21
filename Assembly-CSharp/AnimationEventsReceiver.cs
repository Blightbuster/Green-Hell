using System;
using System.Collections.Generic;
using AIs;
using CJTools;
using Enums;
using UnityEngine;

public class AnimationEventsReceiver : MonoBehaviour
{
	public void Initialize(Being being = null)
	{
		this.m_AnimEventsReceivers = base.GetComponents<IAnimationEventsReceiver>();
		this.m_AnimEventsReceiversEx = base.GetComponents<IAnimationEventsReceiverEx>();
		this.m_SoundEventsReceivers = base.GetComponents<ISoundEventsReceiver>();
		this.m_Animator = base.GetComponent<Animator>();
		this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
		this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.AI);
		this.m_AudioSource.spatialBlend = 1f;
		this.m_AudioSource.rolloffMode = AudioRolloffMode.Linear;
		this.m_AudioSource.minDistance = 2f;
		this.m_AudioSource.maxDistance = 30f;
		this.m_AudioSource.spatialize = true;
		this.m_Being = being;
		this.SetupEvents();
	}

	public static void PreParseAnimationEventScripts()
	{
		foreach (string text in AnimationEventsReceiver.s_ScriptsToPreparse)
		{
			TextAssetParser textAssetParser;
			if (!AnimationEventsReceiver.s_ParsedAnimEventScriptsCache.TryGetValue(text, out textAssetParser))
			{
				textAssetParser = new TextAssetParser();
				textAssetParser.Parse(text, true);
				AnimationEventsReceiver.s_ParsedAnimEventScriptsCache.Add(text, textAssetParser);
			}
		}
	}

	private TextAssetParser GetParser()
	{
		TextAssetParser textAssetParser = null;
		if (this.m_HumanAI)
		{
			string text = null;
			AI.AIID id = this.m_HumanAI.m_ID;
			switch (id)
			{
			case AI.AIID.Hunter:
				text = "AI/SavageAnimEvents";
				break;
			case AI.AIID.Spearman:
				text = "AI/SpearmanAnimEvents";
				break;
			case AI.AIID.Thug:
				text = "AI/ThugAnimEvents";
				break;
			case AI.AIID.Savage:
				text = "AI/SavageAnimEvents";
				break;
			default:
				if (id == AI.AIID.KidRunner)
				{
					text = "AI/KidRunnerAnimEvents";
				}
				break;
			}
			if (text != null && !AnimationEventsReceiver.s_ParsedAnimEventScriptsCache.TryGetValue(text, out textAssetParser))
			{
				textAssetParser = new TextAssetParser();
				textAssetParser.Parse(text, true);
				AnimationEventsReceiver.s_ParsedAnimEventScriptsCache.Add(text, textAssetParser);
			}
		}
		else if (this.m_AnimEventsScript != null && !AnimationEventsReceiver.s_ParsedAnimEventScriptsCache.TryGetValue(this.m_AnimEventsScript.name, out textAssetParser))
		{
			textAssetParser = new TextAssetParser(this.m_AnimEventsScript);
			AnimationEventsReceiver.s_ParsedAnimEventScriptsCache.Add(this.m_AnimEventsScript.name, textAssetParser);
		}
		if (textAssetParser == null)
		{
			Debug.Log("Could not parse animation event script for: " + base.name);
			textAssetParser = new TextAssetParser();
		}
		return textAssetParser;
	}

	private void SetupEvents()
	{
		AI ai = null;
		if (this.m_Being && this.m_Being.IsAI())
		{
			ai = base.GetComponent<AI>();
			if (ai.IsHuman())
			{
				this.m_HumanAI = (HumanAI)ai;
			}
			this.m_AudioSource.spatialize = true;
		}
		TextAssetParser parser = this.GetParser();
		AnimationClip animationClip = null;
		AnimationClip[] animationClips = this.m_Animator.runtimeAnimatorController.animationClips;
		for (int i = 0; i < parser.GetKeysCount(); i++)
		{
			Key key = parser.GetKey(i);
			if (key.GetName() == "Anim")
			{
				string svalue = key.GetVariable(0).SValue;
				foreach (AnimationClip animationClip2 in animationClips)
				{
					if (animationClip2.name == svalue)
					{
						animationClip = animationClip2;
						break;
					}
				}
				if (!animationClip)
				{
					DebugUtils.Assert(false, "Can't find player anim clip " + svalue, true, DebugUtils.AssertType.Info);
				}
				else
				{
					AnimationEvent[] events = animationClip.events;
					float length = animationClip.length;
					for (int k = 0; k < key.GetKeysCount(); k++)
					{
						Key key2 = key.GetKey(k);
						if (key2.GetName() == "Event")
						{
							AnimEventID value = EnumUtils<AnimEventID>.GetValue(key2.GetVariable(0).SValue);
							bool flag = false;
							for (int l = 0; l < events.Length; l++)
							{
								if (events[l].intParameter == (int)value)
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								float fvalue = key2.GetVariable(1).FValue;
								AnimationEvent animationEvent = new AnimationEvent();
								animationEvent.intParameter = (int)value;
								animationEvent.time = length * fvalue;
								animationEvent.functionName = "AnimEvent";
								animationClip.AddEvent(animationEvent);
								List<AnimationEvent> list = null;
								if (!this.m_Events.TryGetValue(Animator.StringToHash(svalue), out list))
								{
									this.m_Events[Animator.StringToHash(svalue)] = new List<AnimationEvent>();
								}
								this.m_Events[Animator.StringToHash(svalue)].Add(animationEvent);
							}
						}
						else if (key2.GetName() == "Sound")
						{
							string svalue2 = key2.GetVariable(0).SValue;
							if (!this.m_Sounds.ContainsKey(svalue2))
							{
								this.m_Sounds[svalue2] = new List<AudioClip>();
								string text = (ai && ai.m_SoundPreset != AI.SoundPreset.None) ? (ai.m_SoundPreset.ToString() + "/") : "";
								AudioClip audioClip = Resources.Load<AudioClip>(string.Concat(new string[]
								{
									"Sounds/",
									key2.GetVariable(1).SValue,
									"/",
									text,
									svalue2
								}));
								if (audioClip)
								{
									this.m_Sounds[svalue2].Add(audioClip);
								}
								else
								{
									for (int m = 1; m < 99; m++)
									{
										audioClip = Resources.Load<AudioClip>(string.Concat(new string[]
										{
											"Sounds/",
											key2.GetVariable(1).SValue,
											"/",
											text,
											svalue2,
											(m < 10) ? "0" : "",
											m.ToString()
										}));
										if (!audioClip)
										{
											break;
										}
										this.m_Sounds[svalue2].Add(audioClip);
									}
								}
							}
							if (this.m_Sounds[svalue2].Count == 0)
							{
								DebugUtils.Assert("Missing clips of sound - " + svalue2, true, DebugUtils.AssertType.Info);
							}
							float num = length * key2.GetVariable(2).FValue;
							bool flag2 = false;
							for (int n = 0; n < events.Length; n++)
							{
								if (!(events[n].functionName != "SoundEvent") && events[n].stringParameter == svalue2 && events[n].time == num)
								{
									flag2 = true;
									break;
								}
							}
							if (!flag2)
							{
								animationClip.AddEvent(new AnimationEvent
								{
									stringParameter = svalue2,
									time = num,
									functionName = "SoundEvent"
								});
							}
						}
						else if (key2.GetName() == "Footstep")
						{
							float num2 = length * key2.GetVariable(0).FValue;
							string svalue3 = key2.GetVariable(1).SValue;
							if (!this.m_TransformsForFXesMap.ContainsKey(svalue3))
							{
								Transform transform = base.transform.FindDeepChild(svalue3);
								DebugUtils.Assert(transform != null, "Can't find objects - " + svalue3, true, DebugUtils.AssertType.Info);
								this.m_TransformsForFXesMap.Add(svalue3, transform);
							}
							bool flag3 = false;
							for (int num3 = 0; num3 < events.Length; num3++)
							{
								if (!(events[num3].functionName != "FootstepEvent") && events[num3].stringParameter == svalue3 && events[num3].time == num2)
								{
									flag3 = true;
									break;
								}
							}
							if (!flag3)
							{
								animationClip.AddEvent(new AnimationEvent
								{
									stringParameter = svalue3,
									time = num2,
									functionName = "FootstepEvent"
								});
							}
						}
					}
				}
			}
		}
	}

	private void LateUpdate()
	{
		this.m_ThisFrameIntAnimEvents.Clear();
	}

	public void AnimEvent(AnimationEvent anim_event)
	{
		if (!this.m_ThisFrameIntAnimEvents.CheckAndStore(anim_event.intParameter))
		{
			for (int i = 0; i < this.m_AnimEventsReceivers.Length; i++)
			{
				if (this.m_AnimEventsReceivers[i].IsActive() || this.m_AnimEventsReceivers[i].ForceReceiveAnimEvent())
				{
					this.m_AnimEventsReceivers[i].OnAnimEvent((AnimEventID)anim_event.intParameter);
				}
			}
		}
		for (int j = 0; j < this.m_AnimEventsReceiversEx.Length; j++)
		{
			if (this.m_AnimEventsReceiversEx[j].IsActive() || this.m_AnimEventsReceiversEx[j].ForceReceiveAnimEvent())
			{
				this.m_AnimEventsReceiversEx[j].OnAnimEventEx(anim_event);
			}
		}
		if (this.m_Log)
		{
			CJDebug.Log("AnimEvent " + anim_event.intParameter.ToString());
		}
	}

	public void SoundEvent(string sound_name)
	{
		if (!this.m_AudioSource)
		{
			return;
		}
		if (!this.m_Sounds.ContainsKey(sound_name))
		{
			DebugUtils.Assert("Can't find sound - " + sound_name, true, DebugUtils.AssertType.Info);
			return;
		}
		List<AudioClip> list = this.m_Sounds[sound_name];
		if (list.Count == 0)
		{
			DebugUtils.Assert("Missing clips of sound - " + sound_name, true, DebugUtils.AssertType.Info);
			return;
		}
		AudioClip audioClip = list[UnityEngine.Random.Range(0, list.Count)];
		if (this.m_HumanAI)
		{
			this.m_HumanAI.m_HumanAISoundModule.PlaySound(audioClip);
			return;
		}
		this.m_AudioSource.Stop();
		this.m_AudioSource.clip = audioClip;
		this.m_AudioSource.Play();
		if (this.m_Log)
		{
			CJDebug.Log("SoundEvent " + audioClip.name);
		}
	}

	public void FootstepEvent(AnimationEvent anim_event)
	{
		if (anim_event.animatorClipInfo.weight >= 0.5f && this.m_LastFootstepEventTime < Time.time - AnimationEventsReceiver.STEP_INTERVAL)
		{
			this.m_LastFootstepEventTime = Time.time;
			EObjectMaterial mat = this.m_Being.IsInWater() ? EObjectMaterial.Water : this.m_Being.GetMaterial();
			foreach (AudioSource audioSource in this.m_FootstepAudioSources)
			{
				if (!audioSource.isPlaying)
				{
					this.PlayFootstep(audioSource, mat);
					return;
				}
			}
			if (this.m_FootstepAudioSources.Count < 4)
			{
				this.PlayFootstep(this.AddFootstepAudioSource(), mat);
			}
		}
	}

	private void PlayFootstep(AudioSource source, EObjectMaterial mat)
	{
		if (source.rolloffMode != AudioRolloffMode.Custom)
		{
			source.rolloffMode = AudioRolloffMode.Custom;
			source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, MainLevel.Instance.m_SoundRolloffCurve);
		}
		source.volume = (this.m_HumanAI ? 1f : 0.5f);
		source.PlayOneShot(AIManager.Get().GetFootstepSound(mat, this.m_Animator.velocity.magnitude < 4f));
	}

	private AudioSource AddFootstepAudioSource()
	{
		AudioSource audioSource = base.gameObject.AddComponent<AudioSource>();
		audioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.EnviroAmplified);
		audioSource.rolloffMode = AudioRolloffMode.Custom;
		audioSource.spatialBlend = 1f;
		audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, MainLevel.Instance.m_SoundRolloffCurve);
		audioSource.minDistance = 2f;
		audioSource.maxDistance = 30f;
		audioSource.spatialize = true;
		this.m_FootstepAudioSources.Add(audioSource);
		return audioSource;
	}

	private IAnimationEventsReceiver[] m_AnimEventsReceivers;

	private IAnimationEventsReceiverEx[] m_AnimEventsReceiversEx;

	private ISoundEventsReceiver[] m_SoundEventsReceivers;

	public TextAsset m_AnimEventsScript;

	public Dictionary<int, List<AnimationEvent>> m_Events = new Dictionary<int, List<AnimationEvent>>();

	private static Dictionary<string, TextAssetParser> s_ParsedAnimEventScriptsCache = new Dictionary<string, TextAssetParser>();

	private Dictionary<string, List<AudioClip>> m_Sounds = new Dictionary<string, List<AudioClip>>();

	private Dictionary<string, Transform> m_TransformsForFXesMap = new Dictionary<string, Transform>();

	private Being m_Being;

	private HumanAI m_HumanAI;

	public bool m_Log;

	private AudioSource m_AudioSource;

	private List<AudioSource> m_FootstepAudioSources = new List<AudioSource>();

	private float m_LastFootstepEventTime = float.MinValue;

	private Animator m_Animator;

	private static List<string> s_ScriptsToPreparse = new List<string>
	{
		"AI/SavageAnimEvents",
		"AI/SpearmanAnimEvents",
		"AI/ThugAnimEvents",
		"AI/PumaAnimEvents",
		"AI/JaguarAnimEvents",
		"AI/BlackCaimanAnimEvents",
		"AI/PeccaryAnimEvents",
		"AI/TapirAnimEvents",
		"AI/CapybaraAnimEvents"
	};

	private AnimationEventsReceiver.ThisFrameIntAnimEvents m_ThisFrameIntAnimEvents = new AnimationEventsReceiver.ThisFrameIntAnimEvents();

	private static float STEP_INTERVAL = 0.1f;

	private const int MAX_STEP_AUDIOSOURCES = 4;

	private class ThisFrameIntAnimEvents
	{
		public bool CheckAndStore(int value)
		{
			if (this.m_Items == null)
			{
				this.m_Items = new List<int>(3);
			}
			for (int i = 0; i < this.m_ItemCount; i++)
			{
				if (this.m_Items[i] == value)
				{
					return true;
				}
			}
			DebugUtils.Assert(this.m_ItemCount <= this.m_Items.Count, true);
			if (this.m_ItemCount == this.m_Items.Count)
			{
				this.m_Items.Add(value);
			}
			else
			{
				this.m_Items[this.m_ItemCount] = value;
			}
			this.m_ItemCount++;
			return false;
		}

		public void Clear()
		{
			this.m_ItemCount = 0;
		}

		private int m_ItemCount;

		private List<int> m_Items;
	}
}
