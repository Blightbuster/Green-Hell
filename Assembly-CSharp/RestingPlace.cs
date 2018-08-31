using System;
using System.Collections.Generic;

public class RestingPlace : Construction
{
	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (!GreenHellGame.ROADSHOW_DEMO && action == TriggerAction.TYPE.Sleep)
		{
			SleepController.Get().StartSleeping(this, true);
		}
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		actions.Add(TriggerAction.TYPE.Sleep);
	}

	public override bool CanTrigger()
	{
		return true;
	}
}
