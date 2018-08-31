using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.Audio;
using UnityStandardAssets.CrossPlatformInput;

public class SwimController : PlayerController
{
	public static SwimController Get()
	{
		return SwimController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		SwimController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.Swim;
		this.m_LookController = base.gameObject.GetComponent<LookController>();
		this.m_CharacterController = base.GetComponent<CharacterController>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Animator.SetBool(this.m_BSwim, true);
		this.m_Player.GetComponent<Rigidbody>().useGravity = false;
		this.m_Player.GetComponent<Rigidbody>().isKinematic = true;
		this.m_DiveBone = this.m_Player.gameObject.transform.FindDeepChild("mixamorig:Spine");
		this.m_SwimBones.Add(this.m_Player.gameObject.transform.FindDeepChild("mixamorig:Neck"));
		this.m_SwimBones.Add(this.m_Player.gameObject.transform.FindDeepChild("mixamorig:Head"));
		this.m_WantedSpeed = this.m_CharacterController.velocity;
		this.m_SwimBonesRotation = this.m_LookController.m_LookDev.y;
		this.m_DiveBonesRotation = 0f;
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().Deactivate();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetBool(this.m_BSwim, false);
		this.m_Animator.SetFloat(this.m_FSwimSpeedLong, 0f);
		this.m_Animator.SetFloat(this.m_FSwimSpeedLeft, 0f);
		this.m_Animator.SetFloat(this.m_FSwimSpeedRight, 0f);
		this.m_Player.GetComponent<Rigidbody>().useGravity = true;
		this.m_Player.GetComponent<Rigidbody>().isKinematic = true;
	}

	private void UpdateInputs()
	{
		if (this.m_Player.GetRotationBlocked())
		{
			return;
		}
		this.m_Inputs.m_Horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
		this.m_Inputs.m_Vertical = CrossPlatformInputManager.GetAxis("Vertical");
		this.m_Inputs.m_MouseX = CrossPlatformInputManager.GetAxis("Mouse X") * this.m_LookSensitivityX;
		this.m_Inputs.m_MouseY = CrossPlatformInputManager.GetAxis("Mouse Y") * this.m_LookSensitivityY;
	}

	private void FixedUpdate()
	{
		this.UpdateWantedSpeed();
	}

	private void UpdateWantedSpeed()
	{
		Vector2 vector = new Vector2(this.m_Inputs.m_Horizontal, this.m_Inputs.m_Vertical);
		Vector3 vector2 = this.m_CharacterController.transform.InverseTransformVector(this.m_WantedSpeed);
		if (this.m_State == SwimState.Swim)
		{
			vector2.x = vector.x * 0.8f;
			vector2.z = vector.y * 2f;
			this.m_WantedSpeed = this.m_CharacterController.transform.TransformVector(vector2);
			float num = Player.DEEP_WATER + 0.3f;
			float waterLevel = this.m_Player.GetWaterLevel();
			Vector3 position = this.m_Player.transform.position;
			Vector3 a = position;
			a.y = waterLevel - num;
			vector2.y = (a - position).y;
		}
		else if (this.m_State == SwimState.Dive)
		{
			Vector3 vector3 = new Vector3(0f, 0f, 1f);
			vector3 = Quaternion.Euler(-this.m_LookController.m_LookDev.y, 0f, 0f) * vector3;
			vector3 *= vector.y * 2f;
			vector3.x = vector.x * 0.8f;
			vector2 = vector3;
		}
		Vector3 a2 = this.m_CharacterController.transform.TransformVector(vector2);
		this.m_WantedSpeed += (a2 - this.m_WantedSpeed) * Time.fixedDeltaTime;
		this.m_CharacterController.Move(this.m_WantedSpeed * Time.fixedDeltaTime);
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		this.UpdateInputs();
		this.UpdateState();
		this.UpdateAnimator();
	}

	private void UpdateState()
	{
		if (this.m_State == SwimState.Swim)
		{
			if (this.m_LookController.m_LookDev.y < -this.m_MinAngleToDive && this.m_Inputs.m_Vertical > 0.5f)
			{
				this.m_State = SwimState.Dive;
				PlayerAudioModule.Get().PlayBeforeDivingSound();
				this.UpdateAudioMixer(true);
			}
		}
		else if (this.m_State == SwimState.Dive && this.m_Player.transform.position.y + Player.DEEP_WATER * 0.9f > this.m_Player.GetWaterLevel())
		{
			this.m_State = SwimState.Swim;
			this.UpdateAudioMixer(false);
		}
	}

	private void UpdateAudioMixer(bool m_IsUnderWater)
	{
		if (m_IsUnderWater && this.m_AudioMixer != null)
		{
			this.m_AudioMixer.SetFloat("MyExposedParam", 2000f);
		}
		else if (!m_IsUnderWater && this.m_AudioMixer != null)
		{
			this.m_AudioMixer.SetFloat("MyExposedParam", 22000f);
		}
	}

