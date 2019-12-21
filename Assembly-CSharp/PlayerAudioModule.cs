using System;
using System.Collections.Generic;
using System.Reflection;
using CJTools;
using Enums;
using UnityEngine;

public class PlayerAudioModule : PlayerModule, IAnimationEventsReceiverEx, IAnimationEventsReceiver
{
	public PlayerAudioModule.GruntPriority m_GruntSoundPlayed { get; private set; } = PlayerAudioModule.GruntPriority.None;

	private List<AudioClip> GetGruntClips(PlayerAudioModule.GruntPriority grunt)
	{
		switch (grunt)
		{
		case PlayerAudioModule.GruntPriority.Breathing:
			return this.m_BreathSounds;
		case PlayerAudioModule.GruntPriority.LowStamina:
			return this.m_BreathLowStaminaSounds;
		case PlayerAudioModule.GruntPriority.Jump:
			return this.m_JumpSounds;
		case PlayerAudioModule.GruntPriority.Landing:
			return this.m_LandingSounds;
		case PlayerAudioModule.GruntPriority.Attack:
			return this.m_AttackSounds;
		case PlayerAudioModule.GruntPriority.ToolDestroyed:
			return this.m_ToolDestroyedSounds;
		case PlayerAudioModule.GruntPriority.LeechOut:
			return this.m_LeechOutSounds;
		case PlayerAudioModule.GruntPriority.Drinking:
			return this.m_DrinkingSounds;
		case PlayerAudioModule.GruntPriority.Eating:
			return this.m_EatingSounds;
		case PlayerAudioModule.GruntPriority.Disgusting:
			return this.m_EatingDisgustingSounds;
		case PlayerAudioModule.GruntPriority.DamageInsects:
			return this.m_DamageInsectsSounds;
		case PlayerAudioModule.GruntPriority.HitReaction:
			return this.m_DamageSounds;
		case PlayerAudioModule.GruntPriority.Vomiting:
			return this.m_VomitingSounds;
		case PlayerAudioModule.GruntPriority.PassOut:
			return this.m_PassOutSounds;
		case PlayerAudioModule.GruntPriority.Death:
			return this.m_DeathSounds;
		default:
			return null;
		}
	}

	public static PlayerAudioModule Get()
	{
		return PlayerAudioModule.s_Instance;
	}

	private void Awake()
	{
		if (base.GetComponent<Player>())
		{
			PlayerAudioModule.s_Instance = this;
		}
		GreenHellGame.Instance.OnAudioSnapshotChangedEvent += this.OnAudioSnapshotChanged;
		this.SetupWeaponAudio();
		this.SetupHitSpecificSounds();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (GreenHellGame.Instance)
		{
			GreenHellGame.Instance.OnAudioSnapshotChangedEvent -= this.OnAudioSnapshotChanged;
		}
	}

	public void InitSources()
	{
		if (this.m_SourcesInited)
		{
			return;
		}
		this.m_AudioSources = new List<AudioSource>(this.m_AudioSourcesCount);
		for (int i = 0; i < this.m_AudioSourcesCount; i++)
		{
			this.m_AudioSources.Add(this.AddSource(AudioMixerGroupGame.Player));
		}
		this.m_SleepSoundSource = this.AddSource(AudioMixerGroupGame.Player);
		this.m_GruntSoundSource = this.AddSource(AudioMixerGroupGame.Player);
		this.m_MakeFireSource = this.AddSource(AudioMixerGroupGame.Player);
		this.m_NotepadEntrySound = this.AddSource(AudioMixerGroupGame.Player);
		for (int j = 0; j < 3; j++)
		{
			this.m_DiveSources.Add(this.AddSource(AudioMixerGroupGame.Enviro));
		}
		this.m_SourcesInited = true;
	}

	public override void Initialize(Being being)
	{
		this.m_Being = being;
		this.m_Player = base.gameObject.GetComponent<Player>();
		this.InitSources();
		this.m_NoiseManager = MainLevel.Instance.GetComponent<NoiseManager>();
		DebugUtils.Assert(this.m_NoiseManager, "[PlayerAudioModule::Initialize] Can't find NoiseManager", true, DebugUtils.AssertType.Info);
		this.LoadScript();
		this.InitializeDreamsAudio();
	}

	private AudioSource AddSource(AudioMixerGroupGame group = AudioMixerGroupGame.Player)
	{
		AudioSource audioSource = base.gameObject.AddComponent<AudioSource>();
		audioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(group);
		audioSource.playOnAwake = false;
		audioSource.priority = 0;
		if (this.m_Player == null)
		{
			audioSource.spatialBlend = 1f;
		}
		return audioSource;
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
		if (id == AnimEventID.Dream4_WT_Click || id == AnimEventID.Dream4_WT_Raise || id == AnimEventID.Dream4_WT_Stop || id == AnimEventID.Dream4_WT_TurnOff || id == AnimEventID.Dream4_StandUp || id == AnimEventID.PreDream_Ayahuasca_Throw_Ingredients || id == AnimEventID.AIRPORT_RADIO_SFX_SWITCH_CHANNEL_1 || id == AnimEventID.AIRPORT_RADIO_SFX_SWITCH_CHANNEL_2 || id == AnimEventID.AIRPORT_RADIO_SFX_PUNCH_RADIO || id == AnimEventID.AIRPORT_RADIO_SFX_PUNCH_DESK_AND_KEY_DROPS || id == AnimEventID.AIRPORT_RADIO_SFX_CHAT_BUTTON_PRESS || id == AnimEventID.AIRPORT_RADIO_SFX_CHAT_BUTTON_RELEASE || id == AnimEventID.END1_SFX_RADIO_POWER_DOWN)
		{
			this.PlayAnimEventSound(id);
			return;
		}
		if (id == AnimEventID.MeleeRightAttackStart || id == AnimEventID.MeleeLeftAttackStart || id == AnimEventID.MeleeUpAttackStart || id == AnimEventID.MeleeThrustAttackStart)
		{
			this.PlayAttackSound(1f, false);
		}
	}

