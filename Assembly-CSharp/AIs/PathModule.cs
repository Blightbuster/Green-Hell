using System;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class PathModule : AIModule
	{
		public override void Initialize()
		{
			base.Initialize();
			this.m_AI.m_Animator.applyRootMotion = false;
			if (this.m_AI.m_Trap)
			{
				return;
			}
			this.m_TempPath = new NavMeshPath();
			this.CreateAgent();
		}

		private void CreateAgent()
		{
			Vector3 vector = base.transform.position;
			for (int i = 0; i < 10; i++)
			{
				if (NavMesh.SamplePosition(vector, out this.m_TempHit, 2f, AIManager.s_WalkableAreaMask))
				{
					base.transform.position = this.m_TempHit.position;
					break;
				}
				vector += Vector3.forward;
			}
			this.m_Agent = base.gameObject.GetComponent<NavMeshAgent>();
			if (!this.m_Agent)
			{
				this.m_Agent = base.gameObject.AddComponent<NavMeshAgent>();
			}
			this.m_Agent.updatePosition = true;
			this.m_Agent.updateRotation = false;
			this.m_Agent.speed = 0f;
			this.m_Agent.radius = Mathf.Max(this.m_AI.m_BoxCollider.size.x, this.m_AI.m_BoxCollider.size.z) * 0.5f;
			this.m_Agent.height = this.m_AI.m_BoxCollider.size.y;
			this.m_Agent.angularSpeed = 0f;
			this.m_Agent.acceleration = 100f;
			this.m_Agent.autoBraking = false;
			this.m_Agent.areaMask = AIManager.s_WalkableAreaMask;
			this.m_Agent.autoRepath = false;
			this.m_Agent.stoppingDistance = 0f;
			this.m_Agent.avoidancePriority = this.m_AI.m_Params.m_AvoidancePriority;
		}

		public bool CalcPath(PathModule.PathType path_type, Vector3 target, float range)
		{
			if (!this.m_Agent.isActiveAndEnabled || !this.m_Agent.isOnNavMesh)
			{
				return false;
			}
			this.m_CurrPathType = path_type;
			this.m_PathValid = false;
			if (path_type == PathModule.PathType.MoveTo)
			{
				if (NavMesh.SamplePosition(target, out this.m_TempHit, range, AIManager.s_WalkableAreaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status != NavMeshPathStatus.PathInvalid)
				{
					this.m_Agent.ResetPath();
					this.m_Agent.SetPath(this.m_TempPath);
					this.m_PathValid = true;
				}
			}
			return this.m_PathValid;
		}

		public bool CalcPath(PathModule.PathType path_type)
		{
			if (!this.m_Agent.isActiveAndEnabled || !this.m_Agent.isOnNavMesh)
			{
				return false;
			}
			this.m_PathValid = false;
			Vector3 vector = Vector3.zero;
			switch (path_type)
			{
			case PathModule.PathType.Loiter:
			{
				Vector3 normalized2D = base.transform.forward.GetNormalized2D();
				for (int i = 0; i < this.m_SafeProbes; i++)
				{
					if (this.m_AI.m_Spawner != null)
					{
						vector = this.m_AI.m_Spawner.GetRandomPositionInside();
					}
					else
					{
						vector = base.transform.position + Quaternion.AngleAxis(UnityEngine.Random.Range(0f, (i >= this.m_SafeProbes / 2) ? 360f : 120f), Vector3.up) * normalized2D * this.m_Range;
					}
					vector.y = MainLevel.GetTerrainY(vector);
					if (NavMesh.SamplePosition(vector, out this.m_TempHit, 1f, AIManager.s_WalkableAreaMask))
					{
						if (NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status != NavMeshPathStatus.PathInvalid)
						{
							this.m_Agent.ResetPath();
							this.m_Agent.SetPath(this.m_TempPath);
							this.m_PathValid = true;
							this.m_CurrPathType = path_type;
						}
						break;
					}
				}
				break;
			}
			case PathModule.PathType.MoveAwayFromEnemy:
			case PathModule.PathType.StalkerRunAway:
			{
				Vector3 vector2 = (base.transform.position - Player.Get().transform.position).GetNormalized2D();
				float num = 0f;
				if (this.m_AI.m_ID == AI.AIID.Mouse)
				{
					this.m_Range = UnityEngine.Random.Range(2f, 4f);
					num = UnityEngine.Random.Range(-90f, 90f);
				}
				for (int j = 0; j < this.m_DangerProbes; j++)
				{
					if (j > 0)
					{
						if (j % 2 == 0)
						{
							num = -num;
						}
						else
						{
							num = Mathf.Abs(num) + 360f / (float)this.m_DangerProbes;
						}
					}
					Vector3 b = Quaternion.AngleAxis(num, Vector3.up) * vector2 * this.m_Range;
					vector = base.transform.position + b;
					vector.y = MainLevel.GetTerrainY(vector);
					if (NavMesh.SamplePosition(vector, out this.m_TempHit, 1f, AIManager.s_WalkableAreaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
					{
						this.m_Agent.ResetPath();
						this.m_Agent.SetPath(this.m_TempPath);
						this.m_PathValid = true;
						this.m_CurrPathType = path_type;
						break;
					}
				}
				break;
			}
			case PathModule.PathType.MoveToEnemy:
				vector = Player.Get().transform.position;
				if (NavMesh.SamplePosition(vector, out this.m_TempHit, 4f, AIManager.s_WalkableAreaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status != NavMeshPathStatus.PathInvalid)
				{
					float num2 = 0f;
					int cornersNonAlloc = this.m_TempPath.GetCornersNonAlloc(this.m_TempCorners);
					for (int k = 1; k < cornersNonAlloc; k++)
					{
						num2 += this.m_TempPath.corners[k].Distance(this.m_TempPath.corners[k - 1]);
					}
					if (num2 > this.m_AI.GetPathPassDistance())
					{
						this.m_Agent.ResetPath();
						this.m_Agent.SetPath(this.m_TempPath);
						this.m_PathValid = true;
						this.m_CurrPathType = path_type;
					}
				}
				break;
			case PathModule.PathType.MoveAroundEnemy:
			case PathModule.PathType.StalkerMoveAround:
			{
				Vector3 vector3 = this.m_AI.transform.position - Player.Get().transform.position;
				Vector3 vector2 = Vector3.Cross(vector3.GetNormalized2D(), Vector3.up);
				vector = Player.Get().transform.position + vector3.normalized * StalkerManager.Get().m_MoveAroundRange + vector2 * 3f;
				vector.y = MainLevel.GetTerrainY(vector);
				if (NavMesh.SamplePosition(vector, out this.m_TempHit, 1f, AIManager.s_WalkableAreaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
				{
					this.m_Agent.ResetPath();
					this.m_Agent.SetPath(this.m_TempPath);
					this.m_PathValid = true;
					this.m_CurrPathType = path_type;
				}
				break;
			}
			case PathModule.PathType.ReturnToSpawner:
				vector = this.m_AI.m_Spawner.GetRandomPositionInside();
				vector.y = MainLevel.GetTerrainY(vector);
				if (NavMesh.SamplePosition(vector, out this.m_TempHit, 1f, AIManager.s_WalkableAreaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status != NavMeshPathStatus.PathInvalid)
				{
					this.m_Agent.ResetPath();
					this.m_Agent.SetPath(this.m_TempPath);
					this.m_PathValid = true;
					this.m_CurrPathType = path_type;
				}
				break;
			case PathModule.PathType.Flank:
				for (int l = 0; l < this.m_StrafeProbes; l++)
				{
					Vector3 vector2 = UnityEngine.Random.insideUnitCircle * Player.Get().transform.position.Distance2D(this.m_AI.transform.position);
					vector = Player.Get().transform.position + vector2;
					vector.y = MainLevel.GetTerrainY(vector);
					if (NavMesh.SamplePosition(vector, out this.m_TempHit, 1f, AIManager.s_WalkableAreaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
					{
						this.m_Agent.ResetPath();
						this.m_Agent.SetPath(this.m_TempPath);
						this.m_PathValid = true;
						this.m_CurrPathType = path_type;
						break;
					}
				}
				break;
			case PathModule.PathType.MoveToBowAttackPos:
			{
				HunterAI hunterAI = (HunterAI)this.m_AI;
				if (!hunterAI)
				{
					return this.m_PathValid;
				}
				float d = UnityEngine.Random.Range(hunterAI.m_MinBowDistance, hunterAI.m_MaxBowDistance);
				Vector3 normalized2D2 = (base.transform.position - Player.Get().transform.position).GetNormalized2D();
				float num3 = UnityEngine.Random.Range(-45f, 45f);
				for (int m = 0; m < this.m_DangerProbes; m++)
				{
					if (m > 0)
					{
						if (m % 2 == 0)
						{
							num3 = -num3;
						}
						else
						{
							num3 = Mathf.Abs(num3) + 180f / (float)this.m_DangerProbes;
						}
					}
					Vector3 b2 = Quaternion.AngleAxis(num3, Vector3.up) * normalized2D2 * d;
					vector = Player.Get().transform.position + b2;
					vector.y = MainLevel.GetTerrainY(vector);
					if (NavMesh.SamplePosition(vector, out this.m_TempHit, 1f, AIManager.s_WalkableAreaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
					{
						this.m_Agent.ResetPath();
						this.m_Agent.SetPath(this.m_TempPath);
						this.m_PathValid = true;
						this.m_CurrPathType = path_type;
						break;
					}
				}
				break;
			}
			case PathModule.PathType.StrafeLeft:
			{
				Vector3 a = Vector3.Cross((Player.Get().transform.position - base.transform.position).GetNormalized2D(), Vector3.up);
				vector = base.transform.position + a * UnityEngine.Random.Range(this.m_StrafeRangeMin, this.m_StrafeRangeMax);
				vector.y = MainLevel.GetTerrainY(vector);
				if (NavMesh.SamplePosition(vector, out this.m_TempHit, 1f, AIManager.s_WalkableAreaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
				{
					this.m_Agent.ResetPath();
					this.m_Agent.SetPath(this.m_TempPath);
					this.m_PathValid = true;
					this.m_CurrPathType = path_type;
				}
				break;
			}
			case PathModule.PathType.StrafeRight:
			{
				Vector3 a2 = Vector3.Cross((Player.Get().transform.position - base.transform.position).GetNormalized2D(), Vector3.down);
				vector = base.transform.position + a2 * UnityEngine.Random.Range(this.m_StrafeRangeMin, this.m_StrafeRangeMax);
				vector.y = MainLevel.GetTerrainY(vector);
				if (NavMesh.SamplePosition(vector, out this.m_TempHit, 1f, AIManager.s_WalkableAreaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
				{
					this.m_Agent.ResetPath();
					this.m_Agent.SetPath(this.m_TempPath);
					this.m_PathValid = true;
					this.m_CurrPathType = path_type;
				}
				break;
			}
			case PathModule.PathType.AnimalMoveToEnemy:
				vector = Player.Get().transform.position;
				if (NavMesh.SamplePosition(vector, out this.m_TempHit, 4f, AIManager.s_WalkableAreaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
				{
					float num4 = 0f;
					int cornersNonAlloc2 = this.m_TempPath.GetCornersNonAlloc(this.m_TempCorners);
					for (int n = 1; n < cornersNonAlloc2; n++)
					{
						num4 += this.m_TempPath.corners[n].Distance(this.m_TempPath.corners[n - 1]);
					}
					if (num4 > this.m_AI.GetPathPassDistance())
					{
						this.m_Agent.ResetPath();
						this.m_Agent.SetPath(this.m_TempPath);
						this.m_PathValid = true;
						this.m_CurrPathType = path_type;
					}
				}
				break;
			case PathModule.PathType.MoveToConstruction:
				vector = ((HumanAI)this.m_AI).m_SelectedConstruction.m_BoxCollider.ClosestPointOnBounds(((HumanAI)this.m_AI).m_SelectedConstruction.transform.position);
				if (NavMesh.SamplePosition(vector, out this.m_TempHit, 2f, AIManager.s_WalkableAreaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
				{
					float num5 = 0f;
					int cornersNonAlloc3 = this.m_TempPath.GetCornersNonAlloc(this.m_TempCorners);
					for (int num6 = 1; num6 < cornersNonAlloc3; num6++)
					{
						num5 += this.m_TempPath.corners[num6].Distance(this.m_TempPath.corners[num6 - 1]);
					}
					this.m_Agent.ResetPath();
					this.m_Agent.SetPath(this.m_TempPath);
					this.m_PathValid = true;
					this.m_CurrPathType = path_type;
				}
				break;
			}
			return this.m_PathValid;
		}

		public PathModule.PathType m_CurrPathType;

		public bool m_PathValid;

		[HideInInspector]
		public NavMeshAgent m_Agent;

		private float m_Range = 10f;

		private NavMeshPath m_TempPath;

		private NavMeshHit m_TempHit;

		private int m_SafeProbes = 10;

		private int m_StrafeProbes = 10;

		private int m_DangerProbes = 10;

		private float m_StrafeRangeMin = 4f;

		private float m_StrafeRangeMax = 8f;

		private Vector3[] m_TempCorners = new Vector3[100];

		public enum PathType
		{
			None,
			Loiter,
			MoveAwayFromEnemy,
			MoveToEnemy,
			MoveAroundEnemy,
			ReturnToSpawner,
			Flank,
			StalkerMoveAround,
			StalkerRunAway,
			MoveToAttractor,
			MoveTo,
			MoveToBowAttackPos,
			StrafeLeft,
			StrafeRight,
			AnimalMoveToEnemy,
			MoveToConstruction
		}
	}
}
