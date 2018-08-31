using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

namespace AIs
{
	public class HumanAIGroupManager : MonoBehaviour
	{
		public static HumanAIGroupManager Get()
		{
			return HumanAIGroupManager.s_Instance;
		}

		private void Awake()
		{
			HumanAIGroupManager.s_Instance = this;
		}

		private void Start()
		{
			List<HumanAIGroup> componentsDeepChild = General.GetComponentsDeepChild<HumanAIGroup>(base.gameObject);
			for (int i = 0; i < componentsDeepChild.Count; i++)
			{
				HumanAIGroup humanAIGroup = componentsDeepChild[i];
				if (!humanAIGroup.m_ChallengeGroup)
				{
					if (humanAIGroup.m_FromBalance)
					{
						if (humanAIGroup.IsPatrol())
						{
							this.m_BalancePatrols.Add((HumanAIPatrol)humanAIGroup);
						}
						else
						{
							this.m_BalanceGroups.Add(humanAIGroup);
						}
					}
					else
					{
						this.m_NoBalanceGroups.Add(humanAIGroup);
					}
					humanAIGroup.Initialize();
					humanAIGroup.gameObject.SetActive(false);
				}
			}
		}

		private bool CanSpawnGroupFromBalanceSystem()
		{
			return BalanceSystem.Get() && BalanceSystem.Get().CanSpawnHumanAIGroup();
		}

		private void Update()
		{
			for (int i = 0; i < this.m_NoBalanceGroups.Count; i++)
			{
				HumanAIGroup group = this.m_NoBalanceGroups[i];
				if (this.CheckGroup(group))
				{
					break;
				}
			}
			if (this.CanSpawnGroupFromBalanceSystem())
			{
				for (int j = 0; j < this.m_BalancePatrols.Count; j++)
				{
					HumanAIPatrol group2 = this.m_BalancePatrols[j];
					if (this.CheckPatrol(group2))
					{
						break;
					}
				}
				for (int k = 0; k < this.m_BalanceGroups.Count; k++)
				{
					HumanAIGroup group3 = this.m_BalanceGroups[k];
					if (this.CheckGroup(group3))
					{
						break;
					}
				}
			}
		}

		private bool CheckGroup(HumanAIGroup group)
		{
			if (group.m_Active)
			{
				return false;
			}
			float num = 0f;
			HumanAI closestMember = group.GetClosestMember(out num);
			bool flag = closestMember && num < this.m_MaxActivationDistance && num > this.m_MinActivationDistance;
			if (flag)
			{
				group.Activate();
				return true;
			}
			return false;
		}

		private bool CheckPatrol(HumanAIPatrol group)
		{
			if (group.m_Active)
			{
				return false;
			}
			float num = 0f;
			AIPathPoint closestPathPoint = group.GetClosestPathPoint(out num);
			bool flag = closestPathPoint && num < this.m_MaxActivationDistance && num > this.m_MinActivationDistance;
			if (flag)
			{
				group.Activate();
				return true;
			}
			return false;
		}

		private void UpdateDestroyers()
		{
			if (this.m_Destroyers)
			{
				if (this.m_Destroyers.m_Members.Count == 0)
				{
					UnityEngine.Object.Destroy(this.m_Destroyers.gameObject);
					this.m_Destroyers = null;
				}
				return;
			}
			for (int i = 0; i < Construction.s_Constructions.Count; i++)
			{
				if (Construction.s_Constructions[i].m_Info.IsShelter())
				{
					this.SpawnDestroyers(Construction.s_Constructions[i]);
					break;
				}
			}
		}

		private void SpawnDestroyers(Construction construction)
		{
			GameObject gameObject = new GameObject("AIWave");
			HumanAIDestroyers humanAIDestroyers = gameObject.AddComponent<HumanAIDestroyers>();
			humanAIDestroyers.m_Count = 3;
			humanAIDestroyers.m_Construction = construction;
			this.m_Destroyers = humanAIDestroyers;
			this.m_Destroyers.Initialize();
			this.m_Destroyers.Activate();
		}

		public void OnDestroyGroup(HumanAIGroup group)
		{
			this.m_BalanceGroups.Remove(group);
			if (group.IsPatrol())
			{
				this.m_BalancePatrols.Remove((HumanAIPatrol)group);
			}
			this.m_NoBalanceGroups.Remove(group);
		}

		private List<HumanAIGroup> m_BalanceGroups = new List<HumanAIGroup>();

		private List<HumanAIPatrol> m_BalancePatrols = new List<HumanAIPatrol>();

		private List<HumanAIGroup> m_NoBalanceGroups = new List<HumanAIGroup>();

		[HideInInspector]
		public float m_DeactivationDistance = 55f;

		private float m_MaxActivationDistance = 45f;

		private float m_MinActivationDistance = 35f;

		private HumanAIDestroyers m_Destroyers;

		private static HumanAIGroupManager s_Instance;
	}
}
