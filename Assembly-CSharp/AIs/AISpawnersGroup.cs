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
			}
		}

		private void Update()
		{
			if (this.m_BlockedCount > 0)
			{
				this.m_BlockedDuration += Time.deltaTime;
				if (this.m_BlockedDuration >= this.m_Cooldown)
				{
					this.m_BlockedDuration = 0f;
					this.m_BlockedCount--;
				}
			}
		}

		public bool CanSpawnAI()
		{
			return this.m_CurrentCount < this.m_MaxCount - this.m_BlockedCount;
		}

		public void OnSpawnAI(AI ai)
		{
			this.m_CurrentCount++;
		}

		public void OnDestroyAI(AI ai)
		{
			if (ai.IsDead())
			{
				this.m_BlockedCount++;
			}
			this.m_CurrentCount--;
		}

		public int m_MaxCount;

		public float m_Cooldown;

		private int m_BlockedCount;

		private float m_BlockedDuration;

		private int m_CurrentCount;

		private List<AISpawner> m_Spawners;
	}
}
