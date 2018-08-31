using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class AISpawnersGroup : MonoBehaviour
	{
		private void Awake()
		{
			AISpawner[] componentsInChildren = base.GetComponentsInChildren<AISpawner>();
			this.m_Spawners = new List<AISpawner>(componentsInChildren);
			foreach (AISpawner aispawner in this.m_Spawners)
			{
				aispawner.m_SpawnersGroup = this;
				aispawner.m_ResetTime = 0f;
			}
			this.m_CurrentMaxCount = this.m_MaxCount;
		}

		private void Update()
		{
			if (this.m_TimeToAddMaxCount == 0f)
			{
				if (this.m_CurrentMaxCount >= this.m_MaxCount)
				{
					return;
				}
				this.m_TimeToAddMaxCount = this.m_Cooldown;
			}
			this.m_TimeToAddMaxCount -= Time.deltaTime;
			if (this.m_TimeToAddMaxCount <= 0f)
			{
				this.m_CurrentMaxCount++;
				if (this.m_CurrentMaxCount < this.m_MaxCount)
				{
					this.m_TimeToAddMaxCount = this.m_Cooldown;
				}
				else
				{
					this.m_TimeToAddMaxCount = 0f;
				}
			}
		}

		public bool CanSpawnAI()
		{
			return this.m_CurrentCount < this.m_CurrentMaxCount;
		}

		public void OnSpawnAI(AI ai)
		{
			this.m_CurrentCount++;
		}

		public void OnDestroyAI(AI ai)
		{
			if (ai.IsDead())
			{
				this.m_CurrentMaxCount--;
				if (this.m_TimeToAddMaxCount <= 0f)
				{
					this.m_TimeToAddMaxCount = this.m_Cooldown;
				}
			}
			this.m_CurrentCount--;
		}

		public int m_MaxCount;

		public float m_Cooldown;

		private float m_TimeToAddMaxCount;

		private int m_CurrentCount;

		private int m_CurrentMaxCount;

		private List<AISpawner> m_Spawners;
	}
}
