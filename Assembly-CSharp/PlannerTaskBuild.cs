using System;
using Enums;

public class PlannerTaskBuild : PlannerTask
{
	public override void Parse(Key key)
	{
		base.Parse(key);
		this.m_ItemID = (ItemID)Enum.Parse(typeof(ItemID), key.GetVariable(7).SValue);
	}

	public override bool OnBuild(ItemID item, bool planned)
	{
		if (item != this.m_ItemID)
		{
			return false;
		}
		if (planned)
		{
			PlayerSanityModule.Get().OnEvent(PlayerSanityModule.SanityEventType.PlannedAction, 1);
			return true;
		}
		return false;
	}

	private ItemID m_ItemID = ItemID.None;
}
