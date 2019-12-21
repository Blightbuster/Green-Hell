using System;

public class SensorScenarioValue : SensorBase
{
	protected override void OnEnter()
	{
		base.OnEnter();
		ScenarioManager.Get().SetBoolVariable(this.m_ValueName, this.m_OnEnterValue);
	}

	public string m_ValueName = string.Empty;

	public bool m_OnEnterValue = true;
}
