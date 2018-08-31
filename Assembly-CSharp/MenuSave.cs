using System;
using Enums;
using UnityEngine;

public class MenuSave : MenuSaveLoadCommon, IYesNoDialogOwner
{
	protected override void OnSlotSelected(int slot_idx)
	{
		if (this.m_WaitingForDecision)
		{
			return;
		}
		this.m_SlotIdx = slot_idx;
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_Warning"), GreenHellGame.Instance.GetLocalization().Get("MenuSaveGame_OverwriteWarning"), false);
		this.m_WaitingForDecision = true;
	}

	public void OnYesFromDialog()
	{
		SaveGame.Save("Slot" + this.m_SlotIdx.ToString() + ".sav");
		this.m_MenuInGameManager.HideMenu();
		this.m_WaitingForDecision = false;
	}

	public void OnNoFromDialog()
	{
		this.m_WaitingForDecision = false;
	}

	public void OnOkFromDialog()
	{
		this.m_WaitingForDecision = false;
	}

	protected override void UpdateSlots()
	{
		if (this.m_WaitingForDecision)
		{
			return;
		}
		base.UpdateSlots();
	}

	protected override void Update()
	{
		base.Update();
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
	}

	public override void OnBack()
	{
		this.m_MenuInGameManager.HideMenu();
	}

	private int m_SlotIdx = -1;

	private bool m_WaitingForDecision;
}
