using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class MudChallenge : Challenge
{
	public override void Activate(GameObject parent)
	{
		base.Activate(parent);
		this.m_RequredItemIDs.Clear();
		this.m_RequredItemNames.Clear();
		for (int i = 0; i < this.m_StoredRequredItemNames.Count; i++)
		{
			ItemID item = (ItemID)Enum.Parse(typeof(ItemID), this.m_StoredRequredItemNames[i]);
			this.m_RequredItemIDs.Add((int)item);
			this.m_RequredItemNames.Add(this.m_StoredRequredItemNames[i]);
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
					this.m_StoredRequredItemNames.Add(array[j]);
				}
			}
			else if (key2.GetName() == "RequireArmorOnAllLimbs")
			{
				this.m_RequireArmorOnAllLimbs = (ArmorType)Enum.Parse(typeof(ArmorType), key2.GetVariable(0).SValue);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		int i = 0;
		while (i < this.m_RequredItemIDs.Count)
		{
			if (InventoryBackpack.Get().Contains((ItemID)this.m_RequredItemIDs[i]))
			{
				this.m_RequredItemIDs.RemoveAt(i);
				this.m_RequredItemNames.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
		if (this.m_RequireArmorOnAllLimbs != ArmorType.None)
		{
			for (int j = 0; j < 4; j++)
			{
				if (!PlayerArmorModule.Get().IsArmorActive((Limb)j, this.m_RequireArmorOnAllLimbs))
				{
					return;
				}
			}
		}
		if (this.m_RequredItemIDs.Count == 0)
		{
			this.Success();
		}
	}

	public override string GetLocalizedInfo()
	{
		Localization localization = GreenHellGame.Instance.GetLocalization();
		string text = localization.Get("MudChallenge_Collect", true);
		for (int i = 0; i < this.m_RequredItemNames.Count; i++)
		{
			if (!InventoryBackpack.Get().Contains((ItemID)this.m_RequredItemIDs[i]))
			{
				text += "\n";
				text += localization.Get(this.m_RequredItemNames[i].ToString(), true);
			}
		}
		if (this.m_RequireArmorOnAllLimbs != ArmorType.None)
		{
			bool flag = true;
			for (int j = 0; j < 4; j++)
			{
				if (!PlayerArmorModule.Get().IsArmorActive((Limb)j, this.m_RequireArmorOnAllLimbs))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				text += "\n";
				text += localization.Get("MudChallenge_Armor", true);
				text += "\n";
				text += localization.Get(this.m_RequireArmorOnAllLimbs.ToString().ToLower() + "_armor", true);
			}
		}
		return text;
	}

	public override bool UpdateHUDChallengeInfo()
	{
		return true;
	}

	private List<int> m_RequredItemIDs = new List<int>();

	private List<string> m_RequredItemNames = new List<string>();

	private List<string> m_StoredRequredItemNames = new List<string>();

	private ArmorType m_RequireArmorOnAllLimbs;
}
