using System;
using Enums;
using UnityEngine;

public class Insomnia : Disease
{
	public override void Load(Key key)
	{
		if (key.GetName() == "NoSleepTimeToActivate")
		{
			this.m_NoSleepTmeToActivate = key.GetVariable(0).FValue;
			return;
		}
		if (key.GetName() == "NoSleepTimeToIncreaseLevel")
		{
			this.m_NoSleepTimeToIncreaseLevel = key.GetVariable(0).FValue;
			return;
		}
		if (key.GetName() == "EnergyLossMull")
		{
			this.m_EnergyLossMul = key.GetVariable(0).FValue;
			return;
		}
		if (key.GetName() == "EnergyLossMulAddByLevel")
		{
			this.m_EnergyLossMulAddByLevel = key.GetVariable(0).FValue;
			return;
		}
		if (key.GetName() == "SleepTimeToDecreaseLevel")
		{
			this.m_SleepTimeToDecreaseLevel = key.GetVariable(0).FValue;
			return;
		}
		if (key.GetName() == "SanityDecreaseVal")
		{
			this.m_SanityDecreaseVal = key.GetVariable(0).IValue;
			return;
		}
		if (key.GetName() == "SanityDecreaseInterval")
		{
			this.m_SanityDecreaseInterval = key.GetVariable(0).FValue;
			return;
		}
		base.Load(key);
	}

	public override void Activate(DiseaseRequest request)
	{
		base.Activate(request);
		this.m_LastSanityDecreaseTime = MainLevel.s_GameTime;
	}

	public override void Deactivate()
	{
		base.Deactivate();
		SleepController.Get().m_LastWakeUpTimeLogical = MainLevel.Instance.GetCurrentTimeMinutes();
		SleepController.Get().m_LastWakeUpTime = MainLevel.Instance.GetCurrentTimeMinutes();
	}

	public override void Check()
	{
		float currentTimeMinutes = MainLevel.Instance.GetCurrentTimeMinutes();
		if (SleepController.Get().m_LastWakeUpTime <= 0f)
		{
			SleepController.Get().m_LastWakeUpTime = currentTimeMinutes;
		}
		if (SleepController.Get().m_LastWakeUpTimeLogical <= 0f)
		{
			SleepController.Get().m_LastWakeUpTimeLogical = currentTimeMinutes;
		}
		if (SleepController.Get().IsActive() && HUDSleeping.Get().GetState() == HUDSleepingState.Progress && this.m_InsomniaLevel == 0f)
		{
			SleepController.Get().m_LastWakeUpTimeLogical = MainLevel.Instance.GetCurrentTimeMinutes();
		}
		if (currentTimeMinutes > SleepController.Get().m_LastWakeUpTimeLogical + this.m_NoSleepTmeToActivate)
		{
			PlayerDiseasesModule.Get().RequestDisease(ConsumeEffect.Insomnia, 0f, 1);
		}
	}

	public override void Update()
	{
		base.Update();
		float currentTimeMinutes = MainLevel.Instance.GetCurrentTimeMinutes();
		if (currentTimeMinutes < SleepController.Get().m_LastWakeUpTimeLogical + this.m_NoSleepTmeToActivate)
		{
			this.m_Level = 0;
			this.m_InsomniaLevel = 0f;
			this.Deactivate();
			return;
		}
		this.m_InsomniaLevel = (currentTimeMinutes - (SleepController.Get().m_LastWakeUpTimeLogical + this.m_NoSleepTmeToActivate)) / this.m_NoSleepTimeToIncreaseLevel;
		if (this.m_InsomniaLevel >= 5.9999f)
		{
			SleepController.Get().m_LastWakeUpTimeLogical = MainLevel.Instance.GetCurrentTimeMinutes();
			SleepController.Get().m_LastWakeUpTimeLogical -= this.m_NoSleepTmeToActivate;
			SleepController.Get().m_LastWakeUpTimeLogical -= this.m_NoSleepTimeToIncreaseLevel * 5.9999f;
		}
		if (this.m_InsomniaLevel <= -0.0001f)
		{
			SleepController.Get().m_LastWakeUpTimeLogical = MainLevel.Instance.GetCurrentTimeMinutes();
		}
		this.m_InsomniaLevel = Mathf.Clamp(this.m_InsomniaLevel, -0.0001f, 5.9999f);
		this.m_Level = (int)Mathf.Ceil(this.m_InsomniaLevel);
		this.m_Level = Mathf.Clamp(this.m_Level, 1, 6);
		if (MainLevel.s_GameTime - this.m_LastSanityDecreaseTime >= this.m_SanityDecreaseInterval && (!SleepController.Get().IsActive() || SleepController.Get().IsWakingUp()))
		{
			PlayerSanityModule.Get().OnEvent(PlayerSanityModule.SanityEventType.Insomnia, this.m_SanityDecreaseVal * this.m_Level);
			this.m_LastSanityDecreaseTime = MainLevel.s_GameTime;
		}
	}

	public void UpdateSleeping()
	{
		float num = 0f;
		float num2 = SleepController.Get().m_Progress * (float)SleepController.Get().m_SleepDuration;
		float num3 = SleepController.Get().m_LastProgress * (float)SleepController.Get().m_SleepDuration;
		if ((num2 >= 4f && num3 < 4f) || (num2 >= 8f && num3 < 8f))
		{
			num = 1f;
		}
		if (num <= 0f)
		{
			SleepController.Get().m_LastWakeUpTimeLogical += SleepController.Get().m_HoursDelta * 60f;
			return;
		}
		if (num >= this.m_InsomniaLevel)
		{
			SleepController.Get().m_LastWakeUpTimeLogical = MainLevel.Instance.GetCurrentTimeMinutes();
			return;
		}
		SleepController.Get().m_LastWakeUpTimeLogical += num * this.m_NoSleepTimeToIncreaseLevel;
	}

	protected override void ApplyPlayerParams()
	{
		float energyLossMulFinal = this.m_EnergyLossMul + this.m_EnergyLossMulAddByLevel * (float)this.m_Level;
		this.m_EnergyLossMulFinal = energyLossMulFinal;
	}

	public float m_NoSleepTmeToActivate = 1000f;

	public float m_NoSleepTimeToIncreaseLevel = 300f;

	private float m_EnergyLossMul = 1f;

	private float m_EnergyLossMulAddByLevel;

	public float m_EnergyLossMulFinal = 1f;

	public float m_SleepTimeToDecreaseLevel = 60f;

	private float m_SanityDecreaseInterval = 120f;

	private float m_LastSanityDecreaseTime;

	private int m_SanityDecreaseVal = 1;

	public float m_InsomniaLevel;
}
