using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDBackpack : HUDBase, IInputsReceiver
{
	public bool m_IsHovered { get; protected set; }

	public static HUDBackpack Get()
	{
		return HUDBackpack.s_Instance;
	}

	protected override void Awake()
	{
		HUDBackpack.s_Instance = this;
		this.m_IsHovered = false;
		RawImage[] componentsInChildren = base.transform.FindDeepChild("Pockets").gameObject.GetComponentsInChildren<RawImage>();
		this.m_PocketImages = new List<PocketImageData>();
		foreach (RawImage rawImage in componentsInChildren)
		{
			if (!(rawImage.name != BackpackPocket.Front.ToString()) || !(rawImage.name != BackpackPocket.Main.ToString()) || !(rawImage.name != BackpackPocket.Left.ToString()) || !(rawImage.name != BackpackPocket.Right.ToString()) || !(rawImage.name != BackpackPocket.Top.ToString()))
			{
				PocketImageData pocketImageData = new PocketImageData();
				pocketImageData.pocket = (BackpackPocket)Enum.Parse(typeof(BackpackPocket), rawImage.name);
				pocketImageData.icon = rawImage;
				pocketImageData.selection = rawImage.transform.Find("Selection").gameObject;
				pocketImageData.selection.SetActive(false);
				pocketImageData.new_count_bg = rawImage.transform.Find("BG").GetComponent<Image>();
				pocketImageData.new_count_text = rawImage.transform.GetComponentInChildren<Text>();
				this.m_PocketImages.Add(pocketImageData);
			}
		}
		GameObject gameObject = new GameObject("EquippedIcon");
		this.m_EquippedIcon = gameObject.AddComponent<Image>();
		Sprite sprite = null;
		ItemsManager.Get().m_ItemIconsSprites.TryGetValue("Default_Pickup", out sprite);
		DebugUtils.Assert(sprite, true);
		this.m_EquippedIcon.sprite = sprite;
		this.m_EquippedIcon.rectTransform.sizeDelta = new Vector2(20f, 20f);
		gameObject.GetComponent<RectTransform>().SetParent(base.transform);
		gameObject.SetActive(true);
		this.m_BG = base.transform.Find("BG").gameObject.GetComponent<RawImage>();
		this.InitializeSounds();
		for (int j = 0; j < base.transform.childCount; j++)
		{
			base.transform.GetChild(j).gameObject.SetActive(false);
		}
		base.enabled = false;
		if (this.m_PADChangeTabs.Count > 0)
		{
			this.m_PADChangeTabDefaultAlpha = this.m_PADChangeTabs[0].color.a;
		}
	}

	private void InitializeSounds()
	{
		for (int i = 0; i < 2; i++)
		{
			AudioClip item = Resources.Load("Sounds/UI/Backpack/ui_backpack_open_0" + (i + 1).ToString()) as AudioClip;
			this.m_OpenSounds.Add(item);
		}
		for (int j = 0; j < 2; j++)
		{
			AudioClip item2 = Resources.Load("Sounds/UI/Backpack/ui_backpack_close_0" + (j + 1).ToString()) as AudioClip;
			this.m_CloseSounds.Add(item2);
		}
		for (int k = 0; k < 4; k++)
		{
			AudioClip item3 = Resources.Load("Sounds/UI/Backpack/ui_backpack_tab_switch_0" + (k + 1).ToString()) as AudioClip;
			this.m_ChangeTabSounds.Add(item3);
		}
	}

	private void SetupSelections()
	{
		foreach (PocketImageData pocketImageData in this.m_PocketImages)
		{
			pocketImageData.selection.SetActive(false);
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.SetupSelections();
		this.PlayOpenSound();
		this.SetupController();
		this.m_PadQuitHint.SetActive(GreenHellGame.IsPadControllerActive() && !BodyInspectionController.Get().IsActive());
		this.m_PadSortHint.SetActive(GreenHellGame.IsPadControllerActive() && !BodyInspectionController.Get().IsActive());
	}

	protected override void OnHide()
	{
		base.OnHide();
		this.m_IsHovered = false;
		this.PlayCloseSound();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.SetupSelections();
	}

	private void PlayOpenSound()
	{
		if (HUDManager.Get() == null)
		{
			return;
		}
		if (this.m_OpenSounds.Count == 0)
		{
			return;
		}
		HUDManager.Get().PlaySound(this.m_OpenSounds[UnityEngine.Random.Range(0, this.m_OpenSounds.Count)]);
	}

	private void PlayCloseSound()
	{
		if (HUDManager.Get() == null)
		{
			return;
		}
		if (this.m_CloseSounds.Count == 0)
		{
			return;
		}
		HUDManager.Get().PlaySound(this.m_CloseSounds[UnityEngine.Random.Range(0, this.m_CloseSounds.Count)]);
	}

	private void PlayChangeTabSound()
	{
		if (HUDManager.Get() == null)
		{
			return;
		}
		if (this.m_ChangeTabSounds.Count == 0)
		{
			return;
		}
		HUDManager.Get().PlaySound(this.m_ChangeTabSounds[UnityEngine.Random.Range(0, this.m_ChangeTabSounds.Count)]);
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override bool ShouldShow()
	{
		return Inventory3DManager.Get().IsActive();
	}

	public void OnPocketClick(string pocket_name)
	{
		if (Inventory3DManager.Get().m_CarriedItem)
		{
			return;
		}
		BackpackPocket pocket = (BackpackPocket)Enum.Parse(typeof(BackpackPocket), pocket_name);
		Inventory3DManager.Get().SetupPocket(pocket);
		this.PlayChangeTabSound();
	}

	public void OnPocketEnter(GameObject obj)
	{
		if (Inventory3DManager.Get().m_CarriedItem)
		{
			return;
		}
		obj.transform.Find("Selection").gameObject.SetActive(true);
	}

	public void OnPocketExit(GameObject obj)
	{
		if (Inventory3DManager.Get().m_CarriedItem)
		{
			return;
		}
		obj.transform.Find("Selection").gameObject.SetActive(false);
	}

	public void SetupPocket(BackpackPocket pocket)
	{
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		int[] array = new int[5];
		foreach (Item item in InventoryBackpack.Get().m_Items)
		{
			if (item != currentItem && !item.gameObject.activeSelf && !item.m_ShownInInventory)
			{
				array[(int)item.m_Info.m_BackpackPocket]++;
			}
		}
		foreach (PocketImageData pocketImageData in this.m_PocketImages)
		{
			if (pocketImageData.new_count_bg && pocketImageData.new_count_text)
			{
				if (pocketImageData.pocket == pocket)
				{
					pocketImageData.new_count_bg.gameObject.SetActive(false);
					pocketImageData.new_count_text.gameObject.SetActive(false);
				}
				else if (array[(int)pocketImageData.pocket] > 0)
				{
					pocketImageData.new_count_bg.gameObject.SetActive(true);
					pocketImageData.new_count_text.gameObject.SetActive(true);
					string text = array[(int)pocketImageData.pocket].ToString();
					if (text != pocketImageData.new_count_text.text && !this.m_Animations.ContainsKey(pocketImageData))
					{
						this.m_Animations.Add(pocketImageData, Time.time);
					}
					pocketImageData.new_count_text.text = text;
				}
				else
				{
					pocketImageData.new_count_bg.gameObject.SetActive(false);
					pocketImageData.new_count_text.gameObject.SetActive(false);
				}
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateAnimations();
		this.UpdateColor();
		this.UpdateEquippedIcon();
		this.m_PadQuitHint.SetActive(GreenHellGame.IsPadControllerActive() && !BodyInspectionController.Get().IsActive());
		this.m_PadSortHint.SetActive(GreenHellGame.IsPadControllerActive() && !BodyInspectionController.Get().IsActive());
		this.m_IsHovered = RectTransformUtility.RectangleContainsScreenPoint(this.m_BG.rectTransform, Input.mousePosition);
		if (this.m_IsHovered)
		{
			PocketImageData pocketImageData = null;
			foreach (PocketImageData pocketImageData2 in this.m_PocketImages)
			{
				if (pocketImageData2.selection.gameObject.activeSelf)
				{
					pocketImageData = pocketImageData2;
					break;
				}
			}
			CursorManager.Get().SetCursor((pocketImageData != null) ? CursorManager.TYPE.MouseOver : CursorManager.TYPE.Normal);
			if (pocketImageData != null && Input.GetKeyDown(InputHelpers.PadButton.Button_X.KeyFromPad()))
			{
				this.OnPocketClick(pocketImageData.pocket.ToString());
			}
		}
		foreach (Image image in this.m_PADChangeTabs)
		{
			if (image.gameObject.activeSelf)
			{
				Color color = image.color;
				color.a = (Inventory3DManager.Get().m_CarriedItem ? (this.m_PADChangeTabDefaultAlpha * 0.5f) : this.m_PADChangeTabDefaultAlpha);
				image.color = color;
			}
		}
	}

	private void UpdateEquippedIcon()
	{
		if (Inventory3DManager.Get().gameObject.activeSelf && Inventory3DManager.Get().m_ActivePocket == BackpackPocket.Left)
		{
			this.m_EquippedIcon.gameObject.SetActive(true);
			Vector3 screenPoint = InventoryBackpack.Get().m_EquippedItemSlot.GetScreenPoint();
			screenPoint.y -= 20f;
			this.m_EquippedIcon.rectTransform.position = screenPoint;
			return;
		}
		this.m_EquippedIcon.gameObject.SetActive(false);
	}

	private void UpdateColor()
	{
		foreach (PocketImageData pocketImageData in this.m_PocketImages)
		{
			Color color = pocketImageData.icon.color;
			color.a = ((Inventory3DManager.Get().m_ActivePocket == pocketImageData.pocket) ? this.m_SelectedAlpha : this.m_NormalAlpha);
			pocketImageData.icon.color = color;
		}
	}

	private void UpdateAnimations()
	{
		float num = 8f;
		List<PocketImageData> list = null;
		foreach (PocketImageData pocketImageData in this.m_Animations.Keys)
		{
			if (Time.time - this.m_Animations[pocketImageData] >= 3.14159274f / num)
			{
				pocketImageData.new_count_bg.transform.localScale = Vector3.one;
				if (list == null)
				{
					list = new List<PocketImageData>();
				}
				list.Add(pocketImageData);
			}
			else
			{
				float d = Mathf.Sin((Time.time - this.m_Animations[pocketImageData]) * num) + 1f;
				pocketImageData.new_count_bg.transform.localScale = Vector3.one * d;
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				this.m_Animations.Remove(list[i]);
			}
		}
	}

	public void OnInputAction(InputActionData action_data)
	{
		if (Inventory3DManager.Get().m_CarriedItem == null)
		{
			if (action_data.m_Action == InputsManager.InputAction.InventoryNextTab)
			{
				if (HUDItem.Get().m_Active)
				{
					HUDItem.Get().Deactivate();
				}
				BackpackPocket pocket = Inventory3DManager.Get().m_ActivePocket;
				switch (pocket)
				{
				case BackpackPocket.Main:
					pocket = BackpackPocket.Front;
					break;
				case BackpackPocket.Front:
					pocket = BackpackPocket.Right;
					break;
				case BackpackPocket.Top:
					pocket = BackpackPocket.Main;
					break;
				case BackpackPocket.Left:
					pocket = BackpackPocket.Top;
					break;
				case BackpackPocket.Right:
					pocket = BackpackPocket.Left;
					break;
				}
				Inventory3DManager.Get().SetupPocket(pocket);
				return;
			}
			if (action_data.m_Action == InputsManager.InputAction.InventoryPrevTab)
			{
				if (HUDItem.Get().m_Active)
				{
					HUDItem.Get().Deactivate();
				}
				BackpackPocket pocket2 = Inventory3DManager.Get().m_ActivePocket;
				switch (pocket2)
				{
				case BackpackPocket.Main:
					pocket2 = BackpackPocket.Top;
					break;
				case BackpackPocket.Front:
					pocket2 = BackpackPocket.Main;
					break;
				case BackpackPocket.Top:
					pocket2 = BackpackPocket.Left;
					break;
				case BackpackPocket.Left:
					pocket2 = BackpackPocket.Right;
					break;
				case BackpackPocket.Right:
					pocket2 = BackpackPocket.Front;
					break;
				}
				Inventory3DManager.Get().SetupPocket(pocket2);
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

	private List<PocketImageData> m_PocketImages;

	private Dictionary<PocketImageData, float> m_Animations = new Dictionary<PocketImageData, float>();

	public float m_NormalAlpha;

	public float m_SelectedAlpha;

	private static HUDBackpack s_Instance;

	private List<AudioClip> m_OpenSounds = new List<AudioClip>();

	private List<AudioClip> m_CloseSounds = new List<AudioClip>();

	private List<AudioClip> m_ChangeTabSounds = new List<AudioClip>();

	public List<Image> m_PADChangeTabs = new List<Image>();

	private float m_PADChangeTabDefaultAlpha;

	private RawImage m_BG;

	private Image m_EquippedIcon;

	public GameObject m_PadQuitHint;

	public GameObject m_PadSortHint;
}
