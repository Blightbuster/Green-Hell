using System;
using System.Collections.Generic;
using AIs;
using CJTools;
using Enums;
using UnityEngine;

public class WeaponSpearController : WeaponController
{
	public static WeaponSpearController Get()
	{
		return WeaponSpearController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		WeaponSpearController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.WeaponSpear;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Animator.SetBool(TriggerController.s_BGrabItem, false);
		this.SetState(WeaponSpearController.State.Idle);
		this.m_ImpaledObject = null;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (this.m_SpearState != WeaponSpearController.State.Throw)
		{
			this.SetState(WeaponSpearController.State.None);
			this.m_Animator.Play(this.m_SpearIdleHash);
		}
		else
		{
			this.m_Animator.SetInteger(this.m_StateHash, 999);
		}
		if (this.m_MovesBlocked)
		{
			this.m_Player.UnblockMoves();
		}
		if (this.m_ItemBody != null)
		{
			this.m_ItemBody.transform.parent = null;
			this.m_ItemBody = null;
		}
	}

	private void SetState(WeaponSpearController.State state)
	{
		if (this.m_SpearState == state)
		{
			this.OnEnterState();
			return;
		}
		this.m_Animator.SetInteger(this.m_StateHash, (int)state);
		this.m_Animator.speed = ((!this.IsAttack()) ? 1f : Skill.Get<SpearFishingSkill>().GetAnimationSpeedMul());
		if (state == WeaponSpearController.State.None)
		{
			this.m_SpearState = WeaponSpearController.State.None;
		}
	}

	public override bool IsAttack()
	{
		return this.m_SpearState == WeaponSpearController.State.Attack || this.m_SpearState == WeaponSpearController.State.AttackDown;
	}

	protected override void EndAttackNonStop()
	{
		base.EndAttackNonStop();
		this.EndAttack();
	}

	protected override void EndAttack()
	{
		base.EndAttack();
		if (this.m_ImpaledObject)
		{
			this.SetState(WeaponSpearController.State.Presentation);
		}
		this.m_WasWaterHit = false;
	}

	private bool IsAim()
	{
		return this.m_SpearState == WeaponSpearController.State.Aim || this.m_SpearState == WeaponSpearController.State.AimIdle || this.m_SpearState == WeaponSpearController.State.AimWalk || this.m_SpearState == WeaponSpearController.State.AimRun;
	}

	private bool IsThrowAim()
	{
		return this.m_SpearState == WeaponSpearController.State.ThrowAim || this.m_SpearState == WeaponSpearController.State.ThrowAimIdle || this.m_SpearState == WeaponSpearController.State.ThrowAimWalk || this.m_SpearState == WeaponSpearController.State.ThrowAimRun;
	}

	public override string ReplaceClipsGetItemName()
	{
		if (this.m_Player.GetCurrentItem(Hand.Right) == null)
		{
			return string.Empty;
		}
		if (this.m_Player.GetCurrentItem(Hand.Right).m_Info == null)
		{
			return string.Empty;
		}
		return this.m_Player.GetCurrentItem(Hand.Right).m_Info.m_ID.ToString();
	}

	protected new bool CanAttack()
	{
		return !MainLevel.Instance.IsPause() && !this.m_Player.GetRotationBlocked() && !Inventory3DManager.Get().gameObject.activeSelf && !HitReactionController.Get().IsActive() && !base.IsBlock() && !this.IsAttack() && !PlayerConditionModule.Get().IsStaminaLevel(this.m_BlockAttackStaminaLevel);
	}

