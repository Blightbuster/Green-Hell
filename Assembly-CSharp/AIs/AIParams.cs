using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace AIs
{
	public class AIParams
	{
		public void Load(Key key)
		{
			for (int i = 0; i < key.GetKeysCount(); i++)
			{
				Key key2 = key.GetKey(i);
				if (key2.GetName() == "Health")
				{
					this.m_Health = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "HealthRegeneration")
				{
					this.m_HealthRegeneration = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "MoraleRegeneration")
				{
					this.m_MoraleRegeneration = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "MoraleDamageDecrease")
				{
					this.m_MoraleDamageDecrease = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "CanWalk")
				{
					this.m_CanWalk = (key2.GetVariable(0).IValue != 0);
				}
				else if (key2.GetName() == "CanTrot")
				{
					this.m_CanTrot = (key2.GetVariable(0).IValue != 0);
				}
				else if (key2.GetName() == "CanRun")
				{
					this.m_CanRun = (key2.GetVariable(0).IValue != 0);
				}
				else if (key2.GetName() == "CanSneak")
				{
					this.m_CanSneak = (key2.GetVariable(0).IValue != 0);
				}
				else if (key2.GetName() == "WalkSpeedMul")
				{
					this.m_WalkSpeedMul = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "TrotSpeedMul")
				{
					this.m_TrotSpeedMul = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "RunSpeedMul")
				{
					this.m_RunSpeedMul = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "SneakSpeedMul")
				{
					this.m_SneakSpeedMul = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "AllignToTerrainFull")
				{
					this.m_AllignToTerrainFull = (key2.GetVariable(0).IValue != 0);
				}
				else if (key2.GetName() == "AllignToTerrainFactor")
				{
					this.m_AllignToTerrainFactor = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "WalkAddRotPower")
				{
					this.m_WalkAddRotPower = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "TrotAddRotPower")
				{
					this.m_TrotAddRotPower = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "RunAddRotPower")
				{
					this.m_RunAddRotPower = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "SneakAddRotPower")
				{
					this.m_SneakAddRotPower = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "AttackRange")
				{
					this.m_AttackRange = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "Damage")
				{
					this.m_Damage = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "DamageEffect")
				{
					this.m_DamageEffect = (Enums.DiseaseSymptom)Enum.Parse(typeof(Enums.DiseaseSymptom), key2.GetVariable(0).SValue);
				}
				else if (key2.GetName() == "EnemySenseRange")
				{
					this.m_EnemySenseRange = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "SightAngle")
				{
					this.m_SightAngle = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "SightRange")
				{
					this.m_SightRange = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "HearingSneakRange")
				{
					this.m_HearingSneakRange = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "HearingWalkRange")
				{
					this.m_HearingWalkRange = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "HearingRunRange")
				{
					this.m_HearingRunRange = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "HearingSwimRange")
				{
					this.m_HearingSwimRange = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "HearingActionRange")
				{
					this.m_HearingActionRange = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "DeadBodyFoodSpoilTime")
				{
					this.m_DeadBodyFoodSpoilTime = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "DisableCollisionsWithPlayer")
				{
					this.m_DisableCollisionsWithPlayer = (key2.GetVariable(0).IValue != 0);
				}
				else if (key2.GetName() == "HarvestingResult")
				{
					string svalue = key2.GetVariable(0).SValue;
					string[] array = svalue.Split(new char[]
					{
						';'
					});
					for (int j = 0; j < array.Length; j++)
					{
						string[] array2 = array[j].Split(new char[]
						{
							'*'
						});
						GameObject prefab = GreenHellGame.Instance.GetPrefab(array2[0]);
						int num = (array2.Length <= 1) ? 1 : int.Parse(array2[1]);
						for (int k = 0; k < num; k++)
						{
							this.m_HarvestingResult.Add(prefab);
						}
					}
				}
				else if (key2.GetName() == "DamageType")
				{
					string svalue2 = key2.GetVariable(0).SValue;
					this.m_DamageType = (DamageType)Enum.Parse(typeof(DamageType), svalue2);
				}
				else if (key2.GetName() == "PoisonLevel")
				{
					this.m_PoisonLevel = key2.GetVariable(0).IValue;
				}
				else if (key2.GetName() == "Human")
				{
					this.m_Human = (key2.GetVariable(0).IValue != 0);
				}
				else if (key2.GetName() == "BigAnimal")
				{
					this.m_BigAnimal = (key2.GetVariable(0).IValue != 0);
				}
				else if (key2.GetName() == "MinBitingDuration")
				{
					this.m_MinBitingDuration = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "MaxBitingDuration")
				{
					this.m_MaxBitingDuration = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "EatBaitChance")
				{
					this.m_EatBaitChance = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "BitingChance")
				{
					this.m_BitingChance = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "Baits")
				{
					string[] array3 = key2.GetVariable(0).SValue.Split(new char[]
					{
						';'
					});
					for (int l = 0; l < array3.Length; l++)
					{
						this.m_Baits.Add((ItemID)Enum.Parse(typeof(ItemID), array3[l]));
					}
				}
				else if (key2.GetName() == "FishingRods")
				{
					string[] array4 = key2.GetVariable(0).SValue.Split(new char[]
					{
						';'
					});
					for (int m = 0; m < array4.Length; m++)
					{
						this.m_FishingRods.Add((ItemID)Enum.Parse(typeof(ItemID), array4[m]));
					}
				}
				else if (key2.GetName() == "CanLeaveSpawner")
				{
					this.m_CanLeaveSpawner = (key2.GetVariable(0).IValue != 0);
				}
				else if (key2.GetName() == "AvoidancePriority")
				{
					this.m_AvoidancePriority = key2.GetVariable(0).IValue;
				}
				else if (key2.GetName() == "JumpAttackRange")
				{
					this.m_JumpAttackRange = key2.GetVariable(0).FValue;
				}
				else if (key2.GetName() == "JumpBackRange")
				{
					this.m_JumpBackRange = key2.GetVariable(0).FValue;
				}
			}
		}

		public bool m_Human;

		public bool m_BigAnimal;

		public float m_Health;

		public float m_HealthRegeneration;

		public float m_MoraleRegeneration;

		public float m_MoraleDamageDecrease;

		public bool m_CanWalk;

		public bool m_CanTrot;

		public bool m_CanRun;

		public bool m_CanSneak;

		public bool m_CanLeaveSpawner;

		public float m_AllignToTerrainFactor = 1f;

		public bool m_AllignToTerrainFull;

		public float m_WalkSpeedMul = 1f;

		public float m_TrotSpeedMul = 1f;

		public float m_RunSpeedMul = 1f;

		public float m_SneakSpeedMul = 1f;

		public float m_WalkAddRotPower;

		public float m_TrotAddRotPower;

		public float m_RunAddRotPower;

		public float m_SwimAddRotPower;

		public float m_SneakAddRotPower;

		public float m_EnemySenseRange;

		public float m_AttackRange;

		public float m_Damage;

		public Enums.DiseaseSymptom m_DamageEffect = Enums.DiseaseSymptom.None;

		public float m_SightAngle;

		public float m_SightRange;

		public float m_HearingSneakRange;

		public float m_HearingWalkRange;

		public float m_HearingRunRange;

		public float m_HearingSwimRange;

		public float m_HearingActionRange;

		public int m_AvoidancePriority = 50;

		public List<GameObject> m_HarvestingResult = new List<GameObject>();

		public float m_DeadBodyFoodSpoilTime;

		public bool m_DisableCollisionsWithPlayer;

		public DamageType m_DamageType;

		public int m_PoisonLevel;

		public float m_MinBitingDuration;

		public float m_MaxBitingDuration;

		public float m_EatBaitChance;

		public float m_BitingChance;

		public List<ItemID> m_Baits = new List<ItemID>();

		public List<ItemID> m_FishingRods = new List<ItemID>();

		public float m_JumpAttackRange;

		public float m_JumpBackRange;
	}
}
