using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class BlowpipeController : PlayerController
{
	protected override void Awake()
	{
		base.Awake();
		base.m_ControllerType = PlayerControllerType.Blowpipe;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
		this.m_Animator.SetInteger(this.m_IWeaponType, (int)((Weapon)currentItem).GetWeaponType());
		this.SetState(BlowpipeController.State.Idle);
		this.m_Arrow = null;
		this.m_Loaded = false;
		this.m_BarrelDummy = base.gameObject.transform.FindDeepChild("Barrel");
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetInteger(this.m_IWeaponType, 0);
		if (this.m_Arrow != null)
		{
			this.m_Arrow.gameObject.transform.parent = null;
			this.m_Arrow.m_Loaded = false;
			InventoryBackpack.Get().InsertItem(this.m_Arrow, null, null, true, true, true, true, true);
			this.m_Arrow = null;
		}
		this.SetState(BlowpipeController.State.None);
	}

	private void SetState(BlowpipeController.State state)
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
		if (this.m_State == BlowpipeController.State.Aim)
		{
			this.m_Player.StopAim();
		}
	}

	private void OnEnterState()
	{
		this.m_EnterStateTime = Time.time;
		this.m_Animator.SetInteger(this.m_IBlowpipeState, (int)this.m_State);
		if (this.m_State == BlowpipeController.State.Aim)
		{
			this.m_Player.StartAim(Player.AimType.Blowpipe, 18f);
		}
	}

	public override void OnInputAction(InputActionData action_data)
	{
		base.OnInputAction(action_data);
		InputsManager.InputAction action = action_data.m_Action;
		if (action != InputsManager.InputAction.BlowpipeAim)
		{
			if (action != InputsManager.InputAction.BlowpipeShot)
			{
				return;
			}
			if (this.m_State == BlowpipeController.State.Aim)
			{
				this.SetState(BlowpipeController.State.Shot);
			}
		}
		else if (!Inventory3DManager.Get().gameObject.activeSelf && this.m_State == BlowpipeController.State.Idle)
		{
			this.SetState(BlowpipeController.State.Aim);
			return;
		}
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		this.UpdateState();
	}

	private void UpdateState()
	{
		switch (this.m_State)
		{
		case BlowpipeController.State.Idle:
			if (!this.m_Loaded && this.HasArrows())
			{
				this.SetState(BlowpipeController.State.Load);
				return;
			}
			break;
		case BlowpipeController.State.Aim:
			if (!InputsManager.Get().IsActionActive(InputsManager.InputAction.BlowpipeAim))
			{
				this.SetState(BlowpipeController.State.Idle);
			}
			break;
		case BlowpipeController.State.Shot:
			break;
		default:
			return;
		}
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.BlowpipeShot)
		{
			if (this.m_Arrow)
			{
				this.m_Player.ThrowItem(this.m_Arrow);
				this.m_Arrow.m_Loaded = false;
				this.m_Arrow = null;
				this.m_Loaded = false;
				return;
			}
		}
		else if (id == AnimEventID.BlowpipeShotEnd)
		{
			if (this.HasArrows())
			{
				this.SetState(BlowpipeController.State.Load);
				return;
			}
			this.SetState(BlowpipeController.State.Idle);
			return;
		}
		else if (id == AnimEventID.BlowpipeLoad)
		{
			Item item = InventoryBackpack.Get().FindItem(ItemID.Blowpipe_Arrow);
			if (item != null)
			{
				this.m_Arrow = (Arrow)item;
				InventoryBackpack.Get().RemoveItem(item, false);
				this.m_Arrow.gameObject.transform.rotation = this.m_BarrelDummy.rotation;
				this.m_Arrow.gameObject.transform.position = this.m_BarrelDummy.position;
				this.m_Arrow.gameObject.transform.parent = this.m_BarrelDummy.transform;
				this.m_Arrow.StaticPhxRequestAdd();
				this.m_Arrow.gameObject.SetActive(true);
				this.m_Arrow.m_Loaded = true;
				this.m_Loaded = true;
				return;
			}
		}
		else if (id == AnimEventID.BlowpipeLoadEnd)
		{
			this.SetState(BlowpipeController.State.Idle);
		}
	}

	private bool HasArrows()
	{
		return InventoryBackpack.Get().Contains(ItemID.Blowpipe_Arrow);
	}

	public override void GetInputActions(ref List<int> actions)
	{
		BlowpipeController.State state = this.m_State;
		if (state == BlowpipeController.State.Idle)
		{
			actions.Add(12);
			return;
		}
		if (state != BlowpipeController.State.Aim)
		{
			return;
		}
		actions.Add(13);
	}

	private BlowpipeController.State m_State;

	private int m_IBlowpipeState = Animator.StringToHash("Blowpipe");

	private int m_IWeaponType = Animator.StringToHash("WeaponType");

	private bool m_Loaded;

	private Arrow m_Arrow;

	private Transform m_BarrelDummy;

	private float m_EnterStateTime;

	private enum State
	{
		None,
		Idle,
		Aim,
		Shot,
		Load
	}
}
