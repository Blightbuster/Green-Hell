using System;
using Enums;

public class LiquidSlot : ItemSlot
{
	public override bool IsLiquidSlot()
	{
		return true;
	}

	private void Start()
	{
		this.m_ItemTypeList.Clear();
		this.m_ItemIDList.Clear();
		this.m_ItemTypeList.Add(ItemType.LiquidContainer);
	}

	protected override void OnInsertItem(Item item)
	{
		base.OnInsertItem(item);
		this.m_Container.Fill((LiquidContainer)item);
		Inventory3DManager.Get().OnLiquidTransfer();
		base.RemoveItem();
	}

	public LiquidContainer m_Container;
}
