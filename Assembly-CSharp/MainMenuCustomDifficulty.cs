using System;
using CJTools;
using Enums;
using UnityEngine;

public class MainMenuCustomDifficulty : MenuScreen
{
	private void Start()
	{
		this.GetSelectButton("BaseDifficulty").FillOptionsFromEnum<GameDifficulty>("DifficultyLevelShort_");
		this.GetSelectButton("NutrientsDepletion").FillOptionsFromEnum<NutrientsDepletion>("NutrientsDepletion_");
		this.SetUsingPreset(DifficultySettings.PresetType.Normal);
	}

	private void SetUsingPreset(DifficultySettings.PresetType preset_type)
	{
		DifficultySettingsPreset preset = DifficultySettings.GetPreset(preset_type);
		this.GetSelectButton("BaseDifficulty").SetSelectedOptionEnumValue<GameDifficulty>(preset.m_BaseDifficulty);
		this.GetSelectButton("NutrientsDepletion").SetSelectedOptionEnumValue<NutrientsDepletion>(preset.m_NutrientsDepletion);
		this.GetSelectButton("Insects").SetSelectedOptionBoolValue(preset.m_Insects);
		this.GetSelectButton("Leeches").SetSelectedOptionBoolValue(preset.m_Leeches);
		this.GetSelectButton("Aquatic").SetSelectedOptionBoolValue(preset.m_Aquatic);
		this.GetSelectButton("PermaDeath").SetSelectedOptionBoolValue(preset.m_PermaDeath);
		this.GetSelectButton("Predators").SetSelectedOptionBoolValue(preset.m_Predators);
		this.GetSelectButton("Sanity").SetSelectedOptionBoolValue(preset.m_Sanity);
		this.GetSelectButton("Snakes").SetSelectedOptionBoolValue(preset.m_Snakes);
		this.GetSelectButton("Tribes").SetSelectedOptionBoolValue(preset.m_Tribes);
		this.GetSelectButton("Energy").SetSelectedOptionBoolValue(preset.m_Energy);
		this.GetSelectButton("HPLoss").SetSelectedOptionBoolValue(preset.m_HPLoss);
	}

	public override void OnAccept()
	{
		DifficultySettingsPreset customPreset = DifficultySettings.CustomPreset;
		customPreset.m_BaseDifficulty = this.GetSelectButton("BaseDifficulty").GetSelectedOptionEnumValue<GameDifficulty>();
		customPreset.m_NutrientsDepletion = this.GetSelectButton("NutrientsDepletion").GetSelectedOptionEnumValue<NutrientsDepletion>();
		customPreset.m_Insects = this.GetSelectButton("Insects").GetSelectedOptionBoolValue();
		customPreset.m_Leeches = this.GetSelectButton("Leeches").GetSelectedOptionBoolValue();
		customPreset.m_Aquatic = this.GetSelectButton("Aquatic").GetSelectedOptionBoolValue();
		customPreset.m_PermaDeath = this.GetSelectButton("PermaDeath").GetSelectedOptionBoolValue();
		customPreset.m_Predators = this.GetSelectButton("Predators").GetSelectedOptionBoolValue();
		customPreset.m_Sanity = this.GetSelectButton("Sanity").GetSelectedOptionBoolValue();
		customPreset.m_Snakes = this.GetSelectButton("Snakes").GetSelectedOptionBoolValue();
		customPreset.m_Tribes = this.GetSelectButton("Tribes").GetSelectedOptionBoolValue();
		customPreset.m_Energy = this.GetSelectButton("Energy").GetSelectedOptionBoolValue();
		customPreset.m_HPLoss = this.GetSelectButton("HPLoss").GetSelectedOptionBoolValue();
		if (!EnumUtils<DifficultySettings.PresetType>.ForeachValueSelector(new Func<DifficultySettings.PresetType, bool>(this.TrySetPresetIfValid)))
		{
			DifficultySettings.SetActivePresetType(DifficultySettings.PresetType.Custom);
		}
		MainMenuManager.Get().SetActiveScreen(typeof(SaveGameMenu), true);
	}

	public bool TrySetPresetIfValid(DifficultySettings.PresetType preset)
	{
		if (preset == DifficultySettings.PresetType.Custom || preset == DifficultySettings.PresetType._Count)
		{
			return false;
		}
		if (DifficultySettings.CustomPreset.Equals(DifficultySettings.GetPreset(preset)))
		{
			DifficultySettings.SetActivePresetType(preset);
			return true;
		}
		return false;
	}

	public override void OnBack()
	{
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenuDifficultyLevel), true);
	}

	public void OnEasy()
	{
		this.SetUsingPreset(DifficultySettings.PresetType.Easy);
	}

	public void OnNormal()
	{
		this.SetUsingPreset(DifficultySettings.PresetType.Normal);
	}

	public void OnHard()
	{
		this.SetUsingPreset(DifficultySettings.PresetType.Hard);
	}

	public void OnPermaDeath()
	{
		this.SetUsingPreset(DifficultySettings.PresetType.PermaDeath);
	}

	public void OnTourist()
	{
		this.SetUsingPreset(DifficultySettings.PresetType.Tourist);
	}

	private UISelectButton GetSelectButton(string button_name)
	{
		Transform transform = base.transform.FindDeepChild(button_name);
		return ((transform != null) ? transform.gameObject : null).GetComponent<UISelectButton>();
	}
}
