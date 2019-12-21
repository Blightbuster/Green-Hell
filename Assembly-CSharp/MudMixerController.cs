using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class MudMixerController : PlayerController
{
	public static MudMixerController Get()
	{
		return MudMixerController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		MudMixerController.s_Instance = this;
		base.m_ControllerType = PlayerControllerType.MudMixer;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Animator.SetBool(this.m_MudMixerHash, true);
		this.m_Animator.SetTrigger(this.m_MudMixerInHash);
		Collider collider = Player.Get().m_Collider;
		Physics.IgnoreCollision(collider, this.m_Mixer.m_BoxCollider);
		foreach (Collider collider2 in this.m_Mixer.gameObject.GetComponentsInChildren<Collider>())
		{
			Physics.IgnoreCollision(collider, collider2);
		}
		this.m_Finish = false;
		this.m_StartPosition = this.m_Player.transform.position;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		LookController.Get().m_WantedLookDev.y = 0f;
		this.m_Player.m_AudioModule.StopHarvestAnimalSound();
		if (this.m_Animator.isInitialized)
		{
			this.m_Animator.SetBool(this.m_MudMixerHash, false);
		}
		Collider collider = Player.Get().m_Collider;
		Physics.IgnoreCollision(collider, this.m_Mixer.m_BoxCollider, false);
		foreach (Collider collider2 in this.m_Mixer.gameObject.GetComponentsInChildren<Collider>())
		{
			Physics.IgnoreCollision(collider, collider2, false);
		}
		this.m_Mixer.OnFinishMixing();
		LookController.Get().m_LookDev.y = 0f;
		LookController.Get().m_WantedLookDev.y = 0f;
		base.ResetBodyRotationBonesParams();
		FPPController.Get().ResetBodyRotationBonesParams();
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		if (!this.m_Finish)
		{
			Vector3 a = this.m_Mixer.transform.TransformPoint(this.m_Mixer.m_BoxCollider.center) - base.transform.position;
			if (a.magnitude > 0.5f)
			{
				base.transform.position += a * Time.deltaTime;
				return;
			}
		}
		else
		{
			Vector3 a2 = this.m_StartPosition - base.transform.position;
			if (a2.magnitude > 0.1f)
			{
				base.transform.position += a2 * Time.deltaTime * 2f;
				return;
			}
			this.Stop();
		}
	}

	public void SetMixer(MudMixer mixer)
	{
		this.m_Mixer = mixer;
	}

	public bool BlockInventoryInputs()
	{
		return base.enabled;
	}

	public override Dictionary<Transform, float> GetBodyRotationBonesParams()
	{
		return null;
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		if (id == AnimEventID.MudMixerEnd)
		{
			this.m_Finish = true;
			return;
		}
		if (id == AnimEventID.MudMixerGetDirty)
		{
			PlayerConditionModule.Get().GetDirtinessAdd(GetDirtyReason.UsingMud, null);
		}
	}

	private int m_MudMixerHash = Animator.StringToHash("MudMixer");

	private int m_MudMixerInHash = Animator.StringToHash("MudMixerIn");

	private MudMixer m_Mixer;

	private Vector3 m_StartPosition = Vector3.zero;

	private bool m_Finish;

	private static MudMixerController s_Instance;
}
