using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class GrapplingHookTeleporter : Trigger
{
	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		actions.Add(TriggerAction.TYPE.ClimbHold);
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.ClimbHold)
		{
			Player.Get().BlockMoves();
			Player.Get().BlockRotation();
			FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
			fadeSystem.FadeOut(FadeType.All, new VDelegate(this.Teleport), 1.5f, null);
		}
	}

	private void Teleport()
	{
		Player.Get().gameObject.transform.position = this.m_TeleportPos.transform.position;
		Player.Get().UnblockMoves();
		Player.Get().UnblockRotation();
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeIn(FadeType.All, null, 1.5f);
	}

	public override bool CanExecuteActions()
	{
		return base.CanExecuteActions();
	}

	public GameObject m_TeleportPos;
}
