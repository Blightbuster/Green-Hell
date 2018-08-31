using System;
using Enums;

public class NotepadPlantCraftingData : NotepadData
{
	public override bool ShouldShow()
	{
		return this.m_ItenName.Length > 2 && ItemsManager.Get().WasCrafted((ItemID)Enum.Parse(typeof(ItemID), this.m_ItenName));
	}

	public string m_ItenName = string.Empty;
}
