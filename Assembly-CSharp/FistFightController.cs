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
		this.m_ControllerType = PlayerControllerType.FistFight;
		this.m_RightHand = base.transform.FindDeepChild("RHolder");
		this.m_LeftHand = base.transform.FindDeepChild("LHolder");
		this.SetAttackParam(0);
		this.m_CurrentPhase = FistFightController.m_FightLeftPunchValue;
		SphereCollider sphereCollider = this.m_LeftHand.gameObject.AddComponent<SphereCollider>();
		sphereCollider.radius = 0.4f;
		sphereCollider.center = Vector3.zero;
		sphereCollider.isTrigger = true;
		sphereCollider.enabled = false;
		this.m_LeftHandCollider = sphereCollider;
		SphereCollider sphereCollider2 = this.m_RightHand.gameObject.AddComponent<SphereCollider>();
		sphereCollider2.radius = 0.4f;
		sphereCollider2.center = Vector3.zero;
		sphereCollider2.isTrigger = true;
		sphereCollider2.enabled = false;
		this.m_RightHandCollider = sphereCollider2;
		this.m_Wait = new WaitForEndOfFrame();
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		if (this.m_Animator.GetInteger(this.m_FFAttack) == 1)
		{
			this.m_Time += Time.deltaTime;
			if (this.m_Time >= this.m_ReleaseTime)
			{
				this.SetAttackParam(0);
				this.m_LeftHandCollider.enabled = false;
				this.m_RightHandCollider.enabled = false;
				this.m_Time = 0f;
			}
		}
		if ((this.m_CurrentParam == 6 || this.m_CurrentParam == 8) && this.m_Animator.GetCurrentAnimatorStateInfo(1).shortNameHash == this.m_UnarmedIdle)
		{
			this.m_ActionAllowed = true;
			this.SetAttackParam(0);
		}
	}

	public override void SetBlock(bool set)
	{
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
		}
		else if (id == AnimEventID.FistFightPunchHardEnd)
		{
			this.PlayerFightPunchAttackEnd(FistFightController.Mode.Hard);
		}
		else if (id == AnimEventID.FistFightPunchStart)
		{
			this.EnableCollider();
		}
	}

	private bool CanAttack()
	{
		return !MainLevel.Instance.IsPause() && !this.m_Player.IsDead() && !SwimController.Get().IsActive() && !HUDWheel.Get().enabled && !BodyInspectionController.Get().IsActive() && !WatchController.Get().IsActive() && !NotepadController.Get().IsActive() && !MapController.Get().IsActive() && this.m_ActionAllowed && !this.m_Player.GetRotationBlocked() && !Inventory3DManager.Get().gameObject.activeSelf && !HitReactionController.Get().IsActive() && !base.IsBlock() && this.m_CurrentParam != 6 && this.m_CurrentParam != 8;
	}

	public override bool IsAttack()
	{
		return this.m_Animator.GetInteger(this.m_FFAttack) > 1;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (this.m_DamageWindow)
		{
			return;
		}
		AI component = other.gameObject.GetComponent<AI>();
		if (component == null)
		{
			return;
		}
		DamageInfo damageInfo = new DamageInfo();
		if (this.m_CurrentMode == FistFightController.Mode.Normal)
		{
			damageInfo.m_Damage = Player.Get().GetParams().m_FistFightNormalDamage * Skill.Get<FistsSkill>().GetDamageMul();
		}
		else
		{
			damageInfo.m_Damage = Player.Get().GetParams().m_FistFightHardDamage * Skill.Get<FistsSkill>().GetDamageMul();
		}
		damageInfo.m_Damager = base.gameObject;
		damageInfo.m_HitDir = base.transform.forward;
		damageInfo.m_Position = ((!this.m_LeftHandCollider.enabled) ? this.m_RightHandCollider.bounds.center : this.m_LeftHandCollider.bounds.center);
		bool flag = component.TakeDamage(damageInfo);
		if (flag)
		{
			PlayerAudioModule.Get().PlayHitSound(1f, false);
		}
		this.m_LeftHandCollider.enabled = false;
		this.m_RightHandCollider.enabled = false;
		Skill.Get<FistsSkill>().OnSkillAction();
	}

	private void Attack(FistFightController.Mode mode)
	{
		this.m_CurrentMode = mode;
		this.Attack();
	}

	protected override void Attack()
	{
		base.Attack();
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
			this.Attack((this.m_ClickBuffer != 1) ? FistFightController.Mode.Hard : FistFightController.Mode.Normal);
		}
		else
		{
			this.SetAttackParam(FistFightController.m_FightIdleValue);
		}
		this.m_ClickBuffer = 0;
		this.m_ActionAllowed = true;
	}

	public override void OnInputAction(InputsManager.InputAction action)
	{
		if (!this.CanAttack())
		{
			return;
		}
		if (action == InputsManager.InputAction.FistFight)
		{
			if (this.IsAttack())
			{
				this.m_ClickBuffer = 1;
			}
			else
			{
				this.Attack(FistFightController.Mode.Normal);
			}
		}
		else if (action == InputsManager.InputAction.FistFightHard)
		{
			if (this.IsAttack())
			{
				this.m_ClickBuffer = 2;
			}
			else
			{
				this.Attack(FistFightController.Mode.Hard);
			}
		}
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
		}
		else
		{
			this.m_RightHandCollider.enabled = true;
		}
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

	private static FistFightController s_Instance;

	private bool m_DamageWindow;

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

	public enum Mode
	{
		None = -1,
		Normal,
		Hard,
		Cancelled
	}
}
