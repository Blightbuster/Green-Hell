using System;
using Enums;

public class ConsumableInfo : ItemInfo
{
	public ConsumableInfo()
	{
		this.m_Fat = 0f;
		this.m_Carbohydrates = 0f;
		this.m_Proteins = 0f;
		this.m_Water = 0f;
		this.m_MinPart = 0f;
		this.m_Capacity = 0f;
		this.m_ConsumeEffect = ConsumeEffect.None;
		this.m_ConsumeEffectDelay = 0f;
		this.m_ConsumeEffectChance = 1f;
		this.m_ConsumeEffectLevel = 0;
		this.m_AddEnergy = 0f;
		this.m_SanityChange = 0;
		this.m_Disgusting = false;
	}

	public float m_Fat { get; set; }

	public float m_Carbohydrates { get; set; }

	public float m_Proteins { get; set; }

	public float m_Water { get; set; }

	public int m_SanityChange { get; set; }

	public float m_MinPart { get; set; }

	public float m_Capacity { get; set; }

	public ConsumeEffect m_ConsumeEffect { get; set; }

	public float m_ConsumeEffectChance { get; set; }

	public float m_ConsumeEffectDelay { get; set; }

	public int m_ConsumeEffectLevel { get; set; }

	public float m_AddEnergy { get; set; }

	public bool m_Disgusting { get; set; }

	public override bool IsConsumable()
	{
		return true;
	}

	protected override void LoadParams(Key key)
	{
		if (key.GetName() == "Fat")
		{
			this.m_Fat = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "Carbohydrates")
		{
			this.m_Carbohydrates = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "Proteins")
		{
			this.m_Proteins = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "Water")
		{
			this.m_Water = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "MinPart")
		{
			this.m_MinPart = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "Capacity")
		{
			this.m_Capacity = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "ConsumeEffect")
		{
			this.m_ConsumeEffect = (ConsumeEffect)Enum.Parse(typeof(ConsumeEffect), key.GetVariable(0).SValue);
		}
		else if (key.GetName() == "ConsumeEffectDelay")
		{
			this.m_ConsumeEffectDelay = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "ConsumeEffectChance")
		{
			this.m_ConsumeEffectChance = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "ConsumeEffectLevel")
		{
			this.m_ConsumeEffectLevel = key.GetVariable(0).IValue;
		}
		else if (key.GetName() == "SanityChange")
		{
			this.m_SanityChange = key.GetVariable(0).IValue;
		}
		else if (key.GetName() == "AddEnergy")
		{
			this.m_AddEnergy = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "Disgusting")
		{
			this.m_Disgusting = (key.GetVariable(0).IValue != 0);
		}
		else
		{
			base.LoadParams(key);
		}
	}

	public override void GetInfoText(ref string result)
	{
		base.GetInfoText(ref result);
		result = result + "Carbohydrates = " + this.m_Carbohydrates.ToString() + "\n";
		result = result + "Fat = " + this.m_Fat.ToString() + "\n";
		result = result + "Proteins = " + this.m_Proteins.ToString() + "\n";
		result = result + "Water = " + this.m_Water.ToString() + "\n";
	}
}
