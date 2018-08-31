using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

public class Ladder : Construction
{
	protected override void Awake()
	{
		base.Awake();
		for (int i = 0; i < 10; i++)
		{
			Transform transform = base.gameObject.transform.FindDeepChild("Trigger" + i.ToString());
			if (transform != null)
			{
				this.m_Triggers.Add(transform);
			}
		}
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		actions.Add(TriggerAction.TYPE.Climb);
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		Player.Get().StartClimbing(this);
	}

	public override bool CanExecuteActions()
	{
		Transform x = null;
		float num = float.MaxValue;
		for (int i = 0; i < this.m_Triggers.Count; i++)
		{
			Transform transform = this.m_Triggers[i];
			float magnitude = (Player.Get().transform.position - transform.position).magnitude;
			if (magnitude < Mathf.Max(this.m_TriggerUseRange, Player.Get().GetParams().GetTriggerUseRange()) && magnitude < num)
			{
				x = transform;
				num = magnitude;
			}
		}
		return x != null;
	}

	public override bool CanTrigger()
	{
		return true;
	}

	public override bool CheckRange()
	{
		return false;
	}

	public override bool CheckDot()
	{
		return false;
	}

	public override bool OnlyInCrosshair()
	{
		return true;
	}

	public override Vector3 GetIconPos()
	{
		return TriggerController.Get().GetBestTriggerHitPos();
	}

	public override bool IsAdditionalCollider(Collider coll)
	{
		for (int i = 0; i < this.m_Triggers.Count; i++)
		{
			if (coll.gameObject.transform == this.m_Triggers[i])
			{
				return true;
			}
		}
		return false;
	}

	public override bool IsLadder()
	{
		return true;
	}

	private List<Transform> m_Triggers = new List<Transform>();

	public float m_PlayerOffset = 0.7f;
}
