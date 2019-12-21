using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class Door : Trigger
{
	protected override void Awake()
	{
		base.Awake();
		if (this.m_RequiredItemInfoName0 != string.Empty && Enum.IsDefined(typeof(ItemID), this.m_RequiredItemInfoName0))
		{
			this.m_RequiredItemID0 = (ItemID)Enum.Parse(typeof(ItemID), this.m_RequiredItemInfoName0);
		}
		if (this.m_RequiredItemInfoName1 != string.Empty && Enum.IsDefined(typeof(ItemID), this.m_RequiredItemInfoName1))
		{
			this.m_RequiredItemID1 = (ItemID)Enum.Parse(typeof(ItemID), this.m_RequiredItemInfoName1);
		}
		Transform transform = base.transform.Find("Hinge");
		this.m_Hinge = (transform ? transform.gameObject : null);
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		Door.DoorState doorState = this.m_DoorState;
		if (doorState == Door.DoorState.Closed)
		{
			actions.Add(TriggerAction.TYPE.Open);
			return;
		}
		if (doorState != Door.DoorState.Open)
		{
			return;
		}
		actions.Add(TriggerAction.TYPE.Close);
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.Open)
		{
			this.SetState(Door.DoorState.Opening);
			return;
		}
		if (action == TriggerAction.TYPE.Close)
		{
			this.SetState(Door.DoorState.Closing);
		}
	}

	private void SetState(Door.DoorState state)
	{
		this.m_DoorState = state;
	}

	public override string GetName()
	{
		if (this.m_DoorState != Door.DoorState.Locked)
		{
			return "HUD_Door";
		}
		return "HUD_DoorLocked";
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateState();
	}

	public override void UpdateBestTrigger()
	{
		base.UpdateBestTrigger();
		if (this.m_RequiredItemID0 == ItemID.None && this.m_RequiredItemID1 == ItemID.None)
		{
			return;
		}
		if (this.m_DoorState == Door.DoorState.Locked)
		{
			if ((InventoryBackpack.Get().Contains(this.m_RequiredItemID0) || this.m_RequiredItemID0 == ItemID.None) && (InventoryBackpack.Get().Contains(this.m_RequiredItemID1) || this.m_RequiredItemID1 == ItemID.None))
			{
				this.SetState(Door.DoorState.Closed);
				return;
			}
		}
		else if (this.m_DoorState == Door.DoorState.Closed && ((!InventoryBackpack.Get().Contains(this.m_RequiredItemID0) && this.m_RequiredItemID0 != ItemID.None) || (!InventoryBackpack.Get().Contains(this.m_RequiredItemID1) && this.m_RequiredItemID1 != ItemID.None)))
		{
			this.SetState(Door.DoorState.Locked);
		}
	}

	private void UpdateState()
	{
		Door.DoorState doorState = this.m_DoorState;
		if (doorState != Door.DoorState.Closing)
		{
			if (doorState == Door.DoorState.Opening)
			{
				base.transform.RotateAround(this.m_Hinge ? this.m_Hinge.transform.position : base.transform.position, Vector3.up, 90f);
				this.SetState(Door.DoorState.Open);
				return;
			}
		}
		else
		{
			base.transform.RotateAround(this.m_Hinge ? this.m_Hinge.transform.position : base.transform.position, Vector3.up, -90f);
			this.SetState(Door.DoorState.Closed);
		}
	}

	public Door.DoorState m_DoorState = Door.DoorState.Closed;

	private GameObject m_Hinge;

	[HideInInspector]
	public string m_RequiredItemInfoName0 = string.Empty;

	[HideInInspector]
	public string m_RequiredItemInfoName1 = string.Empty;

	private ItemID m_RequiredItemID0 = ItemID.None;

	private ItemID m_RequiredItemID1 = ItemID.None;

	public enum DoorState
	{
		Locked,
		Closed,
		Closing,
		Open,
		Opening
	}
}
