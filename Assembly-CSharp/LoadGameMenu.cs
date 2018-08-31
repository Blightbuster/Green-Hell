using System;
using Enums;
using UnityEngine;

public class LoadGameMenu : LoadSaveGameMenuCommon, IYesNoDialogOwner
{
	public override void OnShow()
	{
		base.OnShow();
		this.m_Loading = false;
	}

	protected override void Update()
	{
		base.Update();
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
	}

	protected override void OnSlotSelected(int slot_idx)
	{
		if (this.m_Slots[slot_idx].m_Empty)
		{
			return;
		}
		this.m_SlotIdx = slot_idx;
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_Warning"), GreenHellGame.Instance.GetLocalization().Get("MenuLoadGame_Confirm"), true);
		this.m_Loading = true;
	}

	public void OnYesFromDialog()
	{
		SaveGame.s_MainSaveName = "Slot" + this.m_SlotIdx.ToString() + ".sav";
		GreenHellGame.Instance.m_GameMode = this.m_Slots[this.m_SlotIdx].m_GameMode;
		base.Invoke("OnPreLoadGame", 0.3f);
	}

	public void OnNoFromDialog()
	{
		this.m_Loading = false;
	}

	public void OnOkFromDialog()
	{
		this.m_Loading = false;
	}

	private void OnPreLoadGame()
	{
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeOut(FadeType.All, new VDelegate(this.OnLoadGame), 2f, null);
		CursorManager.Get().ShowCursor(false);
	}

	private void OnLoadGame()
	{
		LoadingScreen.Get().Show(LoadingScreenState.StartGame);
		GreenHellGame.Instance.m_FromSave = true;
		Music.Get().Stop(1f);
		MainMenuManager.Get().HideAllScreens();
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeIn(FadeType.All, null, 2f);
		base.Invoke("OnLoadGameDelayed", 1f);
	}

	private void OnLoadGameDelayed()
	{
		Music.Get().PlayByName("loading_screen", false);
		Music.Get().m_Source.loop = true;
		GreenHellGame.Instance.StartGame();
	}

	public void OnBack()
	{
		if (this.m_Loading)
		{
			return;
		}
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenu), true);
	}

	private int m_SlotIdx = -1;

	private bool m_Loading;
}
