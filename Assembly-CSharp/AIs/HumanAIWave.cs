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
			base.name = "AIWave";
			this.m_FromBalance = false;
			this.m_WaterLayer = LayerMask.NameToLayer("Water");
			this.m_AINames.Add("Savage");
			this.m_AINames.Add("Hunter");
			this.m_AINames.Add("Spearman");
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
			this.m_TempPositions.Clear();
			Vector3 position = Player.Get().transform.position;
			Vector3 vector = Vector3.zero;
			if (position.Distance(base.transform.position) >= EnemyAISpawnManager.s_DeactivationDist)
			{
				vector = base.transform.position;
				vector.y = MainLevel.GetTerrainY(vector);
			}
			else
			{
				for (int i = 0; i < 20; i++)
				{
					Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
					Vector3 vector2 = base.transform.position + insideUnitSphere.GetNormalized2D() * UnityEngine.Random.Range(this.m_MinRange, this.m_MaxRange);
					vector2.y = MainLevel.GetTerrainY(vector2);
					if (position.Distance(vector2) > this.m_MinRange)
					{
						vector = vector2;
						Debug.DrawLine(vector2, vector2 + Vector3.up, Color.blue, 60f);
						break;
					}
				}
			}
			if (vector == Vector3.zero)
			{
				return;
			}
			for (int j = 0; j < 20; j++)
			{
				Vector3 insideUnitSphere2 = UnityEngine.Random.insideUnitSphere;
				Vector3 vector3 = vector + insideUnitSphere2.GetNormalized2D() * UnityEngine.Random.Range(1f, 3f);
				vector3.y = MainLevel.GetTerrainY(vector3);
				for (int k = 0; k < this.m_Count; k++)
				{
					NavMeshHit navMeshHit;
					if (NavMesh.SamplePosition(vector3, out navMeshHit, this.m_MaxSampleRange, AIManager.s_WalkableAreaMask) && NavMesh.CalculatePath(navMeshHit.position, vector, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete && !this.IsPositionInWater(navMeshHit.position))
					{
						this.m_TempPositions.Add(navMeshHit.position);
						if (this.m_TempPositions.Count == this.m_Count)
						{
							break;
						}
					}
				}
				if (this.m_TempPositions.Count == this.m_Count)
				{
					break;
				}
			}
			if (this.m_TempPositions.Count != this.m_Count)
			{
				return;
			}
			Debug.Log("HumanAIWave - " + this.m_TempPositions.Count);
			for (int l = 0; l < this.m_TempPositions.Count; l++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(GreenHellGame.Instance.GetPrefab(this.m_AINames[UnityEngine.Random.Range(0, this.m_AINames.Count)]), this.m_TempPositions[l], Quaternion.identity);
				gameObject.transform.parent = base.transform;
				HumanAI component = gameObject.GetComponent<HumanAI>();
				component.m_AnimationModule.m_StartFromRandomFrame = true;
				component.m_Hallucination = this.m_Hallucination;
				this.AddAI(component);
			}
			base.Activate();
			this.m_WaveSpawned = true;
		}

		private bool IsPositionInWater(Vector3 pos)
		{
			int num = Physics.OverlapBoxNonAlloc(pos, this.m_WaterTestSize, this.m_WaterTestCollidersTemp, Quaternion.identity);
			for (int i = 0; i < num; i++)
			{
				if (this.m_WaterTestCollidersTemp[i].gameObject.layer == this.m_WaterLayer)
				{
					return true;
				}
			}
			return false;
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

		protected override void Update()
		{
			base.Update();
			if (!this.m_WaveSpawned)
			{
				this.TrySpawnWave();
				bool waveSpawned = this.m_WaveSpawned;
				return;
			}
		}

		protected override void UpdateActivity()
		{
			if (!this.m_Active && this.m_WaveSpawned && this.m_Members.Count > 0)
			{
				base.Activate();
			}
			if (this.m_Active)
			{
				foreach (HumanAI humanAI in this.m_Members)
				{
					bool flag = humanAI.transform.position.Distance(Player.Get().transform.position) < EnemyAISpawnManager.s_DeactivationDist;
					if (humanAI.gameObject.activeSelf != flag)
					{
						humanAI.gameObject.SetActive(flag);
					}
				}
			}
		}

		public int m_Count;

		private float m_MinRange = 40f;

		private float m_MaxRange = 45f;

		private float m_MaxSampleRange = 2f;

		private bool m_WaveSpawned;

		private NavMeshPath m_TempPath;

		public bool m_Hallucination;

		private Collider[] m_WaterTestCollidersTemp = new Collider[10];

		private int m_WaterLayer;

		private Vector3 m_WaterTestSize = new Vector3(0.5f, 1.5f, 0.5f);

		private List<string> m_AINames = new List<string>();

		private List<Vector3> m_TempPositions = new List<Vector3>();
	}
}
