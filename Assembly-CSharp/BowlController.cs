using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class BowlController : PlayerController
{
	public static BowlController Get()
	{
		return BowlController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		BowlController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.Bowl;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Animator.SetBool(this.m_CarryingBowlHash, true);
		this.m_Bowl = (Bowl)this.m_Player.GetCurrentItem(Hand.Right);
		DebugUtils.Assert(this.m_Bowl != null, "[CarryingBowlController:OnEnable] ERROR - Currentitem is not a Bowl!", true, DebugUtils.AssertType.Info);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetBool(this.m_CarryingBowlHash, false);
		this.m_Bowl = null;
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
		if (!currentItem || !currentItem.m_Info.IsBowl())
		{
			this.Stop();
		}
	}

	public override void OnInputAction(InputsManager.InputAction action)
	{
		if (action != InputsManager.InputAction.Drop)
		{
			if (action != InputsManager.InputAction.BowlSpil)
			{
				if (action == InputsManager.InputAction.BowlDrink)
				{
					this.Drink();
				}
			}
			else
			{
				this.Spill(-1f);
			}
		}
		else
		{
			this.m_Player.DropItem(this.m_Bowl);
		}
	}

	private void Drink()
	{
		this.m_Bowl.Drink();
	}

	private void Spill(float amount = -1f)
	{
		this.m_Bowl.Spill(amount);
	}

	public void TakeLiquid(LiquidSource source)
	{
		this.m_Bowl.Fill(source);
	}

	public override void GetInputActions(ref List<int> actions)
	{
		if (!this.m_Bowl)
		{
			return;
		}
		actions.Add(0);
		if (this.m_Bowl.m_LCInfo.m_Amount > 0f)
		{
			actions.Add(18);
			actions.Add(17);
		}
	}

	private int m_CarryingBowlHash = Animator.StringToHash("CarryingBowl");

	[HideInInspector]
	public Bowl m_Bowl;

	private static BowlController s_Instance;
}
