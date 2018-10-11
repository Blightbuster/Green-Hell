using System;
using System.Collections.Generic;
using System.Reflection;
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
			ItemID itemID2 = (ItemID)i;
			itemIDToStringMap.Add(key, itemID2.ToString());
		}
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
			Type type = Type.GetType(key.GetVariable(1).SValue + "Info");
			if (type == null)
			{
				DebugUtils.Assert(DebugUtils.AssertType.Info);
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
				Vector3 position = terrain.GetPosition();
				bounds.max = position;
				bounds.min = position;
			}
			else
			{
				bounds.Encapsulate(terrain.GetPosition());
			}
			bounds.Encapsulate(terrain.terrainData.size);
		}
		this.m_QuadTree = new QuadTree(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z, 100, 100);
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
		this.m_QuadTree.InsertObject(item.gameObject, false);
		if (item.m_FallenObject)
		{
			this.m_FallenObjects.Add(item);
		}
		if (update_activity)
		{
			Vector3 cameraPosition = this.GetCameraPosition();
			float num = item.transform.position.Distance(cameraPosition);
			if (num < this.m_DeactivateDist)
			{
				item.gameObject.SetActive(true);
				this.m_ActiveObjects.Add(item.gameObject);
			}
			else
			{
				item.gameObject.SetActive(false);
				this.m_ActiveObjects.Remove(item.gameObject);
			}
		}
		else if (item.gameObject.activeSelf && !this.m_ActiveObjects.Contains(item.gameObject))
		{
			this.m_ActiveObjects.Add(item.gameObject);
		}
	}

	public void UnregisterItem(Item item)
	{
		this.m_ActiveObjects.Remove(item.gameObject);
		this.m_QuadTree.RemoveObject(item.gameObject);
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
		if (item.m_InfoName == string.Empty)
		{
			DebugUtils.Assert("[ItemsManager:CreateItemInfo] ERROR - Missing InfoName of item " + item.name + ". Deafult Banana item created!", true, DebugUtils.AssertType.Info);
			item.m_InfoName = "Banana";
		}
		ItemID id = (ItemID)Enum.Parse(typeof(ItemID), item.m_InfoName);
		ItemInfo info = this.GetInfo(id);
		if (info == null)
		{
			DebugUtils.Assert("[ItemsManager::CreateItemInfo] Can't create iteminfo - " + id.ToString(), true, DebugUtils.AssertType.Info);
			return null;
		}
		Type type = info.GetType();
		ItemInfo itemInfo = Activator.CreateInstance(type) as ItemInfo;
		PropertyInfo[] properties = type.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			propertyInfo.SetValue(itemInfo, propertyInfo.GetValue(info, null), null);
		}
		itemInfo.m_CreationTime = MainLevel.Instance.m_TODSky.Cycle.GameTime;
		item.m_Info = itemInfo;
		itemInfo.m_Item = item;
		return itemInfo;
	}

	public void Save()
	{
		SaveGame.SaveVal("UniqueID", ItemsManager.s_ItemUniqueID);
		for (int i = 0; i < Item.s_AllItems.Count; i++)
		{
			if (Item.s_AllItems[i].m_Info == null)
			{
				DebugUtils.Assert("Item is created and added to s_AllItems but has no item info set up", true, DebugUtils.AssertType.Info);
			}
		}
		int num = 0;
		for (int j = 0; j < Item.s_AllItems.Count; j++)
		{
			if (Item.s_AllItems[j].m_CanSave || Item.s_AllItems[j].WasTriggered())
			{
				if (Item.s_AllItems[j].GetInfoID() != ItemID.Liane_ToHoldHarvest)
				{
					if ((!Item.s_AllItems[j].m_FallenObject && !Item.s_AllItems[j].m_IsFallen) || Item.s_AllItems[j].WasTriggered())
					{
						if (!Item.s_AllItems[j].m_CurrentSlot || !Item.s_AllItems[j].m_CurrentSlot.IsBIWoundSlot())
						{
							SaveGame.SaveVal("ItemID" + num, (int)Item.s_AllItems[j].GetInfoID());
							SaveGame.SaveVal("ItemIMReg" + num, Item.s_AllItems[j].m_Registered);
							Item.s_AllItems[j].Save(num);
							num++;
						}
					}
				}
			}
		}
		SaveGame.SaveVal("ItemsCount", num);
		SaveGame.SaveVal("ItemCreationData", this.m_CreationsData.Count);
		int num2 = 0;
		using (Dictionary<int, int>.KeyCollection.Enumerator enumerator = this.m_CreationsData.Keys.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ItemID itemID = (ItemID)enumerator.Current;
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
		for (int k = 0; k < this.m_WasConsumedData.Count; k++)
		{
			SaveGame.SaveVal("ItemWasConsumedID" + k, (int)this.m_WasConsumedData[k]);
		}
		SaveGame.SaveVal("ItemWasCollected", this.m_WasCollectedData.Count);
		for (int l = 0; l < this.m_WasCollectedData.Count; l++)
		{
			SaveGame.SaveVal("ItemWasCollectedID" + l, (int)this.m_WasCollectedData[l]);
		}
		SaveGame.SaveVal("ItemWasCrafted", this.m_WasCraftedData.Count);
		for (int m = 0; m < this.m_WasCraftedData.Count; m++)
		{
			SaveGame.SaveVal("ItemWasCraftedID" + m, (int)this.m_WasCraftedData[m]);
		}
		SaveGame.SaveVal("WasLiquidBoiled", this.m_WasLiquidBoiledData.Count);
		for (int n = 0; n < this.m_WasLiquidBoiledData.Count; n++)
		{
			SaveGame.SaveVal("WasLiquidBoiledID" + n, (int)this.m_WasLiquidBoiledData[n]);
		}
		SaveGame.SaveVal("UnlockedInNotepadItems", this.m_UnlockedInNotepadItems.Count);
		for (int num3 = 0; num3 < this.m_UnlockedInNotepadItems.Count; num3++)
		{
			SaveGame.SaveVal("UnlockedInNotepadItemID" + num3, (int)this.m_UnlockedInNotepadItems[num3]);
		}
		SaveGame.SaveVal("WasConstructionDestroyed", this.m_WasConstructionDestroyed);
	}

	public void Load()
	{
		while (InventoryBackpack.Get().m_Items.Count > 0)
		{
			InventoryBackpack.Get().RemoveItem(InventoryBackpack.Get().m_Items[0], false);
		}
		int i = 0;
		while (i < Item.s_AllItems.Count)
		{
			if (Item.s_AllItems[i] == null)
			{
				Item.s_AllItems.RemoveAt(i);
			}
			else
			{
				if (Item.s_AllItems[i].GetInfoID() != ItemID.Liane_ToHoldHarvest)
				{
					UnityEngine.Object.Destroy(Item.s_AllItems[i].gameObject);
				}
				i++;
			}
		}
		Item.s_AllItems.Clear();
		this.m_ItemsToSetupAfterLoad.Clear();
		int num = SaveGame.LoadIVal("ItemsCount");
		for (int j = 0; j < num; j++)
		{
			ItemID item_id = (ItemID)SaveGame.LoadIVal("ItemID" + j);
			bool flag = SaveGame.LoadBVal("ItemIMReg" + j);
			Item item = this.CreateItem(item_id, false, Vector3.zero, Quaternion.identity);
			item.Load(j);
			if (flag)
			{
				item.ItemsManagerRegister(false);
			}
			this.m_ItemsToSetupAfterLoad.Add(item);
			ScenarioAction.OnItemCreated(item.gameObject);
			ScenarioCndTF.OnItemCreated(item.gameObject);
		}
		InventoryBackpack.Get().OnInventoryChanged();
		this.m_CreationsData.Clear();
		int num2 = SaveGame.LoadIVal("ItemCreationData");
		for (int k = 0; k < num2; k++)
		{
			ItemID key = (ItemID)SaveGame.LoadIVal("ItemCreationDataID" + k);
			int value = SaveGame.LoadIVal("ItemCreationDataCount" + k);
			this.m_CreationsData.Add((int)key, value);
		}
		this.InitCraftingData();
		this.m_CraftingLockedItems.Clear();
		int num3 = SaveGame.LoadIVal("CraftingLockedItems");
		for (int l = 0; l < num3; l++)
		{
			ItemID item2 = (ItemID)SaveGame.LoadIVal("CraftingLockedItem" + l);
			this.m_CraftingLockedItems.Add(item2);
		}
		this.m_WasConsumedData.Clear();
		num2 = SaveGame.LoadIVal("ItemWasConsumed");
		for (int m = 0; m < num2; m++)
		{
			this.m_WasConsumedData.Add((ItemID)SaveGame.LoadIVal("ItemWasConsumedID" + m));
		}
		this.m_WasCollectedData.Clear();
		num2 = SaveGame.LoadIVal("ItemWasCollected");
		for (int n = 0; n < num2; n++)
		{
			this.m_WasCollectedData.Add((ItemID)SaveGame.LoadIVal("ItemWasCollectedID" + n));
		}
		this.m_WasCraftedData.Clear();
		num2 = SaveGame.LoadIVal("ItemWasCrafted");
		for (int num4 = 0; num4 < num2; num4++)
		{
			this.m_WasCraftedData.Add((ItemID)SaveGame.LoadIVal("ItemWasCraftedID" + num4));
		}
		this.m_WasLiquidBoiledData.Clear();
		num2 = SaveGame.LoadIVal("WasLiquidBoiled");
		for (int num5 = 0; num5 < num2; num5++)
		{
			this.m_WasLiquidBoiledData.Add((LiquidType)SaveGame.LoadIVal("WasLiquidBoiledID" + num5));
		}
		ItemsManager.s_ItemUniqueID = SaveGame.LoadIVal("UniqueID");
		this.m_UnlockedInNotepadItems.Clear();
		int num6 = SaveGame.LoadIVal("UnlockedInNotepadItems");
		for (int num7 = 0; num7 < num6; num7++)
		{
			ItemID id = (ItemID)SaveGame.LoadIVal("UnlockedInNotepadItemID" + num7);
			this.UnlockItemInNotepad(id);
		}
		if (GreenHellGame.s_GameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate4)
		{
			this.m_WasConstructionDestroyed = SaveGame.LoadBVal("WasConstructionDestroyed");
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
		if (this.m_ItemsToSetupAfterLoad.Count > 0)
		{
			this.m_SetupAfterLoad = true;
			for (int i = 0; i < this.m_ItemsToSetupAfterLoad.Count; i++)
			{
				this.m_ItemsToSetupAfterLoad[i].SetupAfterLoad(i);
			}
			this.m_ItemsToSetupAfterLoad.Clear();
			this.m_SetupAfterLoad = false;
		}
		this.UpdateItemsActivity();
		this.UpdateItemSlots();
		this.UpdateFoodProcessors();
		this.UpdateFirecamps();
		this.UpdateDebug();
	}

	private void UpdateItemsActivity()
	{
		if (Inventory3DManager.Get().gameObject.activeSelf)
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
			if ((!currentItem || !(currentItem.gameObject == gameObject)) && (!currentItem2 || !(currentItem2.gameObject == gameObject)))
			{
				if (!this.m_ActiveObjects.Contains(gameObject))
				{
					this.m_ActiveObjects.Add(gameObject);
					gameObject.SetActive(true);
				}
			}
		}
		int j = 0;
		while (j < this.m_ActiveObjects.Count)
		{
			GameObject gameObject2 = this.m_ActiveObjects[j];
			if (gameObject2 == null)
			{
				this.m_ActiveObjects.RemoveAt(j);
			}
			else if (!objectsInRadius.Contains(gameObject2))
			{
				this.m_ActiveObjects.RemoveAt(j);
				Item component = gameObject2.GetComponent<Item>();
				if (component != null && component.m_Info != null && component.m_Info.m_DestroyByItemsManager)
				{
					UnityEngine.Object.Destroy(gameObject2);
				}
				else
				{
					gameObject2.SetActive(false);
				}
			}
			else
			{
				j++;
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

	private Vector3 GetCameraPosition()
	{
		Camera main = Camera.main;
		Vector3 result = Vector3.zero;
		if (main)
		{
			result = main.transform.position;
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

	private void UpdateFirecamps()
	{
		if (Firecamp.s_Firecamps == null)
		{
			return;
		}
		foreach (Firecamp firecamp in Firecamp.s_Firecamps)
		{
			firecamp.ConstantUpdate();
		}
	}

	public void OnCreateItem(ItemID id)
	{
		if (this.m_CreationsData.ContainsKey((int)id))
		{
			Dictionary<int, int> creationsData;
			int key;
			(creationsData = this.m_CreationsData)[key = (int)id] = creationsData[key] + 1;
			if (id == ItemID.Stone_Blade && this.m_CreationsData[(int)id] == 2)
			{
				HintsManager.Get().ShowHint("Crafting_Proggresion", 10f);
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
			this.CreateItem(this.m_DebugSpawnID, true, Player.Get().transform.position + Player.Get().transform.forward * 4f, Player.Get().transform.rotation);
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
		string title = GreenHellGame.Instance.GetLocalization().Get("HUD_InfoLog_NewEntry");
		string text = GreenHellGame.Instance.GetLocalization().Get(id.ToString());
		if (id == ItemID.Small_Fire || id == ItemID.Campfire || id == ItemID.Campfire_Rack || id == ItemID.Smoker || id == ItemID.Stone_Ring)
		{
			MenuNotepad.Get().SetActiveTab(MenuNotepad.MenuNotepadTab.FirecampTab, true);
		}
		else if (id == ItemID.Leaves_Bed || id == ItemID.Logs_Bed || id == ItemID.Small_Shelter || id == ItemID.Medium_Shelter)
		{
			MenuNotepad.Get().SetActiveTab(MenuNotepad.MenuNotepadTab.ConstructionsTab, true);
		}
		else if (id == ItemID.Cage_Trap || id == ItemID.Fish_Rod_Trap || id == ItemID.Killer_Trap || id == ItemID.Snare_Trap || id == ItemID.Stick_Fish_Trap || id == ItemID.Stone_Trap)
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
			hudinfoLog.AddInfo(title, text);
		}
		PlayerAudioModule.Get().PlayNotepadEntrySound();
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
		for (int i = 0; i < Firecamp.s_Firecamps.Count; i++)
		{
			UnityEngine.Object.Destroy(Firecamp.s_Firecamps[i].gameObject);
		}
		Firecamp.s_Firecamps.Clear();
	}

	private void InitDestroyableFallingSound()
	{
		foreach (KeyValuePair<int, ItemInfo> keyValuePair in this.m_ItemInfos)
		{
			if (keyValuePair.Value.m_DestroyableFallSound.Length > 2)
			{
				Dictionary<int, ItemInfo>.Enumerator enumerator;
				KeyValuePair<int, ItemInfo> keyValuePair2 = enumerator.Current;
				int key = Animator.StringToHash(keyValuePair2.Value.m_DestroyableFallSound);
				if (!this.m_DestroyableFallingSounds.ContainsKey(key))
				{
					string str = "Sounds/Destroyable/";
					KeyValuePair<int, ItemInfo> keyValuePair3 = enumerator.Current;
					AudioClip audioClip = Resources.Load(str + keyValuePair3.Value.m_DestroyableFallSound) as AudioClip;
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
				KeyValuePair<int, ItemInfo> keyValuePair2 = enumerator.Current;
				int key = Animator.StringToHash(keyValuePair2.Value.m_DestroyableDestroySound);
				if (!this.m_DestroyableDestroySounds.ContainsKey(key))
				{
					string str = "Sounds/Destroyable/";
					KeyValuePair<int, ItemInfo> keyValuePair3 = enumerator.Current;
					AudioClip audioClip = Resources.Load(str + keyValuePair3.Value.m_DestroyableDestroySound) as AudioClip;
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
			DebugUtils.Assert(DebugUtils.AssertType.Info);
		}
		return info.IsHeavyObject();
	}

	private Dictionary<int, ItemInfo> m_ItemInfos;

	private QuadTree m_QuadTree;

	private List<GameObject> m_ActiveObjects = new List<GameObject>();

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

	private List<Item> m_FallenObjects = new List<Item>(1000);

	public bool m_SetupAfterLoad;

	private List<Item> m_ItemsToSetupAfterLoad = new List<Item>();

	private int m_FallenCurrentIdx;

	private Dictionary<int, AudioClip> m_DestroyableFallingSounds = new Dictionary<int, AudioClip>();

	private Dictionary<int, AudioClip> m_DestroyableDestroySounds = new Dictionary<int, AudioClip>();
}
