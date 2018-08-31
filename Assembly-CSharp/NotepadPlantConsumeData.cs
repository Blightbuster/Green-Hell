using System;
using Enums;

public class NotepadPlantConsumeData : NotepadData
{
	public override bool ShouldShow()
	{
		return ItemsManager.Get().WasConsumed((ItemID)Enum.Parse(typeof(ItemID), this.m_ItenName));
	}

	public string m_ItenName = string.Empty;
}
