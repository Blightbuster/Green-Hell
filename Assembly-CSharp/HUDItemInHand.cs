using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDItemInHand : HUDBase
{
	private void Initialize()
	{
		for (int i = 0; i < 2147483647; i++)
		{
			Transform transform = base.transform.Find("Action" + i);
			if (!transform)
			{
				break;
			}
			Text component = transform.gameObject.GetComponent<Text>();
			HUDItemInHand.Action action = default(HUDItemInHand.Action);
			action.text = component;
			action.parent = transform.gameObject;
			action.rect_trans = (RectTransform)transform;
			action.key_frame = transform.gameObject.FindChild("KeyFrame").GetComponent<RawImage>();
			action.mouse_icon_lmb = transform.gameObject.FindChild("MouseIconLMB").GetComponent<RawImage>();
			action.mouse_icon_rmb = transform.gameObject.FindChild("MouseIconRMB").GetComponent<RawImage>();
			action.mouse_icon_mmb = transform.gameObject.FindChild("MouseIconMMB").GetComponent<RawImage>();
			action.pad_icon = transform.gameObject.FindChild("PadIcon").GetComponent<Image>();
			action.key_text = transform.gameObject.FindChild("KeyText").GetComponent<Text>();
			action.key_text_long = transform.gameObject.FindChild("KeyTextLong").GetComponent<Text>();
			action.position = action.rect_trans.anchoredPosition;
			action.disbled_pos = action.position;
			action.disbled_pos.x = action.disbled_pos.x + 2000f;
			action.parent.SetActive(true);
			this.m_Actions.Add(action);
		}
		this.m_Initialized = true;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	private Item GetCurrentItem()
	{
		if (Player.Get().m_WeaponHiddenFromScenario)
		{
			return null;
		}
		if (Player.Get().GetCurrentItem(Hand.Right))
		{
			return Player.Get().GetCurrentItem(Hand.Right);
		}
		if (Player.Get().GetCurrentItem(Hand.Left))
		{
			return Player.Get().GetCurrentItem(Hand.Left);
		}
		if (InventoryBackpack.Get().m_EquippedItem != null)
		{
			return InventoryBackpack.Get().m_EquippedItem;
		}
		return null;
	}

	protected override bool ShouldShow()
	{
		return !Player.Get().m_DreamActive && !HUDReadableItem.Get().isActiveAndEnabled && !CutscenesManager.Get().IsCutscenePlaying() && this.GetCurrentItem() != null;
	}

	protected override void OnShow()
	{
		base.OnShow();
		if (!this.m_Initialized)
		{
			this.Initialize();
		}
		this.Setup();
	}

	private void Setup()
	{
		this.m_Item = this.GetCurrentItem();
		if (!this.m_Item)
		{
			return;
		}
		string text = this.m_Item.GetIconName();
		if (text.Empty())
		{
			text = "Default_Pickup";
		}
		Sprite sprite = null;
		ItemsManager.Get().m_ItemIconsSprites.TryGetValue(text, out sprite);
		if (sprite != null)
		{
			this.m_Icon.sprite = sprite;
			this.m_Icon.enabled = true;
			this.m_HPBar.enabled = true;
			this.m_HPBarBG.enabled = true;
		}
		else
		{
			this.m_Icon.enabled = false;
			this.m_HPBar.enabled = false;
			this.m_HPBarBG.enabled = false;
		}
		this.UpdateActions();
	}

	protected override void Update()
	{
		base.Update();
		if (this.m_Item != this.GetCurrentItem())
		{
			this.Setup();
		}
		if (!this.m_Item)
		{
			return;
		}
		if (this.m_Item.m_Info.IsHeavyObject())
		{
			HeavyObject heavyObject = (HeavyObject)this.m_Item;
			this.m_HPBar.fillAmount = (float)(heavyObject.m_Attached.Count + 1) / (float)(heavyObject.m_NumObjectsToAttach + 1);
		}
		else
		{
			this.m_HPBar.fillAmount = this.m_Item.m_Info.m_Health / this.m_Item.m_Info.m_MaxHealth;
		}
		this.UpdateActions();
	}

	private void UpdateActions()
	{
		foreach (HUDItemInHand.Action action in this.m_Actions)
		{
			action.rect_trans.anchoredPosition = action.disbled_pos;
		}
		if (!Player.Get().GetCurrentItem())
		{
			return;
		}
		this.m_ActionsTemp.Clear();
		Player.Get().GetInputActions(ref this.m_ActionsTemp);
		if (this.m_ActionsTemp.Count > this.m_Actions.Count)
		{
			DebugUtils.Assert("HUDItemInHand - Not enough action slots!", true, DebugUtils.AssertType.Info);
		}
		bool flag = GreenHellGame.IsPadControllerActive();
		int num = 0;
		while (num < this.m_ActionsTemp.Count && num < this.m_Actions.Count)
		{
			InputActionData inputActionData = InputsManager.Get().GetInputActionData((InputsManager.InputAction)this.m_ActionsTemp[num]);
			string text;
			if (!this.m_CachedTexts.TryGetValue(this.m_ActionsTemp[num], out text))
			{
				text = this.GetText(inputActionData);
				this.m_CachedTexts[this.m_ActionsTemp[num]] = text;
			}
			if (!text.Empty())
			{
				HUDItemInHand.Action action2 = this.m_Actions[num];
				action2.rect_trans.anchoredPosition = action2.position;
				action2.text.text = text;
				if (flag)
				{
					action2.pad_icon.sprite = inputActionData.m_PadIcon;
					action2.key_frame.gameObject.SetActive(false);
					action2.key_text.gameObject.SetActive(false);
					action2.key_text_long.gameObject.SetActive(false);
					action2.mouse_icon_lmb.gameObject.SetActive(false);
					action2.mouse_icon_rmb.gameObject.SetActive(false);
					action2.mouse_icon_mmb.gameObject.SetActive(false);
				}
				else
				{
					string keyText = this.GetKeyText(inputActionData);
					if (inputActionData.m_KeyCode == KeyCode.Mouse0)
					{
						action2.key_frame.gameObject.SetActive(false);
						action2.key_text.gameObject.SetActive(false);
						action2.key_text_long.gameObject.SetActive(false);
						action2.mouse_icon_lmb.gameObject.SetActive(true);
						action2.mouse_icon_rmb.gameObject.SetActive(false);
						action2.mouse_icon_mmb.gameObject.SetActive(false);
					}
					else if (inputActionData.m_KeyCode == KeyCode.Mouse1)
					{
						action2.key_frame.gameObject.SetActive(false);
						action2.key_text.gameObject.SetActive(false);
						action2.key_text_long.gameObject.SetActive(false);
						action2.mouse_icon_lmb.gameObject.SetActive(false);
						action2.mouse_icon_rmb.gameObject.SetActive(true);
						action2.mouse_icon_mmb.gameObject.SetActive(false);
					}
					else if (inputActionData.m_KeyCode == KeyCode.Mouse2)
					{
						action2.key_frame.gameObject.SetActive(false);
						action2.key_text.gameObject.SetActive(false);
						action2.key_text_long.gameObject.SetActive(false);
						action2.mouse_icon_lmb.gameObject.SetActive(false);
						action2.mouse_icon_rmb.gameObject.SetActive(false);
						action2.mouse_icon_mmb.gameObject.SetActive(true);
					}
					else if (action2.key_text.text.Length <= 1)
					{
						action2.key_frame.gameObject.SetActive(true);
						action2.key_text.gameObject.SetActive(true);
						action2.key_text_long.gameObject.SetActive(false);
						action2.mouse_icon_lmb.gameObject.SetActive(false);
						action2.mouse_icon_rmb.gameObject.SetActive(false);
						action2.mouse_icon_mmb.gameObject.SetActive(false);
						action2.key_text.text = keyText;
					}
					else
					{
						action2.key_frame.gameObject.SetActive(false);
						action2.key_text.gameObject.SetActive(false);
						action2.key_text_long.gameObject.SetActive(true);
						action2.mouse_icon_lmb.gameObject.SetActive(false);
						action2.mouse_icon_rmb.gameObject.SetActive(false);
						action2.mouse_icon_mmb.gameObject.SetActive(false);
						action2.key_text_long.text = keyText;
					}
				}
			}
			num++;
		}
	}

	private string GetText(InputActionData data)
	{
		if (data == null)
		{
			return string.Empty;
		}
		string str = string.Empty;
		InputsManager.InputAction action = data.m_Action;
		if (data.m_Hold > 0f)
		{
			str = str + GreenHellGame.Instance.GetLocalization().Get("HUD_Trigger_Hold", true) + " ";
		}
		return str + GreenHellGame.Instance.GetLocalization().Get("HUDItemInHand_" + EnumUtils<InputsManager.InputAction>.GetName((int)action), true);
	}

	private string GetKeyText(InputActionData data)
	{
		if (data == null)
		{
			return string.Empty;
		}
		return KeyCodeToString.GetString(data.m_KeyCode);
	}

	public Image m_Icon;

	public Image m_HPBar;

	public Image m_HPBarBG;

	private Item m_Item;

	private List<HUDItemInHand.Action> m_Actions = new List<HUDItemInHand.Action>();

	private bool m_Initialized;

	private List<int> m_ActionsTemp = new List<int>(200);

	private Dictionary<int, string> m_CachedTexts = new Dictionary<int, string>();

	private struct Action
	{
		public GameObject parent;

		public Text text;

		public RawImage key_frame;

		public RawImage mouse_icon_lmb;

		public RawImage mouse_icon_rmb;

		public RawImage mouse_icon_mmb;

		public Image pad_icon;

		public Text key_text;

		public Text key_text_long;

		public Vector3 position;

		public Vector3 disbled_pos;

		public RectTransform rect_trans;
	}
}
