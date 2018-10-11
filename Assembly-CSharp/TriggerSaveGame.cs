using System;
using System.Collections.Generic;

public class TriggerSaveGame : Trigger, ITriggerThrough
{
	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		actions.Add(TriggerAction.TYPE.SaveGame);
	}

	public override string GetIconName()
	{
		return "SaveGame_icon";
	}

	public override bool CanExecuteActions()
	{
		return base.enabled;
	}

	public override bool CanTrigger()
	{
		return !ChallengesManager.Get().IsChallengeActive();
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		MenuInGameManager.Get().ShowScreen(typeof(MenuSave));
	}
}
