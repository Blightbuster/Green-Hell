using System;

public class Bow : Weapon
{
	public override bool CanTrigger()
	{
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && !WalkieTalkieController.Get().IsActive() && base.CanTrigger();
	}
}
