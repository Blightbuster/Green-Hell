using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace AIs
{
	public class EnemyAISpawnManager : MonoBehaviour, ISaveLoad
	{
		public static EnemyAISpawnManager Get()
		{
			return EnemyAISpawnManager.s_Instance;
		}

		private void Awake()
		{
			EnemyAISpawnManager.s_Instance = this;
		}

		private void Start()
		{
			this.ParseScript();
			foreach (HumanAIGroup humanAIGroup in Resources.FindObjectsOfTypeAll<HumanAIGroup>())
			{
				if (humanAIGroup.gameObject.scene.isLoaded && !humanAIGroup.m_ChallengeGroup)
				{
					if (!DifficultySettings.ActivePreset.m_Tribes)
					{
						UnityEngine.Object.Destroy(humanAIGroup.gameObject);
					}
					else
					{
						if (humanAIGroup.m_FromBalance)
						{
							if (humanAIGroup.IsPatrol())
							{
								this.m_PatrolGroups.Add(humanAIGroup);
							}
							else
							{
								this.m_Groups.Add(humanAIGroup);
							}
						}
						else
						{
							this.m_CampGroups.Add(humanAIGroup);
						}
						humanAIGroup.Initialize();
						humanAIGroup.gameObject.SetActive(false);
					}
				}
			}
			this.m_TimeToNextSpawnGroup = this.m_GroupSpawnCooldown;
			this.m_TimeToNextSpawnPredator = this.m_PredatorSpawnCooldown;
			this.m_CurrentGroupMembersCount = this.m_MinGroupMembersCount;
			this.m_TimeToNextIncreaseGroupMembersCount = this.m_IncreaseGroupMembersInterval;
		}

		private void ParseScript()
		{
			ScriptParser scriptParser = new ScriptParser();
			scriptParser.Parse("EnemySpawnData.txt", true);
			for (int i = 0; i < scriptParser.GetKeysCount(); i++)
			{
				Key key = scriptParser.GetKey(i);
				if (key.GetName() == "MinGroupMembersCount")
				{
					this.m_MinGroupMembersCount = key.GetVariable(0).IValue;
				}
				else if (key.GetName() == "MaxGroupMembersCount")
				{
					this.m_MaxGroupMembersCount = key.GetVariable(0).IValue;
				}
				else if (key.GetName() == "IncreaseGroupMembersInterval")
				{
					this.m_IncreaseGroupMembersInterval = key.GetVariable(0).FValue;
				}
				else if (key.GetName() == "MaxActivationDistance")
				{
					this.m_MaxActivationDistance = key.GetVariable(0).FValue;
				}
				else if (key.GetName() == "MinActivationDistance")
				{
					this.m_MinActivationDistance = key.GetVariable(0).FValue;
				}
				else if (key.GetName() == "GroupSpawnCooldown")
				{
					this.m_GroupSpawnCooldown = key.GetVariable(0).FValue;
				}
				else if (key.GetName() == "PredatorSpawnCooldown")
				{
					this.m_PredatorSpawnCooldown = key.GetVariable(0).FValue;
				}
				else if (key.GetName() == "BurningFactorIncreaseSpeed")
				{
					FirecampGroupsManager.Get().m_BurningFactorIncreaseSpeed[0] = key.GetVariable(0).FValue;
					FirecampGroupsManager.Get().m_BurningFactorIncreaseSpeed[1] = key.GetVariable(1).FValue;
					FirecampGroupsManager.Get().m_BurningFactorIncreaseSpeed[2] = key.GetVariable(2).FValue;
				}
				else if (key.GetName() == "BurningFactorDecreaseSpeed")
				{
					FirecampGroupsManager.Get().m_BurningFactorDecreaseSpeed[0] = key.GetVariable(0).FValue;
					FirecampGroupsManager.Get().m_BurningFactorDecreaseSpeed[1] = key.GetVariable(1).FValue;
					FirecampGroupsManager.Get().m_BurningFactorDecreaseSpeed[2] = key.GetVariable(2).FValue;
				}
				else if (key.GetName() == "WaveSpawnCooldown")
				{
					this.m_WaveSpawnCooldown[0] = key.GetVariable(0).FValue;
					this.m_WaveSpawnCooldown[1] = key.GetVariable(1).FValue;
					this.m_WaveSpawnCooldown[2] = key.GetVariable(2).FValue;
				}
				else if (key.GetName() == "StartWaveSpawnCooldown")
				{
					this.m_StartWaveSpawnCooldown[0] = key.GetVariable(0).FValue;
					this.m_StartWaveSpawnCooldown[1] = key.GetVariable(1).FValue;
					this.m_StartWaveSpawnCooldown[2] = key.GetVariable(2).FValue;
				}
			}
		}

		public void BlockSpawn()
		{
			this.m_ScenarioBlock = true;
		}

		public void UnblockSpawn()
		{
			this.m_ScenarioBlock = false;
			switch (DifficultySettings.ActivePreset.m_BaseDifficulty)
			{
			case GameDifficulty.Easy:
				this.m_TimeToNextSpawnGroup = 1560f;
				this.m_TimeToNextSpawnPredator = 2100f;
				break;
			case GameDifficulty.Normal:
				this.m_TimeToNextSpawnGroup = 900f;
				this.m_TimeToNextSpawnPredator = 900f;
				break;
			case GameDifficulty.Hard:
				this.m_TimeToNextSpawnGroup = 600f;
				this.m_TimeToNextSpawnPredator = 600f;
				break;
			}
			this.m_TimeToNextSpawnWave = this.m_StartWaveSpawnCooldown[(int)DifficultySettings.ActivePreset.m_BaseDifficulty];
			FirecampGroupsManager.Get().ResetBurningFactors();
		}

		public HumanAIWave SpawnWave(int count, bool hallucination = false, FirecampGroup firecamp_group_for_wave = null)
		{
			HumanAIWave humanAIWave = new GameObject("AIWave")
			{
				transform = 
				{
					position = ((firecamp_group_for_wave != null) ? firecamp_group_for_wave.m_Bounds.center : Player.Get().transform.position)
				}
			}.AddComponent<HumanAIWave>();
			humanAIWave.m_Count = count;
			humanAIWave.m_Hallucination = hallucination;
			humanAIWave.SetupConstructionsToDestroy(firecamp_group_for_wave);
			humanAIWave.Initialize();
			this.m_ActiveGroup = humanAIWave;
			return humanAIWave;
		}

		private bool TrySpawnGroup(HumanAIGroup group)
		{
			bool active = group.m_Active;
			if (group.m_Members.Count == 0)
			{
				return false;
			}
			bool flag;
			if (group.IsPatrol())
			{
				flag = (((HumanAIPatrol)group).GetClosestPathPointInRange(this.m_MinActivationDistance, this.m_MaxActivationDistance) != null);
			}
			else
			{
				float num = 0f;
				flag = (group.GetClosestMember(out num) && num < this.m_MaxActivationDistance && num > this.m_MinActivationDistance);
			}
			if (flag)
			{
				group.Activate();
				return true;
			}
			return false;
		}

		public void OnDeactivateGroup(HumanAIGroup group)
		{
			if (this.m_ActiveGroup != group && this.m_ActiveCampGroup != group)
			{
				Debug.Log("OnDeactivateGroup - Inactive group!");
				return;
			}
			if (this.m_ActiveGroup == group)
			{
				this.m_ActiveGroup = null;
				this.m_TimeToNextSpawnGroup = this.m_GroupSpawnCooldown;
				return;
			}
			this.m_ActiveCampGroup = null;
		}

		public void OnActivatePredator(AI ai)
		{
			this.m_ActivePredator = ai;
		}

		public void OnDeactivatePredator(AI ai)
		{
			if (!this.m_ActivePredator)
			{
				if (!ai.IsDead())
				{
					Debug.Log("OnDeactivatePredator - Inactive predator!");
				}
				return;
			}
			if (this.m_ActivePredator == ai)
			{
				this.m_ActivePredator = null;
				this.m_TimeToNextSpawnPredator = this.m_PredatorSpawnCooldown;
			}
		}

		public int GetCurrentGroupMembersCount()
		{
			float weightedAverage = BalanceSystem20.Get().m_WeightedAverage;
			int num = this.m_MaxGroupMembersCount;
			if (weightedAverage < 0.4f)
			{
				num -= 2;
			}
			else if (weightedAverage < 0.8f)
			{
				num--;
			}
			return Mathf.Min(this.m_CurrentGroupMembersCount, num);
		}

		private void Update()
		{
			if (!DifficultySettings.ActivePreset.m_Tribes)
			{
				return;
			}
			if (ScenarioManager.Get().IsDreamOrPreDream())
			{
				return;
			}
			if (DifficultySettings.ActivePreset.m_Tribes)
			{
				this.UpdateCampGroups();
			}
			if (this.m_ScenarioBlock)
			{
				return;
			}
			if (this.m_ActiveGroup && this.m_ActiveGroup.IsWave())
			{
				FirecampGroupsManager.Get().ResetBurningFactors();
			}
			if (DifficultySettings.ActivePreset.m_Tribes)
			{
				this.UpdateGroupMembersCount();
				this.UpdateGroups(this.m_PatrolGroups);
				this.UpdateGroups(this.m_Groups);
				this.UpdateWaves();
			}
			if (DifficultySettings.ActivePreset.m_Predators)
			{
				this.UpdatePredators();
			}
		}

		private void UpdateGroupMembersCount()
		{
			if (MainLevel.Instance.m_Tutorial)
			{
				return;
			}
			if (this.m_CurrentGroupMembersCount == this.m_MaxGroupMembersCount)
			{
				return;
			}
			this.m_TimeToNextIncreaseGroupMembersCount -= Time.deltaTime;
			if (this.m_TimeToNextIncreaseGroupMembersCount <= 0f)
			{
				this.m_CurrentGroupMembersCount++;
				this.m_TimeToNextIncreaseGroupMembersCount = this.m_IncreaseGroupMembersInterval;
			}
		}

		private void UpdateCampGroups()
		{
			if (this.m_ActiveCampGroup)
			{
				return;
			}
			foreach (HumanAIGroup humanAIGroup in this.m_CampGroups)
			{
				if (this.TrySpawnGroup(humanAIGroup))
				{
					this.m_ActiveCampGroup = humanAIGroup;
					break;
				}
			}
		}

		private void UpdateGroups(List<HumanAIGroup> groups)
		{
			if (this.m_ActiveGroup || this.m_ActiveCampGroup || this.m_ActivePredator)
			{
				return;
			}
			this.m_TimeToNextSpawnGroup -= Time.deltaTime;
			if (this.m_TimeToNextSpawnGroup <= 0f)
			{
				foreach (HumanAIGroup humanAIGroup in groups)
				{
					if (this.TrySpawnGroup(humanAIGroup))
					{
						this.m_ActiveGroup = humanAIGroup;
						break;
					}
				}
			}
		}

		private void UpdateWaves()
		{
			if (!DifficultySettings.ActivePreset.m_Tribes)
			{
				return;
			}
			this.m_TimeToNextSpawnWave -= Time.deltaTime;
			if (this.m_TimeToNextSpawnWave < 0f)
			{
				this.m_TimeToNextSpawnWave = 0f;
			}
			else if (this.m_TimeToNextSpawnWave > 0f)
			{
				return;
			}
			if (this.m_ActivePredator)
			{
				return;
			}
			FirecampGroup groupToAttack = FirecampGroupsManager.Get().GetGroupToAttack();
			if (groupToAttack != null)
			{
				if (this.m_ActiveGroup)
				{
					this.m_ActiveGroup.StartWave(groupToAttack);
				}
				else if (this.m_ActiveCampGroup)
				{
					this.m_ActiveCampGroup.StartWave(groupToAttack);
				}
				else
				{
					this.SpawnWave(UnityEngine.Random.Range(1, this.GetCurrentGroupMembersCount() + 1), false, groupToAttack);
				}
				FirecampGroupsManager.Get().ResetBurningFactors();
				this.m_TimeToNextSpawnWave = this.m_WaveSpawnCooldown[(int)DifficultySettings.ActivePreset.m_BaseDifficulty];
			}
		}

		private void UpdatePredators()
		{
			this.m_TimeToNextSpawnPredator -= Time.deltaTime;
		}

		public bool CanSpawnPredator()
		{
			return DifficultySettings.ActivePreset.m_Predators && !this.m_ActivePredator && (!this.m_ActiveCampGroup || this.m_ActiveCampGroup.m_State == HumanAIGroup.State.Calm) && (!this.m_ActiveGroup || this.m_ActiveGroup.m_State == HumanAIGroup.State.Calm) && this.m_TimeToNextSpawnPredator <= 0f;
		}

		public void Save()
		{
			SaveGame.SaveVal("EASM_GroupTime", this.m_TimeToNextSpawnGroup);
			SaveGame.SaveVal("EASM_PredatorTime", this.m_TimeToNextSpawnPredator);
			SaveGame.SaveVal("EASM_WaveTime", this.m_TimeToNextSpawnWave);
		}

		public void Load()
		{
			this.m_TimeToNextSpawnGroup = SaveGame.LoadFVal("EASM_GroupTime");
			this.m_TimeToNextSpawnPredator = SaveGame.LoadFVal("EASM_PredatorTime");
			this.m_TimeToNextSpawnWave = SaveGame.LoadFVal("EASM_WaveTime");
		}

		private bool m_ScenarioBlock;

		private List<HumanAIGroup> m_Groups = new List<HumanAIGroup>();

		private List<HumanAIGroup> m_PatrolGroups = new List<HumanAIGroup>();

		private List<HumanAIGroup> m_CampGroups = new List<HumanAIGroup>();

		private int m_MinGroupMembersCount = 1;

		private int m_MaxGroupMembersCount = 5;

		private float m_IncreaseGroupMembersInterval = 3600f;

		private float m_TimeToNextIncreaseGroupMembersCount;

		private int m_CurrentGroupMembersCount = 1;

		[HideInInspector]
		public float m_MaxActivationDistance = 45f;

		[HideInInspector]
		public float m_MinActivationDistance = 35f;

		public static float s_DeactivationDist = 55f;

		private HumanAIGroup m_ActiveGroup;

		private HumanAIGroup m_ActiveCampGroup;

		private AI m_ActivePredator;

		[HideInInspector]
		public float m_GroupSpawnCooldown = 1200f;

		private float m_PredatorSpawnCooldown = 1200f;

		private float m_TimeToNextSpawnGroup;

		private float m_TimeToNextSpawnPredator;

		private float[] m_StartWaveSpawnCooldown = new float[3];

		private float[] m_WaveSpawnCooldown = new float[3];

		[HideInInspector]
		public float m_TimeToNextSpawnWave;

		private static EnemyAISpawnManager s_Instance = null;
	}
}
