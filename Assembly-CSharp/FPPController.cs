using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CharacterController))]
public class FPPController : PlayerController
{
	public Vector2 GetLookDev()
	{
		return this.m_LookController.m_LookDev;
	}

	public void SetLookDev(Vector2 dev)
	{
		this.m_LookController.m_LookDev = dev;
		this.m_LookController.m_WantedLookDev = dev;
	}

	public static FPPController Get()
	{
		return FPPController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		FPPController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.FPP;
		this.m_Inputs = new FPPControllerInputs();
		this.m_CharacterController = base.GetComponent<CharacterController>();
		this.m_Height = this.m_CharacterController.height;
		this.m_AudioModule = base.gameObject.GetComponent<PlayerAudioModule>();
		DebugUtils.Assert(this.m_AudioModule, true);
		this.m_RigidBody = base.gameObject.GetComponent<Rigidbody>();
		this.m_LookController = base.gameObject.GetComponent<LookController>();
		this.m_BodyRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:Spine"));
		this.m_BodyRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:Spine1"));
		this.m_BodyRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:Spine2"));
		Quaternion rotation = this.m_CharacterController.transform.rotation;
		this.m_LookController.m_LookDev.x = rotation.eulerAngles.y;
		this.m_CurentMoveSpeed = 0f;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_CollisionFlags = CollisionFlags.Below;
		this.m_LastCollisionFlags = CollisionFlags.Below;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (this.m_CharacterController.gameObject.activeSelf)
		{
			this.m_CharacterController.Move(Vector3.zero);
		}
	}

	private void FixedUpdate()
	{
		this.UpdateInputs();
		this.m_LookController.UpdateCharacterControllerCenter();
		this.UpdateWantedSpeed();
		this.UpdateDuck();
	}

	public override void ControllerLateUpdate()
	{
		base.ControllerLateUpdate();
		this.UpdateBodyRotation();
	}

