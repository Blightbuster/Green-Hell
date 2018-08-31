using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class HumanAIDestroyers : HumanAIGroup
	{
		private void Awake()
		{
			base.name = "AIDestroyers";
			this.m_TempPath = new NavMeshPath();
			this.m_FromBalance = false;
		}

		public override bool IsDestroyers()
		{
			return true;
		}

		protected override void InitObjects()
		{
			this.TrySpawnDestroyers();
		}

		private void TrySpawnDestroyers()
		{
			List<Vector3> list = null;
			for (int i = 0; i < 20; i++)
			{
				for (int j = 0; j < this.m_Count; j++)
				{
					Vector3 normalized2D = UnityEngine.Random.insideUnitSphere.GetNormalized2D();
					Vector3 sourcePosition = this.m_Construction.transform.position + normalized2D * 10f;
					NavMeshHit navMeshHit;
					if (NavMesh.SamplePosition(sourcePosition, out navMeshHit, 2f, AIManager.s_WalkableAreaMask))
					{
						sourcePosition = navMeshHit.position;
						Vector3 sourcePosition2 = this.m_Construction.m_BoxCollider.ClosestPointOnBounds(navMeshHit.position);
						if (NavMesh.SamplePosition(sourcePosition2, out navMeshHit, 1f, AIManager.s_WalkableAreaMask))
						{
							if (NavMesh.CalculatePath(sourcePosition, navMeshHit.position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
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
			for (int k = 0; k < list.Count; k++)
			{
				GameObject prefab = GreenHellGame.Instance.GetPrefab((k != 0) ? "Hunter" : "Savage");
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, list[k], Quaternion.identity);
				gameObject.transform.parent = base.transform;
				HumanAI component = gameObject.GetComponent<HumanAI>();
				component.m_AnimationModule.m_StartFromRandomFrame = true;
				this.AddAI(component);
			}
			base.Activate();
			this.m_GroupSpawned = true;
		}

		protected override void OnActivate()
		{
			base.OnActivate();
			for (int i = 0; i < this.m_Members.Count; i++)
			{
				this.m_Members[i].m_SelectedConstruction = this.m_Construction;
			}
		}

		protected override void SetupState()
		{
			base.SetState(HumanAIGroup.State.Attack);
		}

		protected override void Update()
		{
			base.Update();
			if (!this.m_GroupSpawned)
			{
				this.TrySpawnDestroyers();
				if (!this.m_GroupSpawned)
				{
					return;
				}
			}
			if (this.m_Members.Count == 0)
			{
				base.Deactivate();
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		public int m_Count;

		private bool m_GroupSpawned;

		private NavMeshPath m_TempPath;

		public Construction m_Construction;
	}
}
