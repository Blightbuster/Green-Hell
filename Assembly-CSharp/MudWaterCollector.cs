using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class MudWaterCollector : Construction, IItemSlotParent, ITriggerThrough, ITriggerOwner, IProcessor
{
	protected override void Awake()
	{
		base.Awake();
		this.m_WaterStringLocalized = GreenHellGame.Instance.GetLocalization().Get("LiquidType_Water", true);
		this.m_CollectorWaterRenderer = this.m_CollectorWaterVis.gameObject.GetComponent<Renderer>();
		this.m_CollectorData.m_Capacity = this.m_CollectorCapacity;
		this.m_CollectorData.m_YPosMin = 0.4f;
		this.m_CollectorData.m_YPosMax = 0.85f;
		this.m_ContainerData.m_Capacity = this.m_ContainerCapacity;
		this.m_ContainerData.m_YPosMin = 0.23f;
		this.m_ContainerData.m_YPosMax = 0.5f;
		this.m_CollectorWaterVis.SetOwner(this);
		this.m_ContainerWaterVis.SetOwner(this);
		this.SetCollectorLiquidType(LiquidType.Water);
		GameObject gameObject = new GameObject();
		gameObject.name = "GetSlot";
		this.m_GetFromContainerSlot = gameObject.AddComponent<ItemSlot>();
		this.m_GetFromContainerSlot.transform.rotation = base.transform.rotation;
		gameObject.transform.parent = base.transform;
		this.m_GetFromContainerSlot.m_ItemTypeList.Add(ItemType.LiquidContainer);
		this.m_GetFromContainerSlot.m_ShowOnlyIfItemIsCorrect = true;
		this.m_GetFromContainerSlot.m_GOParent = base.gameObject;
		this.m_GetFromContainerSlot.m_ISParents = new IItemSlotParent[1];
		this.m_GetFromContainerSlot.m_ISParents[0] = this;
		this.m_GetFromContainerSlot.m_ForceTransformPos = true;
		this.m_GetFromContainerSlot.m_CanSelect = false;
		this.m_GetFromContainerSlot.SetIcon("HUD_get_water");
		gameObject = new GameObject();
		gameObject.name = "GetSlot";
		this.m_GetFromCollectorSlot = gameObject.AddComponent<ItemSlot>();
		this.m_GetFromCollectorSlot.transform.rotation = base.transform.rotation;
		gameObject.transform.parent = base.transform;
		this.m_GetFromCollectorSlot.m_ItemTypeList.Add(ItemType.LiquidContainer);
		this.m_GetFromCollectorSlot.m_ItemTypeList.Add(ItemType.Bowl);
		this.m_GetFromCollectorSlot.m_ShowOnlyIfItemIsCorrect = true;
		this.m_GetFromCollectorSlot.m_GOParent = base.gameObject;
		this.m_GetFromCollectorSlot.m_ISParents = new IItemSlotParent[1];
		this.m_GetFromCollectorSlot.m_ISParents[0] = this;
		this.m_GetFromCollectorSlot.m_ItemParent = this;
		this.m_GetFromCollectorSlot.SetIcon("HUD_get_water");
		this.m_GetFromCollectorSlot.m_CanSelect = false;
		this.m_GetFromCollectorSlot.gameObject.SetActive(false);
		gameObject = new GameObject();
		gameObject.name = "PourSlot";
		this.m_PourToCollectorSlot = gameObject.AddComponent<ItemSlot>();
		this.m_PourToCollectorSlot.transform.rotation = base.transform.rotation;
		gameObject.transform.parent = base.transform;
		this.m_PourToCollectorSlot.m_ItemTypeList.Add(ItemType.LiquidContainer);
		this.m_PourToCollectorSlot.m_ItemTypeList.Add(ItemType.Bowl);
		this.m_PourToCollectorSlot.m_ShowOnlyIfItemIsCorrect = true;
		this.m_PourToCollectorSlot.m_GOParent = base.gameObject;
		this.m_PourToCollectorSlot.m_ISParents = new IItemSlotParent[1];
		this.m_PourToCollectorSlot.m_ISParents[0] = this;
		this.m_PourToCollectorSlot.m_ItemParent = this;
		this.m_PourToCollectorSlot.SetIcon("HUD_pourOut_water");
		this.m_PourToCollectorSlot.m_CanSelect = false;
		this.m_PourToCollectorSlot.gameObject.SetActive(false);
		base.RegisterConstantUpdateItem();
	}

	protected override void Start()
	{
		base.Start();
		HUDProcess.Get().RegisterProcess(this.m_CollectorWaterVis, "water_filtering", this, true);
		HUDProcess.Get().RegisterProcess(this.m_ContainerWaterVis, "HUD_drinking_water", this, true);
		this.UpdateWaterVis();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		base.UnregisterConstantUpdateItem();
	}

	public float GetProcessProgress(Trigger trigger)
	{
		if (trigger == this.m_CollectorWaterVis)
		{
			return this.m_CollectorData.m_Amount / this.m_CollectorData.m_Capacity;
		}
		if (trigger == this.m_ContainerWaterVis)
		{
			return this.m_ContainerData.m_Amount / this.m_ContainerData.m_Capacity;
		}
		return 0f;
	}

	private void SetCollectorLiquidType(LiquidType type)
	{
		if (this.m_CollectorLiquidType == type)
		{
			return;
		}
		this.m_CollectorLiquidType = type;
		if (type == LiquidType.Water)
		{
			this.m_CollectorWaterRenderer.material = this.m_CleanWaterMaterial;
			return;
		}
		this.m_CollectorWaterRenderer.material = this.m_DirtyWaterMaterial;
	}

	public bool CanInsertItem(Item item)
	{
		return true;
	}

	public void OnInsertItem(ItemSlot slot)
	{
		if (slot == this.m_GetFromContainerSlot)
		{
			Item item = slot.m_Item;
			this.TransferLiquids(LiquidType.Water, this.m_ContainerData, (LiquidContainer)item);
			slot.RemoveItem();
			InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, true);
			return;
		}
		if (slot == this.m_GetFromCollectorSlot)
		{
			Item item2 = slot.m_Item;
			this.TransferLiquids(this.m_CollectorLiquidType, this.m_CollectorData, (LiquidContainer)item2);
			slot.RemoveItem();
			InventoryBackpack.Get().InsertItem(item2, null, null, true, true, true, true, true);
			return;
		}
		if (slot == this.m_PourToCollectorSlot)
		{
			Item item3 = slot.m_Item;
			this.TransferLiquids(this.m_CollectorLiquidType, (LiquidContainer)item3, this.m_CollectorData);
			slot.RemoveItem();
			InventoryBackpack.Get().InsertItem(item3, null, null, true, true, true, true, true);
		}
	}

	private void TransferLiquids(LiquidType liquid_type, ContainerData from, LiquidContainer to)
	{
		LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)to.m_Info;
		if (liquid_type != liquidContainerInfo.m_LiquidType)
		{
			if (liquidContainerInfo.m_Amount > 0f)
			{
				HUDMessages.Get().AddMessage(GreenHellGame.Instance.GetLocalization().Get("Liquids_Conflict", true), null, HUDMessageIcon.None, "", null);
				return;
			}
			liquidContainerInfo.m_LiquidType = liquid_type;
		}
		float num = liquidContainerInfo.m_Capacity - liquidContainerInfo.m_Amount;
		num = Mathf.Min(num, from.m_Amount);
		liquidContainerInfo.m_Amount += num;
		to.OnGet();
		from.m_Amount -= num;
		PlayerAudioModule.Get().PlayWaterSpillSound(1f, false);
	}

	private void TransferLiquids(LiquidType liquid_type, LiquidContainer from, ContainerData to)
	{
		LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)from.m_Info;
		if (liquid_type != liquidContainerInfo.m_LiquidType)
		{
			if (to.m_Amount > 0f)
			{
				HUDMessages.Get().AddMessage(GreenHellGame.Instance.GetLocalization().Get("Liquids_Conflict", true), null, HUDMessageIcon.None, "", null);
				return;
			}
			this.SetCollectorLiquidType(liquidContainerInfo.m_LiquidType);
		}
		to.m_Amount += liquidContainerInfo.m_Amount;
		if (to.m_Amount > to.m_Capacity)
		{
			to.m_Amount = to.m_Capacity;
		}
		liquidContainerInfo.m_Amount = 0f;
		PlayerAudioModule.Get().PlayWaterSpillSound(1f, false);
	}

	public void OnRemoveItem(ItemSlot slot)
	{
	}

	public bool CanTrigger(Trigger trigger)
	{
		if (trigger == this.m_CollectorWaterVis)
		{
			return this.m_CollectorData.m_Amount >= 1f;
		}
		return trigger == this.m_ContainerWaterVis && this.m_ContainerData.m_Amount >= 1f;
	}

	public void OnExecute(Trigger trigger, TriggerAction.TYPE action)
	{
		if (trigger == this.m_CollectorWaterVis)
		{
			float num = Mathf.Min(this.m_CollectorData.m_Amount, this.m_SipHydration);
			Player.Get().GetComponent<EatingController>().Drink(this.m_CollectorLiquidType, num);
			this.m_CollectorData.m_Amount -= num;
			return;
		}
		if (trigger == this.m_ContainerWaterVis)
		{
			float num2 = Mathf.Min(this.m_ContainerData.m_Amount, this.m_SipHydration);
			Player.Get().GetComponent<EatingController>().Drink(LiquidType.Water, num2);
			this.m_ContainerData.m_Amount -= num2;
		}
	}

	public void GetActions(Trigger trigger, List<TriggerAction.TYPE> actions)
	{
		actions.Add(TriggerAction.TYPE.Drink);
	}

	public string GetTriggerInfoLocalized(Trigger trigger)
	{
		if (trigger == this.m_CollectorWaterVis)
		{
			return GreenHellGame.Instance.GetLocalization().Get("LiquidType_" + this.m_CollectorLiquidType.ToString(), true);
		}
		return this.m_WaterStringLocalized;
	}

	public string GetIconName(Trigger trigger)
	{
		return "HUD_drinking_water";
	}

	public override void ConstantUpdate()
	{
		base.ConstantUpdate();
		float delta_time = Time.deltaTime;
		if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
		{
			delta_time = Player.GetSleepTimeFactor();
		}
		this.UpdateWaterFromRain(delta_time);
		this.UpdateFiltering(delta_time);
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateWaterVis();
		this.UpdateSlotActivity();
		this.UpdateSlotPos();
	}

	private void UpdateWaterFromRain(float delta_time)
	{
		if (this.m_CollectorData.m_Amount < this.m_CollectorData.m_Capacity && RainManager.Get().IsRain() && !RainManager.Get().IsInRainCutter(this.m_CollectorWaterVis.transform.position))
		{
			if (this.m_CollectorData.m_Amount < 1f)
			{
				this.m_CollectorLiquidType = LiquidType.Water;
			}
			this.m_CollectorData.m_Amount += delta_time;
			this.m_CollectorData.m_Amount = Mathf.Clamp(this.m_CollectorData.m_Amount, 0f, this.m_CollectorData.m_Capacity);
		}
	}

	private void UpdateWaterVis()
	{
		float num = this.m_CollectorData.m_Amount / this.m_CollectorData.m_Capacity;
		Vector3 localPosition = this.m_CollectorWaterVis.transform.localPosition;
		localPosition.y = this.m_CollectorData.m_YPosMin + (this.m_CollectorData.m_YPosMax - this.m_CollectorData.m_YPosMin) * num;
		this.m_CollectorWaterVis.transform.localPosition = localPosition;
		float num2 = this.m_ContainerData.m_Amount / this.m_ContainerData.m_Capacity;
		localPosition = this.m_ContainerWaterVis.transform.localPosition;
		localPosition.y = this.m_ContainerData.m_YPosMin + (this.m_ContainerData.m_YPosMax - this.m_ContainerData.m_YPosMin) * num2;
		this.m_ContainerWaterVis.transform.localPosition = localPosition;
	}

	private void UpdateFiltering(float delta_time)
	{
		bool flag = false;
		if (this.m_CollectorData.m_Amount > 0f && this.m_ContainerData.m_Amount < this.m_ContainerData.m_Capacity)
		{
			if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
			{
				delta_time = Player.GetSleepTimeFactor();
			}
			float num = delta_time * 0.5f;
			if (num > this.m_CollectorData.m_Amount)
			{
				num = this.m_CollectorData.m_Amount;
			}
			this.m_CollectorData.m_Amount -= num;
			this.m_ContainerData.m_Amount += num;
			flag = true;
		}
		if (this.m_Particle.activeSelf != flag)
		{
			this.m_Particle.SetActive(flag);
		}
	}

	private void UpdateSlotActivity()
	{
		if (!Inventory3DManager.Get().m_CarriedItem)
		{
			this.m_GetFromContainerSlot.gameObject.SetActive(false);
		}
		else if (Inventory3DManager.Get().m_CarriedItem.m_Info.m_ID == ItemID.Coconut)
		{
			this.m_GetFromContainerSlot.gameObject.SetActive(false);
		}
		else if (this.m_GetFromContainerSlot.gameObject.activeSelf)
		{
			if (!this.m_GetFromContainerSlot.CanInsertItem(Inventory3DManager.Get().m_CarriedItem))
			{
				this.m_GetFromContainerSlot.gameObject.SetActive(false);
			}
		}
		else if (this.m_GetFromContainerSlot.CanInsertItem(Inventory3DManager.Get().m_CarriedItem))
		{
			LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info;
			if (liquidContainerInfo.m_Amount < liquidContainerInfo.m_Capacity)
			{
				this.m_GetFromContainerSlot.gameObject.SetActive(true);
			}
		}
		if (this.m_Info.m_ID == ItemID.Coconut)
		{
			this.m_GetFromCollectorSlot.gameObject.SetActive(false);
			this.m_PourToCollectorSlot.gameObject.SetActive(false);
		}
		if (this.m_GetFromCollectorSlot)
		{
			if (this.m_CollectorData.m_Amount < 1f)
			{
				this.m_GetFromCollectorSlot.gameObject.SetActive(false);
			}
			else if (this.m_CollectorData.m_Amount > 0f && this.m_CollectorLiquidType != LiquidType.Water && this.m_CollectorLiquidType != LiquidType.UnsafeWater && this.m_CollectorLiquidType != LiquidType.DirtyWater)
			{
				this.m_GetFromCollectorSlot.gameObject.SetActive(false);
			}
			else if (this.m_GetFromCollectorSlot.gameObject.activeSelf)
			{
				if (this.m_CollectorData.m_Amount < 1f)
				{
					this.m_GetFromCollectorSlot.gameObject.SetActive(false);
				}
				else if (!this.m_GetFromCollectorSlot.CanInsertItem(Inventory3DManager.Get().m_CarriedItem))
				{
					this.m_GetFromCollectorSlot.gameObject.SetActive(false);
				}
				else
				{
					LiquidContainerInfo liquidContainerInfo2 = (LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info;
					if (liquidContainerInfo2.m_Amount >= liquidContainerInfo2.m_Capacity)
					{
						this.m_GetFromCollectorSlot.gameObject.SetActive(false);
					}
				}
			}
			else if (this.m_GetFromCollectorSlot.CanInsertItem(Inventory3DManager.Get().m_CarriedItem))
			{
				if (Inventory3DManager.Get().m_CarriedItem.m_Info.m_ID == ItemID.Coconut)
				{
					this.m_GetFromCollectorSlot.gameObject.SetActive(false);
				}
				else
				{
					LiquidContainerInfo liquidContainerInfo3 = (LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info;
					if (liquidContainerInfo3.m_Amount < liquidContainerInfo3.m_Capacity)
					{
						this.m_GetFromCollectorSlot.gameObject.SetActive(true);
					}
				}
			}
		}
		if (this.m_PourToCollectorSlot)
		{
			if (this.m_PourToCollectorSlot.gameObject.activeSelf)
			{
				if (!Inventory3DManager.Get().m_CarriedItem)
				{
					this.m_PourToCollectorSlot.gameObject.SetActive(false);
					return;
				}
				if (((LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info).m_Amount < 1f)
				{
					this.m_PourToCollectorSlot.gameObject.SetActive(false);
					return;
				}
				if (!this.m_PourToCollectorSlot.CanInsertItem(Inventory3DManager.Get().m_CarriedItem))
				{
					this.m_PourToCollectorSlot.gameObject.SetActive(false);
					return;
				}
				if (this.m_CollectorData.m_Amount >= this.m_CollectorData.m_Capacity)
				{
					this.m_PourToCollectorSlot.gameObject.SetActive(false);
					return;
				}
			}
			else if (this.m_PourToCollectorSlot.CanInsertItem(Inventory3DManager.Get().m_CarriedItem))
			{
				if ((this.m_Info.m_ID == ItemID.Bidon || this.m_Info.m_ID == ItemID.Coconut_Bidon) && Inventory3DManager.Get().m_CarriedItem.m_Info.IsLiquidContainer() && ((LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info).m_Amount >= 1f && ((LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info).m_LiquidType != LiquidType.Water && ((LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info).m_LiquidType != LiquidType.UnsafeWater && ((LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info).m_LiquidType != LiquidType.DirtyWater)
				{
					this.m_PourToCollectorSlot.gameObject.SetActive(false);
					return;
				}
				if (this.m_CollectorData.m_Amount < this.m_CollectorData.m_Capacity)
				{
					this.m_PourToCollectorSlot.gameObject.SetActive(true);
				}
			}
		}
	}

	private void UpdateSlotPos()
	{
		this.m_GetFromContainerSlot.transform.position = this.m_ContainerWaterVis.transform.position;
		float num = 0.05f;
		Vector3 localPosition = this.m_CollectorWaterVis.transform.localPosition;
		localPosition.x += num;
		this.m_GetFromCollectorSlot.transform.localPosition = localPosition;
		localPosition = this.m_CollectorWaterVis.transform.localPosition;
		localPosition.x -= num;
		this.m_PourToCollectorSlot.transform.localPosition = localPosition;
	}

	public override void DestroyMe(bool check_connected = true)
	{
		base.DestroyMe(check_connected);
		if (this.m_CollectorData.m_Amount > 0f || this.m_ContainerData.m_Amount > 0f)
		{
			PlayerAudioModule.Get().PlayWaterSpillSound(1f, false);
		}
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("MudWaterCollectorCLT" + index, (int)this.m_CollectorLiquidType);
		SaveGame.SaveVal("MudWaterCollectorColAmount" + index, this.m_CollectorData.m_Amount);
		SaveGame.SaveVal("MudWaterCollectorConAmount" + index, this.m_ContainerData.m_Amount);
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.SetCollectorLiquidType((LiquidType)SaveGame.LoadIVal("MudWaterCollectorCLT" + index));
		this.m_CollectorData.m_Amount = SaveGame.LoadFVal("MudWaterCollectorColAmount" + index);
		this.m_ContainerData.m_Amount = SaveGame.LoadFVal("MudWaterCollectorConAmount" + index);
		this.UpdateWaterVis();
	}

	public float m_CollectorCapacity = 100f;

	public float m_ContainerCapacity = 100f;

	public Trigger m_CollectorWaterVis;

	private Renderer m_CollectorWaterRenderer;

	public Trigger m_ContainerWaterVis;

	public GameObject m_Particle;

	private ContainerData m_CollectorData = new ContainerData();

	private ContainerData m_ContainerData = new ContainerData();

	private LiquidType m_CollectorLiquidType;

	public float m_SipHydration = 20f;

	private string m_WaterStringLocalized = string.Empty;

	public Material m_CleanWaterMaterial;

	public Material m_DirtyWaterMaterial;

	private ItemSlot m_GetFromCollectorSlot;

	private ItemSlot m_PourToCollectorSlot;

	private ItemSlot m_GetFromContainerSlot;
}
