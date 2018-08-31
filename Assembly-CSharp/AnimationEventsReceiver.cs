using System;
using System.Collections.Generic;
using AIs;
using CJTools;
using Enums;
using UnityEngine;

public class AnimationEventsReceiver : MonoBehaviour
{
	public void Initialize(Being being)
	{
		this.m_AnimEventsReceivers = base.GetComponents<IAnimationEventsReceiver>();
		this.m_SoundEventsReceivers = base.GetComponents<ISoundEventsReceiver>();
		this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
		this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.AI);
		this.m_AudioSource.spatialBlend = 1f;
		this.m_AudioSource.rolloffMode = AudioRolloffMode.Linear;
		this.m_AudioSource.minDistance = 2f;
		this.m_AudioSource.maxDistance = 30f;
		this.m_AudioSource.spatialize = true;
		this.m_FootstepAudioSource = base.gameObject.AddComponent<AudioSource>();
		this.m_FootstepAudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.AI);
		this.m_FootstepAudioSource.spatialBlend = 1f;
		this.m_FootstepAudioSource.rolloffMode = AudioRolloffMode.Linear;
		this.m_FootstepAudioSource.maxDistance = 12f;
		this.m_Being = being;
		this.SetupEvents();
	}

	private void SetupEvents()
	{
		AI ai = null;
		TextAssetParser textAssetParser;
		if (this.m_Being.IsAI())
		{
			ai = base.GetComponent<AI>();
			string text = (!ai.IsHuman()) ? (ai.m_ID.ToString() + "AnimEvents") : "SavageAnimEvents";
			if (!AIManager.Get().m_AnimEventsParsers.ContainsKey(text))
			{
				Debug.Log(text);
			}
			textAssetParser = AIManager.Get().m_AnimEventsParsers[text];
			if (ai.IsHuman())
			{
				this.m_HumanAI = (HumanAI)ai;
			}
			this.m_FootstepAudioSource.spatialize = true;
			this.m_AudioSource.spatialize = true;
		}
		else
		{
			textAssetParser = new TextAssetParser(this.m_AnimEventsScript);
		}
		Animator component = base.gameObject.GetComponent<Animator>();
		AnimationClip animationClip = null;
		AnimationClip[] animationClips = component.runtimeAnimatorController.animationClips;
		for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
		{
			Key key = textAssetParser.GetKey(i);
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
					for (int k = 0; k < key.GetKeysCount(); k++)
					{
						Key key2 = key.GetKey(k);
						if (key2.GetName() == "Event")
						{
							AnimEventID animEventID = (AnimEventID)Enum.Parse(typeof(AnimEventID), key2.GetVariable(0).SValue);
							bool flag = false;
							for (int l = 0; l < events.Length; l++)
							{
								if (events[l].intParameter == (int)animEventID)
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								float fvalue = key2.GetVariable(1).FValue;
								AnimationEvent animationEvent = new AnimationEvent();
								animationEvent.intParameter = (int)animEventID;
								animationEvent.time = animationClip.length * fvalue;
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
								string text2 = (!ai || ai.m_SoundPreset == AI.SoundPreset.None) ? string.Empty : (ai.m_SoundPreset.ToString() + "/");
								string path = string.Concat(new string[]
								{
									"Sounds/",
									key2.GetVariable(1).SValue,
									"/",
									text2,
									svalue2
								});
								AudioClip audioClip = Resources.Load<AudioClip>(path);
								if (audioClip)
								{
									this.m_Sounds[svalue2].Add(audioClip);
								}
								else
								{
									for (int m = 1; m < 99; m++)
									{
										path = string.Concat(new string[]
										{
											"Sounds/",
											key2.GetVariable(1).SValue,
											"/",
											text2,
											svalue2,
											(m >= 10) ? string.Empty : "0",
											m.ToString()
										});
										audioClip = Resources.Load<AudioClip>(path);
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
							float num = animationClip.length * key2.GetVariable(2).FValue;
							bool flag2 = false;
							for (int n = 0; n < events.Length; n++)
							{
								if (!(events[n].functionName != "SoundEvent"))
								{
									if (events[n].stringParameter == svalue2 && events[n].time == num)
									{
										flag2 = true;
										break;
									}
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
							float num2 = animationClip.length * key2.GetVariable(0).FValue;
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
								if (!(events[num3].functionName != "FootstepEvent"))
								{
									if (events[num3].stringParameter == svalue3 && events[num3].time == num2)
									{
										flag3 = true;
										break;
									}
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

	public void AnimEvent(AnimEventID event_id)
	{
		for (int i = 0; i < this.m_AnimEventsReceivers.Length; i++)
		{
			if (this.m_AnimEventsReceivers[i].IsActive() || this.m_AnimEventsReceivers[i].ForceReceiveAnimEvent())
			{
				this.m_AnimEventsReceivers[i].OnAnimEvent(event_id);
			}
		}
		if (this.m_Log)
		{
			CJDebug.Log("AnimEvent " + event_id.ToString());
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

	private void Update()
	{
		if (!this.m_AudioSource)
		{
			return;
		}
		if (this.m_AudioSource.isPlaying)
		{
			this.m_NoSoundDuration = 0f;
		}
		else
		{
			this.m_NoSoundDuration += Time.deltaTime;
		}
	}

	public void FootstepEvent(string obj_name)
	{
		EObjectMaterial mat = (!this.m_Being.IsInWater()) ? this.m_Being.GetMaterial() : EObjectMaterial.Water;
		if (this.m_FootstepAudioSource && !this.m_FootstepAudioSource.isPlaying)
		{
			this.m_FootstepAudioSource.PlayOneShot(AIManager.Get().GetFootstepSound(mat));
		}
	}

	private IAnimationEventsReceiver[] m_AnimEventsReceivers;

	private ISoundEventsReceiver[] m_SoundEventsReceivers;

	public TextAsset m_AnimEventsScript;

	public Dictionary<int, List<AnimationEvent>> m_Events = new Dictionary<int, List<AnimationEvent>>();

	private Dictionary<string, List<AudioClip>> m_Sounds = new Dictionary<string, List<AudioClip>>();

	private Dictionary<string, Transform> m_TransformsForFXesMap = new Dictionary<string, Transform>();

	private Being m_Being;

	private HumanAI m_HumanAI;

	public bool m_Log;

	private AudioSource m_AudioSource;

	private AudioSource m_FootstepAudioSource;

	private float m_NoSoundDuration;
}
