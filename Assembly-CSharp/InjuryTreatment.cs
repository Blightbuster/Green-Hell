using System;
using System.Collections.Generic;
using Enums;

public class InjuryTreatment
{
	public void AddItem(ItemID item, int count)
	{
		this.m_Items.Add((int)item, count);
	}

	public Dictionary<int, int> GetAllItems()
	{
		return this.m_Items;
	}

	private Dictionary<int, int> m_Items = new Dictionary<int, int>();
}
