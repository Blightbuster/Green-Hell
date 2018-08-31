using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class MoveToObject : MoveTo
	{
		public void SetupParams(Vector3 start, GameObject target, AIMoveStyle style, float radius)
		{
			this.m_TargetObject = target;
			this.SetupTargetPos();
		}

		private void SetupTargetPos()
		{
			(this.m_AI.transform.position - this.m_TargetObject.transform.position).Normalize();
			this.m_LastTargetPos = this.m_TargetObject.transform.position;
			this.m_LastSetupTargetTime = Time.time;
		}

		public override void Update()
		{
			if (this.ShouldRecalculatePath())
			{
				this.SetupTargetPos();
			}
			base.Update();
		}

		private bool ShouldRecalculatePath()
		{
			return Time.time - this.m_LastSetupTargetTime > 1f || Vector3.Distance(this.m_TargetObject.transform.position, this.m_LastTargetPos) > this.m_MaxPositionDiff;
		}

		protected override bool ShouldFail()
		{
			return this.m_TargetObject == null || base.ShouldFail();
		}

		private GameObject m_TargetObject;

		private Vector3 m_LastTargetPos = Vector3.zero;

		private float m_MaxPositionDiff = 1f;

		private float m_LastSetupTargetTime;
	}
}
