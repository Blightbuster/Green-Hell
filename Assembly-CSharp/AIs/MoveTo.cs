using System;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class MoveTo : AIAction
	{
		public virtual void SetupParams(GoalMove goal)
		{
			this.m_Goal = goal;
			this.m_AI.m_MoveStyle = this.m_Goal.GetWantedMoveStyle();
		}

		public override void Update()
		{
			base.Update();
			this.m_AI.m_MoveStyle = this.m_Goal.GetWantedMoveStyle();
			if (!this.m_AI.m_TransformModule)
			{
				return;
			}
			this.UpdateWantedDir();
			this.UpdateBlend();
		}

		private void UpdateWantedDir()
		{
			Vector3 normalized2D = (this.m_AI.m_PathModule.m_Agent.steeringTarget - this.m_AI.transform.position).GetNormalized2D();
			this.m_AI.m_TransformModule.m_WantedDirection = normalized2D;
		}

		private void UpdateBlend()
		{
			if (this.m_AI.m_AnimationModule.m_HasBlend)
			{
				Vector3 normalized2D = this.m_AI.transform.forward.GetNormalized2D();
				float b = this.m_AI.m_TransformModule.m_WantedDirection.AngleSigned(normalized2D, Vector3.up);
				float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, b, 45f, -45f);
				this.m_AI.m_AnimationModule.SetWantedBlend(proportionalClamp);
			}
		}

		public override string GetAnimName()
		{
			switch (this.m_AI.m_MoveStyle)
			{
			case AIMoveStyle.Walk:
				return this.m_WalkName;
			case AIMoveStyle.Run:
				return this.m_RunName;
			case AIMoveStyle.Sneak:
				return this.m_SneakName;
			case AIMoveStyle.Trot:
				return this.m_TrotName;
			}
			return this.m_WalkName;
		}

		protected override bool ShouldFinish()
		{
			if (this.m_AI.m_PathModule.m_Agent.pathPending)
			{
				return false;
			}
			if (this.m_AI.m_PathModule.m_Agent.pathStatus == NavMeshPathStatus.PathInvalid)
			{
				return true;
			}
			Vector3 normalized2D = (this.m_AI.m_PathModule.m_Agent.steeringTarget - this.m_AI.transform.position).GetNormalized2D();
			return Vector3.Dot(normalized2D, this.m_AI.transform.forward.GetNormalized2D()) < 0f || this.m_AI.m_PathModule.m_Agent.remainingDistance <= this.m_AI.GetPathPassDistance();
		}

		private GoalMove m_Goal;

		private Vector3[] m_CornersTemp = new Vector3[2];

		private string m_WalkName = "Walk";

		private string m_TrotName = "Trot";

		private string m_RunName = "Run";

		private string m_SneakName = "Sneak";
	}
}
