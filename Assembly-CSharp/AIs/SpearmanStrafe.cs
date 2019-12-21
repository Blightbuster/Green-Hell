using System;
using Enums;
using UnityEngine.AI;

namespace AIs
{
	public class SpearmanStrafe : AIAction
	{
		public override void Start()
		{
			base.Start();
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
		}

		private void UpdateWantedDir()
		{
			this.m_AI.m_TransformModule.m_WantedDirection = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).GetNormalized2D();
		}

		protected override bool ShouldFinish()
		{
			return !this.m_AI.m_PathModule.m_Agent.pathPending && (this.m_AI.m_PathModule.m_Agent.pathStatus == NavMeshPathStatus.PathInvalid || this.m_AI.m_PathModule.m_Agent.remainingDistance <= this.m_AI.GetPathPassDistance());
		}

		private Direction m_Direction;
	}
}
