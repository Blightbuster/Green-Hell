using System;
using Enums;

public class NotepadPlantBoiledData : NotepadData
{
	public override bool ShouldShow()
	{
		return ItemsManager.Get().WasLiquidBoiled((LiquidType)Enum.Parse(typeof(LiquidType), this.m_LiquidType));
	}

	public string m_LiquidType = string.Empty;
}
