using System;
using System.Collections.Generic;
using AIs;
using Enums;
using UnityEngine;

public class FightController : PlayerController
{
	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Player.m_ActiveFightController = this;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.SetBlock(false);
		this.m_Player.m_ActiveFightController = null;
	}

	public virtual bool IsAttack()
	{
		return false;
	}

	protected virtual void Attack()
	{
		AIManager.Get().OnPlayerStartAttack();
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		this.UpdateBlock();
	}

	private void UpdateBlock()
	{
		if (this.m_IsBlock != InputsManager.Get().IsActionActive(InputsManager.InputAction.Block))
		{
			this.SetBlock(!this.m_IsBlock);
		}
		if (this.m_IsBlock)
		{
			PlayerConditionModule.Get().DecreaseStamina(PlayerConditionModule.Get().GetStaminaDecrease(StaminaDecreaseReason.Block) * Time.deltaTime);
			if (PlayerConditionModule.Get().GetStamina() == 0f)
			{
				this.SetBlock(false);
			}
			else if (SwimController.Get().IsActive())
			{
				this.SetBlock(false);
			}
			else if (Inventory3DManager.Get().gameObject.activeSelf)
			{
				this.SetBlock(false);
			}
			else if (HUDReadableItem.Get().enabled)
			{
				this.SetBlock(false);
			}
		}
	}

	public bool IsBlock()
	{
		return this.m_IsBlock;
	}

	protected virtual bool CanBlock()
	{
		return !PlayerConditionModule.Get().IsStaminaCriticalLevel() && !SwimController.Get().IsActive() && !Inventory3DManager.Get().gameObject.activeSelf && !WatchController.Get().IsActive() && !MapController.Get().IsActive() && !TriggerController.Get().IsGrabInProgress() && !Player.Get().m_Aim;
	}

	public virtual void SetBlock(bool set)
	{
		if (set && !this.CanBlock())
		{
			return;
		}
		this.m_IsBlock = set;
		if (this.m_Animator.isInitialized)
		{
			this.m_Player.m_Animator.SetBool(this.m_BlockHash, set);
		}
		if (set)
		{
			this.ResetAttack();
			Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
			if (currentItem)
			{
				currentItem.gameObject.SetActive(true);
			}
		}
	}

	public virtual void ResetAttack()
	{
	}

	public override void GetInputActions(ref List<int> actions)
	{
		base.GetInputActions(ref actions);
		if (this.CanBlock())
		{
			actions.Add(58);
		}
	}

	private int m_BlockHash = Animator.StringToHash("Block");

	private bool m_IsBlock;

	public float m_BlockAttackStaminaLevel = 0.1f;
}
