using System;
using Enums;
using UnityEngine;

public class HitReactionController : PlayerController
{
	public static HitReactionController Get()
	{
		return HitReactionController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		this.m_ControllerType = PlayerControllerType.HitReaction;
		HitReactionController.s_Instance = this;
	}

	protected override void Start()
	{
		base.Start();
		this.m_Animator.SetTrigger(this.m_EnterHash);
		RuntimeAnimatorController runtimeAnimatorController = this.m_Animator.runtimeAnimatorController;
		for (int i = 0; i < runtimeAnimatorController.animationClips.Length; i++)
		{
			if (runtimeAnimatorController.animationClips[i].name == "PL|ShakingOff")
			{
				this.m_Length = runtimeAnimatorController.animationClips[i].length / 2f;
				break;
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		FistFightController.Get().PlayerFightPunchAttackEnd(FistFightController.Mode.Cancelled);
		if (Player.Get().m_ActiveFightController)
		{
			Player.Get().m_ActiveFightController.ResetAttack();
		}
		this.OnTakeDamage();
		this.m_Animator.SetTrigger(this.m_EnterHash);
		this.m_LastHitTime = Time.time;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetLayerWeight(4, this.m_SpineAddLayerBlendWeightToRestore);
	}

	public void OnTakeDamage()
	{
		this.m_Animator.SetTrigger(this.m_EnterHash);
		this.m_LastHitTime = Time.time;
		this.m_SpineAddLayerBlendWeightToRestore = this.m_Animator.GetLayerWeight(4);
		this.m_Animator.SetLayerWeight(4, 1f);
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.HitReactionEnd)
		{
			this.Stop();
		}
	}

	private int m_EnterHash = Animator.StringToHash("HitReactionEnter");

	private float m_Length;

	private float m_LastHitTime;

	private float m_SpineAddLayerBlendWeightToRestore;

	private static HitReactionController s_Instance;
}
