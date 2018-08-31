using System;
using Enums;

public class NotepadConstructionData : NotepadData
{
	public override bool ShouldShow()
	{
		return ItemsManager.Get().m_UnlockedInNotepadItems.Contains((ItemID)Enum.Parse(typeof(ItemID), this.m_ItemID));
	}

	public string m_ItemID = string.Empty;
}
