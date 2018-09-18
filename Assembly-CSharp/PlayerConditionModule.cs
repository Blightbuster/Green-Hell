using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class PlayerConditionModule : PlayerModule, ISaveLoad
{
	public float m_Stamina { get; set; }

	public float m_MaxHP { get; set; }

	public float m_HP { get; set; }

	public float m_MaxEnergy { get; set; }

	public float m_Energy { get; set; }

	public float m_NutritionFat { get; set; }

	public float m_NutritionCarbo { get; set; }

	public float m_NutritionProteins { get; set; }

	public float m_Hydration { get; set; }

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

	public override void Initialize()
	{
		base.Initialize();
		this.ResetParams();
		this.ParseFile();
		this.m_Sky = UnityEngine.Object.FindObjectOfType<TOD_Sky>();
		this.m_PlayerSleepController = this.m_Player.GetComponent<SleepController>();
		this.m_DiseasesModule = this.m_Player.GetComponent<PlayerDiseasesModule>();
		this.m_InjuryModule = this.m_Player.GetComponent<PlayerInjuryModule>();
		this.m_SwimController = this.m_Player.GetComponent<SwimController>();
		this.m_AudioModule = this.m_Player.GetComponent<PlayerAudioModule>();
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
		}
		else
		{
			this.ParseFile("Player_condition");
		}
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
			else if (key.GetName() == "HealthRecoveryPerDay")
			{
				this.m_HealthRecoveryPerDay = key.GetVariable(0).FValue;
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
		}
	}

	public float GetMaxStamina()
	{
		return this.m_MaxStamina;
	}

	public float GetStamina()
	{
		return this.m_Stamina;
	}

	public bool IsStaminaLevel(float level)
	{
		return this.m_Stamina / this.m_MaxStamina <= level;
	}

	public bool IsStaminaCriticalLevel()
	{
		return this.m_Stamina / this.m_MaxStamina <= this.m_CriticalLevel;
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
		if (this.m_Player.m_DreamActive)
		{
			return this.m_MaxHP;
		}
		return this.m_HP;
	}

	public float GetMaxHP()
	{
		return this.m_MaxHP;
	}

	public float GetEnergy()
	{
		if (this.m_Player.m_DreamActive)
		{
			return this.m_MaxEnergy;
		}
		return this.m_Energy;
	}

	public bool IsEnergyCriticalLevel()
	{
		return !this.m_Player.m_DreamActive && this.m_HP / this.m_MaxHP <= this.m_CriticalLevel;
	}

	public float GetMaxNutritionFat()
	{
		return this.m_MaxNutritionFat;
	}

	public float GetNutritionFat()
	{
		if (this.m_Player.m_DreamActive)
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
		if (this.m_Player.m_DreamActive)
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
		if (this.m_Player.m_DreamActive)
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
		if (this.m_Player.m_DreamActive)
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
		return this.m_Stamina < this.m_StaminaDepletedLevel;
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
	}

	public void DecreaseStamina(float value)
	{
		if (Cheats.m_GodMode)
		{
			return;
		}
		this.m_Stamina -= value * ((!PlayerCocaineModule.Get().m_Active) ? 1f : PlayerCocaineModule.Get().m_StaminaConsumptionMul);
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
		this.m_Energy -= val;
		this.m_Energy = Mathf.Clamp(this.m_Energy, 0f, this.m_MaxEnergy);
	}

	public void DecreaseNutritionCarbo(float val)
	{
		if (Cheats.m_GodMode || this.m_LossParametersBlocked)
		{
			return;
		}
		this.m_NutritionCarbo -= val * ((!PlayerCocaineModule.Get().m_Active) ? 1f : PlayerCocaineModule.Get().m_CarboConsumptionMul);
		this.m_NutritionCarbo = Mathf.Clamp(this.m_NutritionCarbo, 0f, this.m_MaxNutritionCarbo);
	}

	public void DecreaseNutritionFat(float val)
	{
		if (Cheats.m_GodMode || this.m_LossParametersBlocked)
		{
			return;
		}
		this.m_NutritionFat -= val * ((!PlayerCocaineModule.Get().m_Active) ? 1f : PlayerCocaineModule.Get().m_FatConsumptionMul);
		this.m_NutritionFat = Mathf.Clamp(this.m_NutritionFat, 0f, this.m_MaxNutritionFat);
	}

	public void DecreaseNutritionProtein(float val)
	{
		if (Cheats.m_GodMode || this.m_LossParametersBlocked)
		{
			return;
		}
		this.m_NutritionProteins -= val * ((!PlayerCocaineModule.Get().m_Active) ? 1f : PlayerCocaineModule.Get().m_ProteinsConsumptionMul);
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
		if (fppcontroller.IsActive() && fppcontroller.IsWalking())
		{
			num -= this.m_StaminaConsumptionWalkPerSecond * deltaTime * ((!PlayerCocaineModule.Get().m_Active) ? 1f : PlayerCocaineModule.Get().m_StaminaConsumptionMul);
		}
		else if (fppcontroller.IsActive() && fppcontroller.IsRunning())
		{
			num -= this.m_StaminaConsumptionRunPerSecond * deltaTime * ((!PlayerCocaineModule.Get().m_Active) ? 1f : PlayerCocaineModule.Get().m_StaminaConsumptionMul);
		}
		else if (fppcontroller.IsActive() && fppcontroller.IsDepleted())
		{
			num -= this.m_StaminaConsumptionDepletedPerSecond * deltaTime * ((!PlayerCocaineModule.Get().m_Active) ? 1f : PlayerCocaineModule.Get().m_StaminaConsumptionMul);
		}
		else if (!MakeFireController.Get().IsActive())
		{
			num += this.m_StaminaRegenerationPerSecond * deltaTime;
		}
		if (num < this.m_Stamina || Time.time - this.m_LastDecreaseStaminaTime >= this.m_StaminaRenerationDelay)
		{
			this.m_Stamina = num;
		}
		float num2 = this.m_Stamina - this.m_PrevStamina;
		if (num2 < 0f)
		{
			this.m_LastDecreaseStaminaTime = Time.time;
		}
		this.m_Stamina = Mathf.Clamp(this.m_Stamina, 0f, this.m_MaxStamina);
		this.m_PrevStamina = this.m_Stamina;
	}

	private void UpdateOxygen()
	{
		if (this.m_Player.GetComponent<SwimController>().IsActive() && this.m_Player.GetComponent<SwimController>().m_State == SwimState.Dive)
		{
			this.m_Oxygen -= this.m_OxygenConsumptionPerSecond * Time.deltaTime;
		}
		else
		{
			this.m_Oxygen = this.m_MaxOxygen;
		}
	}

	private void UpdateEnergy()
	{
		if (this.m_Player.m_DreamActive)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		FPPController fppcontroller = this.m_Player.m_FPPController;
		if (!fppcontroller)
		{
			return;
		}
		DeathController deathController = this.m_Player.m_DeathController;
		if (deathController.IsActive())
		{
			return;
		}
		ConsciousnessController component = this.m_Player.GetComponent<ConsciousnessController>();
		if (component.IsActive())
		{
			return;
		}
		if (!Cheats.m_GodMode && !this.m_LossParametersBlocked)
		{
			this.m_Energy -= this.m_EnergyConsumptionPerSecond * deltaTime;
			if (this.IsNutritionCarboCriticalLevel() || this.IsNutritionFatCriticalLevel() || this.IsNutritionProteinsCriticalLevel())
			{
				this.m_Energy -= this.m_EnergyConsumptionPerSecondNoNutrition * deltaTime;
			}
			if (this.m_DiseasesModule.GetDisease(ConsumeEffect.Fever).IsActive())
			{
				this.m_Energy -= this.m_EnergyConsumptionPerSecondFever * deltaTime;
			}
			if (this.m_DiseasesModule.GetDisease(ConsumeEffect.FoodPoisoning).IsActive())
			{
				this.m_Energy -= this.m_EnergyConsumptionPerSecondFoodPoison * deltaTime;
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
		if (this.m_Player.m_DreamActive)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		FPPController fppcontroller = this.m_Player.m_FPPController;
		if (!fppcontroller)
		{
			return;
		}
		DeathController deathController = this.m_Player.m_DeathController;
		if (deathController.IsActive())
		{
			return;
		}
		ConsciousnessController component = this.m_Player.GetComponent<ConsciousnessController>();
		if (component.IsActive())
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
				if (injuriesList[i].m_ParentInjury == null)
				{
					if (injuriesList[i].m_Type != InjuryType.Worm && injuriesList[i].m_Type != InjuryType.Rash && injuriesList[i].m_Type != InjuryType.Leech)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				float num = MainLevel.Instance.m_TODTime.m_DayLengthInMinutes + MainLevel.Instance.m_TODTime.m_NightLengthInMinutes;
				float num2 = num * 60f;
				this.IncreaseHP(this.m_MaxHP * this.m_HealthRecoveryPerDay / num2 * deltaTime);
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
		if (this.m_Player.m_DreamActive)
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
		float deltaTime = Time.deltaTime;
		float num = 1f;
		float num2 = 1f;
		float num3 = 1f;
		if (fppcontroller.IsRunning())
		{
			num *= this.m_NutritionCarbohydratesConsumptionRunMul;
			num2 *= this.m_NutritionFatConsumptionRunMul;
			num3 *= this.m_NutritionProteinsConsumptionRunMul;
		}
		if (flag)
		{
			num *= this.m_NutritionCarbohydratesConsumptionActionMul;
			num2 *= this.m_NutritionFatConsumptionActionMul;
			num3 *= this.m_NutritionProteinsConsumptionActionMul;
		}
		if (this.IsNutritionCarboCriticalLevel())
		{
			num2 *= this.m_NutritionFatConsumptionMulNoCarbs;
			num3 *= this.m_NutritionProteinsConsumptionMulNoCarbs;
		}
		if (InventoryBackpack.Get().IsCriticalOverload())
		{
			num *= this.m_NutritionCarbohydratesConsumptionWeightCriticalMul;
			num2 *= this.m_NutritionFatConsumptionWeightCriticalMul;
			num3 *= this.m_NutritionProteinsConsumptionWeightCriticalMul;
		}
		else if (InventoryBackpack.Get().IsOverload())
		{
			num *= this.m_NutritionCarbohydratesConsumptionWeightOverloadMul;
			num2 *= this.m_NutritionFatConsumptionWeightOverloadMul;
			num3 *= this.m_NutritionProteinsConsumptionWeightOverloadMul;
		}
		else
		{
			num *= this.m_NutritionCarbohydratesConsumptionWeightNormalMul;
			num2 *= this.m_NutritionFatConsumptionWeightNormalMul;
			num3 *= this.m_NutritionProteinsConsumptionWeightNormalMul;
		}
		if (this.m_ParasiteSickness.IsActive())
		{
			num *= this.m_ParasiteSickness.m_MacroNutricientCarboLossMul * (float)this.m_ParasiteSickness.m_Level;
			num2 *= this.m_ParasiteSickness.m_MacroNutricientFatLossMul * (float)this.m_ParasiteSickness.m_Level;
			num3 *= this.m_ParasiteSickness.m_MacroNutricientProteinsLossMul * (float)this.m_ParasiteSickness.m_Level;
		}
		GameDifficulty gameDifficulty = GreenHellGame.Instance.m_GameDifficulty;
		if (gameDifficulty == GameDifficulty.Normal)
		{
			float s_NormalModeLossMul = GreenHellGame.s_NormalModeLossMul;
			num *= s_NormalModeLossMul;
			num2 *= s_NormalModeLossMul;
			num3 *= s_NormalModeLossMul;
		}
		else if (gameDifficulty == GameDifficulty.Easy)
		{
			float s_EasyModeLossMul = GreenHellGame.s_EasyModeLossMul;
			num *= s_EasyModeLossMul;
			num2 *= s_EasyModeLossMul;
			num3 *= s_EasyModeLossMul;
		}
		this.m_NutritionCarbo -= this.m_NutritionCarbohydratesConsumptionPerSecond * deltaTime * num * ((!PlayerCocaineModule.Get().m_Active) ? 1f : PlayerCocaineModule.Get().m_CarboConsumptionMul);
		this.m_NutritionCarbo = Mathf.Clamp(this.m_NutritionCarbo, 0f, this.m_MaxNutritionCarbo);
		this.m_NutritionFat -= this.m_NutritionFatConsumptionPerSecond * deltaTime * num2 * ((!PlayerCocaineModule.Get().m_Active) ? 1f : PlayerCocaineModule.Get().m_FatConsumptionMul);
		this.m_NutritionFat = Mathf.Clamp(this.m_NutritionFat, 0f, this.m_MaxNutritionFat);
		this.m_NutritionProteins -= this.m_NutritionProteinsConsumptionPerSecond * deltaTime * num3 * ((!PlayerCocaineModule.Get().m_Active) ? 1f : PlayerCocaineModule.Get().m_ProteinsConsumptionMul);
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
		if (Cheats.m_GodMode || this.m_LossParametersBlocked)
		{
			return;
		}
		this.m_Hydration -= amount * ((!PlayerCocaineModule.Get().m_Active) ? 1f : PlayerCocaineModule.Get().m_HydrationConsumptionMul);
		this.m_Hydration = Mathf.Clamp(this.m_Hydration, 0f, this.m_MaxHydration);
	}

	private void UpdateHydration()
	{
		if (this.m_Player.m_DreamActive)
		{
			return;
		}
		FPPController fppcontroller = this.m_Player.m_FPPController;
		if (!fppcontroller)
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
		GameDifficulty gameDifficulty = GreenHellGame.Instance.m_GameDifficulty;
		if (gameDifficulty == GameDifficulty.Normal)
		{
			num *= GreenHellGame.s_NormalModeLossMul;
		}
		else if (gameDifficulty == GameDifficulty.Easy)
		{
			num *= GreenHellGame.s_EasyModeLossMul;
		}
		this.DecreaseHydration(num);
	}

	public override void OnTakeDamage(DamageInfo info)
	{
		if (info.m_Blocked)
		{
			this.DecreaseStamina(StaminaDecreaseReason.PuchBlock, 1f);
		}
		else
		{
			this.m_HP -= info.m_Damage;
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

	private float m_MaxStamina = 100f;

	private float m_PrevStamina;

	private float m_LastDecreaseStaminaTime;

	private float m_StaminaRenerationDelay = 2f;

	private float m_MaxNutritionFat = 100f;

	private float m_MaxNutritionCarbo = 100f;

	private float m_MaxNutritionProteins = 100f;

	public float m_MaxHydration = 100f;

	private float m_StaminaDepletedLevel = 5f;

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

	private float m_HealthRecoveryPerDay = 0.1f;

	private float m_HealthLossPerSecondNoOxygen = 10f;

	private Dictionary<int, float> m_StaminaDecreaseMap = new Dictionary<int, float>();

	private Dictionary<int, float> m_EnergyDecreaseMap = new Dictionary<int, float>();

	private float m_HydrationDecreaseJump = 1f;

	private float m_EnergyLossDueLackOfNutritionPerSecond = 1f;

	private float m_EnergyRecoveryDueNutritionPerSecond = 1f;

	private float m_EnergyRecoveryDueHydrationPerSecond = 1f;

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

	private ParasiteSickness m_ParasiteSickness;
}
