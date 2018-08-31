using System;
using AIs;
using Enums;
using UnityEngine;

public class TribeRunawayChallenge : Challenge
{
	public override void Activate(GameObject parent)
	{
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			GameObject gameObject = parent.transform.GetChild(i).gameObject;
			HumanAIGroup component = gameObject.GetComponent<HumanAIGroup>();
			if (component)
			{
				component.m_ChallengeGroup = true;
				for (int j = 0; j < component.m_Members.Count; j++)
				{
					if (component.m_Members[j].m_AddHarvestingItem != ItemID.None)
					{
						this.m_Tribe = component.m_Members[j].gameObject;
						this.m_ItemID = component.m_Members[j].m_AddHarvestingItem;
						break;
					}
				}
			}
			if (this.m_ItemID != ItemID.None)
			{
				break;
			}
		}
		DebugUtils.Assert(this.m_ItemID != ItemID.None, "Missing Item in Tribes!", true, DebugUtils.AssertType.Info);
		base.Activate(parent);
	}

	public override void Update()
	{
		base.Update();
		if (InventoryBackpack.Get().Contains(this.m_ItemID))
		{
			this.Success();
		}
	}

	public override string GetLocalizedInfo()
	{
		Localization localization = GreenHellGame.Instance.GetLocalization();
		string text = localization.Get("HUDTribeRunawayChallenge_Tribe");
		text += " ";
		int num = 0;
		int num2 = 0;
		Player.Get().GetGPSCoordinates(this.m_Tribe.transform.position, out num, out num2);
		string text2 = text;
		text = string.Concat(new string[]
		{
			text2,
			num.ToString(),
			"'W ",
			num2.ToString(),
			"'S"
		});
		return text + "\n";
	}

	public override bool UpdateHUDChallengeInfo()
	{
		return true;
	}

	private ItemID m_ItemID = ItemID.None;

	private GameObject m_Tribe;
}
