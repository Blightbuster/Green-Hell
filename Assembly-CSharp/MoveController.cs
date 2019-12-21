using System;
using CJTools;
using UnityEngine;

public class MoveController : PlayerController
{
	protected override void Awake()
	{
		base.Awake();
		base.m_ControllerType = PlayerControllerType.Move;
		this.m_AudioModule = base.gameObject.GetComponent<PlayerAudioModule>();
		DebugUtils.Assert(this.m_AudioModule, true);
		this.m_CharacterController = base.gameObject.GetComponent<CharacterControllerProxy>();
		DebugUtils.Assert(this.m_CharacterController, true);
		this.m_FPPController = base.gameObject.GetComponent<FPPController>();
		DebugUtils.Assert(this.m_FPPController, true);
		this.m_LastTimeNoSprint = Time.time;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_DuckAddSpeed.Init(0f, 1f);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (this.m_Animator.isInitialized)
		{
			this.m_Animator.SetFloat("MoveSpeedForward", 0f);
			this.m_Animator.SetFloat("MoveSpeed", 0f);
			this.m_Animator.SetFloat("MoveDirection", 0f);
			this.m_Animator.SetFloat("WantedMoveSpeed", 0f);
		}
		this.m_LastMoveSpeedForward = 0f;
		this.m_LastMoveSpeed = 0f;
		this.m_LastMoveDirection = 0f;
		this.m_LastControllerVelocity = Vector3.zero;
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		this.UpdateAnimator();
		this.UpdateSounds();
	}

	private void UpdateAnimator()
	{
		float num = Mathf.Min(1f, Time.deltaTime * 10f);
		Vector3 vector = Player.Get().transform.InverseTransformDirection(this.m_FPPController.m_CharacterVelocity);
		vector.y = 0f;
		Vector3 vector2 = this.m_LastControllerVelocity + (vector - this.m_LastControllerVelocity) * num;
		if (this.m_FPPController.m_TimeInAir > 0.2f)
		{
			vector2 = Vector3.zero;
			vector = Vector3.zero;
		}
		float num2 = vector2.magnitude.IfAlmostZeroGetZero(0.0001f);
		float num3 = vector.magnitude.IfAlmostZeroGetZero(0.0001f);
		float num4 = (this.m_FPPController.m_CurentMoveSpeed.IfAlmostZeroGetZero(0.0001f) != 0f) ? (vector.z / this.m_FPPController.m_CurentMoveSpeed) : 0f;
		float num5 = num2 / ((vector.z >= 0f) ? this.m_FPPController.m_RunSpeed : (this.m_FPPController.m_RunSpeed * this.m_FPPController.m_BackwardWalkSpeed / this.m_FPPController.m_WalkSpeed));
		float num6 = (num3 != 0f) ? (vector2.x / num3) : 0f;
		if (num4 < -0.1f)
		{
			num6 *= -1f;
		}
		float num7 = (this.m_LastMoveSpeedForward + (num4 - this.m_LastMoveSpeedForward) * num).IfAlmostZeroGetZero(0.0001f);
		float num8 = (this.m_LastMoveSpeed + (num5 - this.m_LastMoveSpeed) * num).IfAlmostZeroGetZero(0.0001f);
		float num9 = (this.m_LastMoveDirection + (num6 - this.m_LastMoveDirection) * num).IfAlmostZeroGetZero(0.0001f);
		if (this.m_FPPController.IsDuck())
		{
			this.m_DuckAddSpeed.target = -2f;
			this.m_DuckAddSpeed.Update(num * 0.75f);
		}
		else
		{
			this.m_DuckAddSpeed.target = 0f;
			this.m_DuckAddSpeed.Update(num);
		}
		this.m_Animator.SetFloat(this.m_MoveSpeedForwardHash, num7);
		this.m_Animator.SetFloat(this.m_MoveSpeedHash, num8 + this.m_DuckAddSpeed.current);
		this.m_Animator.SetFloat(this.m_MoveDirectionHash, num9);
		this.m_Animator.SetFloat(this.m_WantedMoveSpeedHash, this.m_FPPController.m_CurentMoveSpeed);
		this.m_LastMoveSpeedForward = num7;
		this.m_LastMoveSpeed = num8;
		this.m_LastMoveDirection = num9;
		this.m_LastControllerVelocity = vector2;
	}

	private void UpdateSounds()
	{
		this.UpdateBreathingSounds();
	}

	private void UpdateBreathingSounds()
	{
		if (this.m_FPPController.IsRunning())
		{
			if (Time.time - this.m_LastTimeNoSprint > 3f)
			{
				this.m_AudioModule.PlayBreathingSound(1f, false);
				return;
			}
		}
		else
		{
			if (this.m_AudioModule.m_GruntSoundPlayed == PlayerAudioModule.GruntPriority.Breathing && Time.time - this.m_FadeOutBreathLastTime > 3f)
			{
				this.m_AudioModule.StopGruntSound(PlayerAudioModule.GruntPriority.Breathing, 0.2f);
				this.m_FadeOutBreathLastTime = Time.time;
			}
			this.m_LastTimeNoSprint = Time.time;
		}
	}

	private PlayerAudioModule m_AudioModule;

	private CharacterControllerProxy m_CharacterController;

	private FPPController m_FPPController;

	private float m_LastTimeNoSprint;

	private float m_FadeOutBreathLastTime;

	private float m_LastMoveSpeedForward;

	private float m_LastMoveSpeed;

	private float m_LastMoveDirection;

	private Vector3 m_LastControllerVelocity = Vector3.zero;

	private SpringFloat m_DuckAddSpeed;

	private int m_MoveSpeedForwardHash = Animator.StringToHash("MoveSpeedForward");

	private int m_MoveSpeedHash = Animator.StringToHash("MoveSpeed");

	private int m_MoveDirectionHash = Animator.StringToHash("MoveDirection");

	private int m_WantedMoveSpeedHash = Animator.StringToHash("WantedMoveSpeed");
}
