using System;
using Enums;
using UnityEngine;

public class InsectsSensor : SensorBase
{
	protected override void Update()
	{
		base.Update();
		if (!this.m_IsInside)
		{
			return;
		}
		if (!DifficultySettings.ActivePreset.m_Insects)
		{
			return;
		}
		if (InsectsController.Get().IsActive())
		{
			return;
		}
		if (MakeFireController.Get().IsActive())
		{
			return;
		}
		if (HitReactionController.Get().IsActive())
		{
			return;
		}
		if (HarvestingAnimalController.Get().IsActive())
		{
			return;
		}
		if (MudMixerController.Get().IsActive())
		{
			return;
		}
		if (HarvestingSmallAnimalController.Get().IsActive())
		{
			return;
		}
		if (ConsciousnessController.Get().IsActive())
		{
			return;
		}
		if (DeathController.Get().IsActive())
		{
			return;
		}
		if (SleepController.Get().IsActive())
		{
			return;
		}
		if (Player.Get().m_Animator.GetBool(Player.Get().m_CleanUpHash))
		{
			return;
		}
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		if (currentItem && currentItem.m_Info.IsTorch() && ((Torch)currentItem).m_Burning)
		{
			return;
		}
		if (Player.Get().IsDead())
		{
			return;
		}
		if (InsectsSensor.m_LastTime == 0f || Time.time - InsectsSensor.m_LastTime > this.m_Cooldown)
		{
			if (CraftingManager.Get().IsActive())
			{
				CraftingManager.Get().Deactivate();
			}
			InsectsController.Get().m_Sensor = this;
			Player.Get().StartController(PlayerControllerType.Insects);
			InsectsSensor.m_LastTime = Time.time;
		}
	}

	public float m_Cooldown = 5f;

	private static float m_LastTime;

	public InsectsSensor.Type m_Type;

	public enum Type
	{
		Insects,
		Wasps
	}
}
