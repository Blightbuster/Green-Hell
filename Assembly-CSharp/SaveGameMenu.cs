using System;
using Enums;
using UnityEngine;

public class SaveGameMenu : LoadSaveGameMenuCommon, IYesNoDialogOwner
{
	public override void OnShow()
	{
		base.OnShow();
		this.m_IsBusy = false;
		this.m_SlotIdx = -1;
	}

	protected override void OnSlotSelected(int slot_idx)
	{
		if (this.m_IsBusy)
		{
			return;
		}
		if (this.m_SlotIdx == slot_idx)
		{
			return;
		}
		this.m_SlotIdx = slot_idx;
		if (!this.m_Slots[slot_idx].m_Empty)
		{
			GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_Warning", true), GreenHellGame.Instance.GetLocalization().Get("MenuSaveGame_OverwriteWarning", true), !this.m_IsIngame);
			this.m_IsBusy = true;
			return;
		}
		this.DoSaveGame();
	}

	public void OnYesFromDialog()
	{
		Debug.Log("SaveGameMenu:OnYesFromDialog - " + base.GetSelectedSlotFileName());
		this.DoSaveGame();
	}

	public void OnNoFromDialog()
	{
		this.m_IsBusy = false;
		this.m_SlotIdx = -1;
	}

	public void OnOkFromDialog()
	{
		this.m_IsBusy = false;
	}

	public void OnCloseDialog()
	{
	}

	private void DoSaveGame()
	{
		this.m_IsBusy = true;
		if (this.m_IsIngame)
		{
			SaveGame.Save(base.GetSlotSaveName(this.m_SlotIdx));
			this.m_MenuInGameManager.HideMenu();
			return;
		}
		SaveGame.s_MainSaveName = base.GetSlotSaveName(this.m_SlotIdx);
		Debug.Log("SaveGameMenu:DoSaveGame - " + SaveGame.s_MainSaveName);
		MainMenuManager.Get().CallStartGameFadeSequence();
	}

	public override void OnBack()
	{
		if (this.m_IsBusy)
		{
			return;
		}
		this.ShowPreviousScreen();
	}

	private bool m_IsBusy;
}
