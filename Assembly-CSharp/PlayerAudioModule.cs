using System;
using System.Collections.Generic;
using System.Reflection;
using Enums;
using UnityEngine;

public class PlayerAudioModule : PlayerModule
{
	public static PlayerAudioModule Get()
	{
		return PlayerAudioModule.s_Instance;
	}

	private void Awake()
	{
		PlayerAudioModule.s_Instance = this;
	}

	public override void Initialize()
	{
		base.Initialize();
		if (this.m_AudioSources == null)
		{
			this.m_AudioSources = new AudioSource[this.m_AudioSourcesCount];
			for (int i = 0; i < this.m_AudioSourcesCount; i++)
			{
				this.m_AudioSources[i] = this.m_Player.gameObject.AddComponent<AudioSource>();
				this.m_AudioSources[i].outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
			}
		}
		this.m_SleepSoundSource = this.m_Player.gameObject.AddComponent<AudioSource>();
		this.m_SleepSoundSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Sleep);
		this.m_SleepSoundSource.playOnAwake = false;
		this.m_PlayingLowStaminaSoundSource = this.m_Player.gameObject.AddComponent<AudioSource>();
		this.m_PlayingLowStaminaSoundSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		this.m_PlayingLowStaminaSoundSource.playOnAwake = false;
		this.m_NoiseManager = MainLevel.Instance.GetComponent<NoiseManager>();
		DebugUtils.Assert(this.m_NoiseManager, "[PlayerAudioModule::Initialize] Can't find NoiseManager", true, DebugUtils.AssertType.Info);
		this.LoadScript();
		this.m_FPPController = Player.Get().GetComponent<FPPController>();
	}

	public void LoadScript()
	{
		string text = "Scripts/Player/Player_Sounds";
		TextAsset textAsset = Resources.Load(text) as TextAsset;
		if (DebugUtils.Assert(textAsset != null, "[PlayerAudioModule::LoadScript] ERROR - can't load script " + text, true, DebugUtils.AssertType.Info))
		{
			return;
		}
		TextAssetParser textAssetParser = new TextAssetParser(textAsset);
		for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
		{
			Key key = textAssetParser.GetKey(i);
			string text2 = "m_" + key.GetName() + "Sounds";
			FieldInfo field = base.GetType().GetField(text2);
			if (!DebugUtils.Assert(field != null, "[PlayerAudioModule::LoadScript] ERROR - can't find member - " + text2, true, DebugUtils.AssertType.Info))
			{
				List<AudioClip> list = new List<AudioClip>();
				for (int j = 0; j < key.GetVariablesCount(); j++)
				{
					string svalue = key.GetVariable(j).SValue;
					AudioClip audioClip = Resources.Load<AudioClip>("Sounds/Player/" + svalue);
					if (!audioClip)
					{
						audioClip = Resources.Load<AudioClip>("Sounds/TempSounds/Player/" + svalue);
					}
					if (!audioClip)
					{
						audioClip = Resources.Load<AudioClip>("Sounds/PlayerVO/" + svalue);
					}
					if (!audioClip)
					{
						DebugUtils.Assert("[PlayerAudioModule::LoadScript] Can't load audio clip - " + svalue, true, DebugUtils.AssertType.Info);
					}
					else
					{
						list.Add(audioClip);
					}
				}
				field.SetValue(this, list);
			}
		}
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.FootStep)
		{
			this.PlayFootstepSound();
		}
	}

	public bool IsSoundPlaying(AudioClip clip)
	{
		for (int i = 0; i < this.m_AudioSources.Length; i++)
		{
			if (this.m_AudioSources[i].clip == clip)
			{
				return this.m_AudioSources[i].isPlaying;
			}
		}
		return false;
	}

	public bool IsSoundPlaying(List<AudioClip> clips)
	{
		foreach (AudioClip clip in clips)
		{
			if (this.IsSoundPlaying(clip))
			{
				return true;
			}
		}
		return false;
	}

	private AudioSource GetFreeSource()
	{
		if (this.m_AudioSources == null)
		{
			this.m_AudioSources = new AudioSource[this.m_AudioSourcesCount];
			for (int i = 0; i < this.m_AudioSourcesCount; i++)
			{
				this.m_AudioSources[i] = this.m_Player.gameObject.AddComponent<AudioSource>();
				this.m_AudioSources[i].outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
			}
		}
		for (int j = 0; j < this.m_AudioSources.Length; j++)
		{
			if (!this.m_AudioSources[j].isPlaying)
			{
				return this.m_AudioSources[j];
			}
		}
		Debug.Log("PlayerAudioModule::GetFreeSource - no free sources.");
		return null;
	}

	public AudioSource PlaySound(AudioClip clip, float volume = 1f, bool loop = false, Noise.Type noise_type = Noise.Type.None)
	{
		return this.PlaySound(this.GetFreeSource(), clip, volume, loop, noise_type);
	}

	public AudioSource PlayRandomSound(List<AudioClip> clips, float volume = 1f, bool loop = false, Noise.Type noise_type = Noise.Type.None)
	{
		if (clips == null || clips.Count == 0)
		{
			return null;
		}
		if (clips.Count == 1)
		{
			return this.PlaySound(clips[0], volume, loop, noise_type);
		}
		int index = UnityEngine.Random.Range(1, clips.Count);
		AudioClip audioClip = clips[index];
		clips[index] = clips[0];
		clips[0] = audioClip;
		return this.PlaySound(audioClip, volume, loop, noise_type);
	}

	private AudioSource PlaySound(AudioSource source, AudioClip clip, float volume = 1f, bool loop = false, Noise.Type noise_Type = Noise.Type.None)
	{
		if (!source || !clip)
		{
			return null;
		}
		source.clip = clip;
		source.loop = loop;
		source.volume = volume;
		source.Play();
		if (noise_Type != Noise.Type.None)
		{
			this.MakeNoise(noise_Type);
		}
		return source;
	}

	public void StopSound(AudioClip clip)
	{
		for (int i = 0; i < this.m_AudioSources.Length; i++)
		{
			if (this.m_AudioSources[i].clip == clip && this.m_AudioSources[i].isPlaying)
			{
				this.m_AudioSources[i].Stop();
				return;
			}
		}
	}

	private void MakeNoise(Noise.Type type)
	{
		Noise noise = new Noise();
		noise.m_Position = this.m_Player.transform.position;
		noise.m_Time = Time.time;
		noise.m_Type = type;
		this.m_NoiseManager.MakeNoise(noise);
	}

	public void PlayJumpSound()
	{
		this.PlayRandomSound(this.m_JumpSounds, 1f, false, Noise.Type.None);
	}

	public void PlayLandingSound()
	{
		this.PlayRandomSound(this.m_LandingSounds, 1f, false, Noise.Type.None);
	}

	public void PlayFeetLandingSound(float volume = 1f, bool loop = false)
	{
		switch (Player.Get().GetMaterial())
		{
		case EObjectMaterial.Bush:
		case EObjectMaterial.Grass:
			this.PlayRandomSound(this.m_GrassLandingSounds, volume, loop, Noise.Type.Action);
			break;
		case EObjectMaterial.Stone:
			this.PlayRandomSound(this.m_StoneLandingSounds, volume, loop, Noise.Type.Action);
			break;
		case EObjectMaterial.DryLeaves:
			this.PlayRandomSound(this.m_DryLeavesLandingSounds, volume, loop, Noise.Type.Action);
			break;
		case EObjectMaterial.Mud:
			this.PlayRandomSound(this.m_MudLandingSounds, volume, loop, Noise.Type.Action);
			break;
		case EObjectMaterial.Sand:
			this.PlayRandomSound(this.m_SandLandingSounds, volume, loop, Noise.Type.Action);
			break;
		case EObjectMaterial.Soil:
			this.PlayRandomSound(this.m_SoilLandingSounds, volume, loop, Noise.Type.Action);
			break;
		}
	}

	public void PlayFeetJumpSound(float volume = 1f, bool loop = false)
	{
		switch (Player.Get().GetMaterial())
		{
		case EObjectMaterial.Bush:
		case EObjectMaterial.Grass:
			this.PlayRandomSound(this.m_GrassJumpSounds, volume, loop, Noise.Type.Action);
			break;
		case EObjectMaterial.Stone:
			this.PlayRandomSound(this.m_StoneJumpSounds, volume, loop, Noise.Type.Action);
			break;
		case EObjectMaterial.DryLeaves:
			this.PlayRandomSound(this.m_DryLeavesJumpSounds, volume, loop, Noise.Type.Action);
			break;
		case EObjectMaterial.Mud:
			this.PlayRandomSound(this.m_MudJumpSounds, volume, loop, Noise.Type.Action);
			break;
		case EObjectMaterial.Sand:
			this.PlayRandomSound(this.m_SandJumpSounds, volume, loop, Noise.Type.Action);
			break;
		case EObjectMaterial.Soil:
			this.PlayRandomSound(this.m_SoilJumpSounds, volume, loop, Noise.Type.Action);
			break;
		}
	}

	public void PlaySanityLossSound(float volume = 1f)
	{
		this.PlayRandomSound(this.m_SanityLossSounds, volume, false, Noise.Type.None);
	}

	public void PlayDamageSound(float volume = 1f, bool loop = false)
	{
		this.PlayRandomSound(this.m_DamageSounds, volume, loop, Noise.Type.None);
	}

	public void PlayAttackSound(float volume = 1f, bool loop = false)
	{
		this.PlayRandomSound(this.m_AttackSounds, volume, loop, Noise.Type.Action);
	}

	public void PlayUseSound(float volume = 1f, bool loop = false)
	{
		this.PlayRandomSound(this.m_UseSounds, volume, loop, Noise.Type.None);
	}

	public void PlayHitSound(float volume = 1f, bool loop = false)
	{
		this.PlayRandomSound(this.m_HitSounds, volume, loop, Noise.Type.Action);
	}

	public void PlayHeartBeatSound(float volume = 1f, bool loop = false)
	{
		this.m_HeartBeatSoundAS = this.PlayRandomSound(this.m_HeartBeatSounds, volume, loop, Noise.Type.None);
	}

	public void StopHeartBeatSound()
	{
		if (this.m_HeartBeatSoundAS != null && this.m_HeartBeatSoundAS.isPlaying)
		{
			this.m_HeartBeatSoundAS.Stop();
			this.m_HeartBeatSoundAS = null;
		}
	}

	public bool IsHeartBeatSoundPlaying()
	{
		return this.m_HeartBeatSoundAS != null;
	}

	public void PlayFootstepSound()
	{
		if ((Player.Get().GetFPPController().m_LastCollisionFlags & CollisionFlags.Below) == CollisionFlags.None)
		{
			return;
		}
		Vector3 wantedSpeed = this.m_FPPController.m_WantedSpeed;
		wantedSpeed.y = 0f;
		if (wantedSpeed.magnitude < 0.1f)
		{
			return;
		}
		if (Time.time - this.m_LastFootstepSound < 0.1f)
		{
			return;
		}
		List<AudioClip> list = null;
		bool flag = Player.Get().GetFPPController().IsRunning();
		EObjectMaterial eobjectMaterial = EObjectMaterial.Unknown;
		if (this.m_Player.IsInWater() && !this.m_Player.m_SwimController.IsActive())
		{
			list = ((!flag) ? this.m_ShallowWaterWalkSounds : this.m_ShallowWaterRunSounds);
		}
		else
		{
			eobjectMaterial = Player.Get().GetMaterial();
			switch (eobjectMaterial)
			{
			case EObjectMaterial.Unknown:
			case EObjectMaterial.Sand:
				list = ((!flag) ? this.m_SandStepWalkSounds : this.m_SandStepRunSounds);
				break;
			case EObjectMaterial.Bush:
			case EObjectMaterial.Grass:
				list = ((!flag) ? this.m_GrassStepWalkSounds : this.m_GrassStepRunSounds);
				break;
			case EObjectMaterial.Stone:
				list = ((!flag) ? this.m_StoneStepWalkSounds : this.m_StoneStepRunSounds);
				break;
			case EObjectMaterial.DryLeaves:
				list = ((!flag) ? this.m_DryLeavesStepWalkSounds : this.m_DryLeavesStepRunSounds);
				break;
			case EObjectMaterial.Mud:
				list = ((!flag) ? this.m_MudStepWalkSounds : this.m_MudStepRunSounds);
				break;
			case EObjectMaterial.Soil:
				list = ((!flag) ? this.m_SoilStepWalkSounds : this.m_SoilStepRunSounds);
				break;
			}
		}
		Noise.Type noise_type = Noise.Type.None;
		if (FPPController.Get().IsWalking())
		{
			noise_type = ((!FPPController.Get().IsDuck()) ? Noise.Type.Walk : Noise.Type.Sneak);
		}
		else if (flag)
		{
			noise_type = ((!FPPController.Get().IsDuck()) ? Noise.Type.Run : Noise.Type.Sneak);
		}
		else if (SwimController.Get().IsSwimming())
		{
			noise_type = Noise.Type.Swim;
		}
		if (list == null)
		{
			Debug.Log("ERROR PlayerAudioModule PlayFootstepSound no sounds clips player_pos=" + Player.Get().transform.position.ToString() + " Material = " + eobjectMaterial.ToString());
		}
		this.PlayRandomSound(list, 1f, false, noise_type);
		this.m_LastFootstepSound = Time.time;
	}

	public void PlayEatingSound(float volume = 1f, bool loop = false)
	{
		this.PlayRandomSound(this.m_EatingSounds, volume, loop, Noise.Type.None);
	}

	public void PlayEatingDisgustingSound(float volume = 1f, bool loop = false)
	{
		this.PlayRandomSound(this.m_EatingDisgustingSounds, volume, loop, Noise.Type.None);
	}

	public void PlayDrinkingSound(float volume = 1f, bool loop = false)
	{
		this.PlayRandomSound(this.m_DrinkingSounds, volume, loop, Noise.Type.None);
	}

	public void PlayDrinkingDisgustingSound(float volume = 1f, bool loop = false)
	{
		this.PlayRandomSound(this.m_DrinkingDisgustingSounds, volume, loop, Noise.Type.None);
	}

	public void PlayBreathingSound(float volume = 1f, bool loop = false)
	{
		this.m_PlayingBreathSoundSource = this.PlayRandomSound(this.m_BreathSounds, 0f, loop, Noise.Type.None);
		base.StartCoroutine(AudioFadeOut.FadeIn(this.m_PlayingBreathSoundSource, 1f, volume));
	}

	public void PlayVomitingSound(float volume = 1f, bool loop = false)
	{
		this.PlayRandomSound(this.m_VomitingSounds, volume, loop, Noise.Type.Action);
	}

	public void PlayPlantsPushingSound(float volume = 1f, bool loop = false)
	{
		this.PlayRandomSound(this.m_PlantsPushingSounds, volume, loop, Noise.Type.None);
	}

	public void PlayWaterSpillSound(float volume = 1f, bool loop = false)
	{
		this.PlayRandomSound(this.m_WaterSpillSounds, volume, loop, Noise.Type.Action);
	}

	public void PlayDialogSound(float volume = 1f, bool loop = false)
	{
		this.PlayRandomSound(this.m_DialogSounds, volume, loop, Noise.Type.None);
	}

	public void StopBreathingSound()
	{
		if (this.m_PlayingBreathSoundSource != null)
		{
			this.m_PlayingBreathSoundSource.Stop();
		}
		this.m_PlayingBreathSoundSource = null;
	}

	public void FadeOutBreathingSound()
	{
		if (this.m_PlayingBreathSoundSource == null)
		{
			return;
		}
		base.StartCoroutine(AudioFadeOut.FadeOut(this.m_PlayingBreathSoundSource, 0.2f));
		this.m_BreathFadeOutStartTime = Time.time;
	}

	public override void OnTakeDamage(DamageInfo info)
	{
		base.OnTakeDamage(info);
		if (!info.m_Blocked && info.m_PlayDamageSound && Time.time - this.m_LastPlayDamageSound > 2f)
		{
			this.PlayDamageSound(1f, false);
			this.m_LastPlayDamageSound = Time.time;
		}
	}

	public override void Update()
	{
		base.Update();
		this.UpdateBreathStop();
		this.UpdateLowStamina();
	}

	private void UpdateBreathStop()
	{
		if (this.m_Player.GetComponent<SwimController>().m_State != SwimState.Dive && this.m_PlayingBreathSoundSource != null && this.m_BreathFadeOutStartTime > 0f && Time.time - this.m_BreathFadeOutStartTime > 0.2f)
		{
			this.StopBreathingSound();
			this.m_BreathFadeOutStartTime = -1f;
		}
	}

	private void UpdateLowStamina()
	{
		if (this.m_PlayingLowStaminaSoundSource != null && !this.m_PlayingLowStaminaSoundSource.isPlaying)
		{
			this.m_PlayingLowStaminaSoundSource = null;
		}
		if (PlayerConditionModule.Get().IsStaminaCriticalLevel())
		{
			if (this.m_PlayingLowStaminaSoundSource == null)
			{
				this.m_PlayingLowStaminaSoundSource = this.PlayRandomSound(this.m_BreathLowStaminaSounds, 1f, false, Noise.Type.None);
			}
		}
		else if (this.m_PlayingLowStaminaSoundSource && this.m_PlayingLowStaminaSoundSource.isPlaying)
		{
			base.StartCoroutine(AudioFadeOut.FadeOut(this.m_PlayingLowStaminaSoundSource, 0.2f));
			this.m_PlayingLowStaminaSoundSource.SetScheduledEndTime(AudioSettings.dspTime + 0.20000000298023224);
			this.m_PlayingLowStaminaSoundSource = null;
		}
	}

	public void PlayItemSound(string sound)
	{
		if (sound.Contains("item_grab_food"))
		{
			this.PlayRandomSound(this.m_item_grab_foodSounds, 1f, false, Noise.Type.None);
		}
		else if (sound.Contains("item_grab_hard"))
		{
			this.PlayRandomSound(this.m_item_grab_hardSounds, 1f, false, Noise.Type.None);
		}
		else if (sound.Contains("item_grab_leaf_herb"))
		{
			this.PlayRandomSound(this.m_item_grab_leaf_herbSounds, 1f, false, Noise.Type.None);
		}
	}

	public void PlayHOPickupSound()
	{
		this.PlayRandomSound(this.m_PickUpHOSounds, 1f, false, Noise.Type.None);
	}

	public void PlayHODropSound()
	{
		this.PlayRandomSound(this.m_DropHOSounds, 1f, false, Noise.Type.Action);
	}

	public void PlayInsertHOInSlotSound()
	{
		this.PlayRandomSound(this.m_InsertHOInSlotSounds, 1f, false, Noise.Type.None);
	}

	public void PlayInsertItemInSlotSound()
	{
		this.PlayRandomSound(this.m_InsertItemInSlotSounds, 1f, false, Noise.Type.None);
	}

	public void PlayBuildCompletedSound()
	{
		this.PlayRandomSound(this.m_BuildCompletedSounds, 1f, false, Noise.Type.None);
	}

	public void PlayNotepadEntrySound()
	{
		if (this.m_NotepadEntrySound && this.m_NotepadEntrySound.isPlaying)
		{
			return;
		}
		this.m_NotepadEntrySound = this.PlayRandomSound(this.m_NotepadEntrySounds, 1f, false, Noise.Type.None);
	}

	public void PlayDiveSound()
	{
		this.PlayRandomSound(this.m_DiveSounds, 1f, false, Noise.Type.None);
	}

	public void PlaySwimSound()
	{
		this.PlayRandomSound(this.m_SwimSounds, 1f, false, Noise.Type.None);
	}

	public void PlayDeathSound()
	{
		this.PlayRandomSound(this.m_DeathSounds, 1f, false, Noise.Type.None);
	}

	public void PlayBeforeDivingSound()
	{
		this.PlayRandomSound(this.m_BeforeDivingSounds, 1f, false, Noise.Type.None);
	}

	public void PlayUnderwaterDeathSound()
	{
		this.PlayRandomSound(this.m_UnderwaterDeathSounds, 1f, false, Noise.Type.None);
	}

	public void PlayPassOutSound()
	{
		this.PlayRandomSound(this.m_PassOutSounds, 1f, false, Noise.Type.None);
	}

	public void PlayBILeftArmStart()
	{
		this.PlayRandomSound(this.m_BISelectLeftArmSounds, 1f, false, Noise.Type.None);
	}

	public void PlayBIRightArmStart()
	{
		this.PlayRandomSound(this.m_BISelectRightArmSounds, 1f, false, Noise.Type.None);
	}

	public void PlayBILeftLegStart()
	{
		this.PlayRandomSound(this.m_BISelectLeftLegSounds, 1f, false, Noise.Type.None);
	}

	public void PlayBIRightLegStart()
	{
		this.PlayRandomSound(this.m_BISelectRightLegSounds, 1f, false, Noise.Type.None);
	}

	public void PlayWatchShowSound()
	{
		this.PlayRandomSound(this.m_WatchShowSounds, 1f, false, Noise.Type.None);
	}

	public void PlayWatchHideSound()
	{
		this.PlayRandomSound(this.m_WatchHideSounds, 1f, false, Noise.Type.None);
	}

	public void PlayWatchSwitchTabSound()
	{
		this.PlayRandomSound(this.m_WatchTabSwitchSounds, 1f, false, Noise.Type.None);
	}

	public void PlayHarvestAnimalSound()
	{
		this.PlayRandomSound(this.m_HarvestAnimalSounds, 1f, false, Noise.Type.None);
	}

	public void StopHarvestAnimalSound()
	{
		foreach (AudioClip clip in this.m_HarvestAnimalSounds)
		{
			this.StopSound(clip);
		}
	}

	public void PlaySleepSound()
	{
		this.PlaySound(this.m_SleepSoundSource, this.m_SleepSounds[UnityEngine.Random.Range(0, this.m_SleepSounds.Count)], 1f, false, Noise.Type.None);
	}

	public void PlayBodyFallSound()
	{
		switch (Player.Get().GetMaterial())
		{
		case EObjectMaterial.Bush:
		case EObjectMaterial.Grass:
			this.PlayRandomSound(this.m_BodyFallGrassSounds, 1f, false, Noise.Type.None);
			break;
		case EObjectMaterial.Stone:
			this.PlayRandomSound(this.m_BodyFallStoneSounds, 1f, false, Noise.Type.None);
			break;
		case EObjectMaterial.DryLeaves:
			this.PlayRandomSound(this.m_BodyFallLeavesSounds, 1f, false, Noise.Type.None);
			break;
		case EObjectMaterial.Mud:
			this.PlayRandomSound(this.m_BodyFallMudSounds, 1f, false, Noise.Type.None);
			break;
		case EObjectMaterial.Sand:
			this.PlayRandomSound(this.m_BodyFallSandSounds, 1f, false, Noise.Type.None);
			break;
		case EObjectMaterial.Soil:
			this.PlayRandomSound(this.m_BodyFallGrassSounds, 1f, false, Noise.Type.None);
			break;
		}
	}

	public void PlayLadderSound()
	{
		this.PlayRandomSound(this.m_LadderSounds, 1f, false, Noise.Type.None);
	}

	public void PlayLeechOutSound()
	{
		this.PlayRandomSound(this.m_LeechOutSounds, 1f, false, Noise.Type.None);
	}

	public void PlayToolDestroyedSound()
	{
		this.PlayRandomSound(this.m_ToolDestroyedSounds, 1f, false, Noise.Type.None);
	}

	public void PlayMakeFireSound()
	{
		this.m_MakeFireSource = this.PlayRandomSound(this.m_MakeFireSounds, 1f, false, Noise.Type.None);
	}

	public void StopMakeFireSound()
	{
		if (this.m_MakeFireSource)
		{
			this.m_MakeFireSource.Stop();
			this.m_MakeFireSource = null;
		}
	}

	public void PlayMakeFireSuccessSound()
	{
		this.PlayRandomSound(this.m_MakeFireSuccessSounds, 1f, false, Noise.Type.None);
	}

	public void PlayMakeFireFailSound()
	{
		this.PlayRandomSound(this.m_MakeFireFailSounds, 1f, false, Noise.Type.None);
	}

	private int m_AudioSourcesCount = 16;

	private AudioSource[] m_AudioSources;

	private NoiseManager m_NoiseManager;

	private float m_BreathFadeOutStartTime = -1f;

	[HideInInspector]
	public List<AudioClip> m_EatingSounds;

	[HideInInspector]
	public List<AudioClip> m_EatingDisgustingSounds;

	[HideInInspector]
	public List<AudioClip> m_DrinkingSounds;

	[HideInInspector]
	public List<AudioClip> m_DrinkingDisgustingSounds;

	[HideInInspector]
	public List<AudioClip> m_HitSounds;

	[HideInInspector]
	public List<AudioClip> m_UseSounds;

	[HideInInspector]
	public List<AudioClip> m_AttackSounds;

	[HideInInspector]
	public List<AudioClip> m_DamageSounds;

	[HideInInspector]
	public List<AudioClip> m_BreathSounds;

	[HideInInspector]
	public List<AudioClip> m_BreathLowStaminaSounds;

	[HideInInspector]
	public List<AudioClip> m_LandingSounds;

	[HideInInspector]
	public List<AudioClip> m_VomitingSounds;

	[HideInInspector]
	public List<AudioClip> m_PlantsPushingSounds;

	[HideInInspector]
	public List<AudioClip> m_item_grab_foodSounds;

	[HideInInspector]
	public List<AudioClip> m_item_grab_hardSounds;

	[HideInInspector]
	public List<AudioClip> m_item_grab_leaf_herbSounds;

	[HideInInspector]
	public List<AudioClip> m_DryLeavesStepWalkSounds;

	[HideInInspector]
	public List<AudioClip> m_GrassStepWalkSounds;

	[HideInInspector]
	public List<AudioClip> m_MudStepWalkSounds;

	[HideInInspector]
	public List<AudioClip> m_StoneStepWalkSounds;

	[HideInInspector]
	public List<AudioClip> m_SandStepWalkSounds;

	[HideInInspector]
	public List<AudioClip> m_SoilStepWalkSounds;

	[HideInInspector]
	public List<AudioClip> m_ShallowWaterWalkSounds;

	[HideInInspector]
	public List<AudioClip> m_DryLeavesStepRunSounds;

	[HideInInspector]
	public List<AudioClip> m_GrassStepRunSounds;

	[HideInInspector]
	public List<AudioClip> m_MudStepRunSounds;

	[HideInInspector]
	public List<AudioClip> m_StoneStepRunSounds;

	[HideInInspector]
	public List<AudioClip> m_SandStepRunSounds;

	[HideInInspector]
	public List<AudioClip> m_SoilStepRunSounds;

	[HideInInspector]
	public List<AudioClip> m_ShallowWaterRunSounds;

	[HideInInspector]
	public List<AudioClip> m_JumpSounds;

	[HideInInspector]
	public List<AudioClip> m_DryLeavesJumpSounds;

	[HideInInspector]
	public List<AudioClip> m_GrassJumpSounds;

	[HideInInspector]
	public List<AudioClip> m_MudJumpSounds;

	[HideInInspector]
	public List<AudioClip> m_StoneJumpSounds;

	[HideInInspector]
	public List<AudioClip> m_SandJumpSounds;

	[HideInInspector]
	public List<AudioClip> m_SoilJumpSounds;

	[HideInInspector]
	public List<AudioClip> m_DryLeavesLandingSounds;

	[HideInInspector]
	public List<AudioClip> m_GrassLandingSounds;

	[HideInInspector]
	public List<AudioClip> m_MudLandingSounds;

	[HideInInspector]
	public List<AudioClip> m_StoneLandingSounds;

	[HideInInspector]
	public List<AudioClip> m_SandLandingSounds;

	[HideInInspector]
	public List<AudioClip> m_SoilLandingSounds;

	[HideInInspector]
	public List<AudioClip> m_WaterSpillSounds;

	[HideInInspector]
	public List<AudioClip> m_PickUpHOSounds;

	[HideInInspector]
	public List<AudioClip> m_DropHOSounds;

	[HideInInspector]
	public List<AudioClip> m_InsertHOInSlotSounds;

	[HideInInspector]
	public List<AudioClip> m_InsertItemInSlotSounds;

	[HideInInspector]
	public List<AudioClip> m_BuildCompletedSounds;

	[HideInInspector]
	public List<AudioClip> m_NotepadEntrySounds;

	[HideInInspector]
	public List<AudioClip> m_DiveSounds;

	[HideInInspector]
	public List<AudioClip> m_SwimSounds;

	[HideInInspector]
	public List<AudioClip> m_WaterHitSounds;

	[HideInInspector]
	public List<AudioClip> m_HeartBeatSounds;

	[HideInInspector]
	public List<AudioClip> m_DeathSounds;

	[HideInInspector]
	public List<AudioClip> m_UnderwaterDeathSounds;

	[HideInInspector]
	public List<AudioClip> m_BeforeDivingSounds;

	[HideInInspector]
	public List<AudioClip> m_PassOutSounds;

	[HideInInspector]
	public List<AudioClip> m_BISelectLeftArmSounds;

	[HideInInspector]
	public List<AudioClip> m_BISelectRightArmSounds;

	[HideInInspector]
	public List<AudioClip> m_BISelectLeftLegSounds;

	[HideInInspector]
	public List<AudioClip> m_BISelectRightLegSounds;

	[HideInInspector]
	public List<AudioClip> m_WatchShowSounds;

	[HideInInspector]
	public List<AudioClip> m_WatchHideSounds;

	[HideInInspector]
	public List<AudioClip> m_WatchTabSwitchSounds;

	[HideInInspector]
	public List<AudioClip> m_HarvestAnimalSounds;

	[HideInInspector]
	public List<AudioClip> m_BodyFallGrassSounds;

	[HideInInspector]
	public List<AudioClip> m_BodyFallLeavesSounds;

	[HideInInspector]
	public List<AudioClip> m_BodyFallMudSounds;

	[HideInInspector]
	public List<AudioClip> m_BodyFallStoneSounds;

	[HideInInspector]
	public List<AudioClip> m_BodyFallSandSounds;

	[HideInInspector]
	public List<AudioClip> m_BodyFallShalowWaterSounds;

	[HideInInspector]
	public List<AudioClip> m_BodyFallSwampSounds;

	[HideInInspector]
	public List<AudioClip> m_SleepSounds;

	[HideInInspector]
	public List<AudioClip> m_SanityLossSounds;

	[HideInInspector]
	public List<AudioClip> m_LadderSounds;

	[HideInInspector]
	public List<AudioClip> m_DialogSounds;

	[HideInInspector]
	public List<AudioClip> m_LeechOutSounds;

	[HideInInspector]
	public List<AudioClip> m_ToolDestroyedSounds;

	[HideInInspector]
	public List<AudioClip> m_MakeFireSounds;

	[HideInInspector]
	public List<AudioClip> m_MakeFireSuccessSounds;

	[HideInInspector]
	public List<AudioClip> m_MakeFireFailSounds;

	public AudioSource m_PlayingBreathSoundSource;

	public AudioSource m_PlayingLowStaminaSoundSource;

	[HideInInspector]
	public AudioSource m_SleepSoundSource;

	private AudioSource m_NotepadEntrySound;

	private static PlayerAudioModule s_Instance;

	private FPPController m_FPPController;

	private AudioSource m_MakeFireSource;

	private AudioSource m_HeartBeatSoundAS;

	private float m_LastFootstepSound = float.MinValue;

	private float m_LastPlayDamageSound = float.MinValue;
}
