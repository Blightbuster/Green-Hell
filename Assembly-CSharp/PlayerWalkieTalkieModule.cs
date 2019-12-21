using System;
using UnityEngine;

public class PlayerWalkieTalkieModule : PlayerModule
{
	public static PlayerWalkieTalkieModule Get()
	{
		return PlayerWalkieTalkieModule.s_Instance;
	}

	private void Awake()
	{
		PlayerWalkieTalkieModule.s_Instance = this;
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Player/PlayerWalkieTalkie", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "CallBatteryUse")
			{
				this.m_CallBatteryUse = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "BatteryRestorePerSec")
			{
				this.m_BatteryRestorePerSec = key.GetVariable(0).FValue;
			}
		}
	}

	public void OnCall()
	{
		this.m_BatteryLevel -= this.m_CallBatteryUse;
		this.m_BatteryLevel = Mathf.Clamp01(this.m_BatteryLevel);
	}

	public bool CanCall()
	{
		return this.GetBatteryLevel() >= this.m_CallBatteryUse;
	}

	public override void Update()
	{
		base.Update();
		if (MainLevel.Instance.m_TODSky.IsDay)
		{
			float num = Time.deltaTime;
			if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
			{
				num = Player.GetSleepTimeFactor();
			}
			this.m_BatteryLevel += this.m_BatteryRestorePerSec * num;
		}
		this.m_BatteryLevel = Mathf.Clamp01(this.m_BatteryLevel);
	}

	public float GetBatteryLevel()
	{
		if (PlayerSanityModule.Get().IsWhispersLevel())
		{
			return 0f;
		}
		return this.m_BatteryLevel;
	}

	public void SetBatteryLevel(float level)
	{
		this.m_BatteryLevel = level;
	}

	public bool IsBatteryLevelGreater(float level)
	{
		return this.GetBatteryLevel() > level;
	}

	[HideInInspector]
	public float m_BatteryLevel = 1f;

	[HideInInspector]
	public float m_CallBatteryUse = 0.3f;

	private static PlayerWalkieTalkieModule s_Instance;

	private float m_BatteryRestorePerSec = 0.01f;
}
