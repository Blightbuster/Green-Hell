using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGridCell
{
	public int m_X;

	public int m_Y;

	public Vector2 m_Pos = default(Vector2);

	public float m_Size;

	public List<Vector3> m_Points = new List<Vector3>();
}
