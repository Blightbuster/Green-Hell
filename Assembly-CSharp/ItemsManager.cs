using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

[ExecuteInEditMode]
public class ItemsManager : MonoBehaviour, ISaveLoad
{
	public static ItemsManager Get()
	{
		return ItemsManager.s_Instance;
	}

	private void Awake()
	{
		ItemsManager.s_Instance = this;
		if (!this.m_Initialized)
		{
			this.Initialize();
		}
		this.m_StringToItemIDMap.Clear();
		this.m_ItemIDToStringMap.Clear();
		for (int i = -1; i < Enum.GetValues(typeof(ItemID)).Length; i++)
		{
			Dictionary<string, int> stringToItemIDMap = this.m_StringToItemIDMap;
			ItemID itemID = (ItemID)i;
			stringToItemIDMap.Add(itemID.ToString(), i);
			Dictionary<int, string> itemIDToStringMap = this.m_ItemIDToStringMap;
			int key = i;
			itemID = (ItemID)i;
			itemIDToStringMap.Add(key, itemID.ToString());
		}
		ItemIDHelpers.Initialize();
	}

	public int StringToItemID(string item_name)
	{
		int result = -1;
		this.m_StringToItemIDMap.TryGetValue(item_name, out result);
		return result;
	}

	public string ItemIDToString(int id)
	{
		string empty = string.Empty;
		this.m_ItemIDToStringMap.TryGetValue(id, out empty);
		return empty;
	}

	private void Initialize()
	{
		this.LoadData();
		this.CreateQuadTree();
		this.InitCraftingData();
		foreach (ItemInfo itemInfo in this.m_ItemInfos.Values)
		{
			if (itemInfo.m_ActiveInNotepad)
			{
				this.UnlockItemInNotepad(itemInfo.m_ID);
			}
			else
			{
				this.LockItemInNotepad(itemInfo.m_ID);
			}
		}
		this.CreateIconsDictionary();
		this.InitDestroyableFallingSound();
		this.m_Initialized = true;
	}

	public void LoadData()
	{
		if (this.m_ItemInfos == null)
		{
			ItemsManager.LoadInfos(out this.m_ItemInfos);
		}
	}

	private void CreateIconsDictionary()
	{
		Sprite[] array = Resources.LoadAll<Sprite>("HUD");
		for (int i = 0; i < array.Length; i++)
		{
			this.m_ItemIconsSprites[array[i].name] = array[i];
		}
		this.m_ItemAdditionalIconSprites[ItemAdditionalIcon.Boiled] = this.m_ItemIconsSprites["Boiled"];
		this.m_ItemAdditionalIconSprites[ItemAdditionalIcon.Cooked] = this.m_ItemIconsSprites["Cooked"];
		this.m_ItemAdditionalIconSprites[ItemAdditionalIcon.Smoked] = this.m_ItemIconsSprites["Smoked"];
		this.m_ItemAdditionalIconSprites[ItemAdditionalIcon.Burned] = this.m_ItemIconsSprites["Burned"];
		this.m_ItemAdditionalIconSprites[ItemAdditionalIcon.Spoiled] = this.m_ItemIconsSprites["Spoiled"];
		this.m_ItemAdditionalIconSprites[ItemAdditionalIcon.Dried] = this.m_ItemIconsSprites["Dried"];
	}

	public static void LoadInfos(out Dictionary<int, ItemInfo> infos)
	{
		infos = new Dictionary<int, ItemInfo>();
		TextAsset textAsset = Resources.Load("Scripts/Items/Items") as TextAsset;
		DebugUtils.Assert(textAsset, true);
		TextAssetParser textAssetParser = new TextAssetParser(textAsset);
		for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
		{
			Key key = textAssetParser.GetKey(i);
			string svalue = key.GetVariable(1).SValue;
			Type type = Type.GetType(svalue + "Info");
			if (type == null)
			{
				DebugUtils.Assert(false, "Can't find type - " + svalue + "Info", true, DebugUtils.AssertType.Info);
			}
			else
			{
				ItemInfo itemInfo = Activator.CreateInstance(type) as ItemInfo;
				itemInfo.Load(key);
				infos.Add((int)itemInfo.m_ID, itemInfo);
			}
		}
		Resources.UnloadAsset(textAsset);
	}

	public void CreateQuadTree()
	{
		if (this.m_QuadTree != null)
		{
			return;
		}
		Bounds bounds = default(Bounds);
		Terrain[] array = UnityEngine.Object.FindObjectsOfType<Terrain>();
		for (int i = 0; i < array.Length; i++)
		{
			Terrain terrain = array[i];
			if (i == 0)
			{
				bounds.min = (bounds.max = terrain.GetPosition());
			}
			else
			{
				bounds.Encapsulate(terrain.GetPosition());
			}
			bounds.Encapsulate(terrain.terrainData.size);
		}
		if (array.Length != 0)
		{
			this.m_QuadTree = new QuadTree(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z, 100, 100);
			this.m_QuadTreeInitialized = true;
			for (int j = 0; j < this.m_ItemsToRegister.Count; j++)
			{
				if (this.m_ItemsToRegister[j].item != null)
				{
					this.RegisterItem(this.m_ItemsToRegister[j].item, this.m_ItemsToRegister[j].update_activity);
				}
			}
			this.m_ItemsToRegister.Clear();
		}
	}

	private void InitCraftingData()
	{
		foreach (ItemInfo itemInfo in this.m_ItemInfos.Values)
		{
			if (!this.m_CreationsData.ContainsKey((int)itemInfo.m_ID))
			{
				this.m_CreationsData.Add((int)itemInfo.m_ID, 0);
			}
		}
	}

	public void RegisterItem(Item item, bool update_activity = false)
	{
		if (RelevanceSystem.ENABLED)
		{
			return;
		}
		if (!this.m_QuadTreeInitialized)
		{
			ItemsManager.ItemsToRegister item2;
			item2.item = item;
			item2.update_activity = update_activity;
			this.m_ItemsToRegister.Add(item2);
			return;
		}
		this.m_QuadTree.InsertObject(item.gameObject, false);
		if (item.m_FallenObject)
		{
			this.m_FallenObjects.Add(item);
		}
		if (!update_activity)
		{
			if (item.gameObject.activeSelf && !this.m_ActiveObjects.Contains(item.gameObject))
			{
				this.m_ActiveObjects.Add(item.gameObject);
			}
			return;
		}
		Vector3 cameraPosition = this.GetCameraPosition();
		if (item.transform.position.Distance(cameraPosition) < this.m_DeactivateDist)
		{
			this.ActivateItem(item);
			return;
		}
		this.DeactivateItem(item);
	}

