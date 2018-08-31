using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class VomitingController : PlayerController
{
	public static VomitingController Get()
	{
		return VomitingController.s_Instance;
	}

	protected override void Awake()
	{
		VomitingController.s_Instance = this;
		base.Awake();
		this.m_ControllerType = PlayerControllerType.Vomiting;
		this.m_AudioModule = this.m_Player.GetComponent<PlayerAudioModule>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_FXCounter = 0;
		this.m_Player.BlockMoves();
		this.m_State = VomitingController.State.Vomiting;
		this.m_BodyRotationBonesParams[base.gameObject.transform.FindDeepChild("mixamorig:Spine")] = 0f;
		this.m_BodyRotationBonesParams[base.gameObject.transform.FindDeepChild("mixamorig:Spine1")] = 0f;
		this.m_BodyRotationBonesParams[base.gameObject.transform.FindDeepChild("mixamorig:Spine2")] = 0f;
		this.m_Animator.SetBool(this.m_BVomiting, true);
		EventsManager.OnEvent(Enums.Event.Vomit, 1);
		TriggerController.Get().m_TriggerToExecute = null;
		this.m_Animator.SetBool(TriggerController.s_BGrabItem, false);
		this.m_Animator.SetBool(TriggerController.s_BGrabItemBow, false);
		this.m_Animator.SetBool(TriggerController.s_BGrabItemBambooBow, false);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetBool(this.m_BVomiting, false);
		this.m_Player.UnblockMoves();
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		this.UpdateState();
	}

	private void UpdateState()
	{
		VomitingController.State state = this.m_State;
		if (state != VomitingController.State.Vomiting)
		{
			if (state == VomitingController.State.End)
			{
				this.Stop();
			}
		}
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.VomitingThrowUp)
		{
			this.ThrowUp();
		}
		else if (id == AnimEventID.VomitingEnd)
		{
			this.m_Animator.SetBool(this.m_BVomiting, false);
			this.m_State = VomitingController.State.End;
		}
	}

	private void ThrowUp()
	{
		this.m_AudioModule.PlayVomitingSound(1f, false);
		this.SpawnFX();
	}

	public void SpawnFX()
	{
		Transform transform = this.m_Player.gameObject.transform.FindDeepChild("mixamorig:Head");
		Vector3 pos = transform.position + transform.TransformDirection(this.m_ParticlePosShift);
		ParticlesManager.Get().Spawn("Vomit", pos, transform.rotation, transform);
		this.m_FXCounter++;
		if (this.m_FXCounter < 10)
		{
			base.Invoke("SpawnFX", 0.1f);
		}
	}

	public override Dictionary<Transform, float> GetBodyRotationBonesParams()
	{
		return this.m_BodyRotationBonesParams;
	}

	private int m_BVomiting = Animator.StringToHash("Vomiting");

	private PlayerAudioModule m_AudioModule;

	public ChatterManager m_ChatterManager;

	private static VomitingController s_Instance;

	private Dictionary<Transform, float> m_BodyRotationBonesParams = new Dictionary<Transform, float>();

	private VomitingController.State m_State;

	public Vector3 m_ParticlePosShift = Vector3.zero;

	private int m_FXCounter;

	private enum State
	{
		None,
		Vomiting,
		End
	}
}
