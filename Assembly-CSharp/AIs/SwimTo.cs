using System;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class SwimTo : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_State = SwimTo.State.WalkToSwim;
		}

		public override void OnAnimEvent(AnimEventID id)
		{
			if (id == AnimEventID.WalkToSwimEnd)
			{
				this.m_State = SwimTo.State.Swim;
				return;
			}
			if (id == AnimEventID.SwimToWalkEnd)
			{
				this.m_State = SwimTo.State.None;
				return;
			}
			base.OnAnimEvent(id);
		}

		public override void Update()
		{
			base.Update();
			if (!this.m_AI.m_TransformModule)
			{
				return;
			}
			if (!this.m_AI.IsSwimming())
			{
				this.m_State = SwimTo.State.None;
			}
			this.UpdateWantedDir();
			this.UpdateBlend();
		}

		private void UpdateWantedDir()
		{
			Vector3 normalized2D = (this.m_AI.m_PathModule.m_Agent.steeringTarget - this.m_AI.m_PathModule.m_Agent.nextPosition).GetNormalized2D();
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
			return this.m_State.ToString();
		}

		protected override bool ShouldFinish()
		{
			return this.m_State == SwimTo.State.None || (!this.m_AI.m_PathModule.m_Agent.pathPending && (this.m_AI.m_PathModule.m_Agent.pathStatus == NavMeshPathStatus.PathInvalid || Vector3.Dot((this.m_AI.m_PathModule.m_Agent.steeringTarget - this.m_AI.transform.position).GetNormalized2D(), this.m_AI.transform.forward.GetNormalized2D()) < 0f || this.m_AI.m_PathModule.m_Agent.remainingDistance <= this.m_AI.GetPathPassDistance()));
		}

		private SwimTo.State m_State = SwimTo.State.WalkToSwim;

		private Vector3[] m_CornersTemp = new Vector3[2];

		private enum State
		{
			None,
			WalkToSwim,
			Swim,
			SwimToWalk
		}
	}
}
