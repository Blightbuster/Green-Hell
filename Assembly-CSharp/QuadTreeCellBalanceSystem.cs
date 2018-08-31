using System;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeCellBalanceSystem
{
	public void RemoveObject(BalanceSystemObject go)
	{
		this.m_Objects.Remove(go);
	}

	public int m_X;

	public int m_Y;

	public Vector2 m_Pos = default(Vector2);

	public Vector2 m_Size = default(Vector2);

	public List<BalanceSystemObject> m_Objects = new List<BalanceSystemObject>();
}
