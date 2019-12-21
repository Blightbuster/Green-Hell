using System;
using CJTools;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class PatrolModule : AIModule
	{
		public override void Initialize(Being being)
		{
			base.Initialize(being);
			this.m_TempPath = new NavMeshPath();
			this.m_PathShift = UnityEngine.Random.Range(-3f, 3f);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (this.m_AI.m_GoalsModule.m_ActiveGoal != null && this.m_AI.m_GoalsModule.m_ActiveGoal.m_Type == AIGoalType.HumanFollowPatrolPath)
			{
				if (this.m_CalcPathPending)
				{
					this.CalcPath();
				}
				this.UpdateOnPath();
				this.UpdateSpeed();
				return;
			}
			this.m_CalcPathPending = false;
		}

		public void CalcPath()
		{
			if (this.m_AI.m_PathModule == null || this.m_AI.m_PathModule.m_Agent == null)
			{
				this.m_CalcPathPending = true;
				return;
			}
			Vector3 a = Vector3.Cross((this.m_CurrentPathPoint.transform.position - this.m_CurrentPathPoint.m_Prev.transform.position).normalized, Vector3.up);
			if (!NavMesh.SamplePosition(this.m_CurrentPathPoint.transform.position + a * this.m_PathShift, out this.m_TempHit, 2f, AIManager.s_WalkableAreaMask))
			{
				this.m_CalcPathPending = true;
				return;
			}
			if (!NavMesh.CalculatePath(base.transform.position, this.m_TempHit.position, AIManager.s_WalkableAreaMask, this.m_TempPath) || this.m_TempPath.status == NavMeshPathStatus.PathInvalid || this.m_TempPath.status == NavMeshPathStatus.PathPartial)
			{
				this.m_CalcPathPending = true;
				return;
			}
			this.m_AI.m_PathModule.m_Agent.ResetPath();
			this.m_AI.m_PathModule.m_Agent.SetPath(this.m_TempPath);
			this.m_CalcPathPending = false;
		}

		private void UpdateOnPath()
		{
			float num = 0f;
			this.m_PointOnPath = CJTools.Math.ProjectPointOnSegment(this.m_CurrentPathPoint.m_Prev.transform.position, this.m_CurrentPathPoint.transform.position, base.transform.position, out num);
			if (num >= 1f)
			{
				this.PassPathpoint();
				num -= 1f;
			}
			float num2 = this.m_CurrentPathPoint.m_Progress;
			if (num2 == 0f)
			{
				num2 = 1f;
			}
			float progress = this.m_CurrentPathPoint.m_Prev.m_Progress;
			this.m_Progress = CJTools.Math.GetProportionalClamp(progress, num2, num, 0f, 1f) + (float)this.m_Laps;
		}

		private void UpdateSpeed()
		{
			if (!this.m_Patrol.m_Leader || this.m_Patrol.m_Leader.gameObject == this.m_AI.gameObject)
			{
				this.m_AI.m_AnimationModule.ResetForcedSpeed();
				return;
			}
			float num = base.transform.position.Distance(this.m_Patrol.m_Leader.transform.position);
			if (num < 3f)
			{
				this.m_AI.m_AnimationModule.ResetForcedSpeed();
				return;
			}
			if (this.m_Patrol.m_Leader.m_PatrolModule.m_Progress > this.m_Progress)
			{
				this.m_AI.m_AnimationModule.SetForcedSpeed(CJTools.Math.GetProportionalClamp(1f, 1.2f, num, 3f, 6f));
				return;
			}
			this.m_AI.m_AnimationModule.SetForcedSpeed(CJTools.Math.GetProportionalClamp(1f, 0.8f, num, 3f, 6f));
		}

		private void PassPathpoint()
		{
			if (this.m_CurrentPathPoint.m_Progress == 0f)
			{
				this.m_Laps++;
			}
			this.m_CurrentPathPoint = this.m_CurrentPathPoint.m_Next;
			this.CalcPath();
		}

		public AIPathPoint m_CurrentPathPoint;

		[HideInInspector]
		public Vector3 m_Target = Vector3.zero;

		private float m_TargetShift = 2f;

		[HideInInspector]
		public float m_PathShift;

		[HideInInspector]
		public Vector3 m_PointOnPath = Vector3.zero;

		[HideInInspector]
		public float m_Progress;

		private int m_Laps;

		private NavMeshPath m_TempPath;

		private NavMeshHit m_TempHit;

		private bool m_CalcPathPending;

		public HumanAIPatrol m_Patrol;
	}
}
