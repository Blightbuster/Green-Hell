using System;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeBalanceSystemDisabledSpawnersCell
{
	public void RemoveObject(BalanceSpawner go)
	{
		this.m_Objects.Remove(go);
	}

	public int m_X;

	public int m_Y;

	public Vector2 m_Pos;

	public Vector2 m_Size;

	public List<BalanceSpawner> m_Objects = new List<BalanceSpawner>();
}
