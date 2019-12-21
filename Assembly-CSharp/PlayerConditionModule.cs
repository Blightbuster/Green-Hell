using System;
using System.Collections.Generic;
using AIs;
using CJTools;
using Enums;
using UnityEngine;

public class PlayerConditionModule : PlayerModule, ISaveLoad
{
	public float m_Stamina { get; set; }

	public float m_MaxHP { get; set; }

	public event PlayerConditionModule.OnStaminaDecreasedDel OnStaminaDecreasedEvent;

	public float m_HP
	{
		get
		{
			return this.m_HPProp;
		}
		set
		{
			this.m_HPProp = value;
		}
	}

	public int m_Sanity
	{
		get
		{
			return PlayerSanityModule.Get().m_Sanity;
		}
		set
		{
			PlayerSanityModule.Get().m_Sanity = value;
		}
	}

	public float m_MaxEnergy { get; set; }

	public float m_Energy { get; set; }

	public float m_NutritionFat { get; set; }

	public float m_NutritionCarbo { get; set; }

	public float m_NutritionProteins { get; set; }

	public float m_Hydration { get; set; }

	public float m_Dirtiness { get; set; }

	public static PlayerConditionModule Get()
	{
		return PlayerConditionModule.s_Instance;
	}

	private void Awake()
	{
		PlayerConditionModule.s_Instance = this;
	}

	private void Start()
	{
	}

	public override void Initialize(Being being)
	{
		base.Initialize(being);
		this.ResetParams();
		this.ParseFile();
		this.m_Sky = UnityEngine.Object.FindObjectOfType<TOD_Sky>();
		this.m_PlayerSleepController = this.m_Player.GetComponent<SleepController>();
		this.m_DiseasesModule = this.m_Player.GetComponent<PlayerDiseasesModule>();
		this.m_InjuryModule = this.m_Player.GetComponent<PlayerInjuryModule>();
		this.m_SwimController = this.m_Player.GetComponent<SwimController>();
		this.m_AudioModule = this.m_Player.GetComponent<PlayerAudioModule>();
	}

	public override void PostInitialize()
	{
		base.PostInitialize();
		this.m_MaxDirtiness = 100f * (float)PlayerDiseasesModule.Get().GetDisease(ConsumeEffect.DirtSickness).m_MaxLevel;
	}

	public void ResetParams()
	{
		this.m_Stamina = this.m_MaxStamina;
		this.m_HP = this.m_MaxHP;
		this.m_NutritionFat = this.m_MaxNutritionFat;
		this.m_NutritionCarbo = this.m_MaxNutritionCarbo;
		this.m_NutritionProteins = this.m_MaxNutritionProteins;
		this.m_Hydration = this.m_MaxHydration;
		this.m_Energy = this.m_MaxEnergy;
		this.m_Dirtiness = 0f;
	}

	public void ScenarioSetParamsFromScript(string script_name)
	{
		this.ParseFile(script_name);
	}

	public void ClampParams()
	{
		this.m_Stamina = Mathf.Clamp(this.m_Stamina, 0f, this.m_MaxStamina);
		this.m_HP = Mathf.Clamp(this.m_HP, 0f, this.m_MaxHP);
		this.m_NutritionFat = Mathf.Clamp(this.m_NutritionFat, 0f, this.m_MaxNutritionFat);
		this.m_NutritionCarbo = Mathf.Clamp(this.m_NutritionCarbo, 0f, this.m_MaxNutritionCarbo);
		this.m_NutritionProteins = Mathf.Clamp(this.m_NutritionProteins, 0f, this.m_MaxNutritionProteins);
		this.m_Hydration = Mathf.Clamp(this.m_Hydration, 0f, this.m_MaxHydration);
		this.m_Energy = Mathf.Clamp(this.m_Energy, 0f, this.m_MaxEnergy);
	}

	private void ParseFile()
	{
		if (GreenHellGame.TWITCH_DEMO)
		{
			this.ParseFile("Player_condition_TwitchDemo");
			return;
		}
		this.ParseFile("Player_condition");
	}

