using System;
using Enums;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class HUDFireMiniGame : HUDBase
{
	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override void Start()
	{
		base.Start();
		this.m_StartPosX[0] = this.m_Elems[0].rectTransform.position.x;
		this.m_StartPosX[1] = this.m_Elems[1].rectTransform.position.x;
		this.m_PointerInitialPos = this.m_Pointer.rectTransform.position;
	}

	public void Activate(Firecamp firecamp)
	{
		this.m_Firecamp = firecamp;
		this.m_Elems[0].enabled = true;
		this.m_Elems[1].enabled = true;
		this.m_Pointer.enabled = true;
		this.SetState(HUDFireMiniGame.MiniGameState.Begin);
		this.m_MouseX = 0f;
		this.m_CurrentDirection = Direction.Right;
		this.m_Shift = 20f;
		this.m_LeftSinStartTime = Time.time;
		this.m_RightSinStartTime = Time.time;
	}

	private void Deactivate()
	{
		this.m_Firecamp = null;
		Player.Get().UnblockRotation();
		Player.Get().UnblockMoves();
		this.m_Elems[0].enabled = false;
		this.m_Elems[1].enabled = false;
		this.m_Pointer.enabled = false;
		this.SetState(HUDFireMiniGame.MiniGameState.None);
	}

	protected override bool ShouldShow()
	{
		return this.m_State != HUDFireMiniGame.MiniGameState.None;
	}

	private void SetState(HUDFireMiniGame.MiniGameState state)
	{
		this.m_State = state;
		this.m_StartStateTime = Time.time;
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateState();
	}

	private void UpdateState()
	{
		HUDFireMiniGame.MiniGameState state = this.m_State;
		if (state != HUDFireMiniGame.MiniGameState.Begin)
		{
			if (state != HUDFireMiniGame.MiniGameState.Game)
			{
				if (state == HUDFireMiniGame.MiniGameState.Finish)
				{
					if (Time.time - this.m_StartStateTime >= this.m_FinishDuration)
					{
						this.Deactivate();
					}
				}
			}
			else
			{
				this.UpdateInputs();
				this.UpdateMouseMoves();
				this.UpdatePosition();
				this.UpdateColor();
				if (this.m_Shift >= this.m_MaxShift)
				{
					this.m_Firecamp.StartBurning();
					this.m_Elems[0].color = Color.yellow;
					this.m_Elems[1].color = Color.yellow;
					this.SetState(HUDFireMiniGame.MiniGameState.Finish);
				}
			}
		}
		else
		{
			this.UpdatePosition();
			Player.Get().BlockRotation();
			Player.Get().BlockMoves();
			this.SetState(HUDFireMiniGame.MiniGameState.Game);
		}
	}

	private void UpdateInputs()
	{
		if (InputsManager.Get().IsActionActive(InputsManager.InputAction.Quit))
		{
			this.Deactivate();
		}
	}

	private void UpdateMouseMoves()
	{
		float num = CrossPlatformInputManager.GetAxis("Mouse X") * 10f;
		this.m_MouseX += num;
		if (this.m_CurrentDirection == Direction.Left)
		{
			if (this.m_MouseX < -this.m_Shift)
			{
				float num2 = this.GetSinMul(this.m_CurrentDirection);
				if (num2 < 0.8f)
				{
					num2 *= 0.3f;
				}
				this.m_Shift += this.m_ShiftIncrease * num2;
				this.m_CurrentDirection = Direction.Right;
				this.m_RightSinStartTime = Time.time;
			}
		}
		else if (this.m_CurrentDirection == Direction.Right && this.m_MouseX > this.m_Shift)
		{
			float num3 = this.GetSinMul(this.m_CurrentDirection);
			if (num3 < 0.8f)
			{
				num3 *= 0.3f;
			}
			this.m_Shift += this.m_ShiftIncrease * num3;
			this.m_CurrentDirection = Direction.Left;
			this.m_LeftSinStartTime = Time.time;
		}
		this.m_Shift -= this.m_ShiftDecreaseSpeedPerSec * Time.deltaTime;
		this.m_Shift = Mathf.Clamp(this.m_Shift, 0f, this.m_MaxShift * 2f);
		this.m_MouseX = Mathf.Clamp(this.m_MouseX, -this.m_Shift, this.m_Shift);
		this.m_DebugText.text = "MouseX = " + this.m_MouseX.ToString();
		Text debugText = this.m_DebugText;
		debugText.text = debugText.text + "\nShift = " + this.m_Shift.ToString();
	}

	private void UpdatePosition()
	{
		Vector3 position = this.m_Elems[0].rectTransform.position;
		position.x = this.m_StartPosX[0] - this.m_Shift;
		this.m_Elems[0].rectTransform.position = position;
		position = this.m_Elems[1].rectTransform.position;
		position.x = this.m_StartPosX[1] + this.m_Shift;
		this.m_Elems[1].rectTransform.position = position;
		position = this.m_PointerInitialPos;
		position.x += this.m_MouseX;
		this.m_Pointer.rectTransform.position = position;
	}

	private void UpdateColor()
	{
		Color white = Color.white;
		float num = 1f - this.GetSinMul(this.m_CurrentDirection);
		white.r = num;
		white.b = num;
		this.m_Elems[0].color = ((this.m_CurrentDirection != Direction.Left) ? Color.white : white);
		this.m_Elems[1].color = ((this.m_CurrentDirection != Direction.Right) ? Color.white : white);
	}

	private float GetSinMul(Direction dir)
	{
		if (dir == Direction.Left)
		{
			return Mathf.Abs(Mathf.Sin((Time.time - this.m_LeftSinStartTime) * 3f));
		}
		return Mathf.Abs(Mathf.Sin((Time.time - this.m_RightSinStartTime) * 3f));
	}

	private HUDFireMiniGame.MiniGameState m_State;

	public RawImage[] m_Elems = new RawImage[2];

	private float[] m_StartPosX = new float[2];

	private Firecamp m_Firecamp;

	public Direction m_CurrentDirection = Direction.Right;

	public float m_Shift = 10f;

	private float m_ShiftIncrease = 2f;

	private float m_ShiftDecreaseSpeedPerSec = 3f;

	private float m_MaxShift = 100f;

	public bool m_MouseOK;

	public float m_Progress;

	private float m_StartStateTime;

	private float m_FinishDuration = 2f;

	private float m_MouseX;

	public Text m_DebugText;

	public RawImage m_Pointer;

	private Vector3 m_PointerInitialPos;

	private float m_LeftSinStartTime;

	private float m_RightSinStartTime;

	private enum MiniGameState
	{
		None,
		Begin,
		Game,
		Finish
	}
}
