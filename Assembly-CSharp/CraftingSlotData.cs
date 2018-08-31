using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

[Serializable]
public class CraftingSlotData
{
	public string id;

	[HideInInspector]
	public ItemID item_id;

	public List<ItemSlot> activateOnInsert;
}
