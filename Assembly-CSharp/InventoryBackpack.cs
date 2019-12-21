using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;

public class InventoryBackpack : MonoBehaviour, ISaveLoad
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
		this.m_MainPocketGrid.Initialize(InventoryBackpack.Get().m_Backpack.m_GridCellPrefab, BackpackPocket.Main);
		this.m_FrontPocketGrid = new PocketGrid();
		this.m_FrontPocketGrid.m_Grid = this.m_FrontPlane.gameObject;
		this.m_FrontPocketGrid.m_GridSize = new Vector2(10f, 10f);
		this.m_FrontPocketGrid.Initialize(InventoryBackpack.Get().m_Backpack.m_GridCellPrefab, BackpackPocket.Front);
		foreach (GameObject key in this.m_FakeItems)
		{
			this.m_FakeItemsData.Add(key, false);
		}
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
		if (pocket == BackpackPocket.None)
		{
			return;
		}
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
				if (this.m_FakeItemsData.ContainsKey(gameObject) && !this.m_FakeItemsData[gameObject])
				{
					gameObject.SetActive(false);
				}
				else
				{
					gameObject.SetActive(!objectToDisable.Contains(this.m_Backpack.transform.GetChild(i).gameObject));
				}
			}
			foreach (Item item in this.m_Items)
			{
				item.gameObject.SetActive(item.m_Info.m_BackpackPocket == pocket);
				if (pocketGrid != null && item.m_Info.m_InventoryCellsGroup != null && item.m_CurrentSlot == null)
				{
					item.m_Info.m_InventoryCellsGroup.Setup();
					item.m_Info.m_InventoryCellsGroup.SetupTansform(item, pocketGrid.m_Grid);
				}
			}
		}
	}

	public void SetBackpackTransform(BackpackPocket pocket)
	{
		if (pocket != BackpackPocket.None && this.m_Backpack)
		{
			this.m_Backpack.transform.localRotation = this.m_Backpack.m_PocketRotation[(int)pocket];
			this.m_Backpack.transform.localPosition = this.m_Backpack.m_PocketPosition[(int)pocket];
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
					return;
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
				item.m_CurrentSlot.RemoveItem(item, from_destroy);
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
		if (!item.m_IsBeingDestroyed)
		{
			item.transform.parent = null;
		}
		if (item.m_Info.m_InventoryCellsGroup != null)
		{
			item.m_Info.m_InventoryCellsGroup.Remove(item);
			item.m_Info.m_InventoryCellsGroup = null;
		}
		this.m_Items.Remove(item);
		if (!item.m_IsBeingDestroyed)
		{
			item.OnRemoveFromInventory();
		}
		this.OnInventoryChanged();
	}

	public InsertResult InsertItem(Item item, ItemSlot slot = null, InventoryCellsGroup group = null, bool can_stack = true, bool drop_if_cant = true, bool notify_if_cant = true, bool can_auto_select_group = true, bool set_pocket = true)
	{
		if (item.m_IsBeingDestroyed)
		{
			return InsertResult.CantInsert;
		}
		if (!item.m_Info.m_CanBeAddedToInventory)
		{
			Inventory3DManager.Get().DropItem(item);
			return InsertResult.CantInsert;
		}
		if (this.m_Items.Contains(item))
		{
			return InsertResult.AllreadyInInventory;
		}
		bool isStatic = item.gameObject.isStatic;
		item.gameObject.isStatic = false;
		item.transform.parent = null;
		item.transform.localScale = item.m_InventoryLocalScale;
		this.SetBackpackTransform(item.m_Info.m_BackpackPocket);
		if (set_pocket && Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().SetupPocket(item.m_Info.m_BackpackPocket);
		}
		InsertResult insertResult = InsertResult.None;
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
		if (insertResult == InsertResult.Ok)
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
				((HUDMessages)HUDManager.Get().GetHUD(typeof(HUDMessages))).AddMessage(GreenHellGame.Instance.GetLocalization().Get("HUD_NoSpaceInBackpack", true), new Color?(Color.red), HUDMessageIcon.None, "", null);
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
		item.UpdateScale(false);
		return insertResult;
	}

	private InsertResult InsertItemTop(Item item, ItemSlot slot)
	{
		if (!slot)
		{
			slot = this.FindFreeSlot(item);
		}
		if (slot && slot.CanInsertItem(item))
		{
			slot.InsertItem(item);
			return InsertResult.Ok;
		}
		return InsertResult.NoSpace;
	}

	private InsertResult InsertItemLeft(Item item, ItemSlot slot)
	{
		if (!slot)
		{
			slot = this.FindFreeSlot(item);
		}
		if (slot && slot.CanInsertItem(item))
		{
			slot.InsertItem(item);
			return InsertResult.Ok;
		}
		return InsertResult.NoSpace;
	}

	private InsertResult InsertItemRight(Item item, ItemSlot slot)
	{
		if (!slot)
		{
			slot = this.FindFreeSlot(item);
		}
		if (slot && slot.CanInsertItem(item))
		{
			slot.InsertItem(item);
			return InsertResult.Ok;
		}
		return InsertResult.NoSpace;
	}

	private InsertResult InsertItemMain(Item item, ItemSlot slot, InventoryCellsGroup group, bool can_stack, bool can_auto_select_group)
	{
		if (this.m_MainPocketGrid.InsertItem(item, slot, group, can_stack, can_auto_select_group, null))
		{
			return InsertResult.Ok;
		}
		return InsertResult.NoSpace;
	}

	private InsertResult InsertItemFront(Item item, ItemSlot slot, InventoryCellsGroup group, bool can_stack, bool can_auto_select_group)
	{
		if (this.m_FrontPocketGrid.InsertItem(item, slot, group, can_stack, can_auto_select_group, null))
		{
			return InsertResult.Ok;
		}
		return InsertResult.NoSpace;
	}

	public ItemSlot FindFreeSlot(Item item)
	{
		if (item.m_Info.m_BackpackPocket == BackpackPocket.Left)
		{
			if (this.m_EquippedItemSlot && !this.m_EquippedItem)
			{
				return this.m_EquippedItemSlot;
			}
			using (List<ItemSlot>.Enumerator enumerator = this.m_LeftSlots.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ItemSlot itemSlot = enumerator.Current;
					if ((!this.m_EquippedItem || !(this.m_EquippedItemSlot == itemSlot)) && itemSlot.CanInsertItem(item))
					{
						return itemSlot;
					}
				}
				goto IL_123;
			}
		}
		if (item.m_Info.m_BackpackPocket == BackpackPocket.Right)
		{
			using (List<ItemSlot>.Enumerator enumerator = this.m_RightSlots.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ItemSlot itemSlot2 = enumerator.Current;
					if (itemSlot2.CanInsertItem(item))
					{
						return itemSlot2;
					}
				}
				goto IL_123;
			}
		}
		if (item.m_Info.m_BackpackPocket == BackpackPocket.Top)
		{
			foreach (ItemSlot itemSlot3 in this.m_TopSlots)
			{
				if (itemSlot3.CanInsertItem(item))
				{
					return itemSlot3;
				}
			}
		}
		IL_123:
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
		if (pocket == BackpackPocket.Main)
		{
			this.m_MainPocketGrid.OnSetSelectedGroup(group);
			return;
		}
		if (pocket != BackpackPocket.Front)
		{
			return;
		}
		this.m_FrontPocketGrid.OnSetSelectedGroup(group);
	}

	public void OnCloseBackpack()
	{
		this.m_MainPocketGrid.OnCloseBackpack();
		this.m_FrontPocketGrid.OnCloseBackpack();
	}

	public bool IsOverload()
	{
		bool flag = ScenarioManager.Get().IsDreamOrPreDream();
		return this.m_CurrentWeight > this.m_OverloadWeight && !flag;
	}

	public bool IsCriticalOverload()
	{
		bool flag = ScenarioManager.Get().IsDreamOrPreDream();
		return this.m_CurrentWeight > this.m_CriticalWeight && !flag;
	}

	public bool IsMaxOverload()
	{
		bool flag = ScenarioManager.Get().IsDreamOrPreDream();
		return this.m_CurrentWeight > this.m_MaxWeight && !flag;
	}

	public void CalculateCurrentWeight()
	{
		this.m_CurrentWeight = 0f;
		Item currentItem = Player.Get().GetCurrentItem();
		if (currentItem)
		{
			this.m_CurrentWeight += currentItem.m_Info.GetMass();
		}
		foreach (Item item in this.m_Items)
		{
			this.m_CurrentWeight += item.m_Info.GetMass();
		}
		if (CraftingManager.Get().IsActive())
		{
			this.m_CurrentWeight += CraftingManager.Get().GetItemsMass();
		}
	}

	public void OnInventoryChanged()
	{
		this.CalculateCurrentWeight();
		foreach (Item item in this.m_Items)
		{
			item.UpdatePhx();
			item.UpdateScale(false);
			item.UpdateLayer();
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
		using (List<Item>.Enumerator enumerator = this.m_Items.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_Info.m_ID == id)
				{
					num++;
				}
			}
		}
		return num;
	}

	public int GetFoodItemsCount()
	{
		int num = 0;
		using (List<Item>.Enumerator enumerator = this.m_Items.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_Info.IsFood())
				{
					num++;
				}
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

	public bool ContainsBlade()
	{
		using (List<Item>.Enumerator enumerator = this.m_Items.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_Info.IsKnife())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool Contains(ItemID id)
	{
		using (List<Item>.Enumerator enumerator = this.m_Items.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_Info.m_ID == id)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool Contains(ItemType type)
	{
		using (List<Item>.Enumerator enumerator = this.m_Items.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_Info.m_Type == type)
				{
					return true;
				}
			}
		}
		return false;
	}

	public Item FindItem(ItemType type, bool in_slot = false)
	{
		foreach (Item item in this.m_Items)
		{
			if ((!in_slot || item.m_CurrentSlot) && item.m_Info.m_Type == type)
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
		for (int i = 0; i < components.Length; i++)
		{
			Material[] materials = ((Renderer)components[i]).materials;
			for (int j = 0; j < materials.Length; j++)
			{
				materials[j].color = color;
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

	public void DropAllItems()
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
			Inventory3DManager.Get().DropItem(item);
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

	public void ShowFakeItem(string elem_name, bool show)
	{
		string b = elem_name.ToLower();
		foreach (GameObject gameObject in this.m_FakeItemsData.Keys)
		{
			if (gameObject.name.ToLower() == b)
			{
				this.m_FakeItemsData[gameObject] = show;
				gameObject.SetActive(show);
				break;
			}
		}
	}

	public void DeleteItem(string item_name)
	{
		ItemID item_id = (ItemID)Enum.Parse(typeof(ItemID), item_name);
		Item item = this.FindItem(item_id);
		if (!item)
		{
			return;
		}
		if (CraftingManager.Get().gameObject.activeSelf && CraftingManager.Get().ContainsItem(item))
		{
			CraftingManager.Get().RemoveItem(item, false);
		}
		if (!item.m_CurrentSlot && item.m_InventorySlot && item.m_InventorySlot.m_Items.Count > 0)
		{
			item.m_InventorySlot.RemoveItem(item, false);
		}
		else if (item.m_CurrentSlot && item.m_CurrentSlot.m_InventoryStackSlot)
		{
			item.m_CurrentSlot.RemoveItem(item, false);
		}
		if (this.m_EquippedItem == item)
		{
			this.m_EquippedItem = null;
		}
		this.RemoveItem(item, false);
		UnityEngine.Object.Destroy(item.gameObject);
	}

	public void ResetGrids()
	{
		this.m_MainPocketGrid.Reset();
		this.m_FrontPocketGrid.Reset();
	}

	public void Save()
	{
		foreach (GameObject gameObject in this.m_FakeItemsData.Keys)
		{
			if (gameObject)
			{
				SaveGame.SaveVal("FakeItem" + gameObject.name, this.m_FakeItemsData[gameObject]);
			}
		}
	}

	public void Load()
	{
		Inventory3DManager.Get().m_ScenarioBlocked = false;
		for (int i = 0; i < this.m_FakeItemsData.Keys.Count; i++)
		{
			GameObject gameObject = this.m_FakeItemsData.Keys.ElementAt(i);
			if (gameObject)
			{
				bool flag = SaveGame.LoadBVal("FakeItem" + gameObject.name);
				this.m_FakeItemsData[gameObject] = flag;
				gameObject.SetActive(flag);
			}
		}
	}

	private static int CompareBySize(Item i1, Item i2)
	{
		float num = i1.m_BoxCollider.size.x * i1.m_InventoryLocalScale.x * i1.m_BoxCollider.size.z * i1.m_InventoryLocalScale.z;
		float num2 = i2.m_BoxCollider.size.x * i2.m_InventoryLocalScale.x * i2.m_BoxCollider.size.z * i2.m_InventoryLocalScale.z;
		if (num > num2)
		{
			return -1;
		}
		if (num < num2)
		{
			return 1;
		}
		return 0;
	}

	public void SortItemsBySize()
	{
		if (this.m_ActivePocket != BackpackPocket.Front && this.m_ActivePocket != BackpackPocket.Main)
		{
			return;
		}
		PocketGrid pocketGrid = (this.m_ActivePocket == BackpackPocket.Front) ? this.m_FrontPocketGrid : this.m_MainPocketGrid;
		this.m_TempSortItems.Clear();
		foreach (Item item in this.m_Items)
		{
			if (item.m_Info.m_BackpackPocket == this.m_ActivePocket)
			{
				this.m_TempSortItems.Add(item);
			}
		}
		foreach (Item item2 in this.m_TempSortItems)
		{
			this.RemoveItem(item2, false);
		}
		this.m_TempSortItems.Sort(new Comparison<Item>(InventoryBackpack.CompareBySize));
		foreach (Item item3 in this.m_TempSortItems)
		{
			ItemSlot itemSlot = null;
			for (int i = 0; i < this.m_Items.Count; i++)
			{
				Item item4 = this.m_Items[i];
				if (item4.m_InventorySlot && !item4.m_CurrentSlot && item4.m_InventorySlot.CanInsertItem(item3))
				{
					if (item4.m_Info.m_InventoryRotated != item3.m_Info.m_InventoryRotated)
					{
						Inventory3DManager.Get().RotateItem(item3, true);
					}
					itemSlot = item4.m_InventorySlot;
					break;
				}
			}
			if (itemSlot)
			{
				this.InsertItem(item3, itemSlot, null, true, true, false, true, true);
			}
			else
			{
				List<InventoryCellsGroup> list = new List<InventoryCellsGroup>();
				int num = 0;
				int num2 = 0;
				item3.m_Info.m_InventoryRotated = false;
				pocketGrid.CalcRequiredCells(item3, ref num, ref num2);
				int num3 = 0;
				int num4 = 0;
				item3.m_Info.m_InventoryRotated = true;
				pocketGrid.CalcRequiredCells(item3, ref num3, ref num4);
				item3.m_Info.m_InventoryRotated = (num3 > num);
				int num5 = item3.m_Info.m_InventoryRotated ? num3 : num;
				int num6 = item3.m_Info.m_InventoryRotated ? num4 : num2;
				InventoryCell[,] cells = pocketGrid.m_Cells;
				int upperBound = cells.GetUpperBound(0);
				int upperBound2 = cells.GetUpperBound(1);
				for (int j = cells.GetLowerBound(0); j <= upperBound; j++)
				{
					for (int k = cells.GetLowerBound(1); k <= upperBound2; k++)
					{
						InventoryCell inventoryCell = cells[j, k];
						if (inventoryCell.m_IndexX + num5 <= (int)pocketGrid.m_GridSize.x && inventoryCell.m_IndexY + num6 <= (int)pocketGrid.m_GridSize.y)
						{
							InventoryCellsGroup inventoryCellsGroup = new InventoryCellsGroup(this.m_ActivePocket);
							for (int l = inventoryCell.m_IndexX; l < inventoryCell.m_IndexX + num5; l++)
							{
								for (int m = inventoryCell.m_IndexY; m < inventoryCell.m_IndexY + num6; m++)
								{
									inventoryCellsGroup.m_Cells.Add(pocketGrid.m_Cells[l, m]);
								}
							}
							if (inventoryCellsGroup.m_Cells.Count != 0 && inventoryCellsGroup.IsFree())
							{
								inventoryCellsGroup.Setup();
								list.Add(inventoryCellsGroup);
							}
						}
					}
				}
				if (list.Count == 0)
				{
					this.InsertItem(item3, null, null, true, true, false, true, true);
				}
				else
				{
					InventoryCellsGroup inventoryCellsGroup2 = null;
					foreach (InventoryCellsGroup inventoryCellsGroup3 in list)
					{
						if (inventoryCellsGroup2 == null || inventoryCellsGroup3.m_Cells[0].m_IndexY < inventoryCellsGroup2.m_Cells[0].m_IndexY)
						{
							inventoryCellsGroup2 = inventoryCellsGroup3;
						}
					}
					foreach (InventoryCellsGroup inventoryCellsGroup4 in list)
					{
						if (inventoryCellsGroup4.m_Cells[0].m_IndexX > inventoryCellsGroup2.m_Cells[0].m_IndexX && inventoryCellsGroup4.m_Cells[0].m_IndexY == inventoryCellsGroup2.m_Cells[0].m_IndexY)
						{
							inventoryCellsGroup2 = inventoryCellsGroup4;
						}
					}
					InsertResult insertResult = this.InsertItem(item3, null, inventoryCellsGroup2, true, true, false, true, true);
					if (insertResult != InsertResult.Ok)
					{
						insertResult = this.InsertItem(item3, null, null, true, true, false, true, true);
					}
					if (insertResult != InsertResult.Ok)
					{
						DebugUtils.Assert(DebugUtils.AssertType.Info);
					}
				}
			}
		}
	}

	public Backpack m_Backpack;

	public List<GameObject> m_FakeItems = new List<GameObject>();

	private Dictionary<GameObject, bool> m_FakeItemsData = new Dictionary<GameObject, bool>();

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

	[HideInInspector]
	public BackpackPocket m_ActivePocket = BackpackPocket.None;

	public Color m_FreeColor = Color.white;

	public Color m_OccupiedColor = Color.white;

	public ItemSlot m_EquippedItemSlot;

	public Item m_EquippedItem;

	private static InventoryBackpack s_Instance;

	private List<Item> m_TempSortItems = new List<Item>();
}
