using System;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeStaticObjectsCell
{
	public void RemoveObject(GameObject go)
	{
		int i = 0;
		while (i < this.m_Objects.Count)
		{
			if (this.m_Objects[i].m_GameObject == go)
			{
				this.m_Objects.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
	}

	public int m_X;

	public int m_Y;

	public Vector2 m_Pos;

	public Vector2 m_Size;

	public List<StaticObjectClass> m_Objects = new List<StaticObjectClass>();
}
