using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class ItemSlot : MonoBehaviour
{
	public virtual bool IsLiquidSlot()
	{
		return false;
	}

	protected virtual void Awake()
	{
		this.m_Active = false;
		ItemSlot.s_AllItemSlots.Add(this);
		this.m_GOParent = ((!base.transform.parent) ? null : base.transform.parent.gameObject);
		if (this.m_GOParent)
		{
			this.m_ISParents = this.m_GOParent.GetComponents<IItemSlotParent>();
			this.m_ItemParent = this.m_GOParent.GetComponent<Item>();
			for (int i = 0; i < this.m_ISParents.Length; i++)
			{
				if (this.m_ISParents[i].GetType() == typeof(FoodProcessor))
				{
					this.m_FoodProcessorChild = true;
					break;
				}
			}
			this.m_WeaponRackParent = this.m_GOParent.GetComponent<WeaponRack>();
		}
		if (this.m_ItemType != null)
		{
			foreach (string value in this.m_ItemType)
			{
				this.m_ItemTypeList.Add((ItemType)Enum.Parse(typeof(ItemType), value));
			}
		}
		if (this.m_ItemID != null)
		{
			foreach (string value2 in this.m_ItemID)
			{
				this.m_ItemIDList.Add((ItemID)Enum.Parse(typeof(ItemID), value2));
			}
		}
		if (this.m_Crafting != null)
		{
			foreach (CraftingSlotData craftingSlotData in this.m_Crafting)
			{
				craftingSlotData.item_id = (ItemID)Enum.Parse(typeof(ItemID), craftingSlotData.id);
				this.m_ItemIDList.Add(craftingSlotData.item_id);
			}
		}
	}

	public void SetIcon(string icon_name)
	{
		this.m_AddIcon = icon_name;
	}

	public virtual bool IsStack()
	{
		return false;
	}

	public Vector3 GetCheckPosition()
	{
		if (this.m_ForceTransformPos)
		{
			return base.transform.position;
		}
		return (!(this.m_GOParent != null)) ? ((!this.m_PositionDummy) ? base.transform.position : this.m_PositionDummy.transform.position) : this.m_GOParent.transform.position;
	}

	public void UpdateActivity()
	{
		if (this.m_IsBeingDestroyed)
		{
			return;
		}
		if (!this.m_ActivityUpdate)
		{
			return;
		}
		if (this.m_NextUpdateActivityTime > Time.time)
		{
			return;
		}
		if (this.m_ShowOnlyIfItemIsCorrect && !this.CanInsertItem(Inventory3DManager.Get().m_CarriedItem))
		{
			this.Deactivate();
			this.m_NextUpdateActivityTime = Time.time;
			return;
		}
		if (this.m_BackpackSlot || (this.m_ItemParent && this.m_ItemParent.gameObject.activeSelf && this.m_ItemParent.m_InInventory))
		{
			if (InventoryBackpack.Get().gameObject.activeSelf && (!this.m_ItemParent || !this.m_ItemParent.m_OnCraftingTable))
			{
				this.Activate();
			}
			else
			{
				this.Deactivate();
			}
			this.m_NextUpdateActivityTime = Time.time;
			return;
		}
		float num = Vector3.Distance(Player.Get().transform.position, this.GetCheckPosition());
		if (num <= ItemSlot.s_DistToActivate)
		{
			this.Activate();
		}
		else
		{
			this.Deactivate();
		}
		this.m_NextUpdateActivityTime = Time.time + CJTools.Math.GetProportionalClamp(0.1f, 5f, num, 5f, 50f);
	}

	public virtual bool IsOccupied()
	{
		return this.m_Item != null;
	}

	public void Activate(bool activate)
	{
		if (activate)
		{
			this.Activate();
		}
		else
		{
			this.Deactivate();
		}
	}

	public void Activate()
	{
		if (this.m_Active)
		{
			return;
		}
		HUDItemSlot.Get().RegisterSlot(this);
		ItemSlot.s_ActiveItemSlots.Add(this);
		this.m_Active = true;
	}

	public void Deactivate()
	{
		if (!this.m_Active)
		{
			return;
		}
		HUDItemSlot.Get().UnregisterSlot(this);
		ItemSlot.s_ActiveItemSlots.Remove(this);
		this.m_Active = false;
	}

	public virtual bool CanInsertItem(Item item)
	{
		if (this.m_Blocked)
		{
			return false;
		}
		if (!item)
		{
			return false;
		}
		if (item == this.m_ItemParent)
		{
			return false;
		}
		if (this.IsOccupied())
		{
			return false;
		}
		if (((float)this.m_ItemTypeList.Count > 0f || this.m_ItemIDList.Count > 0) && !this.m_ItemTypeList.Contains(item.m_Info.m_Type) && !this.m_ItemIDList.Contains(item.m_Info.m_ID))
		{
			return false;
		}
		bool result = this.m_ISParents == null || this.m_ISParents.Length == 0;
		if (this.m_ISParents != null)
		{
			foreach (IItemSlotParent itemSlotParent in this.m_ISParents)
			{
				if (itemSlotParent.CanInsertItem(item))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public virtual void InsertItem(Item item)
	{
		this.m_Item = item;
		this.OnInsertItem(item);
	}

	protected virtual void OnInsertItem(Item item)
	{
		this.m_InsertTime = Time.time;
		if (item.m_PrevSlot == null)
		{
			item.m_PrevSlot = this;
		}
		item.m_Info.m_PrevInventoryCellsGroup = null;
		item.m_CurrentSlot = this;
		item.StaticPhxRequestAdd();
		item.UpdatePhx();
		item.ReseScale();
		Transform transform = (!(this.m_WeaponRackParent != null) || !item.m_WeaponRackHolder) ? item.m_InventoryHolder : item.m_WeaponRackHolder;
		if (transform)
		{
			Quaternion rhs = Quaternion.Inverse(transform.localRotation);
			item.gameObject.transform.rotation = base.transform.rotation;
			item.gameObject.transform.rotation *= rhs;
			Vector3 b = item.transform.position - transform.position;
			item.gameObject.transform.position = base.transform.position;
			item.gameObject.transform.position += b;
			item.gameObject.transform.parent = base.transform;
		}
		else
		{
			item.transform.parent = base.transform;
			item.transform.localPosition = Vector3.zero;
			if (this.m_AdjustRotation)
			{
				item.transform.localRotation = Quaternion.identity;
			}
		}
		if (this.m_ISParents != null)
		{
			foreach (IItemSlotParent itemSlotParent in this.m_ISParents)
			{
				itemSlotParent.OnInsertItem(this);
			}
		}
		this.UpdateActivity();
	}

	public virtual void RemoveItem(Item item, bool from_destroy = false)
	{
	}

	public void RemoveItem()
	{
		if (!this.m_Item)
		{
			return;
		}
		this.OnRemoveItem();
		this.m_Item.m_PrevSlot = this.m_Item.m_CurrentSlot;
		this.m_Item.m_CurrentSlot = null;
		this.m_Item.StaticPhxRequestRemove();
		this.m_Item.transform.parent = null;
		this.m_Item = null;
	}

	protected virtual void OnRemoveItem()
	{
		if (this.m_ISParents != null)
		{
			foreach (IItemSlotParent itemSlotParent in this.m_ISParents)
			{
				itemSlotParent.OnRemoveItem(this);
			}
		}
		this.UpdateActivity();
	}

	public void DeleteItem()
	{
		if (!this.m_Item)
		{
			return;
		}
		UnityEngine.Object.Destroy(this.m_Item.gameObject);
		this.m_Item = null;
		this.OnRemoveItem();
	}

	public void ReplaceItem(Item item)
	{
		this.DeleteItem();
		this.InsertItem(item);
	}

	private void OnDisable()
	{
		this.Deactivate();
	}

	public void OnDestroyItem(Item item)
	{
		if (this.m_Item == item)
		{
			this.OnRemoveItem();
		}
	}

	private void OnDestroy()
	{
		this.m_IsBeingDestroyed = true;
		this.Deactivate();
		ItemSlot.s_AllItemSlots.Remove(this);
	}

	public virtual Vector3 GetScreenPoint()
	{
		Vector3 position = (!this.m_PositionDummy) ? base.transform.position : this.m_PositionDummy.transform.position;
		if (this.m_Camera)
		{
			return Camera.main.ViewportToScreenPoint(this.m_Camera.WorldToViewportPoint(position));
		}
		if (this.m_ItemParent && this.m_ItemParent.m_InInventory)
		{
			return Camera.main.ViewportToScreenPoint(Inventory3DManager.Get().m_Camera.WorldToViewportPoint(position));
		}
		if (Camera.main)
		{
			return Camera.main.WorldToScreenPoint(position);
		}
		return Vector3.zero;
	}

	public virtual bool IsBIWoundSlot()
	{
		return false;
	}

	public GameObject m_GOParent;

	public IItemSlotParent[] m_ISParents;

	[HideInInspector]
	public Item m_ItemParent;

	[HideInInspector]
	public Item m_Item;

	[HideInInspector]
	public float m_InsertTime;

	public string[] m_ItemType;

	[HideInInspector]
	[NonSerialized]
	public List<ItemType> m_ItemTypeList = new List<ItemType>();

	public string[] m_ItemID;

	[HideInInspector]
	[NonSerialized]
	public List<ItemID> m_ItemIDList = new List<ItemID>();

	public CraftingSlotData[] m_Crafting;

	public static float s_DistToActivate = 3f;

	public static List<ItemSlot> s_AllItemSlots = new List<ItemSlot>();

	public static List<ItemSlot> s_ActiveItemSlots = new List<ItemSlot>();

	private float m_NextUpdateActivityTime;

	public bool m_ActivityUpdate = true;

	[HideInInspector]
	public bool m_Active;

	[HideInInspector]
	public bool m_AdjustRotation = true;

	[HideInInspector]
	public bool m_ShowIconIfFull = true;

	[HideInInspector]
	public Camera m_Camera;

	public GameObject m_PositionDummy;

	public bool m_BackpackSlot;

	public bool m_InventoryStackSlot;

	[NonSerialized]
	public float m_AttrRange = 60f;

	[HideInInspector]
	public bool m_Blocked;

	[HideInInspector]
	public bool m_ShowOnlyIfItemIsCorrect;

	[HideInInspector]
	public string m_AddIcon = string.Empty;

	[HideInInspector]
	public bool m_FoodProcessorChild;

	[HideInInspector]
	public bool m_IsBeingDestroyed;

	[HideInInspector]
	public bool m_ForceTransformPos;

	private WeaponRack m_WeaponRackParent;
}
