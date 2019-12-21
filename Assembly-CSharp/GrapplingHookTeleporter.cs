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
			GreenHellGame.GetFadeSystem().FadeOut(FadeType.All, new VDelegate(this.Teleport), 1.5f, null);
		}
	}

	private void Teleport()
	{
		Player.Get().Reposition(this.m_TeleportPos.transform.position, null);
		Player.Get().UnblockMoves();
		Player.Get().UnblockRotation();
		GreenHellGame.GetFadeSystem().FadeIn(FadeType.All, null, 1.5f);
	}

	public GameObject m_TeleportPos;
}
