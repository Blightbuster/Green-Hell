using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDItem : HUDBase
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
			huditemButton.text = transform.GetComponentInChildren<Text>();
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
		return !TriggerController.Get().IsGrabInProgress() && this.m_Active;
	}

	private void ResetItems()
	{
		this.m_Item = null;
		this.m_LiquidSource = null;
		this.m_PlantFruit = null;
		this.m_ItemReplacer = null;
	}

	private void Activate()
	{
		this.m_ActiveButton = null;
		this.UpdateSelectionBG();
		this.m_Active = true;
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
		this.ResetItems();
		this.m_LiquidSource = source;
		this.ClearSlots();
		this.AddSlot(HUDItem.Action.Drink);
		this.AddSlot(HUDItem.Action.Fill);
		this.Activate();
		return true;
	}

	public bool Activate(ItemReplacer item)
	{
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
		this.Activate();
		return true;
	}

	public bool Activate(Item item)
	{
		if (!item || !item.CanExecuteActions())
		{
			return false;
		}
		this.ResetItems();
		this.m_Item = item;
		this.ClearSlots();
		bool flag = Player.Get().m_SwimController.IsActive();
		if (this.m_Item.m_Info.m_Craftable && !this.m_Item.m_OnCraftingTable && Player.Get().CanStartCrafting())
		{
			this.AddSlot(HUDItem.Action.Craft);
		}
		if (this.m_Item.m_Info.IsHeavyObject() && !this.m_Item.m_Info.m_CanBeAddedToInventory)
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
				this.AddSlot(HUDItem.Action.Use);
			}
			else if (this.m_Item.m_InInventory)
			{
				if (item != InventoryBackpack.Get().m_EquippedItem)
				{
					this.AddSlot(HUDItem.Action.Equip);
				}
			}
			else
			{
				ItemSlot exists = InventoryBackpack.Get().FindFreeSlot(this.m_Item);
				if (exists)
				{
					this.AddSlot(HUDItem.Action.Take);
				}
				else
				{
					this.AddSlot(HUDItem.Action.Swap);
				}
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
		if (this.m_Item.m_Info.IsLiquidContainer())
		{
			LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)this.m_Item.m_Info;
			if (liquidContainerInfo.m_Amount > 0f)
			{
				this.AddSlot(HUDItem.Action.Spill);
			}
		}
		if (this.m_Item.m_Info.m_Harvestable && !flag && (!this.m_Item.RequiresToolToHarvest() || Player.Get().HasBlade()))
		{
			this.AddSlot(HUDItem.Action.Harvest);
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
		else
		{
			this.AddSlot(HUDItem.Action.Destroy);
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
		CursorManager.Get().ShowCursor(true);
		CursorManager.Get().UpdateCursorVisibility();
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
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
		CursorManager.Get().ShowCursor(false);
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		CursorManager.Get().UpdateCursorVisibility();
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
			huditemButton.text.text = localization.Get("None");
			break;
		case HUDItem.Action.Close:
			huditemButton.text.text = localization.Get("HUD_Trigger_Close");
			break;
		case HUDItem.Action.Take:
			huditemButton.text.text = localization.Get("HUD_Trigger_Take");
			break;
		case HUDItem.Action.PickUp:
			huditemButton.text.text = localization.Get("HUD_Trigger_PickUp");
			break;
		case HUDItem.Action.Eat:
			huditemButton.text.text = localization.Get("HUD_Trigger_Eat");
			break;
		case HUDItem.Action.Drink:
			huditemButton.text.text = localization.Get("HUD_Trigger_Drink");
			break;
		case HUDItem.Action.Harvest:
			huditemButton.text.text = localization.Get("HUD_Trigger_Harvest");
			break;
		case HUDItem.Action.Craft:
			huditemButton.text.text = localization.Get("Craft");
			break;
		case HUDItem.Action.Fill:
			huditemButton.text.text = localization.Get("HUD_Trigger_Fill");
			break;
		case HUDItem.Action.Equip:
			huditemButton.text.text = localization.Get("HUD_Trigger_Equip");
			break;
		case HUDItem.Action.Drop:
			huditemButton.text.text = localization.Get("HUD_ItemInHand_Drop");
			break;
		case HUDItem.Action.Swap:
			huditemButton.text.text = localization.Get("HUD_Trigger_SwapHold");
			break;
		case HUDItem.Action.Use:
			huditemButton.text.text = localization.Get("HUD_Trigger_Use");
			break;
		case HUDItem.Action.Spill:
			huditemButton.text.text = localization.Get("HUD_Trigger_Spill");
			break;
		case HUDItem.Action.Destroy:
			huditemButton.text.text = localization.Get("HUD_Trigger_Destroy");
			break;
		case HUDItem.Action.Take3:
			huditemButton.text.text = localization.Get("HUD_Trigger_Take3");
			break;
		case HUDItem.Action.TakeAll:
			huditemButton.text.text = localization.Get("HUD_Trigger_TakeAll");
			break;
		}
		huditemButton.button.SetActive(true);
		this.m_ActiveButtons.Add(huditemButton);
	}

	public void Execute()
	{
		if (this.m_ExecutionInProgress)
		{
			return;
		}
		if (this.m_ActiveButton != null)
		{
			if (Inventory3DManager.Get().gameObject.activeSelf || this.m_ActiveButton.action == HUDItem.Action.Harvest || this.m_ActiveButton.action == HUDItem.Action.Destroy)
			{
				this.ExecuteAction(this.m_ActiveButton.action, this.m_Item);
				base.Invoke("DelayDeactivate", this.m_DelayDeactivate);
				this.m_DelayDeactivateRequested = true;
				this.m_ElemGroup.SetActive(false);
			}
			else
			{
				this.m_ActionToExecute = this.m_ActiveButton.action;
				Player.Get().BlockMoves();
				Player.Get().BlockRotation();
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
				this.m_ExecutionInProgress = true;
			}
		}
		else
		{
			this.Deactivate();
		}
	}

	private void DelayDeactivate()
	{
		this.m_Active = false;
	}

	public void Deactivate()
	{
		this.m_Active = false;
	}

	private void DelayedExecute()
	{
		this.ExecuteAction(this.m_ActionToExecute, this.m_Item);
		Player.Get().UnblockMoves();
		Player.Get().UnblockRotation();
		Player.Get().m_Animator.SetBool(TriggerController.s_BGrabItem, false);
		this.m_Active = false;
		this.m_ExecutionInProgress = false;
	}

	private void ExecuteAction(HUDItem.Action action, Item item)
	{
		if (action != HUDItem.Action.Close && this.m_ItemReplacer != null)
		{
			item = this.m_ItemReplacer.ReplaceItem(true);
			this.m_ItemReplacer = null;
		}
		switch (action)
		{
		case HUDItem.Action.Close:
			this.m_Active = false;
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
				if (InventoryBackpack.Get().m_EquippedItem == item)
				{
					InventoryBackpack.Get().m_EquippedItem = null;
				}
				InventoryBackpack.Get().RemoveItem(item, false);
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
				CraftingManager.Get().RemoveItem(item);
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
			if (MakeFireController.Get().IsActive())
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
					Player.Get().SetWantedItem((!currentItem.m_Info.IsBow()) ? Hand.Right : Hand.Left, null, true);
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
			if (CraftingManager.Get().gameObject.activeSelf && CraftingManager.Get().ContainsItem(item))
			{
				CraftingManager.Get().RemoveItem(item);
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
		}
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
			}
			else if (this.ShouldCancel())
			{
				this.Deactivate();
			}
			else if (HUDWheel.Get().enabled)
			{
				this.m_Active = false;
			}
		}
	}

	private bool ShouldExecute()
	{
		return Input.GetMouseButtonDown(0) || !InputsManager.Get().IsActionActive(TriggerAction.TYPE.Expand);
	}

	private bool ShouldCancel()
	{
		return !InputsManager.Get().IsActionActive(TriggerAction.TYPE.Expand);
	}

	private void ShowElements()
	{
		this.m_ElemGroup.SetActive(true);
		this.SetupIcon((!this.m_Item) ? string.Empty : this.m_Item.GetIconName(), (!this.m_Item) ? ItemAdditionalIcon.None : this.m_Item.GetAdditionalIcon());
		if (this.m_ActiveButtons.Count < 3)
		{
			Vector3 localScale = this.m_SeparatorV.rectTransform.localScale;
			localScale.y = 0.66f;
			this.m_SeparatorV.rectTransform.localScale = localScale;
		}
		else
		{
			Vector3 localScale2 = this.m_SeparatorV.rectTransform.localScale;
			localScale2.y = 1f;
			this.m_SeparatorV.rectTransform.localScale = localScale2;
		}
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
		foreach (HUDItemButton huditemButton in this.m_ActiveButtons)
		{
			if (RectTransformUtility.RectangleContainsScreenPoint(huditemButton.trans, Input.mousePosition))
			{
				this.m_ActiveButton = huditemButton;
				break;
			}
		}
		CursorManager.Get().SetCursor((this.m_ActiveButton == null) ? CursorManager.TYPE.Normal : CursorManager.TYPE.MouseOver);
		this.UpdateSelectionBG();
	}

	private void UpdateSelectionBG()
	{
		if (this.m_ActiveButton != null)
		{
			this.m_SelectionBG.gameObject.SetActive(true);
			this.m_SelectionBG.rectTransform.position = this.m_ActiveButton.trans.position;
		}
		else
		{
			this.m_SelectionBG.gameObject.SetActive(false);
		}
	}

	private void UpdateGrabAnim()
	{
		AnimatorStateInfo currentAnimatorStateInfo = Player.Get().m_Animator.GetCurrentAnimatorStateInfo(2);
		AnimatorStateInfo currentAnimatorStateInfo2 = Player.Get().m_Animator.GetCurrentAnimatorStateInfo(5);
		if ((currentAnimatorStateInfo.shortNameHash == TriggerController.s_BGrabItem || currentAnimatorStateInfo2.shortNameHash == TriggerController.s_BGrabItem || currentAnimatorStateInfo.shortNameHash == TriggerController.s_BGrabItemBow || currentAnimatorStateInfo2.shortNameHash == TriggerController.s_BGrabItemBow || currentAnimatorStateInfo.shortNameHash == TriggerController.s_BGrabItemBambooBow || currentAnimatorStateInfo2.shortNameHash == TriggerController.s_BGrabItemBambooBow) && currentAnimatorStateInfo.normalizedTime > 0.5f)
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

	public Vector3 m_Offset = Vector3.zero;

	public Vector3 m_BackpackOffset = Vector3.zero;

	public Vector3 m_CursorOffset = Vector3.zero;

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
		TakeAll
	}
}
