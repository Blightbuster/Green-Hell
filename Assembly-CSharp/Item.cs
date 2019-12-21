using System;
using System.Collections.Generic;
using AIs;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class Item : Trigger, IReplicatedBehaviour, ICustomReplicationInterval, IReplicatedTransformParams
{
	[HideInInspector]
	public ItemSlot m_CurrentSlot
	{
		get
		{
			return this.m_CurrentSlotProp;
		}
		set
		{
			this.m_CurrentSlotProp = value;
			this.UpdateScale(false);
		}
	}

	[HideInInspector]
	public bool m_InStorage
	{
		get
		{
			return this.m_InStorageProp;
		}
		set
		{
			this.m_InStorageProp = value;
			this.UpdatePhx();
			this.UpdateLayer();
			this.UpdateScale(false);
		}
	}

	[HideInInspector]
	[Replicate(new string[]
	{

	})]
	public bool m_InInventory
	{
		get
		{
			return this.m_InInventoryProp;
		}
		set
		{
			this.m_InInventoryProp = value;
			this.UpdatePhx();
			this.UpdateLayer();
			this.UpdateScale(false);
		}
	}

	[HideInInspector]
	public bool m_OnCraftingTable
	{
		get
		{
			return this.m_OnCraftingTableProp;
		}
		set
		{
			this.m_OnCraftingTableProp = value;
			this.UpdatePhx();
			this.UpdateLayer();
			this.UpdateScale(false);
		}
	}

	protected int m_PhxStaticRequests
	{
		get
		{
			return this.m_PhxStaticRequestsProp;
		}
		set
		{
			this.m_PhxStaticRequestsProp = value;
			this.UpdatePhx();
		}
	}

	public bool m_StaticPhx
	{
		get
		{
			return this.m_StaticPhxProp;
		}
		set
		{
			this.m_StaticPhxProp = value;
			this.UpdatePhx();
		}
	}

	public bool m_ForceNoKinematic
	{
		get
		{
			return this.m_ForceNoKinematicProp;
		}
		set
		{
			this.m_ForceNoKinematicProp = value;
			this.UpdatePhx();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		this.m_Initialized = false;
		this.m_Registered = false;
		this.m_UniqueID = ItemsManager.s_ItemUniqueID++;
		DebugUtils.Assert(this.m_InfoName != "None", "m_InfoName of object " + base.name + " is not set!", true, DebugUtils.AssertType.Info);
		this.m_Holder = base.transform.Find("Holder");
		this.m_Rigidbody = base.gameObject.GetComponent<Rigidbody>();
		if (this.m_Rigidbody)
		{
			this.m_DefaultDrag = this.m_Rigidbody.drag;
		}
		this.m_DamagerStart = base.gameObject.transform.FindDeepChild("DamageStart");
		this.m_DamagerEnd = base.gameObject.transform.FindDeepChild("DamageEnd");
		this.m_LODGroup = base.gameObject.GetComponent<LODGroup>();
		this.InitInventorySlot();
		ItemID type = this.m_InfoName.Empty() ? ItemID.Banana : EnumUtils<ItemID>.GetValue(this.m_InfoName);
		this.m_IsPlant = type.IsPlant();
		this.m_IsTree = type.IsTree();
		this.m_IsLeaf = type.IsLeaf();
		this.m_IsFallen = type.IsFallen();
		Transform transform = base.transform.Find("Pivot");
		if (transform)
		{
			this.m_Pivot = transform.gameObject;
		}
		this.m_TrailRenderer = base.gameObject.GetComponent<TrailRenderer>();
		if (this.m_TrailRenderer)
		{
			this.m_TrailRenderer.enabled = false;
		}
		this.m_CanBeOutlined = true;
		this.m_DefaultLayer = ((this.m_IsPlant || this.m_IsTree) ? base.gameObject.layer : LayerMask.NameToLayer("Item"));
		this.SetLayer(base.transform, this.m_DefaultLayer);
		this.m_Hook = base.gameObject.GetComponent<FishingHook>();
		if (this.m_StaticPhx)
		{
			if (this.m_BoxCollider)
			{
				this.m_BoxCollider.isTrigger = true;
			}
			if (this.m_Rigidbody)
			{
				this.m_Rigidbody.isKinematic = true;
			}
			this.UpdatePhx();
		}
		if (base.name == "QuestItem_Key")
		{
			ItemsManager.Get().m_QuestItemKey = this;
		}
		this.SetupInfo();
	}

	public void InitInventorySlot()
	{
		if (this.m_InventorySlot)
		{
			return;
		}
		if (base.gameObject.GetComponent<DestroyIfNoChildren>())
		{
			return;
		}
		this.m_InventorySlot = base.transform.GetComponentInChildren<ItemSlotStack>(true);
		if (this.m_InventorySlot)
		{
			this.m_InventorySlot.m_BackpackSlot = true;
			this.m_InventorySlot.m_InventoryStackSlot = true;
			this.m_InventorySlot.m_ShowOnlyIfItemIsCorrect = true;
		}
	}

	public override bool TriggerThrough()
	{
		return this.m_IsPlant || this.m_IsTree;
	}

	protected override void Start()
	{
		this.Initialize(true);
		base.Start();
		if (this.m_Rigidbody && !this.m_Rigidbody.isKinematic)
		{
			this.m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
		}
	}

	public void Initialize(bool im_register)
	{
		if (this.m_Initialized)
		{
			return;
		}
		this.m_Initialized = true;
		if (this.m_InventorySlot)
		{
			this.m_InventorySlot.m_Camera = Inventory3DManager.Get().m_Camera;
		}
		if (this.m_Info.m_Static)
		{
			this.ItemsManagerRegister(false);
			this.m_DestroyingOnlyScript = true;
			UnityEngine.Object.Destroy(this);
			return;
		}
		if (Item.s_AllItemIDs.ContainsKey((int)this.m_Info.m_ID))
		{
			Item.s_AllItemIDs[(int)this.m_Info.m_ID] = Item.s_AllItemIDs[(int)this.m_Info.m_ID] + 1;
		}
		else
		{
			Item.s_AllItemIDs.Add((int)this.m_Info.m_ID, 1);
		}
		Item.s_AllItems.Add(this);
		this.m_ScenarioItem = false;
		if (base.gameObject.scene.IsValid() && GreenHellGame.Instance.m_ScenarioScenes.Contains(base.gameObject.scene.name))
		{
			this.m_ScenarioItem = true;
		}
		if (GreenHellGame.ROADSHOW_DEMO)
		{
			if (this.m_Info.m_BackpackPocket == BackpackPocket.Front || this.m_Info.m_BackpackPocket == BackpackPocket.Right)
			{
				this.m_Info.m_BackpackPocket = BackpackPocket.Main;
			}
			this.m_Info.m_Craftable = false;
		}
		if (this.m_Info.m_DestroyedPrefabName != string.Empty || this.m_Info.m_ItemsToBackpackOnDestroy.Count > 0 || base.gameObject.GetComponent<DestroyablePlant>())
		{
			if (this.m_Info.m_DestroyedIsFalling)
			{
				this.m_DestroyableObject = base.gameObject.AddComponent<DestroyableFallingObject>();
			}
			else
			{
				this.m_DestroyableObject = base.gameObject.AddComponent<DestroyableObject>();
			}
			this.m_DestroyableObject.m_Item = this;
		}
		if (this.m_Info.m_CanEquip || this.m_Info.IsHeavyObject())
		{
			DebugUtils.Assert(this.m_Holder != null, "Missing Holder in item " + this.m_InfoName, true, DebugUtils.AssertType.Info);
		}
		if (im_register)
		{
			this.ItemsManagerRegister(true);
		}
		this.m_DefaultLocalScale = base.transform.localScale;
		this.m_InventoryLocalScale = Vector3.one * this.m_Info.m_InventoryScale;
		if (this.m_BoxCollider)
		{
			this.m_DefaultSize = this.m_BoxCollider.size;
		}
		this.m_ThisRenderer = base.gameObject.GetComponent<Renderer>();
		this.m_ChildrenRenderers = base.gameObject.GetComponentsInChildren<Renderer>(true);
		this.m_DestroyableFallingObj = base.gameObject.GetComponent<DestroyableFallingObject>();
		if (this.m_DestroyableFallingObj != null && this.m_AddAngularVelOnStart)
		{
			this.m_DestroyableFallingObj.AddAngularVelocityOnStart(this.m_AngularVelocityOnStart, this.m_AddAngularVelocityOnStartDuration);
		}
	}

	public void AddAngularVelocityOnStart(Vector3 ang_vel, float duration)
	{
		this.m_AddAngularVelOnStart = true;
		this.m_AngularVelocityOnStart = ang_vel;
		this.m_AddAngularVelocityOnStartDuration = duration;
	}

	public static Item Find(ItemID id)
	{
		foreach (Item item in Item.s_AllItems)
		{
			if (item.GetInfoID() == id)
			{
				return item;
			}
		}
		return null;
	}

	private void CreateNavMeshObstacle()
	{
		if (!this.m_BoxCollider)
		{
			return;
		}
		if (this.m_IsPlant || this.m_IsTree || this.m_IsCut || this.m_IsFallen)
		{
			return;
		}
		if (base.gameObject.GetComponent<NavMeshObstacle>() == null)
		{
			if (!base.gameObject.GetMesh())
			{
				Debug.Log("Can't find mesh of Item - " + this.m_InfoName);
				return;
			}
			this.m_NavMeshObstacle = base.gameObject.AddComponent<NavMeshObstacle>();
			this.m_NavMeshObstacle.center = this.m_BoxCollider.center;
			this.m_NavMeshObstacle.size = this.m_BoxCollider.size;
			this.m_NavMeshObstacle.carving = true;
			this.m_NavMeshObstacle.enabled = false;
		}
	}

	public virtual void SetupInfo()
	{
		if (this.m_Info == null)
		{
			this.m_Info = ItemsManager.Get().CreateItemInfo(this);
		}
	}

	public void ItemsManagerRegister(bool update_activity = false)
	{
		if (this.m_Registered)
		{
			return;
		}
		ItemsManager.Get().RegisterItem(this, update_activity);
		this.m_Registered = true;
	}

	public void ItemsManagerUnregister()
	{
		if (!this.m_Registered)
		{
			return;
		}
		ItemsManager.Get().UnregisterItem(this);
		this.m_Registered = false;
	}

	public virtual void OnAddToInventory()
	{
		this.StaticPhxRequestAdd();
		this.m_InPlayersHand = false;
		this.m_InInventory = true;
		this.m_AttachedToSpear = false;
		base.TryRemoveFromFallenObjectsMan();
		this.ItemsManagerUnregister();
		if (!this.m_WasTriggered)
		{
			this.m_WasTriggered = true;
			this.m_FirstTriggerTime = MainLevel.Instance.m_TODSky.Cycle.GameTime;
		}
		this.m_StaticPhx = false;
		this.m_ForceNoKinematic = false;
	}

	public virtual void OnRemoveFromInventory()
	{
		base.transform.parent = null;
		this.StaticPhxRequestRemove();
		this.m_InInventory = false;
		this.ItemsManagerRegister(false);
		this.m_ShownInInventory = false;
	}

	public virtual void OnAddToStorage(Storage storage)
	{
		this.m_Storage = storage;
		this.StaticPhxRequestAdd();
		this.m_InPlayersHand = false;
		this.m_InStorage = true;
		this.m_AttachedToSpear = false;
		base.TryRemoveFromFallenObjectsMan();
		this.ItemsManagerUnregister();
		if (!this.m_WasTriggered)
		{
			this.m_WasTriggered = true;
			this.m_FirstTriggerTime = MainLevel.Instance.m_TODSky.Cycle.GameTime;
		}
		this.m_StaticPhx = false;
		this.m_ForceNoKinematic = false;
	}

	public virtual void OnRemoveFromStorage()
	{
		this.m_Storage = null;
		if (!this.m_IsBeingDestroyed)
		{
			base.transform.parent = null;
		}
		this.StaticPhxRequestRemove();
		this.m_InStorage = false;
		this.ItemsManagerRegister(false);
	}

	public override bool CanTrigger()
	{
		if (this.m_CantTriggerDuringDialog && DialogsManager.Get().IsAnyDialogPlaying())
		{
			return false;
		}
		if (this.m_Thrown || this.m_BlockTrigger || this.m_HallucinationDisappearing)
		{
			return false;
		}
		if (!Inventory3DManager.Get().isActiveAndEnabled && this.m_InPlayersHand)
		{
			return false;
		}
		bool flag = base.enabled && this.m_Info != null && this.m_Info.m_Health > 0f;
		if (this.m_Hook != null)
		{
			return this.m_Hook.CanTrigger();
		}
		if (this.m_CurrentSlot != null && this.m_CurrentSlot.m_IsHookBaitSlot)
		{
			return Inventory3DManager.Get().IsActive();
		}
		return (!(base.transform.parent != null) || !(base.transform.parent.GetComponent<AcreGrowProcess>() != null)) && flag;
	}

	public override string GetName()
	{
		return this.m_InfoName;
	}

	public override string GetTriggerInfoLocalized()
	{
		if (this.m_Info.m_LockedInfoID != string.Empty && !ItemsManager.Get().m_UnlockedItemInfos.Contains(this.m_Info.m_ID))
		{
			return GreenHellGame.Instance.GetLocalization().Get(this.m_Info.m_LockedInfoID, true);
		}
		return GreenHellGame.Instance.GetLocalization().Get(this.GetName(), true);
	}

	public override void GetInfoText(ref string result)
	{
		base.GetInfoText(ref result);
		this.m_Info.GetInfoText(ref result);
	}

	public override bool IsItem()
	{
		return true;
	}

	public ItemID GetInfoID()
	{
		return this.m_Info.m_ID;
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		this.m_CanSaveNotTriggered = true;
		if (action == TriggerAction.TYPE.Take || action == TriggerAction.TYPE.TakeHold || action == TriggerAction.TYPE.TakeHoldLong)
		{
			this.Take();
			return;
		}
		if (action == TriggerAction.TYPE.Expand)
		{
			HUDItem.Get().Activate(this);
			return;
		}
		if (action == TriggerAction.TYPE.PickUp)
		{
			this.PickUp(true);
			return;
		}
		if (action == TriggerAction.TYPE.SwapHold)
		{
			this.Swap();
			return;
		}
		if (action == TriggerAction.TYPE.Pick)
		{
			Inventory3DManager.Get().m_BlockLMouseUP = true;
			Inventory3DManager.Get().StartCarryItem(this, false);
		}
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		if (this.m_OnCraftingTable)
		{
			actions.Add(TriggerAction.TYPE.Remove);
			return;
		}
		if (this.m_Info.m_CanBeAddedToInventory)
		{
			if (GreenHellGame.IsPadControllerActive() && Inventory3DManager.Get().IsActive() && Inventory3DManager.Get().CanSetCarriedItem(true))
			{
				actions.Add(TriggerAction.TYPE.Pick);
			}
			actions.Add(TriggerAction.TYPE.Take);
			actions.Add(TriggerAction.TYPE.Expand);
			return;
		}
		if (!this.m_AttachedToSpear && (this.m_Info.m_Harvestable || this.m_Info.m_Eatable || this.m_Info.CanDrink() || this.m_Info.m_Craftable))
		{
			actions.Add(TriggerAction.TYPE.Expand);
		}
	}

	public override string GetIconName()
	{
		return this.m_Info.m_IconName;
	}

	public override ItemAdditionalIcon GetAdditionalIcon()
	{
		return this.m_Info.m_AdditionalIcon;
	}

	public virtual bool Take()
	{
		if (this.m_Hallucination)
		{
			base.Disappear(true);
			return false;
		}
		this.ReplRequestOwnership(false);
		if (base.transform.parent != null)
		{
			DestroyIfNoChildren component = base.transform.parent.GetComponent<DestroyIfNoChildren>();
			if (component)
			{
				component.OnObjectDestroyed();
			}
		}
		if (this.m_Storage)
		{
			this.m_Storage.RemoveItem(this, false);
		}
		ItemSlot currentSlot = this.m_CurrentSlot;
		if (this.m_CurrentSlot)
		{
			this.m_CurrentSlot.RemoveItem(this, false);
		}
		if (!this.m_CurrentSlot && this.m_InventorySlot && this.m_InventorySlot.m_Items.Count > 0)
		{
			this.m_InventorySlot.RemoveItem(this, false);
		}
		if (this.m_Info.m_InventoryCellsGroup != null)
		{
			this.m_Info.m_InventoryCellsGroup.Remove(this);
		}
		this.m_PhxStaticRequests = 0;
		this.m_StaticPhx = false;
		bool result = true;
		if (this.m_Info.IsHeavyObject())
		{
			result = this.PickUp(false);
		}
		else
		{
			Vector3 position = base.transform.position;
			Quaternion rotation = base.transform.rotation;
			InsertResult insertResult = InventoryBackpack.Get().InsertItem(this, null, null, true, false, true, true, true);
			if (insertResult != InsertResult.Ok)
			{
				if (currentSlot)
				{
					currentSlot.InsertItem(this);
				}
				else if (insertResult == InsertResult.NoSpace)
				{
					base.transform.position = position;
					base.transform.rotation = rotation;
					this.m_Rigidbody.AddForce(Vector3.up * this.m_Rigidbody.mass * 5f);
				}
				return false;
			}
		}
		this.OnTake();
		return result;
	}

	public virtual bool Take3()
	{
		return false;
	}

	public virtual bool TakeAll()
	{
		return false;
	}

	public void AddItemsCountMessage(Item item)
	{
		HUDMessages hudmessages = (HUDMessages)HUDManager.Get().GetHUD(typeof(HUDMessages));
		int num = InventoryBackpack.Get().GetItemsCount(item.GetInfoID());
		if (Player.Get().GetCurrentItem(Hand.Right) && Player.Get().GetCurrentItem(Hand.Right).GetInfoID() == item.GetInfoID())
		{
			num++;
			if (Player.Get().GetCurrentItem(Hand.Right).m_Info.IsHeavyObject())
			{
				HeavyObject heavyObject = (HeavyObject)Player.Get().GetCurrentItem(Hand.Right);
				num += heavyObject.m_Attached.Count;
			}
		}
		if (Player.Get().GetCurrentItem(Hand.Left) && Player.Get().GetCurrentItem(Hand.Left).GetInfoID() == item.GetInfoID())
		{
			num++;
		}
		hudmessages.AddMessage(item.m_Info.GetNameToDisplayLocalized() + " (" + num.ToString() + ")", new Color?(Color.white), HUDMessageIcon.Item, item.GetIconName(), null);
	}

	public virtual void OnTake()
	{
		EventsManager.OnEvent(Enums.Event.TakeItem, 1, (int)this.m_Info.m_ID);
		this.AddItemsCountMessage(this);
		Player.Get().GetComponent<PlayerAudioModule>().PlayItemSound(this.m_Info.m_GrabSound);
		if (this.m_Info != null && this.m_Info.IsHeavyObject())
		{
			PlayerAudioModule.Get().PlayHOPickupSound();
		}
		if (this.m_Rigidbody && this.m_Rigidbody.IsSleeping())
		{
			this.m_Rigidbody.WakeUp();
		}
		if (!Inventory3DManager.Get().gameObject.activeSelf && this.m_CurrentSlot == InventoryBackpack.Get().m_EquippedItemSlot && !SwimController.Get().IsActive())
		{
			Player.Get().SetWantedItem(this.m_Info.IsBow() ? Hand.Left : Hand.Right, this, true);
		}
		ItemsManager.Get().OnTaken(this.m_Info.m_ID);
		base.TryRemoveFromFallenObjectsMan();
		this.m_ForceZeroLocalPos = false;
		if (this.m_Info.m_ID == ItemID.animal_droppings_item || this.m_Info.m_ID == ItemID.animal_droppings_toHold)
		{
			PlayerConditionModule.Get().GetDirtinessAdd(GetDirtyReason.TakeAnimalDroppings, null);
		}
		if (this.m_Acre)
		{
			this.m_Acre.OnTake(this);
		}
	}

	public void Swap()
	{
		if (this.m_Hallucination)
		{
			base.Disappear(true);
			return;
		}
		if (!InventoryBackpack.Get().m_EquippedItem)
		{
			return;
		}
		Item equippedItem = InventoryBackpack.Get().m_EquippedItem;
		ItemSlot currentSlot = this.m_CurrentSlot;
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			InventoryBackpack.Get().RemoveItem(InventoryBackpack.Get().m_EquippedItem, false);
			Inventory3DManager.Get().DropItem(InventoryBackpack.Get().m_EquippedItem);
		}
		else
		{
			Player.Get().DropItem(InventoryBackpack.Get().m_EquippedItem);
		}
		InventoryBackpack.Get().m_EquippedItem = null;
		if (Player.Get().m_ControllerToStart != PlayerControllerType.Unknown)
		{
			Player.Get().StartControllerInternal();
		}
		this.Take();
		if (currentSlot)
		{
			if (currentSlot.CanInsertItem(equippedItem))
			{
				currentSlot.InsertItem(equippedItem);
				return;
			}
			if (currentSlot.m_ItemParent && currentSlot.m_ItemParent.m_Info.m_ID == ItemID.Weapon_Rack)
			{
				((WeaponRack)currentSlot.m_ItemParent).InsertWeapon(equippedItem);
			}
		}
	}

	public bool PickUp(bool show_msg = true)
	{
		if (this.m_Hallucination)
		{
			base.Disappear(true);
			return false;
		}
		this.ReplRequestOwnership(false);
		this.m_StaticPhx = false;
		if (CraftingManager.Get().gameObject.activeSelf && CraftingManager.Get().ContainsItem(this))
		{
			CraftingManager.Get().RemoveItem(this, false);
		}
		if (LiquidInHandsController.Get().IsActive())
		{
			LiquidInHandsController.Get().Spill(-1f);
		}
		Item currentItem = Player.Get().GetCurrentItem();
		Item currentItem2 = Player.Get().GetCurrentItem(Hand.Right);
		bool result;
		if (this.m_Info.IsHeavyObject() && currentItem2 != null && this.m_Info.m_ID == currentItem2.m_Info.m_ID)
		{
			result = ((HeavyObject)currentItem2).AttachHeavyObject((HeavyObject)this);
		}
		else
		{
			Player.Get().SetWantedItem(Hand.Right, this, true);
			result = true;
		}
		if (currentItem && !currentItem.IsFireTool() && !currentItem.m_Info.IsHeavyObject())
		{
			Player.Get().SetWantedItem(Hand.Left, null, true);
			currentItem.m_ShownInInventory = true;
			InventoryBackpack.Get().InsertItem(currentItem, InventoryBackpack.Get().m_EquippedItemSlot, null, true, true, true, true, false);
		}
		Player.Get().UpdateHands();
		if (show_msg)
		{
			this.AddItemsCountMessage(this);
		}
		EventsManager.OnEvent(Enums.Event.TakeItem, 1, (int)this.m_Info.m_ID);
		if (this.m_CurrentSlot)
		{
			this.m_CurrentSlot.RemoveItem();
		}
		if (this.m_Info != null && this.m_Info.IsHeavyObject())
		{
			PlayerAudioModule.Get().PlayHOPickupSound();
		}
		if (this.m_Rigidbody && this.m_Rigidbody.IsSleeping())
		{
			this.m_Rigidbody.WakeUp();
		}
		return result;
	}

	public void Harvest()
	{
		bool flag = InventoryBackpack.Get().Contains(this);
		if (flag)
		{
			InventoryBackpack.Get().RemoveItem(this, true);
		}
		List<ItemID> harvestingResultItems = this.m_Info.m_HarvestingResultItems;
		for (int i = 0; i < harvestingResultItems.Count; i++)
		{
			ItemID item_id = harvestingResultItems[i];
			Item item = ItemsManager.Get().CreateItem(item_id, false, Vector3.zero, Quaternion.identity);
			item.m_WasTriggered = true;
			item.m_FirstTriggerTime = MainLevel.Instance.m_TODSky.Cycle.GameTime;
			if (InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, true) == InsertResult.Ok)
			{
				this.AddItemsCountMessage(item);
				PlayerAudioModule.Get().PlayItemSound(item.m_Info.m_GrabSound);
			}
		}
		this.ReplRequestOwnership(false);
		UnityEngine.Object.Destroy(base.gameObject);
		if (flag)
		{
			Inventory3DManager.Get().SetupPocket(Inventory3DManager.Get().m_ActivePocket);
		}
	}

	public virtual void Eat()
	{
		if (CraftingManager.Get().gameObject.activeSelf && CraftingManager.Get().ContainsItem(this))
		{
			CraftingManager.Get().RemoveItem(this, false);
		}
		if (!this.m_CurrentSlot && this.m_InventorySlot && this.m_InventorySlot.m_Items.Count > 0)
		{
			this.m_InventorySlot.RemoveItem(this, false);
			return;
		}
		if (this.m_CurrentSlot && this.m_CurrentSlot.m_InventoryStackSlot)
		{
			this.m_CurrentSlot.RemoveItem(this, false);
		}
	}

	public virtual void Drink()
	{
	}

	protected virtual void OnDrink()
	{
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (this.m_InventorySlot)
		{
			this.m_InventorySlot.gameObject.SetActive(true);
		}
		this.UpdateLayer();
		this.UpdatePhx();
		this.m_LocalScaleOnEnable = base.transform.lossyScale;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (this.m_InventorySlot)
		{
			this.m_InventorySlot.gameObject.SetActive(false);
		}
		this.m_CurrentWaters.Clear();
		if (this.m_Rigidbody)
		{
			this.m_Rigidbody.drag = this.m_DefaultDrag;
		}
		this.SetLayer(base.transform, this.m_DefaultLayer);
	}

	protected override void Update()
	{
		base.Update();
		Vector3 position = base.transform.position;
		if ((position - this.m_LastPos).sqrMagnitude > 0.05f)
		{
			ItemsManager.Get().OnObjectMoved(base.gameObject);
			this.m_LastPos = position;
		}
		if (this.m_Info != null && this.m_Info.m_Health <= 0f)
		{
			if (this.m_TimeFromDestroyToDissapear > 0f)
			{
				bool enabled = Mathf.Sin(Time.time * 20f) > 0f;
				if (this.m_ThisRenderer != null)
				{
					this.m_ThisRenderer.enabled = enabled;
				}
			}
			for (int i = 0; i < this.m_ChildrenRenderers.Length; i++)
			{
				this.m_ChildrenRenderers[i].enabled = base.enabled;
			}
			if (this.m_TrailRenderer)
			{
				this.m_TrailRenderer.enabled = false;
			}
			if (Player.Get().GetCurrentItem(Hand.Right) == this)
			{
				Player.Get().SetWantedItem(Hand.Right, null, true);
			}
			else if (Player.Get().GetCurrentItem(Hand.Left) == this)
			{
				Player.Get().SetWantedItem(Hand.Left, null, true);
			}
			if (InventoryBackpack.Get().m_EquippedItem == this)
			{
				InventoryBackpack.Get().m_EquippedItem = null;
			}
			if (Time.time > this.m_DestroyTime + this.m_TimeFromDestroyToDissapear && (this.m_Info.IsWeapon() || this.m_Info.IsArmor() || !base.gameObject.activeSelf || base.gameObject.IsInvisibleInaccurateCheck()))
			{
				for (int j = 0; j < this.m_Info.m_ItemsToBackpackOnDestroy.Count; j++)
				{
					Item item = ItemsManager.Get().CreateItem(this.m_Info.m_ItemsToBackpackOnDestroy[j], false, Vector3.zero, Quaternion.identity);
					if (item)
					{
						item.Take();
					}
				}
				if (this.ReplIsOwner())
				{
					UnityEngine.Object.Destroy(base.gameObject);
					return;
				}
			}
		}
		this.UpdateLOD();
		this.UpdateNavMeshObstacle();
		this.CheckUnderTerrain();
		if (this.m_ForceZeroLocalPos)
		{
			if (base.transform.parent)
			{
				base.transform.localPosition = Vector3.zero;
			}
			else
			{
				this.m_ForceZeroLocalPos = false;
			}
		}
		this.UpdateHealth();
	}

	private void UpdateNavMeshObstacle()
	{
		if (!this.m_NavMeshObstacle)
		{
			return;
		}
		bool flag = true;
		if (base.transform.parent)
		{
			flag = false;
		}
		else if (this.m_Thrown)
		{
			flag = false;
		}
		else if (this.m_Rigidbody && (this.m_Rigidbody.isKinematic || this.m_Rigidbody.IsSleeping()))
		{
			flag = false;
		}
		if (this.m_NavMeshObstacle.enabled != flag)
		{
			this.m_NavMeshObstacle.enabled = flag;
		}
	}

	private void CheckUnderTerrain()
	{
		if (Time.time - this.m_LastCheckUnderTerrain < 2f)
		{
			return;
		}
		if (Inventory3DManager.Get().m_CarriedItem != this && !Inventory3DManager.Get().m_StackItems.Contains(this) && !this.m_InInventory && !this.m_InStorage && !this.m_OnCraftingTable && base.transform.position.y < MainLevel.GetTerrainY(base.transform.position) - 1f)
		{
			this.m_CurrentSlot;
		}
		this.m_LastCheckUnderTerrain = Time.time;
	}

	public void StaticPhxRequestAdd()
	{
		int phxStaticRequests = this.m_PhxStaticRequests + 1;
		this.m_PhxStaticRequests = phxStaticRequests;
		this.UpdatePhx();
	}

	public void StaticPhxRequestRemove()
	{
		if (this.m_PhxStaticRequests > 0)
		{
			int phxStaticRequests = this.m_PhxStaticRequests - 1;
			this.m_PhxStaticRequests = phxStaticRequests;
			this.UpdatePhx();
		}
	}

	public void StaticPhxRequestReset()
	{
		if (this.m_PhxStaticRequests != 0)
		{
			this.m_PhxStaticRequests = 0;
			this.UpdatePhx();
		}
	}

	public virtual void UpdatePhx()
	{
		if (this.m_IsBeingDestroyed)
		{
			return;
		}
		bool flag = this.m_PhxStaticRequests > 0 || this.m_StaticPhx || this.m_AttachedToSpear;
		bool flag2 = flag;
		bool flag3 = !this.m_ForceNoKinematic && flag;
		if (!flag && base.transform.parent && this.m_DestroyableFallingObj == null)
		{
			base.transform.parent.gameObject.GetComponents<MonoBehaviour>(Item.s_ComponentsCache);
			if (Item.s_ComponentsCache.Count > 0)
			{
				flag2 = false;
				if (!this.m_ForceNoKinematic && this.m_NoKinematicIfParentIsObject != base.transform.parent.gameObject)
				{
					flag3 = true;
				}
			}
		}
		if (BodyInspectionController.Get() && BodyInspectionController.Get().m_CarryingItemDeleech == this)
		{
			flag2 = false;
			if (!this.m_ForceNoKinematic)
			{
				flag3 = true;
			}
		}
		if (this.m_BoxCollider && this.m_BoxCollider.isTrigger != flag2)
		{
			this.m_BoxCollider.isTrigger = flag2;
		}
		if (this.m_Rigidbody && this.m_Rigidbody.isKinematic != flag3)
		{
			if (flag3)
			{
				this.m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
			}
			this.m_Rigidbody.isKinematic = flag3;
		}
		if (this.m_Rigidbody && !this.m_Rigidbody.isKinematic && !this.m_Thrown)
		{
			this.m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
		}
	}

	public override void UpdateLayer()
	{
		if (this.m_Info == null)
		{
			return;
		}
		if (base.m_ForcedLayer != 0)
		{
			if (base.gameObject.layer != base.m_ForcedLayer)
			{
				this.SetLayer(base.transform, base.m_ForcedLayer);
			}
			return;
		}
		int num = this.m_DefaultLayer;
		if (TriggerController.Get() && this == TriggerController.Get().GetBestTrigger() && this.CanBeOutlined())
		{
			num = this.m_OutlineLayer;
		}
		else if (this.m_InInventory || this.m_OnCraftingTable || this.m_InStorage || (Inventory3DManager.Get() && (Inventory3DManager.Get().m_CarriedItem == this || Inventory3DManager.Get().m_StackItems.Contains(this) || Inventory3DManager.Get().m_NewCraftedItem == this)))
		{
			num = this.m_InventoryLayer;
		}
		if (base.gameObject.layer != num)
		{
			this.SetLayer(base.transform, num);
		}
	}

	private void UpdateLOD()
	{
		if (!this.m_LODGroup)
		{
			return;
		}
		int num = (this.m_InInventory || this.m_OnCraftingTable || this.m_InStorage || Inventory3DManager.Get().m_CarriedItem == this) ? 0 : -1;
		if ((float)num != this.m_ForcedLOD)
		{
			this.m_LODGroup.ForceLOD(num);
		}
	}

	public void ReseScale()
	{
		base.transform.localScale = this.m_DefaultLocalScale;
	}

	public void UpdateScale(bool force_inv_scale = false)
	{
		this.m_WantedScale = this.m_DefaultLocalScale;
		if (this.m_CurrentSlot && this.m_CurrentSlot.m_InventoryStackSlot)
		{
			this.m_WantedScale = Vector3.one;
		}
		else if (Inventory3DManager.Get().m_StackItems.Contains(this))
		{
			this.m_WantedScale = Vector3.one;
		}
		else if (force_inv_scale || ((this.m_InInventory || this.m_InStorage) && (!this.m_CurrentSlot || !this.m_CurrentSlot.m_InventoryStackSlot)) || this.m_OnCraftingTable || Inventory3DManager.Get().m_CarriedItem == this || Inventory3DManager.Get().m_StackItems.Contains(this))
		{
			this.m_WantedScale = this.m_InventoryLocalScale;
		}
		else if (this.m_Info != null && this.m_Info.IsArrow() && ((Arrow)this).m_Loaded)
		{
			this.m_WantedScale = Vector3.one;
		}
		else if (base.transform.parent)
		{
			return;
		}
		if (this.m_WantedScale != base.transform.localScale)
		{
			base.transform.localScale = this.m_WantedScale;
		}
	}

	private bool CanLoseHealth()
	{
		if (this.m_Info.m_Health <= 0f)
		{
			return false;
		}
		if (this.m_Info.m_HealthLossPerSec <= 0f)
		{
			return false;
		}
		if (this.m_Info.m_ID == ItemID.Fire)
		{
			return true;
		}
		if (this.m_InStorage || this.m_InInventory || this.m_OnCraftingTable)
		{
			return false;
		}
		if (this.m_CurrentSlot)
		{
			return false;
		}
		if (Inventory3DManager.Get().m_CarriedItem == this)
		{
			return false;
		}
		if (Inventory3DManager.Get().m_StackItems.Contains(this))
		{
			return false;
		}
		if (this.m_Info.IsArmor() && ((Armor)this).m_Limb != Limb.None)
		{
			return false;
		}
		if (this.m_Info.IsArrow() && ((Arrow)this).m_Loaded)
		{
			return false;
		}
		Item currentItem = Player.Get().GetCurrentItem();
		return (!currentItem || (!(currentItem == this) && !currentItem.m_Info.IsHeavyObject())) && (!this.m_Info.IsHeavyObject() || !currentItem || !currentItem.m_Info.IsHeavyObject() || !((HeavyObject)currentItem).m_Attached.ContainsValue((HeavyObject)this)) && (!this.m_SceneObject || this.m_WasTriggered) && (!CraftingController.Get().IsActive() || !CraftingController.Get().m_Items.Contains(this));
	}

	public virtual void UpdateHealth()
	{
		if (!this.ReplIsOwner())
		{
			return;
		}
		if (this.m_LastHealthUpdate == 0f)
		{
			this.m_LastHealthUpdate = Time.time + (float)(this.m_UniqueID % 100) * 0.01f;
			return;
		}
		if (Item.s_ItemUpdatesThisFrame > 100)
		{
			if (this.m_LastHealthUpdate > Time.time - 0.6f)
			{
				return;
			}
		}
		else if (this.m_LastHealthUpdate > Time.time - 0.2f)
		{
			return;
		}
		float num = Time.time - this.m_LastHealthUpdate;
		bool flag = false;
		if (this.CanLoseHealth())
		{
			float num2 = this.m_Info.m_HealthLossPerSec;
			if (this.GetInfoID() != ItemID.Fire && RainManager.Get().IsRain() && !RainManager.Get().IsInRainCutter(base.transform.position))
			{
				num2 *= 2f;
			}
			this.m_Info.m_Health -= num2 * num;
			if (this.m_Info.m_Health < 0f)
			{
				this.m_Info.m_Health = 0f;
				flag = true;
				if (!base.gameObject.activeSelf)
				{
					UnityEngine.Object.Destroy(base.gameObject);
					return;
				}
			}
		}
		if (this.m_Info.m_Health <= 0f)
		{
			if (Time.time < this.m_DestroyTime)
			{
				this.m_DestroyTime = Time.time;
				if (!flag)
				{
					PlayerAudioModule.Get().PlayToolDestroyedSound();
				}
			}
			Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
			if (currentItem && currentItem.Equals(this))
			{
				Player.Get().OnItemDestroyed(this);
			}
		}
		Item.s_ItemUpdatesThisFrame++;
		this.m_LastHealthUpdate = Time.time;
	}

	private void OnDestroyToCollect()
	{
		for (int i = 0; i < this.m_ObjectsToCollect.Count; i++)
		{
			if (this.m_ObjectsToCollect[i] != null)
			{
				PlantFruit component = this.m_ObjectsToCollect[i].GetComponent<PlantFruit>();
				if (component)
				{
					ItemsManager.Get().CreateItem(component.m_ItemInfo.m_ID, true, this.m_ObjectsToCollect[i].transform);
				}
				else
				{
					ItemReplacer component2 = this.m_ObjectsToCollect[i].GetComponent<ItemReplacer>();
					if (component2)
					{
						ItemsManager.Get().CreateItem(component2.m_ReplaceInfo.m_ID, true, this.m_ObjectsToCollect[i].transform);
					}
					else
					{
						Food component3 = this.m_ObjectsToCollect[i].GetComponent<Food>();
						if (component3)
						{
							component3.transform.SetParent(null);
						}
					}
				}
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (this.m_CurrentSlot)
		{
			this.m_CurrentSlot.OnDestroyItem(this);
		}
		if (this.m_InInventory)
		{
			InventoryBackpack.Get().RemoveItem(this, true);
		}
		if (this.m_Storage)
		{
			this.m_Storage.RemoveItem(this, true);
		}
		if (this.m_Acre)
		{
			this.m_Acre.OnDestroyPlant(base.gameObject);
		}
		if (HUDProcess.Get())
		{
			HUDProcess.Get().UnregisterProcess(this);
		}
		if (InventoryBackpack.Get().m_EquippedItem == this)
		{
			InventoryBackpack.Get().m_EquippedItem = null;
		}
		HUDItem.Get().OnDestroyItem(this);
		if (this.m_Group)
		{
			this.m_Group.RemoveObject(base.gameObject);
		}
		BalanceSystem20.Get().OnItemDestroyed(this);
		base.TryRemoveFromFallenObjectsMan();
		if (this.m_Info != null && !this.m_Info.m_Static)
		{
			this.ItemsManagerUnregister();
		}
		if (this.m_ConnectedParticleObj)
		{
			ParticlesManager.Get().Remove(this.m_ConnectedParticleObj);
			this.m_ConnectedParticleObj = null;
		}
		Item.s_AllItems.Remove(this);
		if (this.m_Info != null)
		{
			int num = 0;
			if (Item.s_AllItemIDs.TryGetValue((int)this.m_Info.m_ID, out num) && this.m_Initialized)
			{
				Item.s_AllItemIDs[(int)this.m_Info.m_ID] = Item.s_AllItemIDs[(int)this.m_Info.m_ID] - 1;
			}
		}
		if (Player.Get())
		{
			if (Player.Get().GetCurrentItem(Hand.Right) == this)
			{
				Player.Get().SetCurrentItem(Hand.Right, null);
			}
			if (Player.Get().GetCurrentItem(Hand.Left) == this)
			{
				Player.Get().SetCurrentItem(Hand.Left, null);
			}
			if (Player.Get().GetWantedItem(Hand.Right) == this)
			{
				Player.Get().SetWantedItem(Hand.Right, null, true);
			}
			if (Player.Get().GetWantedItem(Hand.Left) == this)
			{
				Player.Get().SetWantedItem(Hand.Left, null, true);
			}
		}
	}

	public override bool TakeDamage(DamageInfo damage_info)
	{
		if (this.m_Hallucination)
		{
			base.Disappear(true);
			return false;
		}
		if (Cheats.m_ImmortalItems)
		{
			return false;
		}
		if (this.m_DestroyableObject)
		{
			if (this.ReplIsReplicable())
			{
				this.ReplRequestOwnership(false);
			}
			this.m_DestroyableObject.TakeDamage(damage_info);
			return false;
		}
		if (this.m_Info == null || !this.m_Info.m_CanBeDamaged)
		{
			return false;
		}
		this.m_Info.m_Health -= damage_info.m_Damage;
		this.m_Info.m_Health = Mathf.Clamp(this.m_Info.m_Health, 0f, this.m_Info.m_MaxHealth);
		return true;
	}

	public virtual void OnStartSwing()
	{
	}

	public virtual void OnStopSwing()
	{
	}

	public virtual void Save(int index)
	{
		SaveGame.SaveVal("ItemUniqueID" + index, this.m_UniqueID);
		SaveGame.SaveVal("ItemName" + index, base.gameObject.name);
		SaveGame.SaveVal("ItemPos" + index, base.transform.position);
		SaveGame.SaveVal("ItemRot" + index, base.transform.rotation);
		SaveGame.SaveVal("ItemActive" + index, base.gameObject.activeSelf);
		SaveGame.SaveVal("ItemEnabled" + index, base.enabled);
		Vector3 localScale = base.transform.localScale;
		SaveGame.SaveVal("ItemScale" + index, localScale);
		SaveGame.SaveVal("ItemDefaultScale" + index, this.m_DefaultLocalScale);
		SaveGame.SaveVal("ItemPlayerHL" + index, Player.Get().GetCurrentItem(Hand.Left) == this);
		SaveGame.SaveVal("ItemPlayerHR" + index, Player.Get().GetCurrentItem(Hand.Right) == this);
		SaveGame.SaveVal("ItemShown" + index, this.m_ShownInInventory);
		SaveGame.SaveVal("ItemInvRot" + index, this.m_Info.m_InventoryRotated);
		SaveGame.SaveVal("ItemStatic" + index, this.m_PhxStaticRequests);
		SaveGame.SaveVal("ItemInv" + index, this.m_InInventory);
		SaveGame.SaveVal("ItemStorage" + index, this.m_InStorage);
		if (this.m_InInventory)
		{
			bool flag = this.m_CurrentSlot != null;
			SaveGame.SaveVal("ItemInvInSlot" + index, flag);
			bool flag2 = this.m_CurrentSlot && this.m_CurrentSlot.IsStack() && this.m_CurrentSlot.m_ItemParent;
			SaveGame.SaveVal("ItemInvInStackSlot" + index, flag2);
			bool flag3 = this.m_Info.m_InventoryCellsGroup != null;
			SaveGame.SaveVal("ItemInvInCellGr" + index, flag3);
			if (flag3)
			{
				SaveGame.SaveVal("ItemInvGrCount" + index, this.m_Info.m_InventoryCellsGroup.m_Cells.Count);
				for (int i = 0; i < this.m_Info.m_InventoryCellsGroup.m_Cells.Count; i++)
				{
					SaveGame.SaveVal("ItemInvGrCell" + index * this.m_SaveLoadIndexMul + i, this.m_Info.m_InventoryCellsGroup.m_Cells[i].m_Object.name);
				}
				SaveGame.SaveVal("ItemInvGrPos" + index, this.m_Info.m_InventoryCellsGroup.m_CenterWorld);
			}
			if (flag2)
			{
				SaveGame.SaveVal("ItemInvStackParentId" + index, this.m_CurrentSlot.m_ItemParent.m_UniqueID);
			}
			else if (flag)
			{
				SaveGame.SaveVal("ItemInvSlotName" + index, this.m_CurrentSlot.name);
			}
		}
		else if (this.m_InStorage && this.m_Storage)
		{
			if (this.m_Info.m_InventoryCellsGroup == null)
			{
				DebugUtils.Assert(string.Concat(new string[]
				{
					"Item.Save, m_InStorage == true, m_Info.m_InventoryCellsGroup == null failed to save, previously it caused crash #http://195.136.168.102/mantis/view.php?id=4683 \nitem_name ",
					base.gameObject.name,
					"ItemID ",
					this.m_Info.m_ID.ToString(),
					" position ",
					base.transform.position.ToString()
				}), true, DebugUtils.AssertType.Info);
			}
			else
			{
				bool flag4 = this.m_CurrentSlot && this.m_CurrentSlot.IsStack() && this.m_CurrentSlot.m_ItemParent;
				SaveGame.SaveVal("ItemStorageInStackSlot" + index, flag4);
				bool val = this.m_Info.m_InventoryCellsGroup != null;
				SaveGame.SaveVal("ItemStorageInCellGr" + index, val);
				SaveGame.SaveVal("ItemStorageGrCount" + index, this.m_Info.m_InventoryCellsGroup.m_Cells.Count);
				for (int j = 0; j < this.m_Info.m_InventoryCellsGroup.m_Cells.Count; j++)
				{
					SaveGame.SaveVal("ItemStorageGrCell" + index * this.m_SaveLoadIndexMul + j, this.m_Info.m_InventoryCellsGroup.m_Cells[j].m_Object.name);
				}
				SaveGame.SaveVal("ItemStoragePos" + index, this.m_Storage.transform.position);
				SaveGame.SaveVal("ItemStorageGrPos" + index, this.m_Info.m_InventoryCellsGroup.m_CenterWorld);
				if (flag4)
				{
					SaveGame.SaveVal("ItemStorageStackParentId" + index, this.m_CurrentSlot.m_ItemParent.m_UniqueID);
				}
			}
		}
		else
		{
			SaveGame.SaveVal("ItemSlot" + index, this.m_CurrentSlot != null);
			if (this.m_CurrentSlot != null)
			{
				SaveGame.SaveVal("ItemSlotName" + index, this.m_CurrentSlot.name);
				SaveGame.SaveVal("ItemSlotPos" + index, this.m_CurrentSlot.transform.position);
			}
		}
		SaveGame.SaveVal("WasTriggered" + index, this.m_WasTriggered);
		SaveGame.SaveVal("FirstTriggerTime" + index, this.m_FirstTriggerTime);
		SaveGame.SaveVal("CreationTime" + index, this.m_Info.m_CreationTime);
		SaveGame.SaveVal("ItemHealth" + index, this.m_Info.m_Health);
		SaveGame.SaveVal("EquippedItem" + index, InventoryBackpack.Get().m_EquippedItem == this);
		FishingRod component = base.gameObject.GetComponent<FishingRod>();
		if (component)
		{
			component.Save(index);
		}
		SaveGame.SaveVal("ItemStaticPhx" + index, this.m_StaticPhx);
	}

	public virtual void Load(int index)
	{
		if (SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate11)
		{
			this.m_SaveLoadIndexMul = 10000;
		}
		else
		{
			this.m_SaveLoadIndexMul = 1000;
		}
		this.m_UniqueID = SaveGame.LoadIVal("ItemUniqueID" + index);
		base.gameObject.name = SaveGame.LoadSVal("ItemName" + index);
		base.transform.rotation = SaveGame.LoadQVal("ItemRot" + index);
		Vector3 vector = SaveGame.LoadV3Val("ItemPos" + index);
		if (float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z))
		{
			ItemsManager.Get().AddItemToDestroy(this);
		}
		else
		{
			base.transform.position = vector;
		}
		if (!this.IsCharcoalFurnace() && !this.IsMudMixer())
		{
			base.gameObject.SetActive(SaveGame.LoadBVal("ItemActive" + index));
		}
		if (SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionReleaseCandidate)
		{
			base.enabled = SaveGame.LoadBVal("ItemEnabled" + index);
		}
		if (SaveGame.LoadBVal("ItemPlayerHL" + index))
		{
			Player.Get().SetWantedItem(Hand.Left, this, true);
			base.transform.localScale = this.m_InventoryLocalScale;
		}
		if (SaveGame.LoadBVal("ItemPlayerHR" + index))
		{
			Player.Get().SetWantedItem(Hand.Right, this, false);
			base.transform.localScale = this.m_InventoryLocalScale;
		}
		else if (SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionReleaseCandidate)
		{
			base.transform.localScale = SaveGame.LoadV3Val("ItemScale" + index);
			Vector3 vector2 = SaveGame.LoadV3Val("ItemDefaultScale" + index);
			if (vector2 != Vector3.zero)
			{
				this.m_DefaultLocalScale = vector2;
			}
			else
			{
				this.m_DefaultLocalScale = base.transform.localScale;
			}
		}
		this.m_ShownInInventory = SaveGame.LoadBVal("ItemShown" + index);
		this.m_Info.m_InventoryRotated = SaveGame.LoadBVal("ItemInvRot" + index);
		this.m_PhxStaticRequests = SaveGame.LoadIVal("ItemStatic" + index);
		this.m_InStorage = (SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate11 && SaveGame.LoadBVal("ItemStorage" + index));
		this.m_InInventory = SaveGame.LoadBVal("ItemInv" + index);
		if (this.m_InInventory)
		{
			this.m_PhxStaticRequests = 0;
			bool flag = SaveGame.LoadBVal("ItemInvInSlot" + index);
			bool flag2 = SaveGame.LoadBVal("ItemInvInStackSlot" + index);
			if (SaveGame.LoadBVal("ItemInvInCellGr" + index) && !flag2)
			{
				InventoryBackpack.Get().SetBackpackTransform(this.m_Info.m_BackpackPocket);
				InventoryCellsGroup inventoryCellsGroup = null;
				int num = SaveGame.LoadIVal("ItemInvGrCount" + index);
				if (num > 0)
				{
					float num2 = (float)((SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate10) ? this.m_SaveLoadIndexMul : 1);
					inventoryCellsGroup = new InventoryCellsGroup(this.m_Info.m_BackpackPocket);
					for (int i = 0; i < num; i++)
					{
						InventoryCell cellByName = InventoryBackpack.Get().GetCellByName(SaveGame.LoadSVal("ItemInvGrCell" + (float)index * num2 + i), this.m_Info.m_BackpackPocket);
						if (cellByName == null)
						{
							inventoryCellsGroup = null;
							break;
						}
						inventoryCellsGroup.m_Cells.Add(cellByName);
					}
					if (inventoryCellsGroup != null)
					{
						inventoryCellsGroup.m_CenterWorld = SaveGame.LoadV3Val("ItemInvGrPos" + index);
						inventoryCellsGroup.Setup();
					}
				}
				InsertResult insertResult = InventoryBackpack.Get().InsertItem(this, null, inventoryCellsGroup, false, true, false, true, true);
				if (insertResult != InsertResult.Ok)
				{
					insertResult = InventoryBackpack.Get().InsertItem(this, null, null, false, true, false, true, true);
					if (insertResult != InsertResult.Ok && insertResult != InsertResult.AllreadyInInventory)
					{
						this.m_InInventory = false;
					}
				}
			}
			else if (!flag2)
			{
				if (flag)
				{
					string name = SaveGame.LoadSVal("ItemInvSlotName" + index);
					ItemSlot slotByName = InventoryBackpack.Get().GetSlotByName(name, this.m_Info.m_BackpackPocket);
					InventoryBackpack.Get().InsertItem(this, slotByName, null, false, true, false, true, true);
				}
				else if (InventoryBackpack.Get().InsertItem(this, null, null, true, true, true, true, true) != InsertResult.Ok)
				{
					this.m_InInventory = false;
					DebugUtils.Assert(DebugUtils.AssertType.Info);
				}
			}
		}
		else if (this.m_InStorage)
		{
			this.m_PhxStaticRequests = 0;
		}
		this.m_WasTriggered = SaveGame.LoadBVal("WasTriggered" + index);
		this.m_FirstTriggerTime = SaveGame.LoadFVal("FirstTriggerTime" + index);
		if (SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate3)
		{
			this.m_Info.m_CreationTime = SaveGame.LoadFVal("CreationTime" + index);
		}
		this.m_Info.m_Health = SaveGame.LoadFVal("ItemHealth" + index);
		if (SaveGame.LoadBVal("EquippedItem" + index))
		{
			InventoryBackpack.Get().m_EquippedItem = this;
		}
		if (SaveGame.LoadBVal("ItemSlot" + index))
		{
			this.m_InSlotCheckPos = SaveGame.LoadV3Val("ItemSlotPos" + index);
			if (this.m_InSlotCheckPos == Vector3.zero)
			{
				this.m_InSlotCheckPos = (this.m_InventoryHolder ? this.m_InventoryHolder.transform.position : base.transform.position);
			}
		}
		if (SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate11)
		{
			this.m_StaticPhx = SaveGame.LoadBVal("ItemStaticPhx" + index);
			this.UpdatePhx();
		}
	}

	public virtual void SetupAfterLoad(int index)
	{
		if (this.m_IsBeingDestroyed)
		{
			return;
		}
		if (this.m_InInventory)
		{
			if (SaveGame.LoadBVal("ItemInvInStackSlot" + index))
			{
				this.m_PhxStaticRequests = 0;
				int num = SaveGame.LoadIVal("ItemInvStackParentId" + index);
				foreach (Item item in Item.s_AllItems)
				{
					if (item.m_UniqueID == num && item.m_InInventory)
					{
						if (item.transform.parent != null && item.transform.parent.GetComponent<ItemSlotStack>() != null)
						{
							InventoryBackpack.Get().InsertItem(this, null, null, true, true, false, true, true);
							return;
						}
						InventoryBackpack.Get().InsertItem(this, item.m_InventorySlot, null, true, true, true, true, true);
						return;
					}
				}
				if (InventoryBackpack.Get().InsertItem(this, null, null, true, true, true, true, true) != InsertResult.Ok)
				{
					this.m_InInventory = false;
					DebugUtils.Assert(DebugUtils.AssertType.Info);
				}
			}
			else
			{
				bool flag = SaveGame.LoadBVal("ItemInvInSlot" + index);
				bool flag2 = SaveGame.LoadBVal("ItemInvInCellGr" + index);
				if (!flag && !flag2)
				{
					InsertResult insertResult = InventoryBackpack.Get().InsertItem(this, null, null, true, true, true, true, true);
					if (insertResult != InsertResult.Ok && insertResult != InsertResult.AllreadyInInventory)
					{
						this.m_InInventory = false;
						DebugUtils.Assert(DebugUtils.AssertType.Info);
					}
				}
			}
		}
		else if (this.m_InStorage)
		{
			Storage storage = null;
			Vector3 vector = SaveGame.LoadV3Val("ItemStoragePos" + index);
			foreach (Storage storage2 in Storage.s_AllStorages)
			{
				if ((storage2.transform.position - vector).sqrMagnitude <= 0.01f)
				{
					storage = storage2;
					break;
				}
			}
			if (storage)
			{
				this.m_PhxStaticRequests = 0;
				bool flag3 = SaveGame.LoadBVal("ItemStorageInStackSlot" + index);
				if (SaveGame.LoadBVal("ItemStorageInCellGr" + index))
				{
					int num2 = SaveGame.LoadIVal("ItemStorageGrCount" + index);
					InventoryCellsGroup inventoryCellsGroup = new InventoryCellsGroup(BackpackPocket.Storage);
					for (int i = 0; i < num2; i++)
					{
						InventoryCell cellByName = Storage3D.Get().GetGrid(storage.m_Type).GetCellByName(SaveGame.LoadSVal("ItemStorageGrCell" + index * this.m_SaveLoadIndexMul + i));
						if (cellByName == null)
						{
							inventoryCellsGroup = null;
							break;
						}
						inventoryCellsGroup.m_Cells.Add(cellByName);
					}
					if (inventoryCellsGroup != null)
					{
						inventoryCellsGroup.m_CenterWorld = SaveGame.LoadV3Val("ItemStorageGrPos" + index);
						inventoryCellsGroup.Setup();
						this.m_Info.m_InventoryCellsGroup = inventoryCellsGroup;
					}
				}
				if (flag3)
				{
					this.m_PhxStaticRequests = 0;
					int num3 = SaveGame.LoadIVal("ItemStorageStackParentId" + index);
					foreach (Item item2 in Item.s_AllItems)
					{
						if (item2.m_UniqueID == num3)
						{
							InventoryCellsGroup inventoryCellsGroup2 = this.m_Info.m_InventoryCellsGroup;
							if (storage.InsertItem(this, item2.m_InventorySlot, null, false, false) != InsertResult.Ok && storage.InsertItem(this, null, null, false, false) != InsertResult.Ok)
							{
								Debug.Log("ERROR - missing storage!");
								this.m_InStorage = false;
								base.transform.position = vector;
								this.ItemsManagerRegister(false);
							}
							if (inventoryCellsGroup2 != null)
							{
								this.m_Info.m_InventoryCellsGroup = inventoryCellsGroup2;
							}
							return;
						}
					}
					storage.InsertItem(this, null, null, false, true);
				}
				else
				{
					storage.InsertItem(this, null, this.m_Info.m_InventoryCellsGroup, true, true);
					base.transform.position = SaveGame.LoadV3Val("ItemPos" + index);
				}
			}
			else
			{
				Debug.Log("ERROR - missing storage!");
				this.m_InStorage = false;
				base.transform.position = vector;
				this.ItemsManagerRegister(false);
			}
		}
		else if (SaveGame.LoadBVal("ItemSlot" + index) && !this.m_Info.IsArmor())
		{
			string text = SaveGame.LoadSVal("ItemSlotName" + index);
			this.m_PhxStaticRequests = 0;
			foreach (ItemSlot itemSlot in ItemSlot.s_AllItemSlots)
			{
				if (!itemSlot.IsBIWoundSlot() && !(itemSlot.name != text) && (!itemSlot.m_GOParent || !(itemSlot.m_GOParent == base.gameObject)) && (itemSlot.transform.position - this.m_InSlotCheckPos).sqrMagnitude <= 0.1f)
				{
					itemSlot.InsertItem(this);
					base.transform.localScale = SaveGame.LoadV3Val("ItemScale" + index);
					return;
				}
			}
			if (this.m_Info.m_ID == ItemID.Maggots && text.Contains("BaitSlot"))
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				DebugUtils.Assert(base.name, true, DebugUtils.AssertType.Info);
			}
		}
		FishingRod component = base.gameObject.GetComponent<FishingRod>();
		if (component)
		{
			component.SetupAfterLoad(index);
		}
	}

	public override bool CanReceiveDamageOfType(DamageType damage_type)
	{
		return this.m_Info != null && (this.m_Info.m_ReceiveDamageType & (int)damage_type) > 0;
	}

	public override string GetParticleOnHit()
	{
		if (this.m_Info == null)
		{
			return string.Empty;
		}
		return this.m_Info.m_ParticleOnHit;
	}

	private void FixedUpdate()
	{
		this.CheckThrowRequest();
		if (this.m_Thrown)
		{
			this.UpdateThrown();
		}
	}

	private void CheckThrowRequest()
	{
		if (this.m_RequestThrow)
		{
			this.Throw();
			this.m_RequestThrow = false;
		}
	}

	private void Throw()
	{
		base.transform.parent = null;
		if (this.m_Info.m_ID == ItemID.Tribe_Arrow)
		{
			base.transform.localScale = this.m_DefaultLocalScale;
		}
		Vector3 vector = Vector3.zero;
		Vector3 force = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		if (this.m_Thrower && this.m_Thrower.IsPlayer())
		{
			float proportionalClamp = CJTools.Math.GetProportionalClamp(8f, 0f, Player.Get().m_AimPower, 0f, 1f);
			float angle = UnityEngine.Random.Range(-proportionalClamp, proportionalClamp);
			Vector3 line_point = Player.Get().m_StopAimCameraMtx.GetColumn(3);
			if (this.m_Info.IsAxe())
			{
				Vector3 vector3 = Quaternion.AngleAxis(angle, Vector3.up) * Player.Get().m_StopAimCameraMtx.GetColumn(2);
				vector3 = Quaternion.AngleAxis(angle, Vector3.right) * vector3;
				base.transform.rotation = Quaternion.LookRotation(vector3, -Player.Get().m_StopAimCameraMtx.GetColumn(0));
				vector = base.transform.forward;
				vector2 = base.transform.up;
			}
			else if (this.m_Info.IsArrow())
			{
				vector = Player.Get().m_StopAimCameraMtx.GetColumn(2);
				vector2 = base.transform.forward;
			}
			else
			{
				Vector3 vector4 = Quaternion.AngleAxis(angle, Vector3.up) * -Player.Get().m_StopAimCameraMtx.GetColumn(0);
				vector4 = Quaternion.AngleAxis(angle, Vector3.right) * vector4;
				base.transform.rotation = Quaternion.LookRotation(vector4, Player.Get().m_StopAimCameraMtx.GetColumn(1));
				vector = base.transform.right;
				vector2 = base.transform.forward;
			}
			float d = 0f;
			if (this.m_Info.IsSpear())
			{
				d = ((this.m_Info.m_ID == ItemID.Bamboo_Spear) ? 1.4f : 1.3f);
			}
			Vector3 a = CJTools.Math.ProjectPointOnLine(line_point, vector, base.transform.position);
			base.transform.position = a + vector * d;
			float d2;
			if (this.m_Info.IsSpear())
			{
				d2 = Skill.Get<SpearSkill>().GetThrowForceMul();
			}
			else if (this.m_Info.IsArrow())
			{
				d2 = Skill.Get<ArcherySkill>().GetShotForceMul();
			}
			else if (this.m_Info.IsBlowpipeArrow())
			{
				d2 = Skill.Get<BlowgunSkill>().GetShotForceMul();
			}
			else
			{
				d2 = Skill.Get<ThrowingSkill>().GetThrowForceMul();
			}
			force = vector * this.m_Info.m_ThrowForce * d2;
		}
		else if (this.m_Thrower && this.m_Thrower.GetComponent<BowTrap>())
		{
			vector = base.transform.forward;
			force = vector * this.m_Info.m_ThrowForce;
			vector2 = base.transform.right;
		}
		else
		{
			vector = (Player.Get().GetHeadTransform().position - base.transform.position).normalized;
			force = vector * this.m_Info.m_ThrowForce;
			vector2 = Vector3.Cross(vector, Vector3.up);
		}
		this.m_ThrowRight = vector2;
		this.m_Rigidbody.velocity = force.normalized * 0.01f;
		this.m_Rigidbody.AddForce(force, ForceMode.Impulse);
		if (this.m_Info.m_ThrowTorque > 0f)
		{
			this.m_Rigidbody.maxAngularVelocity = 100f;
			this.m_Rigidbody.angularVelocity = Vector3.zero;
			this.m_Rigidbody.AddTorque(-vector2 * this.m_Info.m_ThrowTorque, ForceMode.VelocityChange);
		}
		this.m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		if (this.m_TrailRenderer && !this.m_TrailRenderer.enabled)
		{
			this.m_TrailRenderer.material = ItemsManager.Get().m_TrailMaterial;
			this.m_TrailRenderer.Clear();
			this.m_TrailRenderer.enabled = true;
			this.m_TrailRenderer.time = float.MaxValue;
		}
		this.m_Thrown = true;
		this.ReplSendAsap();
		if (InventoryBackpack.Get().m_EquippedItem == this)
		{
			InventoryBackpack.Get().m_EquippedItem = null;
		}
		this.UpdateThrown();
	}

	private void UpdateThrown()
	{
		this.m_ThrowVel = this.m_Rigidbody.velocity.normalized;
		ItemID id = this.m_Info.m_ID;
		if (id == ItemID.Stone_Spear || id - ItemID.Four_Pronged_Spear <= 3 || id == ItemID.Tribe_Arrow)
		{
			base.transform.rotation = Quaternion.LookRotation(-this.m_ThrowRight, Vector3.Cross(this.m_ThrowVel, this.m_ThrowRight));
		}
		Debug.DrawLine(base.transform.position, base.transform.position + this.m_ThrowRight, Color.red);
		Debug.DrawLine(base.transform.position, base.transform.position + this.m_ThrowVel, Color.blue);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!this.m_Thrown)
		{
			return;
		}
		if (collision.gameObject.IsPlayer() && this.m_Thrower && this.m_Thrower.gameObject.IsPlayer())
		{
			return;
		}
		this.OnThrowHit(collision.gameObject, collision.contacts[0].point, collision.contacts[0].normal);
	}

	protected virtual void OnTriggerExit(Collider other)
	{
		if (this.IsInWater())
		{
			WaterCollider component = other.gameObject.GetComponent<WaterCollider>();
			if (component)
			{
				this.m_CurrentWaters.Remove(component);
				if (this.m_CurrentWaters.Count == 0 && this.m_Rigidbody)
				{
					this.m_Rigidbody.drag = this.m_DefaultDrag;
				}
			}
		}
	}

	public void RestoreDrag()
	{
		if (this.m_Rigidbody)
		{
			this.m_Rigidbody.drag = this.m_DefaultDrag;
		}
	}

	protected virtual void OnTriggerEnter(Collider other)
	{
		if (this.m_Info != null && this.m_Info.IsArmor() && base.transform.parent)
		{
			return;
		}
		if (this.m_WasTriggered && other.gameObject.IsWater())
		{
			if (Player.Get().GetCurrentItem(Hand.Right) != this && Player.Get().GetCurrentItem(Hand.Left) != this)
			{
				Vector3 position = base.transform.position;
				position.y = Mathf.Max(position.y, other.bounds.max.y + 0.1f);
				if (position.Distance(Player.Get().transform.position) < 10f)
				{
					if (this.m_Info.m_Mass < 1f)
					{
						ParticlesManager.Get().Spawn("SmallSplash_Size_C_quiet", position, Quaternion.identity, Vector3.zero, null, -1f, false);
					}
					else
					{
						ParticlesManager.Get().Spawn("SmallSplash_Size_C", position, Quaternion.identity, Vector3.zero, null, -1f, false);
					}
				}
			}
		}
		else if (this.m_Thrown && other.gameObject.IsAI())
		{
			this.OnThrowHit(other.gameObject, other.gameObject.transform.position, -this.m_ThrowVel.normalized);
		}
		WaterCollider component = other.gameObject.GetComponent<WaterCollider>();
		if (component)
		{
			this.m_CurrentWaters.Add(component);
			if (this.m_Rigidbody)
			{
				this.m_Rigidbody.drag = 10f;
			}
		}
	}

	protected bool IsInWater()
	{
		return this.m_CurrentWaters.Count > 0;
	}

	private void OnThrowHit(GameObject other, Vector3 hit_point, Vector3 hit_normal)
	{
		this.m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
		if (this.m_Thrower == Player.Get().gameObject)
		{
			Physics.IgnoreCollision(base.m_Collider, base.m_Collider, false);
		}
		if (this.m_Info.IsArrow() && this.m_Thrower && this.m_Thrower.IsHumanAI() && other.IsHumanAI())
		{
			return;
		}
		AI component = other.GetComponent<AI>();
		if (component && this.m_Info.IsSpear() && (component.m_ID == AI.AIID.AngelFish || component.m_ID == AI.AIID.DiscusFish))
		{
			return;
		}
		if (other.IsPlayer())
		{
			other = Player.Get().gameObject;
		}
		this.m_Thrown = false;
		bool flag = true;
		Vector3 vector = (other == Player.Get().gameObject && this.m_Info.IsArrow()) ? UnityEngine.Random.insideUnitSphere : this.m_ThrowVel.normalized;
		RagdollBone ragdollBone = null;
		CJObject component2 = other.GetComponent<CJObject>();
		if (component2)
		{
			if (component)
			{
				if (component.m_Trap == null && this.m_Thrower && this.m_Thrower.gameObject.IsPlayer())
				{
					if (this.m_Info.IsSpear())
					{
						Skill.Get<SpearSkill>().OnSkillAction();
					}
					else if (this.m_Info.IsArrow())
					{
						Skill.Get<ArcherySkill>().OnSkillAction();
					}
					else
					{
						Skill.Get<ThrowingSkill>().OnSkillAction();
					}
				}
			}
			else
			{
				flag = false;
				if (this.m_Info.IsArrow() && component2.gameObject.IsPlayer())
				{
					foreach (Renderer renderer in General.GetComponentsDeepChild<Renderer>(base.gameObject))
					{
						if (renderer != this.m_TrailRenderer)
						{
							renderer.enabled = false;
						}
					}
					this.StaticPhxRequestAdd();
					this.m_BlockTrigger = true;
					base.Invoke("DestroyMe", 3f);
				}
			}
			Item item = component2.IsItem() ? other.GetComponent<Item>() : null;
			if (item == null || item.CanReceiveDamageOfType(this.m_Info.m_DamageType))
			{
				DamageInfo damageInfo = new DamageInfo();
				damageInfo.m_Damage = this.m_Info.m_ThrowDamage;
				if (component && component.m_RagdollBones.Count > 0)
				{
					if (this.m_Thrower)
					{
						BowTrap component3 = this.m_Thrower.GetComponent<BowTrap>();
						if (component3 && component3.m_Target.gameObject == component.gameObject)
						{
							ragdollBone = component.GetHeadRagdollBone();
							damageInfo.m_Damage *= 9999f;
						}
					}
					if (!ragdollBone)
					{
						ragdollBone = component.GetClosestRagdollBone(hit_point);
					}
				}
				if (this.m_Thrower && this.m_Thrower.IsPlayer())
				{
					damageInfo.m_Damage *= this.GetThrowDamageMultiplier(ragdollBone);
				}
				damageInfo.m_Damager = (this.m_Thrower ? this.m_Thrower.gameObject : base.gameObject);
				damageInfo.m_DamageItem = this;
				damageInfo.m_DamageType = this.GetDamageType();
				damageInfo.m_Position = hit_point;
				damageInfo.m_Normal = -vector;
				damageInfo.m_HitDir = vector;
				component2.TakeDamage(damageInfo);
				if (this.m_Rigidbody)
				{
					this.m_Rigidbody.velocity *= 0.1f;
				}
			}
		}
		RagdollBone component4 = other.GetComponent<RagdollBone>();
		if (component4)
		{
			flag = false;
			DeadBody component5 = component4.m_ParentObject.GetComponent<DeadBody>();
			if (component5)
			{
				component5.OnTakeDamage(new DamageInfo
				{
					m_DamageItem = this,
					m_Damager = base.gameObject,
					m_Position = hit_point,
					m_HitDir = vector,
					m_Normal = -vector
				});
			}
		}
		else
		{
			FlockChild component6 = other.GetComponent<FlockChild>();
			if (component6)
			{
				component6.Hit(vector);
				flag = false;
				if (this.m_Rigidbody)
				{
					this.m_Rigidbody.velocity *= 0.1f;
				}
			}
			else if (this.m_Rigidbody)
			{
				this.m_Rigidbody.velocity *= 0.4f;
				this.m_Rigidbody.angularVelocity *= 0.5f;
			}
		}
		if (flag)
		{
			this.TryStickOnHit(hit_point, vector, hit_normal, component, other, ragdollBone);
		}
		if (component && component.IsFish() && (this.m_Info.IsArrow() || this.m_Info.IsSpear()))
		{
			Fish fish = (Fish)component;
			if (fish.m_Tank)
			{
				fish.OnHitByItem(this, hit_point);
			}
		}
		if (this.m_TrailRenderer)
		{
			this.m_TrailRenderer.time = 1f;
			base.Invoke("DistableTrail", 1f);
		}
	}

	private void DistableTrail()
	{
		this.m_TrailRenderer.enabled = false;
	}

	private void DestroyMe()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private bool CanStickOnHit(AI ai, GameObject go)
	{
		ObjectMaterial component = go.GetComponent<ObjectMaterial>();
		if (component != null && component.m_ObjectMaterial == EObjectMaterial.Stone)
		{
			return false;
		}
		if (ai && ai.m_Hallucination)
		{
			return false;
		}
		if (this.m_Info.IsArrow() || this.m_Info.IsSpear())
		{
			return true;
		}
		ItemID id = this.m_Info.m_ID;
		if (id <= ItemID.Axe)
		{
			if (id == ItemID.Stone_Blade)
			{
				return true;
			}
			if (id != ItemID.Axe)
			{
				return false;
			}
		}
		else
		{
			switch (id)
			{
			case ItemID.Stone_Spear:
			case ItemID.Four_Pronged_Spear:
			case ItemID.Four_Pronged_Bamboo_Spear:
			case ItemID.Bamboo_Spear:
			case ItemID.Arrow:
			case ItemID.Machete:
			case ItemID.Bone_Knife:
				return true;
			case ItemID.Bone_Spear:
			case ItemID.Obsidian_Spear:
			case ItemID.Bow:
			case ItemID.Bamboo_Bow:
			case ItemID.Rope:
				return false;
			case ItemID.Axe_professional:
				break;
			default:
				if (id != ItemID.Tribe_Spear)
				{
					return false;
				}
				return true;
			}
		}
		return ai == null;
	}

	private float GetThrowDamageMultiplier(RagdollBone bone)
	{
		float num = 1f;
		if (this.m_Info.IsKnife())
		{
			num = Skill.Get<BladeSkill>().GetDamageMul();
		}
		else if (this.m_Info.IsSpear())
		{
			num = Skill.Get<SpearSkill>().GetDamageMul();
		}
		else if (this.m_Info.IsAxe())
		{
			num = Skill.Get<AxeSkill>().GetDamageMul();
		}
		if (bone && (this.m_Info.IsArrow() || this.m_Info.IsSpear()))
		{
			RagdollBone.BoneType boneType = bone.m_BoneType;
			if (boneType == RagdollBone.BoneType.Animal_Head || boneType == RagdollBone.BoneType.Human_Head)
			{
				num *= 9999f;
			}
		}
		return num;
	}

	private void TryStickOnHit(Vector3 hit_point, Vector3 hit_dir, Vector3 hit_normal, AI ai, GameObject other, RagdollBone bone)
	{
		if (!this.CanStickOnHit(ai, other))
		{
			return;
		}
		float min = 0.25f;
		float max = 1f;
		if (this.m_Info.IsAxe())
		{
			if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
			{
				return;
			}
			base.transform.rotation = Quaternion.LookRotation(-hit_normal, Vector3.Cross(hit_dir, Vector3.up));
			base.transform.Rotate(Vector3.up, -UnityEngine.Random.Range(0f, 20f));
			min = 0.5f;
			max = 0.7f;
		}
		else if (this.m_Info.IsMachete())
		{
			if (Vector3.Angle(hit_dir, base.transform.right) > 45f)
			{
				return;
			}
			max = 0.5f;
		}
		else if (this.m_Info.IsKnife())
		{
			if (Vector3.Angle(hit_dir, base.transform.right) > 75f)
			{
				return;
			}
			min = 0.5f;
		}
		Vector3 a = Vector3.zero;
		if (this.m_Info.IsAxe())
		{
			a = base.transform.forward;
		}
		else
		{
			a = base.transform.right;
		}
		base.transform.position = hit_point;
		base.transform.position -= a * (this.m_DamagerStart.position - base.transform.position).magnitude;
		base.transform.position += a * (this.m_DamagerStart.position - this.m_DamagerEnd.position).magnitude * UnityEngine.Random.Range(min, max);
		if (ai && (ai.m_Params.m_BigAnimal || ai.m_Params.m_Human))
		{
			if (bone)
			{
				base.transform.parent = bone.transform;
			}
			if (!base.transform.parent)
			{
				this.m_Rigidbody.velocity = Vector3.zero;
				this.m_Rigidbody.angularVelocity = Vector3.zero;
				return;
			}
			base.transform.localPosition = Vector3.zero;
			this.m_ForceZeroLocalPos = true;
			if (this.m_Info.IsWeapon())
			{
				ai.OnStickWeapon((Weapon)this);
			}
		}
		this.StaticPhxRequestAdd();
		this.m_Rigidbody.Sleep();
	}

	public virtual void OnItemAttachedToHand()
	{
		Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(base.gameObject);
		if (this.m_RenderersToRestoreShadows == null && componentsDeepChild.Length != 0)
		{
			this.m_RenderersToRestoreShadows = new List<Renderer>();
		}
		for (int i = 0; i < componentsDeepChild.Length; i++)
		{
			if (componentsDeepChild[i].shadowCastingMode == ShadowCastingMode.On)
			{
				componentsDeepChild[i].shadowCastingMode = ShadowCastingMode.Off;
				this.m_RenderersToRestoreShadows.Add(componentsDeepChild[i]);
			}
		}
		this.UpdatePhx();
		if (this.m_Rigidbody)
		{
			this.m_Rigidbody.drag = this.m_DefaultDrag;
		}
	}

	public virtual void OnItemDetachedFromHand()
	{
		if (this.m_RenderersToRestoreShadows == null)
		{
			return;
		}
		for (int i = 0; i < this.m_RenderersToRestoreShadows.Count; i++)
		{
			if (this.m_RenderersToRestoreShadows[i] != null)
			{
				this.m_RenderersToRestoreShadows[i].shadowCastingMode = ShadowCastingMode.On;
			}
		}
		this.m_RenderersToRestoreShadows = null;
		this.m_InPlayersHand = false;
		this.UpdatePhx();
	}

	public override bool PlayGrabAnimOnExecute(TriggerAction.TYPE action)
	{
		return !this.m_BlockGrabAnimOnExecute && base.PlayGrabAnimOnExecute(action);
	}

	public override bool CanExecuteActions()
	{
		return !(Inventory3DManager.Get().m_CarriedItem == this) && (this.m_InInventory || this.m_InStorage || base.CanExecuteActions());
	}

	public override bool CheckDot()
	{
		return !this.m_AttachedToSpear && base.CheckDot();
	}

	public void ApplyImpulse(Vector3 force, Vector3 torque)
	{
		Rigidbody componentDeepChild = General.GetComponentDeepChild<Rigidbody>(base.gameObject);
		if (componentDeepChild == null)
		{
			return;
		}
		componentDeepChild.AddForce(force * componentDeepChild.mass);
		componentDeepChild.AddTorque(torque * componentDeepChild.mass);
	}

	public virtual bool CanBeFocuedInInventory()
	{
		return true;
	}

	public virtual void CheckIfInBackPack()
	{
	}

	public void ApplyMaterial(Material material)
	{
		Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(base.gameObject);
		for (int i = 0; i < componentsDeepChild.Length; i++)
		{
			componentsDeepChild[i].material = material;
		}
	}

	public DamageType GetDamageType()
	{
		return this.m_Info.m_DamageType;
	}

	public virtual bool IsFireTool()
	{
		return false;
	}

	public virtual bool IsSpikes()
	{
		return false;
	}

	public virtual bool IsStorage()
	{
		return false;
	}

	public virtual bool IsMudMixer()
	{
		return false;
	}

	public virtual bool CanShowExpandMenu()
	{
		return true;
	}

	private void OnTransformParentChanged()
	{
		this.UpdatePhx();
		this.UpdateScale(false);
		this.UpdateLayer();
	}

	public override float CalculateRelevance(IPeerWorldRepresentation peer, bool is_owner)
	{
		float num = base.CalculateRelevance(peer, is_owner);
		if (num == 0f)
		{
			return num;
		}
		if (is_owner)
		{
			if (this.m_InInventory || this.m_OnCraftingTable)
			{
				if (this.m_Info.m_BackpackPocket == BackpackPocket.None)
				{
					return 1f;
				}
				if (!base.isActiveAndEnabled)
				{
					return 0.1f;
				}
				return 1f;
			}
			else if (Inventory3DManager.Get().m_CarriedItem == this)
			{
				return 1f;
			}
		}
		return -1f;
	}

	public override bool CanBeRemovedByRelevance(bool on_owner)
	{
		return (!on_owner || (!this.m_InInventory && !this.m_InStorage && !this.m_OnCraftingTable && ((this.m_Info != null && this.m_Info.m_DestroyByItemsManager) || this.m_FallenObject))) && base.CanBeRemovedByRelevance(on_owner);
	}

	public virtual void ReplOnChangedOwner(bool was_owner)
	{
		if (was_owner && (Player.Get().GetCurrentItem(Hand.Right) == this || Player.Get().GetCurrentItem(Hand.Left) == this))
		{
			Player.Get().DropItem(this);
			if (this.m_CurrentSlot)
			{
				this.m_CurrentSlot.RemoveItem(this, false);
			}
			if (this.m_InInventory)
			{
				InventoryBackpack.Get().RemoveItem(this, false);
			}
			if (this.m_Storage)
			{
				this.m_Storage.RemoveItem(this, false);
			}
			if (HUDProcess.Get())
			{
				HUDProcess.Get().UnregisterProcess(this);
			}
			if (InventoryBackpack.Get().m_EquippedItem == this)
			{
				InventoryBackpack.Get().m_EquippedItem = null;
			}
		}
	}

	public virtual void ReplOnSpawned()
	{
		this.SetupInfo();
	}

	public virtual void OnReplicationPrepare()
	{
		this.m_ReplPhxStaticRequests = this.m_PhxStaticRequests;
	}

	public virtual void OnReplicationSerialize(P2PNetworkWriter writer, bool initial_state)
	{
		if (initial_state)
		{
			writer.Write(this.m_FallenObject);
		}
		writer.Write(base.enabled);
	}

	public virtual void OnReplicationDeserialize(P2PNetworkReader reader, bool initial_state)
	{
		if (initial_state)
		{
			this.m_FallenObject = reader.ReadBoolean();
		}
		base.enabled = reader.ReadBoolean();
	}

	public virtual void OnReplicationResolve()
	{
		this.m_PhxStaticRequests = this.m_ReplPhxStaticRequests;
	}

	public virtual void ConstantUpdate()
	{
	}

	protected void RegisterConstantUpdateItem()
	{
		if (ItemsManager.Get())
		{
			ItemsManager.Get().RegisterConstantUpdateItem(this);
		}
	}

	protected void UnregisterConstantUpdateItem()
	{
		if (ItemsManager.Get())
		{
			ItemsManager.Get().UnregisterConstantUpdateItem(this);
		}
	}

	public float GetReplicationIntervalMin()
	{
		if (this.m_Thrown)
		{
			return 0.1f;
		}
		return -1f;
	}

	public float GetReplicationIntervalMax()
	{
		if (this.m_Thrown)
		{
			return 0.2f;
		}
		return -1f;
	}

	public bool CanUseSimulatedVerticalVelocity()
	{
		return !this.m_Thrown;
	}

	public virtual void OnReplicationPrepare_CJGenerated()
	{
		if (this.m_InInventory_Repl != this.m_InInventory)
		{
			this.m_InInventory_Repl = this.m_InInventory;
			this.ReplSetDirty();
		}
		if (System.Math.Abs(this.m_Info_m_Health_Repl - this.m_Info.m_Health) >= 1f)
		{
			this.m_Info_m_Health_Repl = this.m_Info.m_Health;
			this.ReplSetDirty();
		}
		if (this.m_ReplPhxStaticRequests_Repl != this.m_ReplPhxStaticRequests)
		{
			this.m_ReplPhxStaticRequests_Repl = this.m_ReplPhxStaticRequests;
			this.ReplSetDirty();
		}
		if (this.m_Thrown_Repl != this.m_Thrown)
		{
			this.m_Thrown_Repl = this.m_Thrown;
			this.ReplSetDirty();
		}
	}

	public virtual void OnReplicationSerialize_CJGenerated(P2PNetworkWriter writer, bool initial_state)
	{
		writer.Write(this.m_InInventory_Repl);
		writer.Write(this.m_Info_m_Health_Repl);
		writer.Write(this.m_ReplPhxStaticRequests_Repl);
		writer.Write(this.m_Thrown_Repl);
	}

	public virtual void OnReplicationDeserialize_CJGenerated(P2PNetworkReader reader, bool initial_state)
	{
		this.m_InInventory_Repl = reader.ReadBoolean();
		this.m_Info_m_Health_Repl = reader.ReadFloat();
		this.m_ReplPhxStaticRequests_Repl = reader.ReadInt32();
		this.m_Thrown_Repl = reader.ReadBoolean();
	}

	public virtual void OnReplicationResolve_CJGenerated()
	{
		this.m_InInventory = this.m_InInventory_Repl;
		this.m_Info.m_Health = this.m_Info_m_Health_Repl;
		this.m_ReplPhxStaticRequests = this.m_ReplPhxStaticRequests_Repl;
		this.m_Thrown = this.m_Thrown_Repl;
	}

	[Replicate(new string[]
	{
		"field:m_Health",
		"precision:1"
	})]
	public ItemInfo m_Info;

	[HideInInspector]
	public string m_InfoName = string.Empty;

	[HideInInspector]
	public bool m_Registered;

	private ItemSlot m_CurrentSlotProp;

	[HideInInspector]
	public ItemSlot m_PrevSlot;

	private float m_DestroyTime = float.MaxValue;

	protected float m_TimeFromDestroyToDissapear;

	[HideInInspector]
	public Transform m_Holder;

	[HideInInspector]
	public Rigidbody m_Rigidbody;

	[HideInInspector]
	public Storage m_Storage;

	private bool m_InStorageProp;

	private bool m_InInventoryProp;

	[HideInInspector]
	public bool m_ShownInInventory;

	private bool m_OnCraftingTableProp;

	private int m_PhxStaticRequestsProp;

	[Replicate(new string[]
	{

	})]
	private int m_ReplPhxStaticRequests;

	private DestroyableObject m_DestroyableObject;

	[HideInInspector]
	public bool m_IsTree;

	[HideInInspector]
	public bool m_IsPlant;

	[HideInInspector]
	public bool m_IsLeaf;

	public static HashSet<Item> s_AllItems = new HashSet<Item>();

	public static Dictionary<int, int> s_AllItemIDs = new Dictionary<int, int>();

	private LODGroup m_LODGroup;

	private float m_ForcedLOD = -1f;

	[HideInInspector]
	public Vector3 m_DefaultLocalScale = Vector3.one;

	[HideInInspector]
	public Vector3 m_InventoryLocalScale = Vector3.one;

	[HideInInspector]
	public Vector3 m_DefaultSize = Vector3.one;

	[HideInInspector]
	public bool m_RequestThrow;

	[HideInInspector]
	public GameObject m_Thrower;

	[HideInInspector]
	[Replicate(new string[]
	{

	})]
	public bool m_Thrown;

	[HideInInspector]
	public Vector3 m_ThrowRight = Vector3.zero;

	private Vector3 m_ThrowVel = Vector3.zero;

	[HideInInspector]
	public bool m_BlockGrabAnimOnExecute;

	[HideInInspector]
	public bool m_AttachedToSpear;

	[HideInInspector]
	public ItemSlotStack m_InventorySlot;

	[HideInInspector]
	public GameObject m_Pivot;

	public Transform m_InventoryHolder;

	public Transform m_WeaponRackHolder;

	private List<Renderer> m_RenderersToRestoreShadows;

	[HideInInspector]
	public Transform m_DamagerStart;

	[HideInInspector]
	public Transform m_DamagerEnd;

	[HideInInspector]
	[NonSerialized]
	public HumanAIGroup m_Group;

	[HideInInspector]
	public bool m_IsFallen;

	private TrailRenderer m_TrailRenderer;

	public int m_UniqueID = -1;

	public bool m_StaticPhxProp;

	[HideInInspector]
	public GameObject m_ConnectedParticleObj;

	private Renderer m_ThisRenderer;

	private Renderer[] m_ChildrenRenderers;

	private DestroyableFallingObject m_DestroyableFallingObj;

	private Vector3 m_WantedScale;

	private NavMeshObstacle m_NavMeshObstacle;

	public bool m_AttractedByItemSlot;

	[HideInInspector]
	public bool m_BlockTrigger;

	private bool m_ForceNoKinematicProp;

	private bool m_ForceZeroLocalPos;

	[NonSerialized]
	public bool m_CanSaveNotTriggered = true;

	[NonSerialized]
	public bool m_CantSave;

	[NonSerialized]
	public bool m_DestroyingOnlyScript;

	protected List<WaterCollider> m_CurrentWaters = new List<WaterCollider>();

	private FishingHook m_Hook;

	public GameVersion m_GameVersion = new GameVersion(0, 0);

	[HideInInspector]
	public bool m_ScenarioItem;

	private float m_LastCheckUnderTerrain;

	private float m_DefaultDrag;

	[HideInInspector]
	public Acre m_Acre;

	public List<GameObject> m_ObjectsToCollect = new List<GameObject>();

	private bool m_ReplEnabled;

	[HideInInspector]
	public bool m_AddAngularVelOnStart;

	[HideInInspector]
	public Vector3 m_AngularVelocityOnStart = Vector3.zero;

	[HideInInspector]
	public float m_AddAngularVelocityOnStartDuration;

	public bool m_InPlayersHand;

	[HideInInspector]
	public Vector3 m_LocalScaleOnEnable = Vector3.one;

	private Vector3 m_LastPos = Vector3.zero;

	private static List<MonoBehaviour> s_ComponentsCache = new List<MonoBehaviour>(10);

	public GameObject m_NoKinematicIfParentIsObject;

	private const float HEALTH_UPDATE_INTERVAL = 0.2f;

	private float m_LastHealthUpdate;

	public static int s_ItemUpdatesThisFrame = 0;

	private int m_SaveLoadIndexMul = 10000;

	private Vector3 m_InSlotCheckPos = Vector3.zero;

	private bool m_InInventory_Repl;

	private float m_Info_m_Health_Repl;

	private int m_ReplPhxStaticRequests_Repl;

	private bool m_Thrown_Repl;
}
