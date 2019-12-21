using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDItemSlot : HUDBase
{
	public static HUDItemSlot Get()
	{
		return HUDItemSlot.s_Instance;
	}

	protected override void Awake()
	{
		HUDItemSlot.s_Instance = this;
		base.Awake();
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override bool ShouldShow()
	{
		return this.m_ActiveSlots.Count > 0 && !MapController.Get().IsActive() && !NotepadController.Get().IsActive() && !ScenarioManager.Get().IsDreamOrPreDream();
	}

	public void RegisterSlot(ItemSlot slot)
	{
		if (this.m_IsBeingDestroyed)
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_ItemSlotPrefab, base.transform);
		SlotData slotData = new SlotData();
		slotData.obj = gameObject;
		slotData.slot = slot;
		slotData.icon = gameObject.transform.Find("Icon").gameObject.GetComponent<RawImage>();
		slotData.add_icon = slotData.icon.transform.Find("AddIcon").gameObject.GetComponent<Image>();
		if (slotData.slot.m_AddIcon.Length > 0)
		{
			Sprite sprite = null;
			ItemsManager.Get().m_ItemIconsSprites.TryGetValue(slotData.slot.m_AddIcon, out sprite);
			slotData.add_icon.sprite = sprite;
		}
		else
		{
			slotData.add_icon.gameObject.SetActive(false);
		}
		this.m_ActiveSlots.Add(slotData);
	}

	public void UnregisterSlot(ItemSlot slot)
	{
		foreach (SlotData slotData in this.m_ActiveSlots)
		{
			if (slotData.slot == slot)
			{
				UnityEngine.Object.Destroy(slotData.obj);
				this.m_ActiveSlots.Remove(slotData);
				break;
			}
		}
	}

	public override void UpdateAfterCamera()
	{
		base.UpdateAfterCamera();
		if (this.m_ActiveSlots.Count == 0 || !base.enabled)
		{
			return;
		}
		this.m_ClosestDistTemp = float.MaxValue;
		SlotData selectedSlotData = this.m_SelectedSlotData;
		this.m_SelectedSlotData = null;
		this.m_VisibleSlots.Clear();
		foreach (SlotData slotData in this.m_ActiveSlots)
		{
			if (slotData.slot.IsBIWoundSlot())
			{
				this.UpdateWoundSlots(slotData);
			}
			else
			{
				this.UpdateSlots(slotData);
			}
		}
		if (this.m_SelectedSlotData == null && Inventory3DManager.Get().IsActive() && Inventory3DManager.Get().m_ActivePocket != BackpackPocket.Left && Player.Get().GetCurrentItem(Hand.Right) && Player.Get().GetCurrentItem(Hand.Right).m_Info.IsFishingRod())
		{
			FishingRod component = Player.Get().GetCurrentItem(Hand.Right).gameObject.GetComponent<FishingRod>();
			ItemSlot y;
			if (!component.m_Hook)
			{
				y = component.m_HookSlot;
			}
			else
			{
				y = component.m_Hook.m_BaitSlot;
			}
			foreach (SlotData slotData2 in this.m_VisibleSlots)
			{
				if (slotData2.slot == y)
				{
					this.m_SelectedSlotData = slotData2;
					break;
				}
			}
		}
		if (this.m_SelectedSlotData != null)
		{
			this.m_SelectedSlotData.icon.rectTransform.localScale = Vector2.one * 2f;
			if (this.m_SelectedSlotData.add_icon)
			{
				this.m_SelectedSlotData.add_icon.rectTransform.localScale = Vector2.one * 0.5f;
			}
			Color color = this.m_SelectedSlotData.icon.color;
			color.a *= 1.5f;
			this.m_SelectedSlotData.icon.color = color;
		}
		if (this.m_SelectedSlotData != selectedSlotData)
		{
			HUDItem.Get().OnChangeSelectedSlot(this.m_SelectedSlotData);
		}
	}

	private void UpdateWoundSlots(SlotData data)
	{
		if (!Inventory3DManager.Get().isActiveAndEnabled || !BodyInspectionController.Get().IsActive())
		{
			data.obj.gameObject.SetActive(false);
			return;
		}
		if (GreenHellGame.IsPCControllerActive() && (Inventory3DManager.Get().m_CarriedItem == null || !data.slot.CanInsertItem(Inventory3DManager.Get().m_CarriedItem)))
		{
			data.obj.gameObject.SetActive(false);
			return;
		}
		BIWoundSlot biwoundSlot = (BIWoundSlot)data.slot;
		if (biwoundSlot == null || biwoundSlot.m_Injury == null)
		{
			data.obj.gameObject.SetActive(false);
			return;
		}
		if (biwoundSlot.m_Injury.m_Type == InjuryType.Leech)
		{
			data.obj.gameObject.SetActive(false);
			return;
		}
		if (!data.obj.gameObject.activeSelf)
		{
			data.obj.gameObject.SetActive(true);
		}
		Vector3 screenPoint = data.slot.GetScreenPoint();
		if (screenPoint.z <= 0f)
		{
			data.icon.enabled = false;
			return;
		}
		data.icon.rectTransform.position = screenPoint;
		if (biwoundSlot.GetInjury() != null && BodyInspectionController.Get() != null && BodyInspectionController.Get().enabled && EnumTools.ConvertInjuryPlaceToLimb(biwoundSlot.GetInjury().m_Place) == HUDBodyInspection.Get().GetSelectedLimb() && biwoundSlot.GetInjury().m_Bandage == null && biwoundSlot.m_Maggots == null && biwoundSlot.GetInjury().m_ParentInjury == null)
		{
			data.icon.enabled = true;
			this.m_SelectedSlotData = data;
			return;
		}
		data.icon.enabled = false;
	}

	private void UpdateSlots(SlotData data)
	{
		bool flag = !Inventory3DManager.Get().m_CarriedItem || data.slot.CanInsertItem(Inventory3DManager.Get().m_CarriedItem);
		data.obj.gameObject.SetActive(flag);
		if (!flag)
		{
			return;
		}
		if (!data.slot.enabled)
		{
			data.icon.gameObject.SetActive(false);
			return;
		}
		Vector3 screenPoint = data.slot.GetScreenPoint();
		if (screenPoint.z <= 0f)
		{
			data.icon.gameObject.SetActive(false);
			return;
		}
		data.icon.rectTransform.position = screenPoint;
		if (data.slot.IsOccupied())
		{
			data.icon.gameObject.SetActive(false);
			return;
		}
		if (data.slot.IsArmorSlot())
		{
			if (!BodyInspectionController.Get().IsActive())
			{
				data.icon.gameObject.SetActive(false);
				return;
			}
			if (!Inventory3DManager.Get().IsActive() || Inventory3DManager.Get().m_CarriedItem == null || !Inventory3DManager.Get().m_CarriedItem.m_Info.IsArmor())
			{
				data.icon.gameObject.SetActive(false);
				return;
			}
		}
		Color color;
		if (Inventory3DManager.Get().m_SelectedSlot == data.slot)
		{
			color = this.m_SelectedColor;
		}
		else
		{
			color = this.m_NormalColor;
			if (data.slot.m_ItemParent && (data.slot.m_ItemParent.m_InInventory || data.slot.m_ItemParent.m_InStorage))
			{
				color.a = 1f;
			}
			else if (!data.slot.m_BackpackSlot)
			{
				float b = Vector3.Distance(data.slot.GetCheckPosition(), Player.Get().transform.position);
				color.a = CJTools.Math.GetProportionalClamp(0f, 0.6f, b, ItemSlot.s_DistToActivate, ItemSlot.s_DistToActivate * 0.5f);
			}
		}
		if (color.a <= 0f)
		{
			data.icon.gameObject.SetActive(false);
			return;
		}
		data.icon.color = color;
		data.icon.gameObject.SetActive(true);
		data.icon.rectTransform.localScale = Vector2.one;
		Vector2 zero = Vector2.zero;
		zero.Set(screenPoint.x, screenPoint.y);
		Vector2 zero2 = Vector2.zero;
		zero2.Set((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
		float num = Vector2.Distance(zero, zero2);
		if (data.slot.m_CanSelect && color.a > 0f && num < this.m_MaxDistToSelect && num < this.m_ClosestDistTemp)
		{
			this.m_SelectedSlotData = data;
			this.m_ClosestDistTemp = num;
		}
		this.m_VisibleSlots.Add(data);
	}

	private void OnDestroy()
	{
		this.m_IsBeingDestroyed = true;
	}

	public GameObject m_ItemSlotPrefab;

	private List<SlotData> m_ActiveSlots = new List<SlotData>();

	private List<SlotData> m_VisibleSlots = new List<SlotData>();

	private static HUDItemSlot s_Instance;

	public Color m_NormalColor = Color.white;

	public Color m_SelectedColor = Color.green;

	private bool m_IsBeingDestroyed;

	[HideInInspector]
	public SlotData m_SelectedSlotData;

	private float m_ClosestDistTemp = float.MaxValue;

	private float m_MaxDistToSelect = 50f;
}
