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
		base.m_ControllerType = PlayerControllerType.WeaponSpear;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Animator.SetBool(TriggerController.s_BGrabItem, false);
		this.m_ImpaledObject = null;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.SetState(WeaponSpearController.State.None);
		if (this.m_ItemBody != null)
		{
			this.m_ItemBody.transform.parent = null;
			this.ResetBodyAnimator();
			this.m_ItemBody.Take();
			this.m_ItemBody = null;
		}
		Player.Get().StopAim();
	}

	private void SetState(WeaponSpearController.State state)
	{
		if (this.m_SpearState == state)
		{
			this.OnEnterState();
			return;
		}
		this.m_Animator.SetInteger(this.m_StateHash, (int)state);
		this.m_Animator.speed = (this.IsAttack() ? Skill.Get<SpearFishingSkill>().GetAnimationSpeedMul() : (1f * Player.Get().m_SpeedMul));
		if (state == WeaponSpearController.State.None)
		{
			this.m_SpearPrevState = this.m_SpearState;
			this.m_SpearState = WeaponSpearController.State.None;
		}
	}

	public override bool IsAttack()
	{
		return this.m_SpearState == WeaponSpearController.State.Attack || this.m_SpearState == WeaponSpearController.State.AttackDown || this.m_SpearState == WeaponSpearController.State.AttackHit || this.m_SpearState == WeaponSpearController.State.AttackNonHit || this.m_SpearState == WeaponSpearController.State.UnAim;
	}

	protected override void EndAttackNonStop()
	{
		if (this.m_ImpaledObject)
		{
			this.SetState(WeaponSpearController.State.Presentation);
		}
		this.m_WasWaterHit = false;
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

	public bool IsThrow()
	{
		return this.m_SpearState == WeaponSpearController.State.Throw;
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
		return EnumUtils<ItemID>.GetName((int)this.m_Player.GetCurrentItem(Hand.Right).m_Info.m_ID);
	}

	protected override bool CanAttack()
	{
		return !MainLevel.Instance.IsPause() && Time.time - MainLevel.Instance.m_LastUnpauseTime >= 0.35f && !this.m_Player.GetRotationBlocked() && !Inventory3DManager.Get().gameObject.activeSelf && !HitReactionController.Get().IsActive() && !base.IsBlock() && !this.IsAttack() && !HUDSelectDialog.Get().enabled && !HUDSelectDialogNode.Get().enabled && Time.time - HUDSelectDialog.Get().m_LastSelectDialogTime >= 0.35f && Time.time - HUDSelectDialogNode.Get().m_LastSelectNodeTime >= 0.35f;
	}

	public void Jump()
	{
		this.SetState(WeaponSpearController.State.Jump);
	}

	public override void OnInputAction(InputActionData action_data)
	{
		base.OnInputAction(action_data);
		if (action_data.m_Action != InputsManager.InputAction.SpearAttack && action_data.m_Action != InputsManager.InputAction.SpearUpAim && action_data.m_Action != InputsManager.InputAction.SpearAttackUp && action_data.m_Action != InputsManager.InputAction.SpearThrowAim && action_data.m_Action != InputsManager.InputAction.SpearThrowReleaseAim && action_data.m_Action != InputsManager.InputAction.SpearThrow)
		{
			return;
		}
		if (!this.CanAttack())
		{
			return;
		}
		if (action_data.m_Action == InputsManager.InputAction.SpearAttack)
		{
			if (this.m_SpearState == WeaponSpearController.State.Idle)
			{
				if (GreenHellGame.IsPCControllerActive() || this.m_SpearPrevState != WeaponSpearController.State.ThrowUnAim)
				{
					this.SetState(WeaponSpearController.State.AttackDown);
					return;
				}
				this.m_SpearPrevState = WeaponSpearController.State.None;
				return;
			}
			else if (this.m_SpearState == WeaponSpearController.State.AttackDown)
			{
				this.m_AttackDownCombo = true;
				return;
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.SpearUpAim)
		{
			if (this.m_SpearState == WeaponSpearController.State.Idle)
			{
				this.SetState(WeaponSpearController.State.Aim);
				return;
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.SpearAttackUp)
		{
			if (this.IsAim())
			{
				this.SetState(WeaponSpearController.State.Attack);
				return;
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.SpearThrowAim)
		{
			if (this.m_SpearState == WeaponSpearController.State.Idle)
			{
				this.SetState(WeaponSpearController.State.ThrowAim);
				return;
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.SpearThrowReleaseAim)
		{
			if (this.IsThrowAim())
			{
				this.SetState(WeaponSpearController.State.ThrowUnAim);
				return;
			}
			if (GreenHellGame.IsPadControllerActive() && this.IsAim())
			{
				this.SetState(WeaponSpearController.State.UnAim);
				return;
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.SpearThrow && this.IsThrowAim())
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
		if (base.IsBlock() && this.m_SpearState != WeaponSpearController.State.BlockIn && this.m_SpearState != WeaponSpearController.State.BlockIdle && this.m_SpearState != WeaponSpearController.State.BlockOut)
		{
			this.SetState(WeaponSpearController.State.BlockIn);
		}
		else if (this.m_SpearState == WeaponSpearController.State.BlockIdle && !base.IsBlock())
		{
			if (this.m_Animator.GetBool(this.m_LowStaminaHash))
			{
				this.SetState(WeaponSpearController.State.Idle);
			}
			else
			{
				this.SetState(WeaponSpearController.State.BlockOut);
			}
		}
		else if (this.m_Animator.GetBool(TriggerController.s_BGrabItem))
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
		else if (this.m_SpearState == WeaponSpearController.State.Jump)
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
				return;
			}
			if (this.IsAim())
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
			this.m_SpearPrevState = this.m_SpearState;
			this.m_SpearState = (WeaponSpearController.State)integer;
			this.OnEnterState();
		}
	}

	private void OnEnterState()
	{
		WeaponSpearController.State spearState = this.m_SpearState;
		switch (spearState)
		{
		case WeaponSpearController.State.Aim:
			this.m_Player.StartAim(Player.AimType.SpearFishing, 18f);
			return;
		case WeaponSpearController.State.AimIdle:
		case WeaponSpearController.State.AimWalk:
		case WeaponSpearController.State.AimRun:
			return;
		case WeaponSpearController.State.UnAim:
			break;
		case WeaponSpearController.State.AttackDown:
			this.Attack();
			this.m_WasWaterHit = false;
			this.m_CanHitWater = false;
			this.m_Player.DecreaseStamina(this.m_Player.GetStaminaDecrease(StaminaDecreaseReason.Attack) * Skill.Get<SpearSkill>().GetStaminaMul());
			return;
		case WeaponSpearController.State.Attack:
			this.Attack();
			this.m_WasWaterHit = false;
			this.m_CanHitWater = false;
			this.m_Player.DecreaseStamina(this.m_Player.GetStaminaDecrease(StaminaDecreaseReason.Attack) * Skill.Get<SpearSkill>().GetStaminaMul());
			this.m_Player.StopAim();
			return;
		default:
			switch (spearState)
			{
			case WeaponSpearController.State.ThrowAim:
				this.m_Player.StartAim(Player.AimType.SpearHunting, 18f);
				return;
			case WeaponSpearController.State.ThrowAimIdle:
			case WeaponSpearController.State.ThrowAimWalk:
			case WeaponSpearController.State.ThrowAimRun:
			case WeaponSpearController.State.PresentationIdle:
				return;
			case WeaponSpearController.State.ThrowUnAim:
				break;
			case WeaponSpearController.State.Throw:
				this.m_Player.StopAim();
				this.m_Player.DecreaseStamina(this.m_Player.GetStaminaDecrease(StaminaDecreaseReason.Throw) * Skill.Get<SpearSkill>().GetStaminaMul());
				return;
			case WeaponSpearController.State.Presentation:
			{
				this.m_ImpaledObject.transform.parent = null;
				AI component = this.m_ImpaledObject.GetComponent<AI>();
				this.m_ImpaledIsStingRay = false;
				this.m_ImpaledArowana = false;
				this.m_ImpaledPiranha = false;
				this.m_ImpaledCrab = false;
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
				else if (component.m_ID == AI.AIID.Crab)
				{
					this.m_ImpaledCrab = true;
					item.transform.Rotate(Vector3.forward, 90f);
				}
				else
				{
					item.transform.Rotate(Vector3.forward, 0f);
				}
				item.transform.position = currentItem.m_DamagerStart.position + b;
				item.transform.parent = this.m_Player.GetCurrentItem(Hand.Right).transform;
				item.m_BlockGrabAnimOnExecute = true;
				item.m_AttachedToSpear = true;
				this.m_ItemBody = item;
				UnityEngine.Object.Destroy(this.m_ImpaledObject.gameObject);
				this.m_ImpaledObject = null;
				item.m_CanBeOutlined = false;
				item.UpdateLayer();
				item.UpdatePhx();
				item.UpdateScale(false);
				this.PlayCatchAnimation();
				return;
			}
			case WeaponSpearController.State.Jump:
				this.m_Player.DecreaseStamina(this.m_Player.GetStaminaDecrease(StaminaDecreaseReason.Jump));
				this.m_Player.StopAim();
				return;
			default:
				return;
			}
			break;
		}
		this.m_Player.StopAim();
	}

	protected override bool CanBlock()
	{
		return this.m_SpearState != WeaponSpearController.State.Throw && this.m_SpearState != WeaponSpearController.State.Presentation && this.m_SpearState != WeaponSpearController.State.PresentationIdle && base.CanBlock();
	}

	private void UpdateItemBody()
	{
		if ((!this.m_ItemBody && (this.m_SpearState == WeaponSpearController.State.Presentation || this.m_SpearState == WeaponSpearController.State.PresentationIdle)) || (this.m_ItemBody && (this.m_ItemBody.m_InInventory || this.m_ItemBody.transform.parent == null || Inventory3DManager.Get().m_CarriedItem == this.m_ItemBody)))
		{
			this.SetState(WeaponSpearController.State.Idle);
			this.ResetBodyAnimator();
			if (this.m_ItemBody)
			{
				this.m_ItemBody.m_CanBeOutlined = true;
				this.m_ItemBody = null;
			}
			if (this.m_MovesBlocked)
			{
				this.m_Player.UnblockMoves();
				this.m_MovesBlocked = false;
			}
		}
	}

	protected override void HitObject(CJObject obj, Vector3 hit_pos, Vector3 hit_dir)
	{
		if (obj.IsFish() && !obj.CanBeImpaledOnSpear())
		{
			return;
		}
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
		Collider component = obj.GetComponent<Collider>();
		if (component)
		{
			Physics.IgnoreCollision(this.m_Player.m_Collider, component);
		}
		Rigidbody component2 = this.m_ImpaledObject.GetComponent<Rigidbody>();
		if (component2)
		{
			UnityEngine.Object.Destroy(component2);
		}
		obj.OnImpaleOnSpear();
		HintsManager.Get().ShowHint("Take_Item_From_Spear", 10f);
		Item component3 = this.m_ImpaledObject.GetComponent<Item>();
		if (component3)
		{
			component3.ItemsManagerUnregister();
			component3.enabled = false;
		}
		this.m_Player.BlockMoves();
		this.m_MovesBlocked = true;
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
				return;
			}
			if (this.m_SpearState == WeaponSpearController.State.ThrowAim)
			{
				this.SetState(WeaponSpearController.State.ThrowAimIdle);
				return;
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
					return;
				}
				this.SetState(WeaponSpearController.State.Idle);
				return;
			}
			else if (this.m_SpearState == WeaponSpearController.State.AttackNonHit)
			{
				if (InputsManager.Get().IsActionActive(InputsManager.InputAction.SpearThrow))
				{
					this.SetState(WeaponSpearController.State.AimIdle);
					return;
				}
				this.SetState(WeaponSpearController.State.UnAim);
				return;
			}
			else if (this.m_SpearState == WeaponSpearController.State.Attack)
			{
				this.SetState(WeaponSpearController.State.AttackNonHit);
				return;
			}
		}
		else
		{
			if (id == AnimEventID.SpearUnAimEnd)
			{
				this.SetState(WeaponSpearController.State.Idle);
				return;
			}
			if (id == AnimEventID.SpearShot && this.m_SpearState == WeaponSpearController.State.Throw)
			{
				this.Shot();
				return;
			}
			if (id == AnimEventID.SpearShotEnd)
			{
				this.SetState(WeaponSpearController.State.None);
				return;
			}
			if (id == AnimEventID.DamageStart)
			{
				this.m_CanHitWater = true;
				return;
			}
			if (id == AnimEventID.DamageEnd)
			{
				this.m_CanHitWater = false;
				return;
			}
			if (id == AnimEventID.SpearEnterPresentationEnd)
			{
				this.SetState(WeaponSpearController.State.PresentationIdle);
				return;
			}
			if (id == AnimEventID.JumpSpearEnd)
			{
				this.SetState(WeaponSpearController.State.Idle);
				return;
			}
			if (id == AnimEventID.SpearBlockInEnd && this.m_SpearState == WeaponSpearController.State.BlockIn)
			{
				this.SetState(WeaponSpearController.State.BlockIdle);
				return;
			}
			if (id == AnimEventID.SpearBlockOutEnd && this.m_SpearState == WeaponSpearController.State.BlockOut)
			{
				this.SetState(WeaponSpearController.State.Idle);
			}
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
			return;
		}
		if (this.m_SpearState == WeaponSpearController.State.Aim)
		{
			actions.Add(8);
			return;
		}
		if (this.m_SpearState == WeaponSpearController.State.ThrowAim || this.m_SpearState == WeaponSpearController.State.ThrowAimIdle)
		{
			actions.Add(11);
			actions.Add(4);
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
		int num = Physics.RaycastNonAlloc(CameraManager.Get().m_MainCamera.transform.position, CameraManager.Get().m_MainCamera.transform.forward, this.m_RaycastResultsTmp, 5f);
		for (int i = 0; i < num; i++)
		{
			if (this.m_RaycastResultsTmp[i].collider.gameObject == water_coll.gameObject)
			{
				ParticlesManager.Get().Spawn("SmallSplash_Size_C", this.m_RaycastResultsTmp[i].point - CameraManager.Get().m_MainCamera.transform.forward * 0.2f, Quaternion.identity, Vector3.zero, null, -1f, false);
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
					else if (this.m_ImpaledCrab)
					{
						this.m_ItemBody.transform.Rotate(Vector3.forward, 90f);
					}
					else
					{
						this.m_ItemBody.transform.Rotate(Vector3.forward, 0f);
					}
					this.m_ItemBody.transform.position = currentItem.m_DamagerStart.position + b;
					if (this.m_ImpaledArowana)
					{
						this.m_ItemBody.transform.position -= this.m_ItemBody.transform.forward * 0.1f;
						return;
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
		if (componentDeepChild != null)
		{
			componentDeepChild.SetBool(WeaponSpearController.s_BodyCatch, true);
		}
	}

	private void ResetBodyAnimator()
	{
		if (this.m_ItemBody == null)
		{
			return;
		}
		Animator componentDeepChild = General.GetComponentDeepChild<Animator>(this.m_ItemBody.gameObject);
		if (componentDeepChild != null)
		{
			componentDeepChild.SetBool(WeaponSpearController.s_BodyCatch, false);
			componentDeepChild.SetBool("Backpack", true);
		}
	}

	public override void ResetAttack()
	{
		base.ResetAttack();
		if (this.m_SpearState != WeaponSpearController.State.BlockIn && this.m_SpearState != WeaponSpearController.State.BlockIdle && this.m_SpearState != WeaponSpearController.State.BlockOut)
		{
			this.m_Player.StopAim();
			this.SetState(WeaponSpearController.State.Idle);
		}
	}

	private WeaponSpearController.State m_SpearState;

	private WeaponSpearController.State m_SpearPrevState;

	private WeaponSpearController.State m_StateAfterGrab;

	private int m_StateHash = Animator.StringToHash("Spear_State");

	private int m_AttackDownHash = Animator.StringToHash("Spear_AttackDown");

	public GameObject m_ImpaledObject;

	[HideInInspector]
	public Item m_ItemBody;

	private bool m_WasWaterHit;

	private bool m_CanHitWater;

	private bool m_ImpaledIsStingRay;

	private bool m_ImpaledArowana;

	private bool m_ImpaledPiranha;

	private bool m_ImpaledCrab;

	private static int s_BodyCatch = Animator.StringToHash("Catch");

	private bool m_AttackDownCombo;

	private static WeaponSpearController s_Instance = null;

	private RaycastHit[] m_RaycastResultsTmp = new RaycastHit[20];

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
		PresentationIdle,
		Jump,
		BlockIn,
		BlockIdle,
		BlockOut
	}
}
