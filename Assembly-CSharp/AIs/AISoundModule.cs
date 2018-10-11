using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class AISoundModule : AIModule
	{
		public override void Initialize()
		{
			base.Initialize();
			bool flag = false;
			this.s_Status.TryGetValue((int)this.m_AI.m_ID, out flag);
			if (!flag)
			{
				this.ParseSoundFile();
			}
			if (this.m_AudioSource == null)
			{
				this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
				this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.AI);
				this.m_AudioSource.spatialBlend = 1f;
				this.m_AudioSource.rolloffMode = AudioRolloffMode.Linear;
				this.m_AudioSource.maxDistance = 12f;
			}
		}

		private void ParseSoundFile()
		{
			ScriptParser scriptParser = new ScriptParser();
			scriptParser.Parse("AI/" + this.m_AI.m_ID.ToString() + "Sounds", true);
			this.s_IdleClips[(int)this.m_AI.m_ID] = new List<AudioClip>();
			this.s_PanicClips[(int)this.m_AI.m_ID] = new List<AudioClip>();
			this.s_DeathClips[(int)this.m_AI.m_ID] = new List<AudioClip>();
			for (int i = 0; i < scriptParser.GetKeysCount(); i++)
			{
				Key key = scriptParser.GetKey(i);
				if (key.GetName() == "Idle")
				{
					for (int j = 0; j < key.GetVariablesCount(); j++)
					{
						CJVariable variable = key.GetVariable(j);
						AudioClip item = Resources.Load("Sounds/AI/" + this.m_AI.m_ID.ToString() + "/" + variable.SValue) as AudioClip;
						this.s_IdleClips[(int)this.m_AI.m_ID].Add(item);
					}
				}
				else if (key.GetName() == "Panic")
				{
					for (int k = 0; k < key.GetVariablesCount(); k++)
					{
						CJVariable variable2 = key.GetVariable(k);
						AudioClip item2 = Resources.Load("Sounds/AI/" + this.m_AI.m_ID.ToString() + "/" + variable2.SValue) as AudioClip;
						this.s_PanicClips[(int)this.m_AI.m_ID].Add(item2);
					}
				}
				else if (key.GetName() == "Death")
				{
					for (int l = 0; l < key.GetVariablesCount(); l++)
					{
						CJVariable variable3 = key.GetVariable(l);
						AudioClip item3 = Resources.Load("Sounds/AI/" + this.m_AI.m_ID.ToString() + "/" + variable3.SValue) as AudioClip;
						this.s_DeathClips[(int)this.m_AI.m_ID].Add(item3);
					}
				}
			}
			this.s_Status[(int)this.m_AI.m_ID] = true;
		}

		public void RequestSound(AISoundType sound_type)
		{
			this.m_RequestedSound = sound_type;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (this.m_RequestedSound == AISoundType.Idle)
			{
				if (Time.time > this.m_IdleSoundNextTime)
				{
					this.m_AudioSource.Stop();
					if (this.s_IdleClips[(int)this.m_AI.m_ID].Count > 0)
					{
						this.m_AudioSource.PlayOneShot(this.s_IdleClips[(int)this.m_AI.m_ID][UnityEngine.Random.Range(0, this.s_IdleClips[(int)this.m_AI.m_ID].Count)]);
					}
					this.m_IdleSoundNextTime = Time.time + UnityEngine.Random.Range(this.m_IdleSoundDelayMin, this.m_IdleSoundDelayMax);
				}
			}
			else if (this.m_RequestedSound == AISoundType.Panic && Time.time > this.m_PanicSoundNextTime)
			{
				this.m_AudioSource.Stop();
				if (this.s_PanicClips[(int)this.m_AI.m_ID].Count > 0)
				{
					this.m_AudioSource.PlayOneShot(this.s_PanicClips[(int)this.m_AI.m_ID][UnityEngine.Random.Range(0, this.s_PanicClips[(int)this.m_AI.m_ID].Count)]);
				}
				this.m_PanicSoundNextTime = Time.time + UnityEngine.Random.Range(this.m_PanicSoundDelayMin, this.m_PanicSoundDelayMax);
			}
		}

		public void OnGoalActivate(AIGoalType goal_type)
		{
			if (goal_type != AIGoalType.Idle)
			{
				if (goal_type == AIGoalType.MoveAwayFromEnemy)
				{
					this.RequestSound(AISoundType.Panic);
				}
			}
			else
			{
				this.RequestSound(AISoundType.Idle);
			}
		}

		public override void OnDie()
		{
			base.OnDie();
			if (Time.time > this.m_DeathSoundNextTime)
			{
				this.m_AudioSource.Stop();
				if (this.s_DeathClips[(int)this.m_AI.m_ID].Count > 0)
				{
					this.m_AudioSource.PlayOneShot(this.s_DeathClips[(int)this.m_AI.m_ID][UnityEngine.Random.Range(0, this.s_DeathClips[(int)this.m_AI.m_ID].Count)]);
				}
				this.m_DeathSoundNextTime = Time.time + UnityEngine.Random.Range(this.m_DeathSoundDelayMin, this.m_DeathSoundDelayMax);
			}
		}

		private Dictionary<int, List<AudioClip>> s_IdleClips = new Dictionary<int, List<AudioClip>>();

		private Dictionary<int, List<AudioClip>> s_PanicClips = new Dictionary<int, List<AudioClip>>();

		private Dictionary<int, List<AudioClip>> s_DeathClips = new Dictionary<int, List<AudioClip>>();

		private Dictionary<int, bool> s_Status = new Dictionary<int, bool>();

		private AudioSource m_AudioSource;

		private AISoundType m_RequestedSound;

		private float m_IdleSoundDelayMin = 20f;

		private float m_IdleSoundDelayMax = 30f;

		private float m_IdleSoundNextTime = float.MinValue;

		private float m_PanicSoundDelayMin = 10f;

		private float m_PanicSoundDelayMax = 20f;

		private float m_PanicSoundNextTime = float.MinValue;

		private float m_DeathSoundDelayMin = 10f;

		private float m_DeathSoundDelayMax = 20f;

		private float m_DeathSoundNextTime = float.MinValue;
	}
}
