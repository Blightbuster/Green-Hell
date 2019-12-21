using System;
using Enums;

public class ArmorSlot : ItemSlot
{
	public override bool CanInsertItem(Item item)
	{
		return !(item == null) && item.m_Info != null && !(this.m_Item != null) && item.m_Info.m_ID != ItemID.broken_armor && item.m_Info.IsArmor();
	}

	protected override void OnInsertItem(Item item)
	{
		base.OnInsertItem(item);
		PlayerArmorModule.Get().OnArmorInsert(this, item);
	}

	public override bool IsArmorSlot()
	{
		return true;
	}

	public Limb m_Limb = Limb.None;
}
