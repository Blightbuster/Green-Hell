using System;
using System.Collections.Generic;

public interface ITriggerOwner
{
	bool CanTrigger(Trigger trigger);

	void OnExecute(Trigger trigger, TriggerAction.TYPE action);

	void GetActions(Trigger trigger, List<TriggerAction.TYPE> actions);

	string GetTriggerInfoLocalized(Trigger trigger);

	string GetIconName(Trigger trigger);
}
