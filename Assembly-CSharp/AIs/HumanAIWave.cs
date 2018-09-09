using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class HumanAIWave : HumanAIGroup
	{
		private void Awake()
		{
			this.m_TempPath = new NavMeshPath();
			base.transform.position = Player.Get().transform.position;
			base.name = "AIWave";
			this.m_FromBalance = false;
			AIWavesManager.Get().m_BlockSpawnWaves = true;
		}

		public override bool IsWave()
		{
			return !this.m_Hallucination;
		}

		protected override void InitObjects()
		{
			this.TrySpawnWave();
		}

		private void TrySpawnWave()
		{
			Vector3 position = Player.Get().transform.position;
			List<Vector3> list = null;
			for (int i = 0; i < 20; i++)
			{
				Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
				Vector3 vector = position + insideUnitSphere.GetNormalized2D() * UnityEngine.Random.Range(this.m_MinRange, this.m_MaxRange);
				vector.y = MainLevel.GetTerrainY(vector);
				float num = Vector3.Distance(vector, position);
				for (int j = 0; j < this.m_Count; j++)
				{
					NavMeshHit navMeshHit;
					if (NavMesh.SamplePosition(vector, out navMeshHit, this.m_MaxSampleRange, AIManager.s_WalkableAreaMask))
					{
						if (NavMesh.CalculatePath(navMeshHit.position, position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
						{
							if (list == null)
							{
								list = new List<Vector3>();
							}
							list.Add(navMeshHit.position);
							if (list.Count == this.m_Count)
							{
								break;
							}
						}
					}
				}
				if (list != null && list.Count == this.m_Count)
				{
					break;
				}
			}
			if (list == null || list.Count != this.m_Count)
			{
				return;
			}
			Debug.Log("HumanAIWave - " + list.Count);
			for (int k = 0; k < list.Count; k++)
			{
				GameObject prefab = GreenHellGame.Instance.GetPrefab((k != 0) ? "Hunter" : "Savage");
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, list[k], Quaternion.identity);
				gameObject.transform.parent = base.transform;
				HumanAI component = gameObject.GetComponent<HumanAI>();
				component.m_AnimationModule.m_StartFromRandomFrame = true;
				component.m_Hallucination = this.m_Hallucination;
				this.AddAI(component);
			}
			base.Activate();
			this.m_WaveSpawned = true;
		}

		protected override void OnActivate()
		{
			base.OnActivate();
			AIWavesManager.Get().m_BlockSpawnWaves = false;
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			if (this.m_Hallucination)
			{
				PlayerSanityModule.Get().OnDeactivateHallucination();
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		protected override void SetupState()
		{
			base.SetState(HumanAIGroup.State.StartWave);
		}

		protected override void OnEnterState()
		{
			HumanAIGroup.State state = this.m_State;
			if (state != HumanAIGroup.State.StartWave)
			{
				base.OnEnterState();
			}
			else
			{
				this.OnEnterStartWaveState();
			}
		}

		private void OnEnterStartWaveState()
		{
			foreach (HumanAI humanAI in this.m_Members)
			{
				humanAI.SetState(HumanAI.State.StartWave);
			}
		}

		protected override void Update()
		{
			base.Update();
			if (!this.m_WaveSpawned)
			{
				this.TrySpawnWave();
				if (!this.m_WaveSpawned)
				{
					return;
				}
			}
		}

		protected override void UpdateState()
		{
			HumanAIGroup.State state = this.m_State;
			if (state != HumanAIGroup.State.StartWave)
			{
				if (state != HumanAIGroup.State.Attack)
				{
					base.UpdateState();
				}
				else
				{
					this.UpdateAttackState();
				}
			}
			else
			{
				this.UpdateStartWaveState();
			}
		}

		private void UpdateStartWaveState()
		{
			foreach (HumanAI humanAI in this.m_Members)
			{
				if (humanAI.m_EnemySenseModule.m_Enemy || humanAI.m_SightModule.m_PlayerVisible)
				{
					base.SetState(HumanAIGroup.State.Attack);
					break;
				}
			}
		}

		private void UpdateAttackState()
		{
			foreach (HumanAI humanAI in this.m_Members)
			{
				humanAI.m_EnemyModule.SetEnemy(true);
			}
		}

		protected override void UpdateActivity()
		{
			if (!this.m_Active && this.m_WaveSpawned && this.m_Members.Count > 0)
			{
				base.Activate();
			}
		}

		public int m_Count;

		private float m_MinRange = 30f;

		private float m_MaxRange = 35f;

		private float m_MaxSampleRange = 2f;

		private bool m_WaveSpawned;

		private NavMeshPath m_TempPath;

		public bool m_Hallucination;
	}
}
