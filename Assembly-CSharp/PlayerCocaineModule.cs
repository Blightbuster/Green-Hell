using System;
using UnityEngine;

public class PlayerCocaineModule : PlayerModule
{
	public static PlayerCocaineModule Get()
	{
		return PlayerCocaineModule.s_Instance;
	}

	private void Awake()
	{
		PlayerCocaineModule.s_Instance = this;
	}

	public override void Initialize()
	{
		base.Initialize();
		this.LoadScript();
	}

	private void LoadScript()
	{
		TextAsset textAsset = Resources.Load(this.m_CocaineScript) as TextAsset;
		if (!textAsset)
		{
			DebugUtils.Assert("Can't load Sanity script - " + this.m_CocaineScript, true, DebugUtils.AssertType.Info);
			return;
		}
		TextAssetParser textAssetParser = new TextAssetParser(textAsset);
		for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
		{
			Key key = textAssetParser.GetKey(i);
			if (key.GetName() == "Duartion")
			{
				this.m_Duration = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "StaminaConsumptionMul")
			{
				this.m_StaminaConsumptionMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HPConsumptionMul")
			{
				this.m_HPConsumptionMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "FatConsumptionMul")
			{
				this.m_FatConsumptionMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "CarboConsumptionMul")
			{
				this.m_CarboConsumptionMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "ProteinsConsumptionMul")
			{
				this.m_ProteinsConsumptionMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HydrationConsumptionMul")
			{
				this.m_HydrationConsumptionMul = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "IncreaseEnergyPerSec")
			{
				this.m_IncreaseEnergyPerSec = key.GetVariable(0).FValue;
			}
		}
		Resources.UnloadAsset(textAsset);
	}

	public void OnEatCocaine()
	{
		this.m_StartTime = MainLevel.Instance.m_TODSky.Cycle.GameTime;
		this.m_Active = true;
	}

	public override void Update()
	{
		base.Update();
		if (this.m_Active && MainLevel.Instance.m_TODSky.Cycle.GameTime - this.m_StartTime >= this.m_Duration)
		{
			this.m_Active = false;
		}
		float weight = PostProcessManager.Get().GetWeight(PostProcessManager.Effect.Coca);
		if (this.m_Active && weight < 1f)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Coca, weight + Time.deltaTime * 3f);
		}
		else if (!this.m_Active && weight > 0f)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Coca, weight - Time.deltaTime * 3f);
		}
		if (this.m_Active && this.m_IncreaseEnergyPerSec > 0f)
		{
			PlayerConditionModule.Get().IncreaseEnergy(this.m_IncreaseEnergyPerSec * Time.deltaTime);
		}
	}

	public string m_CocaineScript = "Scripts/Player/PlayerCocaine";

	private float m_StartTime;

	private float m_Duration = 2f;

	[HideInInspector]
	public bool m_Active;

	[HideInInspector]
	public float m_StaminaConsumptionMul = 1f;

	[HideInInspector]
	public float m_HPConsumptionMul = 1f;

	[HideInInspector]
	public float m_FatConsumptionMul = 1f;

	[HideInInspector]
	public float m_CarboConsumptionMul = 1f;

	[HideInInspector]
	public float m_ProteinsConsumptionMul = 1f;

	[HideInInspector]
	public float m_HydrationConsumptionMul = 1f;

	[HideInInspector]
	public float m_IncreaseEnergyPerSec;

	private static PlayerCocaineModule s_Instance;
}
