using System;

public interface IItemSlotParent
{
	bool CanInsertItem(Item item);

	void OnInsertItem(ItemSlot slot);

	void OnRemoveItem(ItemSlot slot);
}
