using System;
using AIs;
using CJTools;
using Enums;
using UnityEngine;

public class FistFightController : FightController
{
	public static FistFightController Get()
	{
		return FistFightController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		FistFightController.s_Instance = this;
		base.m_ControllerType = PlayerControllerType.FistFight;
		this.m_RightHand = base.transform.FindDeepChild("RHolder");
		this.m_LeftHand = base.transform.FindDeepChild("LHolder");
		this.SetAttackParam(0);
		this.m_CurrentPhase = FistFightController.m_FightLeftPunchValue;
		PlayerFist playerFist = this.m_LeftHand.gameObject.AddComponent<PlayerFist>();
		this.m_LeftHandCollider = playerFist.m_HandCollider;
		PlayerFist playerFist2 = this.m_RightHand.gameObject.AddComponent<PlayerFist>();
		this.m_RightHandCollider = playerFist2.m_HandCollider;
		this.m_Wait = new WaitForEndOfFrame();
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		if (this.m_Animator.GetInteger(this.m_FFAttack) == 1)
		{
			this.m_Time += Time.deltaTime;
			if (this.m_Time >= this.m_ReleaseTime && !base.IsBlock())
			{
				this.SetAttackParam(0);
				this.m_LeftHandCollider.enabled = false;
				this.m_RightHandCollider.enabled = false;
				this.m_Time = 0f;
			}
		}
	}

