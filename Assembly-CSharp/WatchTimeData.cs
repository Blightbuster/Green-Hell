using System;
using UnityEngine;
using UnityEngine.UI;

internal class WatchTimeData : WatchData
{
	public GameObject GetParent()
	{
		return this.m_Parent;
	}

	public GameObject m_Parent;

	public Text m_TimeHourDec;

	public Text m_TimeHourUnit;

	public Text m_TimeMinuteDec;

	public Text m_TimeMinuteUnit;

	public Text m_DayDec;

	public Text m_DayUnit;

	public Text m_DayName;

	public Text m_MonthName;
}
