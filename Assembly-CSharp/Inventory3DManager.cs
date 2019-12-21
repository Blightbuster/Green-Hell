using System;
using System.Collections.Generic;
using AIs;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class Inventory3DManager : MonoBehaviour, IInputsReceiver
{
	[HideInInspector]
	public Item m_CarriedItem
	{
		get
		{
			return this.m_CarriedItemProp;
		}
		set
		{
			Item carriedItemProp = this.m_CarriedItemProp;
			this.m_CarriedItemProp = value;
			if (this.m_CarriedItemProp != null)
			{
				this.m_CarriedItemProp.UpdateLayer();
				this.m_CarriedItemProp.UpdateScale(false);
				return;
			}
			if (carriedItemProp)
			{
				carriedItemProp.UpdateLayer();
				carriedItemProp.UpdateScale(false);
			}
		}
	}

	public static Inventory3DManager Get()
	{
		return Inventory3DManager.s_Instance;
	}

	private void Awake()
	{
		Inventory3DManager.s_Instance = this;
		this.m_BackpackLayerMask = LayerMask.GetMask(new string[]
		{
			"3DInventory",
			"Outline"
		});
		this.m_WorldLayerMask = LayerMask.GetMask(new string[]
		{
			"Item",
			"Outline",
			"ClosedBox"
		});
		this.m_Camera.enabled = false;
		this.m_Canvas.gameObject.SetActive(false);
		this.m_Collider.enabled = false;
		this.InitAudio();
	}

	private void InitAudio()
	{
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		}
		if (this.m_DropItemAudioClip == null)
		{
			this.m_DropItemAudioClip = (Resources.Load("Sounds/Items/click_drop_item_backpack") as AudioClip);
		}
	}

	private void Start()
	{
		base.gameObject.SetActive(false);
		InputsManager.Get().RegisterReceiver(this);
	}

	public void ScenarioBlockBackpack()
	{
		this.m_ScenarioBlocked = true;
	}

	public void ScenarioUnblockBackpack()
	{
		this.m_ScenarioBlocked = false;
	}

	public void Activate()
	{
		if (this.m_ScenarioBlocked)
		{
			return;
		}
		if (CutscenesManager.Get().IsCutscenePlaying())
		{
			return;
		}
		if (base.gameObject.activeSelf)
		{
			return;
		}
		if (BodyInspectionMiniGameController.Get().IsActive())
		{
			return;
		}
		if (BodyInspectionController.Get().IsBandagingInProgress())
		{
			return;
		}
		if (VomitingController.Get().IsActive())
		{
			return;
		}
		if (SwimController.Get().IsActive())
		{
			return;
		}
		if (ConsciousnessController.Get().IsActive())
		{
			return;
		}
		if (WatchController.Get().IsActive())
		{
			return;
		}
		if (SleepController.Get().IsActive())
		{
			return;
		}
		if (InsectsController.Get().IsActive())
		{
			return;
		}
		if (HarvestingAnimalController.Get().IsActive())
		{
			return;
		}
		if (MudMixerController.Get().IsActive())
		{
			return;
		}
		if (HarvestingSmallAnimalController.Get().IsActive())
		{
			return;
		}
		if (Player.Get().IsDead())
		{
			return;
		}
		if (HitReactionController.Get().IsActive())
		{
			return;
		}
		if (TriggerController.Get().IsGrabInProgress())
		{
			return;
		}
		if (HUDMovie.Get().enabled && HUDMovie.Get().gameObject.activeSelf)
		{
			return;
		}
		if (Player.Get().m_Aim || Time.time - Player.Get().m_StopAimTime < 0.5f)
		{
			return;
		}
		if (MakeFireController.Get().IsMakeFireGame())
		{
			return;
		}
		if (Player.Get().m_Animator.GetBool(Player.Get().m_CleanUpHash))
		{
			return;
		}
		if (Player.Get().m_Animator.GetBool(TriggerController.Get().m_BDrinkWater))
		{
			return;
		}
		if (ScenarioManager.Get().IsDreamOrPreDream())
		{
			return;
		}
		if (HUDReadableItem.Get().enabled)
		{
			return;
		}
		if (ScenarioManager.Get().IsBoolVariableTrue("PlayerMechGameEnding"))
		{
			return;
		}
		if (WalkieTalkieController.Get().IsActive())
		{
			WalkieTalkieController.Get().Stop();
		}
		base.gameObject.SetActive(true);
		if (!Player.Get().m_BodyInspectionController.IsActive() && !CraftingManager.Get().gameObject.activeSelf)
		{
			Player.Get().StartController(PlayerControllerType.Inventory);
			if (Player.Get().m_ControllerToStart != PlayerControllerType.Unknown)
			{
				Player.Get().StartControllerInternal();
			}
		}
		this.BlockPlayerRotation(true);
		this.m_Camera.enabled = true;
		HUDItem.Get().Deactivate();
		this.m_Canvas.gameObject.SetActive(true);
		CursorManager.Get().ShowCursor(true, true);
		HUDManager.Get().SetActiveGroup(HUDManager.HUDGroup.Inventory3D);
		this.m_CarriedItem = null;
		this.SetupPocket(this.m_ActivePocket);
		Player.Get().m_BackpackWasOpen = true;
		if (BodyInspectionController.Get().IsActive() && PlayerInjuryModule.Get().IsAnyWound())
		{
			HintsManager.Get().ShowHint("Inspection_Backpack", 10f);
		}
		this.m_ActivityChanged = true;
	}

	public void Deactivate()
	{
		if (this.m_CarriedItem && !this.m_CarriedItem.m_CurrentSlot)
		{
			this.OnLMouseUp();
		}
		this.BlockPlayerRotation(false);
		if (!Player.Get().m_BodyInspectionController.IsActive())
		{
			Player.Get().StopController(PlayerControllerType.Inventory);
		}
		this.m_Camera.enabled = false;
		this.m_Canvas.gameObject.SetActive(false);
		CursorManager.Get().ShowCursor(false, false);
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		HUDManager.Get().SetActiveGroup(HUDManager.HUDGroup.Game);
		if (CraftingManager.Get().IsActive())
		{
			CraftingManager.Get().Deactivate();
		}
		if (Storage3D.Get().IsActive())
		{
			Storage3D.Get().Deactivate();
		}
		if (HUDItem.Get().m_Active)
		{
			HUDItem.Get().Deactivate();
		}
		this.ResetNewCraftedItem();
		this.SetSelectedSlot(null);
		this.m_SelectedGroup = null;
		this.m_MouseOverCraftTable = false;
		this.m_MouseOverBackpack = false;
		this.m_MouseOverStorage = false;
		InventoryBackpack.Get().OnCloseBackpack();
		base.gameObject.SetActive(false);
		this.m_DeactivationTime = Time.time;
		this.m_ActivityChanged = true;
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		if (currentItem != null && currentItem.m_Info.m_ID == ItemID.Fishing_Rod)
		{
			Player.Get().StartController(PlayerControllerType.Fishing);
		}
	}

	public bool IsActive()
	{
		return base.gameObject.activeSelf;
	}

	private void BlockPlayerRotation(bool block)
	{
		if (block && !this.m_PlayerRotationBlocked)
		{
			Player.Get().BlockRotation();
			this.m_PlayerRotationBlocked = true;
			return;
		}
		if (!block && this.m_PlayerRotationBlocked)
		{
			Player.Get().UnblockRotation();
			this.m_PlayerRotationBlocked = false;
		}
	}

	public void CustomUpdate()
	{
		this.UpdatePadControll();
		this.UpdateRaycast();
		this.UpdateMouseOver();
		this.UpdateFocusedItem();
		this.UpdateMouseInputs();
		this.UpdateSelectedGroup();
		this.UpdateCarriedItemPosition();
		this.UpdateCursor();
		this.UpdateNewCraftedItem();
		if (Player.Get().IsDead() || WalkieTalkieController.Get().IsActive())
		{
			this.Deactivate();
		}
		this.m_ActivityChanged = false;
	}

	private void UpdateCursor()
	{
		if (HUDItem.Get().enabled)
		{
			return;
		}
		if (HUDNewWheel.Get().IsSelected())
		{
			return;
		}
		if (HUDBackpack.Get().m_IsHovered)
		{
			return;
		}
		if (this.m_CarriedItem)
		{
			if (CursorManager.Get().GetCursor() != CursorManager.TYPE.Hand_1)
			{
				CursorManager.Get().SetCursor(CursorManager.TYPE.Hand_1);
				return;
			}
		}
		else if (this.m_FocusedItem)
		{
			if (CursorManager.Get().GetCursor() != CursorManager.TYPE.Hand_0)
			{
				CursorManager.Get().SetCursor(CursorManager.TYPE.Hand_0);
				return;
			}
		}
		else if (HUDCrafting.Get().IsOverCraftButton() && CraftingManager.Get().m_Results.Count > 0)
		{
			if (CursorManager.Get().GetCursor() != CursorManager.TYPE.MouseOver)
			{
				CursorManager.Get().SetCursor(CursorManager.TYPE.MouseOver);
				return;
			}
		}
		else if (CursorManager.Get().GetCursor() != CursorManager.TYPE.Normal)
		{
			CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		}
	}

	private void UpdatePadControll()
	{
		if (!GreenHellGame.IsPadControllerActive())
		{
			return;
		}
		if (CrossPlatformInputManager.GetAxis("LeftTrigger") > 0.5f)
		{
			if (this.m_PlayerRotationBlocked)
			{
				this.BlockPlayerRotation(false);
			}
			return;
		}
		if (!this.m_PlayerRotationBlocked)
		{
			this.BlockPlayerRotation(true);
		}
		float speed_mul = 1f;
		if (this.m_FocusedItem)
		{
			speed_mul = (this.m_CarriedItem ? 0.7f : this.m_FocusedItem.m_Info.m_PadCursorSpeedMul);
		}
		else if (HUDCrafting.Get().IsOverCraftButton() || HUDBackpack.Get().m_IsHovered)
		{
			speed_mul = 0.25f;
		}
		else if (BodyInspectionController.Get().IsActive() && BodyInspectionController.Get().IsCursorOverLeech())
		{
			speed_mul = 0.1f;
		}
		CursorManager.Get().UpdatePadCursor(speed_mul);
	}

	private void UpdateRaycast()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.x += 10f;
		float maxDistance = 5f;
		Vector2 zero = Vector2.zero;
		zero.x = mousePosition.x / (float)Screen.width;
		zero.y = mousePosition.y / (float)Screen.height;
		Ray ray = CameraManager.Get().m_MainCamera.ViewportPointToRay(zero);
		if (!HUDNewWheel.Get().IsSelected())
		{
			this.m_WorldHitsCnt = Physics.RaycastNonAlloc(ray, this.m_WorldHits, maxDistance, this.m_WorldLayerMask);
			Array.Sort<RaycastHit>(this.m_WorldHits, 0, this.m_WorldHitsCnt, TriggerController.s_DistComparer);
		}
		else
		{
			this.m_WorldHitsCnt = 0;
		}
		RectTransform rectTransform = this.m_InventoryImage.rectTransform;
		Vector2 zero2 = Vector2.zero;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePosition, null, out zero2);
		zero.x = CJTools.Math.GetProportionalClamp(0f, 1f, zero2.x, -rectTransform.sizeDelta.x * 0.5f, rectTransform.sizeDelta.x * 0.5f);
		zero.y = CJTools.Math.GetProportionalClamp(0f, 1f, zero2.y, -rectTransform.sizeDelta.y * 0.5f, rectTransform.sizeDelta.y * 0.5f);
		ray = this.m_Camera.ViewportPointToRay(zero);
		this.m_BackpackHitsCnt = Physics.RaycastNonAlloc(ray, this.m_BackpackHits, maxDistance, this.m_BackpackLayerMask);
		Array.Sort<RaycastHit>(this.m_BackpackHits, 0, this.m_BackpackHitsCnt, this.m_DistComparer);
	}

	private void UpdateMouseOver()
	{
		this.m_MouseOverCraftTable = false;
		this.m_MouseOverBackpack = false;
		this.m_MouseOverStorage = false;
		if (this.m_SelectedSlot && this.m_SelectedSlot.m_InventoryStackSlot && this.m_SelectedSlot.m_ItemParent && this.m_SelectedSlot.m_ItemParent.m_InInventory)
		{
			this.m_MouseOverBackpack = true;
			return;
		}
		if (this.m_SelectedSlot && this.m_SelectedSlot.m_InventoryStackSlot && this.m_SelectedSlot.m_ItemParent && this.m_SelectedSlot.m_ItemParent.m_InStorage)
		{
			this.m_MouseOverStorage = true;
			return;
		}
		for (int i = 0; i < this.m_BackpackHitsCnt; i++)
		{
			if (this.m_BackpackHits[i].collider.gameObject == this.m_Backpack)
			{
				this.m_MouseOverBackpack = true;
			}
			else if (this.m_BackpackHits[i].collider == CraftingManager.Get().m_TableCollider)
			{
				this.m_MouseOverCraftTable = true;
			}
			else if (Storage3D.Get().m_ActiveData != null && this.m_BackpackHits[i].collider == Storage3D.Get().m_ActiveData.m_Plane)
			{
				this.m_MouseOverStorage = true;
			}
			if (this.m_MouseOverCraftTable || this.m_MouseOverBackpack || this.m_MouseOverStorage)
			{
				break;
			}
		}
	}

	private bool CanFocusItem(Item item)
	{
		return item && item.CanBeFocuedInInventory() && item.m_Info != null && !(item == this.m_NewCraftedItem) && (!item.m_Info.IsArmor() || !PlayerArmorModule.Get().IsItemAttached(item) || HUDBodyInspection.Get().m_ArmorEnabled) && ((item.IsStorage() && !Storage3D.Get().IsActive()) || ((!item.m_Info.IsFishingRod() || !(Player.Get().GetCurrentItem() == item) || this.m_ActivePocket == BackpackPocket.Left) && (item.CanTrigger() && !item.IsItemHold() && item.m_Info != null) && (item.m_Info.m_CanBeFocusedInInventory || item.m_Info.IsHeavyObject() || item.IsItemReplacer())));
	}

	private void UpdateFocusedItem()
	{
		this.m_FocusedItem = null;
		if (HUDBackpack.Get().m_IsHovered)
		{
			return;
		}
		if (HUDCrafting.Get().IsOverCraftButton())
		{
			return;
		}
		if (this.m_CarriedItem)
		{
			this.m_FocusedItem = this.m_CarriedItem;
			return;
		}
		if (HUDQuickAccessBar.Get().AnyButtonSelected())
		{
			return;
		}
		for (int i = 0; i < this.m_BackpackHitsCnt; i++)
		{
			Item component = this.m_BackpackHits[i].collider.gameObject.GetComponent<Item>();
			if (this.CanFocusItem(component))
			{
				this.m_FocusedItem = component;
				return;
			}
		}
		if (!this.m_MouseOverBackpack && !this.m_MouseOverCraftTable && !this.m_MouseOverStorage)
		{
			for (int j = 0; j < this.m_WorldHitsCnt; j++)
			{
				Item component2 = this.m_WorldHits[j].collider.gameObject.GetComponent<Item>();
				if (!component2 && !this.m_WorldHits[j].collider.isTrigger)
				{
					return;
				}
				if (this.CanFocusItem(component2))
				{
					this.m_FocusedItem = component2;
					return;
				}
			}
		}
	}

	public void RotateItem(Item item, bool force = false)
	{
		if (!force && (this.m_ActivePocket == BackpackPocket.Left || this.m_ActivePocket == BackpackPocket.Top))
		{
			return;
		}
		if (!item)
		{
			return;
		}
		item.m_Info.m_InventoryRotated = !item.m_Info.m_InventoryRotated;
		item.transform.rotation = Quaternion.identity;
		if (item.m_Info.m_InventoryRotated)
		{
			item.transform.RotateAround(item.m_BoxCollider.bounds.center, item.transform.up, 90f);
		}
		this.UpdateCarriedItemPosition();
		this.OnModifyCarriedItem();
	}

	public bool CanReceiveAction()
	{
		return true;
	}

	public bool CanReceiveActionPaused()
	{
		return false;
	}

	public void OnInputAction(InputActionData action_data)
	{
		if (CraftingController.Get().BlockInventoryInputs() || HarvestingAnimalController.Get().BlockInventoryInputs() || HarvestingSmallAnimalController.Get().BlockInventoryInputs() || MudMixerController.Get().BlockInventoryInputs())
		{
			return;
		}
		if (!base.gameObject.activeSelf && action_data.m_Action == InputsManager.InputAction.ShowInventory)
		{
			this.Activate();
			return;
		}
		if (!this.m_ActivityChanged && base.gameObject.activeSelf && (action_data.m_Action == InputsManager.InputAction.HideInventory || action_data.m_Action == InputsManager.InputAction.Quit || action_data.m_Action == InputsManager.InputAction.AdditionalQuit || action_data.m_Action == InputsManager.InputAction.ShowInventory))
		{
			if (!GreenHellGame.IsPadControllerActive() || !HUDItem.Get().enabled)
			{
				if (CraftingManager.Get().gameObject.activeSelf)
				{
					CraftingManager.Get().Deactivate();
					return;
				}
				this.Deactivate();
				return;
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.SortItemsInBackpack && base.gameObject.activeSelf)
		{
			this.SortItemsBySize();
		}
	}

	private void UpdateMouseInputs()
	{
		if (Input.GetMouseButtonDown(0))
		{
			this.OnLMouseDown();
		}
		if (Input.GetMouseButtonUp(0) || (Input.GetKeyDown(InputHelpers.PadButton.Button_X.KeyFromPad()) && !HUDItem.Get().enabled))
		{
			if (this.m_BlockLMouseUP)
			{
				this.m_BlockLMouseUP = false;
			}
			else
			{
				this.OnLMouseUp();
			}
		}
		if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(InputHelpers.PadButton.Button_Y.KeyFromPad()))
		{
			this.OnRMouseDown();
		}
		if (Input.GetMouseButtonUp(1))
		{
			this.OnRMouseUp();
		}
	}

	public bool CanSetCarriedItem(bool check_huditem = true)
	{
		return this.m_FocusedItem && !this.m_FocusedItem.IsStorage() && !HUDBackpack.Get().m_IsHovered && !this.m_FocusedItem.m_Info.m_CantBeDraggedInInventory && (!check_huditem || !HUDItem.Get().enabled);
	}

	public void OnLMouseDown()
	{
		if (!this.CanSetCarriedItem(true))
		{
			return;
		}
		this.StartCarryItem(this.m_FocusedItem, false);
	}

	public void StartCarryItem(Item item, bool take_stack = false)
	{
		this.m_TakeStack = take_stack;
		this.SetCarriedItem(item, true);
		this.UpdateSpill();
		if (CraftingManager.Get().gameObject.activeSelf && CraftingManager.Get().ContainsItem(item))
		{
			CraftingManager.Get().RemoveItem(item, false);
		}
		if (item == Player.Get().GetCurrentItem(Hand.Right))
		{
			Player.Get().SetWantedItem(Hand.Right, null, true);
		}
		else if (item == Player.Get().GetCurrentItem(Hand.Left))
		{
			Player.Get().SetWantedItem(Hand.Left, null, true);
		}
		if (item.m_Info.IsArmor())
		{
			PlayerArmorModule.Get().ArmorCarryStarted(item);
		}
		if (item.m_Acre != null)
		{
			item.m_Acre.OnTake(item);
		}
		this.m_TakeStack = false;
	}

	private void UpdateSpill()
	{
	}

	private bool CanInsertCarriedItemToBackpack()
	{
		return this.m_CarriedItem && this.m_CarriedItem.m_Info.m_CanBeAddedToInventory && ((this.m_SelectedSlot && this.m_SelectedSlot.m_BackpackSlot && (!this.m_SelectedSlot.m_ItemParent || !this.m_SelectedSlot.m_ItemParent.m_InStorage)) || (this.m_SelectedGroup != null && this.m_SelectedGroup.IsFree() && this.m_SelectedGroup.m_Pocked != BackpackPocket.Storage));
	}

	private void OnLMouseUp()
	{
		if (!this.m_CarriedItem)
		{
			return;
		}
		if (this.CanInsertCarriedItemToBackpack())
		{
			InsertResult insertResult = InventoryBackpack.Get().InsertItem(this.m_CarriedItem, this.m_SelectedSlot, this.m_SelectedGroup, false, false, false, false, true);
			if (insertResult != InsertResult.Ok)
			{
				insertResult = InventoryBackpack.Get().InsertItem(this.m_CarriedItem, this.m_CarriedItem.m_PrevSlot, this.m_CarriedItem.m_Info.m_PrevInventoryCellsGroup, false, false, false, false, true);
			}
			if (insertResult != InsertResult.Ok)
			{
				insertResult = InventoryBackpack.Get().InsertItem(this.m_CarriedItem, null, null, true, true, true, true, true);
			}
			if (insertResult == InsertResult.Ok)
			{
				this.m_CarriedItem.m_ShownInInventory = true;
				this.PlayDropSound();
			}
			using (List<Item>.Enumerator enumerator = this.m_StackItems.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item item = enumerator.Current;
					insertResult = InventoryBackpack.Get().InsertItem(item, this.m_SelectedSlot ? this.m_SelectedSlot : this.m_CarriedItem.m_InventorySlot, null, false, false, false, false, false);
					if (insertResult != InsertResult.Ok)
					{
						insertResult = InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, true);
					}
					if (insertResult == InsertResult.Ok)
					{
						item.m_ShownInInventory = true;
					}
				}
				goto IL_576;
			}
		}
		if (CraftingManager.Get().gameObject.activeSelf && this.m_MouseOverCraftTable)
		{
			CraftingManager.Get().AddItem(this.m_CarriedItem, false);
			this.PlayDropSound();
			using (List<Item>.Enumerator enumerator = this.m_StackItems.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item item2 = enumerator.Current;
					CraftingManager.Get().AddItem(item2, false);
				}
				goto IL_576;
			}
		}
		if (!this.m_CarriedItem.m_Info.m_CanBeRemovedFromInventory)
		{
			InventoryBackpack.Get().InsertItem(this.m_CarriedItem, null, null, true, true, true, true, true);
			this.PlayDropSound();
		}
		else if (Storage3D.Get().IsActive() && this.m_MouseOverStorage && Storage3D.Get().CanInsertItem(this.m_CarriedItem))
		{
			Storage3D.Get().InsertItem(this.m_CarriedItem, this.m_SelectedSlot, (this.m_SelectedGroup != null && this.m_SelectedGroup.IsFree()) ? this.m_SelectedGroup : null, true, true);
			foreach (Item item3 in this.m_StackItems)
			{
				if (Storage3D.Get().InsertItem(item3, this.m_SelectedSlot ? this.m_SelectedSlot : this.m_CarriedItem.m_InventorySlot, null, false, false) != InsertResult.Ok)
				{
					Storage3D.Get().InsertItem(item3, null, null, true, true);
				}
			}
			this.PlayDropSound();
		}
		else if (BodyInspectionController.Get().IsActive())
		{
			if (this.m_SelectedSlot && (this.m_SelectedSlot.IsBIWoundSlot() || this.m_SelectedSlot.IsArmorSlot()))
			{
				List<Item> list = new List<Item>(this.m_StackItems);
				this.m_StackItems.Clear();
				foreach (Item item4 in list)
				{
					if (item4 != this.m_CarriedItem && InventoryBackpack.Get().InsertItem(item4, (item4.m_PrevSlot != this.m_CarriedItem.m_InventorySlot) ? item4.m_PrevSlot : null, item4.m_Info.m_PrevInventoryCellsGroup, true, true, false, true, true) != InsertResult.Ok)
					{
						InventoryBackpack.Get().InsertItem(item4, null, null, true, true, true, true, true);
					}
				}
				this.m_SelectedSlot.InsertItem(this.m_CarriedItem);
			}
			else
			{
				this.DropItem(this.m_CarriedItem);
			}
			this.PlayDropSound();
		}
		else
		{
			if (this.m_SelectedSlot)
			{
				foreach (Item item5 in this.m_StackItems)
				{
					item5.transform.parent = null;
				}
				bool flag = this.m_SelectedSlot.m_ItemParent && this.m_SelectedSlot.m_ItemParent.IsFireTool();
				this.m_SelectedSlot.InsertItem(this.m_CarriedItem);
				this.PlayDropSound();
				using (List<Item>.Enumerator enumerator = this.m_StackItems.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Item item6 = enumerator.Current;
						if (this.m_SelectedSlot && this.m_SelectedSlot.CanInsertItem(item6))
						{
							this.m_SelectedSlot.InsertItem(item6);
						}
						else if (flag)
						{
							InventoryBackpack.Get().InsertItem(item6, null, null, true, true, true, true, true);
						}
						else
						{
							this.DropItem(item6);
						}
					}
					goto IL_576;
				}
			}
			if (this.m_MouseOverBackpack)
			{
				InventoryBackpack.Get().InsertItem(this.m_CarriedItem, null, null, true, true, true, true, true);
				this.PlayDropSound();
				using (List<Item>.Enumerator enumerator = this.m_StackItems.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Item item7 = enumerator.Current;
						InventoryBackpack.Get().InsertItem(item7, null, null, true, true, true, true, true);
					}
					goto IL_576;
				}
			}
			this.DropItem(this.m_CarriedItem);
			this.PlayDropSound();
			foreach (Item item8 in this.m_StackItems)
			{
				this.DropItem(item8);
			}
		}
		IL_576:
		this.SetCarriedItem(null, true);
	}

	private bool TryDropLiquidContainer()
	{
		return false;
	}

	private void OnRMouseDown()
	{
		if (HUDBackpack.Get().m_IsHovered)
		{
			return;
		}
		if (this.m_CarriedItem)
		{
			this.RotateItem(this.m_CarriedItem, false);
			return;
		}
		if (this.m_FocusedItem)
		{
			if (!this.m_FocusedItem.m_OnCraftingTable && TriggerController.Get().GetBestTrigger() && TriggerController.Get().GetBestTrigger().gameObject == this.m_FocusedItem.gameObject)
			{
				HUDItem.Get().Activate(this.m_FocusedItem);
				return;
			}
			if (this.m_FocusedItem.m_OnCraftingTable)
			{
				CraftingManager.Get().RemoveItem(this.m_FocusedItem, false);
				if (this.m_FocusedItem.m_Info.IsHeavyObject() || !this.m_FocusedItem.Take())
				{
					this.DropItem(this.m_FocusedItem);
				}
				this.m_FocusedItem = null;
				return;
			}
		}
		else if (this.m_PlayerRotationBlocked)
		{
			this.BlockPlayerRotation(false);
		}
	}

	private void OnRMouseUp()
	{
		this.BlockPlayerRotation(true);
	}

	private void UpdateCarriedItemPosition()
	{
		this.SetSelectedSlot(null);
		if (!this.m_CarriedItem)
		{
			return;
		}
		if (this.TryAttractCarriedItemToSlot())
		{
			return;
		}
		if (this.m_SelectedGroup != null && this.m_SelectedGroup.m_Pocked != BackpackPocket.Storage)
		{
			this.AllignCarriedItemToCollider(this.m_BackpackHits, this.m_BackpackHitsCnt, InventoryBackpack.Get().m_ActivePlane, true);
			return;
		}
		if (CraftingManager.Get().gameObject.activeSelf && this.AllignCarriedItemToCraftingTable())
		{
			return;
		}
		if (Storage3D.Get().IsActive() && this.AllignCarriedItemToStorage())
		{
			return;
		}
		this.AllignCarriedItemToCollider(this.m_BackpackHits, this.m_BackpackHitsCnt, this.m_Collider, false);
	}

	private bool AllignCarriedItemToCraftingTable()
	{
		Collider tableCollider = CraftingManager.Get().m_TableCollider;
		for (int i = 0; i < this.m_BackpackHitsCnt; i++)
		{
			RaycastHit raycastHit = this.m_BackpackHits[i];
			if (!(raycastHit.collider != tableCollider))
			{
				this.m_CarriedItem.gameObject.transform.position = raycastHit.point + this.m_CarriedItem.gameObject.transform.up * this.m_CarriedItem.m_BoxCollider.size.y * this.m_CarriedItem.transform.localScale.y * 0.5f;
				Matrix4x4 identity = Matrix4x4.identity;
				Vector3 vector = -tableCollider.transform.forward;
				identity.SetColumn(1, vector);
				Vector3 v = Vector3.Cross(tableCollider.transform.up, vector);
				identity.SetColumn(0, v);
				identity.SetColumn(2, Vector3.Cross(identity.GetColumn(1), identity.GetColumn(0)));
				identity.SetColumn(0, Vector3.Cross(identity.GetColumn(1), identity.GetColumn(2)));
				Quaternion rotation = CJTools.Math.QuaternionFromMatrix(identity);
				this.m_CarriedItem.gameObject.transform.rotation = rotation;
				if (this.m_CarriedItem.m_Info.m_InventoryRotated)
				{
					this.m_CarriedItem.transform.RotateAround(this.m_CarriedItem.m_BoxCollider.bounds.center, this.m_CarriedItem.transform.up, 90f);
				}
				return true;
			}
		}
		return false;
	}

	private bool AllignCarriedItemToStorage()
	{
		if (!Storage3D.Get().CanInsertItem(this.m_CarriedItem))
		{
			return false;
		}
		Collider plane = Storage3D.Get().m_ActiveData.m_Plane;
		for (int i = 0; i < this.m_BackpackHitsCnt; i++)
		{
			RaycastHit raycastHit = this.m_BackpackHits[i];
			if (!(raycastHit.collider != plane))
			{
				this.m_CarriedItem.gameObject.transform.position = raycastHit.point + this.m_CarriedItem.gameObject.transform.up * this.m_CarriedItem.m_BoxCollider.size.y * this.m_CarriedItem.transform.localScale.y * 0.5f;
				Matrix4x4 identity = Matrix4x4.identity;
				Vector3 vector = -plane.transform.forward;
				identity.SetColumn(1, vector);
				Vector3 v = Vector3.Cross(plane.transform.up, vector);
				identity.SetColumn(0, v);
				identity.SetColumn(2, Vector3.Cross(identity.GetColumn(1), identity.GetColumn(0)));
				identity.SetColumn(0, Vector3.Cross(identity.GetColumn(1), identity.GetColumn(2)));
				Quaternion rotation = CJTools.Math.QuaternionFromMatrix(identity);
				this.m_CarriedItem.gameObject.transform.rotation = rotation;
				if (this.m_CarriedItem.m_Info.m_InventoryRotated)
				{
					this.m_CarriedItem.transform.RotateAround(this.m_CarriedItem.m_BoxCollider.bounds.center, this.m_CarriedItem.transform.up, 90f);
				}
				return true;
			}
		}
		return false;
	}

	private void EnableHiddeObject()
	{
		if (this.m_CarriedItem.m_Info.IsDressing() || this.m_CarriedItem.m_Info.m_ID == ItemID.Fish_Bone || this.m_CarriedItem.m_Info.m_ID == ItemID.Bone_Needle)
		{
			this.m_CarriedItem.gameObject.SetActive(true);
		}
	}

	private bool TryAttractCarriedItemToSlot()
	{
		Vector3 b = Vector3.zero;
		ItemSlot itemSlot = null;
		float num = float.MaxValue;
		foreach (ItemSlot itemSlot2 in ItemSlot.s_ActiveItemSlots)
		{
			if (itemSlot2.CanInsertItem(this.m_CarriedItem))
			{
				b = itemSlot2.GetScreenPoint();
				float num2 = Vector3.Distance(Input.mousePosition, b);
				if (num2 <= itemSlot2.m_AttrRange && num2 <= num && (itemSlot2.IsStack() || itemSlot2.m_BackpackSlot || Vector3.Distance(itemSlot2.GetCheckPosition(), Player.Get().transform.position) <= ItemSlot.s_DistToActivate))
				{
					num = num2;
					itemSlot = itemSlot2;
				}
			}
		}
		if (!itemSlot)
		{
			this.EnableHiddeObject();
			return false;
		}
		this.SetSelectedSlot(itemSlot);
		if (this.m_SelectedSlot.IsStack())
		{
			ItemSlotStack itemSlotStack = (ItemSlotStack)this.m_SelectedSlot;
			this.m_CarriedItem.transform.position = itemSlotStack.m_StackDummies[itemSlotStack.m_Items.Count].transform.position;
			if (itemSlotStack.m_AdjustRotation)
			{
				this.m_CarriedItem.transform.rotation = itemSlotStack.m_StackDummies[itemSlotStack.m_Items.Count].transform.rotation;
			}
		}
		else if (this.m_SelectedSlot.m_BackpackSlot && this.m_CarriedItem.m_InventoryHolder && (this.m_CarriedItem.m_Info.IsWeapon() || this.m_CarriedItem.m_Info.IsTool()))
		{
			Quaternion rhs = Quaternion.Inverse(this.m_CarriedItem.m_InventoryHolder.localRotation);
			this.m_CarriedItem.gameObject.transform.rotation = this.m_SelectedSlot.transform.rotation;
			this.m_CarriedItem.gameObject.transform.rotation *= rhs;
			Vector3 b2 = this.m_CarriedItem.transform.position - this.m_CarriedItem.m_InventoryHolder.position;
			this.m_CarriedItem.gameObject.transform.position = this.m_SelectedSlot.transform.position;
			this.m_CarriedItem.gameObject.transform.position += b2;
		}
		else
		{
			Transform transform = (this.m_SelectedSlot.m_WeaponRackParent != null && this.m_CarriedItem.m_WeaponRackHolder) ? this.m_CarriedItem.m_WeaponRackHolder : this.m_CarriedItem.m_InventoryHolder;
			if (transform)
			{
				Quaternion rhs2 = Quaternion.Inverse(transform.localRotation);
				this.m_CarriedItem.gameObject.transform.rotation = this.m_SelectedSlot.transform.rotation;
				this.m_CarriedItem.gameObject.transform.rotation *= rhs2;
				Vector3 b3 = this.m_CarriedItem.transform.position - transform.position;
				this.m_CarriedItem.gameObject.transform.position = this.m_SelectedSlot.transform.position;
				this.m_CarriedItem.gameObject.transform.position += b3;
			}
			else
			{
				this.m_CarriedItem.gameObject.transform.position = this.m_SelectedSlot.transform.position;
				if (this.m_SelectedSlot.m_AdjustRotation)
				{
					this.m_CarriedItem.gameObject.transform.rotation = this.m_SelectedSlot.transform.rotation;
				}
			}
			if (this.m_SelectedSlot.IsArmorSlot())
			{
				PlayerArmorModule.Get().OnDragItemToSlot((ArmorSlot)this.m_SelectedSlot, this.m_CarriedItem);
			}
		}
		this.m_CarriedItem.m_AttractedByItemSlot = true;
		if (this.m_CarriedItem.m_Info.IsDressing() || this.m_CarriedItem.m_Info.m_ID == ItemID.Fish_Bone || this.m_CarriedItem.m_Info.m_ID == ItemID.Bone_Needle)
		{
			this.m_CarriedItem.gameObject.SetActive(false);
		}
		return true;
	}

	private void SetSelectedSlot(ItemSlot slot)
	{
		if (this.m_SelectedSlot == slot)
		{
			return;
		}
		if (this.m_SelectedSlot && this.m_SelectedSlot.IsArmorSlot())
		{
			PlayerArmorModule.Get().OnRemoveItemFromSlot((ArmorSlot)this.m_SelectedSlot, this.m_CarriedItem);
		}
		this.m_SelectedSlot = slot;
	}

	private void AllignCarriedItemToCollider(RaycastHit[] hits, int hits_cnt, Collider collider, bool update_rotation = false)
	{
		if (!collider)
		{
			return;
		}
		if (hits == null)
		{
			return;
		}
		for (int i = 0; i < hits_cnt; i++)
		{
			RaycastHit raycastHit = hits[i];
			if (!(raycastHit.collider != collider))
			{
				Matrix4x4 identity = Matrix4x4.identity;
				identity.SetColumn(1, raycastHit.normal);
				Vector3 v = Vector3.Cross(collider.transform.up, raycastHit.normal);
				identity.SetColumn(0, v);
				identity.SetColumn(2, Vector3.Cross(identity.GetColumn(1), identity.GetColumn(0)));
				identity.SetColumn(0, Vector3.Cross(identity.GetColumn(1), identity.GetColumn(2)));
				Quaternion rotation = CJTools.Math.QuaternionFromMatrix(identity);
				this.m_CarriedItem.gameObject.transform.rotation = rotation;
				if (this.m_CarriedItem.m_Info.m_InventoryRotated)
				{
					this.m_CarriedItem.transform.RotateAround(this.m_CarriedItem.m_BoxCollider.bounds.center, this.m_CarriedItem.transform.up, 90f);
				}
				ItemSlot itemSlot = InventoryBackpack.Get().FindFreeSlot(this.m_CarriedItem);
				if (itemSlot)
				{
					if (this.m_CarriedItem.m_InventoryHolder)
					{
						Quaternion rhs = Quaternion.Inverse(this.m_CarriedItem.m_InventoryHolder.localRotation);
						this.m_CarriedItem.gameObject.transform.rotation = itemSlot.transform.rotation;
						this.m_CarriedItem.gameObject.transform.rotation *= rhs;
					}
					else if (itemSlot.m_AdjustRotation)
					{
						this.m_CarriedItem.transform.rotation = itemSlot.transform.rotation;
					}
				}
				Vector3 b = this.m_CarriedItem.m_BoxCollider.bounds.center - raycastHit.normal * this.m_CarriedItem.m_BoxCollider.size.y * this.m_CarriedItem.transform.localScale.y * 0.5f;
				this.m_CarriedItem.transform.position = raycastHit.point + (this.m_CarriedItem.transform.position - b);
				return;
			}
		}
	}

	private void UpdateSelectedGroup()
	{
		InventoryCellsGroup selectedGroup = this.m_SelectedGroup;
		if (this.m_SelectedSlot)
		{
			if (this.m_SelectedGroup != null)
			{
				if (this.m_SelectedGroup.m_Pocked == BackpackPocket.Storage)
				{
					Storage3D.Get().OnSetSelectedGroup(null);
				}
				else
				{
					InventoryBackpack.Get().OnSetSelectedGroup(this.m_ActivePocket, null);
				}
				this.m_SelectedGroup = null;
			}
			return;
		}
		if (this.m_MouseOverStorage)
		{
			this.m_SelectedGroup = Storage3D.Get().FindBestGroup();
			if (selectedGroup != this.m_SelectedGroup)
			{
				Storage3D.Get().OnSetSelectedGroup(this.m_SelectedGroup);
			}
			if (this.m_SelectedGroup != null)
			{
				return;
			}
		}
		else if (this.m_SelectedGroup != null && this.m_SelectedGroup.m_Pocked == BackpackPocket.Storage)
		{
			this.m_SelectedGroup = null;
			Storage3D.Get().OnSetSelectedGroup(this.m_SelectedGroup);
		}
		if (!this.m_SelectedSlot && (this.m_ActivePocket == BackpackPocket.Main || this.m_ActivePocket == BackpackPocket.Front))
		{
			this.m_SelectedGroup = InventoryBackpack.Get().FindBestGroup(this.m_ActivePocket);
		}
		else
		{
			this.m_SelectedGroup = null;
		}
		if (selectedGroup != this.m_SelectedGroup)
		{
			InventoryBackpack.Get().OnSetSelectedGroup(this.m_ActivePocket, this.m_SelectedGroup);
		}
	}

	private void SetupStack(Item parent, List<Item> items)
	{
		this.temp_pos.Clear();
		this.temp_rot.Clear();
		foreach (Item item in items)
		{
			if (!this.m_StackItems.Contains(item))
			{
				this.m_StackItems.Add(item);
			}
			item.UpdateLayer();
			if (!this.temp_pos.ContainsKey(item))
			{
				this.temp_pos.Add(item, item.transform.position);
			}
			if (!this.temp_rot.ContainsKey(item))
			{
				this.temp_rot.Add(item, item.transform.rotation);
			}
		}
		foreach (Item item2 in this.m_StackItems)
		{
			if (item2.m_InInventory)
			{
				InventoryBackpack.Get().RemoveItem(item2, false);
			}
			else if (item2.m_Storage)
			{
				item2.m_Storage.RemoveItem(item2, false);
			}
			item2.transform.SetParent(parent.transform);
			item2.transform.rotation = this.temp_rot[item2];
			item2.transform.position = this.temp_pos[item2];
		}
	}

	public void SetCarriedItem(Item item, bool setup_stack = true)
	{
		if (item == null)
		{
			if (this.m_CarriedItem)
			{
				this.m_CarriedItem.StaticPhxRequestRemove();
				if (this.m_CarriedItem.m_AttractedByItemSlot && this.m_CarriedItem.m_Info.IsDressing())
				{
					this.m_CarriedItem.gameObject.SetActive(true);
				}
				this.m_CarriedItem.m_AttractedByItemSlot = false;
				this.m_CarriedItem = null;
			}
			if (setup_stack)
			{
				while (this.m_StackItems.Count > 0)
				{
					Item item2 = this.m_StackItems[0];
					this.m_StackItems.Remove(item2);
					if (!item2.m_InInventory && !item2.m_InStorage)
					{
						item2.transform.parent = null;
					}
					item2.UpdateLayer();
					item2.UpdatePhx();
					item2.UpdateScale(false);
				}
			}
			this.RestoreItem();
		}
		else
		{
			if (setup_stack)
			{
				this.m_StackItems.Clear();
			}
			if (this.m_CarriedItem != null)
			{
				if (this.m_CarriedItem.m_AttractedByItemSlot && this.m_CarriedItem.m_Info.IsDressing())
				{
					this.m_CarriedItem.gameObject.SetActive(true);
				}
				this.m_CarriedItem.m_AttractedByItemSlot = false;
			}
			if (setup_stack && this.ShouldTakeStack() && item.m_CurrentSlot && item.m_CurrentSlot.m_InventoryStackSlot)
			{
				item = item.m_CurrentSlot.m_ItemParent;
			}
			if (!item.m_CurrentSlot && item.m_InventorySlot && item.m_InventorySlot.m_Items.Count > 0)
			{
				if (setup_stack && this.ShouldTakeStack())
				{
					this.SetupStack(item, item.m_InventorySlot.m_Items);
				}
				item.m_InventorySlot.RemoveItem(item, false);
			}
			else if (item.m_CurrentSlot && item.m_CurrentSlot.m_InventoryStackSlot)
			{
				item.m_CurrentSlot.RemoveItem(item, false);
			}
			else if (setup_stack && item.m_CurrentSlot && item.m_CurrentSlot.IsStack() && !item.m_CurrentSlot.m_InventoryStackSlot && this.ShouldTakeStack())
			{
				this.SetupStack(item, ((ItemSlotStack)item.m_CurrentSlot).m_Items);
			}
			this.m_CarriedItem = item;
			this.m_CarriedItem.RestoreDrag();
			this.m_CarriedItem.transform.parent = null;
			this.m_CarriedItem.UpdateScale(false);
			this.m_CarriedItem.StaticPhxRequestAdd();
			this.m_CarriedItem.ReplRequestOwnership(false);
			if (this.m_CarriedItem.m_CurrentSlot)
			{
				this.m_CarriedItem.m_CurrentSlot.RemoveItem();
			}
			if (InventoryBackpack.Get().m_EquippedItem == this.m_CarriedItem)
			{
				InventoryBackpack.Get().m_EquippedItem = null;
			}
			InventoryBackpack.Get().RemoveItem(this.m_CarriedItem, false);
			if (Storage3D.Get().IsActive())
			{
				Storage3D.Get().RemoveItem(this.m_CarriedItem, false);
			}
			Item currentItem = Player.Get().GetCurrentItem();
			if (currentItem == this.m_CarriedItem)
			{
				Player.Get().SetWantedItem(Hand.Right, null, true);
				Player.Get().SetWantedItem(Hand.Left, null, true);
			}
			else if (currentItem && !MakeFireController.Get().IsActive() && this.m_CarriedItem.m_Info.m_CanEquip)
			{
				this.StoreItem(currentItem);
			}
			if (!this.m_CarriedItem.m_WasTriggered)
			{
				this.m_CarriedItem.m_WasTriggered = true;
				this.m_CarriedItem.m_FirstTriggerTime = MainLevel.Instance.m_TODSky.Cycle.GameTime;
			}
			if (this.m_CarriedItem == this.m_NewCraftedItem)
			{
				this.ResetNewCraftedItem();
			}
			this.m_CarriedItem.m_StaticPhx = false;
			if (!this.m_WasRotateItemHint && !HUDHint.Get().IsAnyHintActive())
			{
				HintsManager.Get().ShowHint("RotateItem", 10f);
				this.m_WasRotateItemHint = true;
			}
		}
		this.OnModifyCarriedItem();
	}

	private void StoreItem(Item item)
	{
		this.m_ItemToRestore = item;
		this.m_ItemToRestore.gameObject.SetActive(false);
		Player.Get().SetWantedItem(this.m_ItemToRestore.m_Info.IsBow() ? Hand.Left : Hand.Right, null, true);
	}

	private void RestoreItem()
	{
		if (!this.m_ItemToRestore)
		{
			return;
		}
		Player.Get().SetWantedItem(this.m_ItemToRestore, true);
		this.m_ItemToRestore.gameObject.SetActive(true);
		this.m_ItemToRestore = null;
	}

	private void OnModifyCarriedItem()
	{
		bool flag = this.m_CarriedItem != null;
		bool flag2 = this.m_Collider.enabled != flag;
		if (this.m_Collider.enabled != flag)
		{
			this.m_Collider.enabled = flag;
		}
		if (flag2)
		{
			this.UpdateRaycast();
		}
		this.UpdateCarriedItemPosition();
		this.SetupPocket(this.m_CarriedItem ? this.m_CarriedItem.m_Info.m_BackpackPocket : this.m_ActivePocket);
		if (Storage3D.Get().IsActive())
		{
			Storage3D.Get().SetupGrid();
		}
	}

	public void SetupPocket(BackpackPocket pocket)
	{
		if (pocket == BackpackPocket.None)
		{
			return;
		}
		foreach (Item item in InventoryBackpack.Get().m_Items)
		{
			if (item.m_Info.m_BackpackPocket == pocket)
			{
				item.m_ShownInInventory = true;
			}
		}
		InventoryBackpack.Get().SetupPocket(pocket);
		HUDBackpack.Get().SetupPocket(pocket);
		this.m_ActivePocket = pocket;
	}

	public void DropItem(Item item)
	{
		if (item == Player.Get().GetCurrentItem(Hand.Right))
		{
			Player.Get().SetWantedItem(Hand.Right, null, true);
		}
		if (item == Player.Get().GetCurrentItem(Hand.Left))
		{
			Player.Get().SetWantedItem(Hand.Left, null, true);
		}
		if (item.GetInfoID() == ItemID.Fire)
		{
			UnityEngine.Object.Destroy(item.gameObject);
			return;
		}
		item.transform.rotation = Player.Get().transform.rotation;
		Vector3 pos = this.m_Camera.WorldToViewportPoint(item.transform.position);
		Ray ray = CameraManager.Get().m_MainCamera.ViewportPointToRay(pos);
		RaycastHit raycastHit;
		if (!Physics.Raycast(ray, out raycastHit, this.m_DropDistance))
		{
			item.transform.position = CameraManager.Get().m_MainCamera.transform.position + ray.direction * this.m_DropDistance;
		}
		else
		{
			item.transform.position = raycastHit.point;
		}
		if (item.GetInfoID() == ItemID.PoisonDartFrog_Alive)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(GreenHellGame.Instance.GetPrefab("PoisonDartFrog"), item.transform.position, item.transform.rotation);
			Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(item.gameObject);
			Material material = null;
			for (int i = 0; i < componentsDeepChild.Length; i++)
			{
				material = componentsDeepChild[i].material;
			}
			componentsDeepChild = General.GetComponentsDeepChild<Renderer>(gameObject);
			for (int j = 0; j < componentsDeepChild.Length; j++)
			{
				componentsDeepChild[j].material = material;
			}
			gameObject.GetComponent<DartFrog>().m_MaterialApplied = true;
			UnityEngine.Object.Destroy(item.gameObject);
			return;
		}
		item.m_InPlayersHand = false;
		item.m_AttachedToSpear = false;
		item.StaticPhxRequestReset();
		item.transform.parent = null;
		item.m_BoxCollider.isTrigger = false;
		item.m_Rigidbody.isKinematic = false;
		item.UpdatePhx();
		item.m_Info.m_PrevInventoryCellsGroup = null;
		item.m_PrevSlot = null;
		item.m_Rigidbody.AddForce(ray.direction * this.m_DropForce, ForceMode.Impulse);
		ItemsManager.Get().OnObjectMoved(item.gameObject);
	}

	public void OnLiquidTransfer()
	{
		this.SetCarriedItem(null, true);
		this.SetSelectedSlot(null);
	}

	public void SetNewCraftedItem(Item item)
	{
		this.m_NewCraftedItem = item;
		this.m_NewCraftedItemCreationTime = Time.time;
		item.UpdateLayer();
	}

	private void ResetNewCraftedItem()
	{
		if (this.m_NewCraftedItem)
		{
			this.m_NewCraftedItem.m_ForcedLayer = 0;
			this.m_NewCraftedItem = null;
		}
	}

	private void UpdateNewCraftedItem()
	{
		if (!this.m_NewCraftedItem)
		{
			return;
		}
		float num = Mathf.Sin(Time.time * 20f);
		this.m_NewCraftedItem.m_ForcedLayer = ((num > 0f) ? this.m_NewCraftedItem.m_DefaultLayer : this.m_NewCraftedItem.m_OutlineLayer);
		if (Time.time - this.m_NewCraftedItemCreationTime >= 1.5f)
		{
			this.ResetNewCraftedItem();
		}
	}

	private void PlayDropSound()
	{
		this.m_AudioSource.clip = this.m_DropItemAudioClip;
		this.m_AudioSource.Play();
	}

	private void OnDestroy()
	{
		InputsManager.Get().UnregisterReceiver(this);
	}

	private bool ShouldTakeStack()
	{
		return Input.GetKey(KeyCode.LeftAlt) || this.m_TakeStack;
	}

	public void ResetGrids()
	{
		Storage3D.Get().ResetGrids();
		InventoryBackpack.Get().ResetGrids();
	}

	private void SortItemsBySize()
	{
		InventoryBackpack.Get().SortItemsBySize();
	}

	public GameObject m_Backpack;

	[HideInInspector]
	public BackpackPocket m_ActivePocket;

	[Space(10f)]
	public RawImage m_InventoryImage;

	public Camera m_Camera;

	public Collider m_Collider;

	public Canvas m_Canvas;

	private Item m_CarriedItemProp;

	[HideInInspector]
	public Item m_FocusedItem;

	[HideInInspector]
	public ItemSlot m_SelectedSlot;

	[HideInInspector]
	public InventoryCellsGroup m_SelectedGroup;

	[HideInInspector]
	public Item m_NewCraftedItem;

	private float m_NewCraftedItemCreationTime;

	private bool m_PlayerRotationBlocked;

	public float m_DropForce = 1f;

	public float m_DropDistance;

	private Item m_ItemToRestore;

	private int m_BackpackLayerMask = -1;

	private int m_WorldLayerMask = -1;

	private RaycastHit[] m_WorldHits = new RaycastHit[20];

	private int m_WorldHitsCnt;

	[HideInInspector]
	public RaycastHit[] m_BackpackHits = new RaycastHit[20];

	[HideInInspector]
	public int m_BackpackHitsCnt;

	private bool m_MouseOverBackpack;

	private bool m_MouseOverCraftTable;

	private bool m_MouseOverStorage;

	private static Inventory3DManager s_Instance;

	[HideInInspector]
	public float m_DeactivationTime = -1f;

	private bool m_ActivityChanged;

	private AudioSource m_AudioSource;

	private AudioClip m_DropItemAudioClip;

	[HideInInspector]
	public List<Item> m_StackItems = new List<Item>();

	private bool m_WasRotateItemHint;

	private CompareByDist m_DistComparer = new CompareByDist();

	[HideInInspector]
	public bool m_ScenarioBlocked;

	[HideInInspector]
	public bool m_BlockLMouseUP;

	private bool m_TakeStack;

	public float speed_mull = 0.2f;

	private Dictionary<Item, Vector3> temp_pos = new Dictionary<Item, Vector3>();

	private Dictionary<Item, Quaternion> temp_rot = new Dictionary<Item, Quaternion>();
}