	private void UpdateAnimator()
	{
		if (this.m_Inputs.m_Vertical > 0.5f)
		{
			this.m_Animator.SetFloat(this.m_FSwimSpeedLong, 2f);
			this.m_Animator.SetFloat(this.m_FSwimSpeedLeft, 0f);
			this.m_Animator.SetFloat(this.m_FSwimSpeedRight, 0f);
		}
		else if (this.m_Inputs.m_Vertical < -0.5f)
		{
			this.m_Animator.SetFloat(this.m_FSwimSpeedLong, -2f);
			this.m_Animator.SetFloat(this.m_FSwimSpeedLeft, 0f);
			this.m_Animator.SetFloat(this.m_FSwimSpeedRight, 0f);
		}
		else
		{
			this.m_Animator.SetFloat(this.m_FSwimSpeedLong, 0f);
			if (this.m_Inputs.m_Horizontal > 0.5f)
			{
				this.m_Animator.SetFloat(this.m_FSwimSpeedRight, 0.8f);
				this.m_Animator.SetFloat(this.m_FSwimSpeedLeft, 0f);
			}
			else if (this.m_Inputs.m_Horizontal < -0.5f)
			{
				this.m_Animator.SetFloat(this.m_FSwimSpeedRight, 0f);
				this.m_Animator.SetFloat(this.m_FSwimSpeedLeft, 0.8f);
			}
			else
			{
				this.m_Animator.SetFloat(this.m_FSwimSpeedRight, 0f);
				this.m_Animator.SetFloat(this.m_FSwimSpeedLeft, 0f);
			}
		}
	}

	public override void ControllerLateUpdate()
	{
		base.ControllerLateUpdate();
		this.UpdateBodyRotation();
	}

	private void UpdateBodyRotation()
	{
		if (!this.m_Player || !this.m_Player.GetRotationBlocked())
		{
			this.m_LookController.UpdateLookDev(this.m_Inputs.m_MouseX, this.m_Inputs.m_MouseY);
		}
		Quaternion rotation = default(Quaternion);
		rotation = Quaternion.Euler(0f, this.m_LookController.m_LookDev.x, 0f);
		this.m_CharacterController.transform.rotation = rotation;
		float num = 1f / (float)this.m_SwimBones.Count;
		if (this.m_State == SwimState.Swim)
		{
			this.m_SwimBonesRotation += (this.m_LookController.m_LookDev.y - this.m_SwimBonesRotation) * 0.1f;
			this.m_DiveBonesRotation *= 0.9f;
		}
		else if (this.m_State == SwimState.Dive)
		{
			this.m_SwimBonesRotation *= 0.9f;
			this.m_DiveBonesRotation += (this.m_LookController.m_LookDev.y - this.m_DiveBonesRotation) * 0.1f;
		}
		for (int i = 0; i < this.m_SwimBones.Count; i++)
		{
			this.m_BodyRotationBonesParams[this.m_SwimBones[i]] = -this.m_SwimBonesRotation * num;
		}
		this.m_BodyRotationBonesParams[this.m_DiveBone] = -this.m_DiveBonesRotation;
	}

	public override void OnBlockRotation()
	{
		base.OnBlockRotation();
		this.m_Inputs.Reset();
	}

	public SwimState GetState()
	{
		return this.m_State;
	}

	public override Dictionary<Transform, float> GetBodyRotationBonesParams()
	{
		return this.m_BodyRotationBonesParams;
	}

	public bool IsSwimming()
	{
		return base.enabled && this.m_WantedSpeed.magnitude > 0.1f;
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (this.m_State == SwimState.Dive && (id == AnimEventID.DiveSound0 || id == AnimEventID.DiveSound1 || id == AnimEventID.DiveSound2))
		{
			PlayerAudioModule.Get().PlayDiveSound();
		}
		else if (this.m_State == SwimState.Swim && (id == AnimEventID.SwimSound0 || id == AnimEventID.SwimSound1 || id == AnimEventID.SwimSound2))
		{
			PlayerAudioModule.Get().PlaySwimSound();
		}
	}

	private const float SWIM_SPEED_FORWARD = 2f;

	private const float SWIM_SPEED_BACKWARD = 2f;

	private const float SWIM_SPEED_LEFT = 0.8f;

	private const float SWIM_SPEED_RIGHT = 0.8f;

	public SwimState m_State;

	private LookController m_LookController;

	private CharacterController m_CharacterController;

	private SwimControllerInputs m_Inputs = new SwimControllerInputs();

	[SerializeField]
	private float m_LookSensitivityX = 2f;

	[SerializeField]
	private float m_LookSensitivityY = 2f;

	private Vector3 m_WantedSpeed;

	private Transform m_DiveBone;

	private List<Transform> m_SwimBones = new List<Transform>();

	private float m_SwimBonesRotation;

	private float m_DiveBonesRotation;

	private float m_MinAngleToDive = 40f;

	private Dictionary<Transform, float> m_BodyRotationBonesParams = new Dictionary<Transform, float>();

	private int m_BSwim = Animator.StringToHash("Swim");

	private int m_FSwimSpeedLong = Animator.StringToHash("SwimSpeedLong");

	private int m_FSwimSpeedLeft = Animator.StringToHash("SwimSpeedLeft");

	private int m_FSwimSpeedRight = Animator.StringToHash("SwimSpeedRight");

	private static SwimController s_Instance;

	public AudioMixer m_AudioMixer;
}
