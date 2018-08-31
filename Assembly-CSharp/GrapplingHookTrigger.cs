using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class GrapplingHookTrigger : Trigger
{
	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		if (!this.m_Activated)
		{
			actions.Add(TriggerAction.TYPE.UseHold);
		}
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.UseHold)
		{
			this.m_Line.SetActive(true);
			this.m_Activated = true;
		}
	}

	public override bool CanExecuteActions()
	{
		bool flag = base.CanExecuteActions();
		return !this.m_Activated && flag && InventoryBackpack.Get().Contains(ItemID.Grappling_Hook);
	}

	private bool m_Activated;

	public GameObject m_Line;
}
