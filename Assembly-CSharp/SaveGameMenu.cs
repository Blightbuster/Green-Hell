using System;
using Enums;
using UnityEngine;

public class SaveGameMenu : LoadSaveGameMenuCommon, IYesNoDialogOwner
{
	public override void OnShow()
	{
		base.OnShow();
		this.m_Loading = false;
	}

	protected override void OnSlotSelected(int slot_idx)
	{
		this.m_SlotIdx = slot_idx;
		if (!this.m_Slots[slot_idx].m_Empty)
		{
			GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_Warning"), GreenHellGame.Instance.GetLocalization().Get("MenuSaveGame_OverwriteWarning"), true);
			this.m_Loading = true;
		}
		else
		{
			SaveGame.s_MainSaveName = "Slot" + this.m_SlotIdx.ToString() + ".sav";
			base.Invoke("OnPreStartGame", 0.3f);
			this.m_Loading = true;
		}
	}

	public void OnYesFromDialog()
	{
		SaveGame.s_MainSaveName = "Slot" + this.m_SlotIdx.ToString() + ".sav";
		base.Invoke("OnPreStartGame", 0.3f);
	}

	public void OnNoFromDialog()
	{
		this.m_Loading = false;
	}

	public void OnOkFromDialog()
	{
		this.m_Loading = false;
	}

	private void OnPreStartGame()
	{
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeOut(FadeType.All, new VDelegate(this.OnStartGame), 2f, null);
		CursorManager.Get().ShowCursor(false);
	}

	public void OnStartGame()
	{
		LoadingScreen.Get().Show(LoadingScreenState.StartGame);
		GreenHellGame.Instance.m_FromSave = false;
		Music.Get().Stop(1f);
		MainMenuManager.Get().HideAllScreens();
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeIn(FadeType.All, null, 2f);
		base.Invoke("OnStartGameDelayed", 1f);
	}

	private void OnStartGameDelayed()
	{
		Music.Get().PlayByName("loading_screen", false);
		Music.Get().m_Source.loop = true;
		GreenHellGame.Instance.StartGame();
	}

	protected override void Update()
	{
		base.Update();
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
	}

	public void OnBack()
	{
		if (this.m_Loading)
		{
			return;
		}
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenuDifficultyLevel), true);
	}

	private int m_SlotIdx = -1;

	private bool m_Loading;
}
