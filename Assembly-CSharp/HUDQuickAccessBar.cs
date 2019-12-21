using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDQuickAccessBar : HUDBase
{
	public static HUDQuickAccessBar Get()
	{
		return HUDQuickAccessBar.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDQuickAccessBar.s_Instance = this;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
		base.AddToGroup(HUDManager.HUDGroup.InspectionMinigame);
		base.AddToGroup(HUDManager.HUDGroup.ItemsCombine);
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override bool ShouldShow()
	{
		return false;
	}

	public void OnPointerEnter(GameObject obj)
	{
		bool active = true;
		if (Player.Get().m_InspectionBlocked && obj == this.m_Inspect.gameObject)
		{
			active = false;
		}
		obj.transform.GetChild(0).gameObject.SetActive(active);
		this.m_ButtonSelectedCount++;
	}

	public void OnPointerExit(GameObject obj)
	{
		obj.transform.GetChild(0).gameObject.SetActive(false);
		this.m_ButtonSelectedCount--;
	}

	protected override void Update()
	{
		base.Update();
		this.SetupSelections();
	}

	private void SetupSelections()
	{
		Color color = this.m_Craft.color;
		color.a = ((!Player.Get().CanStartCrafting()) ? this.m_InactiveAlpha : (CraftingManager.Get().gameObject.activeSelf ? this.m_SelectedAlpha : this.m_NormalAlpha));
		this.m_Craft.color = color;
		color = this.m_Notepad.color;
		color.a = ((!Player.Get().CanShowNotepad()) ? this.m_InactiveAlpha : (NotepadController.Get().IsActive() ? this.m_SelectedAlpha : this.m_NormalAlpha));
		this.m_Notepad.color = color;
		color = this.m_Backpack.color;
		color.a = (Inventory3DManager.Get().gameObject.activeSelf ? this.m_SelectedAlpha : this.m_NormalAlpha);
		this.m_Backpack.color = color;
		color = this.m_Inspect.color;
		color.a = ((!Player.Get().CanStartBodyInspection()) ? this.m_InactiveAlpha : (BodyInspectionController.Get().IsActive() ? this.m_SelectedAlpha : this.m_NormalAlpha));
		this.m_Inspect.color = color;
	}

	public void OnHideNotepad()
	{
		if (this.m_DelayedType != HUDQuickAccessBar.TYPE.None)
		{
			switch (this.m_DelayedType)
			{
			case HUDQuickAccessBar.TYPE.Craft:
				this.OnCraft(this.m_DelayedObj);
				break;
			case HUDQuickAccessBar.TYPE.Backpack:
				this.OnBackpack(this.m_DelayedObj);
				break;
			case HUDQuickAccessBar.TYPE.Inspect:
				this.OnInspection(this.m_DelayedObj);
				break;
			}
			this.m_DelayedObj = null;
			this.m_DelayedType = HUDQuickAccessBar.TYPE.None;
		}
	}

	public void OnCraft(GameObject obj)
	{
		if (!Player.Get().CanStartCrafting())
		{
			return;
		}
		if (NotepadController.Get().IsActive())
		{
			NotepadController.Get().Hide();
			this.m_DelayedType = HUDQuickAccessBar.TYPE.Craft;
			this.m_DelayedObj = obj;
			return;
		}
		if (BodyInspectionController.Get().IsActive())
		{
			Player.Get().StopController(PlayerControllerType.BodyInspection);
		}
		if (!Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().Activate();
		}
		if (!CraftingManager.Get().gameObject.activeSelf)
		{
			CraftingManager.Get().Activate();
			return;
		}
		CraftingManager.Get().Deactivate();
	}

	public void OnNotepad(GameObject obj)
	{
		if (!Player.Get().CanShowNotepad())
		{
			return;
		}
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().Deactivate();
		}
		if (BodyInspectionController.Get().IsActive())
		{
			Player.Get().StopController(PlayerControllerType.BodyInspection);
		}
		if (!NotepadController.Get().IsActive())
		{
			Player.Get().StartController(PlayerControllerType.Notepad);
			return;
		}
		NotepadController.Get().Hide();
	}

	public void OnBackpack(GameObject obj)
	{
		if (NotepadController.Get().IsActive())
		{
			NotepadController.Get().Hide();
			this.m_DelayedType = HUDQuickAccessBar.TYPE.Backpack;
			this.m_DelayedObj = obj;
			return;
		}
		if (!Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().Activate();
			return;
		}
		Inventory3DManager.Get().Deactivate();
	}

	public void OnInspection(GameObject obj)
	{
		if (!Player.Get().CanStartBodyInspection())
		{
			return;
		}
		if (NotepadController.Get().IsActive())
		{
			NotepadController.Get().Hide();
			this.m_DelayedType = HUDQuickAccessBar.TYPE.Inspect;
			this.m_DelayedObj = obj;
			return;
		}
		if (CraftingManager.Get().gameObject.activeSelf)
		{
			CraftingManager.Get().Deactivate();
		}
		if (!BodyInspectionController.Get().IsActive())
		{
			Player.Get().StartController(PlayerControllerType.BodyInspection);
			return;
		}
		Player.Get().StopController(PlayerControllerType.BodyInspection);
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.m_Craft.transform.GetChild(0).gameObject.SetActive(false);
		this.m_Notepad.transform.GetChild(0).gameObject.SetActive(false);
		this.m_Backpack.transform.GetChild(0).gameObject.SetActive(false);
		this.m_Inspect.transform.GetChild(0).gameObject.SetActive(false);
		this.m_ButtonSelectedCount = 0;
	}

	public bool AnyButtonSelected()
	{
		return this.m_ButtonSelectedCount > 0;
	}

	private HUDQuickAccessBar.TYPE m_DelayedType;

	private GameObject m_DelayedObj;

	public RawImage m_Craft;

	public RawImage m_Notepad;

	public RawImage m_Backpack;

	public RawImage m_Inspect;

	public float m_NormalAlpha;

	public float m_SelectedAlpha;

	public float m_InactiveAlpha = 0.1f;

	private int m_ButtonSelectedCount;

	private static HUDQuickAccessBar s_Instance;

	private enum TYPE
	{
		None,
		Craft,
		Notepad,
		Backpack,
		Inspect
	}
}
