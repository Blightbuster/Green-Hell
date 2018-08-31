using System;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeCell
{
	public void RemoveObject(GameObject go)
	{
		this.m_Objects.Remove(go);
	}

	public int m_X;

	public int m_Y;

	public Vector2 m_Pos = default(Vector2);

	public Vector2 m_Size = default(Vector2);

	public List<GameObject> m_Objects = new List<GameObject>();
}
