using System;
using Enums;
using UnityEngine;

public class TorchController : WeaponMeleeController
{
	public new static TorchController Get()
	{
		return TorchController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		TorchController.s_Instance = this;
		base.m_ControllerType = PlayerControllerType.Torch;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Animator.SetBool(this.m_TorchHash, true);
		this.m_Torch = this.m_Player.GetCurrentItem(Hand.Right).gameObject.GetComponent<Torch>();
		DebugUtils.Assert(this.m_Torch != null, "[TorchController:OnEnable] Missing Torch component in current item!", true, DebugUtils.AssertType.Info);
		this.m_Torch.PlayTakeOutSound();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (this.m_Animator.isInitialized)
		{
			this.m_Animator.SetBool(this.m_TorchHash, false);
			this.m_Animator.SetBool(this.m_TorchIgniteHash, false);
			this.m_Animator.SetBool(this.m_TorchBurningHash, false);
			this.m_Animator.SetBool(this.m_TorchAimHash, false);
			this.m_Animator.ResetTrigger(this.m_TorchThrowHash);
		}
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		if (this.m_Animator.GetBool(this.m_TorchBurningHash) != this.m_Torch.m_Burning)
		{
			this.m_Animator.SetBool(this.m_TorchBurningHash, this.m_Torch.m_Burning);
		}
		this.UpdateState();
		if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.T))
		{
			if (this.m_Torch.m_Burning)
			{
				this.m_Torch.Extinguish();
				return;
			}
			this.m_Torch.m_Fuel = 1f;
			this.m_Torch.StartBurning();
		}
	}

	private void UpdateState()
	{
	}

	public override void OnInputAction(InputActionData action_data)
	{
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			return;
		}
		base.OnInputAction(action_data);
	}

	private void SetState(TorchController.TorchState state)
	{
		if (this.m_TorchState == state)
		{
			return;
		}
		this.OnExitState();
		this.m_TorchState = state;
		this.OnEnterState();
	}

	private void OnEnterState()
	{
		switch (this.m_TorchState)
		{
		case TorchController.TorchState.None:
			this.m_Animator.SetBool(this.m_TorchAimHash, false);
			return;
		case TorchController.TorchState.Swing:
			this.m_Torch.OnStartSwing();
			return;
		case TorchController.TorchState.FinishSwing:
			this.m_Torch.OnStopSwing();
			this.m_Player.DecreaseStamina(StaminaDecreaseReason.Swing);
			return;
		case TorchController.TorchState.Aim:
			this.m_Animator.SetBool(this.m_TorchAimHash, true);
			this.m_Player.StartAim(Player.AimType.Item, 18f);
			return;
		case TorchController.TorchState.Throw:
			this.m_Animator.SetTrigger(this.m_TorchThrowHash);
			this.m_Animator.SetBool(this.m_TorchAimHash, false);
			return;
		default:
			return;
		}
	}

	private void OnExitState()
	{
		TorchController.TorchState torchState = this.m_TorchState;
		if (torchState == TorchController.TorchState.Aim)
		{
			this.m_Player.StopAim();
		}
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.TorchIgnite)
		{
			this.OnIgnite();
			return;
		}
		if (id == AnimEventID.TorchIgniteEnd)
		{
			this.m_FireObject = null;
			this.m_Animator.ResetTrigger(this.m_TorchIgniteHash);
		}
	}

	public void IgniteFireObject(IFireObject fire_object)
	{
		this.m_FireObject = fire_object;
		this.m_Animator.SetBool(this.m_TorchHash, true);
		this.m_Animator.SetTrigger(this.m_TorchIgniteHash);
	}

	private void OnIgnite()
	{
		if (this.m_FireObject != null)
		{
			if (this.m_FireObject.IsBurning() != this.m_Torch.m_Burning)
			{
				if (this.m_Torch.m_Burning)
				{
					this.m_FireObject.Ignite();
					return;
				}
				if (this.m_FireObject.IsBurning())
				{
					this.m_Torch.StartBurning();
					return;
				}
			}
		}
		else if (this.m_Dynamite && this.m_Torch.m_Burning)
		{
			this.m_Dynamite.StartBurning();
		}
	}

	public void OnDynamiteIgnite(Dynamite dynamite)
	{
		this.m_Dynamite = dynamite;
		this.m_Animator.SetTrigger(this.m_TorchIgniteHash);
	}

	public override bool IsAttack()
	{
		return this.m_Animator.GetCurrentAnimatorStateInfo(1).shortNameHash == this.m_TorchSwing;
	}

	private int m_TorchHash = Animator.StringToHash("Torch");

	private int m_TorchIgniteHash = Animator.StringToHash("TorchIgnite");

	private int m_TorchBurningHash = Animator.StringToHash("TorchBurning");

	private int m_TorchAimHash = Animator.StringToHash("TorchAim");

	private int m_TorchThrowHash = Animator.StringToHash("TorchThrow");

	private int m_TorchSwing = Animator.StringToHash("ObjectSwing");

	private TorchController.TorchState m_TorchState;

	private Torch m_Torch;

	private IFireObject m_FireObject;

	private Dynamite m_Dynamite;

	private static TorchController s_Instance;

	private enum TorchState
	{
		None,
		Swing,
		FinishSwing,
		Aim,
		Throw
	}
}
