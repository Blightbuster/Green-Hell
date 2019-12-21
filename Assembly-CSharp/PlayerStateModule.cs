using System;
using System.Collections.Generic;
using AIs;
using UnityEngine;

public class PlayerStateModule : PlayerModule
{
	public static PlayerStateModule Get()
	{
		return PlayerStateModule.s_Instance;
	}

	private void Awake()
	{
		PlayerStateModule.s_Instance = this;
	}

	public override void Initialize(Being being)
	{
		base.Initialize(being);
	}

	private void SetState(PlayerStateModule.State state)
	{
		if (this.m_State == state)
		{
			return;
		}
		this.OnExitState();
		this.m_State = state;
		this.OnEnterState();
	}

	private void OnExitState()
	{
		switch (this.m_State)
		{
		case PlayerStateModule.State.Calm:
			this.OnExitCalmState();
			return;
		case PlayerStateModule.State.Hunting:
			this.OnExitHuntingState();
			return;
		case PlayerStateModule.State.Combat:
			this.OnExitCombatState();
			return;
		default:
			return;
		}
	}

	private void OnExitCalmState()
	{
	}

	private void OnExitCombatState()
	{
		Music.Get().FadeOut(0f, 2f, 0);
	}

	private void OnExitHuntingState()
	{
		Music.Get().FadeOut(0f, 2f, 0);
	}

	private void OnEnterState()
	{
		this.m_EnterStateTime = Time.time;
		switch (this.m_State)
		{
		case PlayerStateModule.State.Calm:
			this.OnEnterCalmState();
			return;
		case PlayerStateModule.State.Hunting:
			this.OnEnterHuntingState();
			return;
		case PlayerStateModule.State.Combat:
			this.OnEnterCombatState();
			return;
		default:
			return;
		}
	}

	private void OnEnterCalmState()
	{
	}

	private void OnEnterCombatState()
	{
		this.m_EnemyInvisibleDuration = 0f;
		Music.Get().Play(this.m_CombatMusicList[UnityEngine.Random.Range(0, this.m_CombatMusicList.Count)], 0f, true, 0);
		Music.Get().FadeIn(0.7f, 1f, 0);
	}

	private void OnEnterHuntingState()
	{
		Music.Get().Play(this.m_HuntingMusicList[UnityEngine.Random.Range(0, this.m_HuntingMusicList.Count)], 0f, true, 0);
		Music.Get().FadeIn(0.7f, 1f, 0);
	}

	public override void Update()
	{
		base.Update();
		this.UpdateEnemyVisibleDuration();
		this.UpdateState();
	}

	private void UpdateEnemyVisibleDuration()
	{
		bool flag = false;
		foreach (AI ai in AIManager.Get().m_EnemyAIs)
		{
			if (!ai.IsDead() && ai.IsVisible() && ai.transform.position.Distance(base.transform.position) < 10f)
			{
				flag = true;
			}
		}
		if (flag)
		{
			this.m_EnemyVisibleDuration += Time.deltaTime;
			this.m_EnemyInvisibleDuration = 0f;
			return;
		}
		this.m_EnemyVisibleDuration = 0f;
		this.m_EnemyInvisibleDuration += Time.deltaTime;
	}

	private bool ShouldSetCombatState()
	{
		if (this.m_EnemyVisibleDuration > 2f)
		{
			return true;
		}
		foreach (AI ai in AIManager.Get().m_EnemyAIs)
		{
			if (ai.IsHunter())
			{
				HunterAI hunterAI = (HunterAI)ai;
				if (hunterAI.m_EnemyModule.m_Enemy && hunterAI.transform.position.Distance(Player.Get().transform.position) <= hunterAI.m_MaxBowDistance)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool ShouldSetHuntingState()
	{
		return this.m_HuntingTarget != null && !this.m_HuntingTarget.IsDead();
	}

	private void UpdateState()
	{
		switch (this.m_State)
		{
		case PlayerStateModule.State.Calm:
			this.UpdateCalmState();
			return;
		case PlayerStateModule.State.Hunting:
			this.UpdateHuntingState();
			return;
		case PlayerStateModule.State.Combat:
			this.UpdateCombatState();
			return;
		default:
			return;
		}
	}

	private void UpdateCalmState()
	{
		if (Time.time - this.m_EnterStateTime < 5f)
		{
			return;
		}
		if (this.ShouldSetCombatState())
		{
			this.SetState(PlayerStateModule.State.Combat);
			return;
		}
		if (this.ShouldSetHuntingState())
		{
			this.SetState(PlayerStateModule.State.Hunting);
		}
	}

	private void UpdateCombatState()
	{
		bool flag = false;
		foreach (AI ai in AIManager.Get().m_EnemyAIs)
		{
			if (!(ai == null) && ai.gameObject.activeInHierarchy)
			{
				if (ai.IsHunter() && !ai.IsDead() && ai.transform.position.Distance(this.m_Player.GetWorldPosition()) < 30f)
				{
					return;
				}
				if (!ai.IsDead())
				{
					flag = true;
				}
			}
		}
		if (!flag)
		{
			this.SetState(PlayerStateModule.State.Calm);
			return;
		}
		if (this.m_EnemyInvisibleDuration > 10f)
		{
			this.SetState(PlayerStateModule.State.Calm);
		}
	}

	private void UpdateHuntingState()
	{
		if (!this.m_HuntingTarget)
		{
			this.SetState(PlayerStateModule.State.Calm);
			return;
		}
		if (Time.time - this.m_LastDamageTime > 10f && (this.m_HuntingTarget.m_InvisibleDuration > 5f || this.m_HuntingTarget.IsDead() || !this.m_HuntingTarget.gameObject.activeSelf))
		{
			this.SetState(PlayerStateModule.State.Calm);
			return;
		}
	}

	public override void OnTakeDamage(DamageInfo info)
	{
		base.OnTakeDamage(info);
		if (!info.m_Damager)
		{
			return;
		}
		AI component = info.m_Damager.GetComponent<AI>();
		if (!component)
		{
			return;
		}
		if (component.IsEnemy())
		{
			this.SetState(PlayerStateModule.State.Combat);
		}
	}

	public bool ScenarioIsCombatState()
	{
		return this.m_State == PlayerStateModule.State.Combat;
	}

	public void ScenarioSetCombatState(bool set)
	{
		if (set)
		{
			this.SetState(PlayerStateModule.State.Combat);
			return;
		}
		this.SetState(PlayerStateModule.State.Calm);
	}

	public void OnGiveDamageToAI(AI ai, DamageInfo info)
	{
		if (this.m_State == PlayerStateModule.State.Combat)
		{
			return;
		}
		if (!ai)
		{
			return;
		}
		if (ai.IsDead())
		{
			return;
		}
		if (!AI.IsHuntingTarget(ai.m_ID))
		{
			return;
		}
		this.m_LastDamageTime = Time.time;
		this.m_HuntingTarget = ai;
		this.SetState(PlayerStateModule.State.Hunting);
	}

	[HideInInspector]
	public PlayerStateModule.State m_State;

	private float m_EnemyVisibleDuration;

	private float m_EnemyInvisibleDuration;

	public List<AudioClip> m_CombatMusicList;

	public List<AudioClip> m_HuntingMusicList;

	private AI m_HuntingTarget;

	private float m_EnterStateTime;

	private float m_LastDamageTime;

	private static PlayerStateModule s_Instance;

	public enum State
	{
		Calm,
		Hunting,
		Combat
	}
}