	private void UpdateWantedSpeed()
	{
		Vector2 vector = new Vector2(this.m_Inputs.m_Horizontal, this.m_Inputs.m_Vertical);
		if (vector.sqrMagnitude > 1f)
		{
			vector.Normalize();
		}
		bool flag = this.m_Inputs.m_Sprint;
		float num = 0f;
		this.m_CurrentMoveSpeed = MoveSpeed.Idle;
		bool flag2 = this.m_Player && this.m_Player.GetMovesBlocked();
		if (this.m_RunDepletedStamina)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				this.m_RunDepletedStamina = false;
			}
		}
		if (flag && this.m_RunBlocked)
		{
			flag = false;
		}
		if (!flag2 && vector.magnitude > 0.5f)
		{
			if (flag)
			{
				if (vector.y < -0.5f)
				{
					num = this.m_BackwardWalkSpeed;
					this.m_CurrentMoveSpeed = MoveSpeed.Walk;
				}
				else
				{
					num = this.m_RunSpeed;
					this.m_CurrentMoveSpeed = MoveSpeed.Run;
				}
			}
			else if (vector.magnitude > 0.5f)
			{
				num = ((vector.y < 0f) ? this.m_BackwardWalkSpeed : this.m_WalkSpeed);
				this.m_CurrentMoveSpeed = MoveSpeed.Walk;
			}
			if (this.m_Player && this.m_Player.IsStaminaDepleted())
			{
				num = this.m_WalkSpeed;
				this.m_CurrentMoveSpeed = MoveSpeed.Walk;
				if (flag)
				{
					this.m_RunDepletedStamina = true;
				}
			}
		}
		if (this.m_Duck)
		{
			num *= this.m_DuckSpeedMul;
		}
		Vector3 vector2 = this.m_CharacterController.transform.InverseTransformVector(this.m_WantedSpeed);
		vector2.x = vector.x * num;
		vector2.z = vector.y * num;
		if (this.m_Dodge)
		{
			num = this.m_DodgeSpeed;
			this.m_WantedSpeed = Vector3.zero;
			if (this.m_DodgeDirection != Direction.Backward)
			{
				this.m_WantedSpeed.x = ((this.m_DodgeDirection != Direction.Right) ? -1f : 1f);
			}
			else
			{
				this.m_WantedSpeed.z = -1f;
			}
			if (Time.time - this.m_DodgeStartTime > 0.1f)
			{
				this.m_Dodge = false;
			}
		}
		float y = vector2.y;
		vector2.y = 0f;
		vector2.Normalize();
		vector2 *= num;
		vector2.y = y;
		this.m_CurentMoveSpeed = num;
		if (InventoryBackpack.Get().IsCriticalOverload())
		{
			vector2 *= this.m_OverloadSpeedMul;
		}
		if (InventoryBackpack.Get().IsMaxOverload())
		{
			vector2 *= this.m_MaxOverloadSpeedMul;
		}
		if (this.m_TimeInAir < 0.5f)
		{
			this.m_WantedSpeed = this.m_CharacterController.transform.TransformVector(vector2);
		}
		if ((this.m_CollisionFlags & CollisionFlags.Below) == CollisionFlags.None)
		{
			Vector3 wantedSpeed = this.m_WantedSpeed;
			wantedSpeed.y = 0f;
			Vector3 additionalSpeed = this.m_AdditionalSpeed;
			additionalSpeed.y = 0f;
			float num2 = Vector3.Dot(wantedSpeed, additionalSpeed);
			num2 = Mathf.Clamp(num2, -1f, 1f);
			if (num2 < 0f)
			{
				this.m_WantedSpeed += wantedSpeed * -1f;
			}
		}
		this.m_WantedSpeed += this.m_AdditionalSpeed;
		this.m_AdditionalSpeed = Vector3.zero;
		if (this.m_RigidBody.useGravity)
		{
			this.m_WantedSpeed += Physics.gravity * Time.fixedDeltaTime;
		}
		else
		{
			this.m_WantedSpeed.y = 0f;
		}
		if (this.m_WantedSpeed.y < -10f)
		{
			this.m_WantedSpeed.y = -10f;
		}
		Vector3 b = Vector3.zero;
		if (!this.m_Player.GetMovesBlocked() && this.m_CharacterController.detectCollisions)
		{
			b = -this.m_CharacterController.transform.TransformVector(this.m_LookController.m_CharacterControllerDelta);
		}
		this.m_CollisionFlags = this.m_CharacterController.Move(this.m_WantedSpeed * Time.fixedDeltaTime + b);
		if ((this.m_CollisionFlags & CollisionFlags.Below) != CollisionFlags.None && (this.m_LastCollisionFlags & CollisionFlags.Below) == CollisionFlags.None)
		{
			this.m_Player.OnLanding(this.m_CharacterController.velocity);
		}
		if ((this.m_CollisionFlags & CollisionFlags.Below) != CollisionFlags.None)
		{
			this.m_TimeInAir = 0f;
		}
		else
		{
			this.m_TimeInAir += Time.fixedDeltaTime;
		}
		this.m_LastCollisionFlags = this.m_CollisionFlags;
	}

	public override void OnInputAction(InputsManager.InputAction action)
	{
		base.OnInputAction(action);
		if (LoadingScreen.Get().m_Active)
		{
			return;
		}
		if (InventoryBackpack.Get().IsCriticalOverload())
		{
			return;
		}
		if (action == InputsManager.InputAction.Jump && HUDMovie.Get().GetMovieType() == MovieType.None && (this.m_CollisionFlags & CollisionFlags.Below) != CollisionFlags.None)
		{
			if (InputsManager.Get().IsActionActive(InputsManager.InputAction.DodgeLeft) && !InputsManager.Get().IsActionActive(InputsManager.InputAction.DodgeForward))
			{
				this.Dodge(Direction.Left);
			}
			else if (InputsManager.Get().IsActionActive(InputsManager.InputAction.DodgeRight) && !InputsManager.Get().IsActionActive(InputsManager.InputAction.DodgeForward))
			{
				this.Dodge(Direction.Right);
			}
			else if (InputsManager.Get().IsActionActive(InputsManager.InputAction.DodgeBackward))
			{
				this.Dodge(Direction.Backward);
			}
			else
			{
				this.Jump();
			}
		}
	}

	private void Jump()
	{
		if (PlayerConditionModule.Get().m_Stamina < PlayerConditionModule.Get().GetStaminaDecrease(StaminaDecreaseReason.Jump))
		{
			return;
		}
		if (Player.Get().GetMovesBlocked())
		{
			return;
		}
		this.m_WantedSpeed.y = this.m_JumpSpeed;
		this.m_Player.DecreaseStamina(StaminaDecreaseReason.Jump);
		EventsManager.OnEvent(Enums.Event.Jump, 1);
		this.m_AudioModule.PlayFeetJumpSound(1f, false);
		this.m_AudioModule.PlayJumpSound();
		this.m_Animator.SetTrigger(this.m_TJump);
	}

	private void Dodge(Direction direction)
	{
		if (PlayerConditionModule.Get().IsStaminaCriticalLevel())
		{
			return;
		}
		this.m_DodgeDirection = direction;
		this.m_Player.DecreaseStamina(StaminaDecreaseReason.Dodge);
		EventsManager.OnEvent(Enums.Event.Dodge, 1);
		this.m_AudioModule.PlayFeetJumpSound(1f, false);
		this.m_AudioModule.PlayJumpSound();
		this.m_Animator.SetTrigger((direction != Direction.Left) ? this.m_TDodgeRight : this.m_TDodgeRight);
		this.m_DodgeStartTime = Time.time;
		this.m_Dodge = true;
	}

	private void UpdateDuck()
	{
		if (base.GetComponent<WeaponController>().IsActive() && base.GetComponent<WeaponController>().DuckDuringAttack())
		{
			this.m_Animator.SetBool(this.m_BDuck, true);
			this.m_Duck = true;
		}
		else
		{
			this.m_Animator.SetBool(this.m_BDuck, false);
			this.m_Duck = false;
		}
		if (this.m_Inputs.m_Duck)
		{
			this.m_Duck = true;
		}
		else
		{
			this.m_Duck = false;
		}
	}

	private void UpdateBodyRotation()
	{
		int count = this.m_BodyRotationBones.Count;
		if (count == 0)
		{
			return;
		}
		if (!this.m_Player || !this.m_Player.GetRotationBlocked())
		{
			this.m_LookController.UpdateLookDev(this.m_Inputs.m_MouseX, this.m_Inputs.m_MouseY);
		}
		Quaternion rotation = default(Quaternion);
		rotation = Quaternion.Euler(0f, this.m_LookController.m_LookDev.x, 0f);
		this.m_CharacterController.transform.rotation = rotation;
		float num = 1f / (float)count;
		for (int i = 0; i < count; i++)
		{
			this.m_BodyRotationBonesParams[this.m_BodyRotationBones[i]] = -this.m_LookController.m_LookDev.y * num;
		}
	}

	public override Dictionary<Transform, float> GetBodyRotationBonesParams()
	{
		return this.m_BodyRotationBonesParams;
	}

	public override void OnBlockRotation()
	{
		base.OnBlockRotation();
		this.m_Inputs.Reset(false);
	}

	private void UpdateInputs()
	{
		if (!Player.Get().GetMovesBlocked())
		{
			this.m_Inputs.m_Horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
			this.m_Inputs.m_Vertical = CrossPlatformInputManager.GetAxis("Vertical");
			this.m_Inputs.m_Jump = InputsManager.Get().IsActionActive(InputsManager.InputAction.Jump);
			this.m_Inputs.m_Duck = InputsManager.Get().IsActionActive(InputsManager.InputAction.Duck);
			this.m_Inputs.m_Sprint = InputsManager.Get().IsActionActive(InputsManager.InputAction.Sprint);
		}
		if (!this.m_Player.GetRotationBlocked())
		{
			this.m_Inputs.m_MouseX = CrossPlatformInputManager.GetAxis("Mouse X") * this.m_LookSensitivityX;
			this.m_Inputs.m_MouseY = CrossPlatformInputManager.GetAxis("Mouse Y") * this.m_LookSensitivityY;
		}
	}

	public bool IsWalking()
	{
		return this.m_CurrentMoveSpeed == MoveSpeed.Walk;
	}

	public bool IsRunning()
	{
		return this.m_CurrentMoveSpeed == MoveSpeed.Run;
	}

	public bool IsDepleted()
	{
		return this.m_CurrentMoveSpeed == MoveSpeed.Depleted;
	}

	public bool IsIdle()
	{
		return this.m_CurrentMoveSpeed == MoveSpeed.Idle;
	}

	public bool IsDuck()
	{
		return this.m_Duck;
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (!base.enabled)
		{
			return;
		}
		Vector3 normal = hit.normal;
		if (57.29578f * Mathf.Abs(Mathf.Acos(normal.y)) > this.m_CharacterController.slopeLimit)
		{
			float num = 0.3f;
			this.m_AdditionalSpeed.x = this.m_AdditionalSpeed.x + (1f - normal.y) * normal.x * (1f - num);
			this.m_AdditionalSpeed.z = this.m_AdditionalSpeed.z + (1f - normal.y) * normal.z * (1f - num);
		}
		else if (this.m_AdditionalSpeed.magnitude > 0.03f)
		{
			this.m_AdditionalSpeed *= 0.03f / this.m_AdditionalSpeed.magnitude;
		}
	}

	public MoveSpeed GetCurrentMoveSpeed()
	{
		return this.m_CurrentMoveSpeed;
	}

	public void ScenarioBlockRun()
	{
		this.m_RunBlocked = true;
	}

	public void ScenarioUnblockRun()
	{
		this.m_RunBlocked = false;
	}

	[SerializeField]
	public float m_WalkSpeed = 4f;

	[SerializeField]
	public float m_BackwardWalkSpeed = 2f;

	[SerializeField]
	public float m_RunSpeed = 8f;

	[SerializeField]
	public float m_DepletedSpeed = 20f;

	[SerializeField]
	public float m_DuckSpeedMul = 0.5f;

	[SerializeField]
	private float m_JumpSpeed = 4.5f;

	private float m_DodgeStartTime;

	private Direction m_DodgeDirection = Direction.Backward;

	private float m_DodgeSpeed = 10f;

	private bool m_Dodge;

	[SerializeField]
	private float m_LookSensitivityX = 4f;

	[SerializeField]
	private float m_LookSensitivityY = 2f;

	private CharacterController m_CharacterController;

	[HideInInspector]
	public Vector3 m_WantedSpeed;

	private CollisionFlags m_CollisionFlags;

	public CollisionFlags m_LastCollisionFlags;

	private FPPControllerInputs m_Inputs;

	private bool m_Duck;

	private float m_Height = 1.8f;

	private List<Transform> m_BodyRotationBones = new List<Transform>();

	private MoveSpeed m_CurrentMoveSpeed;

	[HideInInspector]
	public float m_CurentMoveSpeed;

	private float m_OverloadSpeedMul = 0.5f;

	private float m_MaxOverloadSpeedMul = 0.3f;

	private PlayerAudioModule m_AudioModule;

	private Rigidbody m_RigidBody;

	private LookController m_LookController;

	private Dictionary<Transform, float> m_BodyRotationBonesParams = new Dictionary<Transform, float>();

	private Vector3 m_AdditionalSpeed = Vector3.zero;

	private static FPPController s_Instance;

	private int m_BDuck = Animator.StringToHash("Duck");

	private int m_TJump = Animator.StringToHash("Jump");

	private int m_TDodgeLeft = Animator.StringToHash("DodgeLeft");

	private int m_TDodgeRight = Animator.StringToHash("DodgeRight");

	private bool m_RunDepletedStamina;

	[HideInInspector]
	public bool m_RunBlocked;

	private float m_TimeInAir;
}
