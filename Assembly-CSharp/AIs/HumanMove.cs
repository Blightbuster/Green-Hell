using System;
using Enums;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class HumanMove : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.SetupAnim();
		}

		private void SetupAnim()
		{
			switch (this.m_AI.m_MoveStyle)
			{
			case AIMoveStyle.Walk:
				this.m_Animation = "Walk";
				return;
			case AIMoveStyle.Run:
				this.m_Animation = "Run";
				return;
			case AIMoveStyle.Sneak:
				this.m_Animation = "Sneak";
				return;
			case AIMoveStyle.Swim:
				this.m_Animation = "Swim";
				return;
			case AIMoveStyle.Trot:
				this.m_Animation = "Trot";
				return;
			case AIMoveStyle.Jump:
				this.m_Animation = "Jump";
				return;
			default:
				return;
			}
		}

		public override void Update()
		{
			base.Update();
			this.UpdateWantedDir();
		}

		private void UpdateWantedDir()
		{
			Vector3 normalized2D = (this.m_AI.m_PathModule.m_Agent.steeringTarget - this.m_AI.transform.position).GetNormalized2D();
			this.m_AI.m_TransformModule.m_WantedDirection = normalized2D;
		}

		protected override bool ShouldFinish()
		{
			return !this.m_AI.m_PathModule.m_Agent.pathPending && (this.m_AI.m_PathModule.m_Agent.pathStatus == NavMeshPathStatus.PathInvalid || this.m_AI.m_PathModule.m_Agent.remainingDistance <= this.m_AI.GetPathPassDistance());
		}
	}
}
