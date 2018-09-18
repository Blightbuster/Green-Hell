using System;
using CJTools;
using Enums;
using UnityEngine;

public class LadderController : PlayerController
{
	public static LadderController Get()
	{
		return LadderController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		LadderController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.Ladder;
		this.m_Camera = Camera.main;
		this.m_CharacterController = base.GetComponent<CharacterController>();
		this.m_OffsetHelperController = base.GetComponent<OffsetHelperController>();
		this.m_LookController = base.gameObject.GetComponent<LookController>();
	}

	public void SetLadder(Ladder ladder)
	{
		this.m_Ladder = ladder;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_UpPos = this.m_Ladder.gameObject.transform.FindDeepChild("Up").position;
		if ((this.m_Player.gameObject.transform.position - this.m_UpPos).y < 0f)
		{
			this.m_Animator.SetInteger(this.m_ILadder, 1);
			this.m_State = LadderControllerState.PreEnterDown;
		}
		else
		{
			this.m_Animator.SetInteger(this.m_ILadder, 9);
			this.m_State = LadderControllerState.PreEnterUp;
		}
		this.m_Player.GetComponent<Rigidbody>().useGravity = false;
		this.m_Player.GetComponent<Rigidbody>().isKinematic = true;
		this.SetupCollisions(false);
		this.SetupWantedPos();
		this.SetupWantedDir();
		this.m_CharacterControllerCenterToRestore = this.m_CharacterController.center;
		Vector3 center = this.m_CharacterController.center;
		center.x = 0f;
		center.z = 0f;
		this.m_CharacterController.center = center;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetInteger(this.m_ILadder, 0);
		this.m_Player.GetComponent<Rigidbody>().useGravity = true;
		this.m_Player.GetComponent<Rigidbody>().isKinematic = true;
		this.m_State = LadderControllerState.None;
		this.SetupCollisions(true);
		this.m_CharacterController.center = this.m_CharacterControllerCenterToRestore;
	}

	public static Vector3 GetPlayerWantedPos(Ladder ladder, LadderControllerState state)
	{
		if (ladder == null)
		{
			return Vector3.zero;
		}
		if (state == LadderControllerState.PreEnterUp)
		{
			return ladder.transform.position + ladder.transform.forward * ladder.m_PlayerOffset;
		}
		return ladder.transform.position - ladder.transform.forward * ladder.m_PlayerOffset;
	}

	private void SetupWantedPos()
	{
		if (!this.m_Ladder)
		{
			return;
		}
		this.m_WantedPos = LadderController.GetPlayerWantedPos(this.m_Ladder, this.m_State);
	}

	public static Vector3 GetPlayerWantedDir(Ladder ladder)
	{
		if (ladder == null)
		{
			return Vector3.zero;
		}
		return ladder.transform.forward;
	}

	private void SetupWantedDir()
	{
		if (!this.m_Ladder)
		{
			return;
		}
		this.m_WantedDir = LadderController.GetPlayerWantedDir(this.m_Ladder);
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		this.UpdateState();
		this.UpdateAnimator();
		this.UpdateWantedDir();
		this.UpdateLookDev();
	}

