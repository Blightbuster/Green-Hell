using System;
using UnityEngine;

public class MoveController : PlayerController
{
	protected override void Awake()
	{
		base.Awake();
		this.m_ControllerType = PlayerControllerType.Move;
		this.m_AudioModule = base.gameObject.GetComponent<PlayerAudioModule>();
		DebugUtils.Assert(this.m_AudioModule, true);
		this.m_CharacterController = base.gameObject.GetComponent<CharacterController>();
		DebugUtils.Assert(this.m_CharacterController, true);
		this.m_FPPController = base.gameObject.GetComponent<FPPController>();
		DebugUtils.Assert(this.m_FPPController, true);
		this.m_LastTimeNoSprint = Time.time;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_DuckAddSpeed = 0f;
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
		float deltaTime = Time.deltaTime;
		float num = 10f;
		Vector3 a = Player.Get().transform.InverseTransformDirection(this.m_FPPController.m_WantedSpeed);
		a.y = 0f;
		Vector3 lastControllerVelocity = this.m_LastControllerVelocity + (a - this.m_LastControllerVelocity) * deltaTime * num;
		float magnitude = lastControllerVelocity.magnitude;
		float num2 = (this.m_FPPController.m_CurentMoveSpeed == 0f) ? 0f : (a.z / this.m_FPPController.m_CurentMoveSpeed);
		float num3 = lastControllerVelocity.magnitude / ((a.z < 0f) ? (this.m_FPPController.m_RunSpeed * this.m_FPPController.m_BackwardWalkSpeed / this.m_FPPController.m_WalkSpeed) : this.m_FPPController.m_RunSpeed);
		float num4 = (a.magnitude == 0f) ? 0f : (lastControllerVelocity.x / a.magnitude);
		if (num2 < -0.1f)
		{
			num4 *= -1f;
		}
		float num5 = this.m_LastMoveSpeedForward + (num2 - this.m_LastMoveSpeedForward) * deltaTime * num;
		float num6 = this.m_LastMoveSpeed + (num3 - this.m_LastMoveSpeed) * deltaTime * num;
		float num7 = this.m_LastMoveDirection + (num4 - this.m_LastMoveDirection) * deltaTime * num;
		if (this.m_FPPController.IsDuck())
		{
			this.m_DuckAddSpeed += (-1f - num6) * deltaTime * num * 0.2f;
		}
		else
		{
			this.m_DuckAddSpeed += -this.m_DuckAddSpeed * deltaTime * num;
		}
		num6 += this.m_DuckAddSpeed;
		this.m_Animator.SetFloat("MoveSpeedForward", num5);
		this.m_Animator.SetFloat("MoveSpeed", num6);
		this.m_Animator.SetFloat("MoveDirection", num7);
		this.m_Animator.SetFloat("WantedMoveSpeed", this.m_FPPController.m_CurentMoveSpeed);
		this.m_LastMoveSpeedForward = num5;
		this.m_LastMoveSpeed = num6;
		this.m_LastMoveDirection = num7;
		this.m_LastControllerVelocity = lastControllerVelocity;
	}

	private void UpdateSounds()
	{
		this.UpdateBreathingSounds();
	}

	private void UpdateBreathingSounds()
	{
		if (this.m_FPPController.IsRunning())
		{
			if (this.m_AudioModule.m_PlayingBreathSoundSource == null && Time.time - this.m_LastTimeNoSprint > 3f)
			{
				this.m_AudioModule.PlayBreathingSound(1f, false);
			}
		}
		else
		{
			if (this.m_AudioModule.m_PlayingBreathSoundSource != null && Time.time - this.m_FadeOutBreathLastTime > 3f)
			{
				this.m_AudioModule.FadeOutBreathingSound();
				this.m_FadeOutBreathLastTime = Time.time;
			}
			this.m_LastTimeNoSprint = Time.time;
		}
	}

	private PlayerAudioModule m_AudioModule;

	private CharacterController m_CharacterController;

	private FPPController m_FPPController;

	private float m_LastTimeNoSprint;

	private float m_FadeOutBreathLastTime;

	private float m_LastMoveSpeedForward;

	private float m_LastMoveSpeed;

	private float m_LastMoveDirection;

	private Vector3 m_LastControllerVelocity = Vector3.zero;

	private float m_DuckAddSpeed;
}
