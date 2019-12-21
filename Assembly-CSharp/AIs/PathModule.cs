using System;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class PathModule : AIModule
	{
		public override void Initialize(Being being)
		{
			base.Initialize(being);
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
			int num = AIManager.s_WalkableAreaMask;
			if (this.m_AI.m_ID == AI.AIID.BlackCaiman || this.m_AI.m_ID == AI.AIID.Crab)
			{
				num |= AIManager.s_WaterAreaMask;
			}
			bool flag = false;
			Vector3 vector = base.transform.position;
			if (this.m_AI.m_ID == AI.AIID.BlackCaiman)
			{
				for (int i = 0; i < 10; i++)
				{
					if (NavMesh.SamplePosition(vector, out this.m_TempHit, 5f, AIManager.s_WaterAreaMask))
					{
						base.transform.position = this.m_TempHit.position;
						flag = true;
						break;
					}
					vector += Vector3.forward;
				}
			}
			if (!flag)
			{
				vector = base.transform.position;
				for (int j = 0; j < 10; j++)
				{
					if (NavMesh.SamplePosition(vector, out this.m_TempHit, 5f, num))
					{
						base.transform.position = this.m_TempHit.position;
						break;
					}
					vector += Vector3.forward;
				}
			}
			if (this.m_AI.IsHumanAI())
			{
				((HumanAI)this.m_AI).m_StartPosition = base.transform.position;
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
			this.m_Agent.areaMask = num;
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
			if (path_type == PathModule.PathType.MoveTo && NavMesh.SamplePosition(target, out this.m_TempHit, range, this.m_Agent.areaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, this.m_Agent.areaMask, this.m_TempPath) && this.m_TempPath.status != NavMeshPathStatus.PathInvalid)
			{
				this.m_Agent.ResetPath();
				this.m_Agent.SetPath(this.m_TempPath);
				this.m_PathValid = true;
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
				int i = 0;
				while (i < this.m_SafeProbes)
				{
					if (this.m_AI.m_Spawner != null)
					{
						vector = this.m_AI.m_Spawner.GetRandomPositionInside();
					}
					else
					{
						vector = base.transform.position + Quaternion.AngleAxis(UnityEngine.Random.Range(0f, (i < this.m_SafeProbes / 2) ? 120f : 360f), Vector3.up) * normalized2D * this.m_Range;
					}
					vector.y = MainLevel.GetTerrainY(vector);
					if (NavMesh.SamplePosition(vector, out this.m_TempHit, 1f, this.m_Agent.areaMask))
					{
						if (NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, this.m_Agent.areaMask, this.m_TempPath) && this.m_TempPath.status != NavMeshPathStatus.PathInvalid)
						{
							this.m_Agent.ResetPath();
							this.m_Agent.SetPath(this.m_TempPath);
							this.m_PathValid = true;
							this.m_CurrPathType = path_type;
							break;
						}
						break;
					}
					else
					{
						i++;
					}
				}
				break;
			}
			case PathModule.PathType.MoveAwayFromEnemy:
			case PathModule.PathType.StalkerRunAway:
			{
				Vector3 vector2 = (base.transform.position - this.m_AI.m_EnemyModule.m_Enemy.transform.position).GetNormalized2D();
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
					if (NavMesh.SamplePosition(vector, out this.m_TempHit, 1f, this.m_Agent.areaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, this.m_Agent.areaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
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
				vector = this.m_AI.m_EnemyModule.m_Enemy.transform.position;
				if (NavMesh.SamplePosition(vector, out this.m_TempHit, 4f, this.m_Agent.areaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, this.m_Agent.areaMask, this.m_TempPath) && this.m_TempPath.status != NavMeshPathStatus.PathInvalid)
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
				Vector3 vector3 = this.m_AI.transform.position - this.m_AI.m_EnemyModule.m_Enemy.transform.position;
				Vector3 vector2 = Vector3.Cross(vector3.GetNormalized2D(), Vector3.up);
				vector = this.m_AI.m_EnemyModule.m_Enemy.transform.position + vector3.normalized * StalkerManager.Get().m_MoveAroundRange + vector2 * 3f;
				vector.y = MainLevel.GetTerrainY(vector);
				if (NavMesh.SamplePosition(vector, out this.m_TempHit, 1f, this.m_Agent.areaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, this.m_Agent.areaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
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
				if (NavMesh.SamplePosition(vector, out this.m_TempHit, 1f, this.m_Agent.areaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, this.m_Agent.areaMask, this.m_TempPath) && this.m_TempPath.status != NavMeshPathStatus.PathInvalid)
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
					Vector3 vector2 = UnityEngine.Random.insideUnitCircle * this.m_AI.m_EnemyModule.m_Enemy.transform.position.Distance2D(this.m_AI.transform.position);
					vector = this.m_AI.m_EnemyModule.m_Enemy.transform.position + vector2;
					vector.y = MainLevel.GetTerrainY(vector);
					if (NavMesh.SamplePosition(vector, out this.m_TempHit, 1f, this.m_Agent.areaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, this.m_Agent.areaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
					{
						this.m_Agent.ResetPath();
						this.m_Agent.SetPath(this.m_TempPath);
						this.m_PathValid = true;
						this.m_CurrPathType = path_type;
						break;
					}
				}
				break;
			case PathModule.PathType.MoveToAttractor:
				if (this.m_AI.m_Attractor)
				{
					vector = this.m_AI.m_Attractor.transform.position;
					if (NavMesh.SamplePosition(vector, out this.m_TempHit, 5f, this.m_Agent.areaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, this.m_Agent.areaMask, this.m_TempPath) && this.m_TempPath.status != NavMeshPathStatus.PathInvalid)
					{
						float num3 = 0f;
						for (int m = 1; m < this.m_TempPath.corners.Length; m++)
						{
							num3 += this.m_TempPath.corners[m].Distance(this.m_TempPath.corners[m - 1]);
						}
						if (num3 > this.m_AI.GetPathPassDistance())
						{
							this.m_Agent.ResetPath();
							this.m_Agent.SetPath(this.m_TempPath);
							this.m_PathValid = true;
							this.m_CurrPathType = path_type;
						}
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
				Vector3 normalized2D2 = (base.transform.position - this.m_AI.m_EnemyModule.m_Enemy.transform.position).GetNormalized2D();
				float num4 = UnityEngine.Random.Range(-45f, 45f);
				for (int n = 0; n < this.m_DangerProbes; n++)
				{
					if (n > 0)
					{
						if (n % 2 == 0)
						{
							num4 = -num4;
						}
						else
						{
							num4 = Mathf.Abs(num4) + 180f / (float)this.m_DangerProbes;
						}
					}
					Vector3 b2 = Quaternion.AngleAxis(num4, Vector3.up) * normalized2D2 * d;
					vector = this.m_AI.m_EnemyModule.m_Enemy.transform.position + b2;
					vector.y = MainLevel.GetTerrainY(vector);
					if (NavMesh.SamplePosition(vector, out this.m_TempHit, 1f, this.m_Agent.areaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, this.m_Agent.areaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
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
				Vector3 a = Vector3.Cross((this.m_AI.m_EnemyModule.m_Enemy.transform.position - base.transform.position).GetNormalized2D(), Vector3.up);
				vector = base.transform.position + a * UnityEngine.Random.Range(this.m_StrafeRangeMin, this.m_StrafeRangeMax);
				vector.y = MainLevel.GetTerrainY(vector);
				if (NavMesh.SamplePosition(vector, out this.m_TempHit, 1f, this.m_Agent.areaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, this.m_Agent.areaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
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
				Vector3 a2 = Vector3.Cross((this.m_AI.m_EnemyModule.m_Enemy.transform.position - base.transform.position).GetNormalized2D(), Vector3.down);
				vector = base.transform.position + a2 * UnityEngine.Random.Range(this.m_StrafeRangeMin, this.m_StrafeRangeMax);
				vector.y = MainLevel.GetTerrainY(vector);
				if (NavMesh.SamplePosition(vector, out this.m_TempHit, 1f, this.m_Agent.areaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, this.m_Agent.areaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
				{
					this.m_Agent.ResetPath();
					this.m_Agent.SetPath(this.m_TempPath);
					this.m_PathValid = true;
					this.m_CurrPathType = path_type;
				}
				break;
			}
			case PathModule.PathType.AnimalMoveToEnemy:
			{
				Vector3 vector4 = Vector3.zero;
				Vector3 vector5 = Vector3.zero;
				if (NavMesh.SamplePosition(this.m_AI.m_EnemyModule.m_Enemy.transform.position, out this.m_TempHit, 4f, this.m_Agent.areaMask))
				{
					vector4 = this.m_TempHit.position;
				}
				if ((this.m_Agent.areaMask & AIManager.s_WaterAreaMask) != 0)
				{
					if (NavMesh.SamplePosition(base.transform.position, out this.m_TempHit, 4f, AIManager.s_WaterAreaMask))
					{
						vector5 = this.m_TempHit.position;
					}
					else if (NavMesh.SamplePosition(base.transform.position, out this.m_TempHit, 4f, AIManager.s_WalkableAreaMask))
					{
						vector5 = this.m_TempHit.position;
					}
				}
				else if (NavMesh.SamplePosition(base.transform.position, out this.m_TempHit, 4f, AIManager.s_WalkableAreaMask))
				{
					vector5 = this.m_TempHit.position;
				}
				if (vector5 != Vector3.zero && vector4 != Vector3.zero && NavMesh.CalculatePath(vector5, vector4, this.m_Agent.areaMask, this.m_TempPath) && (this.m_TempPath.status == NavMeshPathStatus.PathComplete || this.m_TempPath.status == NavMeshPathStatus.PathPartial))
				{
					float num5 = 0f;
					int cornersNonAlloc2 = this.m_TempPath.GetCornersNonAlloc(this.m_TempCorners);
					for (int num6 = 1; num6 < cornersNonAlloc2; num6++)
					{
						num5 += this.m_TempPath.corners[num6].Distance(this.m_TempPath.corners[num6 - 1]);
					}
					if (num5 > this.m_AI.GetPathPassDistance())
					{
						this.m_Agent.ResetPath();
						this.m_Agent.SetPath(this.m_TempPath);
						this.m_PathValid = true;
						this.m_CurrPathType = path_type;
					}
				}
				break;
			}
			case PathModule.PathType.MoveToConstruction:
				vector = ((HumanAI)this.m_AI).m_SelectedConstruction.m_BoxCollider.ClosestPointOnBounds(((HumanAI)this.m_AI).m_SelectedConstruction.transform.position);
				if (NavMesh.SamplePosition(vector, out this.m_TempHit, 2f, this.m_Agent.areaMask) && NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, this.m_Agent.areaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
				{
					float num7 = 0f;
					int cornersNonAlloc3 = this.m_TempPath.GetCornersNonAlloc(this.m_TempCorners);
					for (int num8 = 1; num8 < cornersNonAlloc3; num8++)
					{
						num7 += this.m_TempPath.corners[num8].Distance(this.m_TempPath.corners[num8 - 1]);
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
