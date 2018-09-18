using System;
using System.Collections.Generic;
using AIs;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class Item : Trigger
{
	protected override void Awake()
	{
		base.Awake();
		this.m_Initialized = false;
		this.m_Registered = false;
		this.m_UniqueID = ItemsManager.s_ItemUniqueID++;
		DebugUtils.Assert(this.m_InfoName != "None", "m_InfoName of object " + base.name + " is not set!", true, DebugUtils.AssertType.Info);
		this.m_Holder = base.transform.Find("Holder");
		this.m_Rigidbody = base.gameObject.GetComponent<Rigidbody>();
		this.m_DamagerStart = base.gameObject.transform.FindDeepChild("DamageStart");
		this.m_DamagerEnd = base.gameObject.transform.FindDeepChild("DamageEnd");
		this.m_LODGroup = base.gameObject.GetComponent<LODGroup>();
		this.InitInventorySlot();
		string text = this.m_InfoName.ToString().ToLower();
		this.m_IsPlant = text.Contains("plant");
		this.m_IsTree = text.Contains("tree");
		this.m_IsLeaf = text.Contains("leaf");
		this.m_IsFallen = text.EndsWith("_fallen");
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
		this.m_DefaultLayer = ((!this.m_IsPlant && !this.m_IsTree) ? LayerMask.NameToLayer("Item") : base.gameObject.layer);
	}

	public void InitInventorySlot()
	{
		if (this.m_InventorySlot)
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
		base.Start();
		this.Initialize(true);
	}

	public void Initialize(bool im_register)
	{
		if (this.m_Initialized)
		{
			return;
		}
		this.m_Initialized = true;
		this.SetupInfo();
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
			Mesh mesh = base.gameObject.GetMesh();
			if (!mesh)
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
		this.m_InInventory = true;
		this.m_AttachedToSpear = false;
		this.StaticPhxRequestAdd();
		base.TryRemoveFromFallenObjectsMan();
		this.ItemsManagerUnregister();
		this.m_ForceNoKinematic = false;
	}

	public virtual void OnRemoveFromInventory()
	{
		base.transform.parent = null;
		this.m_InInventory = false;
		this.StaticPhxRequestRemove();
		this.ItemsManagerRegister(false);
		this.m_ShownInInventory = false;
	}

	public override bool CanTrigger()
	{
		if (this.m_Thrown || this.m_BlockTrigger || this.m_HallucinationDisappearing)
		{
			return false;
		}
		if (!Inventory3DManager.Get().isActiveAndEnabled)
		{
			bool flag = Player.Get().GetCurrentItem(Hand.Right) == this;
			bool flag2 = Player.Get().GetCurrentItem(Hand.Left) == this;
			if (flag || flag2)
			{
				return false;
			}
		}
		return base.enabled && this.m_Info != null && this.m_Info.m_Health > 0f;
	}

	public override string GetName()
	{
		return this.m_InfoName;
	}

	public override string GetTriggerInfoLocalized()
	{
		if (this.m_Info.m_LockedInfoID != string.Empty && !ItemsManager.Get().m_UnlockedItemInfos.Contains(this.m_Info.m_ID))
		{
			return GreenHellGame.Instance.GetLocalization().Get(this.m_Info.m_LockedInfoID);
		}
		return GreenHellGame.Instance.GetLocalization().Get(this.GetName());
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
		this.m_CanSave = true;
		if (action == TriggerAction.TYPE.Take || action == TriggerAction.TYPE.TakeHold)
		{
			this.Take();
		}
		else if (action == TriggerAction.TYPE.Expand)
		{
			HUDItem.Get().Activate(this);
		}
		else if (action == TriggerAction.TYPE.PickUp)
		{
			this.PickUp(true);
		}
		else if (action == TriggerAction.TYPE.SwapHold)
		{
			this.Swap();
		}
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		if (this.m_Info.m_CanBeAddedToInventory)
		{
			actions.Add(TriggerAction.TYPE.Take);
			actions.Add(TriggerAction.TYPE.Expand);
		}
		else if (!this.m_AttachedToSpear && (this.m_Info.m_Harvestable || this.m_Info.m_Eatable || this.m_Info.CanDrink() || this.m_Info.m_Craftable))
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
		if (base.transform.parent != null)
		{
			DestroyIfNoChildren component = base.transform.parent.GetComponent<DestroyIfNoChildren>();
			if (component)
			{
				component.OnObjectDestroyed();
			}
		}
		ItemSlot currentSlot = this.m_CurrentSlot;
		if (this.m_CurrentSlot)
		{
			this.m_CurrentSlot.RemoveItem();
		}
		this.m_PhxStaticRequests = 0;
		this.m_StaticPhx = false;
		if (this.m_Info.IsHeavyObject())
		{
			this.PickUp(false);
		}
		else
		{
			InventoryBackpack.InsertResult insertResult = InventoryBackpack.Get().InsertItem(this, null, null, true, false, true, true, true);
			if (insertResult != InventoryBackpack.InsertResult.Ok)
			{
				if (currentSlot)
				{
					currentSlot.InsertItem(this);
				}
				return false;
			}
		}
		this.OnTake();
		return true;
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
		hudmessages.AddMessage(item.m_Info.GetNameToDisplayLocalized() + " (" + num.ToString() + ")", new Color?(Color.white), HUDMessageIcon.Item, item.GetIconName());
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
		if (!Inventory3DManager.Get().gameObject.activeSelf && this.m_CurrentSlot == InventoryBackpack.Get().m_EquippedItemSlot)
		{
			Player.Get().SetWantedItem((!this.m_Info.IsBow()) ? Hand.Right : Hand.Left, this, true);
		}
		ItemsManager.Get().OnTaken(this.m_Info.m_ID);
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
	}

	public void PickUp(bool show_msg = true)
	{
		if (this.m_Hallucination)
		{
			base.Disappear(true);
			return;
		}
		this.m_StaticPhx = false;
		if (CraftingManager.Get().gameObject.activeSelf && CraftingManager.Get().ContainsItem(this))
		{
			CraftingManager.Get().RemoveItem(this);
		}
		if (LiquidInHandsController.Get().IsActive())
		{
			LiquidInHandsController.Get().Spill(-1f);
		}
		Item currentItem = Player.Get().GetCurrentItem();
		Item currentItem2 = Player.Get().GetCurrentItem(Hand.Right);
		if (this.m_Info.IsHeavyObject() && currentItem2 != null && this.m_Info.m_ID == currentItem2.m_Info.m_ID)
		{
			((HeavyObject)currentItem2).AttachHeavyObject((HeavyObject)this);
		}
		else
		{
			Player.Get().SetWantedItem(Hand.Right, this, true);
		}
		if (currentItem && !currentItem.IsFireTool() && !currentItem.m_Info.IsHeavyObject())
		{
			Player.Get().SetWantedItem(Hand.Left, null, true);
			currentItem.m_ShownInInventory = true;
			InventoryBackpack.Get().InsertItem(currentItem, InventoryBackpack.Get().m_EquippedItemSlot, null, true, true, true, true, false);
		}
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
	}

	public void Harvest()
	{
		bool flag = InventoryBackpack.Get().Contains(this);
		if (flag)
		{
			InventoryBackpack.Get().RemoveItem(this, false);
		}
		List<ItemID> harvestingResultItems = this.m_Info.m_HarvestingResultItems;
		for (int i = 0; i < harvestingResultItems.Count; i++)
		{
			ItemID item_id = harvestingResultItems[i];
			Item item = ItemsManager.Get().CreateItem(item_id, false, Vector3.zero, Quaternion.identity);
			item.m_Info.m_CreationTime = this.m_Info.m_CreationTime;
			InventoryBackpack.InsertResult insertResult = InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, true);
			if (insertResult == InventoryBackpack.InsertResult.Ok)
			{
				this.AddItemsCountMessage(item);
			}
		}
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
			CraftingManager.Get().RemoveItem(this);
		}
		if (!this.m_CurrentSlot && this.m_InventorySlot && this.m_InventorySlot.m_Items.Count > 0)
		{
			this.m_InventorySlot.RemoveItem(this, false);
		}
		else if (this.m_CurrentSlot && this.m_CurrentSlot.m_InventoryStackSlot)
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

	public override Vector3 GetHudInfoDisplayOffset()
	{
		return this.m_Info.m_HudInfoDisplayOffset;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (this.m_InventorySlot)
		{
			this.m_InventorySlot.gameObject.SetActive(true);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (this.m_InventorySlot)
		{
			this.m_InventorySlot.gameObject.SetActive(false);
		}
		base.SetLayer(base.transform, this.m_DefaultLayer);
	}

	protected override void Update()
	{
		base.Update();
		ItemsManager.Get().OnObjectMoved(base.gameObject);
		if (this.m_Info != null && this.m_Info.m_Health <= 0f)
		{
			bool enabled = Mathf.Sin(Time.time * 20f) > 0f;
			if (this.m_ThisRenderer != null)
			{
				this.m_ThisRenderer.enabled = enabled;
			}
			for (int i = 0; i < this.m_ChildrenRenderers.Length; i++)
			{
				this.m_ChildrenRenderers[i].enabled = enabled;
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
			if (Time.time > this.m_DestroyTime + this.m_TimeFromDestroyToDissapear)
			{
				for (int j = 0; j < this.m_Info.m_ItemsToBackpackOnDestroy.Count; j++)
				{
					Item item = ItemsManager.Get().CreateItem(this.m_Info.m_ItemsToBackpackOnDestroy[j], false, Vector3.zero, Quaternion.identity);
					if (item)
					{
						item.Take();
					}
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		this.UpdatePhx();
		this.UpdateLOD();
		this.UpdateScale(false);
		this.UpdateHealth();
		this.UpdateNavMeshObstacle();
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

	public void StaticPhxRequestAdd()
	{
		this.m_PhxStaticRequests++;
	}

	public void StaticPhxRequestRemove()
	{
		if (this.m_PhxStaticRequests > 0)
		{
			this.m_PhxStaticRequests--;
		}
	}

	public void StaticPhxRequestReset()
	{
		this.m_PhxStaticRequests = 0;
	}

	public virtual void UpdatePhx()
	{
		bool flag = this.m_PhxStaticRequests > 0 || this.m_StaticPhx;
		bool flag2 = flag;
		bool flag3 = !this.m_ForceNoKinematic && flag;
		if (!flag && base.transform.parent && this.m_DestroyableFallingObj == null)
		{
			base.transform.parent.gameObject.GetComponents<MonoBehaviour>(Item.s_ComponentsCache);
			if (Item.s_ComponentsCache.Count > 0)
			{
				flag2 = false;
				if (!this.m_ForceNoKinematic)
				{
					flag3 = true;
				}
			}
		}
		if (BodyInspectionController.Get().m_CarryingItemDeleech == this)
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
			this.m_Rigidbody.isKinematic = flag3;
		}
	}

	protected override void UpdateLayer()
	{
		if (this.m_ForcedLayer != 0)
		{
			if (base.gameObject.layer != this.m_ForcedLayer)
			{
				base.SetLayer(base.transform, this.m_ForcedLayer);
			}
			return;
		}
		if (!this.m_Info.m_CanBeFocusedInInventory)
		{
			return;
		}
		int num = this.m_DefaultLayer;
		if (this == TriggerController.Get().GetBestTrigger() && this.CanBeOutlined())
		{
			num = this.m_OutlineLayer;
		}
		else if (this.m_InInventory || this.m_OnCraftingTable || Inventory3DManager.Get().m_CarriedItem == this || Inventory3DManager.Get().m_NewCraftedItem == this)
		{
			num = this.m_InventoryLayer;
		}
		if (base.gameObject.layer != num)
		{
			base.SetLayer(base.transform, num);
		}
	}

	private void UpdateLOD()
	{
		if (!this.m_LODGroup)
		{
			return;
		}
		int num = (!this.m_InInventory && !this.m_OnCraftingTable && !(Inventory3DManager.Get().m_CarriedItem == this)) ? -1 : 0;
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
		else if (force_inv_scale || (this.m_InInventory && (!this.m_CurrentSlot || !this.m_CurrentSlot.m_InventoryStackSlot)) || this.m_OnCraftingTable || Inventory3DManager.Get().m_CarriedItem == this)
		{
			this.m_WantedScale = this.m_InventoryLocalScale;
		}
		else if (this.m_Info.IsArrow() && ((Arrow)this).m_Loaded)
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

	private void UpdateHealth()
	{
		if (this.m_Info.m_HealthLossPerSec > 0f && this.m_Info.m_Health > 0f)
		{
			this.m_Info.m_Health -= this.m_Info.m_HealthLossPerSec * Time.deltaTime;
			if (this.m_Info.m_Health < 0f)
			{
				this.m_Info.m_Health = 0f;
			}
		}
		if (this.m_Info.m_Health <= 0f)
		{
			if (Time.time < this.m_DestroyTime)
			{
				this.m_DestroyTime = Time.time;
				PlayerAudioModule.Get().PlayToolDestroyedSound();
			}
			Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
			if (currentItem && currentItem.Equals(this))
			{
				Player.Get().OnItemDestroyed(this);
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
		if (HUDProcess.Get())
		{
			HUDProcess.Get().UnregisterProcess(this);
		}
		if (this.m_Group)
		{
			this.m_Group.RemoveObject(base.gameObject);
		}
		BalanceSystem.Get().OnItemDestroyed(this);
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
		SaveGame.SaveVal("ItemPlayerHL" + index, Player.Get().GetCurrentItem(Hand.Left) == this);
		SaveGame.SaveVal("ItemPlayerHR" + index, Player.Get().GetCurrentItem(Hand.Right) == this);
		SaveGame.SaveVal("ItemShown" + index, this.m_ShownInInventory);
		SaveGame.SaveVal("ItemInvRot" + index, this.m_Info.m_InventoryRotated);
		SaveGame.SaveVal("ItemStatic" + index, this.m_PhxStaticRequests);
		SaveGame.SaveVal("ItemInv" + index, this.m_InInventory);
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
					SaveGame.SaveVal("ItemInvGrCell" + index + i, this.m_Info.m_InventoryCellsGroup.m_Cells[i].m_Object.name);
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
	}

	public virtual void Load(int index)
	{
		this.m_UniqueID = SaveGame.LoadIVal("ItemUniqueID" + index);
		base.gameObject.name = SaveGame.LoadSVal("ItemName" + index);
		base.transform.rotation = SaveGame.LoadQVal("ItemRot" + index);
		base.transform.position = SaveGame.LoadV3Val("ItemPos" + index);
		base.gameObject.SetActive(SaveGame.LoadBVal("ItemActive" + index));
		if (SaveGame.LoadBVal("ItemPlayerHL" + index))
		{
			Player.Get().SetWantedItem(Hand.Left, this, true);
		}
		if (SaveGame.LoadBVal("ItemPlayerHR" + index))
		{
			Player.Get().SetWantedItem(Hand.Right, this, true);
		}
		this.m_ShownInInventory = SaveGame.LoadBVal("ItemShown" + index);
		this.m_Info.m_InventoryRotated = SaveGame.LoadBVal("ItemInvRot" + index);
		this.m_PhxStaticRequests = SaveGame.LoadIVal("ItemStatic" + index);
		this.m_InInventory = SaveGame.LoadBVal("ItemInv" + index);
		if (this.m_InInventory)
		{
			this.m_PhxStaticRequests = 0;
			bool flag = SaveGame.LoadBVal("ItemInvInSlot" + index);
			bool flag2 = SaveGame.LoadBVal("ItemInvInStackSlot" + index);
			bool flag3 = SaveGame.LoadBVal("ItemInvInCellGr" + index);
			if (flag3 && !flag2)
			{
				InventoryCellsGroup inventoryCellsGroup = null;
				int num = SaveGame.LoadIVal("ItemInvGrCount" + index);
				if (num > 0)
				{
					inventoryCellsGroup = new InventoryCellsGroup();
					for (int i = 0; i < num; i++)
					{
						inventoryCellsGroup.m_Cells.Add(InventoryBackpack.Get().GetCellByName(SaveGame.LoadSVal("ItemInvGrCell" + index + i), this.m_Info.m_BackpackPocket));
					}
					inventoryCellsGroup.m_CenterWorld = SaveGame.LoadV3Val("ItemInvGrPos" + index);
				}
				InventoryBackpack.Get().InsertItem(this, null, inventoryCellsGroup, false, true, true, true, true);
			}
			else if (!flag2)
			{
				if (flag)
				{
					string name = SaveGame.LoadSVal("ItemInvSlotName" + index);
					ItemSlot slotByName = InventoryBackpack.Get().GetSlotByName(name, this.m_Info.m_BackpackPocket);
					InventoryBackpack.Get().InsertItem(this, slotByName, null, false, true, true, true, true);
				}
			}
		}
		this.m_WasTriggered = SaveGame.LoadBVal("WasTriggered" + index);
		this.m_FirstTriggerTime = SaveGame.LoadFVal("FirstTriggerTime" + index);
		if (GreenHellGame.s_GameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate3)
		{
			this.m_Info.m_CreationTime = SaveGame.LoadFVal("CreationTime" + index);
		}
		this.m_Info.m_Health = SaveGame.LoadFVal("ItemHealth" + index);
		if (SaveGame.LoadBVal("EquippedItem" + index))
		{
			InventoryBackpack.Get().m_EquippedItem = this;
		}
		bool flag4 = SaveGame.LoadBVal("ItemSlot" + index);
		if (flag4)
		{
			this.m_InSlotCheckPos = SaveGame.LoadV3Val("ItemSlotPos" + index);
			if (this.m_InSlotCheckPos == Vector3.zero)
			{
				this.m_InSlotCheckPos = ((!this.m_InventoryHolder) ? base.transform.position : this.m_InventoryHolder.transform.position);
			}
		}
	}

	public virtual void SetupAfterLoad(int index)
	{
		if (this.m_InInventory)
		{
			bool flag = SaveGame.LoadBVal("ItemInvInStackSlot" + index);
			if (flag)
			{
				this.m_PhxStaticRequests = 0;
				int num = SaveGame.LoadIVal("ItemInvStackParentId" + index);
				for (int i = 0; i < Item.s_AllItems.Count; i++)
				{
					if (Item.s_AllItems[i].m_UniqueID == num)
					{
						InventoryBackpack.Get().InsertItem(this, Item.s_AllItems[i].m_InventorySlot, null, true, true, true, true, true);
						return;
					}
				}
				DebugUtils.Assert(DebugUtils.AssertType.Info);
			}
		}
		else
		{
			bool flag2 = SaveGame.LoadBVal("ItemSlot" + index);
			if (flag2)
			{
				string b = SaveGame.LoadSVal("ItemSlotName" + index);
				this.m_PhxStaticRequests = 0;
				foreach (ItemSlot itemSlot in ItemSlot.s_AllItemSlots)
				{
					if (!itemSlot.IsBIWoundSlot())
					{
						if (!(itemSlot.name != b))
						{
							if (!itemSlot.m_GOParent || !(itemSlot.m_GOParent == base.gameObject))
							{
								if ((itemSlot.transform.position - this.m_InSlotCheckPos).sqrMagnitude <= 0.01f)
								{
									itemSlot.InsertItem(this);
									return;
								}
							}
						}
					}
				}
				DebugUtils.Assert(base.name, true, DebugUtils.AssertType.Info);
			}
		}
	}

	public override bool CanReceiveDamageOfType(DamageType damage_type)
	{
		return this.m_Info != null && (this.m_Info.m_ReceiveDamageType & (int)damage_type) > 0;
	}

	public override string GetParticleOnHit()
	{
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
		Vector3 vector = Vector3.zero;
		Vector3 force = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		if (this.m_Thrower.IsPlayer())
		{
			float proportionalClamp = CJTools.Math.GetProportionalClamp(8f, 0f, Player.Get().m_AimPower, 0f, 1f);
			float angle = UnityEngine.Random.Range(-proportionalClamp, proportionalClamp);
			if (this.m_Info.IsAxe())
			{
				Vector3 vector3 = Quaternion.AngleAxis(angle, Vector3.up) * Player.Get().m_StopAimCameraMtx.GetColumn(2);
				vector3 = Quaternion.AngleAxis(angle, Vector3.right) * vector3;
				base.transform.rotation = Quaternion.LookRotation(vector3, -Player.Get().m_StopAimCameraMtx.GetColumn(0));
				vector = base.transform.forward;
				vector2 = base.transform.up;
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
				d = ((this.m_Info.m_ID != ItemID.Bamboo_Spear) ? 1.3f : 1.4f);
			}
			Vector3 a = CJTools.Math.ProjectPointOnLine(Player.Get().m_StopAimCameraMtx.GetColumn(3), vector, base.transform.position);
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
		else
		{
			vector = (Player.Get().GetHeadTransform().position - base.transform.position).normalized;
			force = vector * this.m_Info.m_ThrowForce;
			vector2 = base.transform.forward;
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
		this.m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
		if (this.m_TrailRenderer && !this.m_TrailRenderer.enabled)
		{
			this.m_TrailRenderer.material = ItemsManager.Get().m_TrailMaterial;
			this.m_TrailRenderer.Clear();
			this.m_TrailRenderer.enabled = true;
			this.m_TrailRenderer.time = float.MaxValue;
		}
		this.m_Thrown = true;
		if (InventoryBackpack.Get().m_EquippedItem == this)
		{
			InventoryBackpack.Get().m_EquippedItem = null;
		}
	}

	private void UpdateThrown()
	{
		this.m_ThrowVel = this.m_Rigidbody.velocity.normalized;
		switch (this.m_Info.m_ID)
		{
		case ItemID.Stone_Spear:
		case ItemID.Four_Pronged_Spear:
		case ItemID.Four_Pronged_Bamboo_Spear:
		case ItemID.Bamboo_Spear:
		case ItemID.Arrow:
			base.transform.rotation = Quaternion.LookRotation(-this.m_ThrowRight, Vector3.Cross(this.m_ThrowVel, this.m_ThrowRight));
			break;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!this.m_Thrown)
		{
			return;
		}
		if (collision.gameObject == Player.Get().gameObject && this.m_Thrower && this.m_Thrower.gameObject == Player.Get().gameObject)
		{
			return;
		}
		this.OnThrowHit(collision.gameObject, collision.contacts[0].point, collision.contacts[0].normal);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (this.m_WasTriggered && other.gameObject.IsWater())
		{
			if (Player.Get().GetCurrentItem(Hand.Right) != this && Player.Get().GetCurrentItem(Hand.Left) != this)
			{
				Vector3 position = base.transform.position;
				position.y = Mathf.Max(position.y, other.bounds.max.y + 0.1f);
				if (position.Distance(Player.Get().transform.position) < 10f)
				{
					ParticlesManager.Get().Spawn("SmallSplash_Size_C", position, Quaternion.identity, null);
				}
			}
		}
		else if (this.m_Thrown && other.gameObject.IsAI())
		{
			this.OnThrowHit(other.gameObject, other.gameObject.transform.position, -this.m_ThrowVel.normalized);
		}
	}

	private void OnThrowHit(GameObject other, Vector3 hit_point, Vector3 hit_normal)
	{
		this.m_Thrown = false;
		this.m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
		bool flag = true;
		Vector3 normalized = this.m_ThrowVel.normalized;
		AI ai = null;
		CJObject component = other.GetComponent<CJObject>();
		if (component)
		{
			if (component.IsAI())
			{
				if (this.m_Thrower && this.m_Thrower.gameObject == Player.Get().gameObject)
				{
					ai = component.GetComponent<AI>();
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
				if (this.m_Info.IsArrow() && component.gameObject == Player.Get().gameObject)
				{
					List<Renderer> componentsDeepChild = General.GetComponentsDeepChild<Renderer>(base.gameObject);
					foreach (Renderer renderer in componentsDeepChild)
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
			Item item = (!component.IsItem()) ? null : other.GetComponent<Item>();
			if (item == null || item.CanReceiveDamageOfType(this.m_Info.m_DamageType))
			{
				DamageInfo damageInfo = new DamageInfo();
				damageInfo.m_Damage = ((!this.m_Thrower.IsPlayer()) ? 10f : this.m_Info.m_ThrowDamage);
				damageInfo.m_Damage *= this.GetThrowDamageMultiplier();
				if (ai && !ai.IsHuman() && this.m_Info.IsSpear())
				{
					damageInfo.m_Damage = 9999f;
				}
				damageInfo.m_Damager = ((!this.m_Thrower) ? base.gameObject : this.m_Thrower.gameObject);
				damageInfo.m_DamageItem = this;
				damageInfo.m_DamageType = this.GetDamageType();
				damageInfo.m_Position = hit_point;
				damageInfo.m_Normal = -normalized;
				component.TakeDamage(damageInfo);
				if (this.m_Rigidbody)
				{
					this.m_Rigidbody.velocity *= 0.1f;
				}
			}
		}
		RagdollBone component2 = other.GetComponent<RagdollBone>();
		if (component2)
		{
			flag = false;
			DeadBody component3 = component2.m_Parent.GetComponent<DeadBody>();
			if (component3)
			{
				component3.OnTakeDamage(new DamageInfo
				{
					m_DamageItem = this,
					m_Damager = base.gameObject,
					m_Position = hit_point,
					m_HitDir = normalized,
					m_Normal = -normalized
				});
			}
		}
		else
		{
			FlockChild component4 = other.GetComponent<FlockChild>();
			if (component4)
			{
				component4.Hit(normalized);
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
			this.TryStickOnHit(hit_point, normalized, ai, other);
		}
		if (ai && ai.IsFish() && (this.m_Info.m_ID == ItemID.Arrow || this.m_Info.IsSpear()))
		{
			Fish fish = (Fish)ai;
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
		if (this.m_Info.IsSpear() && ai && ai.m_HealthModule && !ai.m_HealthModule.m_IsDead)
		{
			return false;
		}
		if (this.m_Info.m_Type == ItemType.Arrow || this.m_Info.m_Type == ItemType.BlowpipeArrow || this.m_Info.m_Type == ItemType.Spear)
		{
			return true;
		}
		ItemID id = this.m_Info.m_ID;
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
		default:
			switch (id)
			{
			case ItemID.Stone_Blade:
				return true;
			default:
				if (id != ItemID.Tribe_Spear)
				{
					return false;
				}
				return true;
			case ItemID.Axe:
				break;
			}
			break;
		case ItemID.Axe_professional:
			break;
		}
		return ai == null;
	}

	private float GetThrowDamageMultiplier()
	{
		if (this.IsKnife())
		{
			return Skill.Get<BladeSkill>().GetDamageMul();
		}
		if (this.IsSpear())
		{
			return Skill.Get<SpearSkill>().GetDamageMul();
		}
		if (this.IsAxe())
		{
			return Skill.Get<AxeSkill>().GetDamageMul();
		}
		if (this.IsMachete())
		{
			return Skill.Get<MacheteSkill>().GetDamageMul();
		}
		return 1f;
	}

	private void TryStickOnHit(Vector3 hit_point, Vector3 hit_dir, AI ai, GameObject other)
	{
		if (!this.CanStickOnHit(ai, other))
		{
			return;
		}
		float min = 0.25f;
		float max = 1f;
		ItemID id = this.m_Info.m_ID;
		switch (id)
		{
		case ItemID.Axe_professional:
			break;
		case ItemID.Machete:
			if (Vector3.Angle(hit_dir, base.transform.right) > 45f)
			{
				return;
			}
			max = 0.5f;
			goto IL_11D;
		default:
			switch (id)
			{
			case ItemID.Stone_Blade:
				goto IL_F6;
			case ItemID.Obsidian_Blade:
			case ItemID.Stone_Axe:
				goto IL_11D;
			case ItemID.Axe:
				break;
			default:
				goto IL_11D;
			}
			break;
		case ItemID.Bone_Knife:
			goto IL_F6;
		}
		float num = 0.3f;
		if (-hit_dir.y < -num || -hit_dir.y > num)
		{
			return;
		}
		base.transform.rotation = Quaternion.LookRotation(hit_dir, Vector3.Cross(hit_dir, Vector3.up));
		base.transform.Rotate(Vector3.up, -UnityEngine.Random.Range(0f, 20f));
		max = 0.5f;
		goto IL_11D;
		IL_F6:
		if (Vector3.Angle(hit_dir, base.transform.right) > 75f)
		{
			return;
		}
		min = 0.5f;
		IL_11D:
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
			base.transform.parent = ai.GetClosestRagdollBone(hit_point);
			if (!base.transform.parent)
			{
				this.m_Rigidbody.velocity = Vector3.zero;
				this.m_Rigidbody.angularVelocity = Vector3.zero;
				return;
			}
			base.transform.localPosition = Vector3.zero;
			this.m_ForceZeroLocalPos = true;
		}
		this.StaticPhxRequestAdd();
		this.m_Rigidbody.Sleep();
	}

	public virtual void OnItemAttachedToHand()
	{
		List<Renderer> componentsDeepChild = General.GetComponentsDeepChild<Renderer>(base.gameObject);
		if (this.m_RenderersToRestoreShadows == null && componentsDeepChild.Count > 0)
		{
			this.m_RenderersToRestoreShadows = new List<Renderer>();
		}
		for (int i = 0; i < componentsDeepChild.Count; i++)
		{
			if (componentsDeepChild[i].shadowCastingMode == ShadowCastingMode.On)
			{
				componentsDeepChild[i].shadowCastingMode = ShadowCastingMode.Off;
				this.m_RenderersToRestoreShadows.Add(componentsDeepChild[i]);
			}
		}
		this.UpdatePhx();
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
		this.UpdatePhx();
	}

	public override bool PlayGrabAnimOnExecute(TriggerAction.TYPE action)
	{
		return !this.m_BlockGrabAnimOnExecute && base.PlayGrabAnimOnExecute(action);
	}

	public override bool CanExecuteActions()
	{
		return !(Inventory3DManager.Get().m_CarriedItem == this) && (this.m_InInventory || base.CanExecuteActions());
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
		List<Renderer> componentsDeepChild = General.GetComponentsDeepChild<Renderer>(base.gameObject);
		for (int i = 0; i < componentsDeepChild.Count; i++)
		{
			componentsDeepChild[i].material = material;
		}
	}

	public bool IsKnife()
	{
		return this.m_Info.m_ID == ItemID.Obsidian_Blade || this.m_Info.m_ID == ItemID.Obsidian_Bone_Blade || this.m_Info.m_ID == ItemID.Stick_Blade || this.m_Info.m_ID == ItemID.Stone_Blade || this.m_Info.m_ID == ItemID.Bone_Knife;
	}

	public bool IsSpear()
	{
		return this.m_Info.m_ID == ItemID.Bamboo_Spear || this.m_Info.m_ID == ItemID.Four_Pronged_Bamboo_Spear || this.m_Info.m_ID == ItemID.Four_Pronged_Spear || this.m_Info.m_ID == ItemID.Obsidian_Spear || this.m_Info.m_ID == ItemID.Stone_Spear || this.m_Info.m_ID == ItemID.Tribe_Spear;
	}

	public bool IsAxe()
	{
		return this.m_Info.m_ID == ItemID.Axe || this.m_Info.m_ID == ItemID.Axe_professional || this.m_Info.m_ID == ItemID.Obsidian_Axe || this.m_Info.m_ID == ItemID.Rusted_Axe || this.m_Info.m_ID == ItemID.Tribe_Axe;
	}

	public bool IsMachete()
	{
		return this.m_Info.m_ID == ItemID.Machete || this.m_Info.m_ID == ItemID.Rusted_Machete;
	}

	public DamageType GetDamageType()
	{
		return this.m_Info.m_DamageType;
	}

	public virtual bool IsFireTool()
	{
		return false;
	}

	public ItemInfo m_Info;

	[HideInInspector]
	public string m_InfoName = string.Empty;

	[HideInInspector]
	public bool m_Registered;

	[HideInInspector]
	public ItemSlot m_CurrentSlot;

	[HideInInspector]
	public ItemSlot m_PrevSlot;

	private float m_DestroyTime = float.MaxValue;

	protected float m_TimeFromDestroyToDissapear;

	[HideInInspector]
	public Transform m_Holder;

	[HideInInspector]
	public Rigidbody m_Rigidbody;

	[HideInInspector]
	public bool m_InInventory;

	[HideInInspector]
	public bool m_ShownInInventory;

	[HideInInspector]
	public bool m_OnCraftingTable;

	protected int m_PhxStaticRequests;

	private DestroyableObject m_DestroyableObject;

	[HideInInspector]
	public bool m_IsTree;

	[HideInInspector]
	public bool m_IsPlant;

	[HideInInspector]
	public bool m_IsLeaf;

	public static List<Item> s_AllItems = new List<Item>();

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
	public Being m_Thrower;

	[HideInInspector]
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
	public bool m_Initialized;

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

	public bool m_StaticPhx;

	[HideInInspector]
	public GameObject m_ConnectedParticleObj;

	private Renderer m_ThisRenderer;

	private Renderer[] m_ChildrenRenderers;

	private DestroyableFallingObject m_DestroyableFallingObj;

	private Vector3 m_WantedScale;

	private NavMeshObstacle m_NavMeshObstacle;

	public bool m_AttractedByItemSlot;

	private bool m_BlockTrigger;

	public bool m_ForceNoKinematic;

	private bool m_ForceZeroLocalPos;

	[NonSerialized]
	public bool m_CanSave = true;

	[NonSerialized]
	public bool m_DestroyingOnlyScript;

	private static List<MonoBehaviour> s_ComponentsCache = new List<MonoBehaviour>(10);

	private Vector3 m_InSlotCheckPos = Vector3.zero;
}
