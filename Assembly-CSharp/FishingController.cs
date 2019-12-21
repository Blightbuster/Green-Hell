using System;
using System.Collections.Generic;
using AIs;
using CJTools;
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
		base.m_ControllerType = PlayerControllerType.Fishing;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
		this.m_FishingRod = currentItem.gameObject.GetComponent<FishingRod>();
		this.m_FishingRod.enabled = true;
		currentItem.gameObject.SetActive(true);
		this.SetState(FishingController.State.Idle);
		HintsManager.Get().ShowHint("FishRod_Hook", 10f);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.TakeFish();
		if (this.m_FishingRod)
		{
			this.m_FishingRod.ResetVis();
			this.m_FishingRod.enabled = false;
		}
		this.SetState(FishingController.State.None);
		this.BlockMoves(false);
		this.BlockRotation(false);
		this.m_Animator.SetBool(this.m_BackpackRodInHandHash, false);
	}

	public bool ShouldShowHUD()
	{
		return this.IsActive() && this.m_Animator.GetInteger(this.m_StateHash) == 7;
	}

	public bool CanHideRod()
	{
		if (this.m_Animator.IsInTransition(1))
		{
			return false;
		}
		int shortNameHash = this.m_Animator.GetCurrentAnimatorStateInfo(1).shortNameHash;
		return shortNameHash == this.m_FishingJumpHash || shortNameHash == this.m_FishingRunHash || shortNameHash == this.m_FishingWalkHash || shortNameHash == this.m_FishingIdleHash;
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.FishingCastEnd)
		{
			if (!this.m_Target.IsZero())
			{
				this.SetState(FishingController.State.Waiting);
				return;
			}
			this.SetState(FishingController.State.Reel);
			return;
		}
		else
		{
			if (id == AnimEventID.FishingStrikeEnd)
			{
				this.SetState(FishingController.State.Idle);
				this.ShowFish();
				this.m_FishingRod.StopFishing();
				return;
			}
			if (id == AnimEventID.FishingHideVein && this.m_FishingRod)
			{
				this.m_FishingRod.m_HideVein = true;
			}
			return;
		}
	}

	private void SetState(FishingController.State state)
	{
		this.OnExitState();
		this.m_State = state;
		this.m_Animator.SetInteger(this.m_StateHash, (int)this.m_State);
		Debug.Log(state.ToString());
		this.OnEnterState();
	}

	private void OnExitState()
	{
		if (this.m_Animator.GetInteger(this.m_StateHash) == 5)
		{
			this.m_Player.StopAim();
		}
	}

	private void OnEnterState()
	{
		this.m_EnterStateTime = Time.time;
		switch (this.m_Animator.GetInteger(this.m_StateHash))
		{
		case 0:
			this.DestroyFish();
			this.m_FishingRod.Break();
			return;
		case 1:
			this.BlockMoves(false);
			this.BlockRotation(false);
			return;
		case 2:
		case 3:
		case 4:
		case 10:
		case 11:
			break;
		case 5:
			this.BlockMoves(true);
			this.m_Player.StartAim(Player.AimType.Fishing, 18f);
			return;
		case 6:
			this.m_FishingRod.StartCast();
			PlayerConditionModule.Get().DecreaseStamina(PlayerConditionModule.Get().GetStaminaDecrease(StaminaDecreaseReason.RodCast));
			return;
		case 7:
			HintsManager.Get().ShowHint("Catch_Fish", 10f);
			this.m_FishingRod.StartFishing(this.m_Target);
			ParticlesManager.Get().Spawn("SmallSplash_Size_C", this.m_Target, Quaternion.identity, Vector3.zero, null, -1f, false);
			return;
		case 8:
			PlayerConditionModule.Get().DecreaseStamina(PlayerConditionModule.Get().GetStaminaDecrease(StaminaDecreaseReason.RodReel));
			return;
		case 9:
			PlayerConditionModule.Get().DecreaseStamina(PlayerConditionModule.Get().GetStaminaDecrease(StaminaDecreaseReason.RodStrike));
			break;
		case 12:
			this.ShowFish();
			this.BlockRotation(true);
			return;
		default:
			return;
		}
	}

	private bool CanStartFishing()
	{
		if (MainLevel.Instance.IsPause())
		{
			return false;
		}
		if (this.m_Player.GetRotationBlocked())
		{
			return false;
		}
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			return false;
		}
		if (TriggerController.Get().GetBestTrigger())
		{
			return false;
		}
		FishingController.State integer = (FishingController.State)this.m_Animator.GetInteger(this.m_StateHash);
		return integer == FishingController.State.Idle || integer == FishingController.State.Walk || integer == FishingController.State.Run;
	}

	private bool CanCast()
	{
		return this.m_Animator.GetInteger(this.m_StateHash) == 5;
	}

	public override void OnInputAction(InputActionData action_data)
	{
		InputsManager.InputAction action = action_data.m_Action;
		switch (action)
		{
		case InputsManager.InputAction.FishingAim:
			if (this.CanStartFishing())
			{
				this.SetState(FishingController.State.Aim);
				return;
			}
			return;
		case InputsManager.InputAction.FishingCancelAim:
			if (this.m_Animator.GetInteger(this.m_StateHash) == 5)
			{
				this.SetState(FishingController.State.Idle);
				return;
			}
			return;
		case InputsManager.InputAction.FishingCast:
			if (this.CanCast())
			{
				this.SetState(FishingController.State.Cast);
				return;
			}
			return;
		case InputsManager.InputAction.FishingStrike:
			if (this.m_Animator.GetInteger(this.m_StateHash) == 7)
			{
				this.Strike();
				return;
			}
			return;
		case InputsManager.InputAction.FishingReel:
			break;
		case InputsManager.InputAction.FishingTakeFish:
			this.TakeFish();
			return;
		default:
			if (action != InputsManager.InputAction.AdditionalQuit)
			{
				return;
			}
			break;
		}
		if (this.m_Animator.GetInteger(this.m_StateHash) == 7)
		{
			this.Cancel();
			return;
		}
	}

	public void OnJump()
	{
		this.SetState(FishingController.State.Jump);
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		this.UpdateState();
		bool value = !Inventory3DManager.Get().IsActive() || InventoryBackpack.Get().m_ActivePocket != BackpackPocket.Left;
		this.m_Animator.SetBool(this.m_BackpackRodInHandHash, value);
	}

	private void UpdateState()
	{
		switch (this.m_Animator.GetInteger(this.m_StateHash))
		{
		case 1:
			if (FPPController.Get().IsWalking())
			{
				this.SetState(FishingController.State.Walk);
				return;
			}
			if (FPPController.Get().IsRunning())
			{
				this.SetState(FishingController.State.Run);
				return;
			}
			break;
		case 2:
			if (FPPController.Get().IsRunning())
			{
				this.SetState(FishingController.State.Run);
				return;
			}
			if (!FPPController.Get().IsWalking())
			{
				this.SetState(FishingController.State.Idle);
				return;
			}
			break;
		case 3:
			if (FPPController.Get().IsWalking())
			{
				this.SetState(FishingController.State.Walk);
				return;
			}
			if (!FPPController.Get().IsRunning())
			{
				this.SetState(FishingController.State.Idle);
				return;
			}
			break;
		case 4:
			if (this.IsAnimFinish(this.m_FishingJumpHash))
			{
				this.SetState(FishingController.State.Idle);
				return;
			}
			break;
		case 5:
			this.UpdateAim();
			return;
		case 6:
		case 8:
		case 9:
		case 10:
		case 12:
			break;
		case 7:
		{
			Vector3 normalized2D = (this.m_FishingRod.m_Float.transform.position - base.transform.position).GetNormalized2D();
			if (Vector3.Dot(base.transform.forward.GetNormalized2D(), normalized2D) < 0.2f)
			{
				this.Cancel();
				return;
			}
			break;
		}
		case 11:
			if (this.IsAnimFinish(this.m_FishingFailHash))
			{
				this.SetState(FishingController.State.Idle);
			}
			break;
		default:
			return;
		}
	}

	private bool IsAnimFinish(int hash)
	{
		AnimatorStateInfo currentAnimatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(1);
		return currentAnimatorStateInfo.shortNameHash == hash && currentAnimatorStateInfo.normalizedTime >= 1f;
	}

	private void UpdateAim()
	{
		this.UpdateAimTarget();
		if (!InputsManager.Get().IsActionActive(InputsManager.InputAction.FishingAim))
		{
			this.SetState(FishingController.State.Idle);
		}
	}

	private void UpdateAimTarget()
	{
		Vector3 position = CameraManager.Get().m_MainCamera.transform.position;
		Vector3 forward = CameraManager.Get().m_MainCamera.transform.forward;
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
		this.UpdateFish();
	}

	private void UpdateFish()
	{
		if (this.m_Fish)
		{
			this.m_Fish.transform.position = CameraManager.Get().m_MainCamera.transform.position + CameraManager.Get().m_MainCamera.transform.forward * 0.6f;
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
		this.SetState(FishingController.State.Idle);
		Animator componentDeepChild = General.GetComponentDeepChild<Animator>(item.gameObject);
		if (componentDeepChild != null)
		{
			componentDeepChild.SetBool("Backpack", true);
		}
		HintsManager.Get().HideHint("Cast_FishingRod");
	}

	private void Strike()
	{
		this.SetState(FishingController.State.Strike);
		this.m_FishingRod.Strike();
	}

	private void Cancel()
	{
		this.m_FishingRod.Cancel();
		this.SetState(FishingController.State.Reel);
	}

	private void ShowFish()
	{
		if (!this.m_FishingRod.m_Fish)
		{
			return;
		}
		this.m_Fish = UnityEngine.Object.Instantiate<GameObject>(this.m_FishingRod.m_Fish.GetPrefab());
		Collider component = this.m_Fish.GetComponent<Collider>();
		if (component)
		{
			component.enabled = false;
		}
		HintsManager.Get().ShowHint("Take_Fish", 10f);
		this.m_FishingRod.DestroyFish();
		this.BlockMoves(true);
		this.BlockRotation(true);
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
		FishingController.State integer = (FishingController.State)this.m_Animator.GetInteger(this.m_StateHash);
		if (integer != FishingController.State.Idle)
		{
			if (integer == FishingController.State.Aim)
			{
				actions.Add(23);
				actions.Add(24);
				return;
			}
			if (integer != FishingController.State.Waiting)
			{
				return;
			}
			actions.Add(26);
			actions.Add(25);
			return;
		}
		else
		{
			if (this.m_Fish)
			{
				actions.Add(27);
				return;
			}
			actions.Add(22);
			return;
		}
	}

	private void BlockMoves(bool block)
	{
		if (this.m_MovesBlocked != block)
		{
			if (block)
			{
				Player.Get().BlockMoves();
			}
			else
			{
				Player.Get().UnblockMoves();
			}
			this.m_MovesBlocked = block;
		}
	}

	private void BlockRotation(bool block)
	{
		if (this.m_RotationBlocked != block)
		{
			if (block)
			{
				Player.Get().BlockRotation();
			}
			else
			{
				Player.Get().UnblockRotation();
			}
			this.m_RotationBlocked = block;
		}
	}

	public bool IsFishingInProgress()
	{
		return this.m_State > FishingController.State.Jump;
	}

	public override void OnItemChanged(Item item, Hand hand)
	{
		base.OnItemChanged(item, hand);
		this.SetState(item ? FishingController.State.Idle : FishingController.State.None);
	}

	[HideInInspector]
	public int m_StateHash = Animator.StringToHash("FishingState");

	private int m_BackpackRodInHandHash = Animator.StringToHash("BackpackRodInHand");

	private int m_FishingJumpHash = Animator.StringToHash("FishingJump");

	private int m_FishingRunHash = Animator.StringToHash("FishingRun");

	private int m_FishingWalkHash = Animator.StringToHash("FishingWalk");

	private int m_FishingIdleHash = Animator.StringToHash("FishingIdle");

	private int m_FishingSuccessHash = Animator.StringToHash("FishingSuccess");

	private int m_FishingFailHash = Animator.StringToHash("FishingFail");

	public FishingController.State m_State;

	private float m_EnterStateTime;

	private Vector3 m_Target = Vector3.zero;

	[HideInInspector]
	public FishingRod m_FishingRod;

	private float m_MaxRange = 8f;

	[HideInInspector]
	public GameObject m_Fish;

	private bool m_MovesBlocked;

	private bool m_RotationBlocked;

	private static FishingController s_Instance;

	public enum State
	{
		None,
		Idle,
		Walk,
		Run,
		Jump,
		Aim,
		Cast,
		Waiting,
		Reel,
		Strike,
		Empty,
		Fail,
		Presentation
	}
}
