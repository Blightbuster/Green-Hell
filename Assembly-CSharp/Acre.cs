using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class Acre : Construction, IItemSlotParent, IProcessor
{
	protected override void Awake()
	{
		base.Awake();
		if (this.s_VisMap == null)
		{
			this.InitializeVisMap();
		}
		if (this.s_ItemsMap == null)
		{
			this.InitializeItemsMap();
		}
		if (this.m_PlantRoot == null)
		{
			this.SetupRoot();
		}
		if (this.s_GrowTimeMap == null)
		{
			this.SetupGrowTimeMap();
		}
		this.m_ItemSlot = base.transform.FindDeepChild("ItemSlot").GetComponent<ItemSlot>();
		for (int i = 0; i < this.m_MaterialObjects.Count; i++)
		{
			Material[] materials = this.m_MaterialObjects[i].GetComponent<MeshRenderer>().materials;
			for (int j = 0; j < materials.Length; j++)
			{
				if (materials[j].name.Contains("Acre_soil"))
				{
					this.m_Materials.Add(materials[j]);
					this.m_Shaders.Add(this.m_Materials[i].shader);
					break;
				}
			}
		}
		AcresManager.Get().RegisterAcre(this);
		if (Acre.s_PlowAudioClip == null)
		{
			Acre.s_PlowAudioClip = (AudioClip)Resources.Load("Sounds/Player/climb_end_fail");
		}
	}

	protected override void Start()
	{
		base.Start();
		base.Invoke("Setup", 0.1f);
		if (this.m_AcreState == AcreState.None)
		{
			this.SetState(AcreState.Ready);
		}
	}

	private void Setup()
	{
		if (this.m_Setup)
		{
			return;
		}
		if (MainLevel.Instance.m_GrassManager == null)
		{
			base.Invoke("Setup", 0.1f);
			return;
		}
		TerrainData terrainData = Terrain.activeTerrain.terrainData;
		Bounds bounds = default(Bounds);
		bounds.center = base.transform.position;
		Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(base.gameObject);
		for (int i = 0; i < componentsDeepChild.Length; i++)
		{
			bounds.Encapsulate(componentsDeepChild[i].bounds);
		}
		Vector3 center = bounds.center;
		center.y = -10000f;
		bounds.Encapsulate(center);
		center.y = 10000f;
		bounds.Encapsulate(center);
		this.m_Bounds = bounds;
		MainLevel.Instance.m_GrassManager.ReinitCellAtBounds(bounds);
		this.m_Setup = true;
	}

	private void SetupGrowTimeMap()
	{
		this.s_GrowTimeMap = new Dictionary<int, float>();
		this.s_GrowTimeMap.Add(13, 2880f);
		this.s_GrowTimeMap.Add(823, 4320f);
		this.s_GrowTimeMap.Add(805, 4320f);
		this.s_GrowTimeMap.Add(802, 4320f);
		this.s_GrowTimeMap.Add(799, 4320f);
		this.s_GrowTimeMap.Add(61, 5760f);
		this.s_GrowTimeMap.Add(779, 5760f);
		this.s_GrowTimeMap.Add(804, 5760f);
		this.s_GrowTimeMap.Add(786, 5760f);
		this.s_GrowTimeMap.Add(800, 5760f);
		this.s_GrowTimeMap.Add(785, 7200f);
		this.s_GrowTimeMap.Add(781, 7200f);
		this.s_GrowTimeMap.Add(782, 7200f);
		this.s_GrowTimeMap.Add(807, 7200f);
		this.s_GrowTimeMap.Add(70, 7200f);
	}

	private void SetupRoot()
	{
		this.m_PlantRoot = base.transform.FindDeepChild("PlantRoot");
		DebugUtils.Assert(this.m_PlantRoot, true);
	}

	private void InitializeVisMap()
	{
		this.s_VisMap = new Dictionary<int, GameObject>();
		GameObject prefab = GreenHellGame.Instance.GetPrefab("small_plant_11_acres");
		this.s_VisMap.Add(779, prefab);
		prefab = GreenHellGame.Instance.GetPrefab("medium_plant_02_acres");
		this.s_VisMap.Add(799, prefab);
		prefab = GreenHellGame.Instance.GetPrefab("big_tree_08_acres");
		this.s_VisMap.Add(781, prefab);
		prefab = GreenHellGame.Instance.GetPrefab("medium_plant_08_acres");
		this.s_VisMap.Add(782, prefab);
		prefab = GreenHellGame.Instance.GetPrefab("medium_plant_09_acres");
		this.s_VisMap.Add(13, prefab);
		prefab = GreenHellGame.Instance.GetPrefab("medium_plant_10_acres");
		this.s_VisMap.Add(800, prefab);
		prefab = GreenHellGame.Instance.GetPrefab("medium_plant_12_acres");
		this.s_VisMap.Add(785, prefab);
		prefab = GreenHellGame.Instance.GetPrefab("small_plant_01_acres");
		this.s_VisMap.Add(786, prefab);
		prefab = GreenHellGame.Instance.GetPrefab("small_plant_08_acres");
		this.s_VisMap.Add(802, prefab);
		prefab = GreenHellGame.Instance.GetPrefab("small_plant_10_acres");
		this.s_VisMap.Add(804, prefab);
		prefab = GreenHellGame.Instance.GetPrefab("small_plant_13_acres");
		this.s_VisMap.Add(805, prefab);
		prefab = GreenHellGame.Instance.GetPrefab("small_plant_14_acres");
		this.s_VisMap.Add(823, prefab);
		prefab = GreenHellGame.Instance.GetPrefab("big_tree_07_acres");
		this.s_VisMap.Add(61, prefab);
		prefab = GreenHellGame.Instance.GetPrefab("medium_plant_04_acres");
		this.s_VisMap.Add(807, prefab);
		prefab = GreenHellGame.Instance.GetPrefab("big_tree_03_acres");
		this.s_VisMap.Add(70, prefab);
	}

	private void InitializeItemsMap()
	{
		this.s_ItemsMap = new Dictionary<int, int>();
		this.s_ItemsMap.Add(779, 817);
		this.s_ItemsMap.Add(799, 808);
		this.s_ItemsMap.Add(781, 809);
		this.s_ItemsMap.Add(782, 810);
		this.s_ItemsMap.Add(13, 811);
		this.s_ItemsMap.Add(800, 812);
		this.s_ItemsMap.Add(785, 813);
		this.s_ItemsMap.Add(786, 814);
		this.s_ItemsMap.Add(802, 815);
		this.s_ItemsMap.Add(804, 816);
		this.s_ItemsMap.Add(805, 818);
		this.s_ItemsMap.Add(823, 819);
		this.s_ItemsMap.Add(61, 820);
		this.s_ItemsMap.Add(807, 821);
		this.s_ItemsMap.Add(70, 822);
	}

	public AcreState GetState()
	{
		return this.m_AcreState;
	}

	public bool CanInsertItem(Item item)
	{
		return true;
	}

	public void OnInsertItem(ItemSlot slot)
	{
		if (slot == null || slot.m_Item == null)
		{
			return;
		}
		if (slot == this.m_ItemSlot)
		{
			this.OnInsertSeeds(slot.m_Item);
			return;
		}
		if (slot == this.m_WaterSlot)
		{
			if (EnumTools.IsItemSpoiled(slot.m_Item.m_Info.m_ID) || slot.m_Item.m_Info.m_ID == ItemID.animal_droppings_item)
			{
				this.m_FertilizerAmount += slot.m_Item.m_Info.m_FertilizeAmount;
				this.m_FertilizerAmount = Mathf.Clamp(this.m_FertilizerAmount, 0f, this.m_MaxFertilizerAmount);
				UnityEngine.Object.Destroy(slot.m_Item.gameObject);
				slot.RemoveItem();
				return;
			}
			Item item = slot.m_Item;
			LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)item.m_Info;
			float waterAmount = this.m_WaterAmount;
			this.m_WaterAmount += liquidContainerInfo.m_Amount;
			this.m_WaterAmount = Mathf.Min(this.m_WaterAmount, this.m_MaxWaterAmount);
			float num = this.m_WaterAmount - waterAmount;
			liquidContainerInfo.m_Amount -= num;
			slot.RemoveItem();
			InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, true);
			if (num > 0f)
			{
				PlayerAudioModule.Get().PlayWaterSpillSound(1f, false);
			}
		}
	}

	private void OnInsertSeeds(Item item)
	{
		if (this.m_Plant != null && item != null)
		{
			UnityEngine.Object.Destroy(item.gameObject);
			return;
		}
		this.m_ItemId = item.m_Info.m_ID;
		this.m_TimeToGrow = this.s_GrowTimeMap[(int)this.m_ItemId];
		this.PlantSeed();
		this.SetState(AcreState.Growing);
	}

	private void PlantSeed()
	{
		GameObject original = null;
		if (!this.s_VisMap.TryGetValue((int)this.m_ItemId, out original))
		{
			DebugUtils.Assert("Acre::OnInsertSeed - non existing value", true, DebugUtils.AssertType.Info);
			return;
		}
		this.m_Plant = UnityEngine.Object.Instantiate<GameObject>(original);
		this.m_Plant.transform.position = this.m_PlantRoot.position;
		this.m_Plant.transform.rotation = this.m_PlantRoot.rotation;
		this.m_Plant.transform.SetParent(this.m_PlantRoot, true);
		this.m_Plant.transform.localScale = Vector3.zero;
		this.m_LastUpdateTIme = MainLevel.Instance.GetCurrentTimeMinutes();
		ItemsManager.Get().OnPlant(this.m_ItemId);
	}

	public void OnRemoveItem(ItemSlot slot)
	{
	}

	public void UpdateInternal()
	{
		if (this.m_GrownPlant)
		{
			AcreRespawnFruits component = this.m_GrownPlant.GetComponent<AcreRespawnFruits>();
			if (component)
			{
				component.UpdateInternal();
			}
		}
		float num = Time.deltaTime;
		if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
		{
			num = Player.GetSleepTimeFactor();
		}
		if (RainManager.Get().IsRain())
		{
			this.m_WaterAmount += this.m_FillSpeed * num;
		}
		else if (this.m_ItemId == ItemID.None)
		{
			this.m_WaterAmount -= this.m_DrySpeed * num;
		}
		else
		{
			this.m_WaterAmount -= ItemsManager.Get().GetInfo(this.m_ItemId).m_AcreDehydration * num;
		}
		this.m_WaterAmount = Mathf.Clamp(this.m_WaterAmount, 0f, this.m_MaxWaterAmount);
		if (this.m_ItemId != ItemID.None)
		{
			this.m_FertilizerAmount -= ItemsManager.Get().GetInfo(this.m_ItemId).m_AcreDefertilization * num;
			this.m_FertilizerAmount = Mathf.Clamp(this.m_FertilizerAmount, 0f, this.m_MaxFertilizerAmount);
		}
		this.UpdateShaderProperties();
		float currentTimeMinutes = MainLevel.Instance.GetCurrentTimeMinutes();
		if (!this.m_Plant || this.m_AcreState != AcreState.Growing || this.m_WaterAmount <= 0f)
		{
			this.m_LastUpdateTIme = currentTimeMinutes;
			return;
		}
		bool flag = false;
		float num2 = this.m_Plant.transform.localScale.x;
		float num3 = 1f;
		this.m_NumBuffs = 0;
		if (this.m_WaterAmount > 50f)
		{
			num3 = 1.2f;
			this.m_NumBuffs++;
		}
		float num4 = 1f;
		if (this.m_FertilizerAmount / this.m_MaxFertilizerAmount > 0.5f)
		{
			num4 = 1.4f;
			this.m_NumBuffs += 2;
		}
		else if (this.m_FertilizerAmount / this.m_MaxFertilizerAmount > 0.01f)
		{
			num4 = 1.2f;
			this.m_NumBuffs++;
		}
		if (this.m_WaterAmount > 0f)
		{
			num2 += (currentTimeMinutes - this.m_LastUpdateTIme) / this.m_TimeToGrow * (num4 * num3);
		}
		if (num2 >= 1f)
		{
			flag = true;
		}
		num2 = Mathf.Clamp01(num2);
		Vector3 one = Vector3.one;
		one.Set(num2, num2, num2);
		if (this.m_Plant)
		{
			this.m_Plant.transform.localScale = one;
		}
		if (flag)
		{
			this.SetState(AcreState.Grown);
		}
		this.m_LastUpdateTIme = currentTimeMinutes;
	}

	private void UpdateShaderProperties()
	{
		bool flag = this.m_AcreState != AcreState.Ready;
		float value = this.m_WaterAmount / this.m_MaxWaterAmount;
		for (int i = 0; i < this.m_Materials.Count; i++)
		{
			this.m_Materials[i].SetFloat(Acre.s_TogglePlantPropertyID, flag ? 1f : 0f);
			this.m_Materials[i].SetFloat(Acre.s_WetnessPropertyID, value);
		}
		if (flag)
		{
			if (this.m_SoilEmptyObject.activeSelf)
			{
				this.m_SoilEmptyObject.SetActive(false);
			}
			if (!this.m_SoilSetObject.activeSelf)
			{
				this.m_SoilSetObject.SetActive(true);
				return;
			}
		}
		else
		{
			if (!this.m_SoilEmptyObject.activeSelf)
			{
				this.m_SoilEmptyObject.SetActive(true);
			}
			if (this.m_SoilSetObject.activeSelf)
			{
				this.m_SoilSetObject.SetActive(false);
			}
		}
	}

	public void SetState(AcreState state)
	{
		if (this.m_AcreState == state)
		{
			return;
		}
		this.m_AcreState = state;
		this.OnSetState();
	}

	private void OnSetState()
	{
		switch (this.m_AcreState)
		{
		case AcreState.NotReady:
			this.DeactivateSlot(this.m_WaterSlot);
			this.DeactivateSlot(this.m_ItemSlot);
			HUDProcess.Get().UnregisterProcess(this);
			return;
		case AcreState.Ready:
			if (this.m_ItemSlot.m_Item)
			{
				UnityEngine.Object.Destroy(this.m_ItemSlot.m_Item.gameObject);
			}
			this.DeactivateSlot(this.m_WaterSlot);
			this.ActivateSlot(this.m_ItemSlot);
			if (!HUDProcess.Get().IsProcessRegistered(this.m_AcreGrowProcess))
			{
				HUDProcess.Get().RegisterProcess(this.m_AcreGrowProcess, "plant_icon", this.m_AcreGrowProcess, true);
				return;
			}
			HUDProcess.Get().SetIcon(this.m_AcreGrowProcess, "plant_icon");
			return;
		case AcreState.Growing:
			this.ActivateSlot(this.m_WaterSlot);
			this.DeactivateSlot(this.m_ItemSlot);
			if (!HUDProcess.Get().IsProcessRegistered(this.m_AcreGrowProcess))
			{
				HUDProcess.Get().RegisterProcess(this.m_AcreGrowProcess, "grow_icon0", this.m_AcreGrowProcess, true);
				return;
			}
			HUDProcess.Get().SetIcon(this.m_AcreGrowProcess, "grow_icon0");
			return;
		case AcreState.Grown:
			this.ReplaceToGrown();
			if (this.m_GrownPlant.GetComponent<AcreRespawnFruits>() != null)
			{
				this.ActivateSlot(this.m_WaterSlot);
			}
			else
			{
				this.DeactivateSlot(this.m_WaterSlot);
			}
			this.DeactivateSlot(this.m_ItemSlot);
			HUDProcess.Get().UnregisterProcess(this.m_AcreGrowProcess);
			return;
		case AcreState.GrownNoFruits:
			this.ActivateSlot(this.m_WaterSlot);
			this.DeactivateSlot(this.m_ItemSlot);
			if (!HUDProcess.Get().IsProcessRegistered(this.m_AcreGrowProcess))
			{
				HUDProcess.Get().RegisterProcess(this.m_AcreGrowProcess, "grow_icon0", this.m_AcreGrowProcess, true);
				return;
			}
			HUDProcess.Get().SetIcon(this.m_AcreGrowProcess, "grow_icon0");
			return;
		default:
			return;
		}
	}

	private void ReplaceToGrown()
	{
		if (this.m_GrownPlant != null)
		{
			UnityEngine.Object.Destroy(this.m_GrownPlant);
		}
		this.m_GrownPlant = ItemsManager.Get().CreateItem((ItemID)this.s_ItemsMap[(int)this.m_ItemId], true, this.m_PlantRoot).gameObject;
		Item[] componentsDeepChild = General.GetComponentsDeepChild<Item>(this.m_GrownPlant);
		for (int i = 0; i < componentsDeepChild.Length; i++)
		{
			componentsDeepChild[i].m_CantSave = true;
		}
		if (this.m_GrownPlant.GetComponent<Item>() != null)
		{
			this.m_GrownPlant.GetComponent<Item>().m_Acre = this;
			this.m_GrownPlant.GetComponent<Item>().m_IsCut = true;
		}
		if (this.m_GrownPlant.GetComponent<AcreRespawnFruits>() != null)
		{
			this.m_GrownPlant.GetComponent<AcreRespawnFruits>().SetAcre(this);
		}
		if (this.m_Plant)
		{
			UnityEngine.Object.Destroy(this.m_Plant);
		}
		if (this.m_ItemSlot.m_Item)
		{
			UnityEngine.Object.Destroy(this.m_ItemSlot.m_Item.gameObject);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		AcresManager.Get().UnregisterAcre(this);
		if (this.m_Plant != null)
		{
			UnityEngine.Object.Destroy(this.m_Plant);
		}
		if (this.m_GrownPlant != null)
		{
			UnityEngine.Object.Destroy(this.m_GrownPlant);
		}
		HUDProcess.Get().UnregisterProcess(this);
	}

	public override bool CanExecuteActions()
	{
		return !Inventory3DManager.Get().IsActive();
	}

	public override bool CanTrigger()
	{
		return (!this.m_GrownPlant || this.m_GrownPlant.GetComponent<AcreRespawnFruits>()) && (this.m_AcreState == AcreState.NotReady || this.m_AcreState > AcreState.Ready);
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		base.GetActions(actions);
		if (this.m_AcreState == AcreState.NotReady || this.m_AcreState == AcreState.Growing)
		{
			actions.Add(TriggerAction.TYPE.Plow);
		}
	}

	public override string GetTriggerInfoLocalized()
	{
		if (this.m_AcreState != AcreState.Growing)
		{
			return base.GetTriggerInfoLocalized();
		}
		if (this.m_AcreType != AcreType.Large)
		{
			return GreenHellGame.Instance.GetLocalization().Get("Acre_Small", true);
		}
		return GreenHellGame.Instance.GetLocalization().Get("Acre", true);
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.Plow)
		{
			this.Plow();
		}
	}

	public void Plow()
	{
		if (this.m_Plant != null)
		{
			UnityEngine.Object.Destroy(this.m_Plant);
			this.m_Plant = null;
		}
		if (this.m_GrownPlant != null)
		{
			DestroyableObject component = this.m_GrownPlant.GetComponent<DestroyableObject>();
			if (component)
			{
				component.DestroyMe(null, "");
			}
			else
			{
				UnityEngine.Object.Destroy(this.m_GrownPlant);
			}
			this.m_GrownPlant = null;
		}
		if (this.m_ObjectWithTrunk != null)
		{
			UnityEngine.Object.Destroy(this.m_ObjectWithTrunk.gameObject);
			this.m_ObjectWithTrunk = null;
		}
		this.SetState(AcreState.Ready);
		this.m_WaterAmount = 0f;
		this.m_FertilizerAmount = 0f;
		this.m_AudioSource.PlayOneShot(Acre.s_PlowAudioClip);
		PlayerConditionModule.Get().GetDirtinessAdd(GetDirtyReason.Plow, null);
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("AcreState" + index, (int)this.m_AcreState);
		SaveGame.SaveVal("AcreItemId" + index, (int)this.m_ItemId);
		SaveGame.SaveVal("AcrePlant" + index, this.m_Plant != null);
		SaveGame.SaveVal("AcreTimeToGrow" + index, this.m_TimeToGrow);
		SaveGame.SaveVal("AcreGrownPlant" + index, this.m_GrownPlant != null);
		SaveGame.SaveVal("AcreLastUpdateTime" + index, this.m_LastUpdateTIme);
		if (this.m_GrownPlant)
		{
			AcreRespawnFruits component = this.m_GrownPlant.GetComponent<AcreRespawnFruits>();
			if (component != null)
			{
				SaveGame.SaveVal("AcreGrownPlantRespawn" + index, component.m_TimeToRespawn);
				int num = -1;
				for (int i = 0; i < component.m_ToCollect.Count; i++)
				{
					num = ((component.m_ToCollect[i] == null) ? (~(1 << i) & num) : (1 << i | num));
				}
				SaveGame.SaveVal("AcreGrownPlantMask" + index, num);
			}
		}
		SaveGame.SaveVal("AcreWaterAmount" + index, this.m_WaterAmount);
		SaveGame.SaveVal("AcreFertilizerAmount" + index, this.m_FertilizerAmount);
		float val = 0f;
		if (this.m_Plant != null)
		{
			val = this.m_Plant.transform.localScale.x;
		}
		SaveGame.SaveVal("AcreScale" + index, val);
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.m_ItemId = (ItemID)SaveGame.LoadIVal("AcreItemId" + index);
		this.SetState((AcreState)SaveGame.LoadIVal("AcreState" + index));
		if (SaveGame.LoadBVal("AcrePlant" + index))
		{
			this.PlantSeed();
			float num = SaveGame.LoadFVal("AcreScale" + index);
			Vector3 one = Vector3.one;
			one.Set(num, num, num);
			this.m_Plant.transform.localScale = one;
		}
		this.m_TimeToGrow = SaveGame.LoadFVal("AcreTimeToGrow" + index);
		if (SaveGame.LoadBVal("AcreGrownPlant" + index))
		{
			this.ReplaceToGrown();
			if (this.m_GrownPlant)
			{
				AcreRespawnFruits component = this.m_GrownPlant.GetComponent<AcreRespawnFruits>();
				if (component != null)
				{
					component.m_TimeToRespawn = SaveGame.LoadFVal("AcreGrownPlantRespawn" + index);
					int num2 = SaveGame.LoadIVal("AcreGrownPlantMask" + index);
					for (int i = 0; i < component.m_ToCollect.Count; i++)
					{
						if ((num2 & 1 << i) == 0)
						{
							UnityEngine.Object.Destroy(component.m_ToCollect[i]);
							component.m_ToCollect[i] = null;
						}
					}
				}
			}
		}
		this.m_LastUpdateTIme = SaveGame.LoadFVal("AcreLastUpdateTime" + index);
		this.m_WaterAmount = SaveGame.LoadFVal("AcreWaterAmount" + index);
		this.m_FertilizerAmount = SaveGame.LoadFVal("AcreFertilizerAmount" + index);
	}

	public float GetProcessProgress(Trigger trigger)
	{
		return this.m_WaterAmount / this.m_MaxWaterAmount;
	}

	public void RespawnGrown()
	{
		this.ReplaceToGrown();
	}

	public void OnTake(Item item)
	{
		if (this.m_GrownPlant)
		{
			AcreRespawnFruits component = this.m_GrownPlant.GetComponent<AcreRespawnFruits>();
			if (component != null)
			{
				component.OnTake(item);
			}
		}
	}

	public void OnEat(Item item)
	{
		if (this.m_GrownPlant)
		{
			AcreRespawnFruits component = this.m_GrownPlant.GetComponent<AcreRespawnFruits>();
			if (component != null)
			{
				component.OnEat(item);
			}
		}
	}

	public void OnTake(PlantFruit item)
	{
		if (this.m_GrownPlant)
		{
			AcreRespawnFruits component = this.m_GrownPlant.GetComponent<AcreRespawnFruits>();
			if (component != null)
			{
				component.OnTake(item);
			}
		}
	}

	public void OnEat(PlantFruit item)
	{
		if (this.m_GrownPlant)
		{
			AcreRespawnFruits component = this.m_GrownPlant.GetComponent<AcreRespawnFruits>();
			if (component != null)
			{
				component.OnEat(item);
			}
		}
	}

	public void OnTake(ItemReplacer item)
	{
		if (this.m_GrownPlant)
		{
			AcreRespawnFruits component = this.m_GrownPlant.GetComponent<AcreRespawnFruits>();
			if (component != null)
			{
				component.OnTake(item);
			}
		}
	}

	public void OnDestroyPlant(GameObject obj)
	{
		if (this.m_GrownPlant && SaveGame.m_State == SaveGame.State.None && !this.m_ForceNoRespawn)
		{
			AcreRespawnFruits component = this.m_GrownPlant.GetComponent<AcreRespawnFruits>();
			if (component && this.m_ItemId != ItemID.Cocona_Seeds)
			{
				component.OnDestroyPlant();
			}
		}
		this.m_ForceNoRespawn = false;
		if (this.m_GrownPlant == obj)
		{
			this.SetState(AcreState.NotReady);
		}
	}

	public override bool TakeDamage(DamageInfo damage_info)
	{
		return (!this.m_GrownPlant || !damage_info.m_Damager || !(damage_info.m_Damager == Player.Get().gameObject)) && base.TakeDamage(damage_info);
	}

	private void ActivateSlot(ItemSlot slot)
	{
		slot.m_ActivityUpdate = true;
		slot.Activate();
	}

	private void DeactivateSlot(ItemSlot slot)
	{
		slot.m_ActivityUpdate = false;
		slot.Deactivate();
	}

	public float GetRespawnProgress()
	{
		if (!this.m_GrownPlant)
		{
			return 0f;
		}
		AcreRespawnFruits component = this.m_GrownPlant.GetComponent<AcreRespawnFruits>();
		if (!component)
		{
			return 0f;
		}
		return component.GetRespawnProgress();
	}

	public override Vector3 GetHudInfoDisplayOffset()
	{
		return Vector3.down * 100f;
	}

	public override bool IsAcre()
	{
		return true;
	}

	public GameObject GetGrownPlant()
	{
		return this.m_GrownPlant;
	}

	private Dictionary<int, GameObject> s_VisMap;

	private Dictionary<int, int> s_ItemsMap;

	private Transform m_PlantRoot;

	[HideInInspector]
	public GameObject m_Plant;

	private AcreState m_AcreState = AcreState.None;

	private ItemID m_ItemId = ItemID.None;

	private float m_TimeToGrow = 30f;

	private GameObject m_GrownPlant;

	private ItemSlot m_ItemSlot;

	public ItemSlot m_WaterSlot;

	public float m_FillSpeed = 1f;

	public float m_DrySpeed = 0.5f;

	public float m_MaxWaterAmount = 100f;

	[HideInInspector]
	public float m_WaterAmount;

	public float m_MaxFertilizerAmount = 100f;

	[HideInInspector]
	public float m_FertilizerAmount;

	private Dictionary<int, float> s_GrowTimeMap;

	public Bounds m_Bounds;

	private bool m_Setup;

	public List<Material> m_Materials = new List<Material>();

	public List<Shader> m_Shaders = new List<Shader>();

	public List<GameObject> m_MaterialObjects = new List<GameObject>();

	private static int s_WetnessPropertyID = Shader.PropertyToID("_Wetness");

	private static int s_TogglePlantPropertyID = Shader.PropertyToID("_TogglePlant");

	[HideInInspector]
	public ObjectWithTrunk m_ObjectWithTrunk;

	public GameObject m_SoilEmptyObject;

	public GameObject m_SoilSetObject;

	public AcreGrowProcess m_AcreGrowProcess;

	public AcreType m_AcreType = AcreType.Large;

	private static AudioClip s_PlowAudioClip = null;

	public AudioSource m_AudioSource;

	private float m_LastUpdateTIme = -1f;

	[HideInInspector]
	public int m_NumBuffs;

	[HideInInspector]
	public bool m_ForceNoRespawn;
}
