using System;
using Enums;
using UnityEngine;

public class EatingController : PlayerController
{
	public static EatingController Get()
	{
		return EatingController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		base.m_ControllerType = PlayerControllerType.Eating;
		EatingController.s_Instance = this;
	}

	public void Eat(FoodInfo info)
	{
		if (info == null)
		{
			return;
		}
		PlayerConditionModule.Get().IncreaseNutritionCarbo(info.m_Carbohydrates);
		PlayerConditionModule.Get().IncreaseNutritionFat(info.m_Fat);
		PlayerConditionModule.Get().IncreaseNutritionProteins(info.m_Proteins);
		PlayerConditionModule.Get().IncreaseHydration(info.m_Water);
		PlayerConditionModule.Get().IncreaseEnergy(info.m_AddEnergy);
		this.OnEat(info);
	}

	private void OnEat(ConsumableInfo info)
	{
		if (info.m_ConsumeEffect != ConsumeEffect.None && UnityEngine.Random.Range(0f, 1f) <= info.m_ConsumeEffectChance && info.m_ConsumeEffectLevel >= 0)
		{
			PlayerDiseasesModule.Get().RequestDisease(info.m_ConsumeEffect, info.m_ConsumeEffectDelay, info.m_ConsumeEffectLevel);
		}
		if (info.m_Disgusting)
		{
			PlayerAudioModule.Get().PlayEatingDisgustingSound(1f, false);
		}
		else
		{
			PlayerAudioModule.Get().PlayEatingSound(1f, false);
		}
		EventsManager.OnEvent(Enums.Event.Eat, 1, (int)info.m_ID);
		PlayerSanityModule.Get().OnConsume(info.m_SanityChange);
		Localization localization = GreenHellGame.Instance.GetLocalization();
		HUDMessages hudmessages = (HUDMessages)HUDManager.Get().GetHUD(typeof(HUDMessages));
		string text = string.Empty;
		if (info.m_ConsumeEffect == ConsumeEffect.FoodPoisoning)
		{
			text = info.m_ConsumeEffectLevel.ToString("F0") + " " + localization.Get("HUD_FoodPoisoning", true);
			hudmessages.AddMessage(text, null, HUDMessageIcon.FoodPoisoning, "", null);
		}
		if (info.m_AddEnergy > 0f)
		{
			text = info.m_AddEnergy.ToString("F0") + " " + localization.Get("HUD_Energy", true);
			hudmessages.AddMessage(text, null, HUDMessageIcon.Energy, "", null);
		}
		if (info.m_Water > 0f)
		{
			text = info.m_Water.ToString("F0") + " " + localization.Get("HUD_Hydration", true);
			hudmessages.AddMessage(text, null, HUDMessageIcon.Hydration, "", null);
		}
		if (info.m_Fat > 0f)
		{
			text = info.m_Fat.ToString("F0") + " " + localization.Get("HUD_Nutrition_Fat", true);
			hudmessages.AddMessage(text, null, HUDMessageIcon.Fat, "", null);
		}
		if (info.m_Proteins > 0f)
		{
			text = info.m_Proteins.ToString("F0") + " " + localization.Get("HUD_Nutrition_Protein", true);
			hudmessages.AddMessage(text, null, HUDMessageIcon.Proteins, "", null);
		}
		if (info.m_Carbohydrates > 0f)
		{
			text = info.m_Carbohydrates.ToString("F0") + " " + localization.Get("HUD_Nutrition_Carbo", true);
			hudmessages.AddMessage(text, null, HUDMessageIcon.Carbo, "", null);
		}
		text = localization.Get(TriggerAction.GetTextPerfect(TriggerAction.TYPE.Eat), true) + ": " + info.GetNameToDisplayLocalized();
		hudmessages.AddMessage(text, null, HUDMessageIcon.Item, info.m_IconName, null);
		PlayerDiseasesModule.Get().OnEat(info);
		PlayerInjuryModule.Get().OnEat(info);
		ItemsManager.Get().OnEat(info);
		if (info.m_ID == ItemID.coca_leafs)
		{
			PlayerCocaineModule.Get().OnEatCocaine();
		}
	}

	public void Drink(LiquidContainerInfo info)
	{
		if (info == null || info.m_Amount == 0f)
		{
			return;
		}
		float hydration;
		float num = Mathf.Max(Mathf.Clamp((hydration = PlayerConditionModule.Get().m_Hydration) + info.m_Amount, 0f, PlayerConditionModule.Get().m_MaxHydration) - hydration, Mathf.Min(this.m_MinAmount, info.m_Amount));
		info.m_Amount -= Mathf.Max(num, Mathf.Min(this.m_MinAmount, info.m_Amount));
		if (info.m_Amount < 0.1f)
		{
			info.m_Amount = 0f;
		}
		this.Drink(info.m_LiquidType, num);
	}

