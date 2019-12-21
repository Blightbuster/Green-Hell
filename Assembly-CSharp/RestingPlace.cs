using System;
using System.Collections.Generic;

public class RestingPlace : Construction
{
	public override bool CanExecuteActions()
	{
		return base.CanExecuteActions() && PlayerStateModule.Get().m_State != PlayerStateModule.State.Combat;
	}

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
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && !ReplicatedPlayerTriggerHelper.IsTriggerExecutedByOtherPlayer(this);
	}

	public override bool ShowAdditionalInfo()
	{
		return PlayerStateModule.Get().m_State == PlayerStateModule.State.Combat;
	}

	public override string GetAdditionalInfoLocalized()
	{
		return GreenHellGame.Instance.GetLocalization().Get("HUD_CantSleep_Combat", true);
	}
}
