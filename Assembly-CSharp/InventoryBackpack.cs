using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class InventoryBackpack : MonoBehaviour
{
	public static InventoryBackpack Get()
	{
		return InventoryBackpack.s_Instance;
	}

	private void Awake()
	{
		InventoryBackpack.s_Instance = this;
		this.m_CriticalWeight = 0.9f * this.m_MaxWeight;
		this.m_OverloadWeight = 0.7f * this.m_MaxWeight;
		this.m_Backpack.Initialize();
		for (int i = 0; i < this.m_Backpack.transform.childCount; i++)
		{
			Transform child = this.m_Backpack.transform.GetChild(i);
			if (child.name.Contains("TopSlot"))
			{
				this.m_TopSlots.Add(child.GetComponent<ItemSlot>());
			}
			else if (child.name.Contains("LeftSlot"))
			{
				this.m_LeftSlots.Add(child.GetComponent<ItemSlot>());
			}
			else if (child.name.Contains("RightSlot"))
			{
				this.m_RightSlots.Add(child.GetComponent<ItemSlot>());
			}
			else if (child.name.Contains("MainPlane"))
			{
				this.m_MainPlane = child.GetComponent<Collider>();
			}
			else if (child.name.Contains("FrontPlane"))
			{
				this.m_FrontPlane = child.GetComponent<Collider>();
			}
		}
		this.m_MainPocketGrid = new PocketGrid();
		this.m_MainPocketGrid.m_Grid = this.m_MainPlane.gameObject;
		this.m_MainPocketGrid.m_GridSize = new Vector2(10f, 15f);
		this.m_MainPocketGrid.Initialize();
		this.m_FrontPocketGrid = new PocketGrid();
		this.m_FrontPocketGrid.m_Grid = this.m_FrontPlane.gameObject;
		this.m_FrontPocketGrid.m_GridSize = new Vector2(10f, 10f);
		this.m_FrontPocketGrid.Initialize();
	}

	private void Start()
	{
		Camera camera = Inventory3DManager.Get().m_Camera;
		foreach (ItemSlot itemSlot in this.m_LeftSlots)
		{
			itemSlot.m_Camera = camera;
			itemSlot.m_BackpackSlot = true;
			itemSlot.m_ShowIconIfFull = false;
			itemSlot.m_ShowOnlyIfItemIsCorrect = true;
		}
		this.m_EquippedItemSlot = this.m_LeftSlots[0];
		foreach (ItemSlot itemSlot2 in this.m_RightSlots)
		{
			itemSlot2.m_Camera = camera;
			itemSlot2.m_BackpackSlot = true;
			itemSlot2.m_ShowIconIfFull = false;
			itemSlot2.m_ShowOnlyIfItemIsCorrect = true;
		}
		foreach (ItemSlot itemSlot3 in this.m_TopSlots)
		{
			itemSlot3.m_Camera = camera;
			itemSlot3.m_BackpackSlot = true;
			itemSlot3.m_ShowIconIfFull = false;
			itemSlot3.m_ShowOnlyIfItemIsCorrect = true;
		}
	}

	public void SetupPocket(BackpackPocket pocket)
	{
		this.m_ActivePlane = null;
		this.m_ActivePocket = pocket;
		this.SetBackpackTransform(pocket);
		PocketGrid pocketGrid = null;
		if (pocket != BackpackPocket.Main)
		{
			if (pocket == BackpackPocket.Front)
			{
				this.m_ActivePlane = this.m_FrontPlane;
				this.m_MainPlane.gameObject.SetActive(false);
				this.m_ActivePlane.gameObject.SetActive(true);
				this.m_FrontPocketGrid.Setup();
				pocketGrid = this.m_FrontPocketGrid;
			}
		}
		else
		{
			this.m_ActivePlane = this.m_MainPlane;
			this.m_FrontPlane.gameObject.SetActive(false);
			this.m_ActivePlane.gameObject.SetActive(true);
			this.m_MainPocketGrid.Setup();
			pocketGrid = this.m_MainPocketGrid;
		}
		if (pocket != BackpackPocket.None)
		{
			List<GameObject> objectToDisable = this.m_Backpack.GetObjectToDisable(pocket);
			for (int i = 0; i < this.m_Backpack.transform.childCount; i++)
			{
				GameObject gameObject = this.m_Backpack.transform.GetChild(i).gameObject;
				gameObject.SetActive(!objectToDisable.Contains(this.m_Backpack.transform.GetChild(i).gameObject));
			}
			foreach (Item item in this.m_Items)
			{
				item.gameObject.SetActive(item.m_Info.m_BackpackPocket == pocket);
				if (pocketGrid != null && item.m_Info.m_InventoryCellsGroup != null && item.m_CurrentSlot == null)
				{
					item.m_Info.m_InventoryCellsGroup.SetupTansform(item, pocketGrid.m_Grid);
				}
			}
		}
	}

	private void SetBackpackTransform(BackpackPocket pocket)
	{
		if (pocket != BackpackPocket.None && this.m_Backpack)
		{
			this.m_Backpack.transform.localPosition = this.m_Backpack.m_PocketPosition[(int)pocket];
			this.m_Backpack.transform.localRotation = this.m_Backpack.m_PocketRotation[(int)pocket];
		}
	}

	public bool IsBackpackCollider(Collider collider)
	{
		return collider == this.m_MainPlane || collider == this.m_FrontPlane;
	}

	public void RemoveItem(ItemID id, int count = 1)
	{
		int num = 0;
		int i = 0;
		while (i < this.m_Items.Count)
		{
			if (this.m_Items[i].m_Info.m_ID == id)
			{
				this.RemoveItem(this.m_Items[i], false);
				num++;
				if (num >= count)
				{
					break;
				}
			}
			else
			{
				i++;
			}
		}
	}

	public void RemoveItem(Item item, bool from_destroy = false)
	{
		if (!this.m_Items.Contains(item))
		{
			return;
		}
		if (item.m_CurrentSlot)
		{
			if (item.m_CurrentSlot.IsStack())
			{
				item.m_CurrentSlot.RemoveItem(item, false);
			}
			else
			{
				item.m_CurrentSlot.RemoveItem();
			}
		}
		else if (item.m_InventorySlot && item.m_InventorySlot.m_Items.Count > 0)
		{
			item.m_InventorySlot.RemoveItem(item, from_destroy);
		}
		item.transform.parent = null;
		if (item.m_Info.m_InventoryCellsGroup != null)
		{
			item.m_Info.m_InventoryCellsGroup.Remove(item);
			item.m_Info.m_InventoryCellsGroup = null;
		}
		this.m_Items.Remove(item);
		item.OnRemoveFromInventory();
		this.OnInventoryChanged();
	}

	public InventoryBackpack.InsertResult InsertItem(Item item, ItemSlot slot = null, InventoryCellsGroup group = null, bool can_stack = true, bool drop_if_cant = true, bool notify_if_cant = true, bool can_auto_select_group = true, bool set_pocket = true)
	{
		if (!item.m_Info.m_CanBeAddedToInventory)
		{
			Inventory3DManager.Get().DropItem(item);
			return InventoryBackpack.InsertResult.CantInsert;
		}
		if (this.m_Items.Contains(item))
		{
			return InventoryBackpack.InsertResult.AllreadyInInventory;
		}
		bool isStatic = item.gameObject.isStatic;
		item.gameObject.isStatic = false;
		item.transform.parent = null;
		this.SetBackpackTransform(item.m_Info.m_BackpackPocket);
		if (set_pocket && Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().SetupPocket(item.m_Info.m_BackpackPocket);
		}
		InventoryBackpack.InsertResult insertResult = InventoryBackpack.InsertResult.None;
		if (item.m_Info.m_BackpackPocket == BackpackPocket.Top)
		{
			insertResult = this.InsertItemTop(item, slot);
		}
		else if (item.m_Info.m_BackpackPocket == BackpackPocket.Left)
		{
			insertResult = this.InsertItemLeft(item, slot);
		}
		else if (item.m_Info.m_BackpackPocket == BackpackPocket.Right)
		{
			insertResult = this.InsertItemRight(item, slot);
		}
		else if (item.m_Info.m_BackpackPocket == BackpackPocket.Main)
		{
			insertResult = this.InsertItemMain(item, slot, group, can_stack, can_auto_select_group);
		}
		else if (item.m_Info.m_BackpackPocket == BackpackPocket.Front)
		{
			insertResult = this.InsertItemFront(item, slot, group, can_stack, can_auto_select_group);
		}
		if (insertResult == InventoryBackpack.InsertResult.Ok)
		{
			this.m_Items.Add(item);
			if (!item.m_CurrentSlot)
			{
				item.transform.parent = base.transform;
			}
			else if (item.m_CurrentSlot == this.m_EquippedItemSlot)
			{
				this.m_EquippedItem = item;
			}
			item.OnAddToInventory();
			item.gameObject.SetActive(Inventory3DManager.Get().gameObject.activeSelf && item.m_Info.m_BackpackPocket == Inventory3DManager.Get().m_ActivePocket);
			this.OnInventoryChanged();
		}
		else
		{
			item.gameObject.isStatic = isStatic;
			if (notify_if_cant)
			{
				HUDMessages hudmessages = (HUDMessages)HUDManager.Get().GetHUD(typeof(HUDMessages));
				hudmessages.AddMessage(GreenHellGame.Instance.GetLocalization().Get("HUD_NoSpaceInBackpack"), new Color?(Color.red), HUDMessageIcon.None, string.Empty);
				HUDManager.Get().PlaySound("Sounds/HUD/GH_Inventory_Full");
				if (!drop_if_cant)
				{
					item.ApplyImpulse(new Vector3(0f, 100f, 0f), new Vector3(0f, 0f, UnityEngine.Random.Range(-100f, 100f)));
				}
			}
			if (drop_if_cant)
			{
				Inventory3DManager.Get().DropItem(item);
			}
		}
		this.SetBackpackTransform(Inventory3DManager.Get().m_ActivePocket);
		return insertResult;
	}

	private InventoryBackpack.InsertResult InsertItemTop(Item item, ItemSlot slot)
	{
		if (!slot)
		{
			slot = this.FindFreeSlot(item);
		}
		if (slot && slot.CanInsertItem(item))
		{
			slot.InsertItem(item);
			return InventoryBackpack.InsertResult.Ok;
		}
		return InventoryBackpack.InsertResult.NoSpace;
	}

	private InventoryBackpack.InsertResult InsertItemLeft(Item item, ItemSlot slot)
	{
		if (!slot)
		{
			slot = this.FindFreeSlot(item);
		}
		if (slot && slot.CanInsertItem(item))
		{
			slot.InsertItem(item);
			return InventoryBackpack.InsertResult.Ok;
		}
		return InventoryBackpack.InsertResult.NoSpace;
	}

	private InventoryBackpack.InsertResult InsertItemRight(Item item, ItemSlot slot)
	{
		if (!slot)
		{
			slot = this.FindFreeSlot(item);
		}
		if (slot && slot.CanInsertItem(item))
		{
			slot.InsertItem(item);
			return InventoryBackpack.InsertResult.Ok;
		}
		return InventoryBackpack.InsertResult.NoSpace;
	}

	private InventoryBackpack.InsertResult InsertItemMain(Item item, ItemSlot slot, InventoryCellsGroup group, bool can_stack, bool can_auto_select_group)
	{
		if (this.m_MainPocketGrid.InsertItem(item, slot, group, can_stack, can_auto_select_group))
		{
			return InventoryBackpack.InsertResult.Ok;
		}
		return InventoryBackpack.InsertResult.NoSpace;
	}

	private InventoryBackpack.InsertResult InsertItemFront(Item item, ItemSlot slot, InventoryCellsGroup group, bool can_stack, bool can_auto_select_group)
	{
		if (this.m_FrontPocketGrid.InsertItem(item, slot, group, can_stack, can_auto_select_group))
		{
			return InventoryBackpack.InsertResult.Ok;
		}
		return InventoryBackpack.InsertResult.NoSpace;
	}

	public ItemSlot FindFreeSlot(Item item)
	{
		if (item.m_Info.m_BackpackPocket == BackpackPocket.Left)
		{
			if (this.m_EquippedItemSlot && !this.m_EquippedItem)
			{
				return this.m_EquippedItemSlot;
			}
			foreach (ItemSlot itemSlot in this.m_LeftSlots)
			{
				if (!this.m_EquippedItem || !(this.m_EquippedItemSlot == itemSlot))
				{
					if (itemSlot.CanInsertItem(item))
					{
						return itemSlot;
					}
				}
			}
		}
		else if (item.m_Info.m_BackpackPocket == BackpackPocket.Right)
		{
			foreach (ItemSlot itemSlot2 in this.m_RightSlots)
			{
				if (itemSlot2.CanInsertItem(item))
				{
					return itemSlot2;
				}
			}
		}
		else if (item.m_Info.m_BackpackPocket == BackpackPocket.Top)
		{
			foreach (ItemSlot itemSlot3 in this.m_TopSlots)
			{
				if (itemSlot3.CanInsertItem(item))
				{
					return itemSlot3;
				}
			}
		}
		return null;
	}

	public InventoryCellsGroup FindBestGroup(BackpackPocket pocket)
	{
		if (pocket == BackpackPocket.Main)
		{
			return this.m_MainPocketGrid.FindBestGroup();
		}
		if (pocket != BackpackPocket.Front)
		{
			return null;
		}
		return this.m_FrontPocketGrid.FindBestGroup();
	}

	public void OnSetSelectedGroup(BackpackPocket pocket, InventoryCellsGroup group)
	{
		if (pocket != BackpackPocket.Main)
		{
			if (pocket == BackpackPocket.Front)
			{
				this.m_FrontPocketGrid.OnSetSelectedGroup(group);
			}
		}
		else
		{
			this.m_MainPocketGrid.OnSetSelectedGroup(group);
		}
	}

	public bool IsOverload()
	{
		return this.m_CurrentWeight > this.m_OverloadWeight;
	}

	public bool IsCriticalOverload()
	{
		return this.m_CurrentWeight > this.m_CriticalWeight;
	}

	public bool IsMaxOverload()
	{
		return this.m_CurrentWeight > this.m_MaxWeight;
	}

	public void CalculateCurrentWeight()
	{
		this.m_CurrentWeight = 0f;
		foreach (Item item in this.m_Items)
		{
			this.m_CurrentWeight += item.m_Info.GetMass();
		}
	}

	public void OnInventoryChanged()
	{
		this.CalculateCurrentWeight();
		foreach (Item item in this.m_Items)
		{
			item.UpdatePhx();
			item.UpdateScale(false);
		}
		HUDGather.Get().Setup();
		HUDBackpack.Get().SetupPocket(Inventory3DManager.Get().m_ActivePocket);
	}

	public bool HaveAnyItem(string excluded)
	{
		if (excluded.Length > 0)
		{
			ItemID itemID = (ItemID)Enum.Parse(typeof(ItemID), excluded);
			for (int i = 0; i < this.m_Items.Count; i++)
			{
				if (this.m_Items[i].m_Info.m_ID != itemID)
				{
					return true;
				}
			}
			return false;
		}
		return this.m_Items.Count > 0;
	}

	public int GetItemsCount(ItemID id)
	{
		int num = 0;
		foreach (Item item in this.m_Items)
		{
			if (item.m_Info.m_ID == id)
			{
				num++;
			}
		}
		return num;
	}

	public int GetFoodItemsCount()
	{
		int num = 0;
		foreach (Item item in this.m_Items)
		{
			if (item.m_Info.IsFood())
			{
				num++;
			}
		}
		return num;
	}

	public bool Contains(Item item)
	{
		return this.m_Items.Contains(item);
	}

	public bool ContainsItem(string item_name)
	{
		ItemID id = (ItemID)Enum.Parse(typeof(ItemID), item_name);
		return this.Contains(id);
	}

	public bool Contains(ItemID id)
	{
		foreach (Item item in this.m_Items)
		{
			if (item.m_Info.m_ID == id)
			{
				return true;
			}
		}
		return false;
	}

	public bool Contains(ItemType type)
	{
		foreach (Item item in this.m_Items)
		{
			if (item.m_Info.m_Type == type)
			{
				return true;
			}
		}
		return false;
	}

	public Item FindItem(ItemType type)
	{
		foreach (Item item in this.m_Items)
		{
			if (item.m_Info.m_Type == type)
			{
				return item;
			}
		}
		return null;
	}

	public Item FindItem(ItemID item_id)
	{
		foreach (Item item in this.m_Items)
		{
			if (item.m_Info.m_ID == item_id)
			{
				return item;
			}
		}
		return null;
	}

	private void Update()
	{
		foreach (Item item in this.m_Items)
		{
			if (item is Torch)
			{
				item.CheckIfInBackPack();
			}
		}
	}

	private void SetPlaneColor(BoxCollider plane, Color color)
	{
		Component[] components = plane.gameObject.GetComponents(typeof(Renderer));
		foreach (Renderer renderer in components)
		{
			foreach (Material material in renderer.materials)
			{
				material.color = color;
			}
		}
	}

	public bool HaveLiquid(string liquid_type, float amount)
	{
		LiquidType liquidType = (LiquidType)Enum.Parse(typeof(LiquidType), liquid_type);
		foreach (Item item in this.m_Items)
		{
			if (item.m_Info.IsLiquidContainer())
			{
				LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)item.m_Info;
				if (liquidContainerInfo.m_LiquidType == liquidType && liquidContainerInfo.m_Amount >= amount)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void DeleteAllItems()
	{
		foreach (Item item in this.m_Items)
		{
			if (item.m_CurrentSlot)
			{
				if (item.m_CurrentSlot.IsStack())
				{
					item.m_CurrentSlot.RemoveItem(item, false);
				}
				else
				{
					item.m_CurrentSlot.RemoveItem();
				}
			}
			item.transform.parent = null;
			if (item.m_Info.m_InventoryCellsGroup != null)
			{
				item.m_Info.m_InventoryCellsGroup.Remove(item);
				item.m_Info.m_InventoryCellsGroup = null;
			}
			UnityEngine.Object.Destroy(item.gameObject);
		}
		this.m_Items.Clear();
		this.OnInventoryChanged();
	}

	public ItemSlot GetSlotByIndex(int index, BackpackPocket pocked)
	{
		if (pocked == BackpackPocket.Left)
		{
			return this.m_LeftSlots[index];
		}
		if (pocked == BackpackPocket.Right)
		{
			return this.m_RightSlots[index];
		}
		if (pocked == BackpackPocket.Top)
		{
			return this.m_TopSlots[index];
		}
		return null;
	}

	public ItemSlot GetSlotByName(string name, BackpackPocket pocked)
	{
		if (name == null || name == string.Empty)
		{
			return null;
		}
		if (pocked == BackpackPocket.Left)
		{
			for (int i = 0; i < this.m_LeftSlots.Count; i++)
			{
				if (this.m_LeftSlots[i].name == name)
				{
					return this.m_LeftSlots[i];
				}
			}
		}
		else if (pocked == BackpackPocket.Right)
		{
			for (int j = 0; j < this.m_RightSlots.Count; j++)
			{
				if (this.m_RightSlots[j].name == name)
				{
					return this.m_RightSlots[j];
				}
			}
		}
		else if (pocked == BackpackPocket.Top)
		{
			for (int k = 0; k < this.m_TopSlots.Count; k++)
			{
				if (this.m_TopSlots[k].name == name)
				{
					return this.m_TopSlots[k];
				}
			}
		}
		DebugUtils.Assert(name + " - " + pocked.ToString(), true, DebugUtils.AssertType.Info);
		return null;
	}

	public InventoryCell GetCellByName(string name, BackpackPocket pocked)
	{
		if (pocked == BackpackPocket.Main)
		{
			return this.m_MainPocketGrid.GetCellByName(name);
		}
		if (pocked == BackpackPocket.Front)
		{
			return this.m_FrontPocketGrid.GetCellByName(name);
		}
		DebugUtils.Assert(DebugUtils.AssertType.Info);
		return null;
	}

	public Backpack m_Backpack;

	private List<ItemSlot> m_LeftSlots = new List<ItemSlot>();

	private List<ItemSlot> m_RightSlots = new List<ItemSlot>();

	private List<ItemSlot> m_TopSlots = new List<ItemSlot>();

	private Collider m_MainPlane;

	private Collider m_FrontPlane;

	[HideInInspector]
	public Collider m_ActivePlane;

	private PocketGrid m_MainPocketGrid;

	private PocketGrid m_FrontPocketGrid;

	public List<Item> m_Items = new List<Item>();

	private float m_OverloadWeight = 35f;

	private float m_CriticalWeight = 50f;

	public float m_MaxWeight = 50f;

	public float m_CurrentWeight;

	private BackpackPocket m_ActivePocket = BackpackPocket.None;

	public Color m_FreeColor = Color.white;

	public Color m_OccupiedColor = Color.white;

	public ItemSlot m_EquippedItemSlot;

	public Item m_EquippedItem;

	private static InventoryBackpack s_Instance;

	public enum InsertResult
	{
		None,
		Ok,
		NoSpace,
		CantInsert,
		AllreadyInInventory
	}
}
