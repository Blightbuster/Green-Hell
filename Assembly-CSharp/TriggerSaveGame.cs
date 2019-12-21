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
		return base.enabled && base.CanExecuteActions() && PlayerStateModule.Get().m_State != PlayerStateModule.State.Combat;
	}

	public override bool CanTrigger()
	{
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && !MainLevel.Instance.m_SaveGameBlocked && !GreenHellGame.Instance.IsGamescom() && ReplTools.AmIMaster() && !ChallengesManager.Get().IsChallengeActive();
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		MenuInGameManager.Get().ShowScreen(typeof(SaveGameMenu));
	}

	public override bool ShowAdditionalInfo()
	{
		return PlayerStateModule.Get().m_State == PlayerStateModule.State.Combat;
	}

	public override string GetAdditionalInfoLocalized()
	{
		return GreenHellGame.Instance.GetLocalization().Get("HUD_CantSave_Combat", true);
	}
}
