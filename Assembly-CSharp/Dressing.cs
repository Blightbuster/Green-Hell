using System;

public class Dressing : Item
{
	protected override void Awake()
	{
		base.Awake();
		this.m_InventoryManager = Inventory3DManager.Get();
	}

	public override bool CanBeOutlined()
	{
		return !this.m_InventoryManager.m_CarriedItem == this;
	}

	private Inventory3DManager m_InventoryManager;
}
