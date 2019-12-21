using System;
using UnityEngine;

public class FreeHandsLadderController : PlayerController
{
	public static FreeHandsLadderController Get()
	{
		return FreeHandsLadderController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		FreeHandsLadderController.s_Instance = this;
		base.m_ControllerType = PlayerControllerType.FreeHandsLadder;
		this.m_CharacterController = base.GetComponent<CharacterControllerProxy>();
	}

	public void SetLadder(FreeHandsLadder ladder)
	{
		this.m_Ladder = ladder;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		Vector3 vector = Vector3.zero;
		vector.x = this.m_Inputs.m_Dir.x;
		vector.y = this.m_Inputs.m_Dir.y;
		vector.z = this.m_Inputs.m_Dir.y;
		if ((this.m_CollFlags & CollisionFlags.Below) == CollisionFlags.None)
		{
			if (vector.z < 0f)
			{
				vector.z = 0f;
			}
			if (vector.z > 0f)
			{
				Vector3 rhs = this.m_CharacterController.transform.position - this.m_Ladder.transform.position;
				rhs.y = 0f;
				rhs.Normalize();
				if (Vector3.Dot(this.m_CharacterController.transform.forward, rhs) > 0.5f)
				{
					vector.z = 0f;
					vector.y = -this.m_Inputs.m_Dir.y;
				}
			}
		}
		else if (vector.z > 0f)
		{
			Vector3 rhs2 = this.m_CharacterController.transform.position - this.m_Ladder.transform.position;
			rhs2.y = 0f;
			rhs2.Normalize();
			if (Vector3.Dot(this.m_CharacterController.transform.forward, rhs2) > 0.5f)
			{
				vector.y = -this.m_Inputs.m_Dir.y;
			}
		}
		vector = this.m_CharacterController.transform.TransformVector(vector);
		this.m_CollFlags = this.m_CharacterController.Move(vector * 2f * Time.deltaTime, true);
	}

	public override void ControllerLateUpdate()
	{
		base.ControllerLateUpdate();
		this.UpdateInputs();
	}

	private void UpdateInputs()
	{
		if (this.m_Player.GetRotationBlocked())
		{
			return;
		}
		this.m_Inputs.m_Dir = Vector2.zero;
		if (GreenHellGame.IsPCControllerActive())
		{
			if (InputsManager.Get().IsActionActive(InputsManager.InputAction.Forward))
			{
				this.m_Inputs.m_Dir.y = 1f;
			}
			else if (InputsManager.Get().IsActionActive(InputsManager.InputAction.Backward))
			{
				this.m_Inputs.m_Dir.y = -1f;
			}
			if (InputsManager.Get().IsActionActive(InputsManager.InputAction.Left))
			{
				this.m_Inputs.m_Dir.x = -1f;
				return;
			}
			if (InputsManager.Get().IsActionActive(InputsManager.InputAction.Right))
			{
				this.m_Inputs.m_Dir.x = 1f;
				return;
			}
		}
		else
		{
			float num = 0.5f;
			if (InputsManager.Get().GetActionValue(InputsManager.InputAction.Forward) > num)
			{
				this.m_Inputs.m_Dir.y = 1f;
			}
			else if (InputsManager.Get().GetActionValue(InputsManager.InputAction.Backward) > num)
			{
				this.m_Inputs.m_Dir.y = -1f;
			}
			if (InputsManager.Get().GetActionValue(InputsManager.InputAction.Left) > num)
			{
				this.m_Inputs.m_Dir.x = -1f;
				return;
			}
			if (InputsManager.Get().GetActionValue(InputsManager.InputAction.Right) > num)
			{
				this.m_Inputs.m_Dir.x = 1f;
			}
		}
	}

	private static FreeHandsLadderController s_Instance;

	private FreeHandsLadder m_Ladder;

	private FreeHandsLadderControllerInputs m_Inputs = new FreeHandsLadderControllerInputs();

	private CharacterControllerProxy m_CharacterController;

	private CollisionFlags m_CollFlags;
}