	private void UpdateState()
	{
		AnimatorStateInfo currentAnimatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(0);
		if (currentAnimatorStateInfo.shortNameHash == this.m_StatePreEnterDown)
		{
			Vector3 vector = this.m_WantedPos - this.m_Player.gameObject.transform.position;
			vector.y = 0f;
			if (vector.magnitude < 0.03f)
			{
				this.m_State = LadderControllerState.EnterDown;
			}
		}
		else if (currentAnimatorStateInfo.shortNameHash == this.m_StateEnterDown)
		{
			Vector3 vector2 = this.m_WantedPos - this.m_Player.gameObject.transform.position;
			vector2.y = 0f;
			if (vector2.magnitude < 0.03f)
			{
				this.m_State = LadderControllerState.Idle;
			}
		}
		else if (currentAnimatorStateInfo.shortNameHash == this.m_StatePreEnterUp)
		{
			Vector3 vector3 = this.m_WantedPos - this.m_Player.gameObject.transform.position;
			vector3.y = 0f;
			if (Mathf.Abs(vector3.x) < 0.03f && Mathf.Abs(vector3.z) < 0.03f)
			{
				this.m_State = LadderControllerState.EnterUp;
			}
		}
		else if (currentAnimatorStateInfo.shortNameHash == this.m_StateEnterUp)
		{
			this.m_State = LadderControllerState.Idle;
		}
		else if (currentAnimatorStateInfo.shortNameHash == this.m_StateIdle)
		{
			if (this.m_Inputs.m_Up > 0.5f)
			{
				this.m_State = LadderControllerState.MoveUp;
			}
			else if (this.m_Inputs.m_Up < -0.5f)
			{
				this.m_State = LadderControllerState.MoveDown;
			}
		}
		else if (currentAnimatorStateInfo.shortNameHash == this.m_StateMoveUp || currentAnimatorStateInfo.shortNameHash == this.m_StateMoveDown)
		{
			if (this.m_Inputs.m_Up > 0.5f)
			{
				this.m_State = LadderControllerState.MoveUp;
			}
			else if (this.m_Inputs.m_Up < -0.5f)
			{
				this.m_State = LadderControllerState.MoveDown;
			}
			else
			{
				this.m_State = LadderControllerState.Idle;
			}
		}
		if (currentAnimatorStateInfo.shortNameHash == this.m_StateMoveUp && this.m_Player.gameObject.transform.position.y + 1.2f > this.m_UpPos.y)
		{
			this.m_State = LadderControllerState.ExitUp;
		}
		if (currentAnimatorStateInfo.shortNameHash == this.m_StateMoveUp || currentAnimatorStateInfo.shortNameHash == this.m_StateMoveDown || currentAnimatorStateInfo.shortNameHash == this.m_StateExitUp || currentAnimatorStateInfo.shortNameHash == this.m_StateEnterUp)
		{
			this.m_OffsetHelperController.MoveCharacterController(this.m_CharacterController);
		}
		if (currentAnimatorStateInfo.shortNameHash == this.m_StateMoveDown && (this.m_CharacterController.collisionFlags & CollisionFlags.Below) != CollisionFlags.None)
		{
			this.m_Player.StopClimbing();
			this.m_State = LadderControllerState.None;
			this.m_Animator.SetInteger(this.m_ILadder, 0);
			this.Stop();
		}
		if (currentAnimatorStateInfo.shortNameHash == this.m_StateExitUp && currentAnimatorStateInfo.normalizedTime > 0.9f)
		{
			this.m_Player.StopClimbing();
			this.m_State = LadderControllerState.None;
			this.m_Animator.SetInteger(this.m_ILadder, 0);
			this.Stop();
		}
	}

	private void UpdateAnimator()
	{
		this.m_Animator.SetInteger(this.m_ILadder, (int)this.m_State);
	}

	private void FixedUpdate()
	{
		this.SetupWantedPos();
		this.UpdateWantedSpeed();
	}

	private void UpdateWantedSpeed()
	{
		AnimatorStateInfo currentAnimatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(0);
		if (currentAnimatorStateInfo.shortNameHash == this.m_StatePreEnterDown || currentAnimatorStateInfo.shortNameHash == this.m_StateEnterDown || currentAnimatorStateInfo.shortNameHash == this.m_StatePreEnterUp || currentAnimatorStateInfo.shortNameHash == this.m_StateMoveDown || currentAnimatorStateInfo.shortNameHash == this.m_StateMoveUp || currentAnimatorStateInfo.shortNameHash == this.m_StateIdle)
		{
			Vector3 vector = this.m_WantedPos - this.m_Player.transform.position;
			vector.y = 0f;
			if (vector.magnitude > 0.2f)
			{
				vector.Normalize();
			}
			vector *= Time.fixedDeltaTime * 4f;
			this.m_CharacterController.Move(vector);
		}
	}

