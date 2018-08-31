using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class InventoryCell
{
	public bool IsOccupied()
	{
		return this.m_Items.Count > 0;
	}

	public int m_IndexX;

	public int m_IndexY;

	public List<Item> m_Items = new List<Item>();

	public BackpackPocket pocket;

	public GameObject m_Object;

	public Renderer m_Renderer;
}
