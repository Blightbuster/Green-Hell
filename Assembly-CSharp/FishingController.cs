using System;
using System.Collections.Generic;
using AIs;
using Enums;
using UnityEngine;

public class FishingController : PlayerController
{
	public static FishingController Get()
	{
		return FishingController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		FishingController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.Fishing;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
		this.m_FishingRod = ((!currentItem) ? null : currentItem.gameObject.GetComponent<FishingRod>());
		this.m_FishingRod.enabled = true;
		this.SetState(FishingController.State.None);
		HintsManager.Get().ShowHint("Start_Fishing", 10f);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_FishingRod.enabled = false;
		this.m_Animator.SetBool(this.m_BIdle, false);
		if (this.m_State != FishingController.State.None)
		{
			Player.Get().UnblockMoves();
		}
	}

	public bool ShouldShowHUD()
	{
		return this.IsActive() && this.m_State == FishingController.State.Waiting;
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.FishingCastEnd)
		{
			if (!this.m_Target.IsZero())
			{
				this.SetState(FishingController.State.Waiting);
			}
			else
			{
				this.SetState(FishingController.State.Reel);
			}
		}
		else if (id == AnimEventID.FishingStrikeEnd || id == AnimEventID.FishingReelEnd)
		{
			if (this.m_FishingRod.m_Fish)
			{
				this.SetState(FishingController.State.Fish);
			}
			else
			{
				this.SetState(FishingController.State.None);
			}
			this.m_Player.UnblockMoves();
			this.m_FishingRod.StopFishing();
		}
	}

	private void SetState(FishingController.State state)
	{
		this.OnExitState();
		this.m_State = state;
		this.OnEnterState();
	}

	private void OnExitState()
	{
		if (this.m_State == FishingController.State.Aim)
		{
			this.m_Player.StopAim();
		}
	}

	private void OnEnterState()
	{
		switch (this.m_State)
		{
		case FishingController.State.None:
			this.DestroyFish();
			this.m_Animator.SetBool(this.m_BFishing, false);
			this.m_Animator.SetBool(this.m_BIdle, true);
			break;
		case FishingController.State.Aim:
			this.m_Animator.SetBool(this.m_BFishing, true);
			this.m_Animator.SetBool(this.m_BIdle, false);
			this.m_Player.BlockMoves();
			HintsManager.Get().ShowHint("Cast_FishingRod", 10f);
			this.m_Player.StartAim(Player.AimType.Fishing);
			break;
		case FishingController.State.Cast:
			this.m_Animator.SetTrigger(this.m_TFishingCast);
			break;
		case FishingController.State.Waiting:
			this.m_FishingRod.StartFishing(this.m_Target);
			HintsManager.Get().ShowHint("Catch_Fish", 10f);
			break;
		case FishingController.State.Strike:
			this.m_Animator.SetTrigger(this.m_TFishingStrike);
			break;
		case FishingController.State.Reel:
			this.m_Animator.SetTrigger(this.m_TFishingReel);
			break;
		case FishingController.State.Fish:
			this.ShowFish();
			break;
		}
	}

	private bool CanStartFishing()
	{
		return !MainLevel.Instance.IsPause() && !this.m_Player.GetRotationBlocked() && !Inventory3DManager.Get().gameObject.activeSelf && this.m_State == FishingController.State.None;
	}

	private bool CanCast()
	{
		return this.m_State == FishingController.State.Aim;
	}

	public override void OnInputAction(InputsManager.InputAction action)
	{
		switch (action)
		{
		case InputsManager.InputAction.FishingAim:
			if (this.CanStartFishing())
			{
				this.SetState(FishingController.State.Aim);
			}
			break;
		case InputsManager.InputAction.FishingCast:
			if (this.CanCast())
			{
				this.SetState(FishingController.State.Cast);
			}
			break;
		case InputsManager.InputAction.FishingStrike:
			if (this.m_State == FishingController.State.Waiting)
			{
				this.Strike();
			}
			break;
		case InputsManager.InputAction.FishingReel:
			if (this.m_State == FishingController.State.Waiting)
			{
				this.Reel();
			}
			break;
		case InputsManager.InputAction.FishingTakeFish:
			if (this.m_State == FishingController.State.Fish)
			{
				this.TakeFish();
			}
			break;
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
		case FishingController.State.Aim:
			this.UpdateAim();
			break;
		}
	}

	private void UpdateAim()
	{
		this.UpdateAimTarget();
		if (!InputsManager.Get().IsActionActive(InputsManager.InputAction.FishingAim))
		{
			this.m_Player.UnblockMoves();
			this.SetState(FishingController.State.None);
		}
	}

	private void UpdateAimTarget()
	{
		Vector3 position = Camera.main.transform.position;
		Vector3 forward = Camera.main.transform.forward;
		Vector3 target = Vector3.zero;
		Collider component = this.m_FishingRod.gameObject.GetComponent<Collider>();
		bool enabled = false;
		if (component != null)
		{
			enabled = component.enabled;
			component.enabled = false;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(position, forward, out raycastHit, this.m_MaxRange) && (raycastHit.collider.gameObject.IsWater() || raycastHit.collider.gameObject.GetComponent<FishTank>()))
		{
			target = raycastHit.point;
		}
		this.m_Target = target;
		if (component != null)
		{
			component.enabled = enabled;
		}
	}

	public override void ControllerLateUpdate()
	{
		base.ControllerLateUpdate();
		this.LateUpdateState();
	}

	private void LateUpdateState()
	{
		switch (this.m_State)
		{
		case FishingController.State.Fish:
			this.UpdateFish();
			break;
		}
	}

	private void UpdateFish()
	{
		if (!this.m_Fish)
		{
			this.SetState(FishingController.State.None);
		}
		else
		{
			this.m_Fish.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.3f;
			this.m_Fish.transform.rotation = Quaternion.LookRotation(this.m_Player.transform.right);
		}
	}

	private void TakeFish()
	{
		if (!this.m_Fish)
		{
			return;
		}
		AI component = this.m_Fish.GetComponent<AI>();
		AI.AIID id = component.m_ID;
		Item item = ItemsManager.Get().CreateItem(id.ToString() + "_Body", false);
		InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, true);
		UnityEngine.Object.Destroy(component.gameObject);
		this.m_Fish = null;
	}

	private void Strike()
	{
		this.SetState(FishingController.State.Strike);
		this.m_FishingRod.Strike();
	}

	private void Reel()
	{
		this.m_FishingRod.Reel();
		this.SetState(FishingController.State.Reel);
	}

	private void ShowFish()
	{
		if (!this.m_FishingRod.m_Fish)
		{
			return;
		}
		this.m_Fish = UnityEngine.Object.Instantiate<GameObject>(this.m_FishingRod.m_Fish.GetPrefab());
		this.m_Fish.transform.localScale = this.m_FishingRod.m_Fish.transform.localScale;
		Collider component = this.m_Fish.GetComponent<Collider>();
		if (component)
		{
			component.enabled = false;
		}
		HintsManager.Get().ShowHint("Take_Fish", 10f);
		this.m_FishingRod.DestroyFish();
	}

	private void DestroyFish()
	{
		if (this.m_Fish)
		{
			UnityEngine.Object.Destroy(this.m_Fish.gameObject);
			this.m_Fish = null;
		}
	}

	public override void GetInputActions(ref List<int> actions)
	{
		switch (this.m_State)
		{
		case FishingController.State.None:
			actions.Add(21);
			break;
		case FishingController.State.Aim:
			actions.Add(22);
			break;
		case FishingController.State.Waiting:
			actions.Add(24);
			actions.Add(23);
			break;
		case FishingController.State.Fish:
			actions.Add(25);
			break;
		}
	}

	private int m_BIdle = Animator.StringToHash("FishingIdle");

	private int m_BFishing = Animator.StringToHash("Fishing");

	private int m_TFishingCast = Animator.StringToHash("FishingCast");

	private int m_TFishingStrike = Animator.StringToHash("FishingStrike");

	private int m_TFishingReel = Animator.StringToHash("FishingReel");

	public FishingController.State m_State;

	private Vector3 m_Target = Vector3.zero;

	private FishingRod m_FishingRod;

	private float m_MaxRange = 8f;

	private GameObject m_Fish;

	private static FishingController s_Instance;

	public enum State
	{
		None,
		Aim,
		Cast,
		Waiting,
		Strike,
		Reel,
		Fish
	}
}
