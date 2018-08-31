using System;
using Enums;

public class NotepadPlantsData : NotepadData
{
	public override bool ShouldShow()
	{
		return ItemsManager.Get().WasCollected((ItemID)Enum.Parse(typeof(ItemID), this.m_ItenName));
	}

	public string m_ItenName = string.Empty;
}
