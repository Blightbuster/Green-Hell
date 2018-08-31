using System;
using UnityEngine;
using UnityEngine.UI;

internal class WatchSanityData : WatchData
{
	public GameObject GetParent()
	{
		return this.m_Parent;
	}

	public GameObject m_Parent;

	public SWP_HeartRateMonitor m_Sanity;

	public Text m_SanityText;
}
