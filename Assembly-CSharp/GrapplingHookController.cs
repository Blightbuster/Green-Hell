using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

public class GrapplingHookController : PlayerController
{
	public static GrapplingHookController Get()
	{
		return GrapplingHookController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		GrapplingHookController.s_Instance = this;
		base.m_ControllerType = PlayerControllerType.GrapplingHook;
		this.m_State = GrapplingHookControllerState.None;
		this.m_LookController = base.gameObject.GetComponent<LookController>();
		this.m_CharacterController = base.GetComponent<CharacterControllerProxy>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.SetState(GrapplingHookControllerState.SetupPosition);
		this.ActivateLadder(true);
		this.m_LookController.SetLookAtObject(this.m_Trigger.gameObject);
		this.m_Bones.Clear();
		this.m_Bones.Add(this.m_Player.gameObject.transform.FindDeepChild("mixamorig:Spine"));
		this.m_Bones.Add(this.m_Player.gameObject.transform.FindDeepChild("mixamorig:Spine1"));
		this.m_Bones.Add(this.m_Player.gameObject.transform.FindDeepChild("mixamorig:Spine2"));
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_LookController.SetLookAtObject(null);
		this.ActivateLadder(false);
		this.SetState(GrapplingHookControllerState.None);
	}

	private void SetState(GrapplingHookControllerState state)
	{
		this.m_State = state;
		this.m_Animator.SetInteger(this.m_IGrapplingHook, (int)this.m_State);
	}

	private void FixedUpdate()
	{
		if (this.m_State == GrapplingHookControllerState.SetupPosition)
		{
			this.UpdateWantedSpeed();
		}
	}

	private void UpdateWantedSpeed()
	{
		Vector3 playerWantedPos = LadderController.GetPlayerWantedPos(this.m_Ladder, LadderControllerState.EnterDown);
		if (playerWantedPos.magnitude < 0.1f)
		{
			this.Stop();
			return;
		}
		Vector3 vector = playerWantedPos - this.m_Player.gameObject.transform.position;
		vector.y = 0f;
		if (vector.magnitude > 4f)
		{
			vector.Normalize();
			vector *= 4f;
		}
		this.m_WantedSpeed = vector;
		this.m_WantedSpeed += Physics.gravity * Time.fixedDeltaTime;
		if (this.m_WantedSpeed.y < -10f)
		{
			this.m_WantedSpeed.y = -10f;
		}
		this.m_CharacterController.Move(this.m_WantedSpeed * Time.fixedDeltaTime, true);
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		GrapplingHookControllerState state = this.m_State;
		if (state != GrapplingHookControllerState.SetupPosition)
		{
			if (state == GrapplingHookControllerState.ThrowHook)
			{
				AnimatorStateInfo currentAnimatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(0);
				if (!currentAnimatorStateInfo.Equals(null) && currentAnimatorStateInfo.normalizedTime > 0.8f)
				{
					this.SetState(GrapplingHookControllerState.None);
					this.StartLadder();
				}
			}
		}
		else
		{
			Vector3 a = LadderController.GetPlayerWantedPos(this.m_Ladder, LadderControllerState.EnterDown);
			Vector3 a2 = LadderController.GetPlayerWantedDir(this.m_Ladder);
			if (a.magnitude < 0.1f || a2.magnitude < 0.1f)
			{
				this.Stop();
				return;
			}
			a -= this.m_Player.gameObject.transform.position;
			a.y = 0f;
			a2 -= this.m_Player.gameObject.transform.forward;
			if (a.magnitude < 0.1f && a2.magnitude < 0.03f)
			{
				this.SetState(GrapplingHookControllerState.ThrowHook);
			}
		}
		this.UpdateLookDev();
	}

	private void UpdateLookDev()
	{
		this.m_LookController.UpdateLookDev(0f, 0f);
	}

	public override void ControllerLateUpdate()
	{
		base.ControllerLateUpdate();
		this.UpdateBodyRotation();
	}

	private void UpdateBodyRotation()
	{
		Quaternion rotation = default(Quaternion);
		rotation = Quaternion.Euler(0f, this.m_LookController.m_LookDev.x, 0f);
		this.m_CharacterController.transform.rotation = rotation;
		float num = 1f / (float)this.m_Bones.Count;
		for (int i = 0; i < this.m_Bones.Count; i++)
		{
			this.m_BodyRotationBonesParams[this.m_Bones[i]] = -this.m_LookController.m_LookDev.y * num;
		}
	}

	public override Dictionary<Transform, float> GetBodyRotationBonesParams()
	{
		return this.m_BodyRotationBonesParams;
	}

	private void StartLadder()
	{
		this.m_Player.GetComponent<LadderController>().SetLadder(this.m_Ladder);
		this.m_Player.StartController(PlayerControllerType.Ladder);
	}

	private void ActivateLadder(bool active)
	{
		if (this.m_Ladder == null)
		{
			return;
		}
		this.m_Ladder.gameObject.SetActive(active);
	}

	[HideInInspector]
	public GrapplingHookTrigger m_Trigger;

	[HideInInspector]
	public Ladder m_Ladder;

	private static GrapplingHookController s_Instance;

	private GrapplingHookControllerState m_State;

	private int m_IGrapplingHook = Animator.StringToHash("GrapplingHook");

	private LookController m_LookController;

	private List<Transform> m_Bones = new List<Transform>();

	private Dictionary<Transform, float> m_BodyRotationBonesParams = new Dictionary<Transform, float>();

	private CharacterControllerProxy m_CharacterController;

	private Vector3 m_WantedSpeed = Vector3.zero;
}
