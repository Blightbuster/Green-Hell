using System;
using System.Collections.Generic;
using CJTools;
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
		return this.m_ActiveSlots.Count > 0 && !MapController.Get().IsActive() && !NotepadController.Get().IsActive();
	}

	public void RegisterSlot(ItemSlot slot)
	{
		if (this.m_IsBeingDestroyed)
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_ItemSlotPrefab, base.transform);
		HUDItemSlot.SlotData item = default(HUDItemSlot.SlotData);
		item.obj = gameObject;
		item.slot = slot;
		item.icon = gameObject.transform.Find("Icon").gameObject.GetComponent<RawImage>();
		item.add_icon = item.icon.transform.Find("AddIcon").gameObject.GetComponent<Image>();
		if (item.slot.m_AddIcon.Length > 0)
		{
			Sprite sprite = null;
			ItemsManager.Get().m_ItemIconsSprites.TryGetValue(item.slot.m_AddIcon, out sprite);
			item.add_icon.sprite = sprite;
		}
		else
		{
			item.add_icon.gameObject.SetActive(false);
		}
		this.m_ActiveSlots.Add(item);
	}

	public void UnregisterSlot(ItemSlot slot)
	{
		foreach (HUDItemSlot.SlotData item in this.m_ActiveSlots)
		{
			if (item.slot == slot)
			{
				UnityEngine.Object.Destroy(item.obj);
				this.m_ActiveSlots.Remove(item);
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
		foreach (HUDItemSlot.SlotData data in this.m_ActiveSlots)
		{
			if (data.slot.IsBIWoundSlot())
			{
				this.UpdateWoundSlots(data);
			}
			else
			{
				this.UpdateSlots(data);
			}
		}
	}

	private void UpdateWoundSlots(HUDItemSlot.SlotData data)
	{
		bool flag = Inventory3DManager.Get().isActiveAndEnabled && BodyInspectionController.Get().IsActive() && (!Inventory3DManager.Get().m_CarriedItem || data.slot.CanInsertItem(Inventory3DManager.Get().m_CarriedItem));
		data.obj.gameObject.SetActive(flag);
		if (!flag)
		{
			return;
		}
		Vector3 screenPoint = data.slot.GetScreenPoint();
		if (screenPoint.z <= 0f)
		{
			data.icon.enabled = false;
			return;
		}
		data.icon.rectTransform.position = screenPoint;
		BIWoundSlot biwoundSlot = (BIWoundSlot)data.slot;
		if (biwoundSlot.GetInjury() != null && BodyInspectionController.Get() != null && BodyInspectionController.Get().enabled && biwoundSlot.GetInjury().m_Bandage == null && biwoundSlot.m_Maggots == null && biwoundSlot.GetInjury().m_ParentInjury == null)
		{
			data.icon.enabled = true;
			data.icon.color = ((!(Inventory3DManager.Get().m_SelectedSlot == data.slot)) ? this.m_NormalColor : this.m_SelectedColor);
		}
		else
		{
			data.icon.enabled = false;
		}
	}

	private void UpdateSlots(HUDItemSlot.SlotData data)
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
		Color color;
		if (Inventory3DManager.Get().m_SelectedSlot == data.slot)
		{
			color = this.m_SelectedColor;
		}
		else
		{
			color = this.m_NormalColor;
			if (data.slot.m_ItemParent && data.slot.m_ItemParent.m_InInventory)
			{
				color.a = 1f;
			}
			else if (!data.slot.m_BackpackSlot)
			{
				float b = Vector3.Distance(data.slot.GetCheckPosition(), Player.Get().transform.position);
				color.a = CJTools.Math.GetProportionalClamp(0f, 0.6f, b, ItemSlot.s_DistToActivate, ItemSlot.s_DistToActivate * 0.5f);
			}
		}
		data.icon.color = color;
		data.icon.gameObject.SetActive(true);
	}

	private void OnDestroy()
	{
		this.m_IsBeingDestroyed = true;
	}

	public GameObject m_ItemSlotPrefab;

	private List<HUDItemSlot.SlotData> m_ActiveSlots = new List<HUDItemSlot.SlotData>();

	private static HUDItemSlot s_Instance;

	public Color m_NormalColor = Color.white;

	public Color m_SelectedColor = Color.green;

	private bool m_IsBeingDestroyed;

	private struct SlotData
	{
		public GameObject obj;

		public ItemSlot slot;

		public RawImage icon;

		public Image add_icon;
	}
}
