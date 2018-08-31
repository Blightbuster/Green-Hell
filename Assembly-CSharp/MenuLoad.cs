using System;
using Enums;

public class MenuLoad : MenuSaveLoadCommon, IYesNoDialogOwner
{
	protected override void OnShow()
	{
		base.OnShow();
		this.m_Loading = false;
	}

	protected override void OnSlotSelected(int slot_idx)
	{
		if (this.m_WaitingForDecision)
		{
			return;
		}
		if (this.m_Slots[slot_idx].m_Empty)
		{
			return;
		}
		this.m_SlotIdx = slot_idx;
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_Warning"), GreenHellGame.Instance.GetLocalization().Get("MenuSaveGame_OverwriteWarning"), false);
		this.m_WaitingForDecision = true;
	}

	public void OnYesFromDialog()
	{
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeOut(FadeType.All, new VDelegate(this.OnLoadGame), 1f, null);
		this.m_MenuInGameManager.HideMenu();
		CursorManager.Get().ShowCursor(false);
		this.m_Loading = true;
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
		if (this.m_Loading)
		{
			return;
		}
		base.Update();
	}

	public void OnLoadGame()
	{
		SaveGame.Load("Slot" + this.m_SlotIdx.ToString() + ".sav");
	}

	private void LoadGame()
	{
		SaveGame.Load("Slot" + this.m_SlotIdx.ToString() + ".sav");
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeOut(FadeType.All, new VDelegate(this.OnLoadGameEnd), 1f, null);
	}

	private void OnLoadGameEnd()
	{
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeIn(FadeType.All, null, 1f);
	}

	public override void OnBack()
	{
		if (this.m_Loading)
		{
			return;
		}
		if (Player.Get().IsDead())
		{
			this.m_MenuInGameManager.HideMenu();
		}
		else
		{
			base.OnBack();
		}
	}

	private int m_SlotIdx = -1;

	private bool m_Loading;

	private bool m_WaitingForDecision;
}
