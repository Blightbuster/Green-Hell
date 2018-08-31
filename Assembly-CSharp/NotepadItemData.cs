using System;
using Enums;

public class NotepadItemData : NotepadData
{
	public override void Init()
	{
		base.Init();
		this.m_ID = (ItemID)Enum.Parse(typeof(ItemID), this.m_ItemID);
	}

	public override bool ShouldShow()
	{
		return ItemsManager.Get().m_UnlockedInNotepadItems.Contains(this.m_ID);
	}

	public string m_ItemID = string.Empty;

	private ItemID m_ID = ItemID.None;
}
