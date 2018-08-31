using System;
using System.Collections.Generic;
using UnityEngine;

public class IconScroller
{
	public IconScroller()
	{
		this.m_PositionsList = new List<Vector3>();
		this.m_EnergyIconList = new List<EnergyIcon>();
		this.m_ActiveEnergyIconList = new List<EnergyIcon>();
	}

	public float WIDTH = 36f;

	public float ANIM_TIME = 0.5f;

	public List<Vector3> m_PositionsList;

	public List<EnergyIcon> m_EnergyIconList;

	public List<EnergyIcon> m_ActiveEnergyIconList;
}
