using System;
using System.Collections;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDEnergy : HUDBase
{
	public static HUDEnergy Get()
	{
		return HUDEnergy.s_Instance;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override void Awake()
	{
		base.Awake();
		HUDEnergy.s_Instance = this;
		this.m_OxygenBar.gameObject.SetActive(false);
		this.m_OxygenBarBG.gameObject.SetActive(false);
		this.m_StaminaColor = this.m_Stamina.color;
		for (int i = 0; i < 4; i++)
		{
			this.m_BlinkArmorRedbackgroundActive[i] = false;
			this.m_BlinkArmorRedBackgroundStartTime[i] = float.MinValue;
		}
	}

	protected override void Start()
	{
		base.Start();
		this.m_ConditionModule = Player.Get().GetComponent<PlayerConditionModule>();
		DebugUtils.Assert(this.m_ConditionModule, true);
		this.m_NoNutritionIconStartScale = this.m_EnergyUsedByNoNutritionIcon.transform.localScale;
		this.m_WoundsIconStartScale = this.m_EnergyUsedByWoundsIcon.transform.localScale;
		this.m_FoodPoisonIconStartScale = this.m_EnergyFoodPoisonIcon.transform.localScale;
		this.m_PoisonIconStartScale = this.m_EnergyFoodPoisonIcon.transform.localScale;
		this.m_FeverIconStartScale = this.m_EnergyFeverIcon.transform.localScale;
		this.m_DamagedArmorIconStartScale = this.m_EnergyArmorDamagedIcon.transform.localScale;
		this.m_HealingWoundDummy = base.transform.FindDeepChild("HUDHealingWoundDummy");
		this.ScrollerInitialize();
		this.m_PIM = PlayerInjuryModule.Get();
		this.m_PDM = Player.Get().GetComponent<PlayerDiseasesModule>();
		this.m_PAM = PlayerArmorModule.Get();
		this.m_HealthRed.enabled = false;
	}

	protected override bool ShouldShow()
	{
		return !Cheats.m_GodMode && !Player.Get().m_DreamActive && !HUDReadableItem.Get().isActiveAndEnabled && !CutscenesManager.Get().IsCutscenePlaying();
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.UpdateEnergyBars();
		this.UpdateHealthBar();
		this.UpdateOxygenBar();
		this.UpdateIcons();
		this.UpdateHealingWoundIndicators();
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateEnergyBars();
		this.UpdateHealthBar();
		this.UpdateOxygenBar();
		this.UpdateIcons();
		this.UpdateHealingWoundIndicators();
	}

	private void UpdateEnergyBars()
	{
		this.UpdateEnergyBar();
		this.UpdateStaminaBar();
	}

	private void UpdateEnergyBar()
	{
		if (this.m_ConditionModule == null)
		{
			this.m_ConditionModule = Player.Get().GetComponent<PlayerConditionModule>();
		}
		if (this.m_ConditionModule == null)
		{
			return;
		}
		float num = this.m_ConditionModule.GetEnergy() / this.m_ConditionModule.GetMaxEnergy();
		this.m_Energy.fillAmount = num;
		if (num > 0.1f)
		{
			this.m_Energy.color = Color.white;
			return;
		}
		this.m_Energy.color = this.m_RedColor;
	}

	private void UpdateStaminaBar()
	{
		if (this.m_ConditionModule == null)
		{
			this.m_ConditionModule = Player.Get().GetComponent<PlayerConditionModule>();
		}
		if (this.m_ConditionModule == null)
		{
			return;
		}
		float fillAmount = this.m_ConditionModule.GetStamina() / this.m_ConditionModule.GetMaxEnergy();
		this.m_Stamina.fillAmount = fillAmount;
		if (PlayerConditionModule.Get().IsLowStamina() && this.m_Stamina.color != this.m_StaminaCriticalLevelColor)
		{
			this.m_Stamina.color = Color.Lerp(this.m_Stamina.color, this.m_StaminaCriticalLevelColor, Time.deltaTime * 5f);
			return;
		}
		if (this.m_Stamina.color != this.m_StaminaColor)
		{
			this.m_Stamina.color = Color.Lerp(this.m_Stamina.color, this.m_StaminaColor, Time.deltaTime * 5f);
		}
	}

	private void UpdateHealthBar()
	{
		if (this.m_ConditionModule == null)
		{
			this.m_ConditionModule = Player.Get().GetComponent<PlayerConditionModule>();
		}
		if (this.m_ConditionModule == null)
		{
			return;
		}
		float num = this.m_ConditionModule.GetHP() / 100f;
		this.m_Health.fillAmount = num;
		if (num > 0.1f)
		{
			this.m_Health.color = Color.white;
		}
		else
		{
			this.m_Health.color = this.m_RedColor;
		}
		if (this.m_HealthLastFrame - this.m_ConditionModule.GetHP() > this.m_HPBlinkMinDiff && !this.m_BlinkRedbackgroundActive)
		{
			this.StartBlinkRedBackground(num);
		}
		this.m_HealthLastFrame = this.m_ConditionModule.GetHP();
		float fillAmount = this.m_ConditionModule.GetMaxHP() / 100f;
		this.m_MaxHealth.fillAmount = fillAmount;
		if (this.m_BlinkRedbackgroundActive)
		{
			this.BlinkRedBackground(num);
		}
	}

	private void UpdateIcons()
	{
		float num = this.m_BlinkScaleMin + this.m_BlinkingCurve.Evaluate(this.m_BeginTime) * (this.m_BlinkScaleMax - this.m_BlinkScaleMin);
		this.m_BeginTime += 1f / this.m_BlinkingTime * Time.deltaTime;
		if (this.m_BeginTime >= 1f)
		{
			this.m_BeginTime = 0f;
		}
		Vector3 one = Vector3.one;
		bool flag = false;
		Disease disease = this.m_PDM.GetDisease(ConsumeEffect.Fever);
		float currentTimeMinutes = MainLevel.Instance.GetCurrentTimeMinutes();
		if (disease.IsActive())
		{
			int level = disease.m_Level;
			this.m_isIconFeverEnabled = true;
			one.x = this.m_FeverIconStartScale.x * num;
			one.y = this.m_FeverIconStartScale.y * num;
			one.z = this.m_FeverIconStartScale.z * 1f;
			this.m_EnergyFeverIcon.transform.localScale = one;
			this.m_EnergyFeverRadial.fillAmount = 1f - (currentTimeMinutes - disease.m_StartTime) / disease.m_AutoHealTime;
			if (level > 0)
			{
				this.m_EnergyFeverLevel.enabled = true;
				this.m_EnergyFeverLevel.text = level.ToString();
			}
			else
			{
				this.m_EnergyFeverLevel.enabled = false;
			}
		}
		else
		{
			this.m_isIconFeverEnabled = false;
			this.m_EnergyFeverLevel.enabled = false;
		}
		disease = this.m_PDM.GetDisease(ConsumeEffect.ParasiteSickness);
		if (disease.IsActive())
		{
			int level2 = disease.m_Level;
			this.m_IsIconParasiteSicknessEnabled = true;
			one.x = this.m_ParasiteSicknessIconStartScale.x * num;
			one.y = this.m_ParasiteSicknessIconStartScale.y * num;
			one.z = this.m_ParasiteSicknessIconStartScale.z * 1f;
			this.m_EnergyParasiteSicknessIcon.transform.localScale = one;
			if (level2 > 0)
			{
				this.m_EnergyParasiteSicknessLevel.enabled = true;
				this.m_EnergyParasiteSicknessLevel.text = level2.ToString();
			}
			else
			{
				this.m_EnergyParasiteSicknessLevel.enabled = false;
			}
		}
		else
		{
			this.m_IsIconParasiteSicknessEnabled = false;
			this.m_EnergyParasiteSicknessLevel.enabled = false;
		}
		disease = this.m_PDM.GetDisease(ConsumeEffect.Insomnia);
		if (disease.IsActive())
		{
			int level3 = disease.m_Level;
			this.m_IsIconInsomniaEnabled = true;
			one.x = this.m_InsomniaIconStartScale.x * num;
			one.y = this.m_InsomniaIconStartScale.y * num;
			one.z = this.m_InsomniaIconStartScale.z * 1f;
			this.m_EnergyInsomniaIcon.transform.localScale = one;
			this.m_EnergyInsomniaRadial.fillAmount = ((Insomnia)disease).m_InsomniaLevel % 1f;
			if (level3 > 0)
			{
				this.m_EnergyInsomniaLevel.enabled = true;
				this.m_EnergyInsomniaLevel.text = level3.ToString();
			}
			else
			{
				this.m_EnergyInsomniaLevel.enabled = false;
			}
		}
		else
		{
			this.m_IsIconInsomniaEnabled = false;
			this.m_EnergyInsomniaLevel.enabled = false;
		}
		int num2 = 0;
		bool flag2 = false;
		Injury injury = null;
		float num3 = float.MaxValue;
		for (int i = 0; i < this.m_PIM.m_Injuries.Count; i++)
		{
			if (this.m_PIM.m_Injuries[i].m_Type == InjuryType.VenomBite || this.m_PIM.m_Injuries[i].m_Type == InjuryType.SnakeBite)
			{
				num2 += this.m_PIM.m_Injuries[i].m_PoisonLevel;
				flag2 = true;
				if (this.m_PIM.m_Injuries[i].m_StartTimeInMinutes < num3)
				{
					injury = this.m_PIM.m_Injuries[i];
					num3 = this.m_PIM.m_Injuries[i].m_StartTimeInMinutes;
				}
			}
			if (this.m_PIM.m_Injuries[i].m_HealthDecreasePerSec > 0f)
			{
				flag = true;
			}
		}
		if (flag2)
		{
			this.m_isIconPoisonEnabled = true;
			one.x = this.m_PoisonIconStartScale.x * num;
			one.y = this.m_PoisonIconStartScale.y * num;
			one.z = this.m_PoisonIconStartScale.z * 1f;
			this.m_EnergyPoisonIcon.transform.localScale = one;
			if (num2 > 0)
			{
				this.m_EnergyPoisonLevelText.enabled = true;
				this.m_EnergyPoisonLevelText.text = num2.ToString();
			}
			else
			{
				this.m_EnergyPoisonLevelText.enabled = false;
			}
			this.m_EnergyPoisonRadial.enabled = true;
			this.m_EnergyPoisonRadial.fillAmount = 1f - (MainLevel.Instance.GetCurrentTimeMinutes() - injury.m_StartTimeInMinutes) / Injury.s_PoisonAutoDebufTime;
		}
		else
		{
			this.m_isIconPoisonEnabled = false;
			this.m_EnergyPoisonLevelText.enabled = false;
			this.m_EnergyPoisonRadial.enabled = false;
		}
		disease = this.m_PDM.GetDisease(ConsumeEffect.FoodPoisoning);
		if (disease.IsActive())
		{
			int level4 = disease.m_Level;
			this.m_isIconFoodPoisonEnabled = true;
			one.x = this.m_FoodPoisonIconStartScale.x * num;
			one.y = this.m_FoodPoisonIconStartScale.y * num;
			one.z = this.m_FoodPoisonIconStartScale.z * 1f;
			this.m_EnergyFoodPoisonIcon.transform.localScale = one;
			this.m_EnergyFoodPoisonRadial.fillAmount = 1f - (currentTimeMinutes - disease.m_StartTime) / disease.m_AutoHealTime;
			if (level4 > 0)
			{
				this.m_EnergyFoodPoisonLevel.enabled = true;
				this.m_EnergyFoodPoisonLevel.text = level4.ToString();
			}
			else
			{
				this.m_EnergyFoodPoisonLevel.enabled = false;
			}
		}
		else
		{
			this.m_isIconFoodPoisonEnabled = false;
			this.m_EnergyFoodPoisonLevel.enabled = false;
		}
		PlayerConditionModule playerConditionModule = PlayerConditionModule.Get();
		if (playerConditionModule.IsNutritionCarboCriticalLevel() || playerConditionModule.IsNutritionFatCriticalLevel() || playerConditionModule.IsNutritionProteinsCriticalLevel() || playerConditionModule.IsHydrationCriticalLevel())
		{
			this.m_isIconNutritionEnabled = true;
			one.x = this.m_NoNutritionIconStartScale.x * num;
			one.y = this.m_NoNutritionIconStartScale.y * num;
			one.z = this.m_NoNutritionIconStartScale.z * 1f;
			this.m_EnergyUsedByNoNutritionIcon.transform.localScale = one;
			if (playerConditionModule.IsHydrationCriticalLevel())
			{
				flag = true;
			}
		}
		else
		{
			this.m_isIconNutritionEnabled = false;
		}
		if (this.m_PIM.GetInjuriesCount() > 0 && (this.m_PIM.GetNumWounds() != this.m_PIM.GetInjuriesCount() || !this.m_PIM.AllWoundsHealing()))
		{
			this.m_isIconWoundsEnabled = true;
			one.x = this.m_WoundsIconStartScale.x * num;
			one.y = this.m_WoundsIconStartScale.y * num;
			one.z = this.m_WoundsIconStartScale.z * 1f;
			this.m_EnergyUsedByWoundsIcon.transform.localScale = one;
		}
		else
		{
			this.m_isIconWoundsEnabled = false;
		}
		if (this.m_PAM.IsAnyArmorDamaged())
		{
			this.m_IsIconDamagedArmorEnabled = true;
			one.x = this.m_DamagedArmorIconStartScale.x * num;
			one.y = this.m_DamagedArmorIconStartScale.y * num;
			one.z = this.m_DamagedArmorIconStartScale.z * 1f;
			this.m_EnergyArmorDamagedIcon.transform.localScale = one;
		}
		else
		{
			this.m_IsIconDamagedArmorEnabled = false;
		}
		disease = this.m_PDM.GetDisease(ConsumeEffect.DirtSickness);
		if (disease.IsActive())
		{
			this.m_IsIconDirtinessEnabled = true;
			one.x = this.m_DirtinessIconStartScale.x * num;
			one.y = this.m_DirtinessIconStartScale.y * num;
			one.z = this.m_DirtinessIconStartScale.z * 1f;
			this.m_EnergyArmorDamagedIcon.transform.localScale = one;
			this.m_EnergyDirtinessRadial.fillAmount = this.m_ConditionModule.m_Dirtiness % 100f / 100f;
			if (disease.m_Level > 0)
			{
				this.m_EnergyDirtinessLevel.enabled = true;
				this.m_EnergyDirtinessLevel.text = disease.m_Level.ToString();
			}
			else
			{
				this.m_EnergyDirtinessLevel.enabled = false;
			}
		}
		else
		{
			this.m_IsIconDirtinessEnabled = false;
		}
		this.m_HealthArrowDown.enabled = flag;
		if (flag)
		{
			Color color = this.m_HealthArrowDown.color;
			color.a = 1f;
			if (Mathf.Abs(Mathf.Cos(Time.time * 0.3f)) > 0.9f)
			{
				color.a = Mathf.Abs(Mathf.Sin(Time.time * 2.5f));
			}
			this.m_HealthArrowDown.color = color;
			this.m_HealthArrowUp.enabled = false;
		}
		else if (Time.time - playerConditionModule.m_IncreaseHPLastTime < 3f && playerConditionModule.GetHP() < playerConditionModule.GetMaxHP())
		{
			this.m_HealthArrowUp.enabled = true;
		}
		else
		{
			this.m_HealthArrowUp.enabled = false;
		}
		if (Time.time - playerConditionModule.m_IncreaseEnergyLastTime < 3f)
		{
			this.m_EnergyArrowUp.enabled = true;
			Color color2 = this.m_EnergyArrowUp.color;
			color2.a = Mathf.Abs(Mathf.Sin(Time.time * 2.5f));
			this.m_EnergyArrowUp.color = color2;
		}
		else
		{
			this.m_EnergyArrowUp.enabled = false;
		}
		this.IconsCheckLastFrame();
	}

	private void UpdateHealingWoundIndicators()
	{
		int numHealingWounds = this.m_PIM.GetNumHealingWounds();
		while (this.m_HealingWounds.Count != numHealingWounds)
		{
			if (numHealingWounds > this.m_HealingWounds.Count)
			{
				this.AddHealingWoundIndicator();
			}
			if (numHealingWounds < this.m_HealingWounds.Count)
			{
				this.RemoveHealingWoundIndicator();
			}
		}
		float num = 0f;
		float currentTimeMinutes = MainLevel.Instance.GetCurrentTimeMinutes();
		int num2 = 0;
		for (int i = 0; i < this.m_PIM.m_Injuries.Count; i++)
		{
			Injury injury = this.m_PIM.m_Injuries[i];
			if (injury.IsWound() && injury.IsHealing())
			{
				if (injury.m_Place == InjuryPlace.LHand || injury.m_Place == InjuryPlace.RHand)
				{
					this.m_HealingWounds[num2].m_WoundIcon.sprite = this.m_WoundHandSprite;
					this.m_HealingWounds[num2].m_Progress.sprite = this.m_WoundHandSprite;
				}
				if (injury.m_Place == InjuryPlace.LLeg || injury.m_Place == InjuryPlace.RLeg)
				{
					this.m_HealingWounds[num2].m_WoundIcon.sprite = this.m_WoundLegSprite;
					this.m_HealingWounds[num2].m_Progress.sprite = this.m_WoundLegSprite;
				}
				float num3 = (currentTimeMinutes - injury.m_HealingStartTime) / injury.GetHealingDuration();
				num3 = Mathf.Clamp01(num3);
				this.m_HealingWounds[num2].m_Progress.fillAmount = num3;
				Vector3 zero = Vector3.zero;
				zero.y += num;
				this.m_HealingWounds[num2].m_Object.transform.localPosition = zero;
				num += this.m_HealingWounds[num2].m_Progress.rectTransform.sizeDelta.y;
				num2++;
			}
		}
	}

	private void AddHealingWoundIndicator()
	{
		HUHealingWoundIndicator huhealingWoundIndicator = new HUHealingWoundIndicator();
		huhealingWoundIndicator.m_Object = base.AddElement("HUDHealingWound");
		huhealingWoundIndicator.m_Object.transform.parent = this.m_HealingWoundDummy;
		this.m_HealingWoundDummy.transform.position = this.m_EnergyIconsGroup.transform.position - this.m_WoundIconOffset;
		huhealingWoundIndicator.m_Object.transform.localPosition = Vector3.zero;
		huhealingWoundIndicator.m_Progress = huhealingWoundIndicator.m_Object.FindChild("HUDHealingWoundProgress").GetComponent<Image>();
		huhealingWoundIndicator.m_WoundIcon = huhealingWoundIndicator.m_Object.FindChild("HUDHealingWoundIcon").GetComponent<Image>();
		this.m_HealingWounds.Add(huhealingWoundIndicator);
	}

	private void RemoveHealingWoundIndicator()
	{
		HUHealingWoundIndicator huhealingWoundIndicator = this.m_HealingWounds[this.m_HealingWounds.Count - 1];
		base.RemoveElement(huhealingWoundIndicator.m_Object);
		this.m_HealingWounds.RemoveAt(this.m_HealingWounds.Count - 1);
	}

	private void UpdateOxygenBar()
	{
		if (SwimController.Get() && SwimController.Get().IsActive() && SwimController.Get().m_State == SwimState.Dive)
		{
			this.m_OxygenBar.gameObject.SetActive(true);
			this.m_OxygenBarBG.gameObject.SetActive(true);
			this.m_OxygenBar.fillAmount = this.m_ConditionModule.m_Oxygen / this.m_ConditionModule.m_MaxOxygen;
			Color color = this.m_OxygenBar.color;
			if (Player.Get().m_InfinityDiving)
			{
				color.a = 0.35f;
				if (!this.m_InfiniteDivingIcon.gameObject.activeSelf)
				{
					this.m_InfiniteDivingIcon.gameObject.SetActive(true);
				}
			}
			else
			{
				color.a = 1f;
				if (this.m_InfiniteDivingIcon.gameObject.activeSelf)
				{
					this.m_InfiniteDivingIcon.gameObject.SetActive(false);
				}
			}
			this.m_OxygenBar.color = color;
			return;
		}
		this.m_OxygenBar.gameObject.SetActive(false);
		this.m_OxygenBarBG.gameObject.SetActive(false);
		this.m_InfiniteDivingIcon.gameObject.SetActive(false);
	}

	private void ScrollerInitialize()
	{
		this.m_IconScroller = new IconScroller();
		foreach (RawImage im in this.m_EnergyIconsGroup.GetComponentsInChildren<RawImage>(true))
		{
			this.m_TempIcon = new EnergyIcon(im);
			this.m_IconScroller.m_EnergyIconList.Add(this.m_TempIcon);
		}
		for (int j = 0; j < this.m_IconScroller.m_EnergyIconList.Count; j++)
		{
			this.m_IconScroller.m_EnergyIconList[j].m_IconObject.SetActive(false);
			this.m_IconScroller.m_PositionsList.Add(new Vector3((float)j * this.m_IconScroller.WIDTH, 0f, 0f));
		}
		foreach (EnergyIcon energyIcon in this.m_IconScroller.m_EnergyIconList)
		{
			energyIcon.m_IconTransform.localPosition = this.m_IconScroller.m_PositionsList[this.m_IconScroller.m_EnergyIconList.IndexOf(energyIcon)];
		}
	}

	private void IconToggle(int icon, bool is_enabled)
	{
		this.m_IconScroller.m_EnergyIconList[icon].m_IconObject.SetActive(is_enabled);
		this.m_IconScroller.m_EnergyIconList[icon].m_IsEnabled = is_enabled;
		if (is_enabled)
		{
			this.m_IconScroller.m_ActiveEnergyIconList.Add(this.m_IconScroller.m_EnergyIconList[icon]);
			this.m_IconScroller.m_ActiveEnergyIconList[this.m_IconScroller.m_ActiveEnergyIconList.Count - 1].m_IconTransform.localPosition = this.m_IconScroller.m_PositionsList[this.m_IconScroller.m_ActiveEnergyIconList.Count - 1];
			return;
		}
		this.m_IconScroller.m_ActiveEnergyIconList.Remove(this.m_IconScroller.m_EnergyIconList[icon]);
		foreach (EnergyIcon energyIcon in this.m_IconScroller.m_ActiveEnergyIconList)
		{
			base.StartCoroutine(this.AnimateIcon(energyIcon, this.m_IconScroller.m_PositionsList[this.m_IconScroller.m_ActiveEnergyIconList.IndexOf(energyIcon)]));
		}
	}

	private void IconsCheckLastFrame()
	{
		if (this.m_isIconWoundsEnabled != this.m_isIconWoundsEnabledLastFrame)
		{
			this.IconToggle(0, this.m_isIconWoundsEnabled);
		}
		if (this.m_isIconNutritionEnabled != this.m_isIconNutritionEnabledLastFrame)
		{
			this.IconToggle(1, this.m_isIconNutritionEnabled);
		}
		if (this.m_isIconFoodPoisonEnabled != this.m_isIconFoodPoisonEnabledLastFrame)
		{
			this.IconToggle(2, this.m_isIconFoodPoisonEnabled);
		}
		if (this.m_isIconPoisonEnabled != this.m_isIconPoisonEnabledLastFrame)
		{
			this.IconToggle(3, this.m_isIconPoisonEnabled);
		}
		if (this.m_isIconFeverEnabled != this.m_isIconFeverEnabledLastFrame)
		{
			this.IconToggle(4, this.m_isIconFeverEnabled);
		}
		if (this.m_IsIconParasiteSicknessEnabled != this.m_isIconParasiteSicknessEnabledLastFrame)
		{
			this.IconToggle(5, this.m_IsIconParasiteSicknessEnabled);
		}
		if (this.m_IsIconInsomniaEnabled != this.m_isIconInsomniaEnabledLastFrame)
		{
			this.IconToggle(6, this.m_IsIconInsomniaEnabled);
		}
		if (this.m_IsIconDamagedArmorEnabled != this.m_isIconDamagedArmorEnabledLastFrame)
		{
			this.IconToggle(7, this.m_IsIconDamagedArmorEnabled);
		}
		if (this.m_IsIconDirtinessEnabled != this.m_isIconDirtinessEnabledLastFrame)
		{
			this.IconToggle(8, this.m_IsIconDirtinessEnabled);
		}
		this.m_isIconWoundsEnabledLastFrame = this.m_isIconWoundsEnabled;
		this.m_isIconNutritionEnabledLastFrame = this.m_isIconNutritionEnabled;
		this.m_isIconFoodPoisonEnabledLastFrame = this.m_isIconFoodPoisonEnabled;
		this.m_isIconPoisonEnabledLastFrame = this.m_isIconPoisonEnabled;
		this.m_isIconFeverEnabledLastFrame = this.m_isIconFeverEnabled;
		this.m_isIconParasiteSicknessEnabledLastFrame = this.m_IsIconParasiteSicknessEnabled;
		this.m_isIconInsomniaEnabledLastFrame = this.m_IsIconInsomniaEnabled;
		this.m_isIconDamagedArmorEnabledLastFrame = this.m_IsIconDamagedArmorEnabled;
		this.m_isIconDirtinessEnabledLastFrame = this.m_IsIconDirtinessEnabled;
	}

	private IEnumerator AnimateIcon(EnergyIcon icon, Vector3 newpos)
	{
		float i = 0f;
		while (i < 1f)
		{
			i += Time.deltaTime / this.m_IconScroller.ANIM_TIME;
			icon.m_IconTransform.localPosition = Vector3.Lerp(icon.m_IconTransform.localPosition, newpos, i);
			yield return null;
		}
		icon.m_IconTransform.localPosition = newpos;
		yield break;
		yield break;
	}

	private void StartBlinkRedBackground(float health_scale)
	{
		this.m_BlinkRedbackgroundActive = true;
		this.m_BlinkRedBackgroundStartTime = Time.time;
	}

	private void StopBlinkRedBackground()
	{
		this.m_BlinkRedbackgroundActive = false;
		this.m_BlinkRedBackgroundStartTime = float.MinValue;
	}

	private void BlinkRedBackground(float healthscale)
	{
		float num = Time.time - this.m_BlinkRedBackgroundStartTime;
		Color color = new Color(1f, 1f, 1f, 0f);
		Color color2 = new Color(1f, 1f, 1f, 0.3f);
		if (num < 0.25f)
		{
			this.m_EnergyBackgroundRed.color = Color.Lerp(color, color2, num * 4f);
		}
		else
		{
			this.m_EnergyBackgroundRed.color = color2;
			if (num > 0.25f && num < 0.5f)
			{
				this.m_EnergyBackgroundRed.color = Color.Lerp(color2, color, num * 4f);
			}
			else
			{
				this.m_EnergyBackgroundRed.color = color;
			}
		}
		if (num > 0.75f)
		{
			this.StopBlinkRedBackground();
		}
	}

	private PlayerConditionModule m_ConditionModule;

	private PlayerInjuryModule m_PIM;

	private PlayerDiseasesModule m_PDM;

	private PlayerArmorModule m_PAM;

	public Image m_Energy;

	public Image m_Health;

	public Image m_HealthRed;

	public Image m_MaxHealth;

	public RawImage m_EnergyBackgroundRed;

	public RawImage m_EnergyUsedByNoNutritionIcon;

	public RawImage m_EnergyUsedByWoundsIcon;

	public RawImage m_EnergyFoodPoisonIcon;

	public Image m_EnergyFoodPoisonRadial;

	public Text m_EnergyFoodPoisonLevel;

	public RawImage m_EnergyPoisonIcon;

	public Image m_EnergyPoisonRadial;

	public Text m_EnergyPoisonLevelText;

	public RawImage m_EnergyFeverIcon;

	public Image m_EnergyFeverRadial;

	public Text m_EnergyFeverLevel;

	public RawImage m_EnergyParasiteSicknessIcon;

	public Text m_EnergyParasiteSicknessLevel;

	public RawImage m_EnergyInsomniaIcon;

	public Text m_EnergyInsomniaLevel;

	public Image m_EnergyInsomniaRadial;

	public RawImage m_EnergyArmorDamagedIcon;

	public RawImage m_EnergyDirtinessIcon;

	public Text m_EnergyDirtinessLevel;

	public Image m_EnergyDirtinessRadial;

	public Image m_Stamina;

	private Color m_StaminaColor = Color.white;

	public Color m_StaminaCriticalLevelColor = Color.red;

	public RawImage m_HealthArrowDown;

	public RawImage m_HealthArrowUp;

	public RawImage m_EnergyArrowUp;

	public GameObject m_EnergyIconsGroup;

	public Sprite m_WoundHandSprite;

	public Sprite m_WoundLegSprite;

	private const float m_BlinkingSpeed = 2.5f;

	private Color m_RedColor = new Color(0.78f, 0.082f, 0.082f);

	private float m_BlinkScaleMin = 0.8f;

	private float m_BlinkScaleMax = 1.1f;

	public AnimationCurve m_BlinkingCurve;

	private float m_BeginTime;

	[Range(1f, 4f)]
	public float m_BlinkingTime = 2f;

	private float m_HealthLastFrame;

	private Vector3 m_NoNutritionIconStartScale = Vector3.one;

	private Vector3 m_WoundsIconStartScale = Vector3.one;

	private Vector3 m_FoodPoisonIconStartScale = Vector3.one;

	private Vector3 m_PoisonIconStartScale = Vector3.one;

	private Vector3 m_FeverIconStartScale = Vector3.one;

	private Vector3 m_ParasiteSicknessIconStartScale = Vector3.one;

	private Vector3 m_InsomniaIconStartScale = Vector3.one;

	private Vector3 m_DamagedArmorIconStartScale = Vector3.one;

	private Vector3 m_WoundIconOffset = new Vector3(0f, -42f, 0f);

	private Vector3 m_DirtinessIconStartScale = Vector3.one;

	private List<HUHealingWoundIndicator> m_HealingWounds = new List<HUHealingWoundIndicator>();

	private Transform m_HealingWoundDummy;

	public RawImage m_OxygenBarBG;

	public Image m_OxygenBar;

	public RawImage m_InfiniteDivingIcon;

	public IconScroller m_IconScroller;

	private EnergyIcon m_TempIcon;

	private bool m_isIconWoundsEnabled;

	private bool m_isIconNutritionEnabled;

	private bool m_isIconFoodPoisonEnabled;

	private bool m_isIconPoisonEnabled;

	private bool m_isIconFeverEnabled;

	private bool m_IsIconParasiteSicknessEnabled;

	private bool m_IsIconInsomniaEnabled;

	private bool m_IsIconDamagedArmorEnabled;

	private bool m_IsIconDirtinessEnabled;

	private bool m_isIconWoundsEnabledLastFrame;

	private bool m_isIconNutritionEnabledLastFrame;

	private bool m_isIconFoodPoisonEnabledLastFrame;

	private bool m_isIconPoisonEnabledLastFrame;

	private bool m_isIconFeverEnabledLastFrame;

	private bool m_isIconParasiteSicknessEnabledLastFrame;

	private bool m_isIconInsomniaEnabledLastFrame;

	private bool m_isIconDamagedArmorEnabledLastFrame;

	private bool m_isIconDirtinessEnabledLastFrame;

	public float m_HPBlinkMinDiff = 1f;

	private static HUDEnergy s_Instance;

	private bool m_BlinkRedbackgroundActive;

	private float m_BlinkRedBackgroundStartTime = float.MinValue;

	private bool[] m_BlinkArmorRedbackgroundActive = new bool[4];

	private float[] m_BlinkArmorRedBackgroundStartTime = new float[4];

	private List<RectTransform> m_RectTransformList = new List<RectTransform>(2);

	private float[] m_ArmorHealtLastFrame = new float[4];
}
