using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class HeavyObjectController : PlayerController
{
	public static HeavyObjectController Get()
	{
		return HeavyObjectController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HeavyObjectController.s_Instance = this;
		base.m_ControllerType = PlayerControllerType.HeavyObject;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (InventoryBackpack.Get().m_EquippedItem)
		{
			this.m_Animator.SetInteger(this.m_IHeavyObjectState, 3);
			this.m_Animator.SetBool(TriggerController.s_BGrabItemBow, false);
		}
		else
		{
			this.m_Animator.SetInteger(this.m_IHeavyObjectState, 1);
		}
		this.SetState(HeavyObjectControllerState.Normal);
		this.m_DropScheduled = false;
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
		PlayerConditionModule.Get().GetDirtinessAdd(GetDirtyReason.HeavyObject, (HeavyObjectInfo)currentItem.m_Info);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetInteger(this.m_IHeavyObjectState, 0);
		this.SetState(HeavyObjectControllerState.None);
		this.m_DropScheduled = false;
		if (this.m_Player.GetCurrentItem(Hand.Right))
		{
			this.DropItem();
		}
	}

	public override void OnInputAction(InputActionData action_data)
	{
		if (action_data.m_Action == InputsManager.InputAction.Drop)
		{
			this.Drop();
		}
	}

	public void Drop()
	{
		this.m_DropScheduled = true;
	}

	public void InsertToGhostSlot(GhostSlot slot)
	{
		this.SetState(HeavyObjectControllerState.Leaving);
		this.m_Animator.SetInteger(this.m_IHeavyObjectState, 2);
		this.m_GhostSlot = slot;
	}

	public void InsertToGhostPart(GhostPart part)
	{
		this.SetState(HeavyObjectControllerState.Leaving);
		this.m_Animator.SetInteger(this.m_IHeavyObjectState, 2);
		this.m_GhostPart = part;
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		AnimatorStateInfo currentAnimatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(1);
		if (currentAnimatorStateInfo.shortNameHash == this.m_IHeavyObjectState && !this.m_Animator.IsInTransition(1))
		{
			if (this.m_DropScheduled)
			{
				this.SetState(HeavyObjectControllerState.Leaving);
				this.m_Animator.SetInteger(this.m_IHeavyObjectState, 0);
				this.m_DropScheduled = false;
			}
			else if (this.m_State != HeavyObjectControllerState.Leaving)
			{
				this.m_Animator.SetInteger(this.m_IHeavyObjectState, 1);
			}
		}
		if (this.m_State == HeavyObjectControllerState.Leaving && ((currentAnimatorStateInfo.shortNameHash == this.m_IPostHeavyObjectState && currentAnimatorStateInfo.normalizedTime >= 0.9f) || (currentAnimatorStateInfo.shortNameHash == this.m_IInsertHeavyObjectToSlotState && currentAnimatorStateInfo.normalizedTime >= 0.9f) || currentAnimatorStateInfo.shortNameHash == this.m_UnarmedIdle))
		{
			Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
			this.DropItem();
			if (this.m_GhostSlot)
			{
				this.m_GhostSlot.Fulfill(false);
				this.m_GhostSlot = null;
				UnityEngine.Object.Destroy(currentItem.gameObject);
			}
			if (this.m_GhostPart)
			{
				this.m_GhostPart.Fulfill(false);
				this.m_GhostPart = null;
				UnityEngine.Object.Destroy(currentItem.gameObject);
			}
		}
	}

	private void DropItem()
	{
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
		this.m_Player.DropItem(currentItem);
		if (currentItem)
		{
			currentItem.transform.rotation = Player.Get().transform.rotation;
			if (Camera.main)
			{
				Vector3 vector = new Vector3(0.5f, 0.5f, 1.1f);
				vector.x = Math.Max(0.5f, vector.x);
				Vector3 position = Camera.main.ViewportToWorldPoint(vector);
				currentItem.transform.position = position;
			}
			else
			{
				currentItem.transform.position = Player.Get().transform.position + Vector3.up + Player.Get().transform.forward;
			}
		}
		Player.Get().OnDropHeavyItem();
	}

	public override void GetInputActions(ref List<int> actions)
	{
		actions.Add(0);
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

	private void SetState(HeavyObjectControllerState state)
	{
		this.m_State = state;
	}

	private int m_IHeavyObjectState = Animator.StringToHash("HeavyObject");

	private int m_IPreHeavyObjectState = Animator.StringToHash("PreHeavyObject");

	private int m_IPostHeavyObjectState = Animator.StringToHash("PostHeavyObject");

	private int m_IInsertHeavyObjectToSlotState = Animator.StringToHash("InsertHeavyObjectToSlot");

	private int m_UnarmedIdle = Animator.StringToHash("Unarmed_Idle");

	private HeavyObjectControllerState m_State;

	private GhostSlot m_GhostSlot;

	private GhostPart m_GhostPart;

	private bool m_DropScheduled;

	private static HeavyObjectController s_Instance;
}
