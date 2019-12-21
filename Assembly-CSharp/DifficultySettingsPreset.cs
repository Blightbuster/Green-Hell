using System;
using Enums;
using UnityEngine;

[CreateAssetMenu(menuName = "DifficultyPreset", fileName = "DifficultyPreset")]
public class DifficultySettingsPreset : ScriptableObject, IEquatable<DifficultySettingsPreset>
{
	public DifficultySettingsPreset(string name)
	{
		this.m_PresetName = name;
	}

	public void Load()
	{
		int num = SaveGame.LoadIVal("Difficulty_Version");
		if (1 == num)
		{
			if (!SaveGame.LoadEnumVal<GameDifficulty>("Difficulty_BaseDifficulty", out this.m_BaseDifficulty, false))
			{
				this.m_BaseDifficulty = GameDifficulty.Normal;
			}
			if (!SaveGame.LoadEnumVal<NutrientsDepletion>("Difficulty_NutrientsDepletion", out this.m_NutrientsDepletion, false))
			{
				this.m_NutrientsDepletion = NutrientsDepletion.Normal;
			}
			if (!SaveGame.LoadVal("Difficulty_Tribes", out this.m_Tribes, false))
			{
				this.m_Tribes = true;
			}
			if (!SaveGame.LoadVal("Difficulty_Predators", out this.m_Predators, false))
			{
				this.m_Predators = true;
			}
			if (!SaveGame.LoadVal("Difficulty_Insects", out this.m_Insects, false))
			{
				this.m_Insects = true;
			}
			if (!SaveGame.LoadVal("Difficulty_Leeches", out this.m_Leeches, false))
			{
				this.m_Leeches = true;
			}
			if (!SaveGame.LoadVal("Difficulty_Snakes", out this.m_Snakes, false))
			{
				this.m_Snakes = true;
			}
			if (!SaveGame.LoadVal("Difficulty_Aquatic", out this.m_Aquatic, false))
			{
				this.m_Aquatic = true;
			}
			if (!SaveGame.LoadVal("Difficulty_PermaDeath", out this.m_PermaDeath, false))
			{
				this.m_PermaDeath = true;
			}
			if (!SaveGame.LoadVal("Difficulty_Sanity", out this.m_Sanity, false))
			{
				this.m_Sanity = true;
			}
			if (!SaveGame.LoadVal("Difficulty_Energy", out this.m_Energy, false))
			{
				this.m_Energy = true;
			}
			if (!SaveGame.LoadVal("Difficulty_HPLoss", out this.m_HPLoss, false))
			{
				this.m_HPLoss = true;
			}
		}
	}

	public void Save()
	{
		SaveGame.SaveVal("Difficulty_Version", 1);
		SaveGame.SaveVal("Difficulty_BaseDifficulty", (int)this.m_BaseDifficulty);
		SaveGame.SaveVal("Difficulty_NutrientsDepletion", (int)this.m_NutrientsDepletion);
		SaveGame.SaveVal("Difficulty_Tribes", this.m_Tribes);
		SaveGame.SaveVal("Difficulty_Predators", this.m_Predators);
		SaveGame.SaveVal("Difficulty_Insects", this.m_Insects);
		SaveGame.SaveVal("Difficulty_Leeches", this.m_Leeches);
		SaveGame.SaveVal("Difficulty_Snakes", this.m_Snakes);
		SaveGame.SaveVal("Difficulty_Aquatic", this.m_Aquatic);
		SaveGame.SaveVal("Difficulty_PermaDeath", this.m_PermaDeath);
		SaveGame.SaveVal("Difficulty_Sanity", this.m_Sanity);
		SaveGame.SaveVal("Difficulty_Energy", this.m_Energy);
		SaveGame.SaveVal("Difficulty_HPLoss", this.m_HPLoss);
	}

	public bool Equals(DifficultySettingsPreset other)
	{
		return this.m_BaseDifficulty == other.m_BaseDifficulty && this.m_NutrientsDepletion == other.m_NutrientsDepletion && this.m_Tribes == other.m_Tribes && this.m_Predators == other.m_Predators && this.m_Insects == other.m_Insects && this.m_Leeches == other.m_Leeches && this.m_Snakes == other.m_Snakes && this.m_Aquatic == other.m_Aquatic && this.m_PermaDeath == other.m_PermaDeath && this.m_Sanity == other.m_Sanity && this.m_Energy == other.m_Energy && this.m_HPLoss == other.m_HPLoss;
	}

	public string m_PresetName;

	public GameDifficulty m_BaseDifficulty = GameDifficulty.Normal;

	public NutrientsDepletion m_NutrientsDepletion = NutrientsDepletion.Normal;

	public bool m_Tribes = true;

	public bool m_Predators = true;

	public bool m_Insects = true;

	public bool m_Leeches = true;

	public bool m_Snakes = true;

	public bool m_Aquatic = true;

	public bool m_PermaDeath = true;

	public bool m_Sanity = true;

	public bool m_Energy = true;

	public bool m_HPLoss = true;

	private const int SAVE_VERSION = 1;
}
