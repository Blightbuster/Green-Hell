using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class LiquidInHandsController : PlayerController
{
	public static LiquidInHandsController Get()
	{
		return LiquidInHandsController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		LiquidInHandsController.s_Instance = this;
		base.m_ControllerType = PlayerControllerType.LiquidInHands;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Animator.SetBool(this.m_CarryingWaterHash, true);
		this.m_Container = (LiquidContainer)this.m_Player.GetCurrentItem(Hand.Right);
		DebugUtils.Assert(this.m_Container != null, "[LiquidInHandsController:OnEnable] ERROR - Currentitem is not a LiquidContainer!", true, DebugUtils.AssertType.Info);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetBool(this.m_CarryingWaterHash, false);
		if (!this.m_Container.IsEmpty())
		{
			this.Spill(-1f);
		}
		if (this.m_Container != null)
		{
			UnityEngine.Object.Destroy(this.m_Container.gameObject);
		}
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		this.UpdateMoving();
		this.CheckContainer();
	}

	private void UpdateWaterScale()
	{
		if (this.m_Container != null)
		{
			Vector3 scale = this.m_Scale;
			scale.y *= this.m_Container.m_LCInfo.m_Amount / this.m_Container.m_LCInfo.m_Capacity;
			this.m_Container.transform.localScale = scale;
		}
	}

	private void UpdateMoving()
	{
		if (this.m_Player.IsWalking())
		{
			this.m_SpillAmount += Time.deltaTime * this.m_WalkingSpillPerSec;
		}
		else if (this.m_Player.IsRunning())
		{
			this.m_SpillAmount += Time.deltaTime * this.m_RunningSpillPerSec;
		}
		else
		{
			this.m_SpillAmount = 0f;
			this.m_LastMoveSpill = Time.time;
		}
		if (this.m_SpillAmount > 0f && Time.time - this.m_LastMoveSpill >= this.m_MoveSpillInterval)
		{
			this.Spill(this.m_SpillAmount);
			this.m_LastMoveSpill = Time.time;
			this.m_MoveSpillInterval = UnityEngine.Random.Range(this.m_MoveSpillIntervalMin, this.m_MoveSpillIntervalMax);
		}
	}

	public override void OnInputAction(InputActionData action_data)
	{
		if (action_data.m_Action == InputsManager.InputAction.Quit || action_data.m_Action == InputsManager.InputAction.AdditionalQuit)
		{
			this.Spill(-1f);
			return;
		}
		if (action_data.m_Action == InputsManager.InputAction.WaterDrink)
		{
			this.Drink();
		}
	}

	private void Drink()
	{
		this.m_Container.Drink();
		this.CheckContainer();
	}

	public void Spill(float amount = -1f)
	{
		this.m_Container.Spill(amount);
		PlayerAudioModule.Get().PlayWaterSpillSound(1f, false);
		this.CheckContainer();
	}

	public void TakeLiquid(LiquidSource source)
	{
		if (!this.m_Container)
		{
			this.m_Container = (ItemsManager.Get().CreateItem(ItemID.Water_In_Hands, false, Vector3.zero, Quaternion.identity) as LiquidContainer);
			this.m_Scale = this.m_Container.transform.localScale;
		}
		this.m_Container.Fill(source);
		Player.Get().SetWantedItem(Hand.Right, this.m_Container, true);
		this.m_LastMoveSpill = Time.time;
		this.m_MoveSpillInterval = UnityEngine.Random.Range(this.m_MoveSpillIntervalMin, this.m_MoveSpillIntervalMax);
	}

	public override void GetInputActions(ref List<int> actions)
	{
		actions.Add(70);
		actions.Add(21);
	}

	private void CheckContainer()
	{
		if (this.m_Container == null || this.m_Container.IsEmpty())
		{
			this.Stop();
		}
	}

	private int m_CarryingWaterHash = Animator.StringToHash("CarryingWater");

	public LiquidContainer m_Container;

	private static LiquidInHandsController s_Instance;

	public float m_WalkingSpillPerSec = 10f;

	public float m_RunningSpillPerSec = 20f;

	private Vector3 m_Scale = Vector3.zero;

	private float m_SpillAmount;

	private float m_LastMoveSpill;

	private float m_MoveSpillInterval;

	private float m_MoveSpillIntervalMin = 0.5f;

	private float m_MoveSpillIntervalMax = 1.5f;
}
