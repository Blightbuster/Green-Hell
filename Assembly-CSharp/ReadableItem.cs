using System;
using System.Collections.Generic;

public class ReadableItem : Item
{
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
		HUDReadableItem.Get().Activate(base.gameObject.name);
		this.m_WasReaded = true;
	}

	protected override void UpdateLayer()
	{
		int num = this.m_DefaultLayer;
		if (this == TriggerController.Get().GetBestTrigger() && this.CanBeOutlined())
		{
			num = this.m_OutlineLayer;
		}
		else if (this.m_InInventory || this.m_OnCraftingTable || Inventory3DManager.Get().m_CarriedItem == this)
		{
			num = this.m_InventoryLayer;
		}
		if (base.gameObject.layer != num)
		{
			base.SetLayer(base.transform, num);
		}
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("RIWasReaded" + index, this.m_WasReaded);
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.m_WasReaded = SaveGame.LoadBVal("RIWasReaded" + index);
	}

	public bool m_WasReaded;
}
