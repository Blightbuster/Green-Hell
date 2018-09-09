using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace AIs
{
	public class AIWavesManager
	{
		public AIWavesManager()
		{
			AIWavesManager.s_Instance = this;
		}

		public static AIWavesManager Get()
		{
			return AIWavesManager.s_Instance;
		}

		private bool ShouldSpawnWave()
		{
			return HumanAIGroupManager.Get() && GreenHellGame.Instance.m_GameDifficulty == GameDifficulty.Hard && !this.m_BlockSpawnWaves && BalanceSystem.Get().CanSpawnHumanAIWave();
		}

		public void Update()
		{
			if (this.ShouldSpawnWave() || (GreenHellGame.DEBUG && Input.GetKeyDown(KeyCode.Z)))
			{
				this.SpawnWave(UnityEngine.Random.Range(1, BalanceSystem.Get().GetCurrentHumanAISpawnCount() + 1), false);
			}
			if (this.m_ActiveWaves.Count == 0)
			{
				this.m_TimeFromLastWave += Time.deltaTime;
			}
			else
			{
				this.m_TimeFromLastWave = 0f;
			}
		}

		public HumanAIWave SpawnWave(int count, bool hallucination = false)
		{
			GameObject gameObject = new GameObject("AIWave");
			HumanAIWave humanAIWave = gameObject.AddComponent<HumanAIWave>();
			humanAIWave.m_Count = count;
			humanAIWave.m_Hallucination = hallucination;
			humanAIWave.Initialize();
			this.m_ActiveWaves.Add(humanAIWave);
			return humanAIWave;
		}

		public void StopWave(HumanAIWave wave)
		{
			this.m_ActiveWaves.Remove(wave);
			UnityEngine.Object.Destroy(wave.gameObject);
		}

		public void Save()
		{
			SaveGame.SaveVal("TimeFromLastWave", this.m_TimeFromLastWave);
		}

		public void Load()
		{
			this.m_TimeFromLastWave = SaveGame.LoadFVal("TimeFromLastWave");
		}

		private List<HumanAIWave> m_ActiveWaves = new List<HumanAIWave>();

		private float m_TimeFromLastWave;

		[HideInInspector]
		public bool m_BlockSpawnWaves;

		private static AIWavesManager s_Instance;
	}
}
