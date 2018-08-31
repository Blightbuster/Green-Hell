using System;
using UnityEngine;
using UnityEngine.UI;

public class EnergyIcon
{
	public EnergyIcon(RawImage im)
	{
		this.m_IconObject = im.gameObject;
		this.m_IconTransform = im.gameObject.GetComponent<RectTransform>();
		this.m_IsEnabled = true;
		this.m_IconObject.SetActive(true);
	}

	public GameObject m_IconObject;

	public RectTransform m_IconTransform;

	public bool m_IsEnabled;
}
