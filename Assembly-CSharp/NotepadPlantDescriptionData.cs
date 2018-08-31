using System;
using Enums;

public class NotepadPlantDescriptionData : NotepadData
{
	public override bool ShouldShow()
	{
		ItemID item = (ItemID)Enum.Parse(typeof(ItemID), this.m_ItemID);
		return ItemsManager.Get().m_UnlockedItemInfos.Contains(item);
	}

	public string m_ItemID = string.Empty;
}
