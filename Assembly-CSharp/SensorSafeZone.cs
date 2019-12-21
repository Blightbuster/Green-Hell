using System;

public class SensorSafeZone : SensorBase
{
	protected override void OnEnter()
	{
		base.OnEnter();
		Player.Get().m_CurrentSafeZonesCount++;
	}

	protected override void OnExit()
	{
		base.OnExit();
		if (Player.Get().m_CurrentSafeZonesCount > 0)
		{
			Player.Get().m_CurrentSafeZonesCount--;
		}
	}
}