	public override void OnInputAction(InputsManager.InputAction action)
	{
		base.OnInputAction(action);
		if (!this.CanAttack())
		{
			return;
		}
		if (action == InputsManager.InputAction.SpearAttack)
		{
			if (this.m_SpearState == WeaponSpearController.State.Idle)
			{
				this.SetState(WeaponSpearController.State.AttackDown);
			}
			else if (this.m_SpearState == WeaponSpearController.State.AttackDown)
			{
				this.m_AttackDownCombo = true;
			}
		}
		else if (action == InputsManager.InputAction.SpearUpAim)
		{
			if (this.m_SpearState == WeaponSpearController.State.Idle)
			{
				this.SetState(WeaponSpearController.State.Aim);
			}
		}
		else if (action == InputsManager.InputAction.SpearAttackUp)
		{
			if (this.IsAim())
			{
				this.SetState(WeaponSpearController.State.Attack);
			}
		}
		else if (action == InputsManager.InputAction.SpearThrowAim)
		{
			if (this.m_SpearState == WeaponSpearController.State.Idle)
			{
				this.SetState(WeaponSpearController.State.ThrowAim);
			}
		}
		else if (action == InputsManager.InputAction.SpearThrowReleaseAim)
		{
			if (this.IsThrowAim())
			{
				this.SetState(WeaponSpearController.State.ThrowUnAim);
			}
		}
		else if (action == InputsManager.InputAction.SpearThrow && this.IsThrowAim())
		{
			this.SetState(WeaponSpearController.State.Throw);
		}
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		if (MainLevel.Instance.IsPause())
		{
			return;
		}
		if (this.m_Animator.GetBool(TriggerController.s_BGrabItem))
		{
			if (this.m_StateAfterGrab == WeaponSpearController.State.None)
			{
				this.m_StateAfterGrab = this.m_SpearState;
				this.SetState(WeaponSpearController.State.None);
			}
		}
		else if (this.m_StateAfterGrab != WeaponSpearController.State.None)
		{
			this.SetState(this.m_StateAfterGrab);
			this.m_StateAfterGrab = WeaponSpearController.State.None;
		}
		else if (this.m_SpearState == WeaponSpearController.State.None && this.m_StateAfterGrab == WeaponSpearController.State.None)
		{
			this.SetState(WeaponSpearController.State.Idle);
		}
		this.UpdateState();
		this.UpdateItemBody();
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			if (this.IsThrowAim())
			{
				this.SetState(WeaponSpearController.State.ThrowUnAim);
			}
			else if (this.IsAim())
			{
				this.SetState(WeaponSpearController.State.UnAim);
			}
		}
	}

	public override void ControllerLateUpdate()
	{
		base.ControllerLateUpdate();
		this.UpdateBody();
	}

	private void UpdateState()
	{
		int integer = this.m_Animator.GetInteger(this.m_StateHash);
		if (integer != (int)this.m_SpearState)
		{
			this.m_SpearState = (WeaponSpearController.State)integer;
			this.OnEnterState();
		}
	}

	private void OnEnterState()
	{
		WeaponSpearController.State spearState = this.m_SpearState;
		switch (spearState)
		{
		case WeaponSpearController.State.UnAim:
		case WeaponSpearController.State.ThrowUnAim:
			this.m_Player.StopAim();
			break;
		case WeaponSpearController.State.AttackDown:
			this.Attack();
			this.m_WasWaterHit = false;
			this.m_CanHitWater = false;
			PlayerAudioModule.Get().PlayAttackSound(1f, false);
			this.m_Player.DecreaseStamina(this.m_Player.GetStaminaDecrease(StaminaDecreaseReason.Attack) * Skill.Get<SpearSkill>().GetStaminaMul());
			break;
		case WeaponSpearController.State.Attack:
			this.Attack();
			this.m_WasWaterHit = false;
			this.m_CanHitWater = false;
			PlayerAudioModule.Get().PlayAttackSound(1f, false);
			this.m_Player.DecreaseStamina(this.m_Player.GetStaminaDecrease(StaminaDecreaseReason.Attack) * Skill.Get<SpearSkill>().GetStaminaMul());
			this.m_Player.StopAim();
			break;
		default:
			if (spearState == WeaponSpearController.State.Aim)
			{
				this.m_Player.StartAim(Player.AimType.SpearFishing);
			}
			break;
		case WeaponSpearController.State.ThrowAim:
			this.m_Player.StartAim(Player.AimType.SpearHunting);
			break;
		case WeaponSpearController.State.Throw:
			this.m_Player.StopAim();
			this.m_Player.DecreaseStamina(this.m_Player.GetStaminaDecrease(StaminaDecreaseReason.Throw) * Skill.Get<SpearSkill>().GetStaminaMul());
			break;
		case WeaponSpearController.State.Presentation:
		{
			this.m_ImpaledObject.transform.parent = null;
			AI component = this.m_ImpaledObject.GetComponent<AI>();
			this.m_ImpaledIsStingRay = false;
			this.m_ImpaledArowana = false;
			this.m_ImpaledPiranha = false;
			AI.AIID id = component.m_ID;
			Item item = ItemsManager.Get().CreateItem(id.ToString() + "_Body", false);
			Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
			Vector3 b = (currentItem.m_DamagerStart.position - currentItem.m_DamagerEnd.position).normalized * -0.07f;
			item.transform.rotation = currentItem.m_DamagerStart.rotation;
			if (component.m_ID == AI.AIID.Arowana)
			{
				this.m_ImpaledArowana = true;
				item.transform.Rotate(Vector3.forward, -60f);
			}
			else if (component.m_ID == AI.AIID.Piranha)
			{
				this.m_ImpaledPiranha = true;
				item.transform.Rotate(Vector3.forward, -90f);
			}
			else if (component.IsStringray())
			{
				this.m_ImpaledIsStingRay = true;
				item.transform.Rotate(Vector3.forward, -210f);
			}
			else
			{
				item.transform.Rotate(Vector3.forward, 0f);
				this.m_ImpaledIsStingRay = false;
			}
			item.transform.position = currentItem.m_DamagerStart.position + b;
			item.transform.parent = this.m_Player.GetCurrentItem(Hand.Right).transform;
			item.m_BlockGrabAnimOnExecute = true;
			item.m_AttachedToSpear = true;
			this.m_ItemBody = item;
			UnityEngine.Object.Destroy(this.m_ImpaledObject.gameObject);
			this.m_ImpaledObject = null;
			item.m_CanBeOutlined = false;
			this.PlayCatchAnimation();
			break;
		}
		}
	}

	protected override bool CanBlock()
	{
		return this.m_SpearState != WeaponSpearController.State.Throw && this.m_SpearState != WeaponSpearController.State.Presentation && this.m_SpearState != WeaponSpearController.State.PresentationIdle && base.CanBlock();
	}

	private void UpdateItemBody()
	{
		if (!this.m_ItemBody)
		{
			return;
		}
		if (this.m_ItemBody.m_InInventory || this.m_ItemBody.transform.parent == null || Inventory3DManager.Get().m_CarriedItem == this.m_ItemBody)
		{
			this.SetState(WeaponSpearController.State.Idle);
			this.ResetBodyAnimator();
			this.m_ItemBody.m_CanBeOutlined = true;
			this.m_ItemBody = null;
			if (this.m_MovesBlocked)
			{
				this.m_Player.UnblockMoves();
				this.m_MovesBlocked = false;
			}
		}
	}

	protected override void HitObject(CJObject obj, Vector3 hit_pos, Vector3 hit_dir)
	{
		base.HitObject(obj, hit_pos, hit_dir);
		if (obj.CanBeImpaledOnSpear())
		{
			this.ImpaleObject(obj);
		}
	}

	private void ImpaleObject(CJObject obj)
	{
		if (this.m_ImpaledObject)
		{
			return;
		}
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
		DebugUtils.Assert(currentItem, true);
		this.m_ImpaledObject = obj.gameObject;
		this.m_ImpaledObject.transform.position = currentItem.m_DamagerStart.position;
		this.m_ImpaledObject.transform.parent = currentItem.m_DamagerStart;
		Rigidbody component = this.m_ImpaledObject.GetComponent<Rigidbody>();
		if (component)
		{
			UnityEngine.Object.Destroy(component);
		}
		obj.OnImpaleOnSpear();
		HintsManager.Get().ShowHint("Take_Item_From_Spear", 10f);
		Item component2 = this.m_ImpaledObject.GetComponent<Item>();
		if (component2)
		{
			component2.ItemsManagerUnregister();
			component2.enabled = false;
		}
	}

	public GameObject GetImpaledObject()
	{
		return this.m_ImpaledObject;
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.SpearAimEnd)
		{
			if (this.m_SpearState == WeaponSpearController.State.Aim)
			{
				this.SetState(WeaponSpearController.State.AimIdle);
			}
			else if (this.m_SpearState == WeaponSpearController.State.ThrowAim)
			{
				this.SetState(WeaponSpearController.State.ThrowAimIdle);
			}
		}
		else if (id == AnimEventID.SpearAttackEnd)
		{
			if (this.m_SpearState == WeaponSpearController.State.AttackDown)
			{
				if (this.m_AttackDownCombo)
				{
					this.m_AttackDownCombo = false;
					this.m_Animator.Play(this.m_AttackDownHash, 1, 0f);
					this.SetState(WeaponSpearController.State.AttackDown);
				}
				else
				{
					this.SetState(WeaponSpearController.State.Idle);
				}
			}
			else if (this.m_SpearState == WeaponSpearController.State.AttackNonHit)
			{
				if (InputsManager.Get().IsActionActive(InputsManager.InputAction.SpearThrow))
				{
					this.SetState(WeaponSpearController.State.AimIdle);
				}
				else
				{
					this.SetState(WeaponSpearController.State.Idle);
				}
			}
			else if (this.m_SpearState == WeaponSpearController.State.Attack)
			{
				this.SetState(WeaponSpearController.State.AttackNonHit);
			}
		}
		else if (id == AnimEventID.SpearUnAimEnd)
		{
			this.SetState(WeaponSpearController.State.Idle);
		}
		else if (id == AnimEventID.SpearShot && this.m_SpearState == WeaponSpearController.State.Throw)
		{
			this.Shot();
		}
		else if (id == AnimEventID.SpearShotEnd)
		{
			this.SetState(WeaponSpearController.State.None);
		}
		else if (id == AnimEventID.DamageStart)
		{
			this.m_CanHitWater = true;
		}
		else if (id == AnimEventID.DamageEnd)
		{
			this.m_CanHitWater = false;
		}
		else if (id == AnimEventID.SpearEnterPresentationEnd)
		{
			this.SetState(WeaponSpearController.State.PresentationIdle);
		}
	}

	public override bool ForceReceiveAnimEvent()
	{
		return this.m_SpearState == WeaponSpearController.State.Throw;
	}

	private void Shot()
	{
		if (!this.m_Player.GetCurrentItem(Hand.Right))
		{
			return;
		}
		this.m_Player.GetCurrentItem(Hand.Right).m_Info.m_Health -= this.m_Player.GetCurrentItem(Hand.Right).m_Info.m_DamageSelf;
		this.m_Player.ThrowItem(Hand.Right);
	}

	public override void GetInputActions(ref List<int> actions)
	{
		base.GetInputActions(ref actions);
		if (this.m_SpearState == WeaponSpearController.State.Idle)
		{
			actions.Add(6);
			actions.Add(7);
			actions.Add(9);
		}
		else if (this.m_SpearState == WeaponSpearController.State.Aim)
		{
			actions.Add(8);
		}
		else if (this.m_SpearState == WeaponSpearController.State.ThrowAim || this.m_SpearState == WeaponSpearController.State.ThrowAimIdle)
		{
			actions.Add(11);
		}
	}

	protected override void OnHitWater(Collider water_coll)
	{
		base.OnHitWater(water_coll);
		if (!this.m_CanHitWater)
		{
			return;
		}
		if (this.m_WasWaterHit)
		{
			return;
		}
		water_coll.isTrigger = false;
		RaycastHit[] array = Physics.RaycastAll(Camera.main.transform.position, Camera.main.transform.forward, 5f);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].collider.gameObject == water_coll.gameObject)
			{
				ParticlesManager.Get().Spawn("SmallSplash_Size_C", array[i].point - Camera.main.transform.forward * 0.2f, Quaternion.identity, null);
				this.m_WasWaterHit = true;
				break;
			}
		}
		water_coll.isTrigger = true;
	}

	private void UpdateBody()
	{
		if (this.m_ItemBody != null)
		{
			if (!this.m_ItemBody.m_InInventory)
			{
				Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
				if (currentItem != null)
				{
					Vector3 b = (currentItem.m_DamagerStart.position - currentItem.m_DamagerEnd.position).normalized * -0.07f;
					this.m_ItemBody.transform.rotation = currentItem.m_DamagerStart.rotation;
					if (this.m_ImpaledArowana)
					{
						this.m_ItemBody.transform.Rotate(Vector3.forward, -60f);
					}
					else if (this.m_ImpaledPiranha)
					{
						this.m_ItemBody.transform.Rotate(Vector3.forward, -90f);
					}
					else if (this.m_ImpaledIsStingRay)
					{
						this.m_ItemBody.transform.Rotate(Vector3.forward, 80f);
						this.m_ItemBody.transform.Rotate(Vector3.up, 180f);
					}
					else
					{
						this.m_ItemBody.transform.Rotate(Vector3.forward, 0f);
					}
					this.m_ItemBody.transform.position = currentItem.m_DamagerStart.position + b;
					if (this.m_ImpaledArowana)
					{
						this.m_ItemBody.transform.position -= this.m_ItemBody.transform.forward * 0.1f;
					}
				}
			}
			else
			{
				Animator componentDeepChild = General.GetComponentDeepChild<Animator>(this.m_ItemBody.gameObject);
				if (componentDeepChild != null)
				{
					componentDeepChild.SetBool("Backpack", true);
				}
			}
		}
	}

	private void PlayCatchAnimation()
	{
		if (this.m_ItemBody == null)
		{
			return;
		}
		Animator componentDeepChild = General.GetComponentDeepChild<Animator>(this.m_ItemBody.gameObject);
		componentDeepChild.SetBool(WeaponSpearController.s_BodyCatch, true);
	}

	private void ResetBodyAnimator()
	{
		if (this.m_ItemBody == null)
		{
			return;
		}
		Animator componentDeepChild = General.GetComponentDeepChild<Animator>(this.m_ItemBody.gameObject);
		componentDeepChild.SetBool(WeaponSpearController.s_BodyCatch, false);
		componentDeepChild.SetBool("Backpack", true);
	}

	public override void ResetAttack()
	{
		base.ResetAttack();
		this.m_Player.StopAim();
		this.SetState(WeaponSpearController.State.Idle);
	}

	private WeaponSpearController.State m_SpearState;

	private WeaponSpearController.State m_StateAfterGrab;

	private int m_StateHash = Animator.StringToHash("Spear_State");

	private int m_AttackDownHash = Animator.StringToHash("Spear_AttackDown");

	private GameObject m_ImpaledObject;

	[HideInInspector]
	public Item m_ItemBody;

	private bool m_WasWaterHit;

	private bool m_CanHitWater;

	private bool m_ImpaledIsStingRay;

	private bool m_ImpaledArowana;

	private bool m_ImpaledPiranha;

	private static int s_BodyCatch = Animator.StringToHash("Catch");

	private bool m_AttackDownCombo;

	private static WeaponSpearController s_Instance = null;

	private enum State
	{
		None,
		Idle,
		Walk,
		Run,
		Aim,
		AimIdle,
		AimWalk,
		AimRun,
		UnAim,
		AttackDown,
		Attack,
		AttackNonHit,
		AttackHit,
		ThrowAim,
		ThrowAimIdle,
		ThrowAimWalk,
		ThrowAimRun,
		ThrowUnAim,
		Throw,
		Presentation,
		PresentationIdle
	}
}
