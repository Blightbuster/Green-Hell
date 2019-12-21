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
		base.m_ControllerType = PlayerControllerType.Death;
		this.m_ConditionModule = this.m_Player.GetComponent<PlayerConditionModule>();
		this.m_InjuryModule = this.m_Player.GetComponent<PlayerInjuryModule>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Animator.SetInteger(this.m_DeathTypeHash, (int)this.m_DeathType);
		DialogsManager.Get().StopDialog();
		CutscenesManager.Get().StopCutscene();
		AIManager.Get().OnPlayerDie();
		if (this.m_DeathType == DeathController.DeathType.UnderWater)
		{
			PlayerAudioModule.Get().PlayUnderwaterDeathSound();
		}
		else
		{
			PlayerAudioModule.Get().PlayDeathSound();
		}
		this.SetState(DeathController.DeathState.Dying);
		this.m_DeathTime = new float?(Time.time);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.ResetDeathType();
		this.SetState(DeathController.DeathState.None);
		this.m_DeathType = DeathController.DeathType.Normal;
		this.m_DeathTime = null;
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
				return;
			}
		}
		else
		{
			if (id == AnimEventID.DeathFall)
			{
				PlayerAudioModule.Get().PlayBodyFallSound();
				return;
			}
			base.OnAnimEvent(id);
		}
	}

	public void StartRespawn()
	{
		GreenHellGame.GetFadeSystem().FadeOut(FadeType.All, new VDelegate(this.Respawn), 1.5f, null);
	}

	public void Respawn()
	{
		this.m_InjuryModule.ResetInjuries();
		this.m_ConditionModule.ResetParams();
		if (ReplTools.IsPlayingAlone())
		{
			SaveGame.Load();
		}
		else
		{
			this.DropInventory();
			this.SpawnOnLastSavePoint();
		}
		this.m_DeathTime = null;
		this.ResetDeathType();
		this.Stop();
		GreenHellGame.GetFadeSystem().FadeIn(FadeType.All, null, 1.5f);
	}

	public void ResetDeathType()
	{
		this.m_Animator.SetInteger(this.m_DeathTypeHash, 0);
	}

	private void DropInventory()
	{
		InventoryBackpack.Get().DropAllItems();
	}

	private void SpawnOnLastSavePoint()
	{
		DebugUtils.Assert(Player.Get().m_RespawnPosition != Vector3.zero, true);
		Player.Get().Reposition(Player.Get().m_RespawnPosition, null);
	}

	[HideInInspector]
	public DeathController.DeathType m_DeathType = DeathController.DeathType.Normal;

	private PlayerConditionModule m_ConditionModule;

	private PlayerInjuryModule m_InjuryModule;

	private int m_DeathTypeHash = Animator.StringToHash("DeathType");

	private DeathController.DeathState m_State;

	private float? m_DeathTime;

	private static DeathController s_Instance;

	public enum DeathState
	{
		None,
		Dying,
		Death
	}

	public enum DeathType
	{
		Normal = 1,
		UnderWater,
		Caiman,
		Predator,
		Cut,
		Fall,
		Insects,
		Melee,
		Poison,
		Thrust,
		Infection,
		Piranha,
		OnWater
	}
}
