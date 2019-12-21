using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class HumanAIPatrol : HumanAIGroup
	{
		public override void Initialize()
		{
			base.Initialize();
			if (!this.HasPath())
			{
				DebugUtils.Assert("Path is not set!", true, DebugUtils.AssertType.Info);
				return;
			}
			float num = 0f;
			for (int i = 0; i < this.m_Path.Count; i++)
			{
				this.m_Path[i].m_Next.m_Prev = this.m_Path[i];
				num += Vector3.Distance(this.m_Path[i].transform.position, this.m_Path[i].m_Next.transform.position);
			}
			float progress = 0f;
			float num2 = 0f;
			for (int j = 0; j < this.m_Path.Count; j++)
			{
				if (j == 0)
				{
					this.m_Path[j].m_Progress = progress;
				}
				else
				{
					num2 += Vector3.Distance(this.m_Path[j].m_Prev.transform.position, this.m_Path[j].transform.position);
					this.m_Path[j].m_Progress = num2 / num;
				}
			}
		}

		public override bool IsPatrol()
		{
			return true;
		}

		public override void AddAI(HumanAI ai)
		{
			base.AddAI(ai);
			ai.m_Patrol = this;
			if (!ai.gameObject.GetComponent<PatrolModule>())
			{
				ai.m_PatrolModule = ai.gameObject.AddComponent<PatrolModule>();
			}
			ai.m_PatrolModule.m_Patrol = this;
			ai.m_PatrolModule.Initialize(ai);
		}

		public bool HasPath()
		{
			return this.m_Path.Count >= 2;
		}

		protected override void OnActivate()
		{
			AIPathPoint aipathPoint = this.GetClosestPathPointInRange(EnemyAISpawnManager.Get().m_MinActivationDistance, EnemyAISpawnManager.Get().m_MaxActivationDistance);
			if (!aipathPoint)
			{
				float num = 0f;
				aipathPoint = this.GetClosestPathPoint(out num);
			}
			Vector3 normalized = (aipathPoint.m_Next.transform.position - aipathPoint.transform.position).normalized;
			foreach (HumanAI humanAI in this.m_Members)
			{
				humanAI.transform.position = aipathPoint.transform.position + UnityEngine.Random.insideUnitSphere * humanAI.m_PatrolModule.m_PathShift;
				humanAI.transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
				humanAI.m_PatrolModule.m_CurrentPathPoint = aipathPoint.m_Next;
				humanAI.m_PatrolModule.CalcPath();
			}
			if (this.m_Members.Count > 0)
			{
				this.m_Leader = this.m_Members[UnityEngine.Random.Range(0, this.m_Members.Count)];
			}
			else
			{
				this.m_Leader = null;
			}
			base.OnActivate();
		}

		protected override void OnEnterCalmState()
		{
			foreach (HumanAI humanAI in this.m_Members)
			{
				humanAI.SetState(HumanAI.State.Patrol);
			}
		}

		protected override void UpdateCalmSound()
		{
			if (this.m_Members.Count == 0)
			{
				return;
			}
			if (Time.time >= this.m_NextCalmSoundTime)
			{
				HumanAI humanAI = this.m_Members[UnityEngine.Random.Range(0, this.m_Members.Count)];
				this.m_LastCalmClip = humanAI.m_HumanAISoundModule.PlaySound(HumanAISoundModule.SoundType.Scream);
				if (this.m_LastCalmClip)
				{
					this.m_NextCalmSoundTime = Time.time + this.m_LastCalmClip.length + UnityEngine.Random.Range(this.m_MinCalmSoundInterval, this.m_MaxCalmSoundInterval);
				}
			}
		}

		public AIPathPoint GetClosestPathPoint(out float distance)
		{
			Vector3 position = Player.Get().transform.position;
			AIPathPoint result = null;
			distance = float.MaxValue;
			foreach (AIPathPoint aipathPoint in this.m_Path)
			{
				float num = aipathPoint.transform.position.Distance(position);
				if (num < distance)
				{
					result = aipathPoint;
					distance = num;
				}
			}
			return result;
		}

		public AIPathPoint GetClosestPathPointInRange(float min_dist, float max_dist)
		{
			Vector3 position = Player.Get().transform.position;
			AIPathPoint result = null;
			float num = float.MaxValue;
			foreach (AIPathPoint aipathPoint in this.m_Path)
			{
				float num2 = aipathPoint.transform.position.Distance(position);
				if (num2 >= min_dist && num2 <= max_dist && num2 < num)
				{
					result = aipathPoint;
					num = num2;
				}
			}
			return result;
		}

		public List<AIPathPoint> m_Path = new List<AIPathPoint>();

		[HideInInspector]
		public HumanAI m_Leader;
	}
}
