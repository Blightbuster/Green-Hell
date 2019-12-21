using System;
using System.Linq;
using Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoadGameMenu : LoadSaveGameMenuCommon, IYesNoDialogOwner
{
	protected override void Awake()
	{
		base.Awake();
		this.m_PadSelectButtonActiveAlpha = this.m_PadSelectButton.color.a;
		this.m_PadSelectTextActiveAlpha = this.m_PadSelectButton.GetComponentInChildren<Text>().color.a;
	}

	protected override bool IsLoadGameMenu()
	{
		return true;
	}

	public override void OnShow()
	{
		base.OnShow();
		base.EnableSlots(true);
		this.m_IsDialog = false;
		bool flag = this.m_Slots.Any((SaveGameMenuSlot s) => !s.m_Empty && s.m_SaveInfo.loadable && s.m_SaveInfo.game_version < GreenHellGame.s_GameVersionReleaseCandidate);
		this.m_OldSavesWarning.gameObject.SetActive(flag);
		if (flag)
		{
			Text component = this.m_OldSavesWarning.FindChild("Text").GetComponent<Text>();
			component.text = GreenHellGame.Instance.GetLocalization().Get("SaveGame_PreMasterVersionWarning2", true);
			if (SteamManager.Initialized)
			{
				Text text = component;
				text.text = text.text + "\n\n" + GreenHellGame.Instance.GetLocalization().Get("SaveGame_SteamBetasInfo", true);
			}
		}
		if (GreenHellGame.IsPadControllerActive())
		{
			EventSystem current = EventSystem.current;
			if (current != null)
			{
				current.SetSelectedGameObject(null);
			}
			for (int i = 0; i < this.m_Slots.Count; i++)
			{
				if (this.m_Slots[i].m_SaveInfo.loadable)
				{
					this.m_Slots[i].Select();
					this.m_Slots[i].OnSelect(null);
					return;
				}
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		bool flag = false;
		foreach (SaveGameMenuSlot saveGameMenuSlot in this.m_Slots)
		{
			if (saveGameMenuSlot.gameObject == EventSystem.current.currentSelectedGameObject)
			{
				flag = saveGameMenuSlot.m_SaveInfo.loadable;
				break;
			}
		}
		Color color = this.m_PadSelectButton.color;
		color.a = (flag ? this.m_PadSelectButtonActiveAlpha : MenuScreen.s_InactiveButtonsAlpha);
		this.m_PadSelectButton.color = color;
		Text componentInChildren = this.m_PadSelectButton.GetComponentInChildren<Text>();
		color = componentInChildren.color;
		color.a = (flag ? this.m_PadSelectTextActiveAlpha : MenuScreen.s_InactiveButtonsAlpha);
		componentInChildren.color = color;
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
	}

	protected override void OnSlotSelected(int slot_idx)
	{
		if (GreenHellGame.GetYesNoDialog() && GreenHellGame.GetYesNoDialog().gameObject.activeSelf)
		{
			return;
		}
		if (this.m_Slots[slot_idx].m_Empty)
		{
			return;
		}
		if (!this.m_Slots[slot_idx].m_SaveInfo.loadable)
		{
			return;
		}
		this.m_SlotIdx = slot_idx;
		if (this.m_Slots[slot_idx].m_SaveInfo.game_version < GreenHellGame.s_GameVersionReleaseCandidate)
		{
			this.ShowPreMasterVersionWarning();
		}
		else
		{
			this.ShowConfirmationDialog();
		}
		this.m_IsDialog = true;
	}

	private void ShowPreMasterVersionWarning()
	{
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_Warning", true), GreenHellGame.Instance.GetLocalization().Get("SaveGame_PreMasterVersionWarning", true), !this.m_IsIngame);
	}

	private void ShowConfirmationDialog()
	{
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_Warning", true), GreenHellGame.Instance.GetLocalization().Get("MenuLoadGame_Confirm", true), !this.m_IsIngame);
	}

	public void OnYesFromDialog()
	{
		SaveGame.s_MainSaveName = this.m_Slots[this.m_SlotIdx].m_SaveInfo.file_name;
		Debug.Log("LoadGameMenu:OnYesFromDialog - " + SaveGame.s_MainSaveName);
		if (this.m_IsIngame)
		{
			GreenHellGame.GetFadeSystem().FadeOut(FadeType.All, new VDelegate(this.OnLoadGame), 0.5f, null);
			this.m_MenuInGameManager.HideMenu();
			if (ConsciousnessController.Get().IsActive())
			{
				ConsciousnessController.Get().Stop();
			}
			CursorManager.Get().ShowCursor(false, false);
			return;
		}
		GreenHellGame.Instance.m_GameMode = this.m_Slots[this.m_SlotIdx].m_GameMode;
		MainMenuManager.Get().CallLoadGameFadeSequence();
		base.EnableSlots(false);
	}

	public void OnNoFromDialog()
	{
		this.m_IsDialog = false;
	}

	public void OnOkFromDialog()
	{
		this.m_IsDialog = false;
	}

	public void OnCloseDialog()
	{
		if (GreenHellGame.IsPadControllerActive())
		{
			this.m_Slots[this.m_SlotIdx].Select();
			this.m_Slots[this.m_SlotIdx].OnSelect(null);
		}
	}

	private void OnLoadGame()
	{
		Debug.Log("LoadGameMenu:OnLoadGame - " + base.GetSelectedSlotFileName());
		SaveGame.Load(base.GetSelectedSlotFileName(), this.m_Slots[this.m_SlotIdx].m_GameMode);
		GreenHellGame.GetFadeSystem().FadeIn(FadeType.All, null, 0.5f);
	}

	public override void OnBack()
	{
		if (this.m_IsDialog)
		{
			return;
		}
		if (this.m_IsIngame)
		{
			Player player = Player.Get();
			if (player != null && player.IsDead())
			{
				this.m_MenuInGameManager.HideMenu();
				return;
			}
		}
		base.OnBack();
	}

	public GameObject m_OldSavesWarning;

	private bool m_IsDialog;

	public Image m_PadSelectButton;

	private float m_PadSelectButtonActiveAlpha;

	private float m_PadSelectTextActiveAlpha;

	private enum DialogType
	{
		None,
		ConfirmLoad,
		PreMasterVersion
	}
}
