using System;
using System.Collections.Generic;
using AIs;
using Enums;
using UnityEngine;

public class FightController : PlayerController
{
	protected override void Start()
	{
		base.Start();
		PlayerConditionModule.Get().OnStaminaDecreasedEvent += this.OnStaminaDecreased;
	}

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
		bool flag = InputsManager.Get().IsActionActive(InputsManager.InputAction.Block);
		if (this.m_IsBlock != flag && !Player.Get().m_Animator.GetBool(Player.Get().m_CleanUpHash))
		{
			this.SetBlock(!this.m_IsBlock);
		}
		if (!flag)
		{
			this.m_WasBlockBroken = false;
		}
		if (this.m_IsBlock)
		{
			this.m_LastBlockTime = Time.time;
			PlayerConditionModule.Get().DecreaseStamina(PlayerConditionModule.Get().GetStaminaDecrease(StaminaDecreaseReason.Block) * Time.deltaTime);
			if (SwimController.Get().IsActive())
			{
				this.SetBlock(false);
				return;
			}
			if (Inventory3DManager.Get().gameObject.activeSelf)
			{
				this.SetBlock(false);
				return;
			}
			if (HUDReadableItem.Get().enabled)
			{
				this.SetBlock(false);
				return;
			}
			if (Player.Get().m_Animator.GetBool(Player.Get().m_CleanUpHash))
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
		return !FightController.s_BlockFight && !this.m_WasBlockBroken && !SwimController.Get().IsActive() && !Inventory3DManager.Get().gameObject.activeSelf && !WatchController.Get().IsActive() && !MapController.Get().IsActive() && !TriggerController.Get().IsGrabInProgress() && !Player.Get().m_Aim && !CraftingController.Get().IsActive() && !Player.Get().m_Animator.GetBool(TriggerController.Get().m_BDrinkWater) && this.m_LastBlockTime <= Time.time - (PlayerConditionModule.Get().IsLowStamina() ? 1f : 0.5f) && !ScenarioManager.Get().IsDream() && !ScenarioManager.Get().IsBoolVariableTrue("PlayerMechGameEnding");
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
			this.m_LastBlockTime = Time.time;
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
			actions.Add(56);
		}
	}

	public void BlockFight()
	{
		FightController.s_BlockFight = true;
	}

	public void UnblockFight()
	{
		FightController.s_BlockFight = false;
	}

	protected void OnStaminaDecreased(StaminaDecreaseReason reason, float stamina_val)
	{
		if ((reason == StaminaDecreaseReason.PuchBlock || reason == StaminaDecreaseReason.PuchWeaponBlock) && stamina_val <= 0f && this.IsBlock())
		{
			this.SetBlock(false);
			this.m_WasBlockBroken = true;
		}
	}

	public override bool PlayUnequipAnimation()
	{
		return true;
	}

	private int m_BlockHash = Animator.StringToHash("Block");

	private bool m_IsBlock;

	private bool m_WasBlockBroken;

	private float m_LastBlockTime = float.MinValue;

	protected int m_LowStaminaHash = Animator.StringToHash("LowStamina");

	public static bool s_BlockFight;
}
