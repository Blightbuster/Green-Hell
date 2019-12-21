using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

public class FreeHandsLadder : Construction
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

	protected override void Start()
	{
		base.Initialize(false);
		base.Start();
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		actions.Add(TriggerAction.TYPE.Climb);
	}

	public override bool CanExecuteActions()
	{
		return false;
	}

	public override bool CanTrigger()
	{
		return false;
	}

	public override Vector3 GetIconPos()
	{
		return TriggerController.Get().GetBestTriggerHitPos();
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

	public override bool IsFreeHandsLadder()
	{
		return true;
	}

	private List<Transform> m_Triggers = new List<Transform>();
}
