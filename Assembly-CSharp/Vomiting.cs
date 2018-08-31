using System;
using Enums;
using UnityEngine;

public class Vomiting : global::DiseaseSymptom
{
	public override void Initialize()
	{
		base.Initialize();
		this.m_Type = Enums.DiseaseSymptom.Vomiting;
		this.m_Duration = 300f;
	}

	public override void ParseKey(Key key)
	{
		base.ParseKey(key);
		for (int i = 0; i < key.GetKeysCount(); i++)
		{
			Key key2 = key.GetKey(i);
			if (key2.GetName() == "CarboLoss")
			{
				this.m_CarboLoss = key2.GetVariable(0).FValue;
			}
			else if (key2.GetName() == "ProteinsLoss")
			{
				this.m_ProteinsLoss = key2.GetVariable(0).FValue;
			}
			else if (key2.GetName() == "FatLoss")
			{
				this.m_FatLoss = key2.GetVariable(0).FValue;
			}
			else if (key2.GetName() == "HydrationLoss")
			{
				this.m_HydrationLoss = key2.GetVariable(0).FValue;
			}
		}
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		this.m_NextVomitingTime = 0f;
	}

	public override void Update()
	{
		base.Update();
		if ((Time.time >= this.m_NextVomitingTime && !TriggerController.Get().m_TriggerInAction) || (Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.V)))
		{
			if (Inventory3DManager.Get().isActiveAndEnabled)
			{
				Inventory3DManager.Get().Deactivate();
			}
			Player.Get().StartController(PlayerControllerType.Vomiting);
			this.m_NextVomitingTime = float.MaxValue;
			this.ApplyPlayerParams();
		}
	}

	private void ApplyPlayerParams()
	{
		PlayerConditionModule component = Player.Get().GetComponent<PlayerConditionModule>();
		float num = component.GetNutritionCarbo() * this.m_CarboLoss;
		component.DecreaseNutritionCarbo(num + this.m_CarboLossFromEating);
		num = component.GetNutritionFat() * this.m_FatLoss;
		component.DecreaseNutritionFat(num + this.m_FatLossFromEating);
		num = component.GetNutritionProtein() * this.m_ProteinsLoss;
		component.DecreaseNutritionProtein(num + this.m_ProteinsLossFromEating);
		num = component.GetHydration() * this.m_HydrationLoss;
		component.DecreaseHydration(num + this.m_HydrationLossFromDrinking + this.m_HydrationLossFromEating);
		this.m_HydrationLossFromDrinking = 0f;
		this.m_CarboLossFromEating = 0f;
		this.m_ProteinsLossFromEating = 0f;
		this.m_FatLossFromEating = 0f;
		this.m_HydrationLossFromEating = 0f;
	}

	public override void OnEat(ConsumableInfo info)
	{
		base.OnEat(info);
		this.m_NextVomitingTime = Time.time + this.m_VomitingDelay;
		this.m_CarboLossFromEating = info.m_Carbohydrates;
		this.m_ProteinsLossFromEating = info.m_Proteins;
		this.m_FatLossFromEating = info.m_Fat;
		this.m_HydrationLossFromEating = info.m_Water;
	}

	public override void OnDrink(LiquidType type, float hydration_amount)
	{
		base.OnDrink(type, hydration_amount);
		this.m_NextVomitingTime = Time.time + this.m_VomitingDelay;
		this.m_HydrationLossFromDrinking += hydration_amount;
	}

	private float m_NextVomitingTime;

	private float m_VomitingDelay = 5f;

	private float m_CarboLoss;

	private float m_ProteinsLoss;

	private float m_FatLoss;

	private float m_HydrationLoss;

	private float m_HydrationLossFromDrinking;

	private float m_CarboLossFromEating;

	private float m_ProteinsLossFromEating;

	private float m_FatLossFromEating;

	private float m_HydrationLossFromEating;
}
