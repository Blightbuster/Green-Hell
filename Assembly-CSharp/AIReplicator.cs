using System;
using AIs;
using UnityEngine;

[DisallowMultipleComponent]
public class AIReplicator : ReplicatedBehaviour
{
	public override void ReplOnSpawned()
	{
		base.Invoke("UpdateState", 0f);
	}

	public override void ReplOnChangedOwner(bool was_owner)
	{
		this.UpdateState();
	}

	private void UpdateState()
	{
		bool enabled = base.ReplIsOwner();
		AIModule[] components = base.GetComponents<AIModule>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = enabled;
		}
		AI component = base.GetComponent<AI>();
		if (component)
		{
			component.enabled = enabled;
		}
	}

	private void OnAnimatorMove()
	{
	}
}
