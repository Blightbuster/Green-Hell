using System;
using Enums;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class HumanStrafe : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_PrevDistance = this.m_AI.m_PathModule.m_Agent.remainingDistance;
			this.m_Animation = "Strafe" + this.m_Direction.ToString();
		}

		public void SetDirection(Direction direction)
		{
			this.m_Direction = direction;
			this.m_Animation = "Strafe" + this.m_Direction.ToString();
		}

		public override void Update()
		{
			base.Update();
			this.UpdateWantedDir();
			if (this.m_AI.m_PathModule.m_Agent.remainingDistance >= this.m_PrevDistance)
			{
				this.m_DistanceIncreaingDuration += Time.deltaTime;
			}
			else
			{
				this.m_DistanceIncreaingDuration = 0f;
			}
			this.m_PrevDistance = this.m_AI.m_PathModule.m_Agent.remainingDistance;
		}

		private void UpdateWantedDir()
		{
			this.m_AI.m_TransformModule.m_WantedDirection = (Player.Get().transform.position - this.m_AI.transform.position).GetNormalized2D();
		}

		protected override bool ShouldFinish()
		{
			return !this.m_AI.m_PathModule.m_Agent.pathPending && (this.m_AI.m_PathModule.m_Agent.pathStatus == NavMeshPathStatus.PathInvalid || this.m_DistanceIncreaingDuration > 0.5f || this.m_AI.m_PathModule.m_Agent.remainingDistance <= this.m_AI.GetPathPassDistance());
		}

		private Direction m_Direction;

		private const float MAX_DIST_INCREASE_DUR = 0.5f;

		private float m_PrevDistance;

		private float m_DistanceIncreaingDuration;
	}
}
