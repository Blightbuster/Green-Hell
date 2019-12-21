using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class LiquidManager : MonoBehaviour
{
	public static LiquidManager Get()
	{
		return LiquidManager.s_Instance;
	}

	private void Awake()
	{
		LiquidManager.s_Instance = this;
		this.LoadScript();
	}

	private void LoadScript()
	{
		TextAssetParser textAssetParser = new TextAssetParser(Resources.Load(LiquidManager.s_ScriptName) as TextAsset);
		for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
		{
			Key key = textAssetParser.GetKey(i);
			if (key.GetName() == "Liquid")
			{
				LiquidData liquidData = new LiquidData();
				liquidData.m_LiquidType = (LiquidType)Enum.Parse(typeof(LiquidType), key.GetVariable(0).SValue);
				for (int j = 0; j < key.GetKeysCount(); j++)
				{
					Key key2 = key.GetKey(j);
					if (key2.GetName() == "Components")
					{
						liquidData.m_LiquidComponent = (LiquidType)Enum.Parse(typeof(LiquidType), key2.GetVariable(0).SValue);
						liquidData.m_ItemComponent = (ItemID)Enum.Parse(typeof(ItemID), key2.GetVariable(1).SValue);
					}
					else if (key2.GetName() == "Fat")
					{
						liquidData.m_Fat = key2.GetVariable(0).FValue;
					}
					else if (key2.GetName() == "Carbohydrates")
					{
						liquidData.m_Carbohydrates = key2.GetVariable(0).FValue;
					}
					else if (key2.GetName() == "Proteins")
					{
						liquidData.m_Proteins = key2.GetVariable(0).FValue;
					}
					else if (key2.GetName() == "Water")
					{
						liquidData.m_Water = key2.GetVariable(0).FValue;
					}
					else if (key2.GetName() == "SanityChange")
					{
						liquidData.m_SanityChange = key2.GetVariable(0).IValue;
					}
					else if (key2.GetName() == "CookingResult")
					{
						liquidData.m_CookingResult = (LiquidType)Enum.Parse(typeof(LiquidType), key2.GetVariable(0).SValue);
					}
					else if (key2.GetName() == "DrinkEffect")
					{
						LiquidConsumeEffectData liquidConsumeEffectData = new LiquidConsumeEffectData();
						liquidConsumeEffectData.m_ConsumeEffect = (ConsumeEffect)Enum.Parse(typeof(ConsumeEffect), key2.GetVariable(0).SValue);
						liquidConsumeEffectData.m_ConsumeEffectChance = key2.GetVariable(1).FValue;
						liquidConsumeEffectData.m_ConsumeEffectDelay = key2.GetVariable(2).FValue;
						liquidConsumeEffectData.m_ConsumeEffectLevel = key2.GetVariable(3).IValue;
						liquidData.m_ConsumeEffects.Add(liquidConsumeEffectData);
					}
					else if (key2.GetName() == "Energy")
					{
						liquidData.m_Energy = key2.GetVariable(0).FValue;
					}
					else if (key2.GetName() == "PoisonDebuff")
					{
						liquidData.m_PoisonDebuff = key2.GetVariable(0).IValue;
					}
					else if (key2.GetName() == "Disgusting")
					{
						liquidData.m_Disgusting = (key2.GetVariable(0).IValue != 0);
					}
				}
				this.m_LiquidDatas.Add(liquidData);
			}
		}
	}

	public LiquidData GetLiquidData(LiquidType type)
	{
		foreach (LiquidData liquidData in this.m_LiquidDatas)
		{
			if (liquidData.m_LiquidType == type)
			{
				return liquidData;
			}
		}
		return null;
	}

	public LiquidData GetLiquidDataByComponents(LiquidType type, ItemID id)
	{
		foreach (LiquidData liquidData in this.m_LiquidDatas)
		{
			if (liquidData.m_LiquidComponent == type && liquidData.m_ItemComponent == id)
			{
				return liquidData;
			}
		}
		return null;
	}

	private static LiquidManager s_Instance = null;

	[HideInInspector]
	public List<LiquidData> m_LiquidDatas = new List<LiquidData>();

	private static string s_ScriptName = "Scripts/Liquids";
}
