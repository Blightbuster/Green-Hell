using System;
using System.Collections.Generic;
using UnityEngine;

public class AcresManager : MonoBehaviour
{
	private void Awake()
	{
		AcresManager.s_Instance = this;
	}

	public static AcresManager Get()
	{
		return AcresManager.s_Instance;
	}

	public void RegisterAcre(Acre acre)
	{
		if (!this.m_Acres.Contains(acre))
		{
			this.m_Acres.Add(acre);
		}
	}

	public void UnregisterAcre(Acre acre)
	{
		this.m_Acres.Remove(acre);
	}

	private void Update()
	{
		if (this.m_Acres.Count == 0)
		{
			this.m_CurrentIdx = 0;
			return;
		}
		if (this.m_CurrentIdx >= this.m_Acres.Count)
		{
			this.m_CurrentIdx = 0;
		}
		Acre acre = this.m_Acres[this.m_CurrentIdx];
		if (acre == null)
		{
			this.m_Acres.RemoveAt(this.m_CurrentIdx);
			this.m_CurrentIdx++;
			return;
		}
		acre.UpdateInternal();
		this.m_CurrentIdx++;
	}

	public bool IsPointInsideAny(Vector3 pos)
	{
		for (int i = 0; i < this.m_Acres.Count; i++)
		{
			Acre acre = this.m_Acres[i];
			if (!(acre == null) && acre.m_Bounds.Contains(pos))
			{
				return true;
			}
		}
		return false;
	}

	private static AcresManager s_Instance;

	private List<Acre> m_Acres = new List<Acre>();

	private int m_CurrentIdx;
}
