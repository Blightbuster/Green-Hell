using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

namespace AIs
{
	public class KidRunnerAI : HumanAI
	{
		protected override void Awake()
		{
			base.Awake();
			this.m_SoundPreset = AI.SoundPreset.None;
		}

		protected override void Start()
		{
			base.Start();
			this.SetState(KidRunnerAI.KidState.Run);
			this.UpdateCurrentTarget();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Physics.IgnoreCollision(Player.Get().m_Collider, this.m_BoxCollider);
		}

		public override bool IsKidRunner()
		{
			return true;
		}

		private void SetState(KidRunnerAI.KidState state)
		{
			if (this.m_KidState == state)
			{
				return;
			}
			this.m_KidState = state;
		}

		protected override void Update()
		{
			base.Update();
			float num = Player.Get().transform.position.Distance(base.transform.position);
			KidRunnerAI.KidState kidState = this.m_KidState;
			if (kidState != KidRunnerAI.KidState.Run)
			{
				if (kidState != KidRunnerAI.KidState.Play)
				{
					return;
				}
				if (num < this.m_PlayStateDistance - 2f)
				{
					this.SetState(KidRunnerAI.KidState.Run);
				}
			}
			else
			{
				this.UpdateCurrentPathPoint();
				this.UpdateCurrentTarget();
				if (this.m_PathPointIndex >= this.m_Path.Count - 1)
				{
					float num2 = 0f;
					CJTools.Math.ProjectPointOnSegment(this.m_Path[this.m_PathPointIndex - 1].position, this.m_Path[this.m_PathPointIndex].position, base.transform.position, out num2);
					if (num2 >= 0.99f)
					{
						this.SetState(KidRunnerAI.KidState.Finish);
					}
				}
				if (num >= this.m_PlayStateDistance)
				{
					this.SetState(KidRunnerAI.KidState.Play);
					return;
				}
			}
		}

		private void UpdateCurrentPathPoint()
		{
			if (this.m_PathPointIndex >= this.m_Path.Count - 1)
			{
				return;
			}
			float num = 0f;
			CJTools.Math.ProjectPointOnSegment(this.m_Path[this.m_PathPointIndex].position, this.m_Path[this.m_PathPointIndex + 1].position, base.transform.position, out num);
			if (num >= 0f)
			{
				this.m_PathPointIndex++;
			}
		}

		private void UpdateCurrentTarget()
		{
			float num = 3f;
			Vector3 vector = this.m_Path[this.m_PathPointIndex].position - base.transform.position;
			if (vector.magnitude >= num)
			{
				this.m_CurrentTarget = base.transform.position + vector.normalized * num;
				return;
			}
			num -= vector.magnitude;
			for (int i = this.m_PathPointIndex; i < this.m_Path.Count - 1; i++)
			{
				Vector3 vector2 = this.m_Path[i + 1].position - this.m_Path[i].position;
				if (vector2.magnitude >= num)
				{
					this.m_CurrentTarget = this.m_Path[i].position + vector2.normalized * num;
					return;
				}
				num -= vector2.magnitude;
			}
		}

		[HideInInspector]
		public KidRunnerAI.KidState m_KidState;

		public List<Transform> m_Path = new List<Transform>();

		private int m_PathPointIndex;

		public float m_PlayStateDistance = 10f;

		[HideInInspector]
		public Vector3 m_CurrentTarget = Vector3.zero;

		public enum KidState
		{
			None,
			Run,
			Play,
			Finish
		}
	}
}