	private void ParseFile(string script_name)
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Player/" + script_name, true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "MaxStamina")
			{
				this.m_MaxStamina = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "Stamina")
			{
				this.m_Stamina = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "MaxEnergy")
			{
				this.m_MaxEnergy = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "Energy")
			{
				this.m_Energy = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "MaxHP")
			{
				this.m_MaxHP = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HP")
			{
				this.m_HP = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "MaxNutritionFat")
			{
				this.m_MaxNutritionFat = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionFat")
			{
				this.m_NutritionFat = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "MaxNutritionCarbo")
			{
				this.m_MaxNutritionCarbo = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionCarbo")
			{
				this.m_NutritionCarbo = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "MaxNutritionProtein")
			{
				this.m_MaxNutritionProteins = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionProtein")
			{
				this.m_NutritionProteins = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "MaxHydration")
			{
				this.m_MaxHydration = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "Hydration")
			{
				this.m_Hydration = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "StaminaConsumptionWalkPerSecond")
			{
				this.m_StaminaConsumptionWalkPerSecond = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "StaminaConsumptionRunPerSecond")
			{
				this.m_StaminaConsumptionRunPerSecond = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "StaminaConsumptionDepletedPerSecond")
			{
				this.m_StaminaConsumptionDepletedPerSecond = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "StaminaRegenerationPerSecond")
			{
				this.m_StaminaRegenerationPerSecond = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "StaminaDepletedLevel")
			{
				this.m_StaminaDepletedLevel = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "LowStaminaLevel")
			{
				this.m_LowStaminaLevel = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "LowStaminaRecoveryLevel")
			{
				this.m_LowStaminaRecoveryLevel = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "StaminaDecrease")
			{
				this.m_StaminaDecreaseMap.Add((int)Enum.Parse(typeof(StaminaDecreaseReason), key.GetVariable(0).SValue), key.GetVariable(1).FValue);
			}
			else if (key.GetName() == "EnergyDecrease")
			{
				this.m_EnergyDecreaseMap.Add((int)Enum.Parse(typeof(EnergyDecreaseReason), key.GetVariable(0).SValue), key.GetVariable(1).FValue);
			}
			else if (key.GetName() == "OxygenConsumptionPerSecond")
			{
				this.m_OxygenConsumptionPerSecond = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "EnergyConsumptionPerSecond")
			{
				this.m_EnergyConsumptionPerSecond = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "EnergyConsumptionPerSecondNoNutrition")
			{
				this.m_EnergyConsumptionPerSecondNoNutrition = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "EnergyConsumptionPerSecondFever")
			{
				this.m_EnergyConsumptionPerSecondFever = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "EnergyConsumptionPerSecondFoodPoison")
			{
				this.m_EnergyConsumptionPerSecondFoodPoison = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HealtLossPerSecondNoNutrition")
			{
				this.m_HealthLossPerSecondNoNutrition = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HealthLossPerSecondNoHydration")
			{
				this.m_HealthLossPerSecondNoHydration = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HealthRecoveryPerDayEasyMode")
			{
				this.m_HealthRecoveryPerDayEasyMode = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HealthRecoveryPerDayNormalMode")
			{
				this.m_HealthRecoveryPerDayNormalMode = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HealthRecoveryPerDayHardMode")
			{
				this.m_HealthRecoveryPerDayHardMode = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionCarbohydratesConsumptionPerSecond")
			{
				this.m_NutritionCarbohydratesConsumptionPerSecond = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionFatConsumptionPerSecond")
			{
				this.m_NutritionFatConsumptionPerSecond = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionProteinsConsumptionPerSecond")
			{
				this.m_NutritionProteinsConsumptionPerSecond = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionFatConsumptionMulNoCarbs")
			{
				this.m_NutritionFatConsumptionMulNoCarbs = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionProteinsConsumptionMulNoCarbs")
			{
				this.m_NutritionProteinsConsumptionMulNoCarbs = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionCarbohydratesConsumptionRunMul")
			{
				this.m_NutritionCarbohydratesConsumptionRunMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionFatConsumptionRunMul")
			{
				this.m_NutritionFatConsumptionRunMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionProteinsConsumptionRunMul")
			{
				this.m_NutritionProteinsConsumptionRunMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionCarbohydratesConsumptionActionMul")
			{
				this.m_NutritionCarbohydratesConsumptionActionMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionFatConsumptionActionMul")
			{
				this.m_NutritionFatConsumptionActionMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionProteinsConsumptionActionMul")
			{
				this.m_NutritionProteinsConsumptionActionMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionCarbohydratesConsumptionWeightNormalMul")
			{
				this.m_NutritionCarbohydratesConsumptionWeightNormalMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionFatConsumptionWeightNormalMul")
			{
				this.m_NutritionFatConsumptionWeightNormalMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionFatConsumptionWeightNormalMul")
			{
				this.m_NutritionFatConsumptionWeightNormalMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionProteinsConsumptionWeightNormalMul")
			{
				this.m_NutritionProteinsConsumptionWeightNormalMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionCarbohydratesConsumptionWeightOverloadMul")
			{
				this.m_NutritionCarbohydratesConsumptionWeightOverloadMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionFatConsumptionWeightOverloadMul")
			{
				this.m_NutritionFatConsumptionWeightOverloadMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionProteinsConsumptionWeightOverloadMul")
			{
				this.m_NutritionProteinsConsumptionWeightOverloadMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionCarbohydratesConsumptionWeightCriticalMul")
			{
				this.m_NutritionCarbohydratesConsumptionWeightCriticalMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionFatConsumptionWeightCriticalMul")
			{
				this.m_NutritionFatConsumptionWeightCriticalMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "NutritionProteinsConsumptionWeightCriticalMul")
			{
				this.m_NutritionProteinsConsumptionWeightCriticalMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HydrationConsumptionPerSecond")
			{
				this.m_HydrationConsumptionPerSecond = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HydrationConsumptionDuringFeverPerSecond")
			{
				this.m_HydrationConsumptionDuringFeverPerSecond = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HydrationConsumptionRunMul")
			{
				this.m_HydrationConsumptionRunMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HydrationDecreaseJump")
			{
				this.m_HydrationDecreaseJump = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "EnergyLossDueLackOfNutritionPerSecond")
			{
				this.m_EnergyLossDueLackOfNutritionPerSecond = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "EnergyRecoveryDueNutritionPerSecond")
			{
				this.m_EnergyRecoveryDueNutritionPerSecond = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "EnergyRecoveryDueHydrationPerSecond")
			{
				this.m_EnergyRecoveryDueHydrationPerSecond = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HealthLossPerSecondNoOxygen")
			{
				this.m_HealthLossPerSecondNoOxygen = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "DirtinessIncreasePerSecond")
			{
				this.m_DirtinessIncreasePerSecond = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "DirtAddChoppingPlants")
			{
				this.m_DirtAddChoppingPlants = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "DirtAddTakeAnimalDroppings")
			{
				this.m_DirtAddTakeAnimalDroppings = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "DirtAddPlow")
			{
				this.m_DirtAddPlow = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "DirtAddPickickgUpHeavyObject")
			{
				this.m_DirtAddPickickgUpHeavyObject = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "DirtAddSleepingOnGround")
			{
				this.m_DirtAddSleepingOnGround = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "DirtAddUsingMud")
			{
				this.m_DirtAddUsingMud = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "DirtAddCombat")
			{
				this.m_DirtAddCombat = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "DirtAddLossConsciousness")
			{
				this.m_DirtAddLossConsciousness = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "StaminaConsumptionWalkPerSecond")
			{
				this.m_StaminaConsumptionWalkPerSecond = key.GetVariable(0).FValue;
			}
		}
	}

	public float GetMaxStamina()
	{
		return this.m_MaxStamina;
	}

	public float GetStamina()
	{
		if (ScenarioManager.Get().IsDream())
		{
			return this.m_MaxStamina;
		}
		return this.m_Stamina;
	}

	public bool IsStaminaLevel(float level)
	{
		return this.GetStamina() / this.m_MaxEnergy <= level;
	}

	public bool IsLowStamina()
	{
		return this.m_IsLowStamina;
	}

	public bool IsStaminaCriticalLevel()
	{
		return this.GetStamina() / this.m_MaxEnergy <= this.m_CriticalLevel;
	}

	public float GetMaxEnergy()
	{
		return this.m_MaxEnergy;
	}

	public bool IsHPCriticalLevel()
	{
		return this.m_HP / this.m_MaxHP <= this.m_CriticalLevel;
	}

	public float GetHP()
	{
		return this.m_HP;
	}

	public float GetMaxHP()
	{
		return this.m_MaxHP;
	}

	public float GetEnergy()
	{
		if (ScenarioManager.Get().IsDream())
		{
			return this.m_MaxEnergy;
		}
		return this.m_Energy;
	}

	public bool IsEnergyCriticalLevel()
	{
		return !ScenarioManager.Get().IsDream() && this.m_HP / this.m_MaxHP <= this.m_CriticalLevel;
	}

	public float GetMaxNutritionFat()
	{
		return this.m_MaxNutritionFat;
	}

	public float GetNutritionFat()
	{
		if (ScenarioManager.Get().IsDream())
		{
			return this.m_MaxNutritionFat;
		}
		return this.m_NutritionFat;
	}

	public bool IsNutritionFatCriticalLevel()
	{
		return this.m_NutritionFat / this.m_MaxNutritionFat <= this.m_CriticalLevel;
	}

	public bool IsNutritionFatHealingLevel()
	{
		return this.m_NutritionFat / this.m_MaxNutritionFat >= this.m_HealingLevel;
	}

	public float GetMaxNutritionCarbo()
	{
		return this.m_MaxNutritionCarbo;
	}

	public float GetNutritionCarbo()
	{
		if (ScenarioManager.Get().IsDream())
		{
			return this.m_MaxNutritionCarbo;
		}
		return this.m_NutritionCarbo;
	}

	public bool IsNutritionCarboCriticalLevel()
	{
		return this.m_NutritionCarbo / this.m_MaxNutritionCarbo <= this.m_CriticalLevel;
	}

	public bool IsNutritionCarboHealingLevel()
	{
		return this.m_NutritionCarbo / this.m_MaxNutritionCarbo >= this.m_HealingLevel;
	}

	public float GetMaxNutritionProtein()
	{
		return this.m_MaxNutritionProteins;
	}

	public float GetNutritionProtein()
	{
		if (ScenarioManager.Get().IsDream())
		{
			return this.m_MaxNutritionProteins;
		}
		return this.m_NutritionProteins;
	}

	public bool IsNutritionProteinsCriticalLevel()
	{
		return this.m_NutritionProteins / this.m_MaxNutritionProteins <= this.m_CriticalLevel;
	}

	public bool IsNutritionProteinsHealingLevel()
	{
		return this.m_NutritionProteins / this.m_MaxNutritionProteins >= this.m_HealingLevel;
	}

	public bool IsOxygenCriticalLevel()
	{
		return this.m_Oxygen / this.m_MaxOxygen <= this.m_CriticalLevel;
	}

	public float GetMaxHydration()
	{
		return this.m_MaxHydration;
	}

	public float GetHydration()
	{
		if (ScenarioManager.Get().IsDream())
		{
			return this.m_MaxHydration;
		}
		return this.m_Hydration;
	}

	public bool IsHydrationCriticalLevel()
	{
		return this.m_Hydration / this.m_MaxHydration <= this.m_CriticalLevel;
	}

	public float GetHydrationDecreaseJump()
	{
		return this.m_HydrationDecreaseJump;
	}

	public bool IsStaminaDepleted()
	{
		return this.GetStamina() < this.m_StaminaDepletedLevel;
	}

	public void DecreaseStamina(StaminaDecreaseReason reason, float mul = 1f)
	{
		if (Cheats.m_GodMode)
		{
			return;
		}
		if (this.m_StaminaDecreaseMap.ContainsKey((int)reason))
		{
			this.DecreaseStamina(this.m_StaminaDecreaseMap[(int)reason] * mul);
		}
		PlayerConditionModule.OnStaminaDecreasedDel onStaminaDecreasedEvent = this.OnStaminaDecreasedEvent;
		if (onStaminaDecreasedEvent == null)
		{
			return;
		}
		onStaminaDecreasedEvent(reason, this.GetStamina());
	}

	public void DecreaseStamina(float value)
	{
		if (Cheats.m_GodMode)
		{
			return;
		}
		this.m_Stamina -= value * (PlayerCocaineModule.Get().m_Active ? PlayerCocaineModule.Get().m_StaminaConsumptionMul : 1f);
		this.m_Stamina = Mathf.Clamp(this.m_Stamina, 0f, this.m_MaxStamina);
		if (value > 0f)
		{
			this.m_LastDecreaseStaminaTime = Time.time;
		}
	}

	public float GetStaminaDecrease(StaminaDecreaseReason reason)
	{
		return this.m_StaminaDecreaseMap[(int)reason];
	}

	public void DecreaseEnergy(EnergyDecreaseReason reason)
	{
		if (Cheats.m_GodMode || this.m_LossParametersBlocked)
		{
			return;
		}
		if (this.m_EnergyDecreaseMap.ContainsKey((int)reason))
		{
			this.DecreaseEnergy(this.m_EnergyDecreaseMap[(int)reason]);
		}
	}

	public void DecreaseEnergy(float val)
	{
		if (DifficultySettings.ActivePreset.m_Energy)
		{
			this.m_Energy -= val;
			this.m_Energy = Mathf.Clamp(this.m_Energy, 0f, this.m_MaxEnergy);
		}
	}

	public void DecreaseNutritionCarbo(float val)
	{
		if (DifficultySettings.ActivePreset.m_NutrientsDepletion == NutrientsDepletion.Off || Cheats.m_GodMode || this.m_LossParametersBlocked)
		{
			return;
		}
		this.m_NutritionCarbo -= val * (PlayerCocaineModule.Get().m_Active ? PlayerCocaineModule.Get().m_CarboConsumptionMul : 1f);
		this.m_NutritionCarbo = Mathf.Clamp(this.m_NutritionCarbo, 0f, this.m_MaxNutritionCarbo);
	}

	public void DecreaseNutritionFat(float val)
	{
		if (DifficultySettings.ActivePreset.m_NutrientsDepletion == NutrientsDepletion.Off || Cheats.m_GodMode || this.m_LossParametersBlocked)
		{
			return;
		}
		this.m_NutritionFat -= val * (PlayerCocaineModule.Get().m_Active ? PlayerCocaineModule.Get().m_FatConsumptionMul : 1f);
		this.m_NutritionFat = Mathf.Clamp(this.m_NutritionFat, 0f, this.m_MaxNutritionFat);
	}

	public void DecreaseNutritionProtein(float val)
	{
		if (DifficultySettings.ActivePreset.m_NutrientsDepletion == NutrientsDepletion.Off || Cheats.m_GodMode || this.m_LossParametersBlocked)
		{
			return;
		}
		this.m_NutritionProteins -= val * (PlayerCocaineModule.Get().m_Active ? PlayerCocaineModule.Get().m_ProteinsConsumptionMul : 1f);
		this.m_NutritionProteins = Mathf.Clamp(this.m_NutritionProteins, 0f, this.m_MaxNutritionProteins);
	}

	public float IncreaseNutritionFat(float val)
	{
		if (val <= 0f)
		{
			return 0f;
		}
		float nutritionFat = this.m_NutritionFat;
		this.m_NutritionFat += val;
		this.m_NutritionFat = Mathf.Clamp(this.m_NutritionFat, 0f, this.m_MaxNutritionFat);
		return this.m_NutritionFat - nutritionFat;
	}

	public float IncreaseNutritionProteins(float val)
	{
		if (val <= 0f)
		{
			return 0f;
		}
		float nutritionProteins = this.m_NutritionProteins;
		this.m_NutritionProteins += val;
		this.m_NutritionProteins = Mathf.Clamp(this.m_NutritionProteins, 0f, this.m_MaxNutritionProteins);
		return this.m_NutritionProteins - nutritionProteins;
	}

	public float IncreaseNutritionCarbo(float val)
	{
		if (val <= 0f)
		{
			return 0f;
		}
		float nutritionCarbo = this.m_NutritionCarbo;
		this.m_NutritionCarbo += val;
		this.m_NutritionCarbo = Mathf.Clamp(this.m_NutritionCarbo, 0f, this.m_MaxNutritionCarbo);
		return this.m_NutritionCarbo - nutritionCarbo;
	}

	public void IncreaseEnergy(float val)
	{
		this.m_Energy += val;
		this.m_Energy = Mathf.Clamp(this.m_Energy, 0f, this.m_MaxEnergy);
		if (val > 0f)
		{
			this.m_IncreaseEnergyLastTime = Time.time;
		}
	}

	public void IncreaseHP(float val)
	{
		if (Cheats.m_GodMode && val < 0f)
		{
			return;
		}
		if (this.m_HP <= 0f)
		{
			return;
		}
		this.m_HP += val;
		this.m_HP = Mathf.Clamp(this.m_HP, 0f, this.m_MaxHP);
		if (val > 0f)
		{
			this.m_IncreaseHPLastTime = Time.time;
		}
	}

	public override void Update()
	{
		base.Update();
		this.UpdateStamina();
		this.UpdateEnergy();
		this.UpdateMaxHP();
		this.UpdateHP();
		this.UpdateMaxStamina();
		this.UpdateNutrition();
		this.UpdateHydration();
		this.UpdateOxygen();
		this.UpdateDirtiness();
	}

	private void UpdateStamina()
	{
		float deltaTime = Time.deltaTime;
		FPPController fppcontroller = this.m_Player.m_FPPController;
		if (!fppcontroller)
		{
			return;
		}
		if (Cheats.m_GodMode)
		{
			this.m_Stamina = this.m_MaxStamina;
			return;
		}
		float num = this.m_Stamina;
		bool flag = Player.Get().GetCurrentItem() && Player.Get().GetCurrentItem().m_Info.IsHeavyObject();
		if (fppcontroller.IsActive() && fppcontroller.IsWalking())
		{
			num -= this.m_StaminaConsumptionWalkPerSecond * deltaTime * (PlayerCocaineModule.Get().m_Active ? PlayerCocaineModule.Get().m_StaminaConsumptionMul : 1f) * (flag ? 1.6f : 1f);
		}
		else if (fppcontroller.IsActive() && fppcontroller.IsRunning())
		{
			num -= this.m_StaminaConsumptionRunPerSecond * deltaTime * (PlayerCocaineModule.Get().m_Active ? PlayerCocaineModule.Get().m_StaminaConsumptionMul : 1f) * (flag ? 1.6f : 1f);
		}
		else if (fppcontroller.IsActive() && fppcontroller.IsDepleted())
		{
			num -= this.m_StaminaConsumptionDepletedPerSecond * deltaTime * (PlayerCocaineModule.Get().m_Active ? PlayerCocaineModule.Get().m_StaminaConsumptionMul : 1f);
		}
		else if (!MakeFireController.Get().IsActive())
		{
			num += this.m_StaminaRegenerationPerSecond * deltaTime;
		}
		if (num < this.m_Stamina || Time.time - this.m_LastDecreaseStaminaTime >= this.m_StaminaRenerationDelay)
		{
			this.m_Stamina = num;
		}
		if (this.m_Stamina - this.m_PrevStamina < 0f)
		{
			this.m_LastDecreaseStaminaTime = Time.time;
		}
		this.m_Stamina = Mathf.Clamp(this.m_Stamina, 0f, this.m_MaxStamina);
		if (this.m_IsLowStamina)
		{
			if (this.m_MaxStamina < this.m_LowStaminaRecoveryLevel)
			{
				this.m_IsLowStamina = (this.m_Stamina < this.m_LowStaminaLevel && this.m_Stamina < this.m_MaxStamina);
			}
			else if (this.m_Stamina >= this.m_LowStaminaRecoveryLevel)
			{
				this.m_IsLowStamina = false;
			}
		}
		else if (this.m_Stamina < this.m_LowStaminaLevel && this.m_Stamina < this.m_MaxStamina)
		{
			this.m_IsLowStamina = true;
		}
		this.m_PrevStamina = this.m_Stamina;
	}

	private void UpdateOxygen()
	{
		if (this.m_Player.GetComponent<SwimController>().IsActive() && this.m_Player.GetComponent<SwimController>().m_State == SwimState.Dive && !Player.Get().m_InfinityDiving)
		{
			this.m_Oxygen -= this.m_OxygenConsumptionPerSecond * Time.deltaTime;
		}
		else
		{
			this.m_Oxygen = this.m_MaxOxygen;
		}
		if (this.m_Oxygen <= 0f && this.m_LastOxygen > 0f)
		{
			PlayerAudioModule.Get().PlayNoOxygenDivingSounds();
		}
		this.m_LastOxygen = this.m_Oxygen;
	}

	private void UpdateEnergy()
	{
		if (ScenarioManager.Get().IsDream())
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		if (!this.m_Player.m_FPPController)
		{
			return;
		}
		if (this.m_Player.m_DeathController.IsActive())
		{
			return;
		}
		if (this.m_Player.GetComponent<ConsciousnessController>().IsActive())
		{
			return;
		}
		float num = 1f;
		Insomnia insomnia = (Insomnia)this.m_DiseasesModule.GetDisease(ConsumeEffect.Insomnia);
		if (insomnia.IsActive())
		{
			num = insomnia.m_EnergyLossMulFinal;
		}
		if (DifficultySettings.ActivePreset.m_Energy && !Cheats.m_GodMode && !this.m_LossParametersBlocked)
		{
			this.m_Energy -= this.m_EnergyConsumptionPerSecond * deltaTime * num;
			if (this.IsNutritionCarboCriticalLevel() || this.IsNutritionFatCriticalLevel() || this.IsNutritionProteinsCriticalLevel())
			{
				this.m_Energy -= this.m_EnergyConsumptionPerSecondNoNutrition * deltaTime * num;
			}
			if (this.m_DiseasesModule.GetDisease(ConsumeEffect.Fever).IsActive())
			{
				this.m_Energy -= this.m_EnergyConsumptionPerSecondFever * deltaTime * num;
			}
			if (this.m_DiseasesModule.GetDisease(ConsumeEffect.FoodPoisoning).IsActive())
			{
				this.m_Energy -= this.m_EnergyConsumptionPerSecondFoodPoison * deltaTime * num;
			}
		}
		this.m_Energy = Mathf.Clamp(this.m_Energy, 0f, this.m_MaxEnergy);
		if (this.m_Energy <= PlayerSanityModule.Get().m_LowEnegryWhispersLevel)
		{
			PlayerSanityModule.Get().OnWhispersEvent(PlayerSanityModule.WhisperType.LowEnergy);
		}
	}

	private void UpdateMaxHP()
	{
		this.m_MaxHP = this.m_Hydration * 0.25f + this.m_NutritionFat * 0.25f + this.m_NutritionCarbo * 0.25f + this.m_NutritionProteins * 0.25f;
		this.m_MaxHP = Mathf.Clamp(this.m_MaxHP, 0f, 100f);
	}

	private void UpdateHP()
	{
		if (ScenarioManager.Get().IsDream())
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		if (!this.m_Player.m_FPPController)
		{
			return;
		}
		if (this.m_Player.m_DeathController.IsActive())
		{
			return;
		}
		if (this.m_Player.GetComponent<ConsciousnessController>().IsActive())
		{
			return;
		}
		if (!Cheats.m_GodMode)
		{
			if (this.IsNutritionCarboCriticalLevel() || this.IsNutritionFatCriticalLevel() || this.IsNutritionProteinsCriticalLevel())
			{
				this.IncreaseHP(-this.m_HealthLossPerSecondNoNutrition * deltaTime);
			}
			if (this.IsHydrationCriticalLevel())
			{
				this.IncreaseHP(-this.m_HealthLossPerSecondNoHydration * deltaTime);
			}
			bool flag = true;
			List<Injury> injuriesList = this.m_InjuryModule.GetInjuriesList();
			for (int i = 0; i < injuriesList.Count; i++)
			{
				if (injuriesList[i].m_ParentInjury == null && injuriesList[i].m_Type != InjuryType.Worm && injuriesList[i].m_Type != InjuryType.Leech)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				float num = (MainLevel.Instance.m_TODTime.m_DayLengthInMinutes + MainLevel.Instance.m_TODTime.m_NightLengthInMinutes) * 60f;
				float num2 = this.m_HealthRecoveryPerDayNormalMode;
				GameDifficulty baseDifficulty = DifficultySettings.ActivePreset.m_BaseDifficulty;
				if (baseDifficulty == GameDifficulty.Easy)
				{
					num2 = this.m_HealthRecoveryPerDayEasyMode;
				}
				else if (baseDifficulty == GameDifficulty.Hard)
				{
					num2 = this.m_HealthRecoveryPerDayHardMode;
				}
				if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
				{
					this.IncreaseHP(num2 / num * Player.GetSleepTimeFactor());
				}
				else
				{
					this.IncreaseHP(num2 / num * deltaTime);
				}
			}
			if (this.m_Oxygen <= 0f)
			{
				this.IncreaseHP(-this.m_HealthLossPerSecondNoOxygen * deltaTime);
			}
		}
		this.m_HP = Mathf.Clamp(this.m_HP, 0f, this.m_MaxHP);
		if (this.m_HP < 10f)
		{
			if (!this.m_AudioModule.IsHeartBeatSoundPlaying())
			{
				this.m_AudioModule.PlayHeartBeatSound(1f, true);
				return;
			}
		}
		else if (this.m_AudioModule.IsHeartBeatSoundPlaying())
		{
			this.m_AudioModule.StopHeartBeatSound();
		}
	}

	private void UpdateMaxStamina()
	{
		this.m_MaxStamina = this.m_Energy;
		this.m_MaxStamina = Mathf.Clamp(this.m_MaxStamina, 0f, 100f);
	}

	private void UpdateNutrition()
	{
		if (this.m_ParasiteSickness == null)
		{
			this.m_ParasiteSickness = (ParasiteSickness)PlayerDiseasesModule.Get().GetDisease(ConsumeEffect.ParasiteSickness);
		}
		if (ScenarioManager.Get().IsDream())
		{
			return;
		}
		if (Cheats.m_GodMode || this.m_LossParametersBlocked)
		{
			return;
		}
		FPPController fppcontroller = this.m_Player.m_FPPController;
		if (!fppcontroller)
		{
			return;
		}
		NutrientsDepletion nutrientsDepletion = DifficultySettings.ActivePreset.m_NutrientsDepletion;
		if (nutrientsDepletion == NutrientsDepletion.Off)
		{
			return;
		}
		WeaponController weaponController = this.m_Player.m_WeaponController;
		bool flag = false;
		if (weaponController && weaponController.IsAttack())
		{
			flag = true;
		}
		if (!flag && this.m_Player.GetCurrentItem(Hand.Right) && this.m_Player.GetCurrentItem(Hand.Right).m_Info.IsHeavyObject())
		{
			flag = true;
		}
		float num = Time.deltaTime;
		if (ConsciousnessController.Get().IsUnconscious())
		{
			num = Player.GetUnconsciousTimeFactor();
		}
		float num2 = 1f;
		float num3 = 1f;
		float num4 = 1f;
		if (fppcontroller.IsRunning())
		{
			num2 *= this.m_NutritionCarbohydratesConsumptionRunMul;
			num3 *= this.m_NutritionFatConsumptionRunMul;
			num4 *= this.m_NutritionProteinsConsumptionRunMul;
		}
		if (flag)
		{
			num2 *= this.m_NutritionCarbohydratesConsumptionActionMul;
			num3 *= this.m_NutritionFatConsumptionActionMul;
			num4 *= this.m_NutritionProteinsConsumptionActionMul;
		}
		if (this.IsNutritionCarboCriticalLevel())
		{
			num3 *= this.m_NutritionFatConsumptionMulNoCarbs;
			num4 *= this.m_NutritionProteinsConsumptionMulNoCarbs;
		}
		if (InventoryBackpack.Get().IsCriticalOverload())
		{
			num2 *= this.m_NutritionCarbohydratesConsumptionWeightCriticalMul;
			num3 *= this.m_NutritionFatConsumptionWeightCriticalMul;
			num4 *= this.m_NutritionProteinsConsumptionWeightCriticalMul;
		}
		else if (InventoryBackpack.Get().IsOverload())
		{
			num2 *= this.m_NutritionCarbohydratesConsumptionWeightOverloadMul;
			num3 *= this.m_NutritionFatConsumptionWeightOverloadMul;
			num4 *= this.m_NutritionProteinsConsumptionWeightOverloadMul;
		}
		else
		{
			num2 *= this.m_NutritionCarbohydratesConsumptionWeightNormalMul;
			num3 *= this.m_NutritionFatConsumptionWeightNormalMul;
			num4 *= this.m_NutritionProteinsConsumptionWeightNormalMul;
		}
		if (this.m_ParasiteSickness.IsActive())
		{
			num2 *= this.m_ParasiteSickness.m_MacroNutricientCarboLossMul * (float)this.m_ParasiteSickness.m_Level;
			num3 *= this.m_ParasiteSickness.m_MacroNutricientFatLossMul * (float)this.m_ParasiteSickness.m_Level;
			num4 *= this.m_ParasiteSickness.m_MacroNutricientProteinsLossMul * (float)this.m_ParasiteSickness.m_Level;
		}
		if (nutrientsDepletion == NutrientsDepletion.Normal)
		{
			float s_NormalModeLossMul = GreenHellGame.s_NormalModeLossMul;
			num2 *= s_NormalModeLossMul;
			num3 *= s_NormalModeLossMul;
			num4 *= s_NormalModeLossMul;
		}
		else if (nutrientsDepletion == NutrientsDepletion.Low)
		{
			float s_EasyModeLossMul = GreenHellGame.s_EasyModeLossMul;
			num2 *= s_EasyModeLossMul;
			num3 *= s_EasyModeLossMul;
			num4 *= s_EasyModeLossMul;
		}
		this.m_NutritionCarbo -= this.m_NutritionCarbohydratesConsumptionPerSecond * num * num2 * (PlayerCocaineModule.Get().m_Active ? PlayerCocaineModule.Get().m_CarboConsumptionMul : 1f);
		this.m_NutritionCarbo = Mathf.Clamp(this.m_NutritionCarbo, 0f, this.m_MaxNutritionCarbo);
		this.m_NutritionFat -= this.m_NutritionFatConsumptionPerSecond * num * num3 * (PlayerCocaineModule.Get().m_Active ? PlayerCocaineModule.Get().m_FatConsumptionMul : 1f);
		this.m_NutritionFat = Mathf.Clamp(this.m_NutritionFat, 0f, this.m_MaxNutritionFat);
		this.m_NutritionProteins -= this.m_NutritionProteinsConsumptionPerSecond * num * num4 * (PlayerCocaineModule.Get().m_Active ? PlayerCocaineModule.Get().m_ProteinsConsumptionMul : 1f);
		this.m_NutritionProteins = Mathf.Clamp(this.m_NutritionProteins, 0f, this.m_MaxNutritionProteins);
	}

	public float IncreaseHydration(float amount)
	{
		if (amount <= 0f)
		{
			return 0f;
		}
		float hydration = this.m_Hydration;
		this.m_Hydration += amount;
		this.m_Hydration = Mathf.Clamp(this.m_Hydration, 0f, this.m_MaxHydration);
		return this.m_Hydration - hydration;
	}

	public void DecreaseHydration(float amount)
	{
		if (DifficultySettings.ActivePreset.m_NutrientsDepletion == NutrientsDepletion.Off || Cheats.m_GodMode || this.m_LossParametersBlocked)
		{
			return;
		}
		this.m_Hydration -= amount * (PlayerCocaineModule.Get().m_Active ? PlayerCocaineModule.Get().m_HydrationConsumptionMul : 1f);
		this.m_Hydration = Mathf.Clamp(this.m_Hydration, 0f, this.m_MaxHydration);
	}

	private void UpdateHydration()
	{
		if (ScenarioManager.Get().IsDream())
		{
			return;
		}
		FPPController fppcontroller = this.m_Player.m_FPPController;
		if (!fppcontroller)
		{
			return;
		}
		NutrientsDepletion nutrientsDepletion = DifficultySettings.ActivePreset.m_NutrientsDepletion;
		if (nutrientsDepletion == NutrientsDepletion.Off)
		{
			return;
		}
		float num = 0f;
		float num2 = 1f;
		if (!Cheats.m_GodMode || this.m_LossParametersBlocked)
		{
			float deltaTime = Time.deltaTime;
			if (fppcontroller.IsRunning())
			{
				num2 *= this.m_HydrationConsumptionRunMul;
			}
			num = this.m_HydrationConsumptionPerSecond * deltaTime * num2;
			if (PlayerDiseasesModule.Get().GetDisease(ConsumeEffect.Fever).IsActive())
			{
				num += this.m_HydrationConsumptionDuringFeverPerSecond * deltaTime;
			}
			for (int i = 0; i < this.m_InjuryModule.m_Injuries.Count; i++)
			{
				Injury injury = this.m_InjuryModule.m_Injuries[i];
				if (injury.m_Type == InjuryType.VenomBite || injury.m_Type == InjuryType.SnakeBite)
				{
					num += Injury.s_PoisonedWoundHydrationDecPerSec * deltaTime;
				}
			}
		}
		if (nutrientsDepletion == NutrientsDepletion.Normal)
		{
			num *= GreenHellGame.s_NormalModeLossMul;
		}
		else if (nutrientsDepletion == NutrientsDepletion.Low)
		{
			num *= GreenHellGame.s_EasyModeLossMul;
		}
		this.DecreaseHydration(num);
	}

	public override void OnTakeDamage(DamageInfo info)
	{
		if (!DifficultySettings.ActivePreset.m_HPLoss && !info.m_FromDamageSensor)
		{
			return;
		}
		float num = info.m_Damage;
		info.m_InjuryPlace = PlayerInjuryModule.Get().GetInjuryPlaceFromHit(info);
		if (!info.m_FromInjury)
		{
			if (info.m_Damager != null && info.m_Damager.GetComponent<AI>() != null)
			{
				this.GetDirtinessAdd(GetDirtyReason.Combat, null);
			}
			Limb limb = EnumTools.ConvertInjuryPlaceToLimb(info.m_InjuryPlace);
			if (limb == Limb.None)
			{
				limb = Limb.LArm;
			}
			if (info.m_DamageType != DamageType.Fall && info.m_DamageType != DamageType.SnakePoison && info.m_DamageType != DamageType.VenomPoison && info.m_DamageType != DamageType.Insects && info.m_DamageType != DamageType.Infection)
			{
				num = info.m_Damage * (1f - PlayerArmorModule.Get().GetAbsorption(limb));
			}
			PlayerArmorModule.Get().SetPhaseCompleted(ArmorTakeDamagePhase.ConditionModule);
		}
		if (info.m_Blocked)
		{
			this.DecreaseStamina(Player.Get().GetCurrentItem() ? StaminaDecreaseReason.PuchWeaponBlock : StaminaDecreaseReason.PuchBlock, 1f);
			return;
		}
		this.m_HP -= num;
		if (this.m_HP <= 0f)
		{
			DeathController.DeathType type = DeathController.DeathType.Normal;
			if (info.m_AIType == AI.AIID.BlackCaiman)
			{
				type = DeathController.DeathType.Caiman;
			}
			else if (info.m_DamageType == DamageType.Claws)
			{
				type = DeathController.DeathType.Predator;
			}
			else if (info.m_AIType == AI.AIID.Piranha)
			{
				type = DeathController.DeathType.Piranha;
			}
			else if (info.m_DamageType == DamageType.Cut)
			{
				type = DeathController.DeathType.Cut;
			}
			else if (info.m_DamageType == DamageType.Fall)
			{
				type = DeathController.DeathType.Fall;
			}
			else if (info.m_DamageType == DamageType.Insects)
			{
				type = DeathController.DeathType.Insects;
			}
			else if (info.m_DamageType == DamageType.Melee)
			{
				type = DeathController.DeathType.Melee;
			}
			else if (info.m_DamageType == DamageType.SnakePoison)
			{
				type = DeathController.DeathType.Poison;
			}
			else if (info.m_DamageType == DamageType.Thrust)
			{
				type = DeathController.DeathType.Thrust;
			}
			else if (info.m_DamageType == DamageType.VenomPoison)
			{
				type = DeathController.DeathType.Poison;
			}
			else if (info.m_DamageType == DamageType.Infection)
			{
				type = DeathController.DeathType.Infection;
			}
			else if (SwimController.Get().IsActive())
			{
				if (SwimController.Get().GetState() == SwimState.Dive)
				{
					type = DeathController.DeathType.UnderWater;
				}
				else
				{
					type = DeathController.DeathType.OnWater;
				}
			}
			Player.Get().Die(type);
		}
	}

	public void Save()
	{
		SaveGame.SaveVal("PCM_HP", this.m_HP);
		SaveGame.SaveVal("PCM_Energy", this.m_Energy);
		SaveGame.SaveVal("PCM_Hydration", this.m_Hydration);
		SaveGame.SaveVal("PCM_NutritionCarbo", this.m_NutritionCarbo);
		SaveGame.SaveVal("PCM_NutritionFat", this.m_NutritionFat);
		SaveGame.SaveVal("PCM_NutritionProteins", this.m_NutritionProteins);
		SaveGame.SaveVal("LossParametersBlocked", this.m_LossParametersBlocked);
		SaveGame.SaveVal("PCM_Dirtiness", this.m_Dirtiness);
	}

	public void Load()
	{
		float num = 0f;
		SaveGame.LoadVal("PCM_HP", out num, false);
		this.m_HP = num;
		SaveGame.LoadVal("PCM_Energy", out num, false);
		this.m_Energy = num;
		SaveGame.LoadVal("PCM_Hydration", out num, false);
		this.m_Hydration = num;
		SaveGame.LoadVal("PCM_NutritionCarbo", out num, false);
		this.m_NutritionCarbo = num;
		SaveGame.LoadVal("PCM_NutritionFat", out num, false);
		this.m_NutritionFat = num;
		SaveGame.LoadVal("PCM_NutritionProteins", out num, false);
		this.m_NutritionProteins = num;
		this.m_LossParametersBlocked = SaveGame.LoadBVal("LossParametersBlocked");
		if (SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate11)
		{
			SaveGame.LoadVal("PCM_Dirtiness", out num, false);
			this.m_Dirtiness = num;
		}
	}

	public void BlockParametersLoss()
	{
		this.m_LossParametersBlocked = true;
	}

	public void UnblockParametersLoss()
	{
		this.m_LossParametersBlocked = false;
	}

	public bool GetParameterLossBlocked()
	{
		return this.m_LossParametersBlocked;
	}

	private void UpdateDirtiness()
	{
		bool flag = true;
		if (Player.Get().IsTakingShower())
		{
			this.m_Dirtiness -= 50f * Time.deltaTime;
			this.m_Dirtiness = Mathf.Clamp(this.m_Dirtiness, 0f, this.m_MaxDirtiness);
			flag = false;
		}
		if (SwimController.Get().IsActive())
		{
			if (SwimController.Get().GetState() == SwimState.Dive)
			{
				this.m_Dirtiness -= 30f * Time.deltaTime;
			}
			else
			{
				this.m_Dirtiness -= 10f * Time.deltaTime;
			}
			this.m_Dirtiness = Mathf.Clamp(this.m_Dirtiness, 0f, this.m_MaxDirtiness);
			flag = false;
		}
		if (RainManager.Get().IsRain() && !RainManager.Get().IsInRainCutter(base.transform.position))
		{
			this.m_Dirtiness -= 3f * Time.deltaTime;
			this.m_Dirtiness = Mathf.Clamp(this.m_Dirtiness, 0f, this.m_MaxDirtiness);
			flag = false;
		}
		if (Player.Get().m_Animator.GetBool(Player.Get().m_CleanUpHash))
		{
			this.m_Dirtiness -= 35f * Time.deltaTime;
			this.m_Dirtiness = Mathf.Clamp(this.m_Dirtiness, 0f, this.m_MaxDirtiness);
			flag = false;
		}
		if (!flag)
		{
			return;
		}
		if (this.m_BlockGettingDirty)
		{
			return;
		}
		float num = Time.deltaTime;
		if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
		{
			if (SleepController.Get().GetRestingPlace() != null)
			{
				num = 0f;
			}
			else
			{
				num = Player.GetSleepTimeFactor();
			}
		}
		this.m_Dirtiness += this.m_DirtinessIncreasePerSecond * num;
		this.m_Dirtiness = Mathf.Clamp(this.m_Dirtiness, 0f, this.m_MaxDirtiness);
	}

	public void BlockGettingDirty()
	{
		this.m_BlockGettingDirty = true;
	}

	public void UnblockGettingDirty()
	{
		this.m_BlockGettingDirty = false;
	}

	public void SetDirtiness(float set)
	{
		this.m_Dirtiness = 1f - set;
	}

	public void GetDirtinessAdd(GetDirtyReason reason, HeavyObjectInfo item_info = null)
	{
		if (this.m_BlockGettingDirty)
		{
			return;
		}
		switch (reason)
		{
		case GetDirtyReason.ChopPlants:
			this.m_Dirtiness += this.m_DirtAddChoppingPlants;
			break;
		case GetDirtyReason.HeavyObject:
			this.m_Dirtiness += item_info.m_DirtinessOnTake;
			break;
		case GetDirtyReason.SleepingOnGround:
			this.m_Dirtiness += this.m_DirtAddSleepingOnGround;
			break;
		case GetDirtyReason.UsingMud:
			this.m_Dirtiness += this.m_DirtAddUsingMud;
			break;
		case GetDirtyReason.Combat:
			this.m_Dirtiness += this.m_DirtAddCombat;
			break;
		case GetDirtyReason.LossConsciousness:
			this.m_Dirtiness += this.m_DirtAddLossConsciousness;
			break;
		case GetDirtyReason.TakeAnimalDroppings:
			this.m_Dirtiness += this.m_DirtAddTakeAnimalDroppings;
			break;
		case GetDirtyReason.Plow:
			this.m_Dirtiness += this.m_DirtAddPlow;
			break;
		}
		this.m_Dirtiness = Mathf.Clamp(this.m_Dirtiness, 0f, this.m_MaxDirtiness);
	}

	public void AddDirtiness(float dirt)
	{
		this.m_Dirtiness += dirt;
		this.m_Dirtiness = Mathf.Clamp(this.m_Dirtiness, 0f, this.m_MaxDirtiness);
	}

	private float m_MaxStamina = 100f;

	private bool m_IsLowStamina;

	private float m_PrevStamina;

	private float m_LastDecreaseStaminaTime;

	private float m_StaminaRenerationDelay = 2f;

	private float m_HPProp = 100f;

	private float m_MaxNutritionFat = 100f;

	private float m_MaxNutritionCarbo = 100f;

	private float m_MaxNutritionProteins = 100f;

	public float m_MaxHydration = 100f;

	public float m_MaxDirtiness = 100f;

	private float m_StaminaDepletedLevel = 5f;

	private float m_LowStaminaLevel = 10f;

	private float m_LowStaminaRecoveryLevel = 20f;

	public float m_MaxOxygen = 100f;

	public float m_Oxygen = 100f;

	private float m_StaminaConsumptionWalkPerSecond;

	private float m_StaminaConsumptionRunPerSecond;

	private float m_StaminaConsumptionDepletedPerSecond;

	private float m_StaminaRegenerationPerSecond;

	private float m_NutritionCarbohydratesConsumptionPerSecond = 1f;

	private float m_NutritionFatConsumptionPerSecond = 1f;

	private float m_NutritionProteinsConsumptionPerSecond = 1f;

	private float m_NutritionFatConsumptionMulNoCarbs = 1f;

	private float m_NutritionProteinsConsumptionMulNoCarbs = 1f;

	private float m_NutritionCarbohydratesConsumptionRunMul = 1f;

	private float m_NutritionFatConsumptionRunMul = 1f;

	private float m_NutritionProteinsConsumptionRunMul = 1f;

	private float m_NutritionCarbohydratesConsumptionActionMul = 1f;

	private float m_NutritionFatConsumptionActionMul = 2f;

	private float m_NutritionProteinsConsumptionActionMul = 3f;

	private float m_NutritionCarbohydratesConsumptionWeightNormalMul = 1f;

	private float m_NutritionFatConsumptionWeightNormalMul = 1f;

	private float m_NutritionProteinsConsumptionWeightNormalMul = 1f;

	private float m_NutritionCarbohydratesConsumptionWeightOverloadMul = 1.5f;

	private float m_NutritionFatConsumptionWeightOverloadMul = 1.5f;

	private float m_NutritionProteinsConsumptionWeightOverloadMul = 1.5f;

	private float m_NutritionCarbohydratesConsumptionWeightCriticalMul = 1.8f;

	private float m_NutritionFatConsumptionWeightCriticalMul = 1.8f;

	private float m_NutritionProteinsConsumptionWeightCriticalMul = 1.8f;

	private float m_HydrationConsumptionPerSecond = 0.5f;

	private float m_HydrationConsumptionRunMul = 0.5f;

	private float m_HydrationConsumptionDuringFeverPerSecond = 0.5f;

	private float m_OxygenConsumptionPerSecond = 1f;

	private float m_EnergyConsumptionPerSecond = 0.1f;

	private float m_EnergyConsumptionPerSecondNoNutrition = 0.1f;

	private float m_EnergyConsumptionPerSecondFever = 0.1f;

	private float m_EnergyConsumptionPerSecondFoodPoison = 0.1f;

	private float m_HealthLossPerSecondNoNutrition = 0.05f;

	private float m_HealthLossPerSecondNoHydration = 0.05f;

	private float m_HealthRecoveryPerDayEasyMode = 0.1f;

	private float m_HealthRecoveryPerDayNormalMode = 0.1f;

	private float m_HealthRecoveryPerDayHardMode = 0.1f;

	private float m_HealthLossPerSecondNoOxygen = 10f;

	private Dictionary<int, float> m_StaminaDecreaseMap = new Dictionary<int, float>();

	private Dictionary<int, float> m_EnergyDecreaseMap = new Dictionary<int, float>();

	private float m_HydrationDecreaseJump = 1f;

	private float m_EnergyLossDueLackOfNutritionPerSecond = 1f;

	private float m_EnergyRecoveryDueNutritionPerSecond = 1f;

	private float m_EnergyRecoveryDueHydrationPerSecond = 1f;

	private float m_DirtinessIncreasePerSecond = 0.1f;

	public float m_CriticalLevel = 0.3f;

	private TOD_Sky m_Sky;

	private SleepController m_PlayerSleepController;

	private static PlayerConditionModule s_Instance;

	private PlayerDiseasesModule m_DiseasesModule;

	private PlayerInjuryModule m_InjuryModule;

	private SwimController m_SwimController;

	private float m_HealingLevel = 0.25f;

	[HideInInspector]
	public float m_IncreaseEnergyLastTime = float.MinValue;

	[HideInInspector]
	public float m_IncreaseHPLastTime = float.MinValue;

	private bool m_LossParametersBlocked;

	private PlayerAudioModule m_AudioModule;

	private float m_DirtAddChoppingPlants = 0.01f;

	private float m_DirtAddPickickgUpHeavyObject = 0.01f;

	private float m_DirtAddSleepingOnGround = 0.01f;

	private float m_DirtAddUsingMud = 0.01f;

	private float m_DirtAddCombat = 0.01f;

	private float m_DirtAddLossConsciousness = 0.01f;

	private float m_DirtAddTakeAnimalDroppings = 0.01f;

	private float m_DirtAddPlow = 0.01f;

	private float m_LastOxygen = 100f;

	private ParasiteSickness m_ParasiteSickness;

	private bool m_BlockGettingDirty;

	public delegate void OnStaminaDecreasedDel(StaminaDecreaseReason reason, float stamina_val);
}
