using System;
using Enums;
using UnityEngine.UI;

public class ArmorSmallIcon
{
	public void On()
	{
		this.m_On.gameObject.SetActive(true);
		this.m_Off.gameObject.SetActive(false);
	}

	public void Off()
	{
		this.m_On.gameObject.SetActive(false);
		this.m_Off.gameObject.SetActive(true);
	}

	public void Disable()
	{
		this.m_On.gameObject.SetActive(false);
		this.m_Off.gameObject.SetActive(false);
	}

	public Limb m_Limb = Limb.None;

	public RawImage m_On;

	public RawImage m_Off;
}