	public override void SetBlock(bool set)
	{
		if (set && !this.CanBlock())
		{
			return;
		}
		base.SetBlock(set);
		this.SetAttackParam(1);
		this.m_ActionAllowed = true;
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.FistFightPunchEnd)
		{
			this.PlayerFightPunchAttackEnd(FistFightController.Mode.Normal);
			return;
		}
		if (id == AnimEventID.FistFightPunchHardEnd)
		{
			this.PlayerFightPunchAttackEnd(FistFightController.Mode.Hard);
			return;
		}
		if (id == AnimEventID.FistFightPunchStart)
		{
			this.EnableCollider();
			PlayerAudioModule.Get().PlayAttackSound(1f, false);
			PlayerAudioModule.Get().PlayFistsSwingSound();
		}
	}

	private bool CanAttack()
	{
		return !FightController.s_BlockFight && !MainLevel.Instance.IsPause() && Time.time - MainLevel.Instance.m_LastUnpauseTime >= 0.25f && !this.m_Player.IsDead() && !SwimController.Get().IsActive() && !HUDWheel.Get().enabled && !BodyInspectionController.Get().IsActive() && !WatchController.Get().IsActive() && !NotepadController.Get().IsActive() && !MapController.Get().IsActive() && this.m_ActionAllowed && !this.m_Player.GetRotationBlocked() && !Inventory3DManager.Get().gameObject.activeSelf && !HitReactionController.Get().IsActive() && !base.IsBlock() && this.m_CurrentParam != 6 && this.m_CurrentParam != 8 && !ScenarioManager.Get().IsBoolVariableTrue("PlayerMechGameEnding") && !HUDSelectDialog.Get().enabled;
	}

	public override bool IsAttack()
	{
		return this.m_Animator.GetInteger(this.m_FFAttack) > 1;
	}

	public void GiveDamage(AI ai)
	{
		if (this.m_CurrentMode == FistFightController.Mode.Normal)
		{
			this.m_DamageInfo.m_Damage = Player.Get().GetParams().m_FistFightNormalDamage * Skill.Get<FistsSkill>().GetDamageMul();
		}
		else
		{
			this.m_DamageInfo.m_Damage = Player.Get().GetParams().m_FistFightHardDamage * Skill.Get<FistsSkill>().GetDamageMul();
		}
		this.m_DamageInfo.m_Damager = base.gameObject;
		this.m_DamageInfo.m_HitDir = base.transform.forward;
		this.m_DamageInfo.m_Position = (this.m_LeftHandCollider.enabled ? this.m_LeftHandCollider.bounds.center : this.m_RightHandCollider.bounds.center);
		if (ai.TakeDamage(this.m_DamageInfo))
		{
			PlayerAudioModule.Get().PlayFistsHitSound();
		}
		this.m_LeftHandCollider.enabled = false;
		this.m_RightHandCollider.enabled = false;
		bool flag = true;
		if (ai && ai.m_ID == AI.AIID.ArmadilloThreeBanded && ai.m_GoalsModule != null && ai.m_GoalsModule.m_ActiveGoal != null && ai.m_GoalsModule.m_ActiveGoal.m_Type == AIGoalType.Hide)
		{
			flag = false;
		}
		if (flag)
		{
			Skill.Get<FistsSkill>().OnSkillAction();
		}
	}

	private void Attack(FistFightController.Mode mode)
	{
		this.m_CurrentMode = mode;
		this.Attack();
	}

	protected override void Attack()
	{
		base.Attack();
		if (WalkieTalkieController.Get().IsActive())
		{
			WalkieTalkieController.Get().Stop();
		}
		if (PlayerConditionModule.Get().IsStaminaCriticalLevel())
		{
			this.SetAttackParam(this.m_CurrentPhase + 4);
		}
		else if (this.m_CurrentMode == FistFightController.Mode.Normal)
		{
			this.SetAttackParam(this.m_CurrentPhase);
		}
		else
		{
			this.SetAttackParam(this.m_CurrentPhase + 1);
		}
		this.m_ActionAllowed = false;
		base.Invoke("AllowAction", 0.25f);
	}

	private void AllowAction()
	{
		this.m_ActionAllowed = true;
	}

	public void PlayerFightPunchAttackEnd(FistFightController.Mode mode)
	{
		if (this.m_CurrentPhase == FistFightController.m_FightLeftPunchValue)
		{
			this.m_CurrentPhase = FistFightController.m_FightRightPunchValue;
		}
		else if (this.m_CurrentPhase == FistFightController.m_FightRightPunchValue)
		{
			this.m_CurrentPhase = FistFightController.m_FightLeftPunchValue;
		}
		this.m_Time = 0f;
		this.m_LeftHandCollider.enabled = false;
		this.m_RightHandCollider.enabled = false;
		PlayerConditionModule.Get().DecreaseStamina(StaminaDecreaseReason.Attack, this.GetStaminaConsumptionMul(mode));
		if (this.m_ClickBuffer > 0 && this.CanAttack())
		{
			this.Attack((this.m_ClickBuffer == 1) ? FistFightController.Mode.Normal : FistFightController.Mode.Hard);
		}
		else
		{
			this.SetAttackParam(FistFightController.m_FightIdleValue);
		}
		this.m_ClickBuffer = 0;
		this.m_ActionAllowed = true;
	}

	public override void OnInputAction(InputActionData action_data)
	{
		if (action_data.m_Action != InputsManager.InputAction.FistFight && action_data.m_Action != InputsManager.InputAction.FistFightHard)
		{
			return;
		}
		if (!this.CanAttack())
		{
			return;
		}
		if (action_data.m_Action != InputsManager.InputAction.FistFight)
		{
			if (action_data.m_Action == InputsManager.InputAction.FistFightHard)
			{
				if (this.IsAttack())
				{
					this.m_ClickBuffer = 2;
					return;
				}
				this.Attack(FistFightController.Mode.Hard);
			}
			return;
		}
		if (this.IsAttack())
		{
			this.m_ClickBuffer = 1;
			return;
		}
		this.Attack(FistFightController.Mode.Normal);
	}

	private float GetStaminaConsumptionMul(FistFightController.Mode mode)
	{
		if (mode == FistFightController.Mode.Cancelled)
		{
			return 0f;
		}
		if (mode == FistFightController.Mode.Normal)
		{
			return this.m_StaminaNormal;
		}
		return this.m_StaminaHard;
	}

	private void EnableCollider()
	{
		if (this.m_CurrentPhase == 2)
		{
			this.m_LeftHandCollider.enabled = true;
			return;
		}
		this.m_RightHandCollider.enabled = true;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.SetBlock(false);
		this.SetAttackParam(0);
		this.m_ActionAllowed = true;
		if (this.m_LeftHandCollider)
		{
			this.m_LeftHandCollider.enabled = false;
		}
		if (this.m_RightHandCollider)
		{
			this.m_RightHandCollider.enabled = false;
		}
	}

	private void SetAttackParam(int param)
	{
		if (!this.m_Animator.isInitialized)
		{
			return;
		}
		this.m_Animator.SetInteger(this.m_FFAttack, param);
		this.m_CurrentParam = param;
	}

	public bool IsLeftPunch()
	{
		return this.IsAttack() && this.m_CurrentPhase == FistFightController.m_FightLeftPunchValue;
	}

	public override void OnTakeDamage(DamageInfo info)
	{
		base.OnTakeDamage(info);
		if (!info.m_FromInjury)
		{
			this.PlayerFightPunchAttackEnd(FistFightController.Mode.Cancelled);
		}
	}

	private static FistFightController s_Instance = null;

	private bool m_ActionAllowed = true;

	private Transform m_RightHand;

	private Transform m_LeftHand;

	private int m_FFAttack = Animator.StringToHash("FistFight");

	private int m_FightLeftPunch = Animator.StringToHash("FightLeftPunch");

	private int m_FightLeftPunchHard = Animator.StringToHash("FightLeftPunchHard");

	private int m_FightRightPunch = Animator.StringToHash("FightRightPunch");

	private int m_FightRightPunchHard = Animator.StringToHash("FightRightPunchHard");

	private int m_UnarmedIdle = Animator.StringToHash("Unarmed_Idle");

	private static int m_FightIdleValue = 1;

	private static int m_FightLeftPunchValue = 2;

	private static int m_FightRightPunchValue = 4;

	[HideInInspector]
	public int m_CurrentValue;

	private int m_CurrentPhase;

	private int m_ClickBuffer;

	private float m_Time;

	private float m_ReleaseTime = 1.5f;

	private float m_StaminaNormal = 0.4f;

	private float m_StaminaHard = 1f;

	public Collider m_LeftHandCollider;

	public Collider m_RightHandCollider;

	private WaitForEndOfFrame m_Wait;

	private FistFightController.Mode m_CurrentMode = FistFightController.Mode.None;

	private int m_CurrentParam;

	public float m_ShakePower;

	public float m_ShakeSpeed;

	public float m_ShakeDuration;

	private DamageInfo m_DamageInfo = new DamageInfo();

	public enum Mode
	{
		None = -1,
		Normal,
		Hard,
		Cancelled
	}
}