	public void UnregisterItem(Item item)
	{
		if (RelevanceSystem.ENABLED)
		{
			return;
		}
		this.m_ActiveObjects.Remove(item.gameObject);
		if (this.m_QuadTree == null)
		{
			int i = 0;
			while (i < this.m_ItemsToRegister.Count)
			{
				if (this.m_ItemsToRegister[i].item == item)
				{
					this.m_ItemsToRegister.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
			return;
		}
		this.m_QuadTree.RemoveObject(item.gameObject);
	}

	public void ActivateItem(Item item)
	{
		item.gameObject.SetActive(true);
		this.m_ActiveObjects.Add(item.gameObject);
	}

	public void DeactivateItem(Item item)
	{
		item.gameObject.SetActive(false);
		this.m_ActiveObjects.Remove(item.gameObject);
	}

	public Dictionary<int, ItemInfo> GetAllInfos()
	{
		return this.m_ItemInfos;
	}

	public List<ItemInfo> GetAllInfosOfType(ItemType type)
	{
		List<ItemInfo> list = new List<ItemInfo>();
		foreach (ItemInfo itemInfo in this.m_ItemInfos.Values)
		{
			if (itemInfo.m_Type == type)
			{
				list.Add(itemInfo);
			}
		}
		return list;
	}

	public ItemInfo GetInfo(string name)
	{
		ItemID id = (ItemID)Enum.Parse(typeof(ItemID), name);
		return this.GetInfo(id);
	}

	public ItemInfo GetInfo(ItemID id)
	{
		if (!this.m_Initialized)
		{
			this.Initialize();
		}
		ItemInfo result;
		this.m_ItemInfos.TryGetValue((int)id, out result);
		return result;
	}

	public Item CreateItem(string item_name, bool im_register)
	{
		ItemID item_id = (ItemID)Enum.Parse(typeof(ItemID), item_name);
		return this.CreateItem(item_id, im_register, Vector3.zero, Quaternion.identity);
	}

	public Item CreateItem(string item_name, bool im_register, Transform transform)
	{
		ItemID item_id = (ItemID)Enum.Parse(typeof(ItemID), item_name);
		return this.CreateItem(item_id, im_register, transform.position, transform.rotation);
	}

	public Item CreateItem(ItemID item_id, bool im_register, Transform transform)
	{
		return this.CreateItem(item_id, im_register, transform.position, transform.rotation);
	}

	public Item CreateItem(ItemID item_id, bool im_register, Vector3 position, Quaternion rotation)
	{
		GameObject prefab = GreenHellGame.Instance.GetPrefab(item_id.ToString());
		if (!prefab)
		{
			DebugUtils.Assert("[ItemsManager:CreateItem] Can't find prefab " + item_id.ToString(), true, DebugUtils.AssertType.Info);
			return null;
		}
		return this.CreateItem(prefab, im_register, position, rotation);
	}

	public Item CreateItem(GameObject prefab, bool im_register, Vector3 position, Quaternion rotation)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
		gameObject.name = prefab.name;
		Item component = gameObject.GetComponent<Item>();
		if (!component)
		{
			DebugUtils.Assert("[ItemsManager:CreateItem] Missing Item component - " + prefab.name, true, DebugUtils.AssertType.Info);
			UnityEngine.Object.Destroy(gameObject);
			return null;
		}
		this.CreateItemInfo(component);
		component.Initialize(im_register);
		return component;
	}

	public ItemInfo CreateItemInfo(Item item)
	{
		if (item.m_Info != null)
		{
			return item.m_Info;
		}
		if (item.m_InfoName == string.Empty)
		{
			DebugUtils.Assert("[ItemsManager:CreateItemInfo] ERROR - Missing InfoName of item " + item.name + ". Deafult Banana item created!", true, DebugUtils.AssertType.Info);
			item.m_InfoName = "Banana";
		}
		ItemID value = EnumUtils<ItemID>.GetValue(item.m_InfoName);
		ItemInfo info = this.GetInfo(value);
		if (info == null)
		{
			DebugUtils.Assert("[ItemsManager::CreateItemInfo] Can't create iteminfo - " + value.ToString(), true, DebugUtils.AssertType.Info);
			return null;
		}
		ItemInfo itemInfo = info.ShallowCopy();
		itemInfo.m_CreationTime = MainLevel.Instance.m_TODSky.Cycle.GameTime;
		item.m_Info = itemInfo;
		itemInfo.m_Item = item;
		return itemInfo;
	}

	public void Save()
	{
		SaveGame.SaveVal("UniqueID", ItemsManager.s_ItemUniqueID);
		using (HashSet<Item>.Enumerator enumerator = Item.s_AllItems.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_Info == null)
				{
					DebugUtils.Assert("Item is created and added to s_AllItems but has no item info set up", true, DebugUtils.AssertType.Info);
				}
			}
		}
		int num = 0;
		foreach (Item item in Item.s_AllItems)
		{
			if ((!item.m_ScenarioItem || item.WasTriggered()) && !item.m_CantSave && (item.m_CanSaveNotTriggered || item.WasTriggered()) && (!item.m_Info.IsArrow() || item.WasTriggered()) && item.GetInfoID() != ItemID.Liane_ToHoldHarvest && ((!item.m_FallenObject && !item.m_IsFallen) || item.WasTriggered()) && !(item.gameObject.GetComponent<AcreRespawnFruits>() != null) && (!item.m_CurrentSlot || !item.m_CurrentSlot.IsBIWoundSlot()) && (SaveGame.m_State != SaveGame.State.SaveCoop || item.m_InInventory) && (!item.m_InInventory || item.ReplIsOwner()))
			{
				SaveGame.SaveVal("ItemID" + num, (int)item.GetInfoID());
				SaveGame.SaveVal("ItemIMReg" + num, item.m_Registered);
				item.Save(num);
				num++;
			}
		}
		SaveGame.SaveVal("ItemsCount", num);
		SaveGame.SaveVal("ItemCreationData", this.m_CreationsData.Count);
		int num2 = 0;
		using (Dictionary<int, int>.KeyCollection.Enumerator enumerator2 = this.m_CreationsData.Keys.GetEnumerator())
		{
			while (enumerator2.MoveNext())
			{
				ItemID itemID = (ItemID)enumerator2.Current;
				SaveGame.SaveVal("ItemCreationDataID" + num2, (int)itemID);
				SaveGame.SaveVal("ItemCreationDataCount" + num2, this.m_CreationsData[(int)itemID]);
				num2++;
			}
		}
		SaveGame.SaveVal("CraftingLockedItems", this.m_CraftingLockedItems.Count);
		num2 = 0;
		foreach (ItemID val in this.m_CraftingLockedItems)
		{
			SaveGame.SaveVal("CraftingLockedItem" + num2, (int)val);
			num2++;
		}
		SaveGame.SaveVal("ItemWasConsumed", this.m_WasConsumedData.Count);
		for (int i = 0; i < this.m_WasConsumedData.Count; i++)
		{
			SaveGame.SaveVal("ItemWasConsumedID" + i, (int)this.m_WasConsumedData[i]);
		}
		SaveGame.SaveVal("ItemWasCollected", this.m_WasCollectedData.Count);
		for (int j = 0; j < this.m_WasCollectedData.Count; j++)
		{
			SaveGame.SaveVal("ItemWasCollectedID" + j, (int)this.m_WasCollectedData[j]);
		}
		SaveGame.SaveVal("ItemWasCrafted", this.m_WasCraftedData.Count);
		for (int k = 0; k < this.m_WasCraftedData.Count; k++)
		{
			SaveGame.SaveVal("ItemWasCraftedID" + k, (int)this.m_WasCraftedData[k]);
		}
		SaveGame.SaveVal("WasLiquidBoiled", this.m_WasLiquidBoiledData.Count);
		for (int l = 0; l < this.m_WasLiquidBoiledData.Count; l++)
		{
			SaveGame.SaveVal("WasLiquidBoiledID" + l, (int)this.m_WasLiquidBoiledData[l]);
		}
		SaveGame.SaveVal("UnlockedInNotepadItems", this.m_UnlockedInNotepadItems.Count);
		for (int m = 0; m < this.m_UnlockedInNotepadItems.Count; m++)
		{
			SaveGame.SaveVal("UnlockedInNotepadItemID" + m, (int)this.m_UnlockedInNotepadItems[m]);
		}
		SaveGame.SaveVal("WasConstructionDestroyed", this.m_WasConstructionDestroyed);
		SaveGame.SaveVal("ItemWasPlanted", this.m_WasPlantedData.Count);
		for (int n = 0; n < this.m_WasPlantedData.Count; n++)
		{
			SaveGame.SaveVal("ItemWasPlantedID" + n, (int)this.m_WasPlantedData[n]);
		}
		foreach (Generator generator in Generator.s_AllGenerators)
		{
			generator.SaveGenerator();
		}
		num2 = 0;
		foreach (ItemReplacer itemReplacer in ItemReplacer.s_AllReplacers)
		{
			if (itemReplacer && itemReplacer.m_ReplaceInfo != null && itemReplacer.m_FromPlant && itemReplacer.m_Acre == null)
			{
				SaveGame.SaveVal("ItemReplacerID" + num2, (int)itemReplacer.m_ReplaceInfo.m_ID);
				SaveGame.SaveVal("ItemReplacerPos" + num2, itemReplacer.transform.position);
				SaveGame.SaveVal("ItemReplacerRot" + num2, itemReplacer.transform.rotation);
				num2++;
			}
		}
		SaveGame.SaveVal("ItemReplacersCount", num2);
		for (int num3 = 0; num3 < LootBox.s_AllLootBoxes.Count; num3++)
		{
			LootBox.s_AllLootBoxes[num3].Save();
		}
	}

	private bool IsChildOfItem(Transform trans)
	{
		return trans.parent && (trans.parent.GetComponent<Item>() || this.IsChildOfItem(trans.parent));
	}

	public void Preload()
	{
		PlayerArmorModule.Get().ResetArmor();
		InventoryBackpack.Get().m_EquippedItemSlot.RemoveItem();
		BalanceSystem20.Get().InitializeDisabledSpawnersQuadTree();
		while (InventoryBackpack.Get().m_Items.Count > 0)
		{
			InventoryBackpack.Get().RemoveItem(InventoryBackpack.Get().m_Items[0], false);
		}
		for (int i = 0; i < Storage.s_AllStorages.Count; i++)
		{
			while (Storage.s_AllStorages[i].m_Items.Count > 0)
			{
				Storage.s_AllStorages[i].RemoveItem(Storage.s_AllStorages[i].m_Items[0], false);
			}
		}
		List<Item> list = null;
		foreach (Item item in Item.s_AllItems)
		{
			if (item != null && (item.GetInfoID() != ItemID.Liane_ToHoldHarvest || item.transform.parent == null) && !this.IsChildOfItem(item.transform))
			{
				if (!(SaveGame.m_SaveGameVersion < item.m_GameVersion) || GameVersion.NoVersion(item.m_GameVersion))
				{
					if (item.m_BoxCollider)
					{
						item.m_BoxCollider.enabled = false;
					}
					if (item.transform.parent)
					{
						item.transform.parent = null;
					}
					UnityEngine.Object.Destroy(item.gameObject);
					if (list == null)
					{
						list = new List<Item>();
					}
					list.Add(item);
				}
			}
			else
			{
				if (list == null)
				{
					list = new List<Item>();
				}
				list.Add(item);
			}
		}
		if (list != null)
		{
			foreach (Item item2 in list)
			{
				Item.s_AllItems.Remove(item2);
			}
		}
		foreach (ItemReplacer itemReplacer in ItemReplacer.s_AllReplacers)
		{
			if (itemReplacer && itemReplacer.m_FromPlant)
			{
				UnityEngine.Object.Destroy(itemReplacer.gameObject);
			}
		}
	}

	public void Load()
	{
		bool flag = false;
		Vector3 zero = Vector3.zero;
		zero.Set(468.3327f, 106.7012f, 1399.993f);
		Quaternion identity = Quaternion.identity;
		identity.Set(0f, 0.40191f, 0f, 0.91568f);
		bool flag2 = false;
		Inventory3DManager.Get().ResetGrids();
		this.m_ItemsToSetupAfterLoad.Clear();
		int num = SaveGame.LoadIVal("ItemsCount");
		for (int i = 0; i < num; i++)
		{
			ItemID item_id = (ItemID)SaveGame.LoadIVal("ItemID" + i);
			bool flag3 = SaveGame.LoadBVal("ItemIMReg" + i);
			Item item = this.CreateItem(item_id, false, Vector3.zero, Quaternion.identity);
			item.Load(i);
			if (MainLevel.Instance.m_SceneItems.ContainsKey(item.m_InfoName))
			{
				using (List<Vector3>.Enumerator enumerator = MainLevel.Instance.m_SceneItems[item.m_InfoName].GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if ((enumerator.Current - item.transform.position).sqrMagnitude < 0.1f)
						{
							item.m_SceneObject = true;
							break;
						}
					}
				}
			}
			if (!item.IsStorage() && !item.m_Info.IsConstruction() && (item.m_Info.m_ID == ItemID.Coconut_Green_Destroyable || (!item.m_InStorage && !item.m_InInventory && !item.m_OnCraftingTable && MainLevel.GetTerrainY(item.transform.position) > item.transform.position.y + 1f) || (EnumTools.IsItemSpoiled(item_id) && !item.m_InStorage && !item.m_InInventory && !item.m_OnCraftingTable && !item.m_SceneObject)))
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			else
			{
				if (item.gameObject.name == "Lina_do_Tutoriala_Fall")
				{
					if (flag2)
					{
						UnityEngine.Object.Destroy(item.gameObject);
						goto IL_29E;
					}
					flag2 = true;
				}
				if (item.m_Info.IsReadableItem())
				{
					flag3 = false;
				}
				if (item.IsLadder() && !item.IsFreeHandsLadder())
				{
					flag3 = false;
				}
				if (flag3)
				{
					item.ItemsManagerRegister(false);
				}
				this.m_ItemsToSetupAfterLoad.Add(item, i);
				ScenarioManager.Get().OnItemCreated(item.gameObject);
				if (item.m_Info.m_ID == ItemID.Dryer && !flag && zero.Distance(item.transform.position) <= 0.1f)
				{
					flag = true;
				}
			}
			IL_29E:;
		}
		InventoryBackpack.Get().OnInventoryChanged();
		this.m_CreationsData.Clear();
		int num2 = SaveGame.LoadIVal("ItemCreationData");
		for (int j = 0; j < num2; j++)
		{
			ItemID key = (ItemID)SaveGame.LoadIVal("ItemCreationDataID" + j);
			int value = SaveGame.LoadIVal("ItemCreationDataCount" + j);
			this.m_CreationsData.Add((int)key, value);
		}
		this.InitCraftingData();
		this.m_CraftingLockedItems.Clear();
		int num3 = SaveGame.LoadIVal("CraftingLockedItems");
		for (int k = 0; k < num3; k++)
		{
			ItemID item2 = (ItemID)SaveGame.LoadIVal("CraftingLockedItem" + k);
			this.m_CraftingLockedItems.Add(item2);
		}
		this.m_WasConsumedData.Clear();
		num2 = SaveGame.LoadIVal("ItemWasConsumed");
		for (int l = 0; l < num2; l++)
		{
			this.m_WasConsumedData.Add((ItemID)SaveGame.LoadIVal("ItemWasConsumedID" + l));
		}
		this.m_WasCollectedData.Clear();
		num2 = SaveGame.LoadIVal("ItemWasCollected");
		for (int m = 0; m < num2; m++)
		{
			this.m_WasCollectedData.Add((ItemID)SaveGame.LoadIVal("ItemWasCollectedID" + m));
		}
		this.m_WasCraftedData.Clear();
		num2 = SaveGame.LoadIVal("ItemWasCrafted");
		for (int n = 0; n < num2; n++)
		{
			this.m_WasCraftedData.Add((ItemID)SaveGame.LoadIVal("ItemWasCraftedID" + n));
		}
		this.m_WasLiquidBoiledData.Clear();
		num2 = SaveGame.LoadIVal("WasLiquidBoiled");
		for (int num4 = 0; num4 < num2; num4++)
		{
			this.m_WasLiquidBoiledData.Add((LiquidType)SaveGame.LoadIVal("WasLiquidBoiledID" + num4));
		}
		ItemsManager.s_ItemUniqueID = SaveGame.LoadIVal("UniqueID");
		this.m_WasPlantedData.Clear();
		num2 = SaveGame.LoadIVal("ItemWasPlanted");
		for (int num5 = 0; num5 < num2; num5++)
		{
			this.m_WasPlantedData.Add((ItemID)SaveGame.LoadIVal("ItemWasPlantedID" + num5));
		}
		this.m_UnlockedInNotepadItems.Clear();
		int num6 = SaveGame.LoadIVal("UnlockedInNotepadItems");
		for (int num7 = 0; num7 < num6; num7++)
		{
			ItemID id = (ItemID)SaveGame.LoadIVal("UnlockedInNotepadItemID" + num7);
			this.UnlockItemInNotepad(id);
		}
		if (!flag && !this.m_UnlockedInNotepadItems.Contains(ItemID.Dryer))
		{
			Item item3 = this.CreateItem(ItemID.Dryer, false, zero, identity);
			item3.ItemsManagerRegister(false);
			this.m_ItemsToSetupAfterLoad.Add(item3, this.m_ItemsToSetupAfterLoad.Count);
			ScenarioManager.Get().OnItemCreated(item3.gameObject);
		}
		if (SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate4)
		{
			this.m_WasConstructionDestroyed = SaveGame.LoadBVal("WasConstructionDestroyed");
		}
		foreach (Generator generator in Generator.s_AllGenerators)
		{
			generator.LoadGenerator();
		}
		if (SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionReleaseCandidate)
		{
			int num8 = SaveGame.LoadIVal("ItemReplacersCount");
			for (int num9 = 0; num9 < num8; num9++)
			{
				ItemID item_id2 = (ItemID)SaveGame.LoadIVal("ItemReplacerID" + num9);
				Vector3 position = SaveGame.LoadV3Val("ItemReplacerPos" + num9);
				Quaternion rotation = SaveGame.LoadQVal("ItemReplacerRot" + num9);
				this.CreateItem(item_id2, true, position, rotation);
			}
		}
		for (int num10 = 0; num10 < LootBox.s_AllLootBoxes.Count; num10++)
		{
			LootBox.s_AllLootBoxes[num10].Load();
		}
	}

	public void OnObjectMoved(GameObject go)
	{
		this.m_QuadTree.OnObjectMoved(go);
	}

	private void Update()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (!this.m_QuadTreeInitialized)
		{
			this.CreateQuadTree();
			return;
		}
		if (this.m_ItemsToSetupAfterLoad.Count > 0 && !ScenarioManager.Get().IsSceneLoading())
		{
			this.m_SetupAfterLoad = true;
			this.m_Duplicated.Clear();
			int num = 0;
			foreach (Item item in this.m_ItemsToSetupAfterLoad.Keys)
			{
				if (item != null && !item.m_IsBeingDestroyed)
				{
					item.SetupAfterLoad(this.m_ItemsToSetupAfterLoad[item]);
					if (item.m_Info != null && !item.m_InStorage && !item.m_InInventory && !item.m_CurrentSlot)
					{
						if (!this.m_Duplicated.ContainsKey(item.transform.position))
						{
							this.m_Duplicated.Add(item.transform.position, new Dictionary<int, List<Item>>());
						}
						if (!this.m_Duplicated[item.transform.position].ContainsKey((int)item.m_Info.m_ID))
						{
							this.m_Duplicated[item.transform.position].Add((int)item.m_Info.m_ID, new List<Item>());
						}
						this.m_Duplicated[item.transform.position][(int)item.m_Info.m_ID].Add(item);
					}
				}
				Debug.Log(num);
				num++;
			}
			this.m_ItemsToSetupAfterLoad.Clear();
			ConstructionGhostManager.Get().SetupAfterLoad();
			List<Item> list = new List<Item>();
			foreach (Dictionary<int, List<Item>> dictionary in this.m_Duplicated.Values)
			{
				foreach (List<Item> list2 in dictionary.Values)
				{
					if (list2.Count > 1)
					{
						if (list2[0].IsStorage())
						{
							Storage storage = (Storage)list2[0];
							for (int i = 1; i < list2.Count; i++)
							{
								Storage storage2 = (Storage)list2[i];
								for (int j = 0; j < storage2.m_Items.Count; j++)
								{
									Item item2 = storage2.m_Items[j];
									storage2.RemoveItem(item2, false);
									storage.InsertItem(item2, null, null, false, true);
								}
							}
							for (int k = 1; k < list2.Count; k++)
							{
								list.Add(list2[k]);
							}
						}
						else if (list2[0].m_Info.IsStand())
						{
							Stand stand = (Stand)list2[0];
							for (int l = 1; l < list2.Count; l++)
							{
								Stand stand2 = (Stand)list2[l];
								if (stand2.m_NumItems != 0)
								{
									int num2 = Mathf.Min(stand.m_NumItems + stand2.m_NumItems, stand.m_Vis.Count);
									int num3 = num2 - stand.m_NumItems;
									stand.m_NumItems = num2;
									stand.UpdateVis();
									stand2.m_NumItems -= num3;
									stand2.RemoveItems();
								}
							}
						}
						if (list2[0].m_Info.IsConstruction())
						{
							ItemSlot[] componentsInChildren = list2[0].gameObject.GetComponentsInChildren<ItemSlot>();
							for (int m = 1; m < list2.Count; m++)
							{
								foreach (ItemSlot itemSlot in list2[m].gameObject.GetComponentsInChildren<ItemSlot>())
								{
									if (itemSlot.m_Item)
									{
										Item item3 = itemSlot.m_Item;
										itemSlot.RemoveItem();
										foreach (ItemSlot itemSlot2 in componentsInChildren)
										{
											if (itemSlot2.transform.position == itemSlot.transform.position)
											{
												itemSlot2.InsertItem(item3);
												break;
											}
										}
									}
								}
							}
						}
						for (int num5 = 1; num5 < list2.Count; num5++)
						{
							list.Add(list2[num5]);
						}
					}
				}
			}
			foreach (Construction construction in Construction.s_AllConstructions)
			{
				int num6 = 0;
				while (num6 < construction.m_ConnectedConstructions.Count)
				{
					if (list.Contains(construction.m_ConnectedConstructions[num6]))
					{
						construction.m_ConnectedConstructions.RemoveAt(num6);
					}
					else
					{
						num6++;
					}
				}
			}
			foreach (Item item4 in list)
			{
				UnityEngine.Object.Destroy(item4.gameObject);
			}
			this.m_SetupAfterLoad = false;
		}
		Item.s_ItemUpdatesThisFrame = 0;
		if (this.m_QuestItemKey && !this.m_QuestItemKey.gameObject.activeSelf && !Player.Get().HaveItem(ItemID.QuestItem_Key))
		{
			this.m_QuestItemKey.gameObject.SetActive(true);
		}
		this.UpdateItemsActivity();
		this.UpdateItemSlots();
		this.UpdateFoodProcessors();
		this.UpdateConstantUpdates();
		this.UpdateItemsToDestroy();
		this.UpdateDebug();
	}

	private void UpdateItemsActivity()
	{
		if (RelevanceSystem.ENABLED)
		{
			return;
		}
		if (Time.time - this.m_LastActivityUpdateTime < this.m_ActivityUpdateInterval)
		{
			this.UpdateFallenDestroy();
			return;
		}
		Vector3 cameraPosition = this.GetCameraPosition();
		List<GameObject> objectsInRadius = this.m_QuadTree.GetObjectsInRadius(cameraPosition, this.m_DeactivateDist, false);
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		Item currentItem2 = Player.Get().GetCurrentItem(Hand.Left);
		for (int i = 0; i < objectsInRadius.Count; i++)
		{
			GameObject gameObject = objectsInRadius[i];
			gameObject.hideFlags = HideFlags.NotEditable;
			if ((!currentItem || !(currentItem.gameObject == gameObject)) && (!currentItem2 || !(currentItem2.gameObject == gameObject)))
			{
				if (ScenarioManager.Get().IsPreDream())
				{
					if (gameObject.activeSelf)
					{
						gameObject.SetActive(false);
					}
				}
				else if (!gameObject.activeSelf)
				{
					this.m_ActiveObjects.Add(gameObject);
					gameObject.SetActive(true);
				}
			}
		}
		foreach (GameObject gameObject2 in this.m_ActiveObjects)
		{
			if (gameObject2 == null)
			{
				this.m_ObjectsToRemoveTmp.Add(gameObject2);
			}
			else if (ScenarioManager.Get().IsPreDream())
			{
				gameObject2.SetActive(false);
				this.m_ObjectsToRemoveTmp.Add(gameObject2);
			}
			else if (gameObject2.hideFlags == HideFlags.None)
			{
				Item component = gameObject2.GetComponent<Item>();
				if (!(Inventory3DManager.Get().m_CarriedItem == component) && !Inventory3DManager.Get().m_StackItems.Contains(component) && (!(component != null) || (!component.m_OnCraftingTable && !component.m_InStorage && !component.m_InInventory)))
				{
					this.m_ObjectsToRemoveTmp.Add(gameObject2);
					if (component != null && component.m_Info != null && component.m_Info.m_DestroyByItemsManager && !component.m_Info.m_UsedForCrafting)
					{
						UnityEngine.Object.Destroy(gameObject2);
					}
					else
					{
						gameObject2.SetActive(false);
					}
				}
			}
		}
		if (this.m_ObjectsToRemoveTmp.Count > 0)
		{
			foreach (GameObject item in this.m_ObjectsToRemoveTmp)
			{
				this.m_ActiveObjects.Remove(item);
			}
			this.m_ObjectsToRemoveTmp.Clear();
		}
		for (int j = 0; j < objectsInRadius.Count; j++)
		{
			objectsInRadius[j].hideFlags = HideFlags.None;
		}
		if (ScenarioManager.Get().IsPreDream())
		{
			foreach (Construction construction in Construction.s_AllConstructions)
			{
				if (construction && construction.gameObject.activeSelf && construction.GetInfoID() != ItemID.ayuhasca_rack && construction.GetInfoID() != ItemID.campfire_ayuhasca)
				{
					construction.gameObject.SetActive(false);
				}
			}
		}
		this.UpdateFallenDestroy();
		this.m_LastActivityUpdateTime = Time.time;
	}

	private void UpdateFallenDestroy()
	{
		if (this.m_FallenObjects.Count == 0)
		{
			return;
		}
		if (this.m_FallenCurrentIdx >= this.m_FallenObjects.Count)
		{
			this.m_FallenCurrentIdx = 0;
		}
		Item item = this.m_FallenObjects[this.m_FallenCurrentIdx];
		if (item == null)
		{
			this.m_FallenObjects.RemoveAt(this.m_FallenCurrentIdx);
			return;
		}
		if (item.transform.position.Distance(Player.Get().transform.position) > FallenObjectsManager.s_MaxDistToPlayer * 1.1f && !item.WasTriggered())
		{
			this.m_FallenObjects.RemoveAt(this.m_FallenCurrentIdx);
			UnityEngine.Object.Destroy(item.gameObject);
			return;
		}
		this.m_FallenCurrentIdx++;
	}

	public void ClearFallenObjects()
	{
		for (int i = 0; i < this.m_FallenObjects.Count; i++)
		{
			UnityEngine.Object.Destroy(this.m_FallenObjects[i]);
		}
		this.m_FallenObjects.Clear();
	}

	private Vector3 GetCameraPosition()
	{
		Camera mainCamera = CameraManager.Get().m_MainCamera;
		Vector3 result = Vector3.zero;
		if (mainCamera)
		{
			result = mainCamera.transform.position;
		}
		else
		{
			result = Player.Get().gameObject.transform.position;
		}
		return result;
	}

	private void UpdateItemSlots()
	{
		if (Time.time - this.m_LastUpdateItemSlotsTime < this.m_UpdateItemSlotsInterval)
		{
			return;
		}
		for (int i = 0; i < ItemSlot.s_AllItemSlots.Count; i++)
		{
			if (ItemSlot.s_AllItemSlots[i].gameObject.activeSelf)
			{
				ItemSlot.s_AllItemSlots[i].UpdateActivity();
			}
		}
		this.m_LastUpdateItemSlotsTime = Time.time;
	}

	private void UpdateFoodProcessors()
	{
		if (FoodProcessor.s_AllFoodProcessors == null)
		{
			return;
		}
		foreach (FoodProcessor foodProcessor in FoodProcessor.s_AllFoodProcessors)
		{
			foodProcessor.UpdateProcessing();
		}
	}

	public void RegisterConstantUpdateItem(Item item)
	{
		this.s_ConstantUpdateItems.Add(item);
	}

	public void UnregisterConstantUpdateItem(Item item)
	{
		this.s_ConstantUpdateItems.Remove(item);
	}

	private void UpdateConstantUpdates()
	{
		foreach (Item item in this.s_ConstantUpdateItems)
		{
			item.ConstantUpdate();
		}
	}

	public void OnCreateItem(ItemID id)
	{
		if (this.m_CreationsData.ContainsKey((int)id))
		{
			Dictionary<int, int> creationsData = this.m_CreationsData;
			int key = (int)id;
			int value = creationsData[key] + 1;
			creationsData[key] = value;
			if (id == ItemID.Stone_Blade && this.m_CreationsData[(int)id] == 2)
			{
				HintsManager.Get().ShowHint("Crafting_Proggresion", 10f);
				return;
			}
		}
		else
		{
			DebugUtils.Assert("[ItemsManager:OnCraft] Missing item in crafting data - " + id.ToString(), true, DebugUtils.AssertType.Info);
		}
	}

	private bool ScenarioPlayerCreate(string id)
	{
		ItemID key = (ItemID)Enum.Parse(typeof(ItemID), id);
		return this.m_CreationsData[(int)key] > 0;
	}

	private void UpdateDebug()
	{
		if (!GreenHellGame.DEBUG)
		{
			return;
		}
		if (this.m_DebugSpawnID != ItemID.None && Input.GetKeyDown(KeyCode.I))
		{
			Vector3 forward = Player.Get().GetHeadTransform().forward;
			Vector3 vector = Player.Get().GetHeadTransform().position + 0.5f * forward;
			RaycastHit raycastHit;
			vector = (Physics.Raycast(vector, forward, out raycastHit, 3f) ? raycastHit.point : (vector + forward * 2f));
			this.CreateItem(this.m_DebugSpawnID, true, vector - forward * 0.2f, Player.Get().transform.rotation);
		}
		if (Input.GetKey(KeyCode.U) && Input.GetKey(KeyCode.LeftControl))
		{
			this.UnlockAllItemsInNotepad();
			PlayerDiseasesModule.Get().UnlockAllDiseasesInNotepad();
			PlayerDiseasesModule.Get().UnlockAllDiseasesTratmentInNotepad();
			PlayerDiseasesModule.Get().UnlockAllSymptomsInNotepad();
			PlayerDiseasesModule.Get().UnlockAllSymptomTreatmentsInNotepad();
			PlayerInjuryModule.Get().UnlockAllInjuryState();
			PlayerInjuryModule.Get().UnlockAllInjuryStateTreatment();
			PlayerInjuryModule.Get().UnlockAllKnownInjuries();
			this.UnloackAllConsumed();
			this.UnlockAllCrafted();
			this.UnlockAllBoiledData();
			this.UnlockAllCollected();
			this.UnlockAllItemInfos();
			MapTab mapTab = (MapTab)MenuNotepad.Get().m_Tabs[MenuNotepad.MenuNotepadTab.MapTab];
			if (mapTab == null)
			{
				return;
			}
			mapTab.UnlockAll();
		}
	}

	public void UnlockItemInNotepadNoMsgScenario(string id)
	{
		ItemID itemID = (ItemID)Enum.Parse(typeof(ItemID), id);
		if (!this.m_UnlockedInNotepadItems.Contains(itemID))
		{
			this.m_UnlockedInNotepadItems.Add(itemID);
			this.OnItemInNotepadUnlocked(itemID, false);
		}
	}

	public void UnlockItemInNotepadScenario(string id)
	{
		ItemID itemID = (ItemID)Enum.Parse(typeof(ItemID), id);
		if (!this.m_UnlockedInNotepadItems.Contains(itemID))
		{
			this.m_UnlockedInNotepadItems.Add(itemID);
			this.OnItemInNotepadUnlocked(itemID, true);
		}
	}

	public void LockItemInNotepadScenario(string id)
	{
		ItemID item = (ItemID)Enum.Parse(typeof(ItemID), id);
		if (this.m_UnlockedInNotepadItems.Contains(item))
		{
			this.m_UnlockedInNotepadItems.Remove(item);
		}
	}

	public void UnlockItemInNotepad(ItemID id)
	{
		if (!this.m_UnlockedInNotepadItems.Contains(id))
		{
			this.m_UnlockedInNotepadItems.Add(id);
		}
	}

	public void UnlockPlantInNotepadScenario(string id)
	{
		ItemID item_id = (ItemID)Enum.Parse(typeof(ItemID), id);
		this.OnTaken(item_id);
	}

	public void LockItemInNotepad(ItemID id)
	{
		if (this.m_UnlockedInNotepadItems.Contains(id))
		{
			this.m_UnlockedInNotepadItems.Remove(id);
		}
	}

	public void OnItemInNotepadUnlocked(ItemID id, bool show_msg = true)
	{
		if (HUDManager.Get() == null)
		{
			Debug.Log("ItemsManager OnItemInNotepadUnlocked no HUDManager");
			return;
		}
		HUDInfoLog hudinfoLog = (HUDInfoLog)HUDManager.Get().GetHUD(typeof(HUDInfoLog));
		string title = GreenHellGame.Instance.GetLocalization().Get("HUD_InfoLog_NewEntry", true);
		string text = GreenHellGame.Instance.GetLocalization().Get(id.ToString(), true);
		if (id == ItemID.Small_Fire || id == ItemID.Campfire || id == ItemID.Campfire_Rack || id == ItemID.Smoker || id == ItemID.Stone_Ring)
		{
			MenuNotepad.Get().SetActiveTab(MenuNotepad.MenuNotepadTab.FirecampTab, true);
		}
		else if (id == ItemID.Leaves_Bed || id == ItemID.Logs_Bed || id == ItemID.Small_Shelter || id == ItemID.Medium_Shelter)
		{
			MenuNotepad.Get().SetActiveTab(MenuNotepad.MenuNotepadTab.ConstructionsTab, true);
		}
		else if (ItemInfo.IsTrap(id))
		{
			MenuNotepad.Get().SetActiveTab(MenuNotepad.MenuNotepadTab.TrapsTab, true);
		}
		else if (id == ItemID.Water_Collector || id == ItemID.Water_Filter)
		{
			MenuNotepad.Get().SetActiveTab(MenuNotepad.MenuNotepadTab.WaterConstructionsTab, true);
		}
		else
		{
			MenuNotepad.Get().SetActiveTab(MenuNotepad.MenuNotepadTab.ItemsTab, true);
		}
		MenuNotepad.Get().SetCurrentPageToItem(id);
		if (show_msg)
		{
			hudinfoLog.AddInfo(title, text, HUDInfoLogTextureType.Notepad);
		}
		if (!ScenarioManager.Get().IsDreamOrPreDream())
		{
			PlayerAudioModule.Get().PlayNotepadEntrySound();
		}
	}

	public void UnlockAllItemsInNotepad()
	{
		Array values = Enum.GetValues(typeof(ItemID));
		for (int i = 0; i < values.Length; i++)
		{
			this.UnlockItemInNotepad((ItemID)values.GetValue(i));
		}
	}

	public void LockCraftingItem(string id)
	{
		ItemID item = (ItemID)Enum.Parse(typeof(ItemID), id);
		if (!this.m_CraftingLockedItems.Contains(item))
		{
			this.m_CraftingLockedItems.Add(item);
		}
	}

	public void UnlockCraftingItem(string id)
	{
		ItemID item = (ItemID)Enum.Parse(typeof(ItemID), id);
		if (this.m_CraftingLockedItems.Contains(item))
		{
			this.m_CraftingLockedItems.Remove(item);
		}
	}

	public void LockItemInfo(string id)
	{
		ItemID item = (ItemID)Enum.Parse(typeof(ItemID), id);
		if (this.m_UnlockedItemInfos.Contains(item))
		{
			this.m_UnlockedItemInfos.Remove(item);
		}
	}

	public void UnlockItemInfo(string id)
	{
		ItemID item = (ItemID)Enum.Parse(typeof(ItemID), id);
		if (!this.m_UnlockedItemInfos.Contains(item))
		{
			this.m_UnlockedItemInfos.Add(item);
		}
	}

	public void UnlockAllItemInfos()
	{
		Array values = Enum.GetValues(typeof(ItemID));
		for (int i = 0; i < values.Length; i++)
		{
			this.UnlockItemInfo(values.GetValue(i).ToString());
		}
	}

	public bool ItemExist(string item_id)
	{
		ItemID key = (ItemID)this.StringToItemID(item_id);
		int num = 0;
		Item.s_AllItemIDs.TryGetValue((int)key, out num);
		return num > 0;
	}

	public bool IsFirecampBurning(string firecamp_id)
	{
		ItemID itemID = (ItemID)Enum.Parse(typeof(ItemID), firecamp_id);
		for (int i = 0; i < Firecamp.s_Firecamps.Count; i++)
		{
			if (Firecamp.s_Firecamps[i].m_Info.m_ID == itemID && Firecamp.s_Firecamps[i].m_Burning)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyFirecampBurning()
	{
		for (int i = 0; i < Firecamp.s_Firecamps.Count; i++)
		{
			if (Firecamp.s_Firecamps[i].m_Burning)
			{
				return true;
			}
		}
		return false;
	}

	public bool WasReaded(GameObject item_obj)
	{
		if (!item_obj)
		{
			return false;
		}
		ReadableItem component = item_obj.GetComponent<ReadableItem>();
		if (!component)
		{
			DebugUtils.Assert("Object " + item_obj.name + " is not ReadableItem!", true, DebugUtils.AssertType.Info);
			return false;
		}
		return component.m_WasReaded;
	}

	public bool WasReadedAndOff(GameObject item_obj)
	{
		if (!item_obj)
		{
			return false;
		}
		ReadableItem component = item_obj.GetComponent<ReadableItem>();
		if (!component)
		{
			DebugUtils.Assert("Object " + item_obj.name + " is not ReadableItem!", true, DebugUtils.AssertType.Info);
			return false;
		}
		return component.m_WasReadedAndOff;
	}

	public bool IsSlotFilled(GameObject slot)
	{
		if (!slot)
		{
			return false;
		}
		ItemSlot component = slot.GetComponent<ItemSlot>();
		if (!component)
		{
			DebugUtils.Assert("Object " + slot.name + " is not ItemSlot!", true, DebugUtils.AssertType.Info);
			return false;
		}
		return component.m_Item != null;
	}

	public void OnEat(ConsumableInfo info)
	{
		if (!this.m_WasConsumedData.Contains(info.m_ID))
		{
			this.m_WasConsumedData.Add(info.m_ID);
		}
	}

	public void OnPlant(ItemID id)
	{
		if (!this.m_WasPlantedData.Contains(id))
		{
			this.m_WasPlantedData.Add(id);
		}
	}

	private void UnloackAllConsumed()
	{
		Array values = Enum.GetValues(typeof(ItemID));
		for (int i = 0; i < values.Length; i++)
		{
			ItemID item = (ItemID)i;
			if (!this.m_WasConsumedData.Contains(item))
			{
				this.m_WasConsumedData.Add(item);
			}
		}
	}

	private void UnlockAllCrafted()
	{
		Array values = Enum.GetValues(typeof(ItemID));
		for (int i = 0; i < values.Length; i++)
		{
			ItemID item = (ItemID)i;
			if (!this.m_WasCraftedData.Contains(item))
			{
				this.m_WasCraftedData.Add(item);
			}
		}
	}

	private void UnlockAllBoiledData()
	{
		Array values = Enum.GetValues(typeof(LiquidType));
		for (int i = 0; i < values.Length; i++)
		{
			LiquidType item = (LiquidType)i;
			if (!this.m_WasLiquidBoiledData.Contains(item))
			{
				this.m_WasLiquidBoiledData.Add(item);
			}
		}
	}

	private void UnlockAllCollected()
	{
		Array values = Enum.GetValues(typeof(ItemID));
		for (int i = 0; i < values.Length; i++)
		{
			ItemID item = (ItemID)i;
			if (!this.m_WasCollectedData.Contains(item))
			{
				this.m_WasCollectedData.Add(item);
			}
		}
	}

	public void OnTaken(ItemID item_id)
	{
		if (!this.m_WasCollectedData.Contains(item_id))
		{
			this.m_WasCollectedData.Add(item_id);
		}
	}

	public void OnCrafted(ItemID item_id)
	{
		if (!this.m_WasCraftedData.Contains(item_id))
		{
			this.m_WasCraftedData.Add(item_id);
		}
	}

	public void OnLiquidBoiled(LiquidType liquid_type)
	{
		if (!this.m_WasLiquidBoiledData.Contains(liquid_type))
		{
			this.m_WasLiquidBoiledData.Add(liquid_type);
		}
	}

	public bool WasConsumed(ItemID id)
	{
		return this.m_WasConsumedData.Contains(id);
	}

	public bool WasPlanted(ItemID id)
	{
		return this.m_WasPlantedData.Contains(id);
	}

	public bool WasCollected(ItemID id)
	{
		return this.m_WasCollectedData.Contains(id);
	}

	public bool WasCrafted(ItemID id)
	{
		return this.m_WasCraftedData.Contains(id);
	}

	public bool WasLiquidBoiled(LiquidType liquid_type)
	{
		return this.m_WasLiquidBoiledData.Contains(liquid_type);
	}

	public void ScenarioExtinguishAllFirecamps()
	{
		for (int i = 0; i < Firecamp.s_Firecamps.Count; i++)
		{
			Firecamp.s_Firecamps[i].Extinguish();
		}
	}

	public void ScenarioDeleteAllFirecamps()
	{
		int i = 0;
		while (i < Firecamp.s_Firecamps.Count)
		{
			GameObject gameObject = Firecamp.s_Firecamps[i].gameObject;
			if (gameObject.name == "Campfire" && gameObject.transform.parent && (gameObject.transform.parent.name == "refugees_items" || gameObject.transform.parent.name == "SideCamp_01"))
			{
				i++;
			}
			else
			{
				UnityEngine.Object.Destroy(gameObject);
				Firecamp.s_Firecamps.RemoveAt(i);
			}
		}
	}

	private void InitDestroyableFallingSound()
	{
		foreach (KeyValuePair<int, ItemInfo> keyValuePair in this.m_ItemInfos)
		{
			if (keyValuePair.Value.m_DestroyableFallSound.Length > 2)
			{
				Dictionary<int, ItemInfo>.Enumerator enumerator;
				keyValuePair = enumerator.Current;
				int key = Animator.StringToHash(keyValuePair.Value.m_DestroyableFallSound);
				if (!this.m_DestroyableFallingSounds.ContainsKey(key))
				{
					string str = "Sounds/Destroyable/";
					keyValuePair = enumerator.Current;
					AudioClip audioClip = Resources.Load(str + keyValuePair.Value.m_DestroyableFallSound) as AudioClip;
					if (audioClip)
					{
						this.m_DestroyableFallingSounds.Add(key, audioClip);
					}
				}
			}
		}
	}

	private void InitDestroyableDestroySound()
	{
		foreach (KeyValuePair<int, ItemInfo> keyValuePair in this.m_ItemInfos)
		{
			if (keyValuePair.Value.m_DestroyableDestroySound.Length > 2)
			{
				Dictionary<int, ItemInfo>.Enumerator enumerator;
				keyValuePair = enumerator.Current;
				int key = Animator.StringToHash(keyValuePair.Value.m_DestroyableDestroySound);
				if (!this.m_DestroyableDestroySounds.ContainsKey(key))
				{
					string str = "Sounds/Destroyable/";
					keyValuePair = enumerator.Current;
					AudioClip audioClip = Resources.Load(str + keyValuePair.Value.m_DestroyableDestroySound) as AudioClip;
					if (audioClip)
					{
						this.m_DestroyableDestroySounds.Add(key, audioClip);
					}
				}
			}
		}
	}

	public AudioClip GetDestroyableFallSound(int hash)
	{
		AudioClip result = null;
		this.m_DestroyableFallingSounds.TryGetValue(hash, out result);
		return result;
	}

	public AudioClip GetDestroyableDestroySound(int hash)
	{
		AudioClip result = null;
		this.m_DestroyableDestroySounds.TryGetValue(hash, out result);
		return result;
	}

	public bool IsHeavyObject(ItemID item_id)
	{
		ItemInfo info = this.GetInfo(item_id);
		if (info == null)
		{
			DebugUtils.Assert(item_id.ToString(), true, DebugUtils.AssertType.Info);
			return false;
		}
		return info.IsHeavyObject();
	}

	public void AddItemToDestroy(Item item)
	{
		if (item != null)
		{
			this.m_ToDestroy.Add(item);
		}
	}

	private void UpdateItemsToDestroy()
	{
		int i = 0;
		while (i < this.m_ToDestroy.Count)
		{
			if (this.m_ToDestroy[i] != null)
			{
				UnityEngine.Object.Destroy(this.m_ToDestroy[i].gameObject);
				this.m_ToDestroy.RemoveAt(i);
			}
			else
			{
				this.m_ToDestroy.RemoveAt(i);
			}
		}
	}

	public bool IsRackBurning(string rack_name)
	{
		int num = Animator.StringToHash(rack_name);
		foreach (FirecampRack firecampRack in FirecampRack.s_FirecampRacks)
		{
			if (firecampRack.m_NameHash == num)
			{
				return firecampRack.IsBurning();
			}
		}
		return false;
	}

	public void BurnoutRack(string rack_name)
	{
		foreach (FirecampRack firecampRack in FirecampRack.s_FirecampRacks)
		{
			if (firecampRack.name == rack_name)
			{
				firecampRack.BurnoutFirecamp();
			}
		}
	}

	public void SetRackEndlessFire(string rack_name)
	{
		foreach (FirecampRack firecampRack in FirecampRack.s_FirecampRacks)
		{
			if (firecampRack.name == rack_name)
			{
				firecampRack.SetEndlessFire();
			}
		}
	}

	public void ResetRackEndlessFire(string rack_name)
	{
		foreach (FirecampRack firecampRack in FirecampRack.s_FirecampRacks)
		{
			if (firecampRack.name == rack_name)
			{
				firecampRack.ResetEndlessFire();
			}
		}
	}

	private Dictionary<int, ItemInfo> m_ItemInfos;

	private QuadTree m_QuadTree;

	private HashSet<GameObject> m_ActiveObjects = new HashSet<GameObject>();

	public float m_DeactivateDist = 40f;

	private float m_LastActivityUpdateTime;

	private float m_ActivityUpdateInterval = 1.5f;

	private float m_LastUpdateItemSlotsTime;

	private float m_UpdateItemSlotsInterval = 0.1f;

	private static ItemsManager s_Instance;

	public ItemID m_DebugSpawnID = ItemID.None;

	public Dictionary<int, int> m_CreationsData = new Dictionary<int, int>();

	private bool m_Initialized;

	[HideInInspector]
	public List<ItemID> m_UnlockedInNotepadItems = new List<ItemID>();

	public Dictionary<string, Sprite> m_ItemIconsSprites = new Dictionary<string, Sprite>();

	public Dictionary<ItemAdditionalIcon, Sprite> m_ItemAdditionalIconSprites = new Dictionary<ItemAdditionalIcon, Sprite>();

	public static int s_ItemUniqueID;

	public Material m_TrailMaterial;

	private List<ItemID> m_WasConsumedData = new List<ItemID>();

	private List<ItemID> m_WasPlantedData = new List<ItemID>();

	private List<ItemID> m_WasCollectedData = new List<ItemID>();

	private List<ItemID> m_WasCraftedData = new List<ItemID>();

	private List<LiquidType> m_WasLiquidBoiledData = new List<LiquidType>();

	[HideInInspector]
	public List<ItemID> m_CraftingLockedItems = new List<ItemID>();

	[HideInInspector]
	public List<ItemID> m_UnlockedItemInfos = new List<ItemID>();

	[HideInInspector]
	public bool m_WasConstructionDestroyed;

	private Dictionary<string, int> m_StringToItemIDMap = new Dictionary<string, int>();

	private Dictionary<int, string> m_ItemIDToStringMap = new Dictionary<int, string>();

	[HideInInspector]
	public Item m_QuestItemKey;

	private List<Item> s_ConstantUpdateItems = new List<Item>();

	private bool m_QuadTreeInitialized;

	private List<Item> m_FallenObjects = new List<Item>(1000);

	private List<ItemsManager.ItemsToRegister> m_ItemsToRegister = new List<ItemsManager.ItemsToRegister>();

	public bool m_SetupAfterLoad;

	public Dictionary<Item, int> m_ItemsToSetupAfterLoad = new Dictionary<Item, int>();

	private Dictionary<Vector3, Dictionary<int, List<Item>>> m_Duplicated = new Dictionary<Vector3, Dictionary<int, List<Item>>>();

	private List<GameObject> m_ObjectsToRemoveTmp = new List<GameObject>(10);

	private int m_FallenCurrentIdx;

	private Dictionary<int, AudioClip> m_DestroyableFallingSounds = new Dictionary<int, AudioClip>();

	private Dictionary<int, AudioClip> m_DestroyableDestroySounds = new Dictionary<int, AudioClip>();

	public List<Item> m_ToDestroy = new List<Item>(100);

	private struct ItemsToRegister
	{
		public Item item;

		public bool update_activity;
	}
}
