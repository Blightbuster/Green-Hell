using System;
using AIs;
using UnityEngine;

public class DifficultySettings : MonoBehaviour, ISaveLoad
{
	public static DifficultySettings Get()
	{
		if (DifficultySettings.s_Instance == null)
		{
			DifficultySettings.s_Instance = GreenHellGame.Instance.gameObject.AddComponent<DifficultySettings>();
		}
		return DifficultySettings.s_Instance;
	}

	public static DifficultySettingsPreset CustomPreset
	{
		get
		{
			return DifficultySettings.Get().m_Presets[5];
		}
	}

	public static DifficultySettingsPreset ActivePreset
	{
		get
		{
			return DifficultySettings.Get().m_Presets[(int)DifficultySettings.Get().m_ActivePresetType];
		}
	}

	public static DifficultySettingsPreset GetPreset(DifficultySettings.PresetType preset)
	{
		return DifficultySettings.Get().m_Presets[(int)preset];
	}

	public static void SetActivePresetType(DifficultySettings.PresetType preset)
	{
		DifficultySettings.Get().m_ActivePresetType = preset;
	}

	public static DifficultySettings.PresetType GetActivePresetType()
	{
		return DifficultySettings.Get().m_ActivePresetType;
	}

	private void Awake()
	{
		this.m_Presets[0] = (Resources.Load("Scripts/Difficulty/Tourist") as DifficultySettingsPreset);
		this.m_Presets[1] = (Resources.Load("Scripts/Difficulty/Easy") as DifficultySettingsPreset);
		this.m_Presets[2] = (Resources.Load("Scripts/Difficulty/Normal") as DifficultySettingsPreset);
		this.m_Presets[3] = (Resources.Load("Scripts/Difficulty/Hard") as DifficultySettingsPreset);
		this.m_Presets[4] = (Resources.Load("Scripts/Difficulty/PermaDeath") as DifficultySettingsPreset);
		this.m_Presets[5] = ScriptableObject.CreateInstance<DifficultySettingsPreset>();
		this.m_Presets[5].m_PresetName = "Custom";
	}

	public void Save()
	{
		SaveGame.SaveVal("Difficulty_ActivePresetType", (int)this.m_ActivePresetType);
		if (this.m_ActivePresetType == DifficultySettings.PresetType.Custom)
		{
			this.m_Presets[5].Save();
		}
	}

	public void Load()
	{
		if (SaveGame.m_SaveGameVersion < GreenHellGame.s_GameVersionEarlyAccessUpdate13)
		{
			return;
		}
		int num;
		this.m_ActivePresetType = (DifficultySettings.PresetType)(SaveGame.LoadVal("Difficulty_ActivePresetType", out num, false) ? num : 2);
		if (this.m_ActivePresetType == DifficultySettings.PresetType.Custom)
		{
			this.m_Presets[5].Load();
		}
	}

	public static bool IsAIIDEnabled(AI.AIID id)
	{
		return ((id != AI.AIID.BlackCaiman && !AI.IsCat(id)) || DifficultySettings.ActivePreset.m_Predators) && ((id != AI.AIID.Piranha && id != AI.AIID.Stingray) || DifficultySettings.ActivePreset.m_Aquatic) && (!AI.IsSnake(id) || DifficultySettings.ActivePreset.m_Snakes) && ((id != AI.AIID.BrasilianWanderingSpider && id != AI.AIID.GoliathBirdEater && id != AI.AIID.Scorpion) || DifficultySettings.ActivePreset.m_Insects) && (DifficultySettings.Get().m_ActivePresetType != DifficultySettings.PresetType.Tourist || (id != AI.AIID.Piranha && id != AI.AIID.Stingray && id != AI.AIID.BrasilianWanderingSpider && id != AI.AIID.GoliathBirdEater && id != AI.AIID.Scorpion && !AI.IsSnake(id)));
	}

	private DifficultySettings.PresetType m_ActivePresetType = DifficultySettings.PresetType.Normal;

	private DifficultySettingsPreset[] m_Presets = new DifficultySettingsPreset[6];

	private static DifficultySettings s_Instance;

	public enum PresetType
	{
		Tourist,
		Easy,
		Normal,
		Hard,
		PermaDeath,
		Custom,
		_Count
	}
}
