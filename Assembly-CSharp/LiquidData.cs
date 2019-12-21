using System;
using System.Collections.Generic;
using Enums;

public class LiquidData
{
	public LiquidType m_LiquidType;

	public LiquidType m_LiquidComponent;

	public ItemID m_ItemComponent = ItemID.None;

	public LiquidType m_CookingResult;

	public float m_Fat;

	public float m_Carbohydrates;

	public float m_Proteins;

	public float m_Water;

	public float m_Energy;

	public int m_SanityChange;

	public int m_PoisonDebuff;

	public List<LiquidConsumeEffectData> m_ConsumeEffects = new List<LiquidConsumeEffectData>();

	public bool m_Disgusting;
}
