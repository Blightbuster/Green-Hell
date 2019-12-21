using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuDifficultyLevel : MenuScreen
{
	protected override void Update()
	{
		base.Update();
		this.UpdateButtons();
		this.UpdateDescription();
	}

	private void UpdateDescription()
	{
		if (this.m_SelectedDifficulty != null && this.m_DescCanvasGroup.alpha < 1f && Time.time - this.m_SetActiveButtonTime > 0.3f)
		{
			this.m_DescCanvasGroup.alpha += Time.deltaTime * 0.5f;
			return;
		}
		if (this.m_SelectedDifficulty == null)
		{
			this.m_DescCanvasGroup.alpha = 0f;
		}
	}

	private void UpdateButtons()
	{
		MenuBase.MenuOptionData activeMenuOption = this.m_ActiveMenuOption;
		this.SetActiveButton((activeMenuOption != null) ? activeMenuOption.m_Button : null);
	}

	private void SetActiveButton(Button button)
	{
		if (this.m_ActiveButton == button)
		{
			return;
		}
		this.m_ActiveButton = button;
		if (this.m_ActiveButton == this.m_Tourist || this.m_ActiveButton == this.m_Easy || this.m_ActiveButton == this.m_Normal || this.m_ActiveButton == this.m_Hard || this.m_ActiveButton == this.m_PermaDeath || this.m_ActiveButton == this.m_Custom)
		{
			this.m_SelectedDifficulty = this.m_ActiveButton;
			this.m_SetActiveButtonTime = Time.time;
			this.SetupDescription();
			return;
		}
		this.m_SelectedDifficulty = null;
	}

	private void SetupDescription()
	{
		this.m_DescCanvasGroup.alpha = 0f;
		if (this.m_SelectedDifficulty != null)
		{
			this.m_DescriptionName.text = GreenHellGame.Instance.GetLocalization().Get("DifficultyLevel_" + this.m_SelectedDifficulty.name, true);
			this.m_Description.text = GreenHellGame.Instance.GetLocalization().Get("DifficultyLevel_" + this.m_SelectedDifficulty.name + "_Description", true);
		}
	}

	public void OnTourist()
	{
		DifficultySettings.SetActivePresetType(DifficultySettings.PresetType.Tourist);
		MainMenuManager.Get().SetActiveScreen(typeof(SaveGameMenu), true);
	}

	public void OnEasy()
	{
		DifficultySettings.SetActivePresetType(DifficultySettings.PresetType.Easy);
		MainMenuManager.Get().SetActiveScreen(typeof(SaveGameMenu), true);
	}

	public void OnNormal()
	{
		DifficultySettings.SetActivePresetType(DifficultySettings.PresetType.Normal);
		MainMenuManager.Get().SetActiveScreen(typeof(SaveGameMenu), true);
	}

	public void OnHard()
	{
		DifficultySettings.SetActivePresetType(DifficultySettings.PresetType.Hard);
		MainMenuManager.Get().SetActiveScreen(typeof(SaveGameMenu), true);
	}

	public void OnPermaDeath()
	{
		DifficultySettings.SetActivePresetType(DifficultySettings.PresetType.PermaDeath);
		MainMenuManager.Get().SetActiveScreen(typeof(SaveGameMenu), true);
	}

	public void OnCustom()
	{
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenuCustomDifficulty), true);
	}

	public Button m_Tourist;

	public Button m_Easy;

	public Button m_Normal;

	public Button m_Hard;

	public Button m_PermaDeath;

	public Button m_Custom;

	private Button m_SelectedDifficulty;

	private Button m_ActiveButton;

	public Button m_BackButton;

	public CanvasGroup m_DescCanvasGroup;

	public Text m_DescriptionName;

	public Text m_Description;

	private float m_SetActiveButtonTime;
}
