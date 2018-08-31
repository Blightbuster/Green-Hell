using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MenuDebugWounds : MenuScreen
{
	protected override void OnShow()
	{
		base.OnShow();
		this.Setup();
	}

	private void Setup()
	{
		this.m_ToggleLimbs.Clear();
		this.m_ToggleLimbsState.Clear();
		this.m_ToggleLimbs.Add(this.m_ToggleLH);
		this.m_ToggleLimbsState.Add(this.m_ToggleLH.isOn);
		this.m_ToggleLimbs.Add(this.m_ToggleRH);
		this.m_ToggleLimbsState.Add(this.m_ToggleRH.isOn);
		this.m_ToggleLimbs.Add(this.m_ToggleLL);
		this.m_ToggleLimbsState.Add(this.m_ToggleLL.isOn);
		this.m_ToggleLimbs.Add(this.m_ToggleRL);
		this.m_ToggleLimbsState.Add(this.m_ToggleRL.isOn);
		this.m_WoundTypeList.Clear();
		for (int i = 0; i < Enum.GetValues(typeof(InjuryType)).Length; i++)
		{
			this.m_WoundTypeList.AddElement(Enum.GetValues(typeof(InjuryType)).GetValue(i).ToString(), -1);
		}
		this.m_WoundTypeList.SetFocus(true);
		this.m_WoundParameters.Clear();
		for (int j = 0; j < 10; j++)
		{
			Transform transform = base.gameObject.transform.FindDeepChild("Wound" + j.ToString());
			if (transform != null)
			{
				Text component = transform.gameObject.GetComponent<Text>();
				this.m_WoundParameters.Add(component);
			}
		}
		PlayerConditionModule playerConditionModule = PlayerConditionModule.Get();
		this.m_Proteins.minValue = 0f;
		this.m_Proteins.maxValue = playerConditionModule.GetMaxNutritionProtein();
		this.m_Proteins.value = playerConditionModule.GetNutritionProtein();
		this.m_Fat.minValue = 0f;
		this.m_Fat.maxValue = playerConditionModule.GetMaxNutritionFat();
		this.m_Fat.value = playerConditionModule.GetNutritionFat();
		this.m_Carbo.minValue = 0f;
		this.m_Carbo.maxValue = playerConditionModule.GetMaxNutritionCarbo();
		this.m_Carbo.value = playerConditionModule.GetNutritionCarbo();
		this.m_Hydration.minValue = 0f;
		this.m_Hydration.maxValue = playerConditionModule.GetMaxHydration();
		this.m_Hydration.value = playerConditionModule.GetHydration();
		this.m_HP.minValue = 0f;
		this.m_HP.maxValue = playerConditionModule.GetMaxHP();
		this.m_HP.value = playerConditionModule.GetHP();
		this.m_Energy.minValue = 0f;
		this.m_Energy.maxValue = playerConditionModule.GetMaxEnergy();
		this.m_Energy.value = playerConditionModule.GetEnergy();
		this.m_Sanity.minValue = 0f;
		this.m_Sanity.maxValue = 100f;
		this.m_Sanity.value = (float)PlayerSanityModule.Get().m_Sanity;
	}

	public void OnLimbSelChanged(bool set)
	{
		if (set)
		{
			int num = -1;
			for (int i = 0; i < this.m_ToggleLimbsState.Count; i++)
			{
				if (this.m_ToggleLimbs[i].isOn != this.m_ToggleLimbsState[i])
				{
					num = i;
					break;
				}
			}
			if (num < 0)
			{
				num = 0;
			}
			for (int j = 0; j < this.m_ToggleLimbsState.Count; j++)
			{
				if (j == num)
				{
					this.m_ToggleLimbs[j].isOn = true;
				}
				else
				{
					this.m_ToggleLimbs[j].isOn = false;
				}
				this.m_ToggleLimbsState[j] = this.m_ToggleLimbs[j].isOn;
			}
		}
	}

	public void OnCreateWound()
	{
		PlayerInjuryModule playerInjuryModule = PlayerInjuryModule.Get();
		InjuryType injuryType = (InjuryType)Enum.GetValues(typeof(InjuryType)).GetValue(this.m_WoundTypeList.GetSelectionIndex());
		BIWoundSlot biwoundSlot = null;
		if (this.m_ToggleLH.isOn)
		{
			biwoundSlot = BodyInspectionController.Get().GetFreeWoundSlot(InjuryPlace.LHand, injuryType);
		}
		else if (this.m_ToggleRH.isOn)
		{
			biwoundSlot = BodyInspectionController.Get().GetFreeWoundSlot(InjuryPlace.RHand, injuryType);
		}
		else if (this.m_ToggleLL.isOn)
		{
			biwoundSlot = BodyInspectionController.Get().GetFreeWoundSlot(InjuryPlace.LLeg, injuryType);
		}
		else if (this.m_ToggleRL.isOn)
		{
			biwoundSlot = BodyInspectionController.Get().GetFreeWoundSlot(InjuryPlace.RLeg, injuryType);
		}
		if (biwoundSlot != null)
		{
			int poison_level = 0;
			if (injuryType == InjuryType.VenomBite || injuryType == InjuryType.SnakeBite)
			{
				string text = this.m_PosionLevel.text;
				if (!int.TryParse(text, out poison_level))
				{
					poison_level = 1;
				}
			}
			InjuryState state = InjuryState.Open;
			if (injuryType == InjuryType.Laceration || injuryType == InjuryType.Laceration)
			{
				state = InjuryState.Bleeding;
			}
			else if (injuryType == InjuryType.WormHole)
			{
				state = InjuryState.WormInside;
			}
			playerInjuryModule.AddInjury(injuryType, biwoundSlot.m_InjuryPlace, biwoundSlot, state, poison_level, null);
		}
	}

	public void OnHealAllInjuries()
	{
		PlayerInjuryModule.Get().ResetInjuries();
	}

	protected override void Update()
	{
		base.Update();
		PlayerInjuryModule playerInjuryModule = PlayerInjuryModule.Get();
		PlayerConditionModule playerConditionModule = PlayerConditionModule.Get();
		for (int i = 0; i < this.m_WoundParameters.Count; i++)
		{
			if (i < playerInjuryModule.m_Injuries.Count)
			{
				Injury injury = playerInjuryModule.m_Injuries[i];
				this.m_WoundParameters[i].enabled = true;
				this.m_WoundParameters[i].text = "Type: " + injury.m_Type.ToString();
				if (injury.m_Type == InjuryType.SmallWoundAbrassion)
				{
					Text text = this.m_WoundParameters[i];
					text.text = text.text + " TimeToHeal:" + (injury.GetHealingDuration() - (MainLevel.Instance.GetCurrentTimeMinutes() - injury.m_HealingStartTime)).ToString();
					Text text2 = this.m_WoundParameters[i];
					text2.text = text2.text + " HealTimeBonus: " + injury.m_HealingTimeDec.ToString();
				}
				else if (injury.m_Type == InjuryType.SmallWoundScratch)
				{
					Text text3 = this.m_WoundParameters[i];
					text3.text = text3.text + " TimeToHeal:" + (injury.GetHealingDuration() - (MainLevel.Instance.GetCurrentTimeMinutes() - injury.m_HealingStartTime)).ToString();
					Text text4 = this.m_WoundParameters[i];
					text4.text = text4.text + " HealTimeBonus: " + injury.m_HealingTimeDec.ToString();
				}
				else if (injury.m_Type == InjuryType.Laceration || injury.m_Type == InjuryType.LacerationCat)
				{
					Text text5 = this.m_WoundParameters[i];
					text5.text = text5.text + " TimeToHeal:" + (injury.GetHealingDuration() - (MainLevel.Instance.GetCurrentTimeMinutes() - injury.m_HealingStartTime)).ToString();
					Text text6 = this.m_WoundParameters[i];
					text6.text = text6.text + " Will transform to: " + injury.m_HealingResultInjuryState.ToString();
				}
				else if (injury.m_Type == InjuryType.VenomBite)
				{
					Text text7 = this.m_WoundParameters[i];
					text7.text = text7.text + " TimeToHeal:" + (injury.GetHealingDuration() - (MainLevel.Instance.GetCurrentTimeMinutes() - injury.m_HealingStartTime)).ToString();
					Text text8 = this.m_WoundParameters[i];
					text8.text = text8.text + " PoisonLevel: " + injury.m_PoisonLevel.ToString();
				}
				else if (injury.m_Type == InjuryType.SnakeBite)
				{
					Text text9 = this.m_WoundParameters[i];
					text9.text = text9.text + " TimeToHeal:" + (injury.GetHealingDuration() - (MainLevel.Instance.GetCurrentTimeMinutes() - injury.m_HealingStartTime)).ToString();
					Text text10 = this.m_WoundParameters[i];
					text10.text = text10.text + " PoisonLevel: " + injury.m_PoisonLevel.ToString();
				}
				else if (injury.m_Type == InjuryType.Rash)
				{
					Text text11 = this.m_WoundParameters[i];
					text11.text = text11.text + " TimeToHeal:" + (injury.GetHealingDuration() - (MainLevel.Instance.GetCurrentTimeMinutes() - injury.m_StartTimeInMinutes)).ToString();
					Text text12 = this.m_WoundParameters[i];
					text12.text = text12.text + " HealTimeBonus: " + injury.m_HealingTimeDec.ToString();
				}
				else if (injury.m_Type == InjuryType.WormHole)
				{
					Text text13 = this.m_WoundParameters[i];
					text13.text = text13.text + " TimeToHeal:" + (injury.GetHealingDuration() - (MainLevel.Instance.GetCurrentTimeMinutes() - injury.m_HealingStartTime)).ToString();
					Text text14 = this.m_WoundParameters[i];
					text14.text = text14.text + " HealTimeBonus: " + injury.m_HealingTimeDec.ToString();
				}
			}
			else
			{
				this.m_WoundParameters[i].enabled = false;
			}
		}
		WaterCollider waterPlayerInside = WaterBoxManager.Get().GetWaterPlayerInside();
		this.m_LeechCooldownText.text = "Leech chance: ";
		if (waterPlayerInside == null)
		{
			Text leechCooldownText = this.m_LeechCooldownText;
			leechCooldownText.text += "None";
		}
		else
		{
			Text leechCooldownText2 = this.m_LeechCooldownText;
			leechCooldownText2.text += waterPlayerInside.m_LeechChance.ToString();
		}
		Text leechCooldownText3 = this.m_LeechCooldownText;
		leechCooldownText3.text = leechCooldownText3.text + " Time to next leech: " + (playerInjuryModule.GetLeechNextTime() - MainLevel.Instance.GetCurrentTimeMinutes()).ToString();
		Text leechCooldownText4 = this.m_LeechCooldownText;
		leechCooldownText4.text = leechCooldownText4.text + " CoolDown: " + Injury.s_LeechCooldownInMinutes.ToString();
		this.m_HPText.text = "HP = " + playerConditionModule.GetHP().ToString() + "/" + playerConditionModule.GetMaxHP().ToString();
		this.m_ConditionText.text = "Condition = " + playerConditionModule.GetEnergy().ToString() + "/" + playerConditionModule.GetMaxEnergy().ToString();
		playerConditionModule.m_NutritionProteins = this.m_Proteins.value;
		playerConditionModule.m_NutritionFat = this.m_Fat.value;
		playerConditionModule.m_NutritionCarbo = this.m_Carbo.value;
		playerConditionModule.m_Hydration = this.m_Hydration.value;
		playerConditionModule.m_HP = this.m_HP.value;
		playerConditionModule.m_Energy = this.m_Energy.value;
		PlayerSanityModule.Get().m_Sanity = (int)this.m_Sanity.value;
	}

	public void OnFeverButton()
	{
		PlayerDiseasesModule.Get().RequestDisease(ConsumeEffect.Fever, 0f, 1);
	}

	public Toggle m_ToggleLH;

	public Toggle m_ToggleRH;

	public Toggle m_ToggleLL;

	public Toggle m_ToggleRL;

	private List<Toggle> m_ToggleLimbs = new List<Toggle>();

	private List<bool> m_ToggleLimbsState = new List<bool>();

	public UIList m_WoundTypeList;

	private List<Text> m_WoundParameters = new List<Text>();

	public Text m_LeechCooldownText;

	public Text m_HPText;

	public Text m_ConditionText;

	public Slider m_Proteins;

	public Slider m_Fat;

	public Slider m_Carbo;

	public Slider m_Hydration;

	public Slider m_HP;

	public Slider m_Energy;

	public Slider m_Sanity;

	public InputField m_PosionLevel;
}
