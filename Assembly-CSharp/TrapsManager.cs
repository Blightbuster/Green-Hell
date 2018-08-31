using System;
using System.Collections.Generic;
using UnityEngine;

public class TrapsManager : MonoBehaviour
{
	public static TrapsManager Get()
	{
		return TrapsManager.s_Instance;
	}

	private void Awake()
	{
		TrapsManager.s_Instance = this;
	}

	public void RegisterTrap(Trap trap)
	{
		if (!this.m_AllTraps.Contains(trap))
		{
			this.m_AllTraps.Add(trap);
		}
	}

	public void UnregisterTrap(Trap trap)
	{
		if (this.m_AllTraps.Contains(trap))
		{
			this.m_AllTraps.Remove(trap);
		}
	}

	private void Update()
	{
		foreach (Trap trap in this.m_AllTraps)
		{
			trap.ConstantUpdate();
		}
	}

	private List<Trap> m_AllTraps = new List<Trap>();

	private static TrapsManager s_Instance;
}
