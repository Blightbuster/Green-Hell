using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDItem : HUDBase, IInputsReceiver
{
	public static HUDItem Get()
	{
		return HUDItem.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDItem.s_Instance = this;
		this.m_Icon = base.gameObject.transform.FindDeepChild("Icon").gameObject.GetComponent<Image>();
		this.m_AdditionalIcon = base.gameObject.transform.FindDeepChild("AdditionalIcon").gameObject.GetComponent<Image>();
		for (int i = 0; i < 999; i++)
		{
			Transform transform = base.transform.FindDeepChild("Button" + i);
			if (!transform)
			{
				break;
			}
			HUDItemButton huditemButton = new HUDItemButton();
			huditemButton.button = transform.gameObject;
			huditemButton.trans = transform.gameObject.GetComponent<RectTransform>();
			huditemButton.text = transform.FindDeepChild("Text").GetComponent<Text>();
			huditemButton.confirm = transform.FindDeepChild("Confirm").GetComponent<Text>();
			huditemButton.confirm.gameObject.SetActive(false);
			huditemButton.confirm_sel = huditemButton.trans.FindDeepChild("ConfirmSelection").gameObject;
			huditemButton.confirm_sel.gameObject.SetActive(false);
			huditemButton.confirm_trans = huditemButton.confirm.gameObject.GetComponent<RectTransform>();
			huditemButton.big_trans = huditemButton.confirm.gameObject.transform.GetChild(0).GetComponent<RectTransform>();
			this.m_Buttons.Add(huditemButton);
		}
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override bool ShouldShow()
	{
		return !TriggerController.Get().IsGrabInProgress() && !Player.Get().m_Animator.GetBool(TriggerController.Get().m_BDrinkWater) && !Player.Get().m_Animator.GetBool(Player.Get().m_CleanUpHash) && !Player.Get().IsDead() && this.m_Active;
	}

	private void ResetItems()
	{
		this.m_Item = null;
		this.m_LiquidSource = null;
		this.m_PlantFruit = null;
		this.m_ItemReplacer = null;
		this.m_DestroyButton = null;
		this.m_DestroyStackButton = null;
	}

	private void Activate()
	{
		this.m_PadSelectedIndex = 0;
		this.m_ActiveButton = null;
		this.UpdateSelectionBG();
		this.SetActive(true);
		this.m_ExecutionInProgress = false;
		this.m_DelayDeactivateRequested = false;
	}

	private void ClearSlots()
	{
		for (int i = 0; i < this.m_Buttons.Count; i++)
		{
			this.m_Buttons[i].button.gameObject.SetActive(false);
		}
		this.m_ActiveButtons.Clear();
	}

	public bool Activate(PlantFruit fruit)
	{
		if (this.m_Active)
		{
			return false;
		}
		this.ResetItems();
		this.m_PlantFruit = fruit;
		this.ClearSlots();
		this.AddSlot(HUDItem.Action.Take);
		if (fruit.m_Eatable)
		{
			this.AddSlot(HUDItem.Action.Eat);
		}
		this.Activate();
		return true;
	}

	public bool Activate(LiquidSource source)
	{
		if (this.m_Active)
		{
			return false;
		}
		this.ResetItems();
		this.m_LiquidSource = source;
		this.ClearSlots();
		this.AddSlot(HUDItem.Action.Drink);
		this.AddSlot(HUDItem.Action.CleanUp);
		if (!ScenarioManager.Get().IsDreamOrPreDream())
		{
			this.AddSlot(HUDItem.Action.TakeClay);
		}
		this.Activate();
		return true;
	}

	public bool Activate(ItemReplacer item)
	{
		if (this.m_Active)
		{
			return false;
		}
		this.ResetItems();
		this.m_ItemReplacer = item;
		this.ClearSlots();
		bool flag = Player.Get().m_SwimController.IsActive();
		if (item.m_ReplaceInfo.m_Craftable && Player.Get().CanStartCrafting())
		{
			this.AddSlot(HUDItem.Action.Craft);
		}
		if (item.m_ReplaceInfo.m_CanBeAddedToInventory)
		{
			this.AddSlot(HUDItem.Action.Take);
		}
		if (item.m_ReplaceInfo.IsHeavyObject() && !item.m_ReplaceInfo.m_CanBeAddedToInventory && !MakeFireController.Get().IsActive())
		{
			this.AddSlot(HUDItem.Action.PickUp);
		}
		if (item.m_ReplaceInfo.m_Eatable && !flag)
		{
			this.AddSlot(HUDItem.Action.Eat);
		}
		if (item.m_ReplaceInfo.CanDrink() && !flag)
		{
			this.AddSlot(HUDItem.Action.Drink);
		}
		if (item.m_ReplaceInfo.m_Harvestable && !flag)
		{
			this.AddSlot(HUDItem.Action.Harvest);
		}
		this.AddSlot(HUDItem.Action.Destroy);
		this.Activate();
		return true;
	}

	public bool Activate(Item item)
	{
		if (this.m_Active)
		{
			return false;
		}
		if (!item || !item.CanExecuteActions())
		{
			return false;
		}
		this.ResetItems();
		this.m_Item = item;
		this.ClearSlots();
		if (GreenHellGame.IsPadControllerActive() && Inventory3DManager.Get().IsActive() && Inventory3DManager.Get().CanSetCarriedItem(true))
		{
			this.AddSlot(HUDItem.Action.Pick);
			this.AddSlot(HUDItem.Action.PickStack);
		}
		if (this.m_Item.m_Info.IsArmor() && ((Armor)this.m_Item).m_Limb != Limb.None)
		{
			this.AddSlot(HUDItem.Action.TakeOffArmor);
			this.Activate();
			return true;
		}
		bool flag = Player.Get().m_SwimController.IsActive();
		if (this.m_Item.m_Info.m_Craftable && !this.m_Item.m_OnCraftingTable && Player.Get().CanStartCrafting())
		{
			this.AddSlot(HUDItem.Action.Craft);
		}
		if (this.m_Item.IsStorage())
		{
			this.AddSlot(HUDItem.Action.Use);
		}
		else if (item.IsAcre())
		{
			this.AddSlot(HUDItem.Action.Plow);
		}
		else if (this.m_Item.m_Info.IsHeavyObject() && !this.m_Item.m_Info.m_CanBeAddedToInventory)
		{
			if (!MakeFireController.Get().IsActive())
			{
				this.AddSlot(HUDItem.Action.PickUp);
			}
		}
		else if (this.m_Item.m_Info.m_CanEquip)
		{
			if (this.m_Item.IsFireTool())
			{
				if (!InventoryBackpack.Get().Contains(this.m_Item))
				{
					this.AddSlot(HUDItem.Action.Take);
				}
				if (!MakeFireController.Get().IsActive() && !Player.Get().IsInWater())
				{
					this.AddSlot(HUDItem.Action.Use);
				}
			}
			else if (this.m_Item.m_InInventory || this.m_Item.m_InStorage)
			{
				if (item != InventoryBackpack.Get().m_EquippedItem)
				{
					this.AddSlot(HUDItem.Action.Equip);
				}
			}
			else if (InventoryBackpack.Get().FindFreeSlot(this.m_Item))
			{
				this.AddSlot(HUDItem.Action.Take);
			}
			else
			{
				this.AddSlot(HUDItem.Action.Swap);
			}
		}
		else if (this.m_Item.m_Info.m_CanBeAddedToInventory && !InventoryBackpack.Get().Contains(this.m_Item))
		{
			this.AddSlot(HUDItem.Action.Take);
		}
		if (MakeFireController.Get().IsActive() && MakeFireController.Get().CanUseItemAsKindling(item))
		{
			this.AddSlot(HUDItem.Action.Use);
		}
		if (this.m_Item.m_Info.m_Eatable && !flag)
		{
			this.AddSlot(HUDItem.Action.Eat);
		}
		if (this.m_Item.m_Info.CanDrink() && !flag)
		{
			this.AddSlot(HUDItem.Action.Drink);
		}
		if (this.m_Item.m_Info.IsLiquidContainer() && ((LiquidContainerInfo)this.m_Item.m_Info).CanSpill())
		{
			this.AddSlot(HUDItem.Action.Spill);
		}
		if (this.m_Item.m_Info.m_Harvestable && !flag && (!this.m_Item.RequiresToolToHarvest() || Player.Get().HasBlade()))
		{
			this.AddSlot(HUDItem.Action.Harvest);
		}
		if (this.m_Item.m_Info.IsArmor() && BodyInspectionController.Get().IsActive())
		{
			Limb selectedLimb = HUDBodyInspection.Get().GetSelectedLimb();
			if (PlayerArmorModule.Get().IsArmorActive(selectedLimb))
			{
				this.AddSlot(HUDItem.Action.SwapArmor);
			}
			else
			{
				this.AddSlot(HUDItem.Action.EquipArmor);
			}
		}
		if (HUDItemSlot.Get().m_SelectedSlotData != null && HUDItemSlot.Get().m_SelectedSlotData.slot.CanInsertItem(this.m_Item))
		{
			this.AddSlot(HUDItem.Action.Insert);
		}
		if (GreenHellGame.IsPadControllerActive() && (item.m_InInventory || item.m_InStorage))
		{
			this.AddSlot(HUDItem.Action.Drop);
		}
		if (this.m_Item.m_Info.IsStand())
		{
			Stand stand = (Stand)this.m_Item;
			if (stand.GetNumitems() > 0)
			{
				this.AddSlot(HUDItem.Action.Take);
			}
			if (stand.GetNumitems() >= 3)
			{
				this.AddSlot(HUDItem.Action.Take3);
			}
			if (stand.GetNumitems() >= 2)
			{
				this.AddSlot(HUDItem.Action.TakeAll);
			}
		}
		else if (!item.m_Info.m_CantDestroy && !item.m_AttachedToSpear && item.m_Info.m_ID != ItemID.Chellange_Battery && item.m_Info.m_ID != ItemID.Chellange_WalkieTalkie && !item.IsStorage() && !item.IsAcre())
		{
			this.AddSlot(HUDItem.Action.Destroy);
			if ((item.m_InventorySlot && item.m_InventorySlot.m_Items.Count > 0) || (item.m_CurrentSlot && item.m_CurrentSlot.IsStack()))
			{
				this.AddSlot(HUDItem.Action.DestroyStack);
			}
		}
		this.Activate();
		return true;
	}

	protected override void OnShow()
	{
		base.OnShow();
		if (!this.m_HUDTriggerAttach)
		{
			this.m_HUDTriggerAttach = HUDTrigger.GetNormal().transform.Find("Group/Action_0/KeyFrame0");
		}
		Player.Get().BlockRotation();
		Player.Get().BlockMoves();
		this.m_PadHideCursorPos = Vector2.zero;
		if (GreenHellGame.IsPCControllerActive())
		{
			CursorManager.Get().ShowCursor(CursorManager.TYPE.Normal);
			this.m_CursorVisible = true;
		}
		else if (Inventory3DManager.Get().IsActive() && CursorManager.Get().IsCursorVisible())
		{
			this.m_PadHideCursorPos = Input.mousePosition;
			CursorManager.Get().ShowCursor(false, false);
			this.m_CursorVisible = false;
		}
		Vector3 zero = Vector3.zero;
		if (!Inventory3DManager.Get().gameObject.activeSelf)
		{
			zero.x = (float)Screen.width * this.m_CursorOffset.x;
			zero.y = (float)Screen.height * this.m_CursorOffset.y;
			CursorManager.Get().SetCursorPos(this.m_HUDTriggerAttach.position + zero);
		}
		zero.x = (float)Screen.width * this.m_Offset.x;
		zero.y = (float)Screen.height * this.m_Offset.y;
		base.transform.position = this.m_HUDTriggerAttach.position + zero;
		this.ShowElements();
		float a = float.MaxValue;
		float a2 = float.MaxValue;
		float a3 = float.MinValue;
		float a4 = float.MinValue;
		foreach (HUDItemButton huditemButton in this.m_ActiveButtons)
		{
			Vector3[] array = new Vector3[4];
			huditemButton.trans.GetWorldCorners(array);
			Vector2[] array2 = new Vector2[4];
			for (int i = 0; i < 4; i++)
			{
				array2[i] = RectTransformUtility.WorldToScreenPoint(base.transform.GetComponentInParent<Canvas>().worldCamera, array[i]);
				a = Mathf.Min(a, array2[i].x);
				a3 = Mathf.Max(a3, array2[i].x);
				a2 = Mathf.Min(a2, array2[i].y);
				a4 = Mathf.Max(a4, array2[i].y);
			}
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		Player.Get().UnblockRotation();
		Player.Get().UnblockMoves();
		if (this.m_CursorVisible)
		{
			CursorManager.Get().ShowCursor(false, false);
			CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
			this.m_CursorVisible = false;
		}
		else if (GreenHellGame.IsPadControllerActive() && Inventory3DManager.Get().IsActive() && this.m_PadHideCursorPos != Vector2.zero)
		{
			CursorManager.Get().ShowCursor(this.m_PadHideCursorPos);
		}
		foreach (HUDItemButton huditemButton in this.m_ActiveButtons)
		{
			huditemButton.confirm_sel.gameObject.SetActive(false);
			huditemButton.confirm.gameObject.SetActive(false);
		}
		this.HideElements();
	}

	private void AddSlot(HUDItem.Action action)
	{
		Localization localization = GreenHellGame.Instance.GetLocalization();
		HUDItemButton huditemButton = this.m_Buttons[this.m_ActiveButtons.Count];
		huditemButton.action = action;
		switch (action)
		{
		case HUDItem.Action.None:
			huditemButton.text.text = localization.Get("None", true);
			break;
		case HUDItem.Action.Close:
			huditemButton.text.text = localization.Get("HUD_Trigger_Close", true);
			break;
		case HUDItem.Action.Take:
			huditemButton.text.text = localization.Get("HUD_Trigger_Take", true);
			break;
		case HUDItem.Action.PickUp:
			huditemButton.text.text = localization.Get("HUD_Trigger_PickUp", true);
			break;
		case HUDItem.Action.Eat:
			huditemButton.text.text = localization.Get("HUD_Trigger_Eat", true);
			break;
		case HUDItem.Action.Drink:
			huditemButton.text.text = localization.Get("HUD_Trigger_Drink", true);
			break;
		case HUDItem.Action.Harvest:
			huditemButton.text.text = localization.Get("HUD_Trigger_Harvest", true);
			break;
		case HUDItem.Action.Craft:
			huditemButton.text.text = localization.Get("Craft", true);
			break;
		case HUDItem.Action.Fill:
			huditemButton.text.text = localization.Get("HUD_Trigger_Fill", true);
			break;
		case HUDItem.Action.Equip:
			huditemButton.text.text = localization.Get("HUD_Trigger_Equip", true);
			break;
		case HUDItem.Action.Drop:
			huditemButton.text.text = localization.Get("HUD_ItemInHand_Drop", true);
			break;
		case HUDItem.Action.Swap:
			huditemButton.text.text = localization.Get("HUD_Trigger_SwapHold", true);
			break;
		case HUDItem.Action.Use:
			huditemButton.text.text = localization.Get("HUD_Trigger_Use", true);
			break;
		case HUDItem.Action.Spill:
			huditemButton.text.text = localization.Get("HUD_Trigger_Spill", true);
			break;
		case HUDItem.Action.Destroy:
			huditemButton.text.text = localization.Get("HUD_Trigger_Destroy", true);
			break;
		case HUDItem.Action.Take3:
			huditemButton.text.text = localization.Get("HUD_Trigger_Take3", true);
			break;
		case HUDItem.Action.TakeAll:
			huditemButton.text.text = localization.Get("HUD_Trigger_TakeAll", true);
			break;
		case HUDItem.Action.TakeClay:
			huditemButton.text.text = localization.Get("HUD_Trigger_TakeClay", true);
			break;
		case HUDItem.Action.CleanUp:
			huditemButton.text.text = localization.Get("HUD_Trigger_CleanUp", true);
			break;
		case HUDItem.Action.DestroyStack:
			huditemButton.text.text = localization.Get("HUD_Trigger_DestroyStack", true);
			break;
		case HUDItem.Action.SwapArmor:
			huditemButton.text.text = localization.Get("HUD_Trigger_SwapArmor", true);
			break;
		case HUDItem.Action.EquipArmor:
			huditemButton.text.text = localization.Get("HUD_Trigger_EquipArmor", true);
			break;
		case HUDItem.Action.Insert:
			huditemButton.text.text = localization.Get("HUD_Trigger_Insert", true);
			break;
		case HUDItem.Action.Pick:
			huditemButton.text.text = localization.Get("HUD_Trigger_Pick", true);
			break;
		case HUDItem.Action.TakeOffArmor:
			huditemButton.text.text = localization.Get("HUD_Trigger_TakeOffArmor", true);
			break;
		case HUDItem.Action.Plow:
			huditemButton.text.text = localization.Get("HUD_Trigger_Plow", true);
			break;
		case HUDItem.Action.PickStack:
			huditemButton.text.text = localization.Get("HUD_Trigger_PickStack", true);
			break;
		}
		huditemButton.button.SetActive(true);
		this.m_ActiveButtons.Add(huditemButton);
		if (action == HUDItem.Action.Destroy)
		{
			this.m_DestroyButton = huditemButton;
			return;
		}
		if (action == HUDItem.Action.DestroyStack)
		{
			this.m_DestroyStackButton = huditemButton;
		}
	}

	private void RemoveSlot(HUDItem.Action action)
	{
		this.m_TempButtons.Clear();
		foreach (HUDItemButton item in this.m_ActiveButtons)
		{
			this.m_TempButtons.Add(item);
		}
		this.ClearSlots();
		foreach (HUDItemButton huditemButton in this.m_TempButtons)
		{
			if (huditemButton.action != action)
			{
				this.AddSlot(huditemButton.action);
			}
		}
	}

	public void OnChangeSelectedSlot(SlotData data)
	{
		if (!this.m_Active)
		{
			return;
		}
		if (data == null)
		{
			this.RemoveSlot(HUDItem.Action.Insert);
			this.ShowElements();
		}
	}

	public void Execute()
	{
		if (this.m_ExecutionInProgress)
		{
			return;
		}
		if (this.m_ActiveButton == null)
		{
			this.Deactivate();
			return;
		}
		if (!Inventory3DManager.Get().gameObject.activeSelf && this.m_ActiveButton.action != HUDItem.Action.Harvest && this.m_ActiveButton.action != HUDItem.Action.Destroy && this.m_ActiveButton.action != HUDItem.Action.DestroyStack)
		{
			this.m_ActionToExecute = this.m_ActiveButton.action;
			switch (this.m_ActionToExecute)
			{
			case HUDItem.Action.Take:
			case HUDItem.Action.PickUp:
			case HUDItem.Action.Eat:
			case HUDItem.Action.Drink:
			case HUDItem.Action.Harvest:
			case HUDItem.Action.Craft:
			case HUDItem.Action.Use:
			case HUDItem.Action.Take3:
			case HUDItem.Action.TakeAll:
			case HUDItem.Action.TakeClay:
			case HUDItem.Action.CleanUp:
			case HUDItem.Action.Pick:
			case HUDItem.Action.TakeOffArmor:
			case HUDItem.Action.Plow:
			case HUDItem.Action.PickStack:
				if (WalkieTalkieController.Get().IsActive())
				{
					WalkieTalkieController.Get().Stop();
				}
				break;
			}
			Player.Get().BlockMoves();
			Player.Get().BlockRotation();
			if (this.m_ActionToExecute == HUDItem.Action.Drink && this.m_LiquidSource)
			{
				Player.Get().HideWeapon();
				Player.Get().m_Animator.SetBool(TriggerController.Get().m_BDrinkWater, true);
			}
			else if (this.m_ActionToExecute == HUDItem.Action.CleanUp)
			{
				Player.Get().HideWeapon();
				Player.Get().m_Animator.SetBool(Player.Get().m_CleanUpHash, true);
				PlayerArmorModule.Get().SetMeshesVisible(false);
				BodyInspectionController.Get().OnArmorMeshesDisabled();
				Player.Get().OnStartWashinghands();
			}
			else
			{
				Item currentItem = Player.Get().GetCurrentItem(Hand.Left);
				if (currentItem != null && currentItem.m_Info.m_ID == ItemID.Bow)
				{
					Player.Get().m_Animator.SetBool(TriggerController.s_BGrabItemBow, true);
				}
				else if (currentItem != null && currentItem.m_Info.m_ID == ItemID.Bamboo_Bow)
				{
					Player.Get().m_Animator.SetBool(TriggerController.s_BGrabItemBambooBow, true);
				}
				else if (currentItem && currentItem.m_Info.IsBow())
				{
					Player.Get().m_Animator.SetBool(TriggerController.s_BGrabItemBow, true);
				}
				else
				{
					Player.Get().m_Animator.SetBool(TriggerController.s_BGrabItem, true);
				}
			}
			this.m_ExecutionInProgress = true;
			return;
		}
		if (!this.ExecuteAction(this.m_ActiveButton.action, this.m_Item))
		{
			return;
		}
		base.Invoke("DelayDeactivate", this.m_DelayDeactivate);
		this.m_DelayDeactivateRequested = true;
		this.m_ElemGroup.SetActive(false);
	}

	private void SetActive(bool set)
	{
		if (this.m_Active == set)
		{
			return;
		}
		this.m_Active = set;
		if (!this.m_Active)
		{
			FPPController.Get().OnDeactivateHUDItem();
		}
	}

	private void DelayDeactivate()
	{
		this.SetActive(false);
	}

	public void Deactivate()
	{
		this.SetActive(false);
	}

	public void DelayedExecute()
	{
		this.ExecuteAction(this.m_ActionToExecute, this.m_Item);
		Player.Get().UnblockMoves();
		Player.Get().UnblockRotation();
		Player.Get().m_Animator.SetBool(TriggerController.Get().m_BDrinkWater, false);
		Player.Get().m_Animator.SetBool(TriggerController.s_BGrabItemBambooBow, false);
		Player.Get().m_Animator.SetBool(TriggerController.s_BGrabItemBow, false);
		Player.Get().m_Animator.SetBool(TriggerController.s_BGrabItem, false);
		PlayerArmorModule.Get().SetMeshesVisible(true);
		BodyInspectionController.Get().OnArmorMeshesEnabled();
		Player.Get().m_Animator.SetBool(Player.Get().m_CleanUpHash, false);
		this.SetActive(false);
		this.m_ExecutionInProgress = false;
	}

	private bool ExecuteAction(HUDItem.Action action, Item item)
	{
		if (action != HUDItem.Action.Close && this.m_ItemReplacer != null)
		{
			item = this.m_ItemReplacer.ReplaceItem();
			this.m_ItemReplacer = null;
		}
		switch (action)
		{
		case HUDItem.Action.Close:
			this.SetActive(false);
			break;
		case HUDItem.Action.Take:
			if (item)
			{
				item.Take();
			}
			else if (this.m_PlantFruit)
			{
				this.m_PlantFruit.Take();
			}
			break;
		case HUDItem.Action.PickUp:
			item.PickUp(true);
			break;
		case HUDItem.Action.Eat:
			if (item)
			{
				item.Eat();
			}
			else if (this.m_PlantFruit)
			{
				this.m_PlantFruit.Eat();
			}
			break;
		case HUDItem.Action.Drink:
			if (item)
			{
				item.Drink();
			}
			else if (this.m_LiquidSource)
			{
				this.m_LiquidSource.Drink();
			}
			break;
		case HUDItem.Action.Harvest:
			Player.Get().HideWeapon();
			HarvestingSmallAnimalController.Get().SetItem(item);
			Player.Get().StartController(PlayerControllerType.HarvestingSmallAnimal);
			break;
		case HUDItem.Action.Craft:
			if (Player.Get().CanStartCrafting())
			{
				if (WalkieTalkieController.Get().IsActive())
				{
					WalkieTalkieController.Get().Stop();
				}
				if (Player.Get().GetCurrentItem(Hand.Left) == item)
				{
					Player.Get().SetWantedItem(Hand.Left, null, true);
				}
				else if (Player.Get().GetCurrentItem(Hand.Right) == item)
				{
					Player.Get().SetWantedItem(Hand.Right, null, true);
				}
				else if (!item.m_CurrentSlot && item.m_InventorySlot && item.m_InventorySlot.m_Items.Count > 0)
				{
					item.m_InventorySlot.RemoveItem(item, false);
				}
				else if (item.m_CurrentSlot && item.m_CurrentSlot.m_InventoryStackSlot)
				{
					item.m_CurrentSlot.RemoveItem(item, false);
				}
				else if (item.m_CurrentSlot && item.m_CurrentSlot.m_WeaponRackParent)
				{
					item.m_CurrentSlot.RemoveItem(item, false);
				}
				if (InventoryBackpack.Get().m_EquippedItem == item)
				{
					InventoryBackpack.Get().m_EquippedItem = null;
				}
				InventoryBackpack.Get().RemoveItem(item, false);
				Storage3D.Get().RemoveItem(item, false);
				CraftingManager.Get().Activate();
				CraftingManager.Get().AddItem(item, true);
			}
			break;
		case HUDItem.Action.Fill:
			this.m_LiquidSource.TakeLiquid();
			break;
		case HUDItem.Action.Equip:
			Player.Get().Equip(item.m_CurrentSlot);
			break;
		case HUDItem.Action.Drop:
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
			if (InventoryBackpack.Get().m_EquippedItem == item)
			{
				InventoryBackpack.Get().m_EquippedItem = null;
			}
			InventoryBackpack.Get().RemoveItem(item, false);
			Inventory3DManager.Get().DropItem(item);
			break;
		case HUDItem.Action.Swap:
			item.Swap();
			break;
		case HUDItem.Action.Use:
			if (this.m_Item.IsStorage())
			{
				Storage3D.Get().Activate((Storage)this.m_Item);
			}
			else if (MakeFireController.Get().IsActive())
			{
				InventoryBackpack.Get().RemoveItem(this.m_Item, false);
				MakeFireController.Get().InsertItemToKindlingSlot(this.m_Item);
			}
			else
			{
				Item currentItem = Player.Get().GetCurrentItem();
				if (currentItem)
				{
					InventoryBackpack.Get().InsertItem(currentItem, InventoryBackpack.Get().m_EquippedItemSlot, null, true, true, true, true, false);
					Player.Get().SetWantedItem(currentItem.m_Info.IsBow() ? Hand.Left : Hand.Right, null, true);
					if (Player.Get().m_ControllerToStart != PlayerControllerType.Unknown)
					{
						Player.Get().StartControllerInternal();
					}
				}
				Player.Get().SetWantedItem(this.m_Item, true);
			}
			break;
		case HUDItem.Action.Spill:
			((LiquidContainer)this.m_Item).Spill(-1f);
			break;
		case HUDItem.Action.Destroy:
			if (GreenHellGame.IsPadControllerActive() && !this.m_DestroyButton.confirm.gameObject.activeSelf)
			{
				this.m_DestroyButton.confirm.gameObject.SetActive(true);
				this.m_DestroyButton.confirm_sel.gameObject.SetActive(true);
				return false;
			}
			if (!this.m_DestroyButton.confirm_sel.gameObject.activeSelf)
			{
				return true;
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
			if (InventoryBackpack.Get().m_EquippedItem == item)
			{
				InventoryBackpack.Get().m_EquippedItem = null;
			}
			InventoryBackpack.Get().RemoveItem(item, false);
			UnityEngine.Object.Destroy(item.gameObject);
			break;
		case HUDItem.Action.Take3:
			if (item)
			{
				item.Take3();
			}
			break;
		case HUDItem.Action.TakeAll:
			if (item)
			{
				item.TakeAll();
			}
			break;
		case HUDItem.Action.TakeClay:
			if (this.m_LiquidSource)
			{
				this.m_LiquidSource.TakeClay();
			}
			break;
		case HUDItem.Action.CleanUp:
			PlayerArmorModule.Get().SetMeshesVisible(true);
			BodyInspectionController.Get().OnArmorMeshesEnabled();
			Player.Get().m_Animator.SetBool(Player.Get().m_CleanUpHash, false);
			break;
		case HUDItem.Action.DestroyStack:
			if (GreenHellGame.IsPadControllerActive() && !this.m_DestroyStackButton.confirm.gameObject.activeSelf)
			{
				this.m_DestroyStackButton.confirm.gameObject.SetActive(true);
				this.m_DestroyStackButton.confirm_sel.gameObject.SetActive(true);
				return false;
			}
			if (!this.m_DestroyStackButton.confirm_sel.gameObject.activeSelf)
			{
				return true;
			}
			if (!item.m_CurrentSlot && item.m_InventorySlot && item.m_InventorySlot.m_Items.Count > 0)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			else if (item.m_CurrentSlot && item.m_CurrentSlot.m_InventoryStackSlot)
			{
				UnityEngine.Object.Destroy(item.m_CurrentSlot.m_ItemParent.gameObject);
			}
			else if (item.m_CurrentSlot && item.m_CurrentSlot.m_BackpackSlot && item.m_CurrentSlot.IsStack())
			{
				for (int i = 0; i < ((ItemSlotStack)item.m_CurrentSlot).m_Items.Count; i++)
				{
					UnityEngine.Object.Destroy(((ItemSlotStack)item.m_CurrentSlot).m_Items[i].gameObject);
				}
			}
			break;
		case HUDItem.Action.SwapArmor:
		{
			Limb selectedLimb = HUDBodyInspection.Get().GetSelectedLimb();
			ArmorData armorData = PlayerArmorModule.Get().GetArmorData(selectedLimb);
			Item attachedArmor = armorData.m_AttachedArmor;
			ItemSlot currentSlot = attachedArmor.m_CurrentSlot;
			if (currentSlot != null)
			{
				currentSlot.RemoveItem();
			}
			PlayerArmorModule.Get().ArmorCarryStarted(attachedArmor);
			InventoryCellsGroup inventoryCellsGroup = this.m_Item.m_Info.m_InventoryCellsGroup;
			InventoryBackpack.Get().RemoveItem(this.m_Item, false);
			if (attachedArmor.m_Info.m_InventoryRotated != this.m_Item.m_Info.m_InventoryRotated)
			{
				Inventory3DManager.Get().RotateItem(attachedArmor, false);
			}
			InventoryBackpack.Get().InsertItem(attachedArmor, null, inventoryCellsGroup, true, true, true, true, true);
			armorData.m_Slot.InsertItem(this.m_Item);
			break;
		}
		case HUDItem.Action.EquipArmor:
			InventoryBackpack.Get().RemoveItem(this.m_Item, false);
			PlayerArmorModule.Get().GetArmorData(HUDBodyInspection.Get().GetSelectedLimb()).m_Slot.InsertItem(this.m_Item);
			break;
		case HUDItem.Action.Insert:
			if (HUDItemSlot.Get().m_SelectedSlotData == null)
			{
				return false;
			}
			if (!HUDItemSlot.Get().m_SelectedSlotData.slot.CanInsertItem(this.m_Item))
			{
				((HUDMessages)HUDManager.Get().GetHUD(typeof(HUDMessages))).AddMessage(GreenHellGame.Instance.GetLocalization().Get("HUD_CannotInsertItem", true), new Color?(Color.red), HUDMessageIcon.None, "", null);
				return true;
			}
			if (this.m_Item.m_Storage)
			{
				this.m_Item.m_Storage.RemoveItem(this.m_Item, false);
			}
			InventoryBackpack.Get().RemoveItem(this.m_Item, false);
			HUDItemSlot.Get().m_SelectedSlotData.slot.InsertItem(this.m_Item);
			break;
		case HUDItem.Action.Pick:
			Inventory3DManager.Get().StartCarryItem(this.m_Item, false);
			break;
		case HUDItem.Action.TakeOffArmor:
		{
			Item attachedArmor2 = PlayerArmorModule.Get().GetArmorData(HUDBodyInspection.Get().GetSelectedLimb()).m_AttachedArmor;
			if (attachedArmor2)
			{
				PlayerArmorModule.Get().ArmorCarryStarted(attachedArmor2);
				InventoryBackpack.Get().InsertItem(attachedArmor2, null, null, true, true, true, true, true);
			}
			break;
		}
		case HUDItem.Action.Plow:
		{
			Acre acre = (this.m_Item && this.m_Item.IsAcre()) ? ((Acre)this.m_Item) : null;
			if (!acre)
			{
				return true;
			}
			acre.Plow();
			break;
		}
		case HUDItem.Action.PickStack:
			Inventory3DManager.Get().StartCarryItem(this.m_Item, true);
			break;
		}
		return true;
	}

	public override void ConstantUpdate()
	{
		base.ConstantUpdate();
		this.UpdateActivity();
		if (this.m_Active)
		{
			this.UpdateSelection();
		}
		if (this.m_ExecutionInProgress)
		{
			this.UpdateGrabAnim();
		}
	}

	private void UpdateActivity()
	{
		if (this.m_Active && !this.m_DelayDeactivateRequested)
		{
			if (this.ShouldExecute())
			{
				this.Execute();
				return;
			}
			if (this.ShouldCancel())
			{
				this.Deactivate();
				return;
			}
			if (HUDWheel.Get().enabled)
			{
				this.SetActive(false);
			}
		}
	}

	private bool ShouldExecute()
	{
		return !GreenHellGame.IsPadControllerActive() && (Input.GetMouseButtonDown(0) || !InputsManager.Get().IsActionActive(TriggerAction.TYPE.Expand));
	}

	private bool ShouldCancel()
	{
		return !GreenHellGame.IsPadControllerActive() && !InputsManager.Get().IsActionActive(TriggerAction.TYPE.Expand);
	}

	private void ShowElements()
	{
		this.m_ElemGroup.SetActive(true);
		this.SetupIcon(this.m_Item ? this.m_Item.GetIconName() : string.Empty, this.m_Item ? this.m_Item.GetAdditionalIcon() : ItemAdditionalIcon.None);
		if (this.m_ActiveButtons.Count < 3)
		{
			Vector3 localScale = this.m_SeparatorV.rectTransform.localScale;
			localScale.y = 0.66f;
			this.m_SeparatorV.rectTransform.localScale = localScale;
			return;
		}
		Vector3 localScale2 = this.m_SeparatorV.rectTransform.localScale;
		localScale2.y = 1f;
		this.m_SeparatorV.rectTransform.localScale = localScale2;
	}

	private void SetupIcon(string icon_name, ItemAdditionalIcon additional_icon_type = ItemAdditionalIcon.None)
	{
		this.m_Icon.gameObject.SetActive(false);
		this.m_AdditionalIcon.gameObject.SetActive(false);
	}

	private void HideElements()
	{
		this.m_ElemGroup.SetActive(false);
	}

	public void UpdateSelection()
	{
		this.m_ActiveButton = null;
		if (GreenHellGame.IsPCControllerActive())
		{
			foreach (HUDItemButton huditemButton in this.m_ActiveButtons)
			{
				if (RectTransformUtility.RectangleContainsScreenPoint(huditemButton.trans, Input.mousePosition))
				{
					this.m_ActiveButton = huditemButton;
					break;
				}
			}
			if (this.m_DestroyButton != null && this.m_DestroyButton.confirm.gameObject.activeSelf)
			{
				if (RectTransformUtility.RectangleContainsScreenPoint(this.m_DestroyButton.big_trans, Input.mousePosition))
				{
					this.m_ActiveButton = this.m_DestroyButton;
					if (RectTransformUtility.RectangleContainsScreenPoint(this.m_DestroyButton.confirm_trans, Input.mousePosition))
					{
						this.m_DestroyButton.confirm_sel.gameObject.SetActive(true);
					}
					else
					{
						this.m_DestroyButton.confirm_sel.gameObject.SetActive(false);
					}
				}
				else
				{
					this.m_DestroyButton.confirm_sel.gameObject.SetActive(false);
				}
			}
			else if (this.m_DestroyStackButton != null && this.m_DestroyStackButton.confirm.gameObject.activeSelf)
			{
				if (RectTransformUtility.RectangleContainsScreenPoint(this.m_DestroyStackButton.big_trans, Input.mousePosition))
				{
					this.m_ActiveButton = this.m_DestroyStackButton;
					if (RectTransformUtility.RectangleContainsScreenPoint(this.m_DestroyStackButton.confirm_trans, Input.mousePosition))
					{
						this.m_DestroyStackButton.confirm_sel.gameObject.SetActive(true);
					}
					else
					{
						this.m_DestroyStackButton.confirm_sel.gameObject.SetActive(false);
					}
				}
				else
				{
					this.m_DestroyStackButton.confirm_sel.gameObject.SetActive(false);
				}
			}
		}
		else
		{
			this.m_ActiveButton = this.m_ActiveButtons[this.m_PadSelectedIndex];
			if (this.m_DestroyButton != null && this.m_ActiveButton != this.m_DestroyButton && this.m_DestroyButton.confirm.gameObject.activeSelf)
			{
				this.m_DestroyButton.confirm.gameObject.SetActive(false);
				this.m_DestroyButton.confirm_sel.gameObject.SetActive(false);
			}
			else if (this.m_DestroyStackButton != null && this.m_ActiveButton != this.m_DestroyStackButton && this.m_DestroyStackButton.confirm.gameObject.activeSelf)
			{
				this.m_DestroyStackButton.confirm.gameObject.SetActive(false);
				this.m_DestroyStackButton.confirm_sel.gameObject.SetActive(false);
			}
		}
		CursorManager.Get().SetCursor((this.m_ActiveButton != null) ? CursorManager.TYPE.MouseOver : CursorManager.TYPE.Normal);
		this.UpdateSelectionBG();
	}

	private void UpdateSelectionBG()
	{
		bool flag = false;
		if (this.m_ActiveButton != null)
		{
			this.m_SelectionBG.rectTransform.position = this.m_ActiveButton.trans.position + Vector3.right * 5f;
			flag = true;
			if (GreenHellGame.IsPadControllerActive() && this.m_DestroyButton != null && this.m_DestroyButton.confirm.gameObject.activeSelf)
			{
				flag = false;
			}
			if (GreenHellGame.IsPadControllerActive() && this.m_DestroyStackButton != null && this.m_DestroyStackButton.confirm.gameObject.activeSelf)
			{
				flag = false;
			}
		}
		this.m_SelectionBG.gameObject.SetActive(flag);
		this.m_PadIconSelect.SetActive(GreenHellGame.IsPadControllerActive() && (flag || (this.m_DestroyButton != null && this.m_DestroyButton.confirm_sel.activeSelf)));
		if (this.m_PadIconSelect.activeSelf)
		{
			float x = ((RectTransform)HUDManager.Get().m_CanvasGameObject.transform).localScale.x;
			if (flag)
			{
				Vector3 position = this.m_SelectionBG.transform.position;
				position.x += this.m_SelectionBG.rectTransform.rect.width * 0.5f * x + this.m_PadIconSelect.GetComponent<Image>().rectTransform.rect.width * 0.3f * x;
				this.m_PadIconSelect.transform.position = position;
			}
			else
			{
				Vector3 position2 = this.m_DestroyButton.confirm_sel.transform.position;
				position2.x += this.m_DestroyButton.confirm_sel.GetComponent<Image>().rectTransform.rect.width * 0.5f * x + this.m_PadIconSelect.GetComponent<Image>().rectTransform.rect.width * 0.3f * x;
				this.m_PadIconSelect.transform.position = position2;
			}
		}
		if (GreenHellGame.IsPCControllerActive())
		{
			if (this.m_DestroyButton != null)
			{
				this.m_DestroyButton.confirm.gameObject.SetActive(this.m_DestroyButton == this.m_ActiveButton);
			}
			if (this.m_DestroyStackButton != null)
			{
				this.m_DestroyStackButton.confirm.gameObject.SetActive(this.m_DestroyStackButton == this.m_ActiveButton);
			}
		}
	}

	private void UpdateGrabAnim()
	{
		AnimatorStateInfo currentAnimatorStateInfo = Player.Get().m_Animator.GetCurrentAnimatorStateInfo(2);
		AnimatorStateInfo currentAnimatorStateInfo2 = Player.Get().m_Animator.GetCurrentAnimatorStateInfo(1);
		AnimatorStateInfo currentAnimatorStateInfo3 = Player.Get().m_Animator.GetCurrentAnimatorStateInfo(5);
		if (((currentAnimatorStateInfo.shortNameHash == TriggerController.s_BGrabItem || currentAnimatorStateInfo.shortNameHash == TriggerController.s_BGrabItemBow || currentAnimatorStateInfo.shortNameHash == TriggerController.s_BGrabItemBambooBow) && currentAnimatorStateInfo.normalizedTime > 0.5f) || ((currentAnimatorStateInfo3.shortNameHash == TriggerController.s_BGrabItem || currentAnimatorStateInfo3.shortNameHash == TriggerController.s_BGrabItemBow || currentAnimatorStateInfo3.shortNameHash == TriggerController.s_BGrabItemBambooBow) && currentAnimatorStateInfo3.normalizedTime > 0.5f) || (currentAnimatorStateInfo2.shortNameHash == TriggerController.Get().m_BDrinkWater && currentAnimatorStateInfo2.normalizedTime > 0.5f) || (currentAnimatorStateInfo2.shortNameHash == Player.Get().m_CleanUpHash && currentAnimatorStateInfo2.normalizedTime >= 0.98f))
		{
			this.DelayedExecute();
		}
	}

	protected override void Update()
	{
		base.Update();
		Vector3 zero = Vector3.zero;
		zero.x = (float)Screen.width * this.m_Offset.x;
		zero.y = (float)Screen.height * this.m_Offset.y;
		base.transform.position = this.m_HUDTriggerAttach.position + zero;
		if (Inventory3DManager.Get().IsActive() && this.m_Item && !this.m_Item.m_Info.m_CanBeFocusedInInventory && !this.m_Item.IsStorage())
		{
			this.Deactivate();
		}
	}

	public void OnDestroyItem(Item item)
	{
		if (item && item == this.m_Item)
		{
			this.Deactivate();
		}
		foreach (HUDItemButton huditemButton in this.m_ActiveButtons)
		{
			if (huditemButton.confirm_sel != null)
			{
				huditemButton.confirm_sel.gameObject.SetActive(false);
			}
			if (huditemButton.confirm != null)
			{
				huditemButton.confirm.gameObject.SetActive(false);
			}
		}
	}

	public bool CanReceiveAction()
	{
		return base.enabled;
	}

	public bool CanReceiveActionPaused()
	{
		return false;
	}

	public void OnInputAction(InputActionData action_data)
	{
		if (action_data.m_Action == InputsManager.InputAction.HUDItemPrev || action_data.m_Action == InputsManager.InputAction.DPadDown)
		{
			this.m_PadSelectedIndex++;
			if (this.m_PadSelectedIndex >= this.m_ActiveButtons.Count)
			{
				this.m_PadSelectedIndex = 0;
				return;
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.HUDItemNext || action_data.m_Action == InputsManager.InputAction.DPadUp)
		{
			this.m_PadSelectedIndex--;
			if (this.m_PadSelectedIndex < 0)
			{
				this.m_PadSelectedIndex = this.m_ActiveButtons.Count - 1;
				return;
			}
		}
		else
		{
			if (action_data.m_Action == InputsManager.InputAction.HUDItemSelect)
			{
				this.Execute();
				return;
			}
			if (action_data.m_Action == InputsManager.InputAction.HUDItemCancel)
			{
				if (this.m_DestroyButton != null && this.m_DestroyButton.confirm && this.m_DestroyButton.confirm.gameObject.activeSelf)
				{
					this.m_DestroyButton.confirm.gameObject.SetActive(false);
					this.m_DestroyButton.confirm_sel.gameObject.SetActive(false);
					return;
				}
				this.Deactivate();
			}
		}
	}

	public bool m_Active;

	[HideInInspector]
	public Item m_Item;

	[HideInInspector]
	public LiquidSource m_LiquidSource;

	[HideInInspector]
	public PlantFruit m_PlantFruit;

	[HideInInspector]
	public ItemReplacer m_ItemReplacer;

	private List<HUDItemButton> m_ActiveButtons = new List<HUDItemButton>();

	private HUDItemButton m_ActiveButton;

	private HUDItemButton m_DestroyButton;

	private HUDItemButton m_DestroyStackButton;

	private List<HUDItemButton> m_Buttons = new List<HUDItemButton>();

	private static HUDItem s_Instance;

	public GameObject m_ElemGroup;

	private const int m_MaxElemNum = 4;

	public Image m_SelectionBG;

	public Image m_SeparatorV;

	private Image m_Icon;

	private Image m_AdditionalIcon;

	private HUDItem.Action m_ActionToExecute;

	private bool m_ExecutionInProgress;

	public float m_DelayDeactivate = 0.5f;

	private bool m_DelayDeactivateRequested;

	private Transform m_HUDTriggerAttach;

	private int m_PadSelectedIndex;

	private bool m_CursorVisible;

	public GameObject m_PadIconSelect;

	public Vector3 m_Offset = Vector3.zero;

	public Vector3 m_BackpackOffset = Vector3.zero;

	public Vector3 m_CursorOffset = Vector3.zero;

	private Vector2 m_PadHideCursorPos = Vector2.zero;

	private List<HUDItemButton> m_TempButtons = new List<HUDItemButton>();

	public enum Action
	{
		None,
		Close,
		Take,
		PickUp,
		Eat,
		Drink,
		Harvest,
		Craft,
		Fill,
		Equip,
		Drop,
		Swap,
		Use,
		Spill,
		Destroy,
		Take3,
		TakeAll,
		TakeClay,
		CleanUp,
		DestroyStack,
		SwapArmor,
		EquipArmor,
		Insert,
		Pick,
		TakeOffArmor,
		Plow,
		PickStack
	}
}
