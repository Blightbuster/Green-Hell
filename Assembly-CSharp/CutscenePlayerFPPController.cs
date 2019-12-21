using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class CutscenePlayerFPPController : CutscenePlayerController
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

	public static CutscenePlayerFPPController Get()
	{
		return CutscenePlayerFPPController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		CutscenePlayerFPPController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.FPP;
		this.m_Inputs = new FPPControllerInputs();
		this.m_CharacterController = base.GetComponent<CharacterController>();
		this.m_Height = this.m_CharacterController.height;
		this.m_RigidBody = base.gameObject.GetComponent<Rigidbody>();
		this.m_LookController = base.gameObject.GetComponent<CutscenePlayerLookController>();
		this.m_BodyRotationBones.Add(base.gameObject.transform.FindDeepChild("spine01"));
		this.m_BodyRotationBones.Add(base.gameObject.transform.FindDeepChild("spine02"));
		this.m_BodyRotationBones.Add(base.gameObject.transform.FindDeepChild("spine03"));
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
		this.UpdateWantedSpeed();
		this.UpdateDuck();
	}

	public override void Update()
	{
		base.Update();
		this.UpdateInputs();
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		this.UpdateBodyRotation();
	}

	private void UpdateWantedSpeed()
	{
		Vector2 vector = new Vector2(this.m_Inputs.m_Horizontal, this.m_Inputs.m_Vertical);
		if (vector.sqrMagnitude > 1f)
		{
			vector.Normalize();
		}
		bool sprint = this.m_Inputs.m_Sprint;
		float num = 0f;
		this.m_CurrentMoveSpeed = MoveSpeed.Idle;
		if (!false && vector.magnitude > 0.5f)
		{
			if (sprint)
			{
				num = this.m_RunSpeed;
				this.m_CurrentMoveSpeed = MoveSpeed.Run;
			}
			else if (vector.magnitude > 0.5f)
			{
				num = ((vector.y >= 0f) ? this.m_WalkSpeed : this.m_BackwardWalkSpeed);
				this.m_CurrentMoveSpeed = MoveSpeed.Walk;
			}
		}
		if (this.m_Duck)
		{
			num *= this.m_DuckSpeedMul;
		}
		Vector3 vector2 = this.m_CharacterController.transform.InverseTransformVector(this.m_WantedSpeed);
		vector2.x = vector.x * num;
		vector2.z = vector.y * num;
		float y = vector2.y;
		vector2.y = 0f;
		vector2.Normalize();
		vector2 *= num;
		vector2.y = y;
		this.m_CurentMoveSpeed = num;
		if ((this.m_CollisionFlags & CollisionFlags.Below) != CollisionFlags.None)
		{
			this.m_WantedSpeed = this.m_CharacterController.transform.TransformVector(vector2);
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
		this.UpdateJump();
		if (this.m_WantedSpeed.y < -10f)
		{
			this.m_WantedSpeed.y = -10f;
		}
		this.m_CollisionFlags = this.m_CharacterController.Move(this.m_WantedSpeed * Time.fixedDeltaTime);
		this.m_LastCollisionFlags = this.m_CollisionFlags;
	}

	private void UpdateJump()
	{
		if (this.m_Inputs.m_Jump && (this.m_CollisionFlags & CollisionFlags.Below) != CollisionFlags.None)
		{
			this.Jump();
		}
	}

	private void Jump()
	{
		this.m_WantedSpeed.y = this.m_JumpSpeed;
		EventsManager.OnEvent(Enums.Event.Jump, 1);
	}

	private void UpdateDuck()
	{
		if (this.m_Inputs.m_Duck)
		{
			this.m_Animator.SetBool(this.m_BDuck, true);
			this.m_Duck = true;
			return;
		}
		this.m_Animator.SetBool(this.m_BDuck, false);
		this.m_Duck = false;
	}

	private void UpdateBodyRotation()
	{
		int count = this.m_BodyRotationBones.Count;
		if (count == 0)
		{
			return;
		}
		if (!false)
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

	private void UpdateInputs()
	{
		this.m_Inputs.m_Horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
		this.m_Inputs.m_Vertical = CrossPlatformInputManager.GetAxis("Vertical");
		this.m_Inputs.m_Jump = CrossPlatformInputManager.GetButton("Jump");
		this.m_Inputs.m_Duck = CrossPlatformInputManager.GetButton("Duck");
		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			this.m_Inputs.m_Sprint = !this.m_Inputs.m_Sprint;
		}
		Vector2 lookInput = InputHelpers.GetLookInput(this.m_LookSensitivityX, this.m_LookSensitivityY, 150f);
		this.m_Inputs.m_MouseX = lookInput.x;
		this.m_Inputs.m_MouseY = lookInput.y;
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
		Vector3 normal = hit.normal;
		if (57.29578f * Mathf.Abs(Mathf.Acos(normal.y)) > this.m_CharacterController.slopeLimit)
		{
			float num = 0.3f;
			this.m_AdditionalSpeed.x = this.m_AdditionalSpeed.x + (1f - normal.y) * normal.x * (1f - num);
			this.m_AdditionalSpeed.z = this.m_AdditionalSpeed.z + (1f - normal.y) * normal.z * (1f - num);
		}
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

	private Rigidbody m_RigidBody;

	private CutscenePlayerLookController m_LookController;

	private Dictionary<Transform, float> m_BodyRotationBonesParams = new Dictionary<Transform, float>();

	private Vector3 m_AdditionalSpeed = Vector3.zero;

	private static CutscenePlayerFPPController s_Instance;

	private int m_BDuck = Animator.StringToHash("Duck");
}
