﻿using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class Disease
{
	public virtual void Load(Key key)
	{
		if (key.GetName() == "Symptom")
		{
			string svalue = key.GetVariable(0).SValue;
			this.m_Symptoms.Add((Enums.DiseaseSymptom)Enum.Parse(typeof(Enums.DiseaseSymptom), svalue));
			return;
		}
		if (key.GetName() == "AutoHealTimeMinutes")
		{
			this.m_AutoHealTime = key.GetVariable(0).FValue;
			return;
		}
		if (key.GetName() == "EnergyLossPerSecond")
		{
			this.m_EnergyLossPerSecond = key.GetVariable(0).FValue;
			return;
		}
		if (key.GetName() == "MaxLevel")
		{
			this.m_MaxLevel = key.GetVariable(0).IValue;
		}
	}

	public ConsumeEffect GetDiseaseType()
	{
		return this.m_Type;
	}

	public float GetStartTime()
	{
		return this.m_StartTime;
	}

	public virtual void Update()
	{
		this.ApplyPlayerParams();
		this.CheckAutoHeal();
	}

	protected virtual void ApplyPlayerParams()
	{
		PlayerConditionModule.Get().DecreaseEnergy(this.m_EnergyLossPerSecond * Time.deltaTime);
	}

	protected virtual void CheckAutoHeal()
	{
		float currentTimeMinutes = MainLevel.Instance.GetCurrentTimeMinutes();
		if (currentTimeMinutes > this.m_StartTime + this.m_AutoHealTime)
		{
			this.m_Level--;
			this.m_StartTime = currentTimeMinutes;
		}
		if (this.m_Level == 0)
		{
			this.Deactivate();
		}
	}

	public virtual void Activate(DiseaseRequest request)
	{
		if (!this.m_Active)
		{
			this.m_Active = true;
			this.m_StartTime = MainLevel.Instance.GetCurrentTimeMinutes();
			this.m_Level = request.m_Level;
			this.ActivateSymptoms();
			PlayerSanityModule.Get().OnEvent(PlayerSanityModule.SanityEventType.Disease, 1);
			PlayerDiseasesModule.Get().UnlockKnownDisease(this.m_Type);
			this.m_Activated = true;
		}
	}

	private void ActivateSymptoms()
	{
		for (int i = 0; i < this.m_Symptoms.Count; i++)
		{
			global::DiseaseSymptom symptom = PlayerDiseasesModule.Get().GetSymptom(this.m_Symptoms[i]);
			if (symptom == null)
			{
				DebugUtils.Assert(DebugUtils.AssertType.Info);
			}
			else if (!symptom.IsActive())
			{
				symptom.Activate();
			}
		}
	}

	public void DeactivateSymptoms()
	{
		for (int i = 0; i < this.m_Symptoms.Count; i++)
		{
			global::DiseaseSymptom symptom = PlayerDiseasesModule.Get().GetSymptom(this.m_Symptoms[i]);
			if (symptom == null)
			{
				DebugUtils.Assert(DebugUtils.AssertType.Info);
			}
			else if (symptom.IsActive())
			{
				symptom.Deactivate();
			}
		}
	}

	public virtual void Deactivate()
	{
		this.m_Active = false;
	}

	public bool IsActive()
	{
		return this.m_Active;
	}

	public List<Enums.DiseaseSymptom> GetAllSymptoms()
	{
		return this.m_Symptoms;
	}

	public void OnDrink(LiquidType liquid_type)
	{
		LiquidData liquidData = LiquidManager.Get().GetLiquidData(liquid_type);
		for (int i = 0; i < liquidData.m_ConsumeEffects.Count; i++)
		{
			if (liquidData.m_ConsumeEffects[i].m_ConsumeEffect == this.m_Type)
			{
				this.m_Level += liquidData.m_ConsumeEffects[i].m_ConsumeEffectLevel;
			}
		}
		this.m_Level = Mathf.Clamp(this.m_Level, 0, this.m_MaxLevel);
		if (this.m_Level == 0)
		{
			this.Deactivate();
		}
	}

	protected virtual bool CanApplyConsumeEffect(ConsumableInfo info)
	{
		return true;
	}

	public virtual void OnEat(ConsumableInfo info)
	{
		if (info.m_ConsumeEffect == this.m_Type && this.CanApplyConsumeEffect(info))
		{
			this.m_Level += info.m_ConsumeEffectLevel;
			Localization localization = GreenHellGame.Instance.GetLocalization();
			HUDMessages hudmessages = (HUDMessages)HUDManager.Get().GetHUD(typeof(HUDMessages));
			string text = string.Empty;
			if (info.m_ConsumeEffect == ConsumeEffect.ParasiteSickness)
			{
				text = info.m_ConsumeEffectLevel.ToString("F0") + " " + localization.Get("HUD_ParasiteSickness", true);
				hudmessages.AddMessage(text, null, HUDMessageIcon.ParasiteSickness, "", null);
			}
		}
		this.m_Level = Mathf.Clamp(this.m_Level, 0, this.m_MaxLevel);
		if (this.m_Level == 0)
		{
			this.Deactivate();
		}
	}

	public virtual void Save(int idx)
	{
		SaveGame.SaveVal("Disease" + idx.ToString() + "Active", this.m_Active);
		SaveGame.SaveVal("Disease" + idx.ToString() + "StartTime", this.m_StartTime);
		SaveGame.SaveVal("Disease" + idx.ToString() + "Type", (int)this.m_Type);
		SaveGame.SaveVal("Disease" + idx.ToString() + "EL", this.m_EnergyLossPerSecond);
		SaveGame.SaveVal("Disease" + idx.ToString() + "Lvl", this.m_Level);
		SaveGame.SaveVal("Disease" + idx.ToString() + "Activated", this.m_Activated);
	}

	public virtual void Load(int idx)
	{
		bool flag;
		SaveGame.LoadVal("Disease" + idx.ToString() + "Active", out flag, false);
		this.m_Active = flag;
		float num;
		SaveGame.LoadVal("Disease" + idx.ToString() + "StartTime", out num, false);
		this.m_StartTime = num;
		int num2;
		SaveGame.LoadVal("Disease" + idx.ToString() + "Type", out num2, false);
		this.m_Type = (ConsumeEffect)num2;
		SaveGame.LoadVal("Disease" + idx.ToString() + "EL", out num, false);
		this.m_EnergyLossPerSecond = num;
		SaveGame.LoadVal("Disease" + idx.ToString() + "Lvl", out num2, false);
		this.m_Level = num2;
		SaveGame.LoadVal("Disease" + idx.ToString() + "Activated", out flag, false);
		this.m_Activated = flag;
	}

	public virtual void Check()
	{
	}

	public void IncreaseLevel(int add)
	{
		this.m_Level += add;
		this.m_Level = Mathf.Clamp(this.m_Level, 0, this.m_MaxLevel);
	}

	public List<Enums.DiseaseSymptom> m_Symptoms = new List<Enums.DiseaseSymptom>();

	private bool m_Active;

	[HideInInspector]
	public float m_StartTime;

	public ConsumeEffect m_Type = ConsumeEffect.None;

	[HideInInspector]
	public float m_AutoHealTime = float.MaxValue;

	public float m_EnergyLossPerSecond;

	[HideInInspector]
	public int m_Level;

	[HideInInspector]
	public int m_MaxLevel = int.MaxValue;

	private bool m_Activated;
}