	public void Drink(LiquidType type, float amount)
	{
		LiquidData liquidData = LiquidManager.Get().GetLiquidData(type);
		float num = amount / 100f * liquidData.m_Water;
		PlayerConditionModule.Get().IncreaseHydration(num);
		float fat = liquidData.m_Fat;
		PlayerConditionModule.Get().IncreaseNutritionFat(fat);
		float proteins = liquidData.m_Proteins;
		PlayerConditionModule.Get().IncreaseNutritionProteins(proteins);
		float carbohydrates = liquidData.m_Carbohydrates;
		PlayerConditionModule.Get().IncreaseNutritionCarbo(carbohydrates);
		float energy = liquidData.m_Energy;
		PlayerConditionModule.Get().IncreaseEnergy(energy);
		PlayerSanityModule.Get().OnConsume(liquidData.m_SanityChange);
		this.OnDrink(liquidData, num, fat, proteins, carbohydrates, energy);
	}

	private void OnDrink(LiquidData data, float hydration_amount, float fat_amount, float proteins_amount, float carbo_amount, float energy_amount)
	{
		string text = string.Empty;
		Localization localization = GreenHellGame.Instance.GetLocalization();
		HUDMessages hudmessages = (HUDMessages)HUDManager.Get().GetHUD(typeof(HUDMessages));
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < data.m_ConsumeEffects.Count; i++)
		{
			if (data.m_ConsumeEffects[i].m_ConsumeEffect != ConsumeEffect.None && UnityEngine.Random.Range(0f, 1f) <= data.m_ConsumeEffects[i].m_ConsumeEffectChance && data.m_ConsumeEffects[i].m_ConsumeEffectLevel != 0)
			{
				if (data.m_ConsumeEffects[i].m_ConsumeEffectLevel > 0)
				{
					PlayerDiseasesModule.Get().RequestDisease(data.m_ConsumeEffects[i].m_ConsumeEffect, data.m_ConsumeEffects[i].m_ConsumeEffectDelay, 0);
					flag = true;
				}
				if (data.m_ConsumeEffects[i].m_ConsumeEffectLevel < 0)
				{
					flag2 = true;
				}
				if (data.m_ConsumeEffects[i].m_ConsumeEffect == ConsumeEffect.FoodPoisoning)
				{
					text = data.m_ConsumeEffects[i].m_ConsumeEffectLevel.ToString("F0") + " " + localization.Get("HUD_FoodPoisoning", true);
					hudmessages.AddMessage(text, null, HUDMessageIcon.FoodPoisoning, "", null);
				}
				else if (data.m_ConsumeEffects[i].m_ConsumeEffect == ConsumeEffect.ParasiteSickness)
				{
					text = data.m_ConsumeEffects[i].m_ConsumeEffectLevel.ToString("F0") + " " + localization.Get("HUD_ParasiteSickness", true);
					hudmessages.AddMessage(text, null, HUDMessageIcon.ParasiteSickness, "", null);
				}
			}
		}
		if (flag || flag2)
		{
			PlayerDiseasesModule.Get().OnDrink(data.m_LiquidType, hydration_amount);
		}
		PlayerInjuryModule.Get().OnDrink(data);
		if (data.m_Disgusting)
		{
			PlayerAudioModule.Get().PlayDrinkingDisgustingSound(1f, false);
		}
		else
		{
			PlayerAudioModule.Get().PlayDrinkingSound(1f, false);
		}
		if (energy_amount > 0f)
		{
			text = energy_amount.ToString("F0") + " " + localization.Get("HUD_Energy", true);
			hudmessages.AddMessage(text, null, HUDMessageIcon.Energy, "", null);
		}
		if (hydration_amount > 0f)
		{
			text = hydration_amount.ToString("F0") + " " + localization.Get("HUD_Hydration", true);
			hudmessages.AddMessage(text, null, HUDMessageIcon.Hydration, "", null);
		}
		if (fat_amount > 0f)
		{
			text = fat_amount.ToString("F0") + " " + localization.Get("HUD_Nutrition_Fat", true);
			hudmessages.AddMessage(text, null, HUDMessageIcon.Fat, "", null);
		}
		if (proteins_amount > 0f)
		{
			text = proteins_amount.ToString("F0") + " " + localization.Get("HUD_Nutrition_Protein", true);
			hudmessages.AddMessage(text, null, HUDMessageIcon.Proteins, "", null);
		}
		if (carbo_amount > 0f)
		{
			text = carbo_amount.ToString("F0") + " " + localization.Get("HUD_Nutrition_Carbo", true);
			hudmessages.AddMessage(text, null, HUDMessageIcon.Carbo, "", null);
		}
		text = localization.Get(TriggerAction.GetTextPerfect(TriggerAction.TYPE.Drink), true) + ": " + GreenHellGame.Instance.GetLocalization().Get(data.m_LiquidType.ToString(), true);
		hudmessages.AddMessage(text, null, HUDMessageIcon.None, "", null);
		EventsManager.OnEvent(Enums.Event.Drink, 1, (int)data.m_LiquidType);
		InventoryBackpack.Get().CalculateCurrentWeight();
	}

	public float m_MinAmount = 200f;

	private static EatingController s_Instance;
}
