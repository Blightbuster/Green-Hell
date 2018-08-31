using System;

public class SensorLift : SensorBase
{
	protected override void OnEnter()
	{
		base.OnEnter();
		Player.Get().m_CurrentLift = this;
	}

	protected override void OnExit()
	{
		base.OnExit();
		Player.Get().m_CurrentLift = null;
	}
}