	public void OnAnimEventEx(AnimationEvent anim_event)
	{
		if (anim_event.animatorStateInfo.shortNameHash == this.m_MoveBlendTreeHash)
		{
			if (anim_event.animatorClipInfo.weight < 0.5f)
			{
				return;
			}
			if (anim_event.intParameter == 0 && Player.Get().m_FootestepsSoundsEnabled)
			{
				this.PlayFootstepSound();
			}
		}
	}

	public bool IsSoundPlaying(AudioClip clip)
	{
		for (int i = 0; i < this.m_AudioSources.Count; i++)
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
			this.InitSources();
		}
		for (int i = 0; i < this.m_AudioSources.Count; i++)
		{
			if (!this.m_AudioSources[i].isPlaying && !this.m_AudioSources[i].isVirtual)
			{
				return this.m_AudioSources[i];
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

	public AudioSource PlayRandomSound(AudioSource source, List<AudioClip> clips, float volume = 1f, bool loop = false, Noise.Type noise_type = Noise.Type.None)
	{
		if (clips == null || clips.Count == 0)
		{
			return null;
		}
		if (clips.Count == 1)
		{
			return this.PlaySound(source, clips[0], volume, loop, noise_type);
		}
		int index = UnityEngine.Random.Range(1, clips.Count);
		AudioClip audioClip = clips[index];
		clips[index] = clips[0];
		clips[0] = audioClip;
		return this.PlaySound(source, audioClip, volume, loop, noise_type);
	}

	private AudioSource PlaySound(AudioSource source, AudioClip clip, float volume = 1f, bool loop = false, Noise.Type noise_Type = Noise.Type.None)
	{
		if (!source || !clip)
		{
			return null;
		}
		source.Stop();
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
		for (int i = 0; i < this.m_AudioSources.Count; i++)
		{
			if (this.m_AudioSources[i].clip == clip && this.m_AudioSources[i].isPlaying)
			{
				this.m_AudioSources[i].Stop();
				return;
			}
		}
	}

	public AudioSource PlayGruntSound(PlayerAudioModule.GruntPriority grunt_type, List<AudioClip> clips = null, float volume = 1f, bool loop = false, Noise.Type noise_type = Noise.Type.None, float fadein_duration = 0f)
	{
		DialogsManager dialogsManager = DialogsManager.Get();
		if (dialogsManager != null && dialogsManager.IsPlayerSpeaking())
		{
			return null;
		}
		if (clips == null)
		{
			clips = this.GetGruntClips(grunt_type);
		}
		if (this.ReplIsOwner())
		{
			this.GetPlayerComponent<ReplicatedPlayerSounds>().ReplicateSound(new ReplicatedPlayerSounds.SSoundData
			{
				type = ReplicatedPlayerSounds.EReplicatedSoundType.Grunt,
				grunt = grunt_type
			});
		}
		if (!this.m_GruntSoundSource.isPlaying || grunt_type > this.m_GruntSoundPlayed)
		{
			if (this.m_GruntFadeoutCoroutine != null)
			{
				base.StopCoroutine(this.m_GruntFadeoutCoroutine);
				this.m_GruntFadeoutCoroutine = null;
			}
			if (this.m_GruntFadeinCoroutine != null)
			{
				base.StopCoroutine(this.m_GruntFadeinCoroutine);
				this.m_GruntFadeinCoroutine = null;
			}
			this.m_GruntSoundPlayed = grunt_type;
			this.PlayRandomSound(this.m_GruntSoundSource, clips, (fadein_duration > 0f) ? 0f : volume, loop, noise_type);
			if (fadein_duration > 0f)
			{
				this.m_GruntFadeinCoroutine = base.StartCoroutine(AudioFadeOut.FadeIn(this.m_GruntSoundSource, fadein_duration, volume, null));
			}
			return this.m_GruntSoundSource;
		}
		return null;
	}

	public void StopGruntSound(PlayerAudioModule.GruntPriority grunt_type, float duration = 0f)
	{
		if (this.m_GruntSoundPlayed == grunt_type && this.m_GruntSoundSource.isPlaying)
		{
			this.m_GruntSoundPlayed = PlayerAudioModule.GruntPriority.None;
			if (duration > 0f)
			{
				this.m_GruntFadeoutCoroutine = base.StartCoroutine(AudioFadeOut.FadeOut(this.m_GruntSoundSource, duration, 0f, null));
				this.m_GruntSoundSource.SetScheduledEndTime(AudioSettings.dspTime + (double)duration);
				return;
			}
			this.m_GruntSoundSource.Stop();
		}
	}

	private void MakeNoise(Noise.Type type)
	{
		Noise noise = new Noise();
		noise.m_Position = base.transform.position;
		noise.m_Time = Time.time;
		noise.m_Type = type;
		this.m_NoiseManager.MakeNoise(noise);
	}

	public void PlayJumpSound()
	{
		this.PlayGruntSound(PlayerAudioModule.GruntPriority.Jump, null, 1f, false, Noise.Type.None, 0f);
	}

	public void PlayLandingSound()
	{
		if (this.GetPlayerComponent<ReplicatedPlayerParams>().m_IsInWater)
		{
			this.PlayRandomSound(this.m_LandingWaterSounds, 1f, false, Noise.Type.None);
			return;
		}
		this.PlayGruntSound(PlayerAudioModule.GruntPriority.Landing, null, 1f, false, Noise.Type.None, 0f);
	}

	public void PlayFeetLandingSound(float volume = 1f, bool loop = false)
	{
		if (this.GetPlayerComponent<ReplicatedPlayerParams>().m_IsInWater)
		{
			this.PlayRandomSound(this.m_LandingWaterSounds, 1f, false, Noise.Type.None);
			return;
		}
		switch (this.m_Being.GetMaterial())
		{
		case EObjectMaterial.Wood:
			this.PlayRandomSound(this.m_WoodLandingSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.Bush:
		case EObjectMaterial.Grass:
			this.PlayRandomSound(this.m_GrassLandingSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.Stone:
			this.PlayRandomSound(this.m_StoneLandingSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.DryLeaves:
			this.PlayRandomSound(this.m_DryLeavesLandingSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.Mud:
			this.PlayRandomSound(this.m_MudLandingSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.Sand:
			this.PlayRandomSound(this.m_SandLandingSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.Flesh:
		case EObjectMaterial.Moss:
		case EObjectMaterial.Water:
		case EObjectMaterial.TurtleShell:
			break;
		case EObjectMaterial.Soil:
			this.PlayRandomSound(this.m_SoilLandingSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.WoodTree:
			this.PlayRandomSound(this.m_WoodTreeLandingSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.Metal:
			this.PlayRandomSound(this.m_MetalLandingSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.Tent:
			this.PlayRandomSound(this.m_TentLandingSounds, volume, loop, Noise.Type.Action);
			break;
		default:
			return;
		}
	}

	public void PlayFeetJumpSound(float volume = 1f, bool loop = false)
	{
		switch (this.m_Being.GetMaterial())
		{
		case EObjectMaterial.Wood:
			this.PlayRandomSound(this.m_WoodJumpSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.Bush:
		case EObjectMaterial.Grass:
			this.PlayRandomSound(this.m_GrassJumpSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.Stone:
			this.PlayRandomSound(this.m_StoneJumpSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.DryLeaves:
			this.PlayRandomSound(this.m_DryLeavesJumpSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.Mud:
			this.PlayRandomSound(this.m_MudJumpSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.Sand:
			this.PlayRandomSound(this.m_SandJumpSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.Flesh:
		case EObjectMaterial.Moss:
		case EObjectMaterial.Water:
		case EObjectMaterial.TurtleShell:
			break;
		case EObjectMaterial.Soil:
			this.PlayRandomSound(this.m_SoilJumpSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.WoodTree:
			this.PlayRandomSound(this.m_WoodTreeJumpSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.Metal:
			this.PlayRandomSound(this.m_MetalJumpSounds, volume, loop, Noise.Type.Action);
			return;
		case EObjectMaterial.Tent:
			this.PlayRandomSound(this.m_TentJumpSounds, volume, loop, Noise.Type.Action);
			break;
		default:
			return;
		}
	}

	public void PlaySanityLossSound(float volume = 1f)
	{
		this.PlayRandomSound(this.m_SanityLossSounds, volume, false, Noise.Type.None);
	}

	public void PlayDamageSound(float volume = 1f, bool loop = false)
	{
		this.PlayGruntSound(PlayerAudioModule.GruntPriority.HitReaction, null, volume, loop, Noise.Type.None, 0f);
	}

	public void PlayDamageInsectsSound(float volume = 1f, bool loop = false)
	{
		this.PlayGruntSound(PlayerAudioModule.GruntPriority.DamageInsects, null, volume, loop, Noise.Type.None, 0f);
	}

	public void PlayAttackSound(float volume = 1f, bool loop = false)
	{
		this.PlayGruntSound(PlayerAudioModule.GruntPriority.Attack, null, volume, loop, Noise.Type.Action, 0f);
	}

	public void PlayUseSound(float volume = 1f, bool loop = false)
	{
		this.PlayRandomSound(this.m_UseSounds, volume, loop, Noise.Type.None);
	}

	public void PlayHitSound(float volume = 1f, bool loop = false)
	{
		this.PlayRandomSound(this.m_HitSounds, volume, loop, Noise.Type.Action);
	}

	public void PlayHitArmorSound(float volume = 1f, bool loop = false)
	{
		this.PlayRandomSound(this.m_HitArmorSounds, volume, loop, Noise.Type.Action);
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
		if ((this.GetPlayerComponent<ReplicatedPlayerParams>().m_LastCollisionFlags & 4) == 0)
		{
			return;
		}
		if (this.GetPlayerComponent<ReplicatedPlayerParams>().m_WantedSpeed2d < 0.1f)
		{
			return;
		}
		if (Time.time - this.m_LastFootstepSound < 0.1f)
		{
			return;
		}
		bool isRunning = this.GetPlayerComponent<ReplicatedPlayerParams>().m_IsRunning;
		EObjectMaterial eobjectMaterial = EObjectMaterial.Unknown;
		List<AudioClip> list;
		if (this.GetPlayerComponent<ReplicatedPlayerParams>().m_IsInWater && !this.GetPlayerComponent<ReplicatedPlayerParams>().m_IsSwimming)
		{
			list = (isRunning ? this.m_ShallowWaterRunSounds : this.m_ShallowWaterWalkSounds);
		}
		else
		{
			eobjectMaterial = this.m_Being.GetMaterial();
			switch (eobjectMaterial)
			{
			case EObjectMaterial.Wood:
				list = (isRunning ? this.m_WoodRunSounds : this.m_WoodWalkSounds);
				goto IL_1B5;
			case EObjectMaterial.Bush:
			case EObjectMaterial.Grass:
				list = (isRunning ? this.m_GrassStepRunSounds : this.m_GrassStepWalkSounds);
				goto IL_1B5;
			case EObjectMaterial.Stone:
				list = (isRunning ? this.m_StoneStepRunSounds : this.m_StoneStepWalkSounds);
				goto IL_1B5;
			case EObjectMaterial.DryLeaves:
				list = (isRunning ? this.m_DryLeavesStepRunSounds : this.m_DryLeavesStepWalkSounds);
				goto IL_1B5;
			case EObjectMaterial.Mud:
				list = (isRunning ? this.m_MudStepRunSounds : this.m_MudStepWalkSounds);
				goto IL_1B5;
			case EObjectMaterial.Soil:
				list = (isRunning ? this.m_SoilStepRunSounds : this.m_SoilStepWalkSounds);
				goto IL_1B5;
			case EObjectMaterial.Water:
				list = (isRunning ? this.m_ShallowWaterRunSounds : this.m_ShallowWaterWalkSounds);
				goto IL_1B5;
			case EObjectMaterial.WoodTree:
				list = (isRunning ? this.m_WoodTreeRunSounds : this.m_WoodTreeWalkSounds);
				goto IL_1B5;
			case EObjectMaterial.Metal:
				list = (isRunning ? this.m_MetalRunSounds : this.m_MetalWalkSounds);
				goto IL_1B5;
			case EObjectMaterial.Tent:
				list = (isRunning ? this.m_TentRunSounds : this.m_TentWalkSounds);
				goto IL_1B5;
			}
			list = (isRunning ? this.m_SandStepRunSounds : this.m_SandStepWalkSounds);
		}
		IL_1B5:
		Noise.Type noise_type = Noise.Type.None;
		if (FPPController.Get().IsWalking())
		{
			noise_type = (FPPController.Get().IsDuck() ? Noise.Type.Sneak : Noise.Type.Walk);
		}
		else if (isRunning)
		{
			noise_type = (FPPController.Get().IsDuck() ? Noise.Type.Sneak : Noise.Type.Run);
		}
		else if (SwimController.Get().IsSwimming())
		{
			noise_type = Noise.Type.Swim;
		}
		if (list == null)
		{
			Debug.Log("ERROR PlayerAudioModule PlayFootstepSound no sounds clips player_pos=" + base.transform.position.ToString() + " Material = " + eobjectMaterial.ToString());
		}
		this.PlayRandomSound(list, 1f, false, noise_type);
		this.m_LastFootstepSound = Time.time;
	}

	public void PlayEatingSound(float volume = 1f, bool loop = false)
	{
		this.PlayGruntSound(PlayerAudioModule.GruntPriority.Eating, null, volume, loop, Noise.Type.None, 0f);
	}

	public void PlayEatingDisgustingSound(float volume = 1f, bool loop = false)
	{
		this.PlayGruntSound(PlayerAudioModule.GruntPriority.Disgusting, null, volume, loop, Noise.Type.None, 0f);
	}

	public void PlayDrinkingSound(float volume = 1f, bool loop = false)
	{
		this.PlayGruntSound(PlayerAudioModule.GruntPriority.Drinking, null, volume, loop, Noise.Type.None, 0f);
	}

	public void PlayDrinkingDisgustingSound(float volume = 1f, bool loop = false)
	{
		this.PlayGruntSound(PlayerAudioModule.GruntPriority.Disgusting, null, volume, loop, Noise.Type.None, 0f);
	}

	public void PlayBreathingSound(float volume = 1f, bool loop = false)
	{
		this.PlayGruntSound(PlayerAudioModule.GruntPriority.Breathing, null, volume, loop, Noise.Type.None, 1f);
	}

	public void PlayVomitingSound(float volume = 1f, bool loop = false)
	{
		this.PlayGruntSound(PlayerAudioModule.GruntPriority.Vomiting, null, volume, loop, Noise.Type.Action, 0f);
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
		this.StopGruntSound(PlayerAudioModule.GruntPriority.Breathing, 0f);
	}

	public void FadeOutBreathingSound()
	{
		this.StopGruntSound(PlayerAudioModule.GruntPriority.Breathing, 0.2f);
	}

	public override void OnTakeDamage(DamageInfo info)
	{
		base.OnTakeDamage(info);
		if (!info.m_Blocked && info.m_PlayDamageSound && Time.time - this.m_LastPlayDamageSound > 2f)
		{
			if (info.m_DamageType == DamageType.Insects)
			{
				this.PlayDamageInsectsSound(1f, false);
			}
			else
			{
				this.PlayDamageSound(1f, false);
			}
			this.m_LastPlayDamageSound = Time.time;
			info.m_InjuryPlace = PlayerInjuryModule.Get().GetInjuryPlaceFromHit(info);
			Limb limb = EnumTools.ConvertInjuryPlaceToLimb(info.m_InjuryPlace);
			if (info.m_DamageType != DamageType.Insects && info.m_DamageType != DamageType.Fall && PlayerArmorModule.Get().IsArmorActive(limb) && !PlayerArmorModule.Get().IsArmorDestroyed(limb))
			{
				this.PlayHitArmorSound(1f, false);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		int num = 0;
		int i = 0;
		while (i < this.m_AudioSources.Count)
		{
			AudioSource audioSource = this.m_AudioSources[i];
			if (audioSource.isVirtual && !audioSource.isPlaying)
			{
				audioSource.Stop();
				audioSource.clip = null;
				UnityEngine.Object.Destroy(audioSource);
				this.m_AudioSources.RemoveAt(i);
				num++;
			}
			else
			{
				i++;
			}
		}
		num = this.m_AudioSourcesCount - this.m_AudioSources.Count;
		if (num > 0)
		{
			for (int j = 0; j < num; j++)
			{
				this.m_AudioSources.Add(this.AddSource(AudioMixerGroupGame.Player));
			}
		}
		this.UpdateBreathStop();
		if (this.m_Player == Player.Get())
		{
			this.UpdateLowStamina();
		}
	}

	private void UpdateBreathStop()
	{
	}

	private void UpdateLowStamina()
	{
		if (PlayerConditionModule.Get().IsLowStamina())
		{
			if (Time.time > this.m_LowStaminaSoundStartTime + 1f && this.PlayGruntSound(PlayerAudioModule.GruntPriority.LowStamina, null, 1f, false, Noise.Type.None, 0f))
			{
				this.m_LowStaminaSoundStartTime = Time.time;
				return;
			}
		}
		else if (this.m_GruntSoundPlayed == PlayerAudioModule.GruntPriority.LowStamina)
		{
			this.StopGruntSound(PlayerAudioModule.GruntPriority.LowStamina, 1f);
		}
	}

	public void PlayItemSound(string sound)
	{
		if (sound.Contains("item_grab_food"))
		{
			this.PlayRandomSound(this.m_item_grab_foodSounds, 1f, false, Noise.Type.None);
			return;
		}
		if (sound.Contains("item_grab_hard"))
		{
			this.PlayRandomSound(this.m_item_grab_hardSounds, 1f, false, Noise.Type.None);
			return;
		}
		if (sound.Contains("item_grab_leaf_herb"))
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
		this.PlayRandomSound(this.m_NotepadEntrySound, this.m_NotepadEntrySounds, 1f, false, Noise.Type.None);
	}

	public void PlayDiveSound()
	{
		foreach (AudioSource audioSource in this.m_DiveSources)
		{
			if (!audioSource.isPlaying)
			{
				this.PlayRandomSound(audioSource, this.m_DiveSounds, 0.5f, false, Noise.Type.None);
				break;
			}
		}
		this.MakeNoise(Noise.Type.Swim);
	}

	public void PlaySwimSound()
	{
		this.PlayRandomSound(this.m_SwimSounds, 1f, false, Noise.Type.None);
		this.MakeNoise(Noise.Type.Swim);
	}

	public void PlayDeathSound()
	{
		this.PlayGruntSound(PlayerAudioModule.GruntPriority.Death, null, 1f, false, Noise.Type.None, 0f);
	}

	public void PlayBeforeDivingSound()
	{
		this.PlayRandomSound(this.m_BeforeDivingSounds, 1f, false, Noise.Type.None);
	}

	public void PlayNoOxygenDivingSounds()
	{
		this.PlayRandomSound(this.m_NoOxygenDivingSounds, 1f, false, Noise.Type.None);
	}

	public void PlayAfterDivingSound()
	{
		this.PlayRandomSound(this.m_InhaleAfterDivingSounds, 1f, false, Noise.Type.None);
	}

	public void PlayUnderwaterDeathSound()
	{
		this.PlayGruntSound(PlayerAudioModule.GruntPriority.Death, this.m_UnderwaterDeathSounds, 1f, false, Noise.Type.None, 0f);
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
		switch (this.m_Being.GetMaterial())
		{
		case EObjectMaterial.Bush:
		case EObjectMaterial.Grass:
			this.PlayRandomSound(this.m_BodyFallGrassSounds, 1f, false, Noise.Type.None);
			return;
		case EObjectMaterial.Stone:
			this.PlayRandomSound(this.m_BodyFallStoneSounds, 1f, false, Noise.Type.None);
			return;
		case EObjectMaterial.DryLeaves:
			this.PlayRandomSound(this.m_BodyFallLeavesSounds, 1f, false, Noise.Type.None);
			return;
		case EObjectMaterial.Mud:
			this.PlayRandomSound(this.m_BodyFallMudSounds, 1f, false, Noise.Type.None);
			return;
		case EObjectMaterial.Sand:
			this.PlayRandomSound(this.m_BodyFallSandSounds, 1f, false, Noise.Type.None);
			return;
		case EObjectMaterial.Flesh:
		case EObjectMaterial.Moss:
			break;
		case EObjectMaterial.Soil:
			this.PlayRandomSound(this.m_BodyFallGrassSounds, 1f, false, Noise.Type.None);
			return;
		case EObjectMaterial.Water:
			this.PlayRandomSound(this.m_BodyFallShalowWaterSounds, 1f, false, Noise.Type.None);
			break;
		default:
			return;
		}
	}

	public void PlayLadderSound()
	{
		this.PlayRandomSound(this.m_LadderSounds, 1f, false, Noise.Type.None);
	}

	public void PlayLeechOutSound()
	{
		this.PlayGruntSound(PlayerAudioModule.GruntPriority.LeechOut, null, 1f, false, Noise.Type.None, 0f);
	}

	public void PlayToolDestroyedSound()
	{
		this.PlayGruntSound(PlayerAudioModule.GruntPriority.ToolDestroyed, null, 1f, false, Noise.Type.None, 0f);
	}

	public void PlayMakeFireSound()
	{
		this.PlayRandomSound(this.m_MakeFireSource, this.m_MakeFireSounds, 1f, false, Noise.Type.None);
	}

	public void StopMakeFireSound()
	{
		if (this.m_MakeFireSource)
		{
			this.m_MakeFireSource.Stop();
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

	public void PlayFallIntoWaterSound()
	{
		this.PlayRandomSound(this.m_FallIntoWaterSounds, 1f, false, Noise.Type.None);
	}

	public void PlayFistsSwingSound()
	{
		this.PlayRandomSound(this.m_FistsSwingSounds, 1f, false, Noise.Type.None);
	}

	public void PlayFistsHitSound()
	{
		this.PlayRandomSound(this.m_FistsHitSounds, 1f, false, Noise.Type.None);
	}

	private void InitializeDreamsAudio()
	{
		AudioClip value = (AudioClip)Resources.Load("Sounds/Dreams/Dream04_telefon_odsluchanie_wiadomosci");
		this.m_AudioClipsMap.Add(AnimEventID.Dream4_WT_Click, value);
		value = (AudioClip)Resources.Load("Sounds/Dreams/Dream04_telefon_ciuchy_podnosi");
		this.m_AudioClipsMap.Add(AnimEventID.Dream4_WT_Raise, value);
		value = (AudioClip)Resources.Load("Sounds/Dreams/Dream04_telefon_ciuchy_opuszcza");
		this.m_AudioClipsMap.Add(AnimEventID.Dream4_WT_Stop, value);
		value = (AudioClip)Resources.Load("Sounds/Dreams/Dream04_telefon_wylacza");
		this.m_AudioClipsMap.Add(AnimEventID.Dream4_WT_TurnOff, value);
		value = (AudioClip)Resources.Load("Sounds/Dreams/Dream04_telefon_ciuchy_wstaje");
		this.m_AudioClipsMap.Add(AnimEventID.Dream4_StandUp, value);
		value = (AudioClip)Resources.Load("Sounds/PreDream/ayahuaska_throw_ingredient");
		this.m_AudioClipsMap.Add(AnimEventID.PreDream_Ayahuasca_Throw_Ingredients, value);
		value = (AudioClip)Resources.Load("Sounds/Story/airport_radio_sfx_switch_channel_1");
		this.m_AudioClipsMap.Add(AnimEventID.AIRPORT_RADIO_SFX_SWITCH_CHANNEL_1, value);
		value = (AudioClip)Resources.Load("Sounds/Story/airport_radio_sfx_switch_channel_2");
		this.m_AudioClipsMap.Add(AnimEventID.AIRPORT_RADIO_SFX_SWITCH_CHANNEL_2, value);
		value = (AudioClip)Resources.Load("Sounds/Story/airport_radio_sfx_punch_radio");
		this.m_AudioClipsMap.Add(AnimEventID.AIRPORT_RADIO_SFX_PUNCH_RADIO, value);
		value = (AudioClip)Resources.Load("Sounds/Story/airport_radio_sfx_punch_desk_and_key_drops");
		this.m_AudioClipsMap.Add(AnimEventID.AIRPORT_RADIO_SFX_PUNCH_DESK_AND_KEY_DROPS, value);
		value = (AudioClip)Resources.Load("Sounds/Story/airport_radio_sfx_chat_button_press");
		this.m_AudioClipsMap.Add(AnimEventID.AIRPORT_RADIO_SFX_CHAT_BUTTON_PRESS, value);
		value = (AudioClip)Resources.Load("Sounds/Story/airport_radio_sfx_chat_button_release");
		this.m_AudioClipsMap.Add(AnimEventID.AIRPORT_RADIO_SFX_CHAT_BUTTON_RELEASE, value);
		value = (AudioClip)Resources.Load("Sounds/Story/end1_sfx_radio_power_down");
		this.m_AudioClipsMap.Add(AnimEventID.END1_SFX_RADIO_POWER_DOWN, value);
	}

	private void PlayAnimEventSound(AnimEventID event_id)
	{
		if (this.m_AudioClipsMap.ContainsKey(event_id))
		{
			AudioClip clip = this.m_AudioClipsMap[event_id];
			AudioSource freeSource = this.GetFreeSource();
			freeSource.clip = clip;
			freeSource.Play();
		}
	}

	public void PlayWashingHandsSound()
	{
		this.PlayRandomSound(this.m_WashHandsSounds, 1f, false, Noise.Type.None);
	}

	public void OnAudioSnapshotChanged(AudioMixerSnapshotGame prev_snapshot, AudioMixerSnapshotGame new_snapshot)
	{
		if (prev_snapshot == AudioMixerSnapshotGame.Underwater)
		{
			foreach (AudioSource audioSource in this.m_DiveSources)
			{
				if (audioSource.isPlaying)
				{
					base.StartCoroutine(AudioFadeOut.FadeOut(audioSource, 0.3f, 0f, null));
				}
			}
		}
	}

	private void SetupWeaponAudio()
	{
		for (int i = 0; i < 16; i++)
		{
			this.m_AudioClipsHit[i] = new List<AudioClip>();
			for (int j = 1; j < 10; j++)
			{
				string str = string.Empty;
				switch (i)
				{
				case 0:
					str = "axe_unknown_0" + j.ToString();
					break;
				case 1:
				case 13:
					str = "axe_wood_0" + j.ToString();
					break;
				case 2:
					str = "axe_bush_0" + j.ToString();
					break;
				case 3:
					str = "axe_stone_0" + j.ToString();
					break;
				case 8:
				case 15:
					str = "axe_flesh_0" + j.ToString();
					break;
				case 12:
					str = "axe_wood_0" + j.ToString();
					break;
				case 14:
					str = "axe_metal_0" + j.ToString();
					break;
				}
				AudioClip audioClip = (AudioClip)Resources.Load("Sounds/Hit/" + str);
				if (!(audioClip != null))
				{
					break;
				}
				this.m_AudioClipsHit[i].Add(audioClip);
			}
		}
		this.m_SwingSoundClipsDict[-1] = new List<AudioClip>();
		AudioClip item = (AudioClip)Resources.Load("Sounds/Weapon/axe_swing_01");
		this.m_SwingSoundClipsDict[-1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_swing_02");
		this.m_SwingSoundClipsDict[-1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_swing_03");
		this.m_SwingSoundClipsDict[-1].Add(item);
		this.m_SwingSoundClipsDict[291] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_swing_01");
		this.m_SwingSoundClipsDict[291].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_swing_02");
		this.m_SwingSoundClipsDict[291].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_swing_03");
		this.m_SwingSoundClipsDict[291].Add(item);
		this.m_SwingSoundClipsDict[312] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_whoosh_01");
		this.m_SwingSoundClipsDict[312].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_whoosh_02");
		this.m_SwingSoundClipsDict[312].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_whoosh_03");
		this.m_SwingSoundClipsDict[312].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_whoosh_04");
		this.m_SwingSoundClipsDict[312].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_whoosh_05");
		this.m_SwingSoundClipsDict[312].Add(item);
		this.m_SwingSoundClipsDict[288] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/stoneblade_swing_01");
		this.m_SwingSoundClipsDict[288].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/stoneblade_swing_02");
		this.m_SwingSoundClipsDict[288].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/stoneblade_swing_03");
		this.m_SwingSoundClipsDict[288].Add(item);
		this.m_SwingSoundClipsDict[308] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_attack_swing_01");
		this.m_SwingSoundClipsDict[308].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_attack_swing_02");
		this.m_SwingSoundClipsDict[308].Add(item);
		this.m_SwingSoundClipsDict[303] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_attack_swing_01");
		this.m_SwingSoundClipsDict[303].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_attack_swing_02");
		this.m_SwingSoundClipsDict[303].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_attack_swing_03");
		this.m_SwingSoundClipsDict[303].Add(item);
		this.m_SwingSoundClipsDict[313] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_whoosh_01");
		this.m_SwingSoundClipsDict[313].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_whoosh_01");
		this.m_SwingSoundClipsDict[313].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_whoosh_01");
		this.m_SwingSoundClipsDict[313].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_whoosh_01");
		this.m_SwingSoundClipsDict[313].Add(item);
	}

	private void SetupHitSpecificSounds()
	{
		this.m_HitSpecificSounds[312] = new Dictionary<int, List<AudioClip>>();
		this.m_HitSpecificSounds[312][1] = new List<AudioClip>();
		AudioClip item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_hit_01");
		this.m_HitSpecificSounds[312][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_hit_02");
		this.m_HitSpecificSounds[312][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_hit_03");
		this.m_HitSpecificSounds[312][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_hit_04");
		this.m_HitSpecificSounds[312][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_hit_05");
		this.m_HitSpecificSounds[312][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_hit_06");
		this.m_HitSpecificSounds[312][1].Add(item);
		this.m_HitSpecificSounds[313] = new Dictionary<int, List<AudioClip>>();
		this.m_HitSpecificSounds[313][2] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_branch_hit_01");
		this.m_HitSpecificSounds[313][2].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_branch_hit_02");
		this.m_HitSpecificSounds[313][2].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_branch_hit_03");
		this.m_HitSpecificSounds[313][2].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_branch_hit_04");
		this.m_HitSpecificSounds[313][2].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_branch_hit_05");
		this.m_HitSpecificSounds[313][2].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_branch_hit_06");
		this.m_HitSpecificSounds[313][2].Add(item);
		this.m_HitSpecificSounds[313][1] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_tree_hit_01");
		this.m_HitSpecificSounds[313][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_tree_hit_02");
		this.m_HitSpecificSounds[313][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_tree_hit_03");
		this.m_HitSpecificSounds[313][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_tree_hit_04");
		this.m_HitSpecificSounds[313][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_tree_hit_05");
		this.m_HitSpecificSounds[313][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_tree_hit_06");
		this.m_HitSpecificSounds[313][1].Add(item);
		this.m_HitSpecificSounds[308] = new Dictionary<int, List<AudioClip>>();
		this.m_HitSpecificSounds[308][8] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_flesh_hit_01");
		this.m_HitSpecificSounds[308][8].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_flesh_hit_02");
		this.m_HitSpecificSounds[308][8].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_flesh_hit_03");
		this.m_HitSpecificSounds[308][8].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_flesh_hit_04");
		this.m_HitSpecificSounds[308][8].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_flesh_hit_05");
		this.m_HitSpecificSounds[308][8].Add(item);
		this.m_HitSpecificSounds[307] = this.m_HitSpecificSounds[308];
		this.m_HitSpecificSounds[306] = this.m_HitSpecificSounds[308];
		this.m_HitSpecificSounds[305] = this.m_HitSpecificSounds[308];
		this.m_HitSpecificSounds[303] = this.m_HitSpecificSounds[308];
		this.m_HitSpecificSounds[327] = this.m_HitSpecificSounds[308];
	}

	public void MakeHitSound(GameObject obj, ItemID item_id)
	{
		ObjectMaterial component = obj.GetComponent<ObjectMaterial>();
		this.MakeHitSound((component != null) ? component.m_ObjectMaterial : EObjectMaterial.Unknown, item_id);
	}

	public void MakeHitSound(EObjectMaterial material, ItemID item_id)
	{
		AudioClip specificSound = this.GetSpecificSound(item_id, material);
		if (specificSound)
		{
			this.PlaySound(specificSound, 1f, false, Noise.Type.None);
			return;
		}
		if (this.m_AudioClipsHit[(int)material].Count > 0)
		{
			int index = UnityEngine.Random.Range(0, this.m_AudioClipsHit[(int)material].Count);
			AudioClip clip = this.m_AudioClipsHit[(int)material][index];
			this.PlaySound(clip, 1f, false, Noise.Type.None);
		}
		else
		{
			int index2 = UnityEngine.Random.Range(0, this.m_AudioClipsHit[0].Count);
			AudioClip clip2 = this.m_AudioClipsHit[0][index2];
			this.PlaySound(clip2, 1f, false, Noise.Type.None);
		}
		if (this.ReplIsOwner())
		{
			this.GetPlayerComponent<ReplicatedPlayerSounds>().ReplicateSound(new ReplicatedPlayerSounds.SSoundData
			{
				type = ReplicatedPlayerSounds.EReplicatedSoundType.Hit,
				material = material,
				item_id = item_id
			});
		}
	}

	public void PlaySwingSound(ItemID item_id)
	{
		List<AudioClip> list = null;
		if (this.m_SwingSoundClipsDict.TryGetValue((int)item_id, out list))
		{
			this.PlaySound(list[UnityEngine.Random.Range(0, list.Count)], 1f, false, Noise.Type.None);
		}
		else if (this.m_SwingSoundClipsDict.TryGetValue(-1, out list))
		{
			this.PlaySound(list[UnityEngine.Random.Range(0, list.Count)], 1f, false, Noise.Type.None);
		}
		if (this.ReplIsOwner())
		{
			this.GetPlayerComponent<ReplicatedPlayerSounds>().ReplicateSound(new ReplicatedPlayerSounds.SSoundData
			{
				type = ReplicatedPlayerSounds.EReplicatedSoundType.Swing,
				item_id = item_id
			});
		}
	}

	private AudioClip GetSpecificSound(ItemID item_id, EObjectMaterial material)
	{
		Dictionary<int, List<AudioClip>> dictionary;
		if (!this.m_HitSpecificSounds.TryGetValue((int)item_id, out dictionary))
		{
			return null;
		}
		List<AudioClip> list;
		if (!dictionary.TryGetValue((int)material, out list))
		{
			return null;
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	private int m_AudioSourcesCount = 15;

	private List<AudioSource> m_AudioSources;

	private NoiseManager m_NoiseManager;

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
	public List<AudioClip> m_HitArmorSounds;

	[HideInInspector]
	public List<AudioClip> m_UseSounds;

	[HideInInspector]
	public List<AudioClip> m_AttackSounds;

	[HideInInspector]
	public List<AudioClip> m_DamageSounds;

	[HideInInspector]
	public List<AudioClip> m_DamageInsectsSounds;

	[HideInInspector]
	public List<AudioClip> m_BreathSounds;

	[HideInInspector]
	public List<AudioClip> m_BreathLowStaminaSounds;

	[HideInInspector]
	public List<AudioClip> m_LandingSounds;

	[HideInInspector]
	public List<AudioClip> m_LandingWaterSounds;

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
	public List<AudioClip> m_WoodWalkSounds;

	[HideInInspector]
	public List<AudioClip> m_MetalWalkSounds;

	[HideInInspector]
	public List<AudioClip> m_TentWalkSounds;

	[HideInInspector]
	public List<AudioClip> m_WoodTreeWalkSounds;

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
	public List<AudioClip> m_WoodRunSounds;

	[HideInInspector]
	public List<AudioClip> m_MetalRunSounds;

	[HideInInspector]
	public List<AudioClip> m_TentRunSounds;

	[HideInInspector]
	public List<AudioClip> m_WoodTreeRunSounds;

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
	public List<AudioClip> m_WoodJumpSounds;

	[HideInInspector]
	public List<AudioClip> m_MetalJumpSounds;

	[HideInInspector]
	public List<AudioClip> m_TentJumpSounds;

	[HideInInspector]
	public List<AudioClip> m_WoodTreeJumpSounds;

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
	public List<AudioClip> m_WoodLandingSounds;

	[HideInInspector]
	public List<AudioClip> m_WoodTreeLandingSounds;

	[HideInInspector]
	public List<AudioClip> m_MetalLandingSounds;

	[HideInInspector]
	public List<AudioClip> m_TentLandingSounds;

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
	public List<AudioClip> m_NoOxygenDivingSounds;

	[HideInInspector]
	public List<AudioClip> m_InhaleAfterDivingSounds;

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

	[HideInInspector]
	public List<AudioClip> m_FallIntoWaterSounds;

	[HideInInspector]
	public List<AudioClip> m_FistsSwingSounds;

	[HideInInspector]
	public List<AudioClip> m_FistsHitSounds;

	[HideInInspector]
	public List<AudioClip> m_WashHandsSounds;

	[HideInInspector]
	public AudioSource m_SleepSoundSource;

	private AudioSource m_NotepadEntrySound;

	private static PlayerAudioModule s_Instance;

	private AudioSource m_MakeFireSource;

	private List<AudioSource> m_DiveSources = new List<AudioSource>(3);

	private bool m_SourcesInited;

	private int m_MoveBlendTreeHash = Animator.StringToHash("Idle");

	private AudioSource m_GruntSoundSource;

	private Coroutine m_GruntFadeinCoroutine;

	private Coroutine m_GruntFadeoutCoroutine;

	private AudioSource m_HeartBeatSoundAS;

	private float m_LastFootstepSound = float.MinValue;

	private float m_LastPlayDamageSound = float.MinValue;

	private float m_LowStaminaSoundStartTime = float.MinValue;

	private const float m_LowStaminaSoundCooldown = 1f;

	private Dictionary<AnimEventID, AudioClip> m_AudioClipsMap = new Dictionary<AnimEventID, AudioClip>();

	private Dictionary<int, Dictionary<int, List<AudioClip>>> m_HitSpecificSounds = new Dictionary<int, Dictionary<int, List<AudioClip>>>();

	private Dictionary<int, List<AudioClip>> m_SwingSoundClipsDict = new Dictionary<int, List<AudioClip>>();

	private Dictionary<int, List<AudioClip>> m_AudioClipsHit = new Dictionary<int, List<AudioClip>>();

	public enum GruntPriority
	{
		None = -1,
		Breathing,
		LowStamina,
		Jump,
		Landing,
		Attack,
		ToolDestroyed,
		LeechOut,
		Drinking,
		Eating,
		Disgusting,
		DamageInsects,
		HitReaction,
		Vomiting,
		PassOut,
		Death
	}
}
