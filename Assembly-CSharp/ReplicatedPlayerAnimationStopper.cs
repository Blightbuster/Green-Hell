using System;
using UnityEngine;

public class ReplicatedPlayerAnimationStopper : ReplicatedBehaviour
{
	private void Awake()
	{
		this.m_Animator = base.GetComponent<Animator>();
		this.m_ReplAnimator = base.GetComponent<ReplicatedAnimator>();
	}

	private void Update()
	{
		if (!base.ReplIsOwner() && this.m_AnimationStopTime >= 0f)
		{
			this.UpdateStoppedAnimation();
		}
	}

	public override void OnReplicationPrepare()
	{
		if (WeaponMeleeController.Get().m_AnimationStopped)
		{
			if (this.m_AnimationStopped != WeaponMeleeController.Get().m_AnimationStopped)
			{
				this.m_AnimationStopped = WeaponMeleeController.Get().m_AnimationStopped;
				if (this.m_AnimationStopped)
				{
					this.m_AnimationStopFrame = WeaponMeleeController.Get().m_AnimationStopFrame;
					this.m_StateHash = WeaponMeleeController.Get().m_AnimationStopStateHash;
					return;
				}
			}
		}
		else
		{
			this.m_AnimationStopped = false;
			this.m_AnimationStopFrame = -1f;
		}
	}

	public override void OnReplicationResolve()
	{
		if (this.m_AnimationStopped_Repl != this.m_AnimationStopped && this.m_AnimationStopped_Repl)
		{
			this.StopAnimation();
		}
	}

	private void StartAnimation()
	{
		this.m_AnimationStopTime = -1f;
		this.m_AnimationStopFrame = -1f;
		this.m_ReplAnimator.enabled = true;
	}

	private void StopAnimation()
	{
		this.m_ReplAnimator.enabled = false;
		this.m_AnimationStopFrameSaved = this.m_AnimationStopFrame_Repl;
		this.m_AnimationStopTime = Time.time;
		this.m_Animator.CrossFadeInFixedTime(this.m_StateHash_Repl, 0.25f, 1);
		this.UpdateStoppedAnimation();
	}

	private void UpdateStoppedAnimation()
	{
		this.m_Animator.PlayInFixedTime(this.m_StateHash_Repl, 1, this.m_AnimationStopFrameSaved);
		if (Time.time > 0.5f + this.m_AnimationStopTime)
		{
			this.StartAnimation();
		}
	}

	public virtual void OnReplicationPrepare_CJGenerated()
	{
		if (this.m_AnimationStopped_Repl != this.m_AnimationStopped)
		{
			this.m_AnimationStopped_Repl = this.m_AnimationStopped;
			this.ReplSetDirty();
		}
		if (this.m_AnimationStopFrame_Repl != this.m_AnimationStopFrame)
		{
			this.m_AnimationStopFrame_Repl = this.m_AnimationStopFrame;
			this.ReplSetDirty();
		}
		if (this.m_StateHash_Repl != this.m_StateHash)
		{
			this.m_StateHash_Repl = this.m_StateHash;
			this.ReplSetDirty();
		}
	}

	public virtual void OnReplicationSerialize_CJGenerated(P2PNetworkWriter writer, bool initial_state)
	{
		writer.Write(this.m_AnimationStopped_Repl);
		writer.Write(this.m_AnimationStopFrame_Repl);
		writer.Write(this.m_StateHash_Repl);
	}

	public virtual void OnReplicationDeserialize_CJGenerated(P2PNetworkReader reader, bool initial_state)
	{
		this.m_AnimationStopped_Repl = reader.ReadBoolean();
		this.m_AnimationStopFrame_Repl = reader.ReadFloat();
		this.m_StateHash_Repl = reader.ReadInt32();
	}

	public virtual void OnReplicationResolve_CJGenerated()
	{
		this.m_AnimationStopped = this.m_AnimationStopped_Repl;
		this.m_AnimationStopFrame = this.m_AnimationStopFrame_Repl;
		this.m_StateHash = this.m_StateHash_Repl;
	}

	[Replicate(new string[]
	{

	})]
	private bool m_AnimationStopped;

	[Replicate(new string[]
	{

	})]
	private float m_AnimationStopFrame = -1f;

	[Replicate(new string[]
	{

	})]
	private int m_StateHash;

	private float m_AnimationStopTime = -1f;

	private float m_AnimationStopFrameSaved = -1f;

	private const float CROSSFADE_DURATION = 0.25f;

	private const float STOP_DURATION_MAX = 0.5f;

	private Animator m_Animator;

	private ReplicatedAnimator m_ReplAnimator;

	private bool m_AnimationStopped_Repl;

	private float m_AnimationStopFrame_Repl;

	private int m_StateHash_Repl;
}
