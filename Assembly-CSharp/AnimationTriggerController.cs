using System;
using Enums;
using UnityEngine;

public class AnimationTriggerController : PlayerController
{
	public static AnimationTriggerController Get()
	{
		return AnimationTriggerController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		AnimationTriggerController.s_Instance = this;
		base.m_ControllerType = PlayerControllerType.AnimationTrigger;
		this.m_CharacterController = base.GetComponent<CharacterControllerProxy>();
		this.m_OffsetHelperController = base.GetComponent<OffsetHelperController>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (!this.m_Trigger)
		{
			this.Stop();
			return;
		}
		this.SetState(AnimationTriggerController.State.Enter);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.CrossFade(this.m_Idle, 0.2f, 0);
		this.m_Animator.CrossFade(this.m_UnarmedIdle, 0.2f, 1);
		if (this.m_Trigger && Player.Get())
		{
			Collider[] componentsInChildren = this.m_Trigger.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Physics.IgnoreCollision(componentsInChildren[i], Player.Get().m_Collider, false);
			}
		}
		if (this.m_State == AnimationTriggerController.State.Animation && this.m_Trigger)
		{
			ScenarioManager.Get().SetBoolVariable(this.m_Trigger.m_ScenarioBoolVariable, true);
		}
		LookController.Get().m_LookDev.y = 0f;
		LookController.Get().m_WantedLookDev.y = 0f;
		base.ResetBodyRotationBonesParams();
		FPPController.Get().ResetBodyRotationBonesParams();
	}

	private void SetState(AnimationTriggerController.State state)
	{
		this.m_State = state;
		this.OnEnterState();
	}

	private void OnEnterState()
	{
		if (this.m_State == AnimationTriggerController.State.Enter)
		{
			LookController.Get().SetWantedLookDir(this.m_Trigger.m_TransformObject.transform.forward);
			return;
		}
		if (this.m_State == AnimationTriggerController.State.Animation)
		{
			LookController.Get().SetWantedLookDir(Vector3.zero);
			this.m_Animator.CrossFade(this.m_Trigger.m_PlayerAnimationHash, 0.1f, 0);
			this.m_Animator.CrossFade(this.m_Trigger.m_PlayerAnimationHash, 0.1f, 1);
			this.m_Trigger.PlayAnim();
			LookController.Get().m_LookDev.y = 0f;
			LookController.Get().m_WantedLookDev.y = 0f;
			base.ResetBodyRotationBonesParams();
		}
	}

	public void SetTrigger(AnimationTrigger trigger)
	{
		this.m_Trigger = trigger;
		Collider[] componentsInChildren = this.m_Trigger.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Physics.IgnoreCollision(componentsInChildren[i], Player.Get().m_Collider);
		}
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		if (this.m_State == AnimationTriggerController.State.Animation)
		{
			this.m_OffsetHelperController.MoveCharacterController(this.m_CharacterController);
		}
	}

	public override void ControllerLateUpdate()
	{
		base.ControllerLateUpdate();
		if (this.m_State == AnimationTriggerController.State.Enter)
		{
			this.UpdateEnterState();
		}
	}

	private void UpdateEnterState()
	{
		Vector3 vector = this.m_Trigger.m_TransformObject.transform.position - base.transform.position;
		Vector3 a = vector.normalized * this.m_Speed;
		float f = Vector3.Angle(Player.Get().transform.forward.GetNormalized2D(), this.m_Trigger.m_TransformObject.transform.forward.GetNormalized2D());
		Vector3 motion = a * Time.deltaTime;
		if (vector.To2D().magnitude > motion.magnitude || Mathf.Abs(f) > 1f)
		{
			if (vector.To2D().magnitude > motion.magnitude)
			{
				this.m_CharacterController.Move(motion, true);
			}
			else
			{
				this.m_CharacterController.Move(vector, true);
			}
			LookController.Get().UpdateWantedLookDir();
			FPPController.Get().UpdateBodyRotation();
			return;
		}
		this.m_CharacterController.Move(vector, true);
		this.SetState(AnimationTriggerController.State.Animation);
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.AnimationTriggerEnd)
		{
			this.Stop();
		}
	}

	public void SetMoveSpeed(float speed)
	{
		this.m_Speed = speed;
	}

	public void ResetMoveSpeed()
	{
		this.m_Speed = 1.5f;
	}

	private CharacterControllerProxy m_CharacterController;

	private AnimationTrigger m_Trigger;

	private OffsetHelperController m_OffsetHelperController;

	private static AnimationTriggerController s_Instance;

	private AnimationTriggerController.State m_State;

	private int m_Idle = Animator.StringToHash("Idle");

	private int m_UnarmedIdle = Animator.StringToHash("Unarmed_Idle");

	private const float DEFAULT_SPEED = 1.5f;

	public float m_Speed = 1.5f;

	private enum State
	{
		None,
		Enter,
		Animation
	}
}
