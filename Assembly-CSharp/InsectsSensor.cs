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
		if (InsectsController.Get().IsActive())
		{
			return;
		}
		if (MakeFireController.Get().IsActive())
		{
			return;
		}
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		if (currentItem && currentItem.m_Info.IsTorch())
		{
			Torch torch = (Torch)currentItem;
			if (torch.m_Burning)
			{
				return;
			}
		}
		if (Player.Get().IsDead())
		{
			return;
		}
		if (this.m_LastTime == 0f || Time.time - this.m_LastTime > this.m_Cooldown)
		{
			InsectsController.Get().m_Sensor = this;
			Player.Get().StartController(PlayerControllerType.Insects);
			this.m_LastTime = Time.time;
		}
	}

	public float m_Cooldown = 5f;

	private float m_LastTime;

	public InsectsSensor.Type m_Type;

	public enum Type
	{
		Insects,
		Wasps
	}
}
