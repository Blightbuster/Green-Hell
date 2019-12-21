using System;
using System.Collections.Generic;
using UnityEngine;

public class ReplicatedAnimClipReplacer : ReplicatedBehaviour
{
	public override void OnReplicationPrepare()
	{
		PlayerController localActiveControllerAnimClipReplacer = this.GetLocalActiveControllerAnimClipReplacer();
		PlayerControllerType playerControllerType = localActiveControllerAnimClipReplacer ? localActiveControllerAnimClipReplacer.m_ControllerType : PlayerControllerType.Unknown;
		if (playerControllerType != this.m_ReplControllerType)
		{
			this.m_ReplControllerType = playerControllerType;
			this.ReplSetDirty();
		}
		if (localActiveControllerAnimClipReplacer)
		{
			string text = localActiveControllerAnimClipReplacer.ReplaceClipsGetItemName();
			if (text != this.m_ReplItemName)
			{
				this.m_ReplItemName = text;
				this.ReplSetDirty();
			}
		}
	}

	public override void OnReplicationSerialize(P2PNetworkWriter writer, bool initial_state)
	{
		writer.Write((int)this.m_ReplControllerType);
		if (this.m_ReplControllerType != PlayerControllerType.Unknown)
		{
			writer.Write(this.m_ReplItemName);
		}
	}

	public override void OnReplicationDeserialize(P2PNetworkReader reader, bool initial_state)
	{
		this.m_ReplControllerType = (PlayerControllerType)reader.ReadInt32();
		if (this.m_ReplControllerType != PlayerControllerType.Unknown)
		{
			this.m_ReplItemName = reader.ReadString();
		}
	}

	public override void OnReplicationResolve()
	{
		DebugUtils.Assert(this.m_Animator != null, true);
		this.RestoreClips();
		this.ReplaceClipsForAnimator();
	}

	public PlayerController GetLocalActiveControllerAnimClipReplacer()
	{
		if (BowController.Get().enabled)
		{
			return BowController.Get();
		}
		if (HeavyObjectController.Get().enabled)
		{
			return HeavyObjectController.Get();
		}
		if (ItemController.Get().enabled)
		{
			return ItemController.Get();
		}
		if (WeaponMeleeController.Get().enabled)
		{
			return WeaponMeleeController.Get();
		}
		if (WeaponSpearController.Get().enabled)
		{
			return WeaponSpearController.Get();
		}
		return null;
	}

	public void ReplaceClipsForAnimator()
	{
		if (this.m_ReplControllerType == PlayerControllerType.Unknown)
		{
			return;
		}
		PlayerController controller = Player.Get().GetController(this.m_ReplControllerType);
		if (controller.m_ClipOverrides == null)
		{
			this.m_SetOverrides = null;
			return;
		}
		if (controller.m_ClipOverrides.TryGetValue(this.m_ReplItemName, out this.m_SetOverrides))
		{
			AnimatorOverrideController animatorOverrideController = this.m_Animator.runtimeAnimatorController as AnimatorOverrideController;
			if (animatorOverrideController == null)
			{
				animatorOverrideController = new AnimatorOverrideController(this.m_Animator.runtimeAnimatorController);
				this.m_Animator.runtimeAnimatorController = animatorOverrideController;
			}
			DebugUtils.Assert(animatorOverrideController != null, true);
			animatorOverrideController.ApplyOverrides(this.m_SetOverrides);
		}
	}

	private void RestoreClips()
	{
		if (this.m_SetOverrides != null)
		{
			AnimatorOverrideController animatorOverrideController = this.m_Animator.runtimeAnimatorController as AnimatorOverrideController;
			if (animatorOverrideController == null)
			{
				animatorOverrideController = new AnimatorOverrideController(this.m_Animator.runtimeAnimatorController);
				this.m_Animator.runtimeAnimatorController = animatorOverrideController;
			}
			DebugUtils.Assert(animatorOverrideController != null, true);
			for (int i = 0; i < this.m_SetOverrides.Count; i++)
			{
				animatorOverrideController[this.m_SetOverrides[i].Key] = null;
			}
			this.m_SetOverrides = null;
		}
	}

	public Animator m_Animator;

	private List<KeyValuePair<AnimationClip, AnimationClip>> m_SetOverrides;

	private PlayerControllerType m_ReplControllerType;

	private string m_ReplItemName;
}
