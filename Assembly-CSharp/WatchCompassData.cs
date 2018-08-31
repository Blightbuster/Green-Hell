using System;
using UnityEngine;
using UnityEngine.UI;

internal class WatchCompassData : WatchData
{
	public GameObject GetParent()
	{
		return this.m_Parent;
	}

	public GameObject m_Parent;

	public GameObject m_Compass;

	public Text m_GPSCoordinates;
}
