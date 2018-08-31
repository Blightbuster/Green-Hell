using System;
using AIs;
using Enums;
using UnityEngine;

public class DeathController : PlayerController
{
	public static DeathController Get()
	{
		return DeathController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		DeathController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.Death;
		this.m_ConditionModule = this.m_Player.GetComponent<PlayerConditionModule>();
		this.m_InjuryModule = this.m_Player.GetComponent<PlayerInjuryModule>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (this.m_Player.GetComponent<PlayerConditionModule>().IsOxygenCriticalLevel())
		{
			this.m_Animator.SetBool(this.m_IsDeadUnderwaterHash, true);
		}
		else
		{
			this.m_Animator.SetBool(this.m_IsDeadHash, true);
		}
		DialogsManager.Get().StopDialog();
		CutscenesManager.Get().StopCutscene();
		AIManager.Get().OnPlayerDie();
		if (this.m_Player.ShouldSwim() && this.m_Player.GetComponent<SwimController>().m_State == SwimState.Dive)
		{
			PlayerAudioModule.Get().PlayUnderwaterDeathSound();
		}
		else
		{
			PlayerAudioModule.Get().PlayDeathSound();
		}
		this.SetState(DeathController.DeathState.Dying);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetBool(this.m_IsDeadHash, false);
		this.m_Animator.SetBool(this.m_IsDeadUnderwaterHash, false);
		this.SetState(DeathController.DeathState.None);
	}

	public bool IsState(DeathController.DeathState state)
	{
		return this.m_State == state;
	}

	private void SetState(DeathController.DeathState state)
	{
		if (this.m_State == state)
		{
			return;
		}
		this.m_State = state;
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		if (id == AnimEventID.DeathEnd)
		{
			this.SetState(DeathController.DeathState.Death);
			if (ChallengesManager.Get().IsChallengeActive())
			{
				ChallengesManager.Get().FailChallenge();
			}
		}
		else if (id == AnimEventID.DeathFall)
		{
			PlayerAudioModule.Get().PlayBodyFallSound();
		}
		else
		{
			base.OnAnimEvent(id);
		}
	}

	public void StartRespawn()
	{
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeOut(FadeType.All, new VDelegate(this.Respawn), 1.5f, null);
	}

	public void Respawn()
	{
		this.m_InjuryModule.ResetInjuries();
		this.m_ConditionModule.ResetParams();
		this.m_Player.GetComponent<SleepController>().UpdateLastWakeUpTime();
		MainLevel.Instance.ResetGameBeforeLoad();
		SaveGame.Load();
		this.m_Animator.SetBool(this.m_IsDeadHash, false);
		this.m_Animator.SetBool(this.m_IsDeadUnderwaterHash, false);
		this.Stop();
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeIn(FadeType.All, null, 1.5f);
	}

	private PlayerConditionModule m_ConditionModule;

	private PlayerInjuryModule m_InjuryModule;

	private int m_IsDeadHash = Animator.StringToHash("IsDead");

	private int m_IsDeadUnderwaterHash = Animator.StringToHash("IsDeadUnderwater");

	private DeathController.DeathState m_State;

	private static DeathController s_Instance;

	public enum DeathState
	{
		None,
		Dying,
		Death
	}
}
