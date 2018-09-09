using System;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class Inventory3DManager : MonoBehaviour, IInputsReceiver
{
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
			"Outline"
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

	public void Activate()
	{
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
		base.gameObject.SetActive(true);
		this.BlockPlayerRotation(true);
		if (!Player.Get().m_BodyInspectionController.IsActive() && !CraftingManager.Get().gameObject.activeSelf)
		{
			Player.Get().StartController(PlayerControllerType.Inventory);
			if (Player.Get().m_ControllerToStart != PlayerControllerType.Unknown)
			{
				Player.Get().StartControllerInternal();
			}
		}
		this.m_Camera.enabled = true;
		this.m_Canvas.gameObject.SetActive(true);
		CursorManager.Get().ShowCursor(true);
		HUDManager.Get().SetActiveGroup(HUDManager.HUDGroup.Inventory3D);
		this.m_CarriedItem = null;
		this.SetupPocket(this.m_ActivePocket);
		Player.Get().m_BackpackWasOpen = true;
		if (BodyInspectionController.Get().IsActive())
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
		CursorManager.Get().ShowCursor(false);
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		HUDManager.Get().SetActiveGroup(HUDManager.HUDGroup.Game);
		if (CraftingManager.Get().gameObject.activeSelf)
		{
			CraftingManager.Get().Deactivate();
		}
		if (HUDItem.Get().m_Active)
		{
			HUDItem.Get().Deactivate();
		}
		this.ResetNewCraftedItem();
		this.m_SelectedSlot = null;
		this.m_SelectedGroup = null;
		this.m_MouseOverCraftTable = false;
		this.m_MouseOverBackpack = false;
		InventoryBackpack.Get().OnCloseBackpack();
		base.gameObject.SetActive(false);
		this.m_DeactivationTime = Time.time;
		this.m_ActivityChanged = true;
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
		}
		else if (!block && this.m_PlayerRotationBlocked)
		{
			Player.Get().UnblockRotation();
			this.m_PlayerRotationBlocked = false;
		}
	}

	public void CustomUpdate()
	{
		this.UpdateRaycast();
		this.UpdateMouseOver();
		this.UpdateFocusedItem();
		this.UpdateMouseInputs();
		this.UpdateSelectedGroup();
		this.UpdateCarriedItemPosition();
		this.UpdateCursor();
		this.UpdateNewCraftedItem();
		if (Player.Get().IsDead())
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
			}
		}
		else if (this.m_FocusedItem)
		{
			if (CursorManager.Get().GetCursor() != CursorManager.TYPE.Hand_0)
			{
				CursorManager.Get().SetCursor(CursorManager.TYPE.Hand_0);
			}
		}
		else if (CursorManager.Get().GetCursor() != CursorManager.TYPE.Normal)
		{
			CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		}
	}

	private void UpdateRaycast()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.x += 10f;
		float maxDistance = 3f;
		Vector2 zero = Vector2.zero;
		zero.x = mousePosition.x / (float)Screen.width;
		zero.y = mousePosition.y / (float)Screen.height;
		Ray ray = Camera.main.ViewportPointToRay(zero);
		this.m_WorldHits = Physics.RaycastAll(ray, maxDistance, this.m_WorldLayerMask);
		RectTransform rectTransform = this.m_InventoryImage.rectTransform;
		Vector2 zero2 = Vector2.zero;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePosition, null, out zero2);
		zero.x = CJTools.Math.GetProportionalClamp(0f, 1f, zero2.x, -rectTransform.sizeDelta.x * 0.5f, rectTransform.sizeDelta.x * 0.5f);
		zero.y = CJTools.Math.GetProportionalClamp(0f, 1f, zero2.y, -rectTransform.sizeDelta.y * 0.5f, rectTransform.sizeDelta.y * 0.5f);
		ray = this.m_Camera.ViewportPointToRay(zero);
		this.m_BackpackHits = Physics.RaycastAll(ray, maxDistance, this.m_BackpackLayerMask);
	}

	private void UpdateMouseOver()
	{
		this.m_MouseOverCraftTable = false;
		this.m_MouseOverBackpack = false;
		for (int i = 0; i < this.m_BackpackHits.Length; i++)
		{
			if (this.m_BackpackHits[i].collider.gameObject == this.m_Backpack)
			{
				this.m_MouseOverBackpack = true;
			}
			else if (this.m_BackpackHits[i].collider == CraftingManager.Get().m_TableCollider)
			{
				this.m_MouseOverCraftTable = true;
			}
			if (this.m_MouseOverCraftTable && this.m_MouseOverBackpack)
			{
				break;
			}
		}
	}

	private bool CanFocusItem(Item item)
	{
		return item && item.CanBeFocuedInInventory() && !(item == this.m_NewCraftedItem) && (item && item.CanTrigger() && !item.IsItemHold() && item.m_Info != null) && (item.m_Info.m_CanBeFocusedInInventory || item.m_Info.IsHeavyObject() || item.IsItemReplacer());
	}

	private void UpdateFocusedItem()
	{
		this.m_FocusedItem = null;
		if (HUDBackpack.Get().m_IsHovered)
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
		for (int i = 0; i < this.m_BackpackHits.Length; i++)
		{
			Item component = this.m_BackpackHits[i].collider.gameObject.GetComponent<Item>();
			if (this.CanFocusItem(component))
			{
				this.m_FocusedItem = component;
				return;
			}
		}
		if (!this.m_MouseOverBackpack && !this.m_MouseOverCraftTable)
		{
			for (int j = 0; j < this.m_WorldHits.Length; j++)
			{
				Item component2 = this.m_WorldHits[j].collider.gameObject.GetComponent<Item>();
				if (this.CanFocusItem(component2))
				{
					this.m_FocusedItem = component2;
					return;
				}
			}
		}
	}

	public void RotateItem(Item item)
	{
		if (this.m_ActivePocket == BackpackPocket.Left || this.m_ActivePocket == BackpackPocket.Top)
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

	public void OnInputAction(InputsManager.InputAction action)
	{
		if (this.m_InputsBlocked)
		{
			return;
		}
		if (!base.gameObject.activeSelf && action == InputsManager.InputAction.ShowInventory)
		{
			this.Activate();
		}
		else if (!this.m_ActivityChanged && base.gameObject.activeSelf && (action == InputsManager.InputAction.HideInventory || action == InputsManager.InputAction.ShowInventory))
		{
			if (CraftingManager.Get().gameObject.activeSelf)
			{
				CraftingManager.Get().Deactivate();
			}
			else
			{
				this.Deactivate();
			}
		}
	}

	private void UpdateMouseInputs()
	{
		if (Input.GetMouseButtonDown(0))
		{
			this.OnLMouseDown();
		}
		if (Input.GetMouseButtonUp(0))
		{
			this.OnLMouseUp();
		}
		if (Input.GetMouseButtonDown(1))
		{
			this.OnRMouseDown();
		}
		if (Input.GetMouseButtonUp(1))
		{
			this.OnRMouseUp();
		}
	}

	private void OnLMouseDown()
	{
		if (!this.m_FocusedItem || HUDBackpack.Get().m_IsHovered || this.m_FocusedItem.m_Info.m_CantBeDraggedInInventory)
		{
			return;
		}
		this.SetCarriedItem(this.m_FocusedItem);
		if (CraftingManager.Get().gameObject.activeSelf && CraftingManager.Get().ContainsItem(this.m_FocusedItem))
		{
			CraftingManager.Get().RemoveItem(this.m_FocusedItem);
		}
		if (this.m_FocusedItem == Player.Get().GetCurrentItem(Hand.Right))
		{
			Player.Get().SetWantedItem(Hand.Right, null, true);
		}
		else if (this.m_FocusedItem == Player.Get().GetCurrentItem(Hand.Left))
		{
			Player.Get().SetWantedItem(Hand.Left, null, true);
		}
	}

	private bool CanInsertCarriedItemToBackpack()
	{
		return this.m_CarriedItem && this.m_CarriedItem.m_Info.m_CanBeAddedToInventory && ((this.m_SelectedSlot && this.m_SelectedSlot.m_BackpackSlot) || this.m_SelectedGroup != null);
	}

	private void OnLMouseUp()
	{
		if (!this.m_CarriedItem)
		{
			return;
		}
		if (this.CanInsertCarriedItemToBackpack())
		{
			InventoryBackpack.InsertResult insertResult = InventoryBackpack.Get().InsertItem(this.m_CarriedItem, this.m_SelectedSlot, this.m_SelectedGroup, false, false, false, false, true);
			if (insertResult != InventoryBackpack.InsertResult.Ok)
			{
				insertResult = InventoryBackpack.Get().InsertItem(this.m_CarriedItem, this.m_CarriedItem.m_PrevSlot, this.m_CarriedItem.m_Info.m_PrevInventoryCellsGroup, false, true, true, false, true);
			}
			if (insertResult == InventoryBackpack.InsertResult.Ok)
			{
				this.m_CarriedItem.m_ShownInInventory = true;
				this.PlayDropSound();
			}
		}
		else if (CraftingManager.Get().gameObject.activeSelf && this.m_MouseOverCraftTable)
		{
			CraftingManager.Get().AddItem(this.m_CarriedItem, false);
			this.PlayDropSound();
		}
		else if (this.m_CarriedItem.m_Info.m_CanBeRemovedFromInventory)
		{
			if (BodyInspectionController.Get().IsActive())
			{
				if (this.m_SelectedSlot && this.m_SelectedSlot.IsBIWoundSlot())
				{
					this.m_SelectedSlot.InsertItem(this.m_CarriedItem);
				}
				else
				{
					this.DropItem(this.m_CarriedItem);
				}
				this.PlayDropSound();
			}
			else if (this.m_SelectedSlot)
			{
				this.m_SelectedSlot.InsertItem(this.m_CarriedItem);
				this.PlayDropSound();
			}
			else if (this.m_MouseOverBackpack)
			{
				InventoryBackpack.Get().InsertItem(this.m_CarriedItem, null, null, true, true, true, true, true);
				this.PlayDropSound();
			}
			else
			{
				this.DropItem(this.m_CarriedItem);
				this.PlayDropSound();
			}
		}
		this.SetCarriedItem(null);
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
			this.RotateItem(this.m_CarriedItem);
		}
		else if (this.m_FocusedItem)
		{
			if (!this.m_FocusedItem.m_OnCraftingTable)
			{
				HUDItem.Get().Activate(this.m_FocusedItem);
			}
			else
			{
				CraftingManager.Get().RemoveItem(this.m_FocusedItem);
				if (!this.m_FocusedItem.Take())
				{
					this.DropItem(this.m_FocusedItem);
				}
				this.m_FocusedItem = null;
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
		this.m_SelectedSlot = null;
		if (!this.m_CarriedItem)
		{
			return;
		}
		if (this.TryAttractCarriedItemToSlot())
		{
			return;
		}
		if (this.m_SelectedGroup != null)
		{
			this.AllignCarriedItemToCollider(this.m_BackpackHits, InventoryBackpack.Get().m_ActivePlane, true);
			return;
		}
		if (CraftingManager.Get().gameObject.activeSelf && this.AllignCarriedItemToCraftingTable())
		{
			return;
		}
		this.AllignCarriedItemToCollider(this.m_BackpackHits, this.m_Collider, false);
	}

	private bool AllignCarriedItemToCraftingTable()
	{
		Collider tableCollider = CraftingManager.Get().m_TableCollider;
		for (int i = 0; i < this.m_BackpackHits.Length; i++)
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
				if (num2 <= itemSlot2.m_AttrRange)
				{
					if (num2 <= num)
					{
						num = num2;
						itemSlot = itemSlot2;
					}
				}
			}
		}
		if (!itemSlot)
		{
			this.EnableHiddeObject();
			return false;
		}
		this.m_SelectedSlot = itemSlot;
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
			this.m_CarriedItem.gameObject.transform.position = this.m_SelectedSlot.transform.position;
			if (this.m_SelectedSlot.m_AdjustRotation)
			{
				this.m_CarriedItem.gameObject.transform.rotation = this.m_SelectedSlot.transform.rotation;
			}
		}
		this.m_CarriedItem.m_AttractedByItemSlot = true;
		if (this.m_CarriedItem.m_Info.IsDressing() || this.m_CarriedItem.m_Info.m_ID == ItemID.Fish_Bone || this.m_CarriedItem.m_Info.m_ID == ItemID.Bone_Needle)
		{
			this.m_CarriedItem.gameObject.SetActive(false);
		}
		return true;
	}

	private void AllignCarriedItemToCollider(RaycastHit[] hits, Collider collider, bool update_rotation = false)
	{
		if (!collider)
		{
			return;
		}
		if (hits == null)
		{
			return;
		}
		foreach (RaycastHit raycastHit in hits)
		{
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
				break;
			}
		}
	}

	private void UpdateSelectedGroup()
	{
		InventoryCellsGroup selectedGroup = this.m_SelectedGroup;
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

	public void SetCarriedItem(Item item)
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
			this.RestoreItem();
		}
		else
		{
			if (this.m_CarriedItem != null)
			{
				if (this.m_CarriedItem.m_AttractedByItemSlot && this.m_CarriedItem.m_Info.IsDressing())
				{
					this.m_CarriedItem.gameObject.SetActive(true);
				}
				this.m_CarriedItem.m_AttractedByItemSlot = false;
			}
			if (!item.m_CurrentSlot && item.m_InventorySlot && item.m_InventorySlot.m_Items.Count > 0)
			{
				item.m_InventorySlot.RemoveItem(item, false);
			}
			else if (item.m_CurrentSlot && item.m_CurrentSlot.m_InventoryStackSlot)
			{
				item.m_CurrentSlot.RemoveItem(item, false);
			}
			this.m_CarriedItem = item;
			this.m_CarriedItem.UpdateScale(false);
			this.m_CarriedItem.StaticPhxRequestAdd();
			if (this.m_CarriedItem.m_CurrentSlot)
			{
				this.m_CarriedItem.m_CurrentSlot.RemoveItem();
			}
			if (InventoryBackpack.Get().m_EquippedItem == this.m_CarriedItem)
			{
				InventoryBackpack.Get().m_EquippedItem = null;
			}
			InventoryBackpack.Get().RemoveItem(this.m_CarriedItem, false);
			Item currentItem = Player.Get().GetCurrentItem();
			if (currentItem == this.m_CarriedItem)
			{
				Player.Get().SetWantedItem(Hand.Right, null, true);
				Player.Get().SetWantedItem(Hand.Left, null, true);
			}
			else if (currentItem && this.m_CarriedItem.m_Info.m_CanEquip)
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
		}
		this.OnModifyCarriedItem();
	}

	private void StoreItem(Item item)
	{
		this.m_ItemToRestore = item;
		this.m_ItemToRestore.gameObject.SetActive(false);
		Player.Get().SetWantedItem((!this.m_ItemToRestore.m_Info.IsBow()) ? Hand.Right : Hand.Left, null, true);
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
		this.m_Collider.enabled = (this.m_CarriedItem != null);
		this.UpdateCarriedItemPosition();
		this.SetupPocket((!this.m_CarriedItem) ? this.m_ActivePocket : this.m_CarriedItem.m_Info.m_BackpackPocket);
	}

	public void SetupPocket(BackpackPocket pocket)
	{
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
		item.StaticPhxRequestReset();
		item.transform.parent = null;
		item.m_BoxCollider.isTrigger = false;
		item.m_Rigidbody.isKinematic = false;
		item.m_Info.m_PrevInventoryCellsGroup = null;
		item.m_PrevSlot = null;
		item.transform.rotation = Player.Get().transform.rotation;
		Vector3 position = new Vector3(0.5f, 0.5f, this.m_DropDistance);
		position.x = System.Math.Max(0.5f, position.x);
		Vector3 vector = Camera.main.ViewportToWorldPoint(position);
		item.transform.position = vector;
		Vector3 normalized = (vector - Camera.main.transform.position).normalized;
		item.m_Rigidbody.AddForce(normalized * this.m_DropForce, ForceMode.Impulse);
	}

	public void OnLiquidTransfer()
	{
		this.SetCarriedItem(null);
		this.m_SelectedSlot = null;
	}

	public void SetNewCraftedItem(Item item)
	{
		this.m_NewCraftedItem = item;
		this.m_NewCraftedItemCreationTime = Time.time;
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
		this.m_NewCraftedItem.m_ForcedLayer = ((num <= 0f) ? this.m_NewCraftedItem.m_OutlineLayer : this.m_NewCraftedItem.m_DefaultLayer);
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

	public GameObject m_Backpack;

	[HideInInspector]
	public BackpackPocket m_ActivePocket;

	[Space(10f)]
	public RawImage m_InventoryImage;

	public Camera m_Camera;

	public Collider m_Collider;

	public Canvas m_Canvas;

	[HideInInspector]
	public Item m_CarriedItem;

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

	private RaycastHit[] m_WorldHits;

	private RaycastHit[] m_BackpackHits;

	private bool m_MouseOverBackpack;

	private bool m_MouseOverCraftTable;

	private static Inventory3DManager s_Instance;

	[HideInInspector]
	public float m_DeactivationTime = -1f;

	[HideInInspector]
	public bool m_InputsBlocked;

	private bool m_ActivityChanged;

	private AudioSource m_AudioSource;

	private AudioClip m_DropItemAudioClip;
}
