using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class CombatChallenge : Challenge
{
	public override void Activate(GameObject parent)
	{
		base.Activate(parent);
		this.m_RequredItemIDs.Clear();
		this.m_RequredHoldItems.Clear();
		for (int i = 0; i < this.m_RequredItemNames.Count; i++)
		{
			ItemHold itemHold = ItemHold.FindByID((ItemID)Enum.Parse(typeof(ItemID), this.m_RequredItemNames[i]));
			this.m_RequredHoldItems.Add(itemHold);
			this.m_RequredItemIDs.Add(itemHold.m_ReplaceInfoID);
		}
	}

	public override void Load(Key key)
	{
		base.Load(key);
		for (int i = 0; i < key.GetKeysCount(); i++)
		{
			Key key2 = key.GetKey(i);
			if (key2.GetName() == "RequiredItems")
			{
				string[] array = key2.GetVariable(0).SValue.Split(new char[]
				{
					';'
				});
				for (int j = 0; j < array.Length; j++)
				{
					this.m_RequredItemNames.Add(array[j]);
				}
			}
		}
	}

	public override void Update()
	{
		base.Update();
		bool flag = true;
		Player.Get().GetCurrentItem();
		foreach (ItemID id in this.m_RequredItemIDs)
		{
			if (!InventoryBackpack.Get().Contains(id))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			this.Success();
		}
	}

	public override string GetLocalizedInfo()
	{
		Player.Get().GetCurrentItem();
		Localization localization = GreenHellGame.Instance.GetLocalization();
		string text = localization.Get("CombatChallenge_Collect", true);
		text += "\n";
		foreach (ItemHold itemHold in this.m_RequredHoldItems)
		{
			if (!(itemHold == null) && !itemHold.m_InInventory)
			{
				text += localization.Get(itemHold.m_ReplaceInfoName, true);
				text += " ";
				int num = 0;
				int num2 = 0;
				Player.Get().GetGPSCoordinates(itemHold.transform.position, out num, out num2);
				text = string.Concat(new string[]
				{
					text,
					num.ToString(),
					"'W ",
					num2.ToString(),
					"'S"
				});
				text += "\n";
			}
		}
		return text;
	}

	public override bool UpdateHUDChallengeInfo()
	{
		return true;
	}

	private List<ItemID> m_RequredItemIDs = new List<ItemID>();

	private List<ItemHold> m_RequredHoldItems = new List<ItemHold>();

	private List<string> m_RequredItemNames = new List<string>();
}
