using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDBackpack : HUDBase
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
		GameObject gameObject = base.transform.FindDeepChild("Pockets").gameObject;
		RawImage[] componentsInChildren = gameObject.GetComponentsInChildren<RawImage>();
		this.m_PocketImages = new List<HUDBackpack.PocketImageData>();
		foreach (RawImage rawImage in componentsInChildren)
		{
			if (!(rawImage.name != BackpackPocket.Front.ToString()) || !(rawImage.name != BackpackPocket.Main.ToString()) || !(rawImage.name != BackpackPocket.Left.ToString()) || !(rawImage.name != BackpackPocket.Right.ToString()) || !(rawImage.name != BackpackPocket.Top.ToString()))
			{
				HUDBackpack.PocketImageData item = default(HUDBackpack.PocketImageData);
				item.pocket = (BackpackPocket)Enum.Parse(typeof(BackpackPocket), rawImage.name);
				item.icon = rawImage;
				item.selection = rawImage.transform.Find("Selection").gameObject;
				item.selection.SetActive(false);
				item.new_count_bg = rawImage.transform.Find("BG").GetComponent<Image>();
				item.new_count_text = rawImage.transform.GetComponentInChildren<Text>();
				this.m_PocketImages.Add(item);
			}
		}
		GameObject gameObject2 = new GameObject("EquippedIcon");
		this.m_EquippedIcon = gameObject2.AddComponent<Image>();
		Sprite sprite = null;
		ItemsManager.Get().m_ItemIconsSprites.TryGetValue("Default_Pickup", out sprite);
		DebugUtils.Assert(sprite, true);
		this.m_EquippedIcon.sprite = sprite;
		this.m_EquippedIcon.rectTransform.sizeDelta = new Vector2(20f, 20f);
		gameObject2.GetComponent<RectTransform>().SetParent(base.transform);
		gameObject2.SetActive(true);
		this.m_BG = base.transform.Find("BG").gameObject.GetComponent<RawImage>();
		this.InitializeSounds();
		for (int j = 0; j < base.transform.childCount; j++)
		{
			base.transform.GetChild(j).gameObject.SetActive(false);
		}
		base.enabled = false;
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
		foreach (HUDBackpack.PocketImageData pocketImageData in this.m_PocketImages)
		{
			pocketImageData.selection.SetActive(false);
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.SetupSelections();
		this.PlayOpenSound();
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
		BackpackPocket pocket = (BackpackPocket)Enum.Parse(typeof(BackpackPocket), pocket_name);
		Inventory3DManager.Get().SetupPocket(pocket);
		this.PlayChangeTabSound();
	}

	public void OnPocketEnter(GameObject obj)
	{
		obj.transform.Find("Selection").gameObject.SetActive(true);
	}

	public void OnPocketExit(GameObject obj)
	{
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
		foreach (HUDBackpack.PocketImageData key in this.m_PocketImages)
		{
			if (key.new_count_bg && key.new_count_text)
			{
				if (key.pocket == pocket)
				{
					key.new_count_bg.gameObject.SetActive(false);
					key.new_count_text.gameObject.SetActive(false);
				}
				else if (array[(int)key.pocket] > 0)
				{
					key.new_count_bg.gameObject.SetActive(true);
					key.new_count_text.gameObject.SetActive(true);
					string text = array[(int)key.pocket].ToString();
					if (text != key.new_count_text.text && !this.m_Animations.ContainsKey(key))
					{
						this.m_Animations.Add(key, Time.time);
					}
					key.new_count_text.text = text;
				}
				else
				{
					key.new_count_bg.gameObject.SetActive(false);
					key.new_count_text.gameObject.SetActive(false);
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
		this.m_IsHovered = RectTransformUtility.RectangleContainsScreenPoint(this.m_BG.rectTransform, Input.mousePosition);
		if (this.m_IsHovered)
		{
			bool flag = false;
			foreach (HUDBackpack.PocketImageData pocketImageData in this.m_PocketImages)
			{
				if (pocketImageData.selection.gameObject.activeSelf)
				{
					flag = true;
					break;
				}
			}
			CursorManager.Get().SetCursor((!flag) ? CursorManager.TYPE.Normal : CursorManager.TYPE.MouseOver);
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
		}
		else
		{
			this.m_EquippedIcon.gameObject.SetActive(false);
		}
	}

	private void UpdateColor()
	{
		foreach (HUDBackpack.PocketImageData pocketImageData in this.m_PocketImages)
		{
			Color color = pocketImageData.icon.color;
			color.a = ((Inventory3DManager.Get().m_ActivePocket != pocketImageData.pocket) ? this.m_NormalAlpha : this.m_SelectedAlpha);
			pocketImageData.icon.color = color;
		}
	}

	private void UpdateAnimations()
	{
		float num = 8f;
		List<HUDBackpack.PocketImageData> list = null;
		foreach (HUDBackpack.PocketImageData pocketImageData in this.m_Animations.Keys)
		{
			if (Time.time - this.m_Animations[pocketImageData] >= 3.14159274f / num)
			{
				pocketImageData.new_count_bg.transform.localScale = Vector3.one;
				if (list == null)
				{
					list = new List<HUDBackpack.PocketImageData>();
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

	private List<HUDBackpack.PocketImageData> m_PocketImages;

	private Dictionary<HUDBackpack.PocketImageData, float> m_Animations = new Dictionary<HUDBackpack.PocketImageData, float>();

	public float m_NormalAlpha;

	public float m_SelectedAlpha;

	private static HUDBackpack s_Instance;

	private List<AudioClip> m_OpenSounds = new List<AudioClip>();

	private List<AudioClip> m_CloseSounds = new List<AudioClip>();

	private List<AudioClip> m_ChangeTabSounds = new List<AudioClip>();

	private RawImage m_BG;

	private Image m_EquippedIcon;

	private struct PocketImageData
	{
		public BackpackPocket pocket;

		public RawImage icon;

		public GameObject selection;

		public Image new_count_bg;

		public Text new_count_text;
	}
}
