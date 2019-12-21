using System;
using System.Collections.Generic;

public class ReadableItem : Item
{
	protected override void Start()
	{
		base.Initialize(false);
		base.Start();
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		actions.Add(TriggerAction.TYPE.Read);
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action != TriggerAction.TYPE.Read)
		{
			return;
		}
		HUDReadableItem.Get().Activate(base.gameObject.name, this);
		this.m_WasReaded = true;
	}

	public override void UpdateLayer()
	{
		if (TriggerController.Get() == null)
		{
			return;
		}
		int num = this.m_DefaultLayer;
		if (this == TriggerController.Get().GetBestTrigger() && this.CanBeOutlined())
		{
			num = this.m_OutlineLayer;
		}
		else if (base.m_InInventory || base.m_OnCraftingTable || base.m_InStorage || Inventory3DManager.Get().m_CarriedItem == this)
		{
			num = this.m_InventoryLayer;
		}
		if (base.gameObject.layer != num)
		{
			this.SetLayer(base.transform, num);
		}
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("RIWasReaded" + index, this.m_WasReaded);
		SaveGame.SaveVal("RIWasReadedAndOff" + index, this.m_WasReadedAndOff);
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.m_WasReaded = SaveGame.LoadBVal("RIWasReaded" + index);
		this.m_WasReadedAndOff = SaveGame.LoadBVal("RIWasReadedAndOff" + index);
	}

	public bool m_WasReaded;

	public bool m_WasReadedAndOff;
}
