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
		this.m_ControllerType = PlayerControllerType.HeavyObject;
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
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetInteger(this.m_IHeavyObjectState, 0);
		this.SetState(HeavyObjectControllerState.None);
		this.m_DropScheduled = false;
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
		if (currentItem)
		{
			this.DropItem();
		}
	}

	public override void OnInputAction(InputsManager.InputAction action)
	{
		if (action == InputsManager.InputAction.Drop)
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
		if (this.m_State == HeavyObjectControllerState.Leaving && ((currentAnimatorStateInfo.shortNameHash == this.m_IPostHeavyObjectState && currentAnimatorStateInfo.normalizedTime >= 0.5f) || (currentAnimatorStateInfo.shortNameHash == this.m_IInsertHeavyObjectToSlotState && currentAnimatorStateInfo.normalizedTime >= 0.6f)))
		{
			Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
			this.DropItem();
			if (this.m_GhostSlot)
			{
				this.m_GhostSlot.Fulfill(false);
				this.m_GhostSlot = null;
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
			Vector3 position = new Vector3(0.5f, 0.5f, 1.1f);
			position.x = Math.Max(0.5f, position.x);
			Vector3 position2 = Camera.main.ViewportToWorldPoint(position);
			currentItem.transform.position = position2;
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
		return this.m_Player.GetCurrentItem(Hand.Right).m_Info.m_ID.ToString();
	}

	private void SetState(HeavyObjectControllerState state)
	{
		this.m_State = state;
	}

	private int m_IHeavyObjectState = Animator.StringToHash("HeavyObject");

	private int m_IPreHeavyObjectState = Animator.StringToHash("PreHeavyObject");

	private int m_IPostHeavyObjectState = Animator.StringToHash("PostHeavyObject");

	private int m_IInsertHeavyObjectToSlotState = Animator.StringToHash("InsertHeavyObjectToSlot");

	private HeavyObjectControllerState m_State;

	private GhostSlot m_GhostSlot;

	private bool m_DropScheduled;

	private static HeavyObjectController s_Instance;
}
