using System;
using UnityEngine;
using UnityEngine.UI;

internal class WatchMacronutrientsData : WatchData
{
	public GameObject GetParent()
	{
		return this.m_Parent;
	}

	public GameObject m_Parent;

	public Image m_Fat;

	public Image m_Carbo;

	public Image m_Proteins;

	public Image m_Hydration;

	public Image m_FatBG;

	public Image m_CarboBG;

	public Image m_ProteinsBG;

	public Image m_HydrationBG;
}
