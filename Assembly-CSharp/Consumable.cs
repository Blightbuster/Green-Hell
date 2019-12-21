using System;

public class Consumable : Item
{
	public override void OnTake()
	{
		base.OnTake();
		if (((ConsumableInfo)this.m_Info).m_Disgusting)
		{
			PlayerSanityModule.Get().OnWhispersEvent(PlayerSanityModule.WhisperType.TakeItemBad);
			return;
		}
		PlayerSanityModule.Get().OnWhispersEvent(PlayerSanityModule.WhisperType.TakeItemGood);
	}
}