	private void UpdateWantedDir()
	{
		AnimatorStateInfo currentAnimatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(0);
		if ((currentAnimatorStateInfo.shortNameHash == this.m_StatePreEnterDown || currentAnimatorStateInfo.shortNameHash == this.m_StateEnterDown || currentAnimatorStateInfo.shortNameHash == this.m_StatePreEnterUp || currentAnimatorStateInfo.shortNameHash == this.m_StateIdle || currentAnimatorStateInfo.shortNameHash == this.m_StateMoveUp || currentAnimatorStateInfo.shortNameHash == this.m_StateMoveDown) && this.m_WantedDir.magnitude > 0.1f)
		{
			Quaternion b;
			if (currentAnimatorStateInfo.shortNameHash == this.m_StatePreEnterUp)
			{
				b = Quaternion.LookRotation(-this.m_WantedDir, Vector3.up);
			}
			else
			{
				b = Quaternion.LookRotation(this.m_WantedDir, Vector3.up);
			}
			Quaternion rotation = Quaternion.Slerp(this.m_Player.gameObject.transform.rotation, b, 0.3f);
			this.m_CharacterController.transform.rotation = rotation;
		}
	}

	public override void ControllerLateUpdate()
	{
		base.ControllerLateUpdate();
		this.UpdateInputs();
		this.UpdateCamera();
	}

	private void UpdateCamera()
	{
		Vector3 vector = this.m_Player.GetREyeTransform().position - this.m_Player.GetLEyeTransform().position;
		vector *= 0.5f;
		vector += this.m_Player.GetLEyeTransform().position;
		this.m_Camera.transform.localPosition = vector;
		this.m_Camera.transform.rotation = this.m_Player.GetREyeTransform().rotation;
	}

	private void SetupCollisions(bool set)
	{
		if (this.m_Player == null || this.m_Ladder == null)
		{
			return;
		}
		Collider component = this.m_Player.gameObject.GetComponent<Collider>();
		Collider component2 = this.m_Ladder.gameObject.GetComponent<Collider>();
		if (component == null || component2 == null)
		{
			return;
		}
		Physics.IgnoreCollision(component, component2, !set);
	}

	private void UpdateInputs()
	{
		if (this.m_Player.GetRotationBlocked())
		{
			return;
		}
		this.m_Inputs.m_Up = 0f;
		if (InputsManager.Get().IsActionActive(InputsManager.InputAction.Forward))
		{
			this.m_Inputs.m_Up = 1f;
		}
		else if (InputsManager.Get().IsActionActive(InputsManager.InputAction.Backward))
		{
			this.m_Inputs.m_Up = -1f;
		}
	}

	private void UpdateLookDev()
	{
		this.m_LookController.CalculateLookDev(this.m_CharacterController.transform.forward);
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		if (id == AnimEventID.LadderSound)
		{
			PlayerAudioModule.Get().PlayLadderSound();
		}
		else
		{
			base.OnAnimEvent(id);
		}
	}

	private Ladder m_Ladder;

	private LadderControllerState m_State;

	private CharacterController m_CharacterController;

	private OffsetHelperController m_OffsetHelperController;

	private LookController m_LookController;

	private Camera m_Camera;

	private Vector3 m_WantedPos;

	private Vector3 m_WantedDir;

	private LadderControllerInputs m_Inputs = new LadderControllerInputs();

	private int m_ILadder = Animator.StringToHash("Ladder");

	private int m_StatePreEnterDown = Animator.StringToHash("LadderPreEnterDown");

	private int m_StateEnterDown = Animator.StringToHash("LadderEnterDown");

	private int m_StateEnterUp = Animator.StringToHash("LadderEnterUp");

	private int m_StatePreEnterUp = Animator.StringToHash("LadderPreEnterUp");

	private int m_StateIdle = Animator.StringToHash("LadderIdle");

	private int m_StateMoveUp = Animator.StringToHash("LadderMoveUp");

	private int m_StateMoveDown = Animator.StringToHash("LadderMoveDown");

	private int m_StateExitUp = Animator.StringToHash("LadderExitUp");

	private Vector3 m_UpPos = Vector3.zero;

	private Vector3 m_CharacterControllerCenterToRestore = Vector3.zero;

	private static LadderController s_Instance;
}
