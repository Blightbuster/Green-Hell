using System;

public class TriggerHoldData
{
	public TriggerHoldData(TriggerAction.TYPE orig, TriggerAction.TYPE hold)
	{
		this.m_OrigAction = orig;
		this.m_HoldAction = hold;
	}

	public TriggerAction.TYPE m_OrigAction;

	public TriggerAction.TYPE m_HoldAction;
}
