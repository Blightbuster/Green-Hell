using System;
using System.Collections.Generic;

public class LookObject : Trigger
{
	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		base.GetActions(actions);
		actions.Add(TriggerAction.TYPE.Look);
	}

	public override void GetInfoText(ref string result)
	{
	}

	public override bool CanTrigger()
	{
		return (!Player.Get().m_DreamActive || !ChatterManager.Get().IsAnyChatterPlaying()) && (!this.m_OneTimeUse || !base.WasTriggered());
	}

	public bool m_OneTimeUse = true;
}
