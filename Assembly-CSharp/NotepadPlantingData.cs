using System;
using Enums;

public class NotepadPlantingData : NotepadData
{
	public override bool ShouldShow()
	{
		return ItemsManager.Get().WasPlanted((ItemID)Enum.Parse(typeof(ItemID), this.m_ItenName));
	}

	public string m_ItenName = string.Empty;
}
